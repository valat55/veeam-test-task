using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeeamTestTask;

namespace TestProject
{
    [TestClass]
    public class ConstructorTests : BaseTestClass
    {
        [TestMethod]
        public void EmptyModelTest()
        {
            var model = new FolderModel(_src, _dest);

            Assert.AreEqual(_dest.Name, model.Name);
            Assert.AreEqual(0, model.Files.Count);
            Assert.AreEqual(0, model.SubFolders.Count);
            Assert.AreEqual(0, model.Indentation);
        }

        [TestMethod]
        public void CopyFilesToDestinationTest()
        {
            CreateSrcFile(fileName1, "hello");
            CreateSrcFile(fileName2, "world");

            var model = new FolderModel(_src, _dest);

            Assert.AreEqual(2, model.Files.Count);
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, fileName1)));
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, fileName2)));
            Assert.AreEqual("hello", File.ReadAllText(Path.Join(_dest.FullName, fileName1)));
            Assert.AreEqual("world", File.ReadAllText(Path.Join(_dest.FullName, fileName2)));
        }

        [TestMethod]
        public void CopiesSubfoldersRecursivelyTest()
        {
            var sub = CreateSrcSubfolder(folderName1);
            CreateFileIn(sub, fileName1, "nested content");

            var model = new FolderModel(_src, _dest);

            Assert.AreEqual(1, model.SubFolders.Count);
            Assert.AreEqual(folderName1, model.SubFolders[0].Name);
            Assert.IsTrue(File.Exists(Path.Join(_dest.FullName, folderName1, fileName1)));
        }

        [TestMethod]
        public void IndentationIncrementsForSubfoldersTest()
        {
            CreateSrcSubfolder(folderName1);

            var model = new FolderModel(_src, _dest);

            Assert.AreEqual(0, model.Indentation);
            Assert.AreEqual(1, model.SubFolders[0].Indentation);
        }
    }
}
