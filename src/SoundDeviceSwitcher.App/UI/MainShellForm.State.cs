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
    private void HandleLanguageChanged()
    {
        if (_isLoading || _languageComboBox.SelectedValue is not AppLanguage language)
        {
            return;
        }

        _services.Localizer.SetLanguage(language);
        ApplyLanguage();
        ApplyTheme(GetSelectedThemeMode());
        AutoSaveCurrentSelections(_services.Localizer.Get("StatusLanguageChanged"));
    }

    private void HandleThemeChanged(AppThemeMode themeMode)
    {
        if (_isLoading)
        {
            return;
        }

        ApplyTheme(themeMode);
        AutoSaveCurrentSelections(_services.Localizer.Get("StatusThemeChanged"));
    }

    private void HandleUpdateNotificationsChanged()
    {
        if (_isLoading)
        {
            return;
        }

        if (_enableUpdateNotificationsCheckBox.Checked)
        {
            BeginUpdateCheck();
        }
        else
        {
            ClearAvailableUpdate();
        }

        AutoSaveCurrentSelections();
    }

    private void HandleCommunicationDeviceSyncChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();

        if (_syncCommunicationDeviceWithPlaybackCheckBox.Checked)
        {
            SyncCommunicationDeviceToCurrentPlaybackIfEnabled(trackChange: true);
            SyncCommunicationDeviceToCurrentRecordingIfEnabled(trackChange: true);
        }
    }

    private void HandleAutoSwitchToNewPlaybackDeviceChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();
    }

    private void HandleProfilesFeatureChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();
    }

    private void SafeLoadInitialState()
    {
        try
        {
            LoadInitialState();
            InitializeAudioDeviceNotificationMonitor();
            SyncCommunicationDeviceToCurrentPlaybackIfEnabled(trackChange: false);
            SyncCommunicationDeviceToCurrentRecordingIfEnabled(trackChange: false);
            EvaluateProcessProfiles();
            InitializeRecentAudioStateTracking();
            BeginUpdateCheck();

            if (!_startHiddenToTrayRequested && !_isHiddenToTray)
            {
                BeginInvoke(new Action(RevealWindow));
            }

            if (_launchedFromPostInstall)
            {
                BeginInvoke(new Action(EnsureVisibleAfterPostInstall));
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.Load", ex);
            var message =
                _services.Localizer.Get("ErrorAppCouldNotLoad") +
                "\n\n" +
                ex.Message +
                $"\n\nLog file: {AppLogger.LatestLogPath}";
            RevealWindow();
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadInitialState()
    {
        _isLoading = true;

        try
        {
            _languageComboBox.SelectedValue = _services.Localizer.CurrentLanguage;
            SetThemeSelection(AppThemeMode.System);
            ApplyLanguage();
            ApplyTheme(AppThemeMode.System);
            RefreshDeviceLists(preserveSelections: false);
            RefreshAvailableIcons(preserveSelections: false);

            if (_services.ConfigStore.TryLoad(out var config, out _))
            {
                _services.Localizer.SetLanguage(config!.Language);
                _languageComboBox.SelectedValue = config.Language;
                ApplyLanguage();
                SetThemeSelection(config.Theme);
                ApplyTheme(config.Theme);
                ApplySavedConfig(config);
                SetStatus(_services.Localizer.Get("LoadedSavedSettings"));

                if (_launchedFromStartup && config.StartMinimizedAtStartup)
                {
                    WindowState = FormWindowState.Minimized;
                    _startHiddenToTrayRequested = true;
                    BeginInvoke(new Action(() => HideToTray(showNotification: false)));
                }
            }
            else
            {
                _cachedConfig = null;
                SelectFirstTwoDevices();
                SetStatus(_services.Localizer.Get("NoSavedSettings"));
            }

            UpdateHotkeyControls();
            UpdateRecentSwitchUndoHotkeyControls();
            UpdateOverlayHotkeyControls();
            UpdateStartupControls();
            UpdateTrayHotkeyRegistration();
            UpdateRecentSwitchUndoHotkeyRegistration();
            UpdateOverlayHotkeyRegistration();
            RefreshProfileLists(refreshDeviceChoices: true);
            RefreshMainProgramToggleList(refreshDeviceChoices: true);
            RefreshToggleRuleLists(refreshDeviceChoices: true);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void RefreshDeviceLists(bool preserveSelections)
    {
        RunWithoutAutoSave(() =>
        {
            var selectedPrimaryId = preserveSelections ? _primaryDeviceComboBox.SelectedValue as string : null;
            var selectedSecondaryId = preserveSelections ? _secondaryDeviceComboBox.SelectedValue as string : null;

            var devices = _services.AudioDeviceService.GetPlaybackDevices().ToList();
            _primaryDeviceComboBox.DataSource = devices.ToList();
            _secondaryDeviceComboBox.DataSource = devices.ToList();

            if (!string.IsNullOrWhiteSpace(selectedPrimaryId))
            {
                SelectDevice(_primaryDeviceComboBox, selectedPrimaryId);
            }

            if (!string.IsNullOrWhiteSpace(selectedSecondaryId))
            {
                SelectDevice(_secondaryDeviceComboBox, selectedSecondaryId);
            }

            if (_primaryDeviceComboBox.SelectedIndex < 0 || _secondaryDeviceComboBox.SelectedIndex < 0)
            {
                SelectFirstTwoDevices();
            }

            RefreshProfileLists(refreshDeviceChoices: true);
            RefreshMainProgramToggleList(refreshDeviceChoices: true);
            RefreshToggleRuleLists(refreshDeviceChoices: true);
        });
    }

    private void ApplySavedConfig(AppConfig config)
    {
        _cachedConfig = config;
        _startWithWindowsCheckBox.Checked = config.StartWithWindows;
        _startMinimizedAtStartupCheckBox.Checked = config.StartMinimizedAtStartup;
        _minimizeToTrayOnCloseCheckBox.Checked = config.MinimizeToTrayOnClose;
        _enableUpdateNotificationsCheckBox.Checked = config.EnableUpdateNotifications;
        _syncCommunicationDeviceWithPlaybackCheckBox.Checked = config.SyncCommunicationDeviceWithPlayback;
        _autoSwitchToNewPlaybackDeviceCheckBox.Checked = config.AutoSwitchToNewPlaybackDevice;
        _enableProfilesCheckBox.Checked = config.EnableProfiles;
        SelectDevice(_primaryDeviceComboBox, config.PrimaryDevice.Id);
        SelectDevice(_secondaryDeviceComboBox, config.SecondaryDevice.Id);
        var iconFileName = string.IsNullOrWhiteSpace(config.NotificationIconFileName)
            ? !string.IsNullOrWhiteSpace(config.PrimaryIconFileName)
                ? config.PrimaryIconFileName
                : config.SecondaryIconFileName
            : config.NotificationIconFileName;
        SelectIcon(_primaryIconComboBox, iconFileName);
        SelectIcon(_secondaryIconComboBox, iconFileName);
        _enableHotkeyCheckBox.Checked = config.Hotkey.Enabled;
        _controlCheckBox.Checked = config.Hotkey.Control;
        _altCheckBox.Checked = config.Hotkey.Alt;
        _shiftCheckBox.Checked = config.Hotkey.Shift;
        _windowsCheckBox.Checked = config.Hotkey.WindowsKey;
        SelectKeyOption(_hotkeyComboBox, config.Hotkey.Key);
        _enableRecentSwitchUndoHotkeyCheckBox.Checked = config.RecentSwitchUndoHotkey.Enabled;
        _recentSwitchUndoControlCheckBox.Checked = config.RecentSwitchUndoHotkey.Control;
        _recentSwitchUndoAltCheckBox.Checked = config.RecentSwitchUndoHotkey.Alt;
        _recentSwitchUndoShiftCheckBox.Checked = config.RecentSwitchUndoHotkey.Shift;
        _recentSwitchUndoWindowsCheckBox.Checked = config.RecentSwitchUndoHotkey.WindowsKey;
        SelectKeyOption(_recentSwitchUndoHotkeyComboBox, config.RecentSwitchUndoHotkey.Key);
        _enableOverlayHotkeyCheckBox.Checked = config.OverlayHotkey.Enabled;
        _overlayControlCheckBox.Checked = config.OverlayHotkey.Control;
        _overlayAltCheckBox.Checked = config.OverlayHotkey.Alt;
        _overlayShiftCheckBox.Checked = config.OverlayHotkey.Shift;
        _overlayWindowsCheckBox.Checked = config.OverlayHotkey.WindowsKey;
        SelectKeyOption(_overlayHotkeyComboBox, config.OverlayHotkey.Key);
        _overlayHeightNumericUpDown!.Value = Math.Clamp(config.OverlayHeightPercent, (int)_overlayHeightNumericUpDown.Minimum, (int)_overlayHeightNumericUpDown.Maximum);
        SetOverlayAnchorSelection(config.OverlayAnchor);
        SetOverlayLayoutOrientationSelection(config.OverlayLayoutOrientation);
        _editableProfiles.Clear();
        _editableProfiles.AddRange(
            config.Profiles
                .OrderByDescending(profile => profile.Priority)
                .ThenBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
                .Select(profile =>
                {
                    var clone = profile.Clone();
                    clone.Enabled = true;
                    return clone;
                }));
        _editableToggleRules.Clear();
        _editableToggleRules.AddRange(
            config.ToggleRules
                .OrderByDescending(rule => rule.Priority)
                .ThenBy(rule => rule.Name, StringComparer.OrdinalIgnoreCase)
                .Select(rule =>
                {
                    var clone = rule.Clone();
                    clone.Enabled = true;
                    return clone;
                }));
        RefreshProfileLists(refreshDeviceChoices: true);
        RefreshMainProgramToggleList(refreshDeviceChoices: true);
        RefreshToggleRuleLists(refreshDeviceChoices: true);
    }

    private void SelectFirstTwoDevices()
    {
        if (_primaryDeviceComboBox.Items.Count > 0)
        {
            _primaryDeviceComboBox.SelectedIndex = 0;
        }

        if (_secondaryDeviceComboBox.Items.Count > 1)
        {
            _secondaryDeviceComboBox.SelectedIndex = 1;
        }
        else if (_secondaryDeviceComboBox.Items.Count > 0)
        {
            _secondaryDeviceComboBox.SelectedIndex = 0;
        }
    }

    private static void SelectDevice(ComboBox comboBox, string deviceId)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is AudioDeviceInfo device &&
                string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static void SelectIcon(ComboBox comboBox, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var normalizedFileName = NotificationIconCatalog.NormalizeFileName(fileName);

        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is NotificationIconCatalog.IconChoice choice &&
                string.Equals(choice.FileName, normalizedFileName, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static NotificationIconCatalog.IconChoice? GetSelectedIconChoice(ComboBox comboBox)
    {
        return comboBox.SelectedItem as NotificationIconCatalog.IconChoice;
    }

    private static string GetSelectedIconFileName(ComboBox comboBox, string fallbackFileName)
    {
        return GetSelectedIconChoice(comboBox)?.FileName ?? fallbackFileName;
    }

    private static void SelectKeyOption(ComboBox comboBox, Keys key)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is KeyOption option && option.Value == key)
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static Keys GetSelectedKeyOption(ComboBox comboBox, Keys fallbackKey)
    {
        return comboBox.SelectedItem is KeyOption option ? option.Value : fallbackKey;
    }

}
