using System.Linq;

namespace VeeamTestTask
{
    public class SyncPlan
    {
        public List<FileInfo> FilesToDelete { get; }
        public List<FileInfo> FilesToUpdate { get; }
        public List<FileInfo> FilesToAdd { get; }
        public List<FolderModel> FoldersToDelete { get; }
        public List<DirectoryInfo> FoldersToAdd { get; }

        public SyncPlan(
            List<FileInfo> destFiles,
            List<FolderModel> destFolders,
            DirectoryInfo source)
        {
            var srcFiles = source.EnumerateFiles().ToList();
            var srcFileNames = srcFiles.Select(f => f.Name).ToHashSet();
            var destFileNames = destFiles.Select(f => f.Name).ToHashSet();

            FilesToDelete = destFiles.Where(f => !srcFileNames.Contains(f.Name)).ToList();
            FilesToUpdate = srcFiles.Where(f => destFileNames.Contains(f.Name)).ToList();
            FilesToAdd = srcFiles.Where(f => !destFileNames.Contains(f.Name)).ToList();

            var srcDirs = source.EnumerateDirectories()
                .Where(d => !d.Attributes.HasFlag(FileAttributes.ReparsePoint))
                .ToList();
            var srcDirNames = srcDirs.Select(d => d.Name).ToHashSet();
            var destDirNames = destFolders.Select(f => f.Name).ToHashSet();

            FoldersToDelete = destFolders.Where(f => !srcDirNames.Contains(f.Name)).ToList();
            FoldersToAdd = srcDirs.Where(d => !destDirNames.Contains(d.Name)).ToList();
        }
    }
}