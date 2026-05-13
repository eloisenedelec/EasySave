using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public interface IBackupController
    {
        void AddObserver(IBackupObserver observer);
        void RemoveObserver(IBackupObserver observer);
        void ExecuteBackup(BackupJob job);
        void ExecuteMultipleBackups(List<int> jobIds);
        void PauseAll();
        void ResumeAll();
        void StopAll();
    }
}