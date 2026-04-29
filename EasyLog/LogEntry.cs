using System;
using System.Text.Json;

namespace EasyLog
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string JobName { get; set; }
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public long FileSize { get; set; }
        public long TransferTimeMs { get; set; }
        public long EncryptionTimeMs { get; set; }

        public LogEntry()
        {
            Timestamp = DateTime.Now;
            JobName = string.Empty;
            SourceFile = string.Empty;
            TargetFile = string.Empty;
        }

        public string ToJson() 
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // opt retours à la ligne 
            };

            return JsonSerializer.Serialize(this, options);
        }
    }
}
