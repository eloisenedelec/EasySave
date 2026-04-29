using EasySave.Models;

namespace EasySave.Repositories
{
    public interface ISettingsRepository
    {
        List<Process> Load();
        void SaveProcess(List<Process> processes);
        bool DeleteProcess(int id);
    }
}