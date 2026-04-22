using System.IO;
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
         
        private BackupManager() {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "jobs.json"
            );
            _repository = new JsonBackupRepository(filePath);
            _jobs = _repository.Load() ?? new List<BackupJob>();

        }

        public static BackupManager GetInstance() {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new BackupManager();
                    }
                }
            }
            return _instance;
        }
        public bool AddBackupJob(BackupJob job) {
            if (_jobs.Count >= _maxJobs) return false;

            _jobs.Add(job);
            _repository.Save(_jobs);
            return true;

        }
        public bool RemoveBackupJob(int id) {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                _jobs.Remove(job);
                _repository.Save(_jobs);
                return true;
            }
            return false;
        }
        public BackupJob GetBackupJob(int id) { return _jobs.FirstOrDefault(j => j.Id == id); }
        public List<BackupJob> GetAllBackupJobs() { return _jobs; } // TODO
        public int GetJobCount() { return _jobs.Count; } // TODO
    }
}
