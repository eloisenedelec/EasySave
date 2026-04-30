using System.Text.Json;
using EasySave.Models;

namespace EasySave.Repositories
{
    public class JsonSettingsRepository : ISettingsRepository
    {
        private readonly string _settingsFilePath;

        public JsonSettingsRepository(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;

            string? directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public List<SettingsProcess> Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new List<SettingsProcess>();
            }

            string jsonString = File.ReadAllText(_settingsFilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<SettingsProcess>();
            }

            return JsonSerializer.Deserialize<List<SettingsProcess>>(jsonString) ?? new List<SettingsProcess>();
        }

        public void SaveProcess(List<SettingsProcess> processes)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(processes, options);
            File.WriteAllText(_settingsFilePath, jsonString);
        }

        public bool DeleteProcess(int id)
        {
            var processes = Load();

            var processToRemove = processes.FirstOrDefault(p => p.Id == id);
            if (processToRemove == null)
            {
                return false;
            }

            processes.Remove(processToRemove);
            SaveProcess(processes);
            return true;
        }
    }
}