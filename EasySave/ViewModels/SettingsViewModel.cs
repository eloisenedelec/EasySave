using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services;
using EasySave.UI;
using EasyLog;

namespace EasySave.ViewModels;

public class LanguageOption
{
    public string Code { get; }
    public string Label { get; }
    public LanguageOption(string code, string label) { Code = code; Label = label; }
}

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly Action _onClose;
    private readonly SettingsManager _settingsManager;

    private LanguageOption _selectedLanguage;
    private string _selectedLogFormat;
    private string _newProcessName = string.Empty;
    private string _newExtension = string.Empty;
    private string _newPriorityExtension = string.Empty;
    private string _largeFileSizeKb = string.Empty;

    public List<LanguageOption> Languages { get; } = new()
    {
        new("fr", "Français"),
        new("en", "English"),
    };

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set { _selectedLanguage = value; OnPropertyChanged(); }
    }

    public List<string> LogFormats { get; } = new() { "JSON", "XML" };

    public string SelectedLogFormat
    {
        get => _selectedLogFormat;
        set { _selectedLogFormat = value; OnPropertyChanged(); }
    }

    public ObservableCollection<SettingsProcess> Processes { get; } = new();

    public string NewProcessName
    {
        get => _newProcessName;
        set { _newProcessName = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> Extensions { get; } = new();

    public string NewExtension
    {
        get => _newExtension;
        set { _newExtension = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> PriorityExtensions { get; } = new();

    public string NewPriorityExtension
    {
        get => _newPriorityExtension;
        set { _newPriorityExtension = value; OnPropertyChanged(); }
    }

    public string LargeFileSizeKb
    {
        get => _largeFileSizeKb;
        set { _largeFileSizeKb = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand AddProcessCommand { get; }
    public ICommand RemoveProcessCommand { get; }
    public ICommand AddExtensionCommand { get; }
    public ICommand RemoveExtensionCommand { get; }
    public ICommand AddPriorityExtensionCommand { get; }
    public ICommand RemovePriorityExtensionCommand { get; }
    public ICommand OpenLogsCommand { get; }

    public SettingsViewModel(Action onClose)
    {
        _onClose = onClose;
        _settingsManager = SettingsManager.GetInstance();

        _selectedLanguage = Languages[0];
        _selectedLogFormat = _settingsManager.GetLogFormat();

        foreach (var p in _settingsManager.GetAllProcesses())
            Processes.Add(p);

        foreach (var e in _settingsManager.GetEncryptedExtensions())
            Extensions.Add(e);

        foreach (var e in _settingsManager.GetPriorityExtensions())
            PriorityExtensions.Add(e);

        LargeFileSizeKb = (_settingsManager.GetLargeFileSizeLimit() / 1024).ToString();

        SaveCommand                    = new RelayCommand(Save);
        CloseCommand                   = new RelayCommand(_onClose);
        AddProcessCommand              = new RelayCommand(AddProcess);
        RemoveProcessCommand           = new RelayCommand<SettingsProcess>(RemoveProcess);
        AddExtensionCommand            = new RelayCommand(AddExtension);
        RemoveExtensionCommand         = new RelayCommand<string>(RemoveExtension);
        AddPriorityExtensionCommand    = new RelayCommand(AddPriorityExtension);
        RemovePriorityExtensionCommand = new RelayCommand<string>(RemovePriorityExtension);
        OpenLogsCommand                = new RelayCommand(OpenLogs);
    }

    private void OpenLogs()
    {
        string logsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "Logs");

        Directory.CreateDirectory(logsPath);
        System.Diagnostics.Process.Start("explorer.exe", logsPath);
    }

    private void Save()
    {
        _settingsManager.SetLogFormat(SelectedLogFormat);
        Logger.GetInstance().SetFormat(SelectedLogFormat);
        LanguageManager.GetInstance().LoadLanguage(SelectedLanguage.Code);

        if (long.TryParse(LargeFileSizeKb, out long kb) && kb > 0)
            _settingsManager.SetLargeFileSizeLimit(kb * 1024);

        _onClose();
    }

    private void AddProcess()
    {
        if (string.IsNullOrWhiteSpace(NewProcessName)) return;
        var process = new SettingsProcess { Name = NewProcessName.Trim() };
        if (_settingsManager.AddProcess(process))
        {
            Processes.Add(process);
            NewProcessName = string.Empty;
        }
    }

    private void RemoveProcess(SettingsProcess process)
    {
        if (_settingsManager.RemoveProcess(process.Id))
            Processes.Remove(process);
    }

    private void AddExtension()
    {
        if (string.IsNullOrWhiteSpace(NewExtension)) return;
        var ext = NewExtension.Trim().ToLower();
        if (!ext.StartsWith(".")) ext = "." + ext;
        if (Extensions.Contains(ext)) return;
        _settingsManager.AddExtension(ext);
        Extensions.Add(ext);
        NewExtension = string.Empty;
    }

    private void RemoveExtension(string extension)
    {
        _settingsManager.RemoveExtension(extension);
        Extensions.Remove(extension);
    }

    private void AddPriorityExtension()
    {
        if (string.IsNullOrWhiteSpace(NewPriorityExtension)) return;
        var ext = NewPriorityExtension.Trim().ToLower();
        if (!ext.StartsWith(".")) ext = "." + ext;
        if (PriorityExtensions.Contains(ext)) return;
        _settingsManager.AddPriorityExtension(ext);
        PriorityExtensions.Add(ext);
        NewPriorityExtension = string.Empty;
    }

    private void RemovePriorityExtension(string extension)
    {
        _settingsManager.RemovePriorityExtension(extension);
        PriorityExtensions.Remove(extension);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
