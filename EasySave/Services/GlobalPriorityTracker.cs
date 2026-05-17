using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EasySave.Services
{
    public class GlobalPriorityTracker
    {
        private static readonly Lazy<GlobalPriorityTracker> _instance = new(() => new GlobalPriorityTracker());
        public static GlobalPriorityTracker Instance => _instance.Value;

        private readonly ConcurrentDictionary<int, int> _jobPriorityCounts = new();
        private readonly ManualResetEventSlim _priorityWaitHandle = new(true);
        private readonly object _lock = new object();

        private GlobalPriorityTracker() { }

        // 1. Enregistre les fichiers prioritaires au début d'un Job
        public void RegisterJobPriorityFiles(int jobId, int count)
        {
            if (count <= 0) return;

            lock (_lock)
            {
                _jobPriorityCounts.AddOrUpdate(jobId, count, (id, old) => old + count);
                _priorityWaitHandle.Reset();
            }
        }

        // 2. Appelé dès qu'un fichier prioritaire est fini
        public void PriorityFileFinished(int jobId)
        {
            lock (_lock)
            {
                if (_jobPriorityCounts.TryGetValue(jobId, out int count) && count > 0)
                {
                    int newCount = count - 1;
                    if (newCount <= 0)
                        _jobPriorityCounts.TryRemove(jobId, out _);
                    else
                        _jobPriorityCounts[jobId] = newCount;

                    if (_jobPriorityCounts.IsEmpty || _jobPriorityCounts.Values.All(v => v <= 0))
                    {
                        _priorityWaitHandle.Set();
                    }
                }
            }
        }

        // 3. Bloque les fichiers normaux si la barrière est fermée
        public bool WaitForPriorityIfNeeded(TimeSpan timeout, CancellationToken token)
        {
            try
            {
                return _priorityWaitHandle.Wait(timeout, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _jobPriorityCounts.Clear();
                _priorityWaitHandle.Set();
            }
        }

        // Pour les tests unitaires
        public void ResetForTests() => Reset();
    }
}