using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Services;

namespace EasySave.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IBackupObserver
{
    private readonly BackupExecutor _executor;
    private readonly BackupOrchestrator _orchestrator;
    private readonly ProcessMonitorService _processMonitor;

    private bool _isAddingJob;
    private bool _isSettingsOpen;
    private AddJobViewModel? _addJobVM;
    private SettingsViewModel? _settingsVM;
    private bool _isAnyRunning;
    private string _statusMessage = string.Empty;

    public ObservableCollection<JobRowViewModel> Jobs { get; } = new();

    public bool IsAddingJob
    {
        get => _isAddingJob;
        private set { _isAddingJob = value; OnPropertyChanged(); }
    }

    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        private set { _isSettingsOpen = value; OnPropertyChanged(); }
    }

    public AddJobViewModel? AddJobVM
    {
        get => _addJobVM;
        private set { _addJobVM = value; OnPropertyChanged(); }
    }

    public SettingsViewModel? SettingsVM
    {
        get => _settingsVM;
        private set { _settingsVM = value; OnPropertyChanged(); }
    }

    public bool IsAnyRunning
    {
        get => _isAnyRunning;
        private set { _isAnyRunning = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatus)); }
    }

    public bool HasStatus        => !string.IsNullOrEmpty(_statusMessage);
    public bool HasSelectedJobs  => Jobs.Any(j => j.IsSelected);

    public ICommand OpenAddJobCommand      { get; }
    public ICommand ExecuteSelectedCommand { get; }
    public ICommand OpenSettingsCommand    { get; }
    public ICommand PauseAllCommand        { get; }
    public ICommand ResumeAllCommand       { get; }
    public ICommand StopAllCommand         { get; }

    public MainViewModel(BackupExecutor executor)
    {
        _executor    = executor;
        _orchestrator = executor.Orchestrator;
        _executor.AddObserver(this);

        _processMonitor = new ProcessMonitorService(_orchestrator);
        _processMonitor.Start();

        OpenAddJobCommand      = new RelayCommand(OpenAddJob);
        ExecuteSelectedCommand = new RelayCommand(ExecuteSelected, () => HasSelectedJobs && !IsAnyRunning);
        OpenSettingsCommand    = new RelayCommand(OpenSettings);
        PauseAllCommand        = new RelayCommand(() => _orchestrator.PauseAll(),  () => IsAnyRunning);
        ResumeAllCommand       = new RelayCommand(() => _orchestrator.ResumeAll(), () => IsAnyRunning);
        StopAllCommand         = new RelayCommand(() => _orchestrator.StopAll(),   () => IsAnyRunning);

        LoadJobs();
    }

    private void LoadJobs()
    {
        Jobs.Clear();
        foreach (var job in BackupManager.GetInstance().GetAllBackupJobs())
            AddJobRow(job);
    }

    private void AddJobRow(BackupJob job)
    {
        var row = new JobRowViewModel(job, DeleteJob, ExecuteOne);
        row.OnSelectionChanged = () =>
        {
            OnPropertyChanged(nameof(HasSelectedJobs));
            CommandManager.InvalidateRequerySuggested();
        };
        Jobs.Add(row);
    }

    private void DeleteJob(JobRowViewModel row)
    {
        BackupManager.GetInstance().RemoveBackupJob(row.Job.Id);
        Jobs.Remove(row);
        OnPropertyChanged(nameof(HasSelectedJobs));
    }

    private async void ExecuteOne(JobRowViewModel row)
    {
        await RunJobsAsync(new[] { row });
    }

    private async void ExecuteSelected()
    {
        var selected = Jobs.Where(j => j.IsSelected).ToList();
        await RunJobsAsync(selected);
    }

    private async Task RunJobsAsync(IEnumerable<JobRowViewModel> rows)
    {
        var rowList = rows.ToList();
        if (rowList.Count == 0) return;

        IsAnyRunning = true;
        StatusMessage = "Sauvegarde en cours...";
        _orchestrator.Clear();

        foreach (var row in rowList)
        {
            var composite = new CompositeObserver(_executor, row);
            var task = _orchestrator.CreateTask(row.Job, composite);
            row.SetTask(task);
        }

        try
        {
            await _orchestrator.RunAllAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsAnyRunning = false;
            CommandManager.InvalidateRequerySuggested();

            // Résumé basé sur l'état réel des jobs
            bool anyError = rowList.Any(r => r.State is BackupTaskState.Error or BackupTaskState.Stopped);
            if (anyError)
                StatusMessage = "Sauvegarde terminée avec des erreurs — voir les jobs en rouge";
            else
                StatusMessage = "Sauvegarde terminée";
        }
    }

    private void OpenAddJob()
    {
        AddJobVM = new AddJobViewModel(OnJobSaved, () => IsAddingJob = false);
        IsAddingJob = true;
    }

    private void OnJobSaved(BackupJob job)
    {
        AddJobRow(job);
        IsAddingJob = false;
    }

    private void OpenSettings()
    {
        SettingsVM = new SettingsViewModel(() => IsSettingsOpen = false);
        IsSettingsOpen = true;
    }

    // Observer global — reçoit les events de tous les jobs via BackupExecutor
    public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
    {
        Application.Current.Dispatcher.Invoke(() => StatusMessage = $"{jobName} — {totalFiles} fichiers");
    }

    public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs) { }

    public void OnBackupCompleted(string jobName)
    {
        Application.Current.Dispatcher.Invoke(() => StatusMessage = $"{jobName} — terminé");
    }

    public void OnBackupError(string jobName, string error)
    {
        Application.Current.Dispatcher.Invoke(() => StatusMessage = $"Erreur [{jobName}] : {error}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
