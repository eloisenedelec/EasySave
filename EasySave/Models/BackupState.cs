using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Models
{
    public class BackupState
    {
        [JsonInclude] private string _jobName;
        [JsonInclude] private DateTime _timestamp;
        [JsonInclude] private JobStatus _status;
        [JsonInclude] private int _totalFiles;
        [JsonInclude] private long _totalSize;
        [JsonInclude] private int _filesProcessed;
        [JsonInclude] private int _filesRemaining;
        [JsonInclude] private long _sizeRemaining;
        [JsonInclude] private string _currentSourceFile;
        [JsonInclude] private string _currentTargetFile;

        public BackupState(string jobName) 
        {
            _jobName = jobName;
            _status = JobStatus.Inactive;
            _timestamp = DateTime.Now;
        }

        public void UpdateProgress(int filesProcessed, long sizeRemaining, string currentFile) 
        {
            _filesProcessed = filesProcessed;
            _sizeRemaining = sizeRemaining;
            _currentSourceFile = currentFile;

            _filesRemaining = _totalFiles - _filesProcessed;
            _timestamp = DateTime.Now;
        }
        public void SetStatus(JobStatus status)
        {
            _status = status;
            _timestamp = DateTime.Now;
        }
        public string ToJson() 
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
