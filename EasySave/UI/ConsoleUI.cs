using EasySave.Models;
using EasySave.Services;

namespace EasySave.UI
{
    public class ConsoleUI
    {
        private readonly BackupManager _backupManager;
        private readonly BackupExecutor _backupExecutor;
        private readonly LanguageManager _lang;

        public ConsoleUI(BackupExecutor executor)
        {
            _backupManager = BackupManager.GetInstance();
            _backupExecutor = executor;
            _lang = LanguageManager.GetInstance();
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
                    case 5: ChangeLanguageMenu(); break;
                    case 6: running = false; break;
                    default:
                        Console.WriteLine(_lang.GetText("invalid_choice"));
                        break;
                }
            }
        }

        public void DisplayMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine(_lang.GetText("menu_title"));
            Console.WriteLine(_lang.GetText("menu_create"));
            Console.WriteLine(_lang.GetText("menu_execute"));
            Console.WriteLine(_lang.GetText("menu_list"));
            Console.WriteLine(_lang.GetText("menu_delete"));
            Console.WriteLine(_lang.GetText("menu_language"));
            Console.WriteLine(_lang.GetText("menu_quit"));
            Console.Write("> ");
        }

        public void CreateBackupJobMenu()
        {
            Console.WriteLine($"\n{_lang.GetText("create_title")}");

            string name   = GetUserInput(_lang.GetText("create_name"));
            string source = GetUserInput(_lang.GetText("create_source"));
            string target = GetUserInput(_lang.GetText("create_target"));

            Console.WriteLine(_lang.GetText("create_type_prompt"));
            Console.Write("> ");
            BackupType type = GetUserChoice() == 2 ? BackupType.Differential : BackupType.Full;

            int id = _backupManager.GetJobCount() + 1;
            var job = new BackupJob(id, name, source, target, type);

            if (_backupManager.AddBackupJob(job))
                Console.WriteLine(_lang.GetText("create_success"));
            else
                Console.WriteLine(_lang.GetText("create_limit"));
        }

        public void ExecuteBackupMenu()
        {
            var jobs = _backupManager.GetAllBackupJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine(_lang.GetText("execute_none"));
                return;
            }

            Console.WriteLine($"\n{_lang.GetText("execute_title")}");
            foreach (var j in jobs)
                Console.WriteLine($"  {j.Id}. {j.Name}  [{j.Type}]  {j.SourcePath} -> {j.TargetPath}");

            Console.Write(_lang.GetText("execute_prompt"));
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
                    Console.WriteLine(_lang.GetText("execute_not_found"));
            }
        }

        public void DisplayAllBackupsMenu()
        {
            Console.WriteLine($"\n{_lang.GetText("list_title")}");
            var jobs = _backupManager.GetAllBackupJobs();
            if (jobs.Count == 0)
            {
                Console.WriteLine(_lang.GetText("list_empty"));
                return;
            }
            foreach (var j in jobs)
                Console.WriteLine($"  [{j.Id}] {j.Name}  |  {j.Type}  |  {j.SourcePath}  ->  {j.TargetPath}");
        }

        public void DeleteBackupMenu()
        {
            DisplayAllBackupsMenu();
            if (_backupManager.GetJobCount() == 0) return;

            Console.Write(_lang.GetText("delete_prompt"));
            int id = GetUserChoice();
            if (_backupManager.RemoveBackupJob(id))
                Console.WriteLine(_lang.GetText("delete_success"));
            else
                Console.WriteLine(_lang.GetText("delete_not_found"));
        }

        private void ChangeLanguageMenu()
        {
            Console.WriteLine($"\n{_lang.GetText("language_title")}");
            Console.WriteLine(_lang.GetText("language_prompt"));
            Console.Write("> ");
            string code = GetUserChoice() == 2 ? "en" : "fr";
            _lang.LoadLanguage(code);
            Console.WriteLine(_lang.GetText("language_changed"));
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
