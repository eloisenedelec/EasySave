using EasySave.Observers;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DiffBackupStrategy : IBackupStrategy
    {
        private bool IsFileModified(string sourceFile, string sourceRoot, string lastBackupRoot)
        {
            var relative = Path.GetRelativePath(sourceRoot, sourceFile);
            var targetFile = Path.Combine(lastBackupRoot, relative);

            // Nouveau fichier
            if (!File.Exists(targetFile))
                return true;

            var sourceInfo = new FileInfo(sourceFile);
            var targetInfo = new FileInfo(targetFile);

            // Modifié si la date d'écriture est plus récente
            return sourceInfo.LastWriteTime > targetInfo.LastWriteTime;
        }


        private long GetTotalSize(string sourcePath, string lastBackupPath)
        {
            return Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Where(file => IsFileModified(file, sourcePath, lastBackupPath))
                .Sum(file => new FileInfo(file).Length);
        }


        public void Execute(BackupJob job, IBackupObserver observer) {
            string sourcePath = job.GetSourcePath();
            string targetPath = job.GetTargetPath();
            long totalSize = GetTotalSize(sourcePath, targetPath);
            int totalFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Count(file => IsFileModified(file, sourcePath, targetPath));
            observer.OnBackupStarted(job.GetName(), totalFiles, totalSize);
            try
            {
                CopyModifiedFiles(sourcePath, targetPath, observer);
                observer.OnBackupCompleted(job.GetName());
            }
            catch (Exception ex)
            {
                observer.OnBackupError(job.GetName(), ex.Message);
            }
        }

        private void CopyModifiedFiles(string source, string target, IBackupObserver observer) {
            foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (IsFileModified(sourceFile, source, target))
                {
                    var relative = Path.GetRelativePath(source, sourceFile);
                    var targetFile = Path.Combine(target, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                    var startTime = DateTime.Now;
                    File.Copy(sourceFile, targetFile, true);
                    var endTime = DateTime.Now;
                    var fileSize = new FileInfo(sourceFile).Length;
                    var transferTime = (endTime - startTime).Ticks;
                    observer.OnFileProcessed(relative, fileSize, transferTime);
                }
            }
        }
    }
}
