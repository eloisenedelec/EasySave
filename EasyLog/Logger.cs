using System;
using System.IO;
using EasySave.Observers;
using EasyLog.Repositories;

namespace EasyLog
{
    public class Logger : IBackupObserver
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private string? _currentJobName;
        private ILogRepository _repository;

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
            _repository.Append(new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = _currentJobName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTime
            });
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
