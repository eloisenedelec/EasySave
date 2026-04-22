using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Observers;

namespace EasyLog
{
    public class Logger : IBackupObserver
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private string _logDirectoryPath;
        private string? _currentJobName;

        private Logger() {
            _logDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Logs"
            );
            Directory.CreateDirectory(_logDirectoryPath);
        }

        public static Logger GetInstance() {
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

        public void OnBackupStarted(string jobName, int totalFiles, long totalSize) {
            _currentJobName = jobName;
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime) {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = _currentJobName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTime
            };
            WriteToFile(entry);
        }

        public void OnBackupCompleted(string jobName) { }

        public void OnBackupError(string jobName, string error) {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourceFile = error,
                TargetFile = string.Empty,
                FileSize = 0,
                TransferTimeMs = -1
            };
            WriteToFile(entry);
        }

        private string GetDailyLogFileName() {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(_logDirectoryPath, $"{date}.json");
        }

        private void WriteToFile(LogEntry entry) {
            string logFilePath = GetDailyLogFileName();
            lock (_lock)
            {
                try
                {
                    List<LogEntry> entries;
                    if (File.Exists(logFilePath))
                    {
                        string existingJson = File.ReadAllText(logFilePath);
                        entries = JsonSerializer.Deserialize<List<LogEntry>>(existingJson)
                                  ?? new List<LogEntry>();
                    }
                    else
                    {
                        entries = new List<LogEntry>();
                    }

                    entries.Add(entry);

                    string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(logFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger] Erreur d'écriture : {ex.Message}");
                }
            }
        }
    }
}
