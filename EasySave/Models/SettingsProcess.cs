using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class SettingsProcess
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public SettingsProcess()
        {
        }

        public SettingsProcess(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
