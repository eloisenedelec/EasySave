using EasySave.Models;

namespace EasySave.Repositories
{
    public interface IBackupRepository
    {
        void Save(List<BackupJob> jobs);
        List<BackupJob> Load();
        bool Delete(int id);
    }
}
