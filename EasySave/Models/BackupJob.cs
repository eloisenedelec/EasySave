using System;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class BackupJob
    {
        [JsonInclude] private int _id;
        [JsonInclude] private string _name;
        [JsonInclude] private string _sourcePath;
        [JsonInclude] private string _targetPath;
        [JsonInclude] private BackupType _type;

        [JsonConstructor] public BackupJob() { }

        public BackupJob(int id, string name, string source, string target, BackupType type)
        {
            _id = id;
            _name = name;
            _sourcePath = source;
            _targetPath = target;
            _type = type;
        }

        public int GetId() { return _id; }
        public string GetName() { return _name; }
        public string GetSourcePath() { return _sourcePath; }
        public string GetTargetPath() { return _targetPath; }
        public BackupType GetType() { return _type; }
    }
}
