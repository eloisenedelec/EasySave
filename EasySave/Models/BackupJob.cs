using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class BackupJob
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string SourcePath { get; init; } = string.Empty;
        public string TargetPath { get; init; } = string.Empty;
        public BackupType Type { get; init; }

        [JsonConstructor]
        public BackupJob() { }

        public BackupJob(int id, string name, string source, string target, BackupType type)
        {
            Id = id;
            Name = name;
            SourcePath = source;
            TargetPath = target;
            Type = type;
        }
    }
}
