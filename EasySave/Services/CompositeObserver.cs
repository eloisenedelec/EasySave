using EasySave.Observers;

namespace EasySave.Services
{
    public class CompositeObserver : IBackupObserver
    {
        private readonly IBackupObserver[] _observers;

        public CompositeObserver(params IBackupObserver[] observers)
        {
            _observers = observers;
        }

        public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            foreach (var o in _observers) o.OnBackupStarted(jobName, totalFiles, totalSize);
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            foreach (var o in _observers) o.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTimeMs);
        }

        public void OnBackupCompleted(string jobName)
        {
            foreach (var o in _observers) o.OnBackupCompleted(jobName);
        }

        public void OnBackupError(string jobName, string error)
        {
            foreach (var o in _observers) o.OnBackupError(jobName, error);
        }
    }
}
