using System.Text.Json;
using EasyLog.Contracts;


namespace EasyLog.Repositories
{
    public class JsonLogRepository : ILogRepository
    {
        private readonly string _logDirectory;
        private readonly object _lock = new object();

        public JsonLogRepository(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        public void Append(LogEntry entry)
        {
            string filePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");
            lock (_lock)
            {
                List<LogEntry> entries = new();
                if (File.Exists(filePath))
                {
                    string existing = File.ReadAllText(filePath);
                    entries = JsonSerializer.Deserialize<List<LogEntry>>(existing) ?? new();
                }
                entries.Add(entry);
                File.WriteAllText(filePath, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
