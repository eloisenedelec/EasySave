using System;
using System.IO;
using System.Linq;
using System.Threading;
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

        // On remet BackupExecutionContext ici aussi !
        public void Execute(BackupJob job, BackupExecutionContext context)
        {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var modifiedFiles = Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Where(f => IsFileModified(f, sourcePath, targetPath))
                .ToList();

            long totalSize = modifiedFiles.Sum(f => new FileInfo(f).Length);

            var settings = SettingsManager.GetInstance();
            var encryptedExtensions = settings.GetEncryptedExtensions();
            var prioExtensions = settings.GetPriorityExtensions();

            int prioCount = modifiedFiles.Count(f => prioExtensions.Contains(Path.GetExtension(f).ToLower()));
            context.PriorityTracker.RegisterJobPriorityFiles(job.Id, prioCount);

            context.Observer.OnBackupStarted(job.Name, modifiedFiles.Count, totalSize);

            var cryptoManager = new CryptoSoftManager();

            try
            {
                foreach (var sourceFile in modifiedFiles)
                {
                    context.CheckPauseAndCancellation();

                    var extension = Path.GetExtension(sourceFile).ToLower();
                    var isPriorityFile = prioExtensions.Contains(extension);

                    if (!isPriorityFile)
                    {
                        context.PriorityTracker.WaitForPriorityIfNeeded(
                            Timeout.InfiniteTimeSpan,
                            context.CancellationToken
                        );
                    }

                    var relative = Path.GetRelativePath(sourcePath, sourceFile);
                    var targetFile = Path.Combine(targetPath, relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(sourceFile).Length;
                    var startTime = DateTime.Now;
                    long encryptionTime = 0;

                    if (encryptedExtensions.Contains(extension))
                    {
                        // On chronomètre manuellement
                        var encStart = DateTime.Now;
                        cryptoManager.EncryptFile(sourceFile, targetFile);
                        encryptionTime = (long)(DateTime.Now - encStart).TotalMilliseconds;
                    }
                    else
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }

                    var transferTime = (DateTime.Now - startTime).Ticks;

                    context.Observer.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTime);

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