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
    private async void BeginUpdateCheck()
    {
        if (_updateCheckInProgress || !_enableUpdateNotificationsCheckBox.Checked || IsDisposed)
        {
            return;
        }

        _updateCheckInProgress = true;

        try
        {
        var updateRelease = await _services.UpdateChecker.CheckForUpdateAsync();
        if (updateRelease is null || IsDisposed || !_enableUpdateNotificationsCheckBox.Checked)
        {
            return;
        }

        ApplyAvailableUpdate(updateRelease);
        }
        finally
        {
            _updateCheckInProgress = false;
        }
    }

    private void ApplyAvailableUpdate(UpdateReleaseInfo updateRelease)
    {
        _availableUpdate = updateRelease;
        _statusActionButton.Visible = true;
        _statusActionButton.Text = _services.Localizer.Get("UpdateStatusButton");
        SetStatus(_services.Localizer.Format("StatusUpdateAvailable", updateRelease.VersionDisplay));

        if (_updateDialogShown ||
            _startHiddenToTrayRequested ||
            _isHiddenToTray ||
            !Visible ||
            !ShowInTaskbar ||
            WindowState == FormWindowState.Minimized)
        {
            return;
        }

        _updateDialogShown = true;
        BeginInvoke(new Action(ShowAvailableUpdateDialog));
    }

    private void ShowAvailableUpdateDialog()
    {
        if (_availableUpdate is null || IsDisposed)
        {
            return;
        }

        using var dialog = new UpdateAvailableDialog(
            _services.Localizer,
            _availableUpdate,
            _services.UpdateChecker.CurrentVersionDisplay,
            GetSelectedThemeMode());
        dialog.ShowDialog(this);
    }

    private void ClearAvailableUpdate()
    {
        _availableUpdate = null;
        _updateDialogShown = false;
        _statusActionButton.Visible = false;
    }
}
