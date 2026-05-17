using System.Windows;
using EasySave.Services;
using EasySave.UI;
using EasyLog;
using EasyLog.Contracts;

namespace EasySave;

public partial class App : Application
{
    public static BackupExecutor Executor { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Executor = new BackupExecutor();

        var settings = SettingsManager.GetInstance();
        LanguageManager.GetInstance().LoadLanguage(settings.GetLanguage());

        var logger = Logger.GetInstance();
        logger.Initialize(settings);
        logger.SetFormat(settings.GetLogFormat());

        Executor.AddObserver(logger);
        Executor.AddObserver(StateManager.GetInstance());
    }
}
