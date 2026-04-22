using EasySave.Models;
using EasySave.Repositories;

namespace EasySave.Services
{
    public class BackupManager
    {
        private static BackupManager _instance;
        private static object _lock = new object();
        private List<BackupJob> _jobs;
        private IBackupRepository _repository;
        private int _maxJobs = 5;

        private BackupManager() { // TODO }

        public static BackupManager GetInstance() { throw new NotImplementedException(); } // TODO
        public bool AddBackupJob(BackupJob job) { throw new NotImplementedException(); } // TODO
        public bool RemoveBackupJob(int id) { throw new NotImplementedException(); } // TODO
        public BackupJob GetBackupJob(int id) { throw new NotImplementedException(); } // TODO
        public List<BackupJob> GetAllBackupJobs() { throw new NotImplementedException(); } // TODO
        public int GetJobCount() { throw new NotImplementedException(); } // TODO
    }
}
