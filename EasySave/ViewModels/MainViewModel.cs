using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

    private bool _isAddingJob;
    private bool _isSettingsOpen;
    private AddJobViewModel? _addJobVM;
    private SettingsViewModel? _settingsVM;

    private bool _isExecuting;
    private double _progress;
    private string _statusMessage = string.Empty;
    private string _currentFile = string.Empty;
    private string _progressText = string.Empty;
    private int _totalFiles;
    private int _filesProcessed;

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

    public bool IsExecuting
    {
        get => _isExecuting;
        private set { _isExecuting = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotExecuting)); }
    }
    public bool IsNotExecuting => !_isExecuting;

    public double Progress
    {
        get => _progress;
        private set { _progress = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatus)); }
    }

    public string CurrentFile
    {
        get => _currentFile;
        private set { _currentFile = value; OnPropertyChanged(); }
    }

    public string ProgressText
    {
        get => _progressText;
        private set { _progressText = value; OnPropertyChanged(); }
    }

    public bool HasStatus => !string.IsNullOrEmpty(_statusMessage);
    public bool HasSelectedJobs => Jobs.Any(j => j.IsSelected);

    public ICommand OpenAddJobCommand { get; }
    public ICommand ExecuteSelectedCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    public MainViewModel(BackupExecutor executor)
    {
        _executor = executor;
        _executor.AddObserver(this);

        OpenAddJobCommand      = new RelayCommand(OpenAddJob);
        ExecuteSelectedCommand = new RelayCommand(ExecuteSelected, () => HasSelectedJobs && !IsExecuting);
        OpenSettingsCommand    = new RelayCommand(OpenSettings);

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
        if (IsExecuting) return;
        IsExecuting = true;
        row.IsRunning = true;
        await Task.Run(() => _executor.ExecuteBackup(row.Job));
        row.IsRunning = false;
        IsExecuting = false;
    }

    private async void ExecuteSelected()
    {
        IsExecuting = true;
        var ids = Jobs.Where(j => j.IsSelected).Select(j => j.Job.Id).ToList();
        await Task.Run(() => _executor.ExecuteMultipleBackups(ids));
        IsExecuting = false;
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

    public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _totalFiles = totalFiles;
            _filesProcessed = 0;
            Progress = 0;
            CurrentFile = string.Empty;
            StatusMessage = jobName;
            ProgressText = $"0 / {totalFiles}";
        });
    }

    public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _filesProcessed++;
            CurrentFile = Path.GetFileName(sourceFile);
            Progress = _totalFiles > 0 ? (double)_filesProcessed / _totalFiles * 100 : 0;
            ProgressText = $"{_filesProcessed} / {_totalFiles}";
        });
    }

    public void OnBackupCompleted(string jobName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Progress = 100;
            CurrentFile = string.Empty;
            ProgressText = string.Empty;
            StatusMessage = $"{jobName} — terminé";
        });
    }

    public void OnBackupError(string jobName, string error)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentFile = string.Empty;
            StatusMessage = $"Erreur : {error}";
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
