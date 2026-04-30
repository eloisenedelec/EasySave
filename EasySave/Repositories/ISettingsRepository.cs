using EasySave.Models;

namespace EasySave.Repositories
{
    public interface ISettingsRepository
    {
        GlobalSettings Load();
        void Save(GlobalSettings settings);
    }
}