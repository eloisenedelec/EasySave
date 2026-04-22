namespace EasySave.Observers
{
    public interface IBackupObserver
    {
        void OnFileProcessed(string fileName, long fileSize, long transferTime);
        void OnBackupStarted(string jobName, int totalFiles, long totalSize);
        void OnBackupCompleted(string jobName);
        void OnBackupError(string jobName, string error);
    }
}
