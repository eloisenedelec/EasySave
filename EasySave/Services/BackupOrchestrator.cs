using EasySave.Models;
using EasySave.Observers;

namespace EasySave.Services
{
    public class BackupOrchestrator
    {
        private readonly List<BackupTask> _tasks = new();

        private readonly GlobalPriorityTracker _tracker;
        private readonly LargeFileCoordinator _coordinator;

        public IReadOnlyList<BackupTask> Tasks => _tasks;

        public BackupOrchestrator(GlobalPriorityTracker tracker, LargeFileCoordinator coordinator)
        {
            _tracker = tracker;
            _coordinator = coordinator;
        }

        public BackupTask CreateTask(BackupJob job, IBackupObserver observer)
        {
            var task = new BackupTask(job, observer, _tracker, _coordinator);
            _tasks.Add(task);
            return task;
        }

        public void Clear()
        {
            foreach (var task in _tasks)
            {
                task.Dispose();
            }
            _tasks.Clear();
        }
    
        public async Task RunAllAsync()
        {
            _tracker.Reset(); // Nettoie l'état laissé par un run précédent stoppé/annulé

            var runningTasks = _tasks
                .Where(task => task.State == BackupTaskState.Idle)
                .Select(task => task.RunAsync())
                .ToList();

            await Task.WhenAll(runningTasks);
        }

        public async Task RunSelectedAsync(IEnumerable<BackupTask> selectedTasks)
        {
            var runningTasks = selectedTasks
                .Where(task => task.State == BackupTaskState.Idle)
                .Select(task => task.RunAsync())
                .ToList();

            await Task.WhenAll(runningTasks);
        }

        public void PauseAll()
        {
            foreach (var task in _tasks)
            {
                task.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var task in _tasks)
            {
                task.Resume();
            }
        }

        public void StopAll()
        {
            foreach (var task in _tasks)
            {
                task.Stop();
            }
        }
    }
}