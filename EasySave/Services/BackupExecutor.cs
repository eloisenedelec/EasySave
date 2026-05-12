using EasySave.Factories;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Strategies;
using EasyLog.Contracts;
using System.IO;

namespace EasySave.Services
{
    public class BackupExecutor : IBackupObserver
    {
        private List<IBackupObserver> _observers;
        private readonly CryptoSoftManager _cryptoManager = new();

        public BackupExecutor()
        {
            _observers = new List<IBackupObserver>();
        }

        public void AddObserver(IBackupObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
        public void RemoveObserver(IBackupObserver observer)
        {
            _observers.Remove(observer);
        }
        public void ExecuteBackup(BackupJob job)
        {
            // Utilisation du SettingsManager pour vérifier les extensions à chiffrer
            var settings = SettingsManager.GetInstance();
            var encryptedExtensions = settings.GetEncryptedExtensions();

            // Monitoring existant
            ProcessMonitoring pm = new ProcessMonitoring(settings);
            if (!pm.AreNoBusinessProcessesRunning())
            {
                NotifyBackupError(job.Name, "Annulation : Logiciel métier en cours d'exécution.");
                return;
            }

            try
            {
                // On passe désormais 'this' (l'executor) à la stratégie pour qu'elle utilise nos méthodes
                IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(job.Type);
                strategy.Execute(job, this);
            }
            catch (Exception ex)
            {
                NotifyBackupError(job.Name, ex.Message);
            }
        }

        // NOUVELLE MÉTHODE : Centralise la logique de copie et le chiffrement
        public void ProcessFileCopy(string source, string target)
        {
            var settings = SettingsManager.GetInstance();
            string extension = Path.GetExtension(source).ToLower();

            // Vérifie si l'extension doit être chiffrée selon settings.json
            bool shouldEncrypt = settings.GetEncryptedExtensions().Contains(extension);

            if (shouldEncrypt)
            {
                // Utilise le CryptoSoftManager avec son Mutex global
                _cryptoManager.EncryptFile(source, target);
            }
            else
            {
                // Copie standard
                File.Copy(source, target, true);
            }
        }
        public void ExecuteMultipleBackups(List<int> jobIds)
        {
            BackupManager manager = BackupManager.GetInstance();
            ProcessMonitoring pm = new ProcessMonitoring(SettingsManager.GetInstance());

            foreach (int id in jobIds)
            {
                if (!pm.AreNoBusinessProcessesRunning())
                {
                    break;
                }

                var job = manager.GetBackupJob(id);
                if (job != null)
                {
                    ExecuteBackup(job);
                }
            }
        }

        public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            NotifyFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTimeMs);
        }
        public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            NotifyBackupStarted(jobName, totalFiles, totalSize);
        }
        public void OnBackupCompleted(string jobName)
        {
            NotifyBackupCompleted(jobName);
        }
        public void OnBackupError(string jobName, string error)
        {
            NotifyBackupError(jobName, error);
        }

        private void NotifyFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
        {
            _observers.ForEach(o => o.OnFileProcessed(sourceFile, targetFile, fileSize, transferTime, encryptionTimeMs));
        }
        private void NotifyBackupStarted(string jobName, int totalFiles, long totalSize)
        {
            _observers.ForEach(o => o.OnBackupStarted(jobName, totalFiles, totalSize));
        }
        private void NotifyBackupCompleted(string jobName)
        {
            _observers.ForEach(o => o.OnBackupCompleted(jobName));
        }
        private void NotifyBackupError(string jobName, string error)
        {
            _observers.ForEach(o => o.OnBackupError(jobName, error));
        }


    }
}
