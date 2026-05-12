using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyLog.Contracts;

namespace EasySave.Models
{
    public class GlobalSettings
    {
        public List<SettingsProcess> BusinessProcesses { get; set; } = new List<SettingsProcess>();
        public string LogFormat { get; set; } = "JSON";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();
        public List<string> PriorityExtensions { get; set; } = new();
        public int LargeFileThresholdKb { get; set; }
        public LogMode LogMode { get; set; } = LogMode.Local;
        public string LogServerUrl { get; set; } = "http://localhost:5000";
    }
}
