using System;
using System.Text.Json;

namespace EasySave.Models
{
    public class BackupState
    {
        public string JobName { get; private set; }
        public DateTime Timestamp { get; set; }
        public JobStatus Status { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesProcessed { get; set; }
        public int FilesRemaining { get; set; }
        public long SizeRemaining { get; set; }
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }

        public BackupState(string jobName)
        {
            JobName = jobName;
            Status = JobStatus.Inactive;
            Timestamp = DateTime.Now;
        }
        public string ToJson() 
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
