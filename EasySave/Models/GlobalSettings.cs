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
        public string Language { get; set; } = "fr";
        public string LogFormat { get; set; } = "JSON";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();
        public LogMode LogMode { get; set; } = LogMode.Local;
        public string LogServerUrl { get; set; } = "http://localhost:5050";
        public List<string> PriorityExtensions { get; set; } = new List<string>();
        public long LargeFileSizeLimit { get; set; } = 100 * 1024;
    }
}
