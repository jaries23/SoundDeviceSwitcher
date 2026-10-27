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
    private void ToggleProfileOverlay()
    {
        if (_profileOverlayForm is not null && !_profileOverlayForm.IsDisposed && _profileOverlayForm.Visible)
        {
            CloseProfileOverlay();
            return;
        }

        OpenProfileOverlay();
    }

    private void OpenProfileOverlay()
    {
        if (!TryGetOverlayRuntimeConfig(out var config))
        {
            return;
        }

        var profiles = BuildOverlayProfiles(config!);
        if (profiles.Count == 0)
        {
            var message = _services.Localizer.Get("ErrorNoOverlayProfiles");
            SetStatus(message, isError: true);
            _trayNotifications.Show(message, ToolTipIcon.Warning, 2500);
            return;
        }

        CloseProfileOverlay();

        _profileOverlayForm = new ProfileOverlayForm(
            profiles,
            _services.Localizer,
            config!.Theme,
            config.OverlayHeightPercent,
            config.OverlayAnchor,
            config.OverlayLayoutOrientation);
        _profileOverlayForm.ProfileSelected += HandleOverlayProfileSelected;
        _profileOverlayForm.FormClosed += HandleProfileOverlayClosed;
        _profileOverlayForm.Show();
        _profileOverlayForm.Activate();
    }

    private bool TryGetOverlayRuntimeConfig(out AppConfig? config)
    {
        var language = _languageComboBox.SelectedValue is AppLanguage selectedLanguage
            ? selectedLanguage
            : _services.Localizer.CurrentLanguage;

        config = new AppConfig
        {
            Language = language,
            Theme = GetSelectedThemeMode(),
            OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value),
            OverlayAnchor = GetSelectedOverlayAnchor(),
            OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation(),
            Profiles = BuildProfilesForConfig()
        };

        _services.Localizer.SetLanguage(language);
        return true;
    }

    private bool TryLoadOverlayConfig(out AppConfig? config)
    {
        config = _cachedConfig;
        if (config is not null)
        {
            _services.Localizer.SetLanguage(config.Language);
            return true;
        }

        if (!_services.ConfigStore.TryLoad(out config, out var errorMessage))
        {
            var message = errorMessage ?? _services.Localizer.Get("ErrorNoConfig");
            SetStatus(message, isError: true);
            _trayNotifications.Show(message, ToolTipIcon.Warning, 2500);
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config!.Language);
        return true;
    }

    private List<ProcessAudioProfile> BuildOverlayProfiles(AppConfig config)
    {
        return config.Profiles
            .OrderByDescending(profile => profile.Priority)
            .Where(profile =>
                !string.IsNullOrWhiteSpace(profile.PlaybackDevice.Id) ||
                !string.IsNullOrWhiteSpace(profile.RecordingDevice.Id))
            .Select(profile =>
            {
                var clone = profile.Clone();
                clone.Enabled = true;
                return clone;
            })
            .ToList();
    }

    private void HandleOverlayProfileSelected(object? sender, ProfileOverlaySelectionEventArgs eventArgs)
    {
        if (!TryApplyProcessProfile(eventArgs.Profile, out var message, out var isError, out var devicesChanged))
        {
            if (isError && !string.IsNullOrWhiteSpace(message))
            {
                SetStatus(message, isError: true);
            }

            return;
        }

        _activeProcessProfileId = eventArgs.Profile.Id;
        _activeProcessProfilePlaybackDeviceId = eventArgs.Profile.PlaybackDevice.Id;
        _activeProcessProfileRecordingDeviceId = eventArgs.Profile.RecordingDevice.Id;

        if (devicesChanged && !string.IsNullOrWhiteSpace(message))
        {
            SetStatus(message);
            _trayNotifications.Show(
                message,
                ToolTipIcon.Info,
                2000,
                imagePath: NotificationIconCatalog.ResolvePath(_cachedConfig?.NotificationIconFileName ?? AppConfig.DefaultIconFileName));
        }
    }

    private void HandleProfileOverlayClosed(object? sender, FormClosedEventArgs eventArgs)
    {
        if (_profileOverlayForm is null)
        {
            return;
        }

        _profileOverlayForm.ProfileSelected -= HandleOverlayProfileSelected;
        _profileOverlayForm.FormClosed -= HandleProfileOverlayClosed;
        _profileOverlayForm.Dispose();
        _profileOverlayForm = null;
    }

    private void CloseProfileOverlay()
    {
        if (_profileOverlayForm is null)
        {
            return;
        }

        var overlayForm = _profileOverlayForm;
        _profileOverlayForm = null;
        overlayForm.ProfileSelected -= HandleOverlayProfileSelected;
        overlayForm.FormClosed -= HandleProfileOverlayClosed;

        if (!overlayForm.IsDisposed)
        {
            overlayForm.Close();
            overlayForm.Dispose();
        }
    }

}
