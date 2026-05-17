using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EasySave.Observers;
using EasyLog.Contracts;
using EasyLog.Repositories;


namespace EasyLog
{
    public class Logger : IBackupObserver
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private static readonly HttpClient _httpClient = new HttpClient();
        private string? _currentJobName;
        private ILogRepository _repository;
        private ILogSettings? _settings;

        public void Initialize(ILogSettings settings)
        {
            _settings = settings;
        }

        private Logger()
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Logs"
            );
            _repository = new JsonLogRepository(logDirectory);
        }

        public static Logger GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new Logger();
                }
            }
            return _instance;
        }

        public void SetRepository(ILogRepository repository)
        {
            _repository = repository;
        }

        public void SetFormat(string format)
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Logs"
            );
            _repository = format.Equals("xml", StringComparison.OrdinalIgnoreCase)
                ? new XmlLogRepository(logDirectory)
                : new JsonLogRepository(logDirectory);
        }

        public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            _currentJobName = jobName;
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = _currentJobName ?? string.Empty,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTime,
                EncryptionTimeMs = encryptionTimeMs,
                MachineName = Environment.MachineName,
                UserName = Environment.UserName
            };

            LogMode mode = _settings?.GetLogMode() ?? LogMode.Local;

            if (mode == LogMode.Local || mode == LogMode.Both)
                _repository.Append(entry);

            if (mode == LogMode.Centralized || mode == LogMode.Both)
                _ = SendToDockerAsync(_settings!.GetLogServerUrl(), entry);
        }

        private async Task SendToDockerAsync(string url, LogEntry entry)
        {
            try
            {
                await _httpClient.PostAsJsonAsync($"{url.TrimEnd('/')}/api/log", entry);
            }
            catch { }
        }

        public void OnBackupCompleted(string jobName) { }

        public void OnBackupError(string jobName, string error)
        {
            _repository.Append(new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourceFile = error,
                TargetFile = string.Empty,
                FileSize = 0,
                TransferTimeMs = -1
            });
        }
    }
}
