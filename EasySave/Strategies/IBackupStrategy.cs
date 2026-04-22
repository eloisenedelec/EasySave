using EasySave.Observers;

namespace EasySave.Strategies
{
    public interface IBackupStrategy
    {
        void Execute(string sourcePath, string targetPath, IBackupObserver observer);
    }
}
