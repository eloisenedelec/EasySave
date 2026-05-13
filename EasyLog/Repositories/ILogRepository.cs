using EasyLog.Contracts;

namespace EasyLog.Repositories
{
    public interface ILogRepository
    {
        void Append(LogEntry entry);
    }
}
