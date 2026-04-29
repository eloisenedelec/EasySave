using EasySave.Models;
using System.Diagnostics;

namespace EasySave.Services
{
    public class ProcessMonitoring
    {
        private readonly SettingsManager _settingsManager;

        public ProcessMonitoring(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public bool AreNoBusinessProcessesRunning()
        {
            var configuredProcesses = _settingsManager.GetAllProcesses();

            if (configuredProcesses.Count == 0)
            {
                return true;
            }

            foreach (var configuredProcess in configuredProcesses)
            {
                if (IsProcessRunning(configuredProcess.Name))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }

            string cleanName = processName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);
            return Process.GetProcessesByName(cleanName).Length > 0;
        }
    }
}