using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class StateManager : IBackupObserver
    {
        private static StateManager _instance;
        private static object _lock = new object();
        private string _stateFilePath;
        private Dictionary<string, BackupState> _states;

        private StateManager() { // TODO }

        public static StateManager GetInstance() { throw new NotImplementedException(); } // TODO
        public void UpdateState(BackupState state) { // TODO }
        public BackupState GetState(string jobName) { throw new NotImplementedException(); } // TODO
        public void OnFileProcessed(string fileName, long fileSize, long transferTime) { // TODO }
        public void OnBackupStarted(string jobName, int totalFiles, long totalSize) { // TODO }
        public void OnBackupCompleted(string jobName) { // TODO }
        public void OnBackupError(string jobName, string error) { // TODO }

        private void SaveToFile() { // TODO }
    }
}
