using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class LargeFileCoordinator
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task RequestPermissionAsync(long fileSize, long limit)
        {
            if (fileSize > limit)
                await _semaphore.WaitAsync();
        }

        public void RequestPermission(long fileSize, long limit)
        {
            if (fileSize > limit)
                _semaphore.Wait();
        }

        public void ReleasePermission(long fileSize, long limit)
        {
            if (fileSize > limit)
                _semaphore.Release();
        }
    }
}