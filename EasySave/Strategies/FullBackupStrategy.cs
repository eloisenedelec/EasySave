using EasySave.Observers;

namespace EasySave.Strategies
{
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(string sourcePath, string targetPath, IBackupObserver observer) { // TODO }

        private void CopyAllFiles(string source, string target, IBackupObserver observer) { // TODO }
    }
}
