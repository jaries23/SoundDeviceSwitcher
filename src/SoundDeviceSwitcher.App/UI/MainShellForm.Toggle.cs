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
    private void ToggleUsingSavedSettings(bool showFailureDialog)
    {
        if (!_services.ConfigStore.TryLoad(out var config, out var errorMessage))
        {
            var message = errorMessage ?? _services.Localizer.Get("NotifyNoConfig");
            _trayNotifications.Show(message, ToolTipIcon.Warning, 3000);

            if (showFailureDialog && !_isHiddenToTray)
            {
                MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return;
        }

        _services.Localizer.SetLanguage(config!.Language);
        ApplyLanguage(refreshIconChoices: false);
        ApplyTheme(config.Theme);

        var result = ExecuteTrackedAudioChange(() => ToggleWithForegroundRuleOverride(config));
        if (!_isHiddenToTray)
        {
            SetStatus(result.Message, isError: !result.Success);
        }

        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            imagePath: NotificationIconCatalog.ResolveToggleImagePath(config, result));

        if (!result.Success && showFailureDialog && !_isHiddenToTray)
        {
            MessageBox.Show(this, result.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ToggleUsingCurrentSelections()
    {
        if (!TryBuildConfigFromCurrentSelections(out var config))
        {
            return;
        }

        _services.Localizer.SetLanguage(config!.Language);
        ApplyLanguage(refreshIconChoices: false);
        ApplyTheme(config.Theme);

        var result = ExecuteTrackedAudioChange(() => ToggleWithForegroundRuleOverride(config));
        SetStatus(result.Message, isError: !result.Success);
        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            imagePath: NotificationIconCatalog.ResolveToggleImagePath(config, result));

        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private ToggleResult ToggleWithForegroundRuleOverride(AppConfig config)
    {
        var toggleMatch = _processProfileMatcher.FindForegroundToggleRuleMatch(config.ToggleRules);
        if (toggleMatch is null)
        {
            return _services.AudioDeviceService.Toggle(config);
        }

        return toggleMatch.Rule.Target == ProfileToggleTarget.ApplicationAudioDevice
            ? _services.AudioDeviceService.ToggleApplicationPlaybackDevice(
                toggleMatch.ProcessId,
                toggleMatch.Rule.PrimaryDevice,
                toggleMatch.Rule.SecondaryDevice)
            : _services.AudioDeviceService.Toggle(
                toggleMatch.Rule.PrimaryDevice,
                toggleMatch.Rule.SecondaryDevice);
    }

    private void CreateToggleShortcut()
    {
        if (!PersistCurrentSelections(showValidationUi: true, showExceptionUi: true))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Windows Shortcut (*.lnk)|*.lnk",
            FileName = "SoundDeviceSwitcher Toggle.lnk",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _services.ShortcutManager.CreateToggleShortcut(dialog.FileName);
            SetStatus(_services.Localizer.Format("StatusCreatedShortcut", dialog.FileName));
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool PersistCurrentSelections(bool showValidationUi, bool showExceptionUi, string? successStatus = null)
    {
        if (!TryBuildConfigFromCurrentSelections(out var config, showValidationUi))
        {
            return false;
        }

        try
        {
            _services.Localizer.SetLanguage(config!.Language);
            ApplyTrayText();
            _trayNotifications.SetThemeMode(config.Theme);
            _services.ConfigStore.Save(config);
            _cachedConfig = config;
            _services.ShortcutManager.SyncStartupShortcut(config);
            var hotkeyRegistered = UpdateTrayHotkeyRegistration(config);
            var recentSwitchUndoHotkeyRegistered = UpdateRecentSwitchUndoHotkeyRegistration(config);
            var overlayHotkeyRegistered = UpdateOverlayHotkeyRegistration(config);
            EvaluateProcessProfiles(config);

            if (hotkeyRegistered &&
                recentSwitchUndoHotkeyRegistered &&
                overlayHotkeyRegistered &&
                !string.IsNullOrWhiteSpace(successStatus))
            {
                SetStatus(successStatus);
            }

            return true;
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);

            if (showExceptionUi)
            {
                MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
    }

    private bool TryBuildConfigFromCurrentSelections(out AppConfig? config, bool showValidationUi = true)
    {
        config = null;

        if (_primaryDeviceComboBox.SelectedItem is not AudioDeviceInfo primaryDevice ||
            _secondaryDeviceComboBox.SelectedItem is not AudioDeviceInfo secondaryDevice)
        {
            var message = _services.Localizer.Get("ErrorNeedTwoPlaybackDevices");
            SetStatus(message, isError: true);

            if (showValidationUi)
            {
                MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        config = new AppConfig
        {
            Language = _languageComboBox.SelectedValue is AppLanguage language ? language : _services.Localizer.CurrentLanguage,
            Theme = GetSelectedThemeMode(),
            StartWithWindows = _startWithWindowsCheckBox.Checked,
            StartMinimizedAtStartup = _startWithWindowsCheckBox.Checked && _startMinimizedAtStartupCheckBox.Checked,
            MinimizeToTrayOnClose = _minimizeToTrayOnCloseCheckBox.Checked,
            EnableUpdateNotifications = _enableUpdateNotificationsCheckBox.Checked,
            SyncCommunicationDeviceWithPlayback = _syncCommunicationDeviceWithPlaybackCheckBox.Checked,
            AutoSwitchToNewPlaybackDevice = _autoSwitchToNewPlaybackDeviceCheckBox.Checked,
            EnableProfiles = _enableProfilesCheckBox.Checked,
            PrimaryDevice = new DeviceSelection
            {
                Id = primaryDevice.Id,
                Name = primaryDevice.Name
            },
            SecondaryDevice = new DeviceSelection
            {
                Id = secondaryDevice.Id,
                Name = secondaryDevice.Name
            },
            NotificationIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            PrimaryIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            SecondaryIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value),
            OverlayAnchor = GetSelectedOverlayAnchor(),
            OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation(),
            Profiles = BuildProfilesForConfig(),
            ToggleRules = BuildToggleRulesForConfig(),
            Hotkey = new HotkeySettings
            {
                Enabled = _enableHotkeyCheckBox.Checked,
                Control = _controlCheckBox.Checked,
                Alt = _altCheckBox.Checked,
                Shift = _shiftCheckBox.Checked,
                WindowsKey = _windowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_hotkeyComboBox, Keys.F10)
            },
            RecentSwitchUndoHotkey = new HotkeySettings
            {
                Enabled = _enableRecentSwitchUndoHotkeyCheckBox.Checked,
                Control = _recentSwitchUndoControlCheckBox.Checked,
                Alt = _recentSwitchUndoAltCheckBox.Checked,
                Shift = _recentSwitchUndoShiftCheckBox.Checked,
                WindowsKey = _recentSwitchUndoWindowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_recentSwitchUndoHotkeyComboBox, Keys.Z)
            },
            OverlayHotkey = new HotkeySettings
            {
                Enabled = _enableOverlayHotkeyCheckBox.Checked,
                Control = _overlayControlCheckBox.Checked,
                Alt = _overlayAltCheckBox.Checked,
                Shift = _overlayShiftCheckBox.Checked,
                WindowsKey = _overlayWindowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_overlayHotkeyComboBox, Keys.V)
            }
        };

        _services.Localizer.SetLanguage(config.Language);

        if (!_services.ConfigStore.Validate(config, out var errorMessage))
        {
            SetStatus(errorMessage!, isError: true);

            if (showValidationUi)
            {
                MessageBox.Show(this, errorMessage!, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            config = null;
            return false;
        }

        return true;
    }

    private void PersistOverlayPresentationSelections()
    {
        if (_isLoading || _suppressAutoSave)
        {
            return;
        }

        AppConfig? config = _cachedConfig;
        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            return;
        }

        if (config is null)
        {
            return;
        }

        config.Language = _languageComboBox.SelectedValue is AppLanguage selectedLanguage
            ? selectedLanguage
            : _services.Localizer.CurrentLanguage;
        config.Theme = GetSelectedThemeMode();
        config.OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value);
        config.OverlayAnchor = GetSelectedOverlayAnchor();
        config.OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation();

        try
        {
            _services.ConfigStore.Save(config);
            _cachedConfig = config;
            SetStatus(_services.Localizer.Get("StatusSettingsSaved"));
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.PersistOverlayPresentationSelections", ex);
        }
    }

    private List<ProcessAudioProfile> BuildProfilesForConfig()
    {
        var count = _editableProfiles.Count;
        return _editableProfiles
            .Select((profile, index) =>
            {
                var clone = profile.Clone();
                clone.Priority = count - index;
                clone.Enabled = true;
                return clone;
            })
            .ToList();
    }

    private List<ProgramToggleRule> BuildToggleRulesForConfig()
    {
        var count = _editableToggleRules.Count;
        return _editableToggleRules
            .Select((rule, index) =>
            {
                var clone = rule.Clone();
                clone.Priority = count - index;
                clone.Enabled = true;
                return clone;
            })
            .ToList();
    }

}
