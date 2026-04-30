using System.IO;
using EasySave.Observers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class DiffBackupStrategy : IBackupStrategy
    {
        private readonly EncryptionService _encryption = new();
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
            var encryptedExtensions = SettingsManager.GetInstance().GetEncryptedExtensions();

            observer.OnBackupStarted(job.Name, modifiedFiles.Count, totalSize);
            try
            {
                foreach (var sourceFile in modifiedFiles)
                {
                    var relative = Path.GetRelativePath(sourcePath, sourceFile);
                    var targetFile = Path.Combine(targetPath, relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(sourceFile).Length;
                    long encryptionTime = 0;

                    var startTime = DateTime.Now;
                    if (encryptedExtensions.Contains(Path.GetExtension(sourceFile).ToLower()))
                        encryptionTime = _encryption.EncryptFile(sourceFile, targetFile);
                    else
                        File.Copy(sourceFile, targetFile, true);
                    var transferTime = (DateTime.Now - startTime).Ticks;

                    observer.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTime);
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
