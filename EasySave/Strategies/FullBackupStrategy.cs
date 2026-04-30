using System.IO;
using EasySave.Observers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private readonly EncryptionService _encryption = new();

        public void Execute(BackupJob job, IBackupObserver observer) {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            var encryptedExtensions = SettingsManager.GetInstance().GetEncryptedExtensions();

            observer.OnBackupStarted(job.Name, files.Length, totalSize);
            try
            {
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var targetFile = Path.Combine(targetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(file).Length;
                    long encryptionTime = 0;

                    var startTime = DateTime.Now;
                    if (encryptedExtensions.Contains(Path.GetExtension(file).ToLower()))
                        encryptionTime = _encryption.EncryptFile(file, targetFile);
                    else
                        File.Copy(file, targetFile, true);
                    var transferTime = (DateTime.Now - startTime).Ticks;

                    observer.OnFileProcessed(file, targetFile, fileSize, transferTime, encryptionTime);
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
