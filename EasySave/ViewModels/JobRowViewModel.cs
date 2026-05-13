using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Observers;
using EasySave.Services;

namespace EasySave.ViewModels;

public class JobRowViewModel : INotifyPropertyChanged, IBackupObserver
{
    private bool _isSelected;
    private bool _isRunning;
    private double _progress;
    private string _progressText = string.Empty;
    private BackupTaskState _state = BackupTaskState.Idle;
    private BackupTask? _task;
    private int _totalFiles;
    private int _filesProcessed;

    public BackupJob Job { get; }
    public Action? OnSelectionChanged { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); OnSelectionChanged?.Invoke(); }
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set { _isRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowIdleButtons)); CommandManager.InvalidateRequerySuggested(); }
    }

    public double Progress
    {
        get => _progress;
        private set { _progress = value; OnPropertyChanged(); }
    }

    public string ProgressText
    {
        get => _progressText;
        private set { _progressText = value; OnPropertyChanged(); }
    }

    public BackupTaskState State
    {
        get => _state;
        private set
        {
            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowIdleButtons));
            OnPropertyChanged(nameof(ShowPauseButton));
            OnPropertyChanged(nameof(ShowResumeButton));
            OnPropertyChanged(nameof(ShowStopButton));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool ShowIdleButtons  => _state is BackupTaskState.Idle or BackupTaskState.Completed or BackupTaskState.Error or BackupTaskState.Stopped;
    public bool ShowPauseButton  => _state == BackupTaskState.Running;
    public bool ShowResumeButton => _state == BackupTaskState.Paused;
    public bool ShowStopButton   => _state is BackupTaskState.Running or BackupTaskState.Paused;

    public ICommand DeleteCommand     { get; }
    public ICommand ExecuteOneCommand { get; }
    public ICommand PauseCommand      { get; }
    public ICommand ResumeCommand     { get; }
    public ICommand StopCommand       { get; }

    public JobRowViewModel(BackupJob job, Action<JobRowViewModel> onDelete, Action<JobRowViewModel> onExecuteOne)
    {
        Job = job;
        DeleteCommand     = new RelayCommand(() => onDelete(this),     () => ShowIdleButtons);
        ExecuteOneCommand = new RelayCommand(() => onExecuteOne(this), () => ShowIdleButtons);
        PauseCommand      = new RelayCommand(() => _task?.Pause(),     () => ShowPauseButton);
        ResumeCommand     = new RelayCommand(() => _task?.Resume(),    () => ShowResumeButton);
        StopCommand       = new RelayCommand(() => _task?.Stop(),      () => ShowStopButton);
    }

    public void SetTask(BackupTask task)
    {
        _task = task;
        _task.StateChanged += OnTaskStateChanged;
        Reset();
    }

    private void OnTaskStateChanged(BackupTaskState newState)
    {
        Application.Current.Dispatcher.Invoke(() => State = newState);
    }

    public void Reset()
    {
        _totalFiles = 0;
        _filesProcessed = 0;
        Progress = 0;
        ProgressText = string.Empty;
        State = BackupTaskState.Idle;
        IsRunning = false;
    }

    public void OnBackupStarted(string jobName, int totalFiles, long totalSize)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _totalFiles = totalFiles;
            _filesProcessed = 0;
            Progress = 0;
            ProgressText = $"0 / {totalFiles}";
            State = BackupTaskState.Running;
            IsRunning = true;
        });
    }

    public void OnFileProcessed(string sourceFile, string targetFile, long fileSize, long transferTime, long encryptionTimeMs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _filesProcessed++;
            Progress = _totalFiles > 0 ? (double)_filesProcessed / _totalFiles * 100 : 0;
            ProgressText = $"{_filesProcessed} / {_totalFiles}";
        });
    }

    public void OnBackupCompleted(string jobName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Progress = 100;
            ProgressText = "Terminé";
            State = BackupTaskState.Completed;
            IsRunning = false;
        });
    }

    public void OnBackupError(string jobName, string error)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ProgressText = "Erreur";
            State = BackupTaskState.Error;
            IsRunning = false;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
