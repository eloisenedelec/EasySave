using System;
using System.IO;
using System.Linq;
using System.Threading;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        // On remet BackupExecutionContext ici !
        public void Execute(BackupJob job, BackupExecutionContext context)
        {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            var settings = SettingsManager.GetInstance();
            var encryptedExtensions = settings.GetEncryptedExtensions();
            var prioExtensions = settings.GetPriorityExtensions();

            int prioCount = files.Count(f => prioExtensions.Contains(Path.GetExtension(f).ToLower()));
            context.PriorityTracker.RegisterJobPriorityFiles(job.Id, prioCount);

            context.Observer.OnBackupStarted(job.Name, files.Length, totalSize);

            var cryptoManager = new CryptoSoftManager();

            try
            {
                foreach (var file in files)
                {
                    context.CheckPauseAndCancellation();

                    var extension = Path.GetExtension(file).ToLower();
                    var isPriorityFile = prioExtensions.Contains(extension);

                    if (!isPriorityFile)
                    {
                        context.PriorityTracker.WaitForPriorityIfNeeded(
                            Timeout.InfiniteTimeSpan,
                            context.CancellationToken
                        );
                    }

                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var targetFile = Path.Combine(targetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var startTime = DateTime.Now;
                    long encryptionTime = 0;

                    if (encryptedExtensions.Contains(extension))
                    {
                        // On chronomètre manuellement
                        var encStart = DateTime.Now;
                        cryptoManager.EncryptFile(file, targetFile);
                        encryptionTime = (long)(DateTime.Now - encStart).TotalMilliseconds;
                    }
                    else
                    {
                        File.Copy(file, targetFile, true);
                    }

                    var transferTime = (DateTime.Now - startTime).Ticks;
                    var fileSize = new FileInfo(file).Length;

                    context.Observer.OnFileProcessed(file, targetFile, fileSize, transferTime, encryptionTime);

                    if (isPriorityFile)
                    {
                        context.PriorityTracker.PriorityFileFinished(job.Id);
                    }
                }
                context.Observer.OnBackupCompleted(job.Name);
            }
            catch (OperationCanceledException)
            {
                context.Observer.OnBackupError(job.Name, "Sauvegarde annulée");
            }
            catch (Exception ex)
            {
                context.Observer.OnBackupError(job.Name, ex.Message);
            }
        }
    }
}