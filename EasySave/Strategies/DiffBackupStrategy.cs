using EasySave.Observers;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class DiffBackupStrategy : IBackupStrategy
    {
        private bool IsFileModified(string sourceFile, string sourceRoot, string lastBackupRoot)
        {
            var relative = Path.GetRelativePath(sourceRoot, sourceFile);
            var backupFile = Path.Combine(lastBackupRoot, relative);

            if (!File.Exists(backupFile))
                return true;

            return new FileInfo(sourceFile).LastWriteTime > new FileInfo(backupFile).LastWriteTime;
        }

        public void Execute(BackupJob job, IBackupObserver observer) {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var modifiedFiles = Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Where(f => IsFileModified(f, sourcePath, targetPath))
                .ToList();

            long totalSize = modifiedFiles.Sum(f => new FileInfo(f).Length);

            observer.OnBackupStarted(job.Name, modifiedFiles.Count, totalSize);
            try
            {
                foreach (var sourceFile in modifiedFiles)
                {
                    var relative = Path.GetRelativePath(sourcePath, sourceFile);
                    var targetFile = Path.Combine(targetPath, relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(sourceFile).Length;
                    var startTime = DateTime.Now;
                    File.Copy(sourceFile, targetFile, true);
                    var transferTime = (DateTime.Now - startTime).Ticks;

                    observer.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, 0);
                }
                observer.OnBackupCompleted(job.Name);
            }
            catch (Exception ex)
            {
                observer.OnBackupError(job.Name, ex.Message);
            }
        }
    }
}
