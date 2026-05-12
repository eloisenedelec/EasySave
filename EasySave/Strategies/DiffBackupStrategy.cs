using System.IO;
using EasySave.Models;
using EasySave.Services;

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

        public void Execute(BackupJob job, BackupExecutor executor)
        {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var modifiedFiles = Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Where(f => IsFileModified(f, sourcePath, targetPath))
                .ToList();

            long totalSize = modifiedFiles.Sum(f => new FileInfo(f).Length);

            // On utilise l'executor au lieu de l'observer
            executor.OnBackupStarted(job.Name, modifiedFiles.Count, totalSize);

            try
            {
                foreach (var sourceFile in modifiedFiles)
                {
                    var relative = Path.GetRelativePath(sourcePath, sourceFile);
                    var targetFile = Path.Combine(targetPath, relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(sourceFile).Length;
                    var startTime = DateTime.Now;

                    // CENTRALISATION : Utilisation du processus de copie avec Mutex
                    executor.ProcessFileCopy(sourceFile, targetFile);

                    var transferTime = (DateTime.Now - startTime).Ticks;

                    executor.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, 0);
                }
                executor.OnBackupCompleted(job.Name);
            }
            catch (Exception ex)
            {
                executor.OnBackupError(job.Name, ex.Message);
            }
        }
    }
}