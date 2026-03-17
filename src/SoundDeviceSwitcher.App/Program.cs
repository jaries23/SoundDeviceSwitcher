using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.UI;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, eventArgs) => AppLogger.LogException("UI thread", eventArgs.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            if (eventArgs.ExceptionObject is Exception exception)
            {
                AppLogger.LogException("AppDomain", exception);
            }
        };

        ApplicationConfiguration.Initialize();

        var localizer = new LocalizationService();
        var services = new AppServices(
            localizer,
            new AppConfigStore(localizer),
            new AudioDeviceService(localizer),
            new ShortcutManager(localizer),
            new GitHubUpdateChecker());

        if (args.Any(arg => arg.Equals("--toggle", StringComparison.OrdinalIgnoreCase)))
        {
            RunToggleMode(services);
            return;
        }

        var launchedFromPostInstall = args.Any(arg => arg.Equals("--postinstall", StringComparison.OrdinalIgnoreCase));
        using var mainMutex = MainInstanceManager.CreateMutex(out var createdNew);
        if (!createdNew)
        {
            MainInstanceManager.SignalExistingInstance(launchedFromPostInstall ? 12 : 1, launchedFromPostInstall ? 500 : 0);
            return;
        }

        var launchedFromStartup = args.Any(arg => arg.Equals("--startup", StringComparison.OrdinalIgnoreCase));
        Application.Run(new MainShellForm(services, launchedFromStartup, launchedFromPostInstall));
    }

    private static void RunToggleMode(AppServices services)
    {
        if (!services.ConfigStore.TryLoad(out var config, out var errorMessage))
        {
            MessageBox.Show(
                errorMessage ?? services.Localizer.Get("MessageConfigureBeforeToggle"),
                services.Localizer.Get("AppName"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var result = services.AudioDeviceService.Toggle(config!);
        TrayNotificationService.ShowTransient(
            services.Localizer.Get("AppName"),
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            imagePath: NotificationIconCatalog.ResolveToggleImagePath(config!, result),
            themeMode: config!.Theme);

        if (!result.Success)
        {
            MessageBox.Show(
                result.Message,
                services.Localizer.Get("AppName"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
