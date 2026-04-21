namespace EasySave.Models
{
    public class BackupState
    {
        private string _jobName;
        private DateTime _timestamp;
        private JobStatus _status;
        private int _totalFiles;
        private long _totalSize;
        private int _filesProcessed;
        private int _filesRemaining;
        private long _sizeRemaining;
        private string _currentSourceFile;
        private string _currentTargetFile;

        public BackupState(string jobName) { // TODO }

        public void UpdateProgress(int filesProcessed, long sizeRemaining, string currentFile) { // TODO }
        public void SetStatus(JobStatus status) { // TODO }
        public string ToJson() { throw new NotImplementedException(); } // TODO
    }
}
