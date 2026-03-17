using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App;

internal sealed class AppServices
{
    public AppServices(
        LocalizationService localizer,
        AppConfigStore configStore,
        AudioDeviceService audioDeviceService,
        ShortcutManager shortcutManager,
        GitHubUpdateChecker updateChecker)
    {
        Localizer = localizer;
        ConfigStore = configStore;
        AudioDeviceService = audioDeviceService;
        ShortcutManager = shortcutManager;
        UpdateChecker = updateChecker;
    }

    public LocalizationService Localizer { get; }

    public AppConfigStore ConfigStore { get; }

    public AudioDeviceService AudioDeviceService { get; }

    public ShortcutManager ShortcutManager { get; }

    public GitHubUpdateChecker UpdateChecker { get; }
}
