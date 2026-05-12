using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EasySave.Services
{
    public class GlobalPriorityTracker
    {
        // Singleton : une seule instance pour toute l'app
        private static readonly Lazy<GlobalPriorityTracker> _instance = new(() => new GlobalPriorityTracker());
        public static GlobalPriorityTracker Instance => _instance.Value;

        // Dictionnaire pour savoir quel Job (ID) bloque avec combien de fichiers
        private readonly ConcurrentDictionary<int, int> _jobPriorityCounts = new();

        // La barrière qui bloque ou laisse passer les threads
        private readonly ManualResetEventSlim _priorityWaitHandle = new(true);
        private readonly object _lock = new object();

        private GlobalPriorityTracker() { }

        // 1. Enregistre les fichiers prioritaires au début d'un Job
        public void RegisterJobPriorityFiles(int jobId, int count)
        {
            if (count <= 0) return;

            lock (_lock)
            {
                // Ajoute ou met à jour le nombre de fichiers prioritaires pour ce Job
                _jobPriorityCounts.AddOrUpdate(jobId, count, (id, old) => old + count);
                _priorityWaitHandle.Reset(); // Ferme la barrière (Rouge)
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

                    // Si plus aucun job n'a de fichiers prioritaires, on ouvre la barrière
                    if (_jobPriorityCounts.IsEmpty || _jobPriorityCounts.Values.All(v => v <= 0))
                    {
                        _priorityWaitHandle.Set(); // Ouvre la barrière (Vert)
                    }
                }
            }
        }

        // 3. Bloque les fichiers normaux si la barrière est fermée
        // Gère aussi l'annulation si on clique sur "Stop" (token)
        public void WaitForPriorityIfNeeded(CancellationToken token)
        {
            try
            {
                _priorityWaitHandle.Wait(token);
            }
            catch (OperationCanceledException)
            {
                // On remonte l'info que le job a été annulé
                throw;
            }
        }

        // 4. Pour tes Tests Unitaires (nettoie le Singleton)
        public void ResetForTests()
        {
            lock (_lock)
            {
                _jobPriorityCounts.Clear();
                _priorityWaitHandle.Set();
            }
        }
    }
}