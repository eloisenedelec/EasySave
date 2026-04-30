using EasySave.Factories;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Strategies;
using EasySave.Services;

namespace EasySave.Services
{
    public class BackupExecutor : IBackupObserver
    {
        private List<IBackupObserver> _observers;

        public BackupExecutor()
        {
            _observers = new List<IBackupObserver>();
        }

        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
        public void RemoveObserver(IBackupObserver observer)
        {
            _observers.Remove(observer);
        }
        public void ExecuteBackup(BackupJob job)
        {
            ProcessMonitoring pm = new ProcessMonitoring(SettingsManager.GetInstance());
            if (!pm.AreNoBusinessProcessesRunning())
            {
                NotifyBackupError(job.Name, "Annulation : Logiciel métier en cours d'exécution.");
                return;
            }

            try
            {
                IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(job.Type);
                strategy.Execute(job, this);
            }
            catch (Exception ex)
            {
                NotifyBackupError(job.Name, ex.Message);
            }
        }
        public void ExecuteMultipleBackups(List<int> jobIds)
        {
            BackupManager manager = BackupManager.GetInstance();
            ProcessMonitoring pm = new ProcessMonitoring(SettingsManager.GetInstance());

            foreach (int id in jobIds)
            {
                if (!pm.AreNoBusinessProcessesRunning())
                {
                    break;
                }

                var job = manager.GetBackupJob(id);
                if (job != null)
                {
                    ExecuteBackup(job);
                }
            }
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            NotifyFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTimeMs);
        }
        public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            NotifyBackupStarted(jobName, totalFiles, totalSize);
        }
        public void OnBackupCompleted(string jobName)
        {
            NotifyBackupCompleted(jobName);
        }
        public void OnBackupError(string jobName, string error)
        {
            NotifyBackupError(jobName, error);
        }

        private void NotifyFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            _observers.ForEach(o => o.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTimeMs));
        }
        private void NotifyBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            _observers.ForEach(o => o.OnBackupStarted(jobName, totalFiles, totalSize));
        }
        private void NotifyBackupCompleted(string jobName)
        {
            _observers.ForEach(o => o.OnBackupCompleted(jobName));
        }
        private void NotifyBackupError(string jobName, string error)
        {
            _observers.ForEach(o => o.OnBackupError(jobName, error));
        }
    }
}
