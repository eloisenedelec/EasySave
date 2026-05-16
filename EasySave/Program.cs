using EasySave.Models;
using EasySave.Services;
using EasySave.UI;
using EasyLog;

namespace EasySave
{
    class Program
    {
        public static void Main(string[] args)
        {
            var executor = new BackupExecutor();
            executor.AddObserver(Logger.GetInstance());
            executor.AddObserver(StateManager.GetInstance());

            if (args.Length > 0)
            {
                RunCli(executor, args);
            }
            else
            {
                new ConsoleUI(executor).Run();
            }
        }

        private static void RunCli(BackupExecutor executor, string[] args)
        {
            var manager = BackupManager.GetInstance();

            switch (args[0].ToLower())
            {
                case "list":
                    var jobs = manager.GetAllBackupJobs();
                    if (jobs.Count == 0) { Console.WriteLine("Aucun job de sauvegarde configuré."); break; }
                    foreach (var job in jobs)
                        Console.WriteLine($"[{job.Id}] {job.Name} | {job.Type} | {job.SourcePath} -> {job.TargetPath}");
                    break;

                case "execute":
                    if (args.Length < 2) { Console.WriteLine("Usage: EasySave.exe execute <all|id1,id2,...>"); break; }
                    if (args[1].ToLower() == "all")
                    {
                        var allIds = manager.GetAllBackupJobs().Select(j => j.Id).ToList();
                        if (allIds.Count == 0) { Console.WriteLine("Aucun job à exécuter."); break; }
                        executor.ExecuteMultipleBackups(allIds);
                        Console.WriteLine("Tous les jobs ont été exécutés.");
                    }
                    else
                    {
                        var ids = args[1].Split(',')
                            .Select(s => int.TryParse(s.Trim(), out int id) ? (int?)id : null)
                            .Where(id => id.HasValue).Select(id => id!.Value).ToList();
                        if (ids.Count == 0) { Console.WriteLine("Aucun ID valide fourni."); break; }
                        executor.ExecuteMultipleBackups(ids);
                        Console.WriteLine($"Job(s) {string.Join(", ", ids)} exécuté(s).");
                    }
                    break;

                case "create":
                    // EasySave.exe create --name "NomJob" --source "C:\src" --target "D:\dst" --type full
                    var parsed = ParseFlags(args[1..]);
                    if (!parsed.TryGetValue("name", out var name) ||
                        !parsed.TryGetValue("source", out var source) ||
                        !parsed.TryGetValue("target", out var target))
                    {
                        Console.WriteLine("Usage: EasySave.exe create --name <nom> --source <chemin> --target <chemin>");
                        break;
                    }
                    int newId = manager.GetJobCount() + 1;
                    var newJob = new BackupJob(newId, name, source, target, BackupType.Full);
                    if (manager.AddBackupJob(newJob))
                        Console.WriteLine($"Job '{name}' créé avec l'ID {newId} (type: {type}).");
                    else
                        Console.WriteLine("Impossible de créer le job : limite de 5 jobs atteinte.");
                    break;

                case "delete":
                    // EasySave.exe delete <id>
                    if (args.Length < 2 || !int.TryParse(args[1], out int deleteId))
                    {
                        Console.WriteLine("Usage: EasySave.exe delete <id>");
                        break;
                    }
                    if (manager.RemoveBackupJob(deleteId))
                        Console.WriteLine($"Job {deleteId} supprimé.");
                    else
                        Console.WriteLine($"Job {deleteId} introuvable.");
                    break;

                default:
                    Console.WriteLine("Commandes disponibles :");
                    Console.WriteLine("  EasySave.exe list");
                    Console.WriteLine("  EasySave.exe execute all");
                    Console.WriteLine("  EasySave.exe execute <id1,id2,...>");
                    Console.WriteLine("  EasySave.exe create --name <nom> --source <chemin> --target <chemin>");
                    Console.WriteLine("  EasySave.exe delete <id>");
                    break;
            }
        }

        private static Dictionary<string, string> ParseFlags(string[] args)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    string key = args[i][2..];
                    string value = args[i + 1];
                    result[key] = value;
                    i++;
                }
            }
            return result;
        }
    }
}
