using EasySave.Models;
using EasySave.Repositories;
using System.Linq;

namespace EasySave.Services
{
    public class SettingsManager
    {
        private static SettingsManager? _instance;
        private static readonly object _lock = new object();

        private readonly ISettingsRepository _repository;
        private GlobalSettings _settings;

        private SettingsManager()
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "settings.json"
            );

            _repository = new JsonSettingsRepository(filePath);
            _settings = _repository.Load();
        }

        public static SettingsManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SettingsManager();
                    }
                }
            }
            return _instance;
        }

        // 1. GESTION DES LOGICIELS MÉTIERS
        public List<SettingsProcess> GetAllProcesses() => _settings.BusinessProcesses.ToList();

        public SettingsProcess? GetProcess(int id) => _settings.BusinessProcesses.FirstOrDefault(p => p.Id == id);

        public bool AddProcess(SettingsProcess process)
        {
            if (process == null || string.IsNullOrWhiteSpace(process.Name)) return false;

            if (process.Id <= 0) process.Id = GetNextId();

            if (_settings.BusinessProcesses.Any(p => p.Id == process.Id)) return false;

            _settings.BusinessProcesses.Add(process);
            _repository.Save(_settings);
            return true;
        }

        public bool RemoveProcess(int id)
        {
            var process = _settings.BusinessProcesses.FirstOrDefault(p => p.Id == id);
            if (process == null) return false;

            _settings.BusinessProcesses.Remove(process);
            _repository.Save(_settings);
            return true;
        }

        private int GetNextId() => _settings.BusinessProcesses.Count == 0
            ? 1
            : _settings.BusinessProcesses.Max(p => p.Id) + 1;


        // 2. GESTION DU FORMAT DE LOG (JSON / XML)
        public string GetLogFormat() => _settings.LogFormat;

        public void SetLogFormat(string format)
        {
            if (format.ToUpper() == "JSON" || format.ToUpper() == "XML")
            {
                _settings.LogFormat = format.ToUpper();
                _repository.Save(_settings);
            }
        }


        // 3. GESTION DES EXTENSIONS À CHIFFRER
        public List<string> GetEncryptedExtensions() => _settings.EncryptedExtensions.ToList();

        public void AddExtension(string extension)
        {
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (!extension.StartsWith(".")) extension = "." + extension;

                if (!_settings.EncryptedExtensions.Contains(extension))
                {
                    _settings.EncryptedExtensions.Add(extension);
                    _repository.Save(_settings);
                }
            }
        }

        public void RemoveExtension(string extension)
        {
            if (!extension.StartsWith(".")) extension = "." + extension;

            if (_settings.EncryptedExtensions.Contains(extension))
            {
                _settings.EncryptedExtensions.Remove(extension);
                _repository.Save(_settings);
            }
        }
    }
}