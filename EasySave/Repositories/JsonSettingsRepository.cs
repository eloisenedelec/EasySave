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

        public GlobalSettings Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new GlobalSettings();
            }

            string jsonString = File.ReadAllText(_settingsFilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new GlobalSettings();
            }

            return JsonSerializer.Deserialize<GlobalSettings>(jsonString) ?? new GlobalSettings();
        }

        public void Save(GlobalSettings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsFilePath, jsonString);
        }
    }
}