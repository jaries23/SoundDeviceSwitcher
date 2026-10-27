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
    private void InitializeRecentAudioStateTracking()
    {
        try
        {
            _recentAudioStateObservationTimer.Stop();
            _recentUndoWindowTimer.Stop();
            _recentAudioStateTracker.Initialize(_services.AudioDeviceService.CaptureCurrentState());
            _ignoreObservedAudioDeviceChangesUntilUtc = DateTime.MinValue;
            UpdateRecentSwitchUndoHotkeyRegistration();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.InitializeRecentAudioStateTracking", ex);
        }
    }

    private T ExecuteTrackedAudioChange<T>(Func<T> action)
    {
        FlushPendingObservedAudioDeviceStateCapture();
        SuppressObservedAudioDeviceNotifications();

        try
        {
            return action();
        }
        finally
        {
            CaptureCurrentAudioDeviceStateAsRecentChange();
        }
    }

    private void QueueObservedAudioDeviceStateCapture()
    {
        if (IsDisposed || _isLoading || ShouldIgnoreObservedAudioDeviceNotifications())
        {
            return;
        }

        _recentAudioStateObservationTimer.Stop();
        _recentAudioStateObservationTimer.Start();
    }

    private void FlushPendingObservedAudioDeviceStateCapture()
    {
        if (!_recentAudioStateObservationTimer.Enabled)
        {
            return;
        }

        _recentAudioStateObservationTimer.Stop();
        CaptureObservedAudioDeviceState();
    }

    private void CaptureObservedAudioDeviceState()
    {
        if (IsDisposed || _isLoading || ShouldIgnoreObservedAudioDeviceNotifications())
        {
            return;
        }

        CaptureCurrentAudioDeviceStateAsRecentChange("observed", collapseIfReturningToPreviousState: true);
    }

    private void CaptureCurrentAudioDeviceStateAsRecentChange(
        string source = "internal",
        bool collapseIfReturningToPreviousState = false)
    {
        try
        {
            var currentState = _services.AudioDeviceService.CaptureCurrentState();
            var changeResult = _recentAudioStateTracker.RecordStateChange(
                currentState,
                DateTime.UtcNow,
                collapseIfReturningToPreviousState);
            if (changeResult == RecentAudioStateChangeResult.None)
            {
                return;
            }

            if (changeResult == RecentAudioStateChangeResult.Cleared)
            {
                AppLogger.LogInfo($"Recent audio state cleared. Source={source}");
                _recentUndoWindowTimer.Stop();
                UpdateRecentSwitchUndoHotkeyRegistration();
                return;
            }

            AppLogger.LogInfo($"Recent audio state updated. Source={source}");
            RestartRecentUndoWindowTimer();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.CaptureCurrentAudioDeviceStateAsRecentChange", ex);
        }
    }

    private void RestartRecentUndoWindowTimer()
    {
        _recentUndoWindowTimer.Stop();
        if (!IsRecentSwitchUndoWindowActive())
        {
            UpdateRecentSwitchUndoHotkeyRegistration();
            return;
        }

        UpdateRecentSwitchUndoHotkeyRegistration();
        _recentUndoWindowTimer.Start();
    }

    private bool IsRecentSwitchUndoWindowActive()
    {
        var undoAvailableUntilUtc = _recentAudioStateTracker.UndoAvailableUntilUtc;
        return undoAvailableUntilUtc.HasValue && DateTime.UtcNow <= undoAvailableUntilUtc.Value;
    }

    private void SuppressObservedAudioDeviceNotifications()
    {
        _recentAudioStateObservationTimer.Stop();
        _ignoreObservedAudioDeviceChangesUntilUtc = DateTime.UtcNow.AddMilliseconds(RecentAudioStateNotificationSuppressionMs);
    }

    private bool ShouldIgnoreObservedAudioDeviceNotifications()
    {
        return DateTime.UtcNow < _ignoreObservedAudioDeviceChangesUntilUtc;
    }

    private void UndoRecentAudioDeviceChange()
    {
        FlushPendingObservedAudioDeviceStateCapture();

        if (!_recentAudioStateTracker.TryGetUndoTarget(DateTime.UtcNow, out var targetState, out var availability) ||
            targetState is null)
        {
            _recentAudioStateTracker.InvalidateUndo();
            _recentUndoWindowTimer.Stop();
            UpdateRecentSwitchUndoHotkeyRegistration();

            var unavailableMessage = availability == RecentAudioUndoAvailability.Expired
                ? _services.Localizer.Get("ErrorRecentSwitchUndoExpired")
                : _services.Localizer.Get("ErrorRecentSwitchUndoUnavailable");
            SetStatus(unavailableMessage, isError: true);
            AppLogger.LogInfo($"Recent audio undo ignored. Availability={availability}");
            return;
        }

        SuppressObservedAudioDeviceNotifications();
        var result = _services.AudioDeviceService.ApplyState(targetState);
        if (result.Success)
        {
            _recentAudioStateTracker.CommitUndo(targetState, DateTime.UtcNow);
            RestartRecentUndoWindowTimer();
        }
        else
        {
            CaptureCurrentAudioDeviceStateAsRecentChange("undo-failed");
        }

        SetStatus(result.Message, isError: !result.Success);
        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            2500,
            imagePath: NotificationIconCatalog.ResolvePath(_cachedConfig?.NotificationIconFileName ?? AppConfig.DefaultIconFileName));
    }

    private void RunWithoutAutoSave(Action action)
    {
        var previousState = _suppressAutoSave;
        _suppressAutoSave = true;

        try
        {
            action();
        }
        finally
        {
            _suppressAutoSave = previousState;
        }
    }

    private void HandleRestoreRequest()
    {
        if (IsDisposed)
        {
            return;
        }

        if (_isHiddenToTray)
        {
            RestoreFromTray();
            return;
        }

        if (!Visible)
        {
            Show();
        }

        RevealWindow();
    }

}
