using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Hotkeys;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Profiles;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App.UI;

internal sealed partial class MainShellForm
{
    private bool UpdateTrayHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null)
        {
            config = _cachedConfig;
        }

        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _trayHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _trayHotkey.Unregister();
            return false;
        }

        _cachedConfig = config;

        _services.Localizer.SetLanguage(config.Language);
        ApplyTrayText();
        _trayNotifications.SetThemeMode(config.Theme);

        if (!config.Hotkey.Enabled)
        {
            _trayHotkey.Unregister();
            return true;
        }

        if (_trayHotkey.Register(config.Hotkey, out var errorMessage))
        {
            return true;
        }

        SetStatus(errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"), isError: true);

        if (showErrorNotification)
        {
            _trayNotifications.Show(
                errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"),
                ToolTipIcon.Warning,
                3000);
        }

        return false;
    }

    private bool UpdateRecentSwitchUndoHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null)
        {
            config = _cachedConfig;
        }

        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _recentSwitchUndoHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _recentSwitchUndoHotkey.Unregister();
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config.Language);

        if (!config.RecentSwitchUndoHotkey.Enabled || !IsRecentSwitchUndoWindowActive())
        {
            _recentSwitchUndoHotkey.Unregister();
            return true;
        }

        if (_recentSwitchUndoHotkey.Register(config.RecentSwitchUndoHotkey, out var errorMessage))
        {
            return true;
        }

        SetStatus(errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"), isError: true);

        if (showErrorNotification)
        {
            _trayNotifications.Show(
                errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"),
                ToolTipIcon.Warning,
                3000);
        }

        return false;
    }

    private bool UpdateOverlayHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null)
        {
            config = _cachedConfig;
        }

        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _overlayHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _overlayHotkey.Unregister();
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config.Language);

        if (!config.OverlayHotkey.Enabled)
        {
            _overlayHotkey.Unregister();
            return true;
        }

        if (_overlayHotkey.Register(config.OverlayHotkey, out var errorMessage))
        {
            return true;
        }

        SetStatus(errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"), isError: true);

        if (showErrorNotification)
        {
            _trayNotifications.Show(
                errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"),
                ToolTipIcon.Warning,
                3000);
        }

        return false;
    }

}
