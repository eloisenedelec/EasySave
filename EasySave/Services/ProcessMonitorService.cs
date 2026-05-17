namespace EasySave.Services
{
    public class ProcessMonitorService
    {
        private readonly BackupOrchestrator _orchestrator;
        private readonly ProcessMonitoring _monitor;
        private CancellationTokenSource? _cts;
        private bool _autopaused = false;

        public ProcessMonitorService(BackupOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
            _monitor = new ProcessMonitoring(SettingsManager.GetInstance());
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    bool businessRunning = !_monitor.AreNoBusinessProcessesRunning();

                    if (businessRunning && !_autopaused)
                    {
                        _orchestrator.PauseAll();
                        _autopaused = true;
                    }
                    else if (!businessRunning && _autopaused)
                    {
                        _orchestrator.ResumeAll();
                        _autopaused = false;
                    }

                    try { await Task.Delay(1500, token); }
                    catch (TaskCanceledException) { break; }
                }
            }, token);
        }

        public void Stop() => _cts?.Cancel();
    }
}
