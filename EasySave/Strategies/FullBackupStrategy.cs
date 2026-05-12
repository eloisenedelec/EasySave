using System.IO;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        // On supprime l'ancien EncryptionService car on utilise désormais CryptoSoft via l'executor

        public void Execute(BackupJob job, BackupExecutor executor)
        {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            // On utilise l'executor pour démarrer le job
            executor.OnBackupStarted(job.Name, files.Length, totalSize);

            try
            {
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var targetFile = Path.Combine(targetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var startTime = DateTime.Now;

                    // CENTRALISATION : C'est l'executor qui gère le Mutex et le chiffrement
                    executor.ProcessFileCopy(file, targetFile);

                    var transferTime = (DateTime.Now - startTime).Ticks;
                    var fileSize = new FileInfo(file).Length;

                    // Notification via l'executor
                    executor.OnFileProcessed(file, targetFile, fileSize, transferTime, 0);
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