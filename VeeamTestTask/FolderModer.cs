
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace VeeamTestTask
{
    public class FolderModel
    {
        public int Indentation { get; set; }
        public List<FolderModel> SubFolders { get; set; }
        public List<FileInfo> Files { get; set; }
        public string Name { get; set; }
        public DirectoryInfo Source { get; }
        public DirectoryInfo Dest { get; }
        public List<string> Errors = new List<string>();


        public FolderModel(DirectoryInfo src, DirectoryInfo destination, int indentation = -1)
        {
            SubFolders = new();
            Source = src;
            Dest = destination;
            Name = destination.Name;
            Files = copySrcFiles(src, destination);
            this.Indentation = indentation + 1;
            foreach (var subFolder in src.EnumerateDirectories())
            {
                // Skip symlinks and junction points — don't follow links outside the source tree
                if (subFolder.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    continue;
                var subCopy = new DirectoryInfo(Path.Join(destination.FullName, subFolder.Name));
                subCopy.Create();
                SubFolders.Add(new FolderModel(subFolder, subCopy, this.Indentation));
            }
        }


        public override string ToString() => BuildString(null);

        public string ToString(string action) => BuildString(action);

        private string BuildString(string? action)
        {
            var sb = new StringBuilder();
            var indent = new string(' ', Indentation * 2);
            var childIndent = new string(' ', (Indentation + 1) * 2);
            var suffix = action != null ? $" was {action}" : "";

            sb.AppendLine($"{indent}/{Name}{suffix}");

            foreach (var file in Files)
                sb.AppendLine($"{childIndent}{file.Name}{suffix}");

            foreach (var subFolder in SubFolders)
                sb.Append(action != null ? subFolder.ToString(action) : subFolder.ToString());

            return sb.ToString();
        }

        private List<FileInfo> copySrcFiles(DirectoryInfo src, DirectoryInfo destination)
        {
            var copy = new List<FileInfo>();
            foreach (var file in src.EnumerateFiles())
            {
                var newFileName = Path.Join(destination.FullName, file.Name);
                var newFile = file.CopyTo(newFileName);
                copy.Add(newFile);
            }
            return copy;
        }

        public string Sync()
        {
            Errors = new();
            var plan = new SyncPlan(Files, SubFolders, Source);

            var result = deleteFiles(plan.FilesToDelete);
            result += DeleteRemovedSubfolders(plan.FoldersToDelete);
            result += UpdateFiles(plan.FilesToUpdate);

            foreach (var subfolder in SubFolders)
                result += subfolder.Sync();

            result += AddNewFiles(plan.FilesToAdd);
            result += AddNewSubfolders(plan.FoldersToAdd);

            if (result.Length > 0)
                result = new string(' ', Indentation * 2) + $"/{Name}\n" + result;

            return result;
        }

        private string deleteFiles(List<FileInfo> toDelete)
        {
            var result = "";
            foreach (var file in toDelete)
            {
                try
                {
                    file.Delete();
                    Files.Remove(file);
                    result += $"{new string(' ', (Indentation + 1) * 2)}{file.Name} was deleted\n";
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Errors.Add($"Failed to delete '{file.Name}': {ex.Message}");
                }
            }
            return result;
        }

        private string DeleteRemovedSubfolders(List<FolderModel> toDelete)
        {
            var result = "";
            foreach (var folder in toDelete)
            {
                try
                {
                    folder.Dest.Delete(recursive: true);
                    SubFolders.Remove(folder);
                    result += folder.ToString("deleted");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Errors.Add($"Failed to delete folder '{folder.Name}': {ex.Message}");
                }
            }
            return result;
        }


        private string UpdateFiles(List<FileInfo> toUpdate)
        {
            var destFileMap = Files.ToDictionary(f => f.Name);
            var result = "";

            foreach (var srcFile in toUpdate)
            {
                try
                {
                    var destFile = destFileMap[srcFile.Name];

                    if (srcFile.Length != destFile.Length || !compareHashes(srcFile, destFile))
                    {
                        srcFile.CopyTo(destFile.FullName, overwrite: true);
                        destFile.Refresh();
                        result += $"{new string(' ', (Indentation + 1) * 2)}{destFile.Name} was updated\n";
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Errors.Add($"Failed to update '{srcFile.Name}': {ex.Message}");
                }
            }

            return result;
        }

        private static bool compareHashes(FileInfo src, FileInfo dest)
        {
            using var srcStream = src.OpenRead();
            var srcHash = MD5.HashData(srcStream);

            using var destStream = dest.OpenRead();
            var destHash = MD5.HashData(destStream);

            return srcHash.SequenceEqual(destHash);
        }

        private string AddNewFiles(List<FileInfo> toAdd)
        {
            var result = "";
            foreach (var srcFile in toAdd)
            {
                try
                {
                    var destPath = Path.Join(Dest.FullName, srcFile.Name);
                    var copiedFile = srcFile.CopyTo(destPath, overwrite: true);
                    Files.Add(copiedFile);
                    result += $"{new string(' ', (Indentation + 1) * 2)}{srcFile.Name} was added\n";
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Errors.Add($"Failed to add '{srcFile.Name}': {ex.Message}");
                }
            }
            return result;
        }

        private string AddNewSubfolders(List<DirectoryInfo> newDirs)
        {
            var result = "";

            foreach (var dir in newDirs)
            {
                try
                {
                    var destSub = new DirectoryInfo(Path.Join(Dest.FullName, dir.Name));
                    destSub.Create();
                    var newFolder = new FolderModel(dir, destSub, Indentation);
                    SubFolders.Add(newFolder);
                    result += newFolder.ToString("added");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Errors.Add($"Failed to add folder '{dir.Name}': {ex.Message}");
                }
            }
            return result;
        }

    }
}
