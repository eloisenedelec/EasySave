using System;
using System.Diagnostics;

namespace EasySave.Services
{
    public class ProcessWatcher
    {
        public bool IsBusinessSoftwareRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            string cleanName = processName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

            Process[] processes = Process.GetProcessesByName(cleanName);

            return processes.Length > 0;
        }
    }
}