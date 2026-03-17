using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeeamTestTask;

namespace TestProject
{

    [TestClass]
    public class SyncPlanTests : BaseTestClass
    {
        

        [TestMethod]
        public void BothEmptyTest()
        {
            var destFiles = new List<FileInfo>();
            var destFolders = new List<FolderModel>();

            var plan = new SyncPlan(destFiles, destFolders, _src);

            Assert.AreEqual(0, plan.FilesToDelete.Count);
            Assert.AreEqual(0, plan.FilesToUpdate.Count);
            Assert.AreEqual(0, plan.FilesToAdd.Count);
            Assert.AreEqual(0, plan.FoldersToDelete.Count);
            Assert.AreEqual(0, plan.FoldersToAdd.Count);
        }

        [TestMethod]
        public void SrcEmptyDestWithFileTest()
        {
            var destFile = CreateFileIn(_dest, fileName1);
            var destFiles = new List<FileInfo> { destFile };

            var plan = new SyncPlan(destFiles, new List<FolderModel>(), _src);

            Assert.AreEqual(1, plan.FilesToDelete.Count);
            Assert.AreEqual(fileName1, plan.FilesToDelete[0].Name);
            Assert.AreEqual(0, plan.FilesToAdd.Count);
            Assert.AreEqual(0, plan.FilesToUpdate.Count);
        }

        [TestMethod]
        public void FileInSrcDestEmptyTest()
        {
            CreateFileIn(_src, fileName1);

            var plan = new SyncPlan(new List<FileInfo>(), new List<FolderModel>(), _src);

            Assert.AreEqual(1, plan.FilesToAdd.Count);
            Assert.AreEqual(fileName1, plan.FilesToAdd[0].Name);
            Assert.AreEqual(0, plan.FilesToDelete.Count);
        }

        [TestMethod]
        public void FileInBothTest()
        {
            CreateFileIn(_src, fileName1, "src version");
            var destFile = CreateFileIn(_dest, fileName1, "dest version");
            var destFiles = new List<FileInfo> { destFile };

            var plan = new SyncPlan(destFiles, new List<FolderModel>(), _src);

            Assert.AreEqual(1, plan.FilesToUpdate.Count);
            Assert.AreEqual(fileName1, plan.FilesToUpdate[0].Name);
            Assert.AreEqual(0, plan.FilesToDelete.Count);
            Assert.AreEqual(0, plan.FilesToAdd.Count);
        }

        [TestMethod]
        public void SetsAreDisjointTest()
        {
            CreateFileIn(_src, fileName1);
            CreateFileIn(_src, fileName2);
            var destBoth = CreateFileIn(_dest, fileName1);
            var destOrphan = CreateFileIn(_dest, fileName3);
            var destFiles = new List<FileInfo> { destBoth, destOrphan };

            var plan = new SyncPlan(destFiles, new List<FolderModel>(), _src);

            var allFileNames = plan.FilesToDelete.Select(f => f.Name)
                .Concat(plan.FilesToUpdate.Select(f => f.Name))
                .Concat(plan.FilesToAdd.Select(f => f.Name))
                .ToList();

            Assert.AreEqual(allFileNames.Count, allFileNames.Distinct().Count());
        }

        [TestMethod]
        public void FolderToAddTest()
        {
            _src.CreateSubdirectory(folderName1);

            var plan = new SyncPlan(new List<FileInfo>(), new List<FolderModel>(), _src);

            Assert.AreEqual(1, plan.FoldersToAdd.Count);
            Assert.AreEqual(folderName1, plan.FoldersToAdd[0].Name);
        }
    }
}
