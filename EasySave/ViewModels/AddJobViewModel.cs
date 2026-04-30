using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services;
using Microsoft.Win32;

namespace EasySave.ViewModels;

public class AddJobViewModel : INotifyPropertyChanged
{
    private readonly Action<BackupJob> _onSaved;
    private readonly Action _onCancelled;

    private string _name = string.Empty;
    private string _sourcePath = string.Empty;
    private string _targetPath = string.Empty;
    private BackupType _selectedType = BackupType.Full;
    private string? _errorMessage;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string SourcePath
    {
        get => _sourcePath;
        set { _sourcePath = value; OnPropertyChanged(); }
    }

    public string TargetPath
    {
        get => _targetPath;
        set { _targetPath = value; OnPropertyChanged(); }
    }

    public BackupType SelectedType
    {
        get => _selectedType;
        set { _selectedType = value; OnPropertyChanged(); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);
    public IEnumerable<BackupType> BackupTypes => Enum.GetValues<BackupType>();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseSourceCommand { get; }
    public ICommand BrowseTargetCommand { get; }

    public AddJobViewModel(Action<BackupJob> onSaved, Action onCancelled)
    {
        _onSaved     = onSaved;
        _onCancelled = onCancelled;

        SaveCommand         = new RelayCommand(Save);
        CancelCommand       = new RelayCommand(_onCancelled);
        BrowseSourceCommand = new RelayCommand(() => BrowseFolder(p => SourcePath = p));
        BrowseTargetCommand = new RelayCommand(() => BrowseFolder(p => TargetPath = p));
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))       { ErrorMessage = "Le nom est requis."; return; }
        if (string.IsNullOrWhiteSpace(SourcePath)) { ErrorMessage = "Le dossier source est requis."; return; }
        if (string.IsNullOrWhiteSpace(TargetPath)) { ErrorMessage = "Le dossier destination est requis."; return; }

        var manager = BackupManager.GetInstance();
        var job = new BackupJob(manager.GetJobCount() + 1, Name, SourcePath, TargetPath, SelectedType);

        if (!manager.AddBackupJob(job)) { ErrorMessage = "Impossible d'ajouter le job."; return; }

        _onSaved(job);
    }

    private static void BrowseFolder(Action<string> onSelected)
    {
        var dialog = new OpenFolderDialog { Title = "Sélectionner un dossier" };
        if (dialog.ShowDialog() == true)
            onSelected(dialog.FolderName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
