using VeeamTestTask;

namespace TestProject
{
    [TestClass]
    public class SyncTests : BaseTestClass
    {
        [TestMethod]
        public void RemovesFileFromDestinationTest()
        {
            CreateSrcFile(fileName1);
            CreateSrcFile(fileName2);

            var model = new FolderModel(_src, _dest);

            File.Delete(Path.Join(_src.FullName, fileName1));

            var result = model.Sync();

            Assert.IsTrue(result.Contains($"{fileName1} was deleted"));
            Assert.IsFalse(File.Exists(Path.Join(_dest.FullName, fileName1)));
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, fileName2)));
        }

        [TestMethod]
        public void NoChangesReturnsEmptyStringTest()
        {
            CreateSrcFile(fileName1);

            var model = new FolderModel(_src, _dest);
            var result = model.Sync();

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void DifferentFileSizeTest()
        {
            CreateSrcFile(fileName1, "short");

            var model = new FolderModel(_src, _dest);

            File.WriteAllText(Path.Join(_src.FullName, fileName1), "much longer content now");

            var result = model.Sync();

            Assert.IsTrue(result.Contains($"{fileName1} was updated"));
            Assert.AreEqual(
                "much longer content now",
                File.ReadAllText(Path.Join(_dest.FullName, fileName1)));
        }

        [TestMethod]
        public void SameSizeDifferentHashTest()
        {
            CreateSrcFile(fileName1, "aaaa");

            var model = new FolderModel(_src, _dest);

            File.WriteAllText(Path.Join(_src.FullName, fileName1), "bbbb");

            var result = model.Sync();

            Assert.IsTrue(result.Contains($"{fileName1} was updated"));
            Assert.AreEqual(
                "bbbb",
                File.ReadAllText(Path.Join(_dest.FullName, fileName1)));
        }

        [TestMethod]
        public void UnmodifiedFileTest()
        {
            CreateSrcFile(fileName1);

            var model = new FolderModel(_src, _dest);
            var result = model.Sync();

            Assert.IsTrue(result == "");
        }

        [TestMethod]
        public void NewFileAddedTest()
        {
            CreateSrcFile(fileName1);

            var model = new FolderModel(_src, _dest);

            CreateSrcFile(fileName2, "brand new");

            var result = model.Sync();

            Assert.IsTrue(result.Contains($"{fileName2} was added"));
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, fileName2)));
            Assert.AreEqual(
                "brand new",
                File.ReadAllText(Path.Join(_dest.FullName, fileName2)));
        }


        [TestMethod]
        public void DeletedSubfolderInSourceTest()
        {
            var sub = CreateSrcSubfolder(folderName1);
            CreateFileIn(sub, fileName1);

            var model = new FolderModel(_src, _dest);

            Assert.IsTrue(Directory.Exists(Path.Join(_dest.FullName, folderName1)));

            Directory.Delete(sub.FullName, recursive: true);

            var result = model.Sync();

            Assert.IsTrue(result == $"/dest\n  /{folderName1} was deleted\r\n    {fileName1} was deleted\r\n");
            Assert.IsFalse(Directory.Exists(Path.Join(_dest.FullName, folderName1)));
        }

        [TestMethod]
        public void NewSubfolderAddedToDestinationTest()
        {
            var model = new FolderModel(_src, _dest);

            var newSub = CreateSrcSubfolder(folderName1);
            CreateFileIn(newSub, fileName1, "new content");

            var result = model.Sync();

            Assert.IsTrue(result == $"/dest\n  /{folderName1} was added\r\n    {fileName1} was added\r\n");
            Assert.IsTrue(Directory.Exists(Path.Join(_dest.FullName, folderName1)));
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, folderName1, fileName1)));
        }


        [TestMethod]
        public void MultipleOperationsTest()
        {
            string deleteFile = "delete.txt";
            string updateFile = "update.txt";
            string subFile = "sub.txt";
            string subFile2 = "sub2.txt";
            string addedFile = "added.txt";
            string expectedResult = $"/dest\n  {deleteFile} was deleted\n  /{folderName1} was deleted\r\n    {subFile} was deleted\r\n  {updateFile} was updated\n  {addedFile} was added\n  /{folderName2} was added\r\n    {subFile2} was added\r\n";

            CreateSrcFile(deleteFile, "bye");
            CreateSrcFile(updateFile, "old");
            var sub = CreateSrcSubfolder(folderName1);
            CreateFileIn(sub, subFile);

            var model = new FolderModel(_src, _dest);

            // delete
            File.Delete(Path.Join(_src.FullName, deleteFile));
            // update
            File.WriteAllText(Path.Join(_src.FullName, updateFile), "new content!");
            // add
            CreateSrcFile(addedFile);
            // add subfolder
            var newSub = CreateSrcSubfolder(folderName2);
            CreateFileIn(newSub, subFile2);
            // delete subfolder
            Directory.Delete(sub.FullName, recursive: true);

            var result = model.Sync();

            Assert.IsTrue(result == expectedResult);

            // delete
            Assert.IsFalse(File.Exists(Path.Join(_dest.FullName, deleteFile)));
            // update
            Assert.AreEqual("new content!", File.ReadAllText(Path.Join(_dest.FullName, updateFile)));
            // add
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, addedFile)));
            // add subfolder
            string subfolder = Path.Join(_dest.FullName, folderName2);
            Assert.IsTrue(Directory.Exists(subfolder));
            Assert.IsTrue(File.Exists(Path.Join(subfolder, subFile2)));
            // delete subfolder
            Assert.IsFalse(Directory.Exists(Path.Join(_dest.FullName, folderName1)));
        }

        [TestMethod]
        public void NestedChangesTest()
        {
            var sub = CreateSrcSubfolder(folderName1);
            CreateFileIn(sub, fileName1, "original");

            var model = new FolderModel(_src, _dest);

            File.WriteAllText(Path.Join(sub.FullName, fileName1), "modified");

            var result = model.Sync();

            Assert.IsTrue(result == $"/dest\n  /{folderName1}\n    {fileName1} was updated\n");
            Assert.AreEqual(
                "modified",
                File.ReadAllText(Path.Join(_dest.FullName, folderName1, fileName1)));
        }

        [TestMethod]
        public void LockedFileContinuesBestEffortTest()
        {
            CreateSrcFile("locked.txt", "content");
            CreateSrcFile("normal.txt", "content");

            var model = new FolderModel(_src, _dest);

            File.WriteAllText(Path.Join(_src.FullName, "locked.txt"), "changed");
            File.WriteAllText(Path.Join(_src.FullName, "normal.txt"), "changed");

            var destLockedPath = Path.Join(_dest.FullName, "locked.txt");
            using var lockStream = new FileStream(destLockedPath, FileMode.Open, FileAccess.Read, FileShare.None);

            var result = model.Sync();

            Assert.IsTrue(result.Contains("normal.txt was updated"));
            Assert.IsTrue(model.Errors.Count > 0);
            Assert.IsTrue(model.Errors.Any(e => e.Contains("locked.txt")));
        }
    }
}