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
    private void RefreshAvailableIcons(bool preserveSelections, bool showStatus = false)
    {
        RunWithoutAutoSave(() =>
        {
            var selectedIcon = preserveSelections
                ? GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName)
                : AppConfig.DefaultIconFileName;
            var selectedProfile = GetSelectedProfile();
            var previousSuppressProfileEditorEvents = _suppressProfileEditorEvents;
            _suppressProfileEditorEvents = true;

            try
            {
                var icons = NotificationIconCatalog.GetSelectableIcons(_services.Localizer).ToList();
                SetComboBoxItems(_primaryIconComboBox, icons);
                SetComboBoxItems(_secondaryIconComboBox, icons);
                SetComboBoxItems(_profileIconComboBox, icons);

                var hasIcons = icons.Count > 0;
                _primaryIconComboBox.Enabled = hasIcons;
                _secondaryIconComboBox.Enabled = hasIcons;
                _profileIconComboBox.Enabled = hasIcons && selectedProfile is not null;

                if (!hasIcons)
                {
                    SetPictureBoxImage(_primaryIconPreview, null);
                    SetPictureBoxImage(_secondaryIconPreview, null);
                    SetPictureBoxImage(_profileIconPreview, null);

                    if (showStatus)
                    {
                        SetStatus(_services.Localizer.Get("StatusNoIconsFound"), isError: true);
                    }

                    return;
                }

                SelectIcon(_primaryIconComboBox, selectedIcon);
                SelectIcon(_secondaryIconComboBox, selectedIcon);
                SelectIcon(_profileIconComboBox, selectedProfile?.IconFileName ?? AppConfig.DefaultIconFileName);

                if (_primaryIconComboBox.SelectedIndex < 0)
                {
                    _primaryIconComboBox.SelectedIndex = _primaryIconComboBox.Items.Count > 0 ? 0 : -1;
                }

                if (_secondaryIconComboBox.SelectedIndex < 0)
                {
                    _secondaryIconComboBox.SelectedIndex = _secondaryIconComboBox.Items.Count > 0
                        ? Math.Max(_primaryIconComboBox.SelectedIndex, 0)
                        : -1;
                }

                if (_profileIconComboBox.SelectedIndex < 0)
                {
                    _profileIconComboBox.SelectedIndex = _profileIconComboBox.Items.Count > 0 ? 0 : -1;
                }

                UpdateDeviceIconPreviews();
                if (selectedProfile is null)
                {
                    _profileIconComboBox.SelectedIndex = -1;
                    SetPictureBoxImage(_profileIconPreview, null);
                }
                else
                {
                    UpdateDeviceIconPreview(_profileIconComboBox, _profileIconPreview);
                }

                if (showStatus)
                {
                    SetStatus(_services.Localizer.Format("StatusIconsRefreshed", icons.Count));
                }
            }
            finally
            {
                _suppressProfileEditorEvents = previousSuppressProfileEditorEvents;
            }
        });
    }

    private void OpenIconFolder()
    {
        try
        {
            NotificationIconCatalog.EnsureUserIconFolderInitialized();

            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{NotificationIconCatalog.UserIconDirectoryPath}\"")
            {
                UseShellExecute = true
            });

            SetStatus(_services.Localizer.Get("StatusOpenedIconFolder"));
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateDeviceIconPreviews()
    {
        UpdateDeviceIconPreview(_primaryIconComboBox, _primaryIconPreview);
        UpdateDeviceIconPreview(_secondaryIconComboBox, _secondaryIconPreview);
    }

    private static void UpdateDeviceIconPreview(ComboBox comboBox, PictureBox previewBox)
    {
        var fileName = GetSelectedIconChoice(comboBox)?.FileName;
        var path = NotificationIconCatalog.ResolvePath(fileName);
        SetPictureBoxImage(previewBox, NotificationIconCatalog.LoadImage(path));
    }

    private static void SetPictureBoxImage(PictureBox pictureBox, Image? image)
    {
        var previousImage = pictureBox.Image;
        pictureBox.Image = image;
        previousImage?.Dispose();
    }

    private void UpdateHotkeyControls()
    {
        var enabled = _enableHotkeyCheckBox.Checked;
        _controlCheckBox.Enabled = enabled;
        _altCheckBox.Enabled = enabled;
        _shiftCheckBox.Enabled = enabled;
        _windowsCheckBox.Enabled = enabled;
        _hotkeyComboBox.Enabled = enabled;
    }

    private void UpdateRecentSwitchUndoHotkeyControls()
    {
        var enabled = _enableRecentSwitchUndoHotkeyCheckBox.Checked;
        _recentSwitchUndoControlCheckBox.Enabled = enabled;
        _recentSwitchUndoAltCheckBox.Enabled = enabled;
        _recentSwitchUndoShiftCheckBox.Enabled = enabled;
        _recentSwitchUndoWindowsCheckBox.Enabled = enabled;
        _recentSwitchUndoHotkeyComboBox.Enabled = enabled;
    }

    private void UpdateOverlayHotkeyControls()
    {
        var enabled = _enableOverlayHotkeyCheckBox.Checked;
        _overlayControlCheckBox.Enabled = enabled;
        _overlayAltCheckBox.Enabled = enabled;
        _overlayShiftCheckBox.Enabled = enabled;
        _overlayWindowsCheckBox.Enabled = enabled;
        _overlayHotkeyComboBox.Enabled = enabled;
    }

    private void UpdateStartupControls()
    {
        _startMinimizedAtStartupCheckBox.Enabled = _startWithWindowsCheckBox.Checked;
    }

    private void AutoSaveCurrentSelections(string? successStatus = null)
    {
        if (_isLoading || _suppressAutoSave)
        {
            return;
        }

        PersistCurrentSelections(
            showValidationUi: false,
            showExceptionUi: false,
            successStatus ?? _services.Localizer.Get("StatusSettingsSaved"));
    }

    private void ApplyDefaultHotkeyValues()
    {
        var defaultConfig = new AppConfig();

        RunWithoutAutoSave(() =>
        {
            _controlCheckBox.Checked = defaultConfig.Hotkey.Control;
            _altCheckBox.Checked = defaultConfig.Hotkey.Alt;
            _shiftCheckBox.Checked = defaultConfig.Hotkey.Shift;
            _windowsCheckBox.Checked = defaultConfig.Hotkey.WindowsKey;
            SelectKeyOption(_hotkeyComboBox, defaultConfig.Hotkey.Key);

            _recentSwitchUndoControlCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Control;
            _recentSwitchUndoAltCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Alt;
            _recentSwitchUndoShiftCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Shift;
            _recentSwitchUndoWindowsCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.WindowsKey;
            SelectKeyOption(_recentSwitchUndoHotkeyComboBox, defaultConfig.RecentSwitchUndoHotkey.Key);

            _overlayControlCheckBox.Checked = defaultConfig.OverlayHotkey.Control;
            _overlayAltCheckBox.Checked = defaultConfig.OverlayHotkey.Alt;
            _overlayShiftCheckBox.Checked = defaultConfig.OverlayHotkey.Shift;
            _overlayWindowsCheckBox.Checked = defaultConfig.OverlayHotkey.WindowsKey;
            SelectKeyOption(_overlayHotkeyComboBox, defaultConfig.OverlayHotkey.Key);
            if (_overlayHeightNumericUpDown is not null)
            {
                _overlayHeightNumericUpDown.Value = Math.Clamp(
                    defaultConfig.OverlayHeightPercent,
                    (int)_overlayHeightNumericUpDown.Minimum,
                    (int)_overlayHeightNumericUpDown.Maximum);
            }
            SetOverlayAnchorSelection(defaultConfig.OverlayAnchor);
            SetOverlayLayoutOrientationSelection(defaultConfig.OverlayLayoutOrientation);
        });
    }

}
