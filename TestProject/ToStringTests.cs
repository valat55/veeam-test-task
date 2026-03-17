using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeeamTestTask;

namespace TestProject
{
    [TestClass]
    public class ToStringTests : BaseTestClass
    {
        [TestMethod]
        public void EmptyFolderTest()
        {
            var model = new FolderModel(_src, _dest);
            var result = model.ToString();

            Assert.IsTrue(result == $"/{_dest.Name}\r\n");
        }

        [TestMethod]
        public void FileNamesTest()
        {
            CreateSrcFile(fileName1);

            var model = new FolderModel(_src, _dest);
            var result = model.ToString();

            Assert.IsTrue(result == $"/dest\r\n  {fileName1}\r\n");
        }

        [TestMethod]
        public void ActionTextTest()
        {
            CreateSrcFile(fileName1);

            var model = new FolderModel(_src, _dest);
            var result = model.ToString("deleted");

            Assert.IsTrue(result == $"/dest was deleted\r\n  {fileName1} was deleted\r\n");
        }

        [TestMethod]
        public void SubfolderContentTest()
        {
            var sub = CreateSrcSubfolder(folderName1);
            CreateFileIn(sub, fileName1);

            var model = new FolderModel(_src, _dest);
            var result = model.ToString();

            Assert.IsTrue(result == $"/dest\r\n  /{folderName1}\r\n    {fileName1}\r\n");
        }
    }
}
