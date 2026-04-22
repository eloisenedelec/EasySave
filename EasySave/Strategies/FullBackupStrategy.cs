using EasySave.Observers;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(BackupJob job, IBackupObserver observer) {
            string sourcePath = job.SourcePath;
            string targetPath = job.TargetPath;

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            long totalSize = files.Sum(f => new FileInfo(f).Length);

            observer.OnBackupStarted(job.Name, files.Length, totalSize);
            try
            {
                foreach (var file in files)
                {
                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var targetFile = Path.Combine(targetPath, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

                    var fileSize = new FileInfo(file).Length;
                    var startTime = DateTime.Now;
                    File.Copy(file, targetFile, true);
                    var transferTime = (DateTime.Now - startTime).Ticks;

                    observer.OnFileProcessed(file, targetFile, fileSize, transferTime);
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
