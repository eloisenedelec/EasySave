using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class BackupExecutor
    {
        private List<IBackupObserver> _observers;

        public BackupExecutor() { // TODO }

        public void AddObserver(IBackupObserver observer) { // TODO }
        public void RemoveObserver(IBackupObserver observer) { // TODO }
        public void ExecuteBackup(BackupJob job) { // TODO }
        public void ExecuteMultipleBackups(List<int> jobIds) { // TODO }

        private void NotifyFileProcessed(string fileName, long size, long time) { // TODO }
        private void NotifyBackupStarted(string jobName, int totalFiles, long totalSize) { // TODO }
        private void NotifyBackupCompleted(string jobName) { // TODO }
        private void NotifyBackupError(string jobName, string error) { // TODO }
    }
}
