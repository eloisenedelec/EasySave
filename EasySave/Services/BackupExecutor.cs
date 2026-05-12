using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class BackupExecutor : IBackupObserver
    {
        private readonly List<IBackupObserver> _observers = new();
        private readonly BackupOrchestrator _orchestrator = new();

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
            ExecuteJobsAsync(new[] { job }).GetAwaiter().GetResult();
        }

        public void ExecuteMultipleBackups(List<int> jobIds)
        {
            var jobs = BackupManager.GetInstance()
                .GetAllBackupJobs()
                .Where(job => jobIds.Contains(job.Id))
                .ToList();

            ExecuteJobsAsync(jobs).GetAwaiter().GetResult();
        }

        private async Task ExecuteJobsAsync(IEnumerable<BackupJob> jobs)
        {
            var jobList = jobs.ToList();
            if (jobList.Count == 0)
            {
                return;
            }

            _orchestrator.Clear();

            foreach (var job in jobList)
            {
                _orchestrator.CreateTask(job, this);
            }

            try
            {
                await _orchestrator.RunAllAsync();
            }
            catch (Exception ex)
            {
                NotifyBackupError("BackupExecutor", ex.Message);
            }
            finally
            {
                _orchestrator.Clear();
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