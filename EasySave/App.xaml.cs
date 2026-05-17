using System.Windows;
using EasySave.Services;
using EasyLog;

namespace EasySave;

public partial class App : Application
{
    public static BackupExecutor Executor { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Executor = new BackupExecutor();

        var logger = Logger.GetInstance();
        logger.Initialize(SettingsManager.GetInstance());
        logger.SetFormat(SettingsManager.GetInstance().GetLogFormat());
        Executor.AddObserver(logger);
        Executor.AddObserver(StateManager.GetInstance());
    }
}
