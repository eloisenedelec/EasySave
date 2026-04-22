using EasySave.Models;
using EasySave.Services;

namespace EasySave.UI
{
    public class ConsoleUI
    {
        private readonly BackupManager _backupManager;
        private readonly BackupExecutor _backupExecutor;

        public ConsoleUI(BackupExecutor executor)
        {
            _backupManager = BackupManager.GetInstance();
            _backupExecutor = executor;
        }

        public void Run()
        {
            bool running = true;
            while (running)
            {
                DisplayMainMenu();
                switch (GetUserChoice())
                {
                    case 1: CreateBackupJobMenu(); break;
                    case 2: ExecuteBackupMenu(); break;
                    case 3: DisplayAllBackupsMenu(); break;
                    case 4: DeleteBackupMenu(); break;
                    case 5: running = false; break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        public void DisplayMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== EasySave ===");
            Console.WriteLine("1. Créer un job de sauvegarde");
            Console.WriteLine("2. Exécuter un job");
            Console.WriteLine("3. Afficher tous les jobs");
            Console.WriteLine("4. Supprimer un job");
            Console.WriteLine("5. Quitter");
            Console.Write("> ");
        }

        public void CreateBackupJobMenu()
        {
            Console.WriteLine("\n--- Nouveau job ---");

            string name = GetUserInput("Nom du job : ");
            string source = GetUserInput("Chemin source : ");
            string target = GetUserInput("Chemin destination : ");

            BackupType type = BackupType.Full;
            Console.WriteLine("Type de sauvegarde : 1) Complète  2) Différentielle");
            Console.Write("> ");
            if (GetUserChoice() == 2)
                type = BackupType.Differential;

            int id = _backupManager.GetJobCount() + 1;
            var job = new BackupJob(id, name, source, target, type);

            if (_backupManager.AddBackupJob(job))
                Console.WriteLine($"Job '{name}' créé (id={id}).");
            else
                Console.WriteLine("Limite de 5 jobs atteinte.");
        }

        public void ExecuteBackupMenu()
        {
            var jobs = _backupManager.GetAllBackupJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job disponible.");
                return;
            }

            Console.WriteLine("\n--- Exécuter un job ---");
            foreach (var j in jobs)
                Console.WriteLine($"  {j.Id}. {j.Name}  [{j.Type}]  {j.SourcePath} -> {j.TargetPath}");

            Console.Write("ID du job à exécuter (0 = tous) : ");
            int choice = GetUserChoice();

            if (choice == 0)
            {
                foreach (var j in jobs)
                    _backupExecutor.ExecuteBackup(j);
            }
            else
            {
                var job = _backupManager.GetBackupJob(choice);
                if (job != null)
                    _backupExecutor.ExecuteBackup(job);
                else
                    Console.WriteLine("Job introuvable.");
            }
        }

        public void DisplayAllBackupsMenu()
        {
            var jobs = _backupManager.GetAllBackupJobs();
            Console.WriteLine("\n--- Jobs de sauvegarde ---");
            if (jobs.Count == 0)
            {
                Console.WriteLine("Aucun job enregistré.");
                return;
            }
            foreach (var j in jobs)
                Console.WriteLine($"  [{j.Id}] {j.Name}  |  {j.Type}  |  {j.SourcePath}  ->  {j.TargetPath}");
        }

        public void DeleteBackupMenu()
        {
            DisplayAllBackupsMenu();
            if (_backupManager.GetJobCount() == 0) return;

            Console.Write("\nID du job à supprimer : ");
            int id = GetUserChoice();
            if (_backupManager.RemoveBackupJob(id))
                Console.WriteLine("Job supprimé.");
            else
                Console.WriteLine("Job introuvable.");
        }

        private int GetUserChoice()
        {
            if (int.TryParse(Console.ReadLine(), out int choice))
                return choice;
            return -1;
        }

        private string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }
    }
}
