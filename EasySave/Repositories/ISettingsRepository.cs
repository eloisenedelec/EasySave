using EasySave.Models;

namespace EasySave.Repositories
{
    public interface ISettingsRepository
    {
        List<SettingsProcess> Load();
        void SaveProcess(List<SettingsProcess> processes);
        bool DeleteProcess(int id);
    }
}