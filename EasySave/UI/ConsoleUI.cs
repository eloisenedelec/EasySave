using EasySave.Services;

namespace EasySave.UI
{
    public class ConsoleUI
    {
        private LanguageManager _languageManager;
        private BackupManager _backupManager;
        private BackupExecutor _backupExecutor;

        public ConsoleUI() { // TODO }

        public void Run() { // TODO }
        public void DisplayMainMenu() { // TODO }
        public void CreateBackupJobMenu() { // TODO }
        public void ExecuteBackupMenu() { // TODO }
        public void DisplayAllBackupsMenu() { // TODO }
        public void DeleteBackupMenu() { // TODO }

        private int GetUserChoice() { throw new NotImplementedException(); } // TODO
        private string GetUserInput(string prompt) { throw new NotImplementedException(); } // TODO
    }
}
