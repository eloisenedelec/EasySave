using System.Xml.Serialization;
using EasyLog.Contracts;


namespace EasyLog.Repositories
{
    public class XmlLogRepository : ILogRepository
    {
        private readonly string _logDirectory;
        private readonly object _lock = new object();
        private readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<LogEntry>));

        public XmlLogRepository(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        public void Append(LogEntry entry)
        {
            string filePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyy-MM-dd}.xml");
            lock (_lock)
            {
                List<LogEntry> entries = new();
                if (File.Exists(filePath))
                {
                    using var reader = new StringReader(File.ReadAllText(filePath));
                    entries = (List<LogEntry>?)_serializer.Deserialize(reader) ?? new();
                }
                entries.Add(entry);
                using var writer = new StringWriter();
                _serializer.Serialize(writer, entries);
                File.WriteAllText(filePath, writer.ToString());
            }
        }
    }
}
