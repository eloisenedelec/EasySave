using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasyLog.Contracts;

namespace EasyLog
{
    public class Logger
    {
        private static Logger _instance;
        private static object _lock = new object();
        private string _logDirectoryPath;
        private string _currentJobName;

        private Logger() {

            _logDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Logs"
            );

            Directory.CreateDirectory(_logDirectoryPath);

            Console.WriteLine($"[Logger] Dossier de logs : {_logDirectoryPath}");

        }

        public static Logger GetInstance() {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }
            }
            return _instance;
        } 

        public void OnFileProcessed(string fileName, long fileSize, long transferTime) {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = _currentJobName,
                SourceFile = fileName,
                TargetFile = string.Empty, // TODO: sera rempli par BackupExecutor plus tard
                FileSize = fileSize,
                TransferTimeMs = transferTime
            };

           
            WriteToFile(entry);
        }

        public void OnBackupStarted(string jobName, int totalFiles, long totalSize) {
            _currentJobName = jobName;
            Console.WriteLine($"[Logger] Sauvegarde '{jobName}' d�marr�e ({totalFiles} fichiers, {totalSize} octets)");
        }

        public void OnBackupCompleted(string jobName) { 
        }
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
            Console.WriteLine($"[Logger] Erreur dans '{jobName}': {error}");
        }

        private string GetDailyLogFileName() {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            return Path.Combine(_logDirectoryPath, $"{date}.json");
        } 
        private void WriteToFile(LogEntry entry) {
            string logFilePath = GetDailyLogFileName();

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

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(entries, options);

                File.WriteAllText(logFilePath, json);

                Console.WriteLine($"[Logger] Entr�e ajout�e : {entry.SourceFile} ({entry.FileSize} octets, {entry.TransferTimeMs} ms)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Erreur d'�criture : {ex.Message}");
            }
        }
    }
}
