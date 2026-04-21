namespace EasyLog
{
    public class Logger
    {
        private static Logger _instance;
        private static object _lock = new object();
        private string _logDirectoryPath;

        private Logger() { // TODO }

        public static Logger GetInstance() { throw new NotImplementedException(); } // TODO
        public void OnFileProcessed(string fileName, long fileSize, long transferTime) { // TODO }
        public void OnBackupStarted(string jobName, int totalFiles, long totalSize) { // TODO }
        public void OnBackupCompleted(string jobName) { // TODO }
        public void OnBackupError(string jobName, string error) { // TODO }

        private string GetDailyLogFileName() { throw new NotImplementedException(); } // TODO
        private void WriteToFile(LogEntry entry) { // TODO }
    }
}
