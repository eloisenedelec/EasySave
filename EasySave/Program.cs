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

            new ConsoleUI(executor).Run();
        }
    }
}
