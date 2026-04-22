using EasySave.Observers;
using EasySave.Models;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        private long GetTotalSize(string sourcePath)
        {
            return Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }

        private int GetTotalFiles(string sourcePath)
        {
            return Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).Length;
        }


        public void Execute(BackupJob job, IBackupObserver observer) {
            string sourcePath = job.GetSourcePath();
            string targetPath = job.GetTargetPath();
            long totalSize = GetTotalSize(sourcePath);
            int totalFiles = GetTotalFiles(sourcePath);
            observer.OnBackupStarted(job.GetName(), totalFiles, totalSize);
            try
            {
                CopyAllFiles(sourcePath, targetPath, observer);
                observer.OnBackupCompleted(job.GetName());
            }
            catch (Exception ex)
            {
                observer.OnBackupError(job.GetName(), ex.Message);
            }
        }

        private void CopyAllFiles(string source, string target, IBackupObserver observer) { 
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source, file);
                var targetFile = Path.Combine(target, relativePath);
                var targetDir = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                var fileInfo = new FileInfo(file);
                var startTime = DateTime.Now;
                File.Copy(file, targetFile, true);
                var endTime = DateTime.Now;
                observer.OnFileProcessed(file, targetFile, fileInfo.Length, (endTime - startTime).Ticks);
            }
        }
    }
}
