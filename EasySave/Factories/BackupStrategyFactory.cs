using EasySave.Models;
using EasySave.Strategies;

namespace EasySave.Factories
{
    public static class BackupStrategyFactory
    {
        public static IBackupStrategy CreateStrategy(BackupType type) {
            return type switch
            {
                BackupType.Full => new FullBackupStrategy(),
                BackupType.Differential => new DiffBackupStrategy(),
                _ => throw new ArgumentException("Invalid backup type")
            };
        }
    }
}
