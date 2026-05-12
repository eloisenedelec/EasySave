using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Services
{
    public class GlobalPriorityTracker
    {
        private static readonly Lazy<GlobalPriorityTracker> _instance = new Lazy<GlobalPriorityTracker>(() => new GlobalPriorityTracker());
        public static GlobalPriorityTracker Instance => _instance.Value;
        private int _priorityFilesRemaining = 0;
        private readonly ManualResetEventSlim _priorityWaitHandle = new ManualResetEventSlim(true);
        private readonly object _lock = new object();

        public void AddPriorityFiles(int count)
        {
            if (count <= 0) return;

            lock (_lock)
            {
                _priorityFilesRemaining += count;
                _priorityWaitHandle.Reset();
            }
        }

        public void PriorityFileFinished()
        {
            lock (_lock)
            {
                if (_priorityFilesRemaining > 0)
                {
                    _priorityFilesRemaining--;

                    if (_priorityFilesRemaining == 0)
                    {
                        _priorityWaitHandle.Set();
                    }
                }
            }
        }

        public void WaitForPriorityIfNeeded()
        {
            _priorityWaitHandle.Wait();
        }
    }
}
