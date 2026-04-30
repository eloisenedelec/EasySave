using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Repositories
{
    public class JsonBackupRepository : IBackupRepository
    {
        private string _filePath;

        public JsonBackupRepository(string filePath) 
        {
            _filePath = filePath;

            string? directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory)) 
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Save(List<BackupJob> jobs) 
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(jobs, options);

            File.WriteAllText(_filePath, jsonString);
        }
        public List<BackupJob> Load() 
        { 
            if (!File.Exists(_filePath))
            {
                return new List<BackupJob>();
            }

            string jsonString = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<BackupJob>();
            }

            return JsonSerializer.Deserialize<List<BackupJob>>(jsonString) ?? new List<BackupJob>();
        }
        public bool Delete(int id) 
        {
            List<BackupJob> jobs = Load();

            BackupJob? jobToRemove = jobs.Find(j => j.Id == id);

            if (jobToRemove != null)
            {
                jobs.Remove(jobToRemove);
                Save(jobs);
                return true;
            }

            return false;
        }
    }
}
