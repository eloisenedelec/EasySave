using EasySave.Factories;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Strategies;

namespace EasySave.Services
{
    public class BackupExecutor
    {
        private List<IBackupObserver> _observers;

        public BackupExecutor() { 
            _observers = new List<IBackupObserver>();
        }

        public void AddObserver(IBackupObserver observer) {
            if(!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
        public void RemoveObserver(IBackupObserver observer) { 
            _observers.Remove(observer);
        }
        public void ExecuteBackup(BackupJob job) {
            try
            {
                IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(job.Type);

                NotifyBackupStarted(job.Name, 0, 0);

                strategy.Execute(job.SourcePath, job.TargetPath, this);

                NotifyBackupCompleted(job.Name);
            }
            catch (Exception ex)
            {
                NotifyBackupError(job.Name, ex.Message);
            }
        }
        public void ExecuteMultipleBackups(List<int> jobIds) {
            BackupManager manager = BackupManager.GetInstance();
            foreach (int id in jobIds)
            {
                var job = manager.GetBackupJob(id);
                if (job != null)
                {
                    ExecuteBackup(job);
                }
            }
        }

        private void NotifyFileProcessed(string fileName, long size, long time) {
            _observers.ForEach(o => o.OnFileProcessed(fileName, size, time));
        }
        private void NotifyBackupStarted(string jobName, int totalFiles, long totalSize) {
            _observers.ForEach(o => o.OnBackupStarted(jobName, totalFiles, totalSize));
        }
        private void NotifyBackupCompleted(string jobName) {
            _observers.ForEach(o => o.OnBackupCompleted(jobName));
        }
        private void NotifyBackupError(string jobName, string error) {
            _observers.ForEach(o => o.OnBackupError(jobName, error));
        }
    }
}
