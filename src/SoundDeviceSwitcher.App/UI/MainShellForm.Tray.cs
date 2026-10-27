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
    private void HandleFormClosing(FormClosingEventArgs eventArgs)
    {
        if (ShouldSuppressPostInstallClose(eventArgs))
        {
            eventArgs.Cancel = true;
            BeginInvoke(new Action(EnsureVisibleAfterPostInstall));
            return;
        }

        if (_allowClose || _isHiddenToTray)
        {
            if (_allowClose)
            {
                PrepareForHiddenState();
            }

            return;
        }

        if (eventArgs.CloseReason is CloseReason.ApplicationExitCall or CloseReason.WindowsShutDown or CloseReason.TaskManagerClosing)
        {
            PrepareForHiddenState();
            return;
        }

        if (!_minimizeToTrayOnCloseCheckBox.Checked)
        {
            PrepareForHiddenState();
            return;
        }

        PrepareForHiddenState();
        eventArgs.Cancel = true;
        HideToTray(showNotification: true);
    }

    private bool ShouldSuppressPostInstallClose(FormClosingEventArgs eventArgs)
    {
        if (!_postInstallCloseGuardPending ||
            !_launchedFromPostInstall ||
            _allowClose ||
            _isHiddenToTray)
        {
            return false;
        }

        if (eventArgs.CloseReason is CloseReason.ApplicationExitCall or CloseReason.WindowsShutDown or CloseReason.TaskManagerClosing)
        {
            return false;
        }

        if (DateTime.UtcNow - _postInstallStartedAtUtc > TimeSpan.FromSeconds(5))
        {
            return false;
        }

        _postInstallCloseGuardPending = false;
        AppLogger.LogInfo($"Suppressed post-install close request. Reason={eventArgs.CloseReason}.");
        return true;
    }

    private void HideToTray(bool showNotification)
    {
        if (_isHiddenToTray)
        {
            return;
        }

        _startHiddenToTrayRequested = false;
        _isHiddenToTray = true;
        PrepareForHiddenState();
        UpdateTrayHotkeyRegistration(showErrorNotification: showNotification);

        if (showNotification)
        {
            _trayNotifications.Show(
                _services.Localizer.Get("StatusMinimizedToTray"),
                ToolTipIcon.Info,
                imagePath: NotificationIconCatalog.ResolvePath(AppConfig.DefaultIconFileName));
        }
    }

    private void RestoreFromTray()
    {
        if (!_isHiddenToTray)
        {
            RevealWindow();
            return;
        }

        _isHiddenToTray = false;
        RevealWindow();
        UpdateTrayHotkeyRegistration();

        if (_availableUpdate is not null && !_updateDialogShown)
        {
            _updateDialogShown = true;
            BeginInvoke(new Action(ShowAvailableUpdateDialog));
        }
    }

    private void ExitFromTray()
    {
        _allowClose = true;
        _notifyIcon.Visible = false;
        PrepareForHiddenState();
        Close();
    }

    private void RevealWindow()
    {
        if (IsDisposed)
        {
            return;
        }

        SuspendLayout();

        try
        {
            _allowFormVisibility = true;
            ShowInTaskbar = true;

            if (!Visible)
            {
                Show();
            }

            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            BringToFront();
            Activate();
            ForceForeground();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }
    }

    private async void EnsureVisibleAfterPostInstall()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            if (IsDisposed)
            {
                return;
            }

            _startHiddenToTrayRequested = false;
            _isHiddenToTray = false;
            RevealWindow();

            if (attempt < 2)
            {
                await Task.Delay(500);
            }
        }
    }

    private void ForceForeground()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        ShowWindow(Handle, SwRestore);
        SetForegroundWindow(Handle);
        TopMost = true;
        TopMost = false;
    }

    private void PrepareForHiddenState()
    {
        if (IsDisposed)
        {
            return;
        }

        SuspendLayout();

        try
        {
            _allowFormVisibility = false;
            ShowInTaskbar = false;
            CloseProfileOverlay();

            if (Visible)
            {
                Hide();
            }
        }
        finally
        {
            ResumeLayout(performLayout: false);
        }
    }

}
