using EasySave.Models;
using EasySave.Observers;
using EasySave.Strategies;
using EasySave.Factories;

namespace EasySave.Services
{
    public class BackupTask
    {
        public BackupJob Job { get; }
        public BackupTaskState State { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public ManualResetEventSlim PauseEvent { get; }

        private readonly IBackupObserver _observer;
        private string? _errorMessage;
        private readonly GlobalPriorityTracker _tracker;
        private readonly LargeFileCoordinator _coordinator;

        public BackupTask(BackupJob job, IBackupObserver observer, GlobalPriorityTracker tracker, LargeFileCoordinator coordinator)
        {
            Job = job;
            _observer = observer;
            _tracker = tracker;
            _coordinator = coordinator;
            State = BackupTaskState.Idle;
            CancellationTokenSource = new CancellationTokenSource();
            PauseEvent = new ManualResetEventSlim(true); // Initialisé à signalé (pas en pause)
        }

        public async Task RunAsync()
        {
            try
            {
                State = BackupTaskState.Running;

                ProcessMonitoring pm = new ProcessMonitoring(SettingsManager.GetInstance());
                if (!pm.AreNoBusinessProcessesRunning())
                {
                    _observer.OnBackupError(Job.Name, "Annulation : Logiciel métier en cours d'exécution.");
                    State = BackupTaskState.Error;
                    _errorMessage = "Logiciel métier actif";
                    return;
                }

                IBackupStrategy strategy = BackupStrategyFactory.CreateStrategy(Job.Type);
                
                // Passer le contexte d'exécution à la stratégie
                await ExecuteStrategyAsync(strategy);

                if (CancellationTokenSource.Token.IsCancellationRequested)
                {
                    State = BackupTaskState.Stopped;
                    _observer.OnBackupError(Job.Name, "Sauvegarde annulée par l'utilisateur.");
                }
                else
                {
                    State = BackupTaskState.Completed;
                }
            }
            catch (OperationCanceledException)
            {
                State = BackupTaskState.Stopped;
                _observer.OnBackupError(Job.Name, "Sauvegarde arrêtée.");
            }
            catch (Exception ex)
            {
                State = BackupTaskState.Error;
                _errorMessage = ex.Message;
                _observer.OnBackupError(Job.Name, ex.Message);
            }
            finally
            {
                PauseEvent.Set(); // S'assurer que la pause est levée
            }
        }

        private async Task ExecuteStrategyAsync(IBackupStrategy strategy)
        {
            var executionContext = new BackupExecutionContext(
                CancellationTokenSource.Token,
                PauseEvent,
                _observer,
                _tracker,
                _coordinator
            );
            strategy.Execute(Job, executionContext);
            await Task.CompletedTask;
        }

        public void Pause()
        {
            if (State == BackupTaskState.Running)
            {
                State = BackupTaskState.Paused;
                PauseEvent.Reset(); // Bloquer à la prochaine vérification
            }
        }

        public void Resume()
        {
            if (State == BackupTaskState.Paused)
            {
                State = BackupTaskState.Running;
                PauseEvent.Set(); // Relancer l'exécution
            }
        }

        public void Stop()
        {
            if (State is BackupTaskState.Running or BackupTaskState.Paused)
            {
                State = BackupTaskState.Stopped;
                CancellationTokenSource.Cancel();
                PauseEvent.Set(); // S'assurer que le thread n'est pas bloqué en pause
            }
        }

        public string? GetErrorMessage() => _errorMessage;

        public void Dispose()
        {
            CancellationTokenSource?.Dispose();
            PauseEvent?.Dispose();
        }
    }

    /// Contexte d'exécution fourni à la stratégie pour accéder aux signaux d'annulation et de pause.
    public class BackupExecutionContext
    {
        public CancellationToken CancellationToken { get; }
        public ManualResetEventSlim PauseEvent { get; }
        public IBackupObserver Observer { get; }
        public GlobalPriorityTracker PriorityTracker { get; }
        public LargeFileCoordinator FileCoordinator { get; }

        public BackupExecutionContext(CancellationToken cancellationToken, ManualResetEventSlim pauseEvent, IBackupObserver observer, GlobalPriorityTracker priorityTracker, LargeFileCoordinator fileCoordinator)
        {
            CancellationToken = cancellationToken;
            PauseEvent = pauseEvent;
            Observer = observer;
            PriorityTracker = priorityTracker;
            FileCoordinator = fileCoordinator;
        }

        /// Attend que l'exécution soit reprise si elle est en pause, ou jette une exception si annulation demandée.
        public void CheckPauseAndCancellation()
        {
            CancellationToken.ThrowIfCancellationRequested();
            PauseEvent.Wait(CancellationToken);
        }
    }
}