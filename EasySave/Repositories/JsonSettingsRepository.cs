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

        public List<Process> Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new List<Process>();
            }

            string jsonString = File.ReadAllText(_settingsFilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<Process>();
            }

            return JsonSerializer.Deserialize<List<Process>>(jsonString) ?? new List<Process>();
        }

        public void SaveProcess(List<Process> processes)
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