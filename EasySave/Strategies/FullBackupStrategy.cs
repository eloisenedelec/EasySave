using System.IO;
using EasySave.Observers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private readonly EncryptionService _encryption = new();

        public void Execute(BackupJob job, BackupExecutionContext context) {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);
            var encryptedExtensions = SettingsManager.GetInstance().GetEncryptedExtensions();
            var settings = SettingsManager.GetInstance();
            var prioExtensions = settings.GetPriorityExtensions();
            int prioCount = files.Count(f => prioExtensions.Contains(Path.GetExtension(f).ToLower()));
            context.PriorityTracker.RegisterJobPriorityFiles(job.Id, prioCount);

            context.Observer.OnBackupStarted(job.Name, files.Length, totalSize);
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

                    var fileSize = new FileInfo(file).Length;
                    long encryptionTime = 0;

                    var startTime = DateTime.Now;
                    if (encryptedExtensions.Contains(extension))
                        encryptionTime = _encryption.EncryptFile(file, targetFile);
                    else
                        File.Copy(file, targetFile, true);
                    var transferTime = (DateTime.Now - startTime).Ticks;

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
