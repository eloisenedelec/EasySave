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
        private List<Process> _businessProcesses;

        private SettingsManager()
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "settings.json"
            );

            _repository = new JsonSettingsRepository(filePath);
            _businessProcesses = _repository.Load();
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

        public List<Process> GetAllProcesses()
        {
            return _businessProcesses.ToList();
        }

        public Process? GetProcess(int id)
        {
            return _businessProcesses.FirstOrDefault(p => p.Id == id);
        }

        public bool AddProcess(Process process)
        {
            if (process == null || string.IsNullOrWhiteSpace(process.Name))
            {
                return false;
            }

            if (process.Id <= 0)
            {
                process.Id = GetNextId();
            }

            if (_businessProcesses.Any(p => p.Id == process.Id))
            {
                return false;
            }

            _businessProcesses.Add(process);
            _repository.SaveProcess(_businessProcesses);
            return true;
        }

        public bool RemoveProcess(int id)
        {
            var process = _businessProcesses.FirstOrDefault(p => p.Id == id);
            if (process == null)
            {
                return false;
            }

            _businessProcesses.Remove(process);
            _repository.SaveProcess(_businessProcesses);
            return true;
        }

        private int GetNextId()
        {
            return _businessProcesses.Count == 0
                ? 1
                : _businessProcesses.Max(p => p.Id) + 1;
        }
    }
}