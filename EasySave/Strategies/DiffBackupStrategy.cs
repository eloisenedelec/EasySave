using EasySave.Observers;

namespace EasySave.Strategies
{
    public class DiffBackupStrategy : IBackupStrategy
    {
        public void Execute(string sourcePath, string targetPath, IBackupObserver observer) { // TODO }

        private void CopyModifiedFiles(string source, string target, IBackupObserver observer) { // TODO }
        private bool IsFileModified(string sourceFile, string targetFile) { throw new NotImplementedException(); } // TODO
    }
}
