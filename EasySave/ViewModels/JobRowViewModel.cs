using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Models;

namespace EasySave.ViewModels;

public class JobRowViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isRunning;

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
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public ICommand DeleteCommand { get; }
    public ICommand ExecuteOneCommand { get; }

    public JobRowViewModel(BackupJob job, Action<JobRowViewModel> onDelete, Action<JobRowViewModel> onExecuteOne)
    {
        Job = job;
        DeleteCommand    = new RelayCommand(() => onDelete(this));
        ExecuteOneCommand = new RelayCommand(() => onExecuteOne(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
