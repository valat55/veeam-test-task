using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 6)]
namespace TestProject
{
    [TestClass]
    public class BaseTestClass
    {
        protected string _testRoot = null!;
        protected DirectoryInfo _src = null!;
        protected DirectoryInfo _dest = null!;

        protected const string fileName1 = "file1.txt";
        protected const string fileName2 = "file2.txt";
        protected const string fileName3 = "file3.txt";

        protected const string folderName1 = "folder1";
        protected const string folderName2 = "folder2";

        [TestInitialize]
        public void Setup()
        {
            _testRoot = Path.Join(Path.GetTempPath(), "FolderModelTests_" + Guid.NewGuid());
            _src = new DirectoryInfo(Path.Join(_testRoot, "src"));
            _dest = new DirectoryInfo(Path.Join(_testRoot, "dest"));
            _src.Create();
            _dest.Create();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, recursive: true);
        }


        protected void CreateSrcFile(string name, string content = "test")
        {
            File.WriteAllText(Path.Join(_src.FullName, name), content);
        }

        protected void CreateDestFile(string name, string content = "test")
        {
            File.WriteAllText(Path.Join(_dest.FullName, name), content);
        }

        protected DirectoryInfo CreateSrcSubfolder(string name)
        {
            var dir = new DirectoryInfo(Path.Join(_src.FullName, name));
            dir.Create();
            return dir;
        }

        protected FileInfo CreateFileIn(DirectoryInfo dir, string name, string content = "test")
        {
            var path = Path.Join(dir.FullName, name);
            File.WriteAllText(path, content);
            return new FileInfo(path);
        }
    }
}
