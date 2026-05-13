using EasySave.Models;
using EasySave.Observers;
using System.Collections.Generic;

namespace EasySave.Services
{
    public class BackupControlService : IBackupController
    {
        private readonly BackupExecutor _backupExecutor;
        private readonly BackupOrchestrator _backupOrchestrator;

        public BackupControlService()
        {
            // Correction : On instancie les nouveaux composants correctement
            _backupOrchestrator = new BackupOrchestrator(GlobalPriorityTracker.Instance, new LargeFileCoordinator());
            // On partage l'orchestrateur avec l'Executor
            _backupExecutor = new BackupExecutor(_backupOrchestrator);
        }

        public BackupControlService(BackupExecutor backupExecutor, BackupOrchestrator backupOrchestrator)
        {
            _backupExecutor = backupExecutor;
            _backupOrchestrator = backupOrchestrator;
        }

        public void AddObserver(IBackupObserver observer)
        {
            _backupExecutor.AddObserver(observer);
        }

        public void RemoveObserver(IBackupObserver observer)
        {
            _backupExecutor.RemoveObserver(observer);
        }

        public void ExecuteBackup(BackupJob job)
        {
            _backupExecutor.ExecuteBackup(job);
        }

        public void ExecuteMultipleBackups(List<int> jobIds)
        {
            _backupExecutor.ExecuteMultipleBackups(jobIds);
        }

        public void PauseAll()
        {
            _backupOrchestrator.PauseAll();
        }

        public void ResumeAll()
        {
            _backupOrchestrator.ResumeAll();
        }

        public void StopAll()
        {
            _backupOrchestrator.StopAll();
        }
    }
}