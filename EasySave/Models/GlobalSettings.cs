using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Models
{
    public class GlobalSettings
    {
        public List<SettingsProcess> BusinessProcesses { get; set; } = new List<SettingsProcess>();
        public string LogFormat { get; set; } = "JSON";
        public List<string> EncryptedExtensions { get; set; } = new List<string>();
    }
}
