using System;
using System.IO;
using EasySave.Observers;
using EasyLog.Repositories;
using System.Net.Http.Json;
using EasyLog.Contracts;


namespace EasyLog
{
    public class Logger : IBackupObserver
    {
        private static Logger? _instance;
        private static readonly object _lock = new object();
        private static readonly HttpClient _httpClient = new HttpClient();
        private string? _currentJobName;
        private ILogRepository _repository;
        private ILogSettings _settings;

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
            // 1. On prépare l'entrée de log (avec les nouveaux champs Machine/User)
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = _currentJobName,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                FileSize = fileSize,
                TransferTimeMs = transferTime,
                EncryptionTimeMs = encryptionTimeMs,
                MachineName = Environment.MachineName, // Automatique via ton constructeur LogEntry
                UserName = Environment.UserName        // Automatique via ton constructeur LogEntry
            };

            if (_settings != null)
            {
                LogMode mode = _settings.GetLogMode();
                // TEST 1 : Est-ce qu'on entre ici ?
                // MessageBox.Show($"Mode détecté : {mode}"); 

                if (mode == LogMode.Centralized || mode == LogMode.Both)
                {
                    // TEST 2 : Quelle URL on utilise ?
                    // MessageBox.Show($"URL : {_settings.GetLogServerUrl()}");
                    SendToDocker(_settings.GetLogServerUrl(), entry);
                }
            }
        }

        private async void SendToDocker(string url, LogEntry entry)
        {
            try
            {
                Console.WriteLine($"Tentative d'envoi vers : {url.TrimEnd('/')}/api/log");
                var response = await _httpClient.PostAsJsonAsync($"{url.TrimEnd('/')}/api/log", entry);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Log envoyé avec succès !");
                }
                else
                {
                    Console.WriteLine($"Échec du serveur : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // ICI : On affiche enfin l'erreur dans ta console de debug
                Console.WriteLine($"ERREUR HTTP : {ex.Message}");
            }
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
