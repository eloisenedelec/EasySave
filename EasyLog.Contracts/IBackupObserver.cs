namespace EasySave.Observers
{
    public interface IBackupObserver
    {
        void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs);
        void OnBackupStarted(string jobName, int totalFiles, long totalSize);
        void OnBackupCompleted(string jobName);
        void OnBackupError(string jobName, string error);
    }
}
