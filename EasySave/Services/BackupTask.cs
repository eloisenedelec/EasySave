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

        public BackupTask(BackupJob job, IBackupObserver observer)
        {
            Job = job;
            _observer = observer;
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
                _observer
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

    
    public class BackupExecutionContext
    {
        public CancellationToken CancellationToken { get; }
        public ManualResetEventSlim PauseEvent { get; }
        public IBackupObserver Observer { get; }

        public BackupExecutionContext(CancellationToken cancellationToken, ManualResetEventSlim pauseEvent, IBackupObserver observer)
        {
            CancellationToken = cancellationToken;
            PauseEvent = pauseEvent;
            Observer = observer;
        }

        
        public void CheckPauseAndCancellation()
        {
            CancellationToken.ThrowIfCancellationRequested();
            PauseEvent.Wait(CancellationToken);
        }
    }
}