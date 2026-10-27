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
    private void SetStatus(string message, bool isError = false)
    {
        _lastStatusWasError = isError;
        _statusLabel.ForeColor = isError ? _activePalette.ErrorText : _activePalette.SuccessText;
        _statusLabel.Text = message;
    }

    private void EnsureForegroundChangeHook()
    {
        if (_foregroundChangeHook != IntPtr.Zero || IsDisposed)
        {
            return;
        }

        _foregroundChangeHook = SetWinEventHook(
            EventSystemForeground,
            EventSystemForeground,
            IntPtr.Zero,
            _foregroundChangedCallback,
            0,
            0,
            WinEventOutOfContext);

        if (_foregroundChangeHook == IntPtr.Zero)
        {
            AppLogger.LogInfo(
                $"Foreground hook unavailable. Falling back to timer-based profile evaluation. Win32Error={Marshal.GetLastWin32Error()}");
        }
    }

    private void ReleaseForegroundChangeHook()
    {
        if (_foregroundChangeHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWinEvent(_foregroundChangeHook);
        _foregroundChangeHook = IntPtr.Zero;
    }

    private void HandleForegroundChanged(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime)
    {
        if (IsDisposed || !IsHandleCreated || !_initialStateLoaded)
        {
            return;
        }

        BeginInvoke(new Action(() => EvaluateProcessProfiles()));
    }

    private void RefreshProfileEvaluationTimer(bool requireBackgroundMonitoring)
    {
        var shouldRun = _foregroundChangeHook == IntPtr.Zero || requireBackgroundMonitoring;
        if (shouldRun)
        {
            if (!_profileEvaluationTimer.Enabled)
            {
                _profileEvaluationTimer.Start();
            }

            return;
        }

        if (_profileEvaluationTimer.Enabled)
        {
            _profileEvaluationTimer.Stop();
        }
    }

    private void InitializeAudioDeviceNotificationMonitor()
    {
        if (_audioDeviceNotificationMonitor is not null || IsDisposed)
        {
            return;
        }

        try
        {
            _audioDeviceNotificationMonitor = new AudioDeviceNotificationMonitor();
            _audioDeviceNotificationMonitor.DefaultAudioDeviceChanged += HandleTrackedDefaultAudioDeviceChanged;
            _audioDeviceNotificationMonitor.DefaultPlaybackDeviceChanged += HandleDefaultPlaybackDeviceChanged;
            _audioDeviceNotificationMonitor.DefaultRecordingDeviceChanged += HandleDefaultRecordingDeviceChanged;
            _audioDeviceNotificationMonitor.AudioDeviceAdded += HandleAudioDeviceAdded;
            _audioDeviceNotificationMonitor.Start();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.AudioDeviceNotificationMonitor", ex);
            _audioDeviceNotificationMonitor?.Dispose();
            _audioDeviceNotificationMonitor = null;
        }
    }

    private void HandleTrackedDefaultAudioDeviceChanged(object? sender, DefaultAudioDeviceChangedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(QueueObservedAudioDeviceStateCapture));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void HandleDefaultPlaybackDeviceChanged(object? sender, DefaultPlaybackDeviceChangedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (ShouldIgnoreObservedAudioDeviceNotifications())
                {
                    return;
                }

                SyncCommunicationDeviceWithPlayback(eventArgs.DeviceId, trackChange: false);
            }));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void HandleDefaultRecordingDeviceChanged(object? sender, DefaultRecordingDeviceChangedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (ShouldIgnoreObservedAudioDeviceNotifications())
                {
                    return;
                }

                SyncCommunicationDeviceWithRecording(eventArgs.DeviceId, trackChange: false);
            }));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void HandleAudioDeviceAdded(object? sender, AudioDeviceAddedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() => _ = HandleNewPlaybackDeviceConnectedAsync(eventArgs.DeviceId)));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void SyncCommunicationDeviceToCurrentPlaybackIfEnabled(bool trackChange)
    {
        if (!_syncCommunicationDeviceWithPlaybackCheckBox.Checked)
        {
            return;
        }

        var playbackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
        if (playbackDevice is null)
        {
            return;
        }

        SyncCommunicationDeviceWithPlayback(playbackDevice.Id, trackChange);
    }

    private void SyncCommunicationDeviceWithPlayback(string playbackDeviceId, bool trackChange)
    {
        if (IsDisposed ||
            _isLoading ||
            !_syncCommunicationDeviceWithPlaybackCheckBox.Checked ||
            string.IsNullOrWhiteSpace(playbackDeviceId))
        {
            return;
        }

        try
        {
            var communicationDevice = _services.AudioDeviceService.GetDefaultCommunicationDevice();
            if (communicationDevice is not null &&
                string.Equals(communicationDevice.Id, playbackDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (trackChange)
            {
                _ = ExecuteTrackedAudioChange(() =>
                {
                    _services.AudioDeviceService.SetDefaultCommunicationDevice(playbackDeviceId);
                    return true;
                });
                return;
            }

            _services.AudioDeviceService.SetDefaultCommunicationDevice(playbackDeviceId);
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.SyncCommunicationDeviceWithPlayback", ex);
        }
    }

    private void SyncCommunicationDeviceToCurrentRecordingIfEnabled(bool trackChange)
    {
        if (!_syncCommunicationDeviceWithPlaybackCheckBox.Checked)
        {
            return;
        }

        var recordingDevice = _services.AudioDeviceService.GetDefaultRecordingDevice();
        if (recordingDevice is null)
        {
            return;
        }

        SyncCommunicationDeviceWithRecording(recordingDevice.Id, trackChange);
    }

    private void SyncCommunicationDeviceWithRecording(string recordingDeviceId, bool trackChange)
    {
        if (IsDisposed ||
            _isLoading ||
            !_syncCommunicationDeviceWithPlaybackCheckBox.Checked ||
            string.IsNullOrWhiteSpace(recordingDeviceId))
        {
            return;
        }

        try
        {
            var communicationDevice = _services.AudioDeviceService.GetDefaultCommunicationRecordingDevice();
            if (communicationDevice is not null &&
                string.Equals(communicationDevice.Id, recordingDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (trackChange)
            {
                _ = ExecuteTrackedAudioChange(() =>
                {
                    _services.AudioDeviceService.SetDefaultCommunicationRecordingDevice(recordingDeviceId);
                    return true;
                });
                return;
            }

            _services.AudioDeviceService.SetDefaultCommunicationRecordingDevice(recordingDeviceId);
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.SyncCommunicationDeviceWithRecording", ex);
        }
    }

    private async Task HandleNewPlaybackDeviceConnectedAsync(string deviceId)
    {
        if (IsDisposed ||
            _isLoading ||
            !_autoSwitchToNewPlaybackDeviceCheckBox.Checked ||
            string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        AppLogger.LogInfo($"Auto-switch candidate received. DeviceId={deviceId}");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (!_services.AudioDeviceService.IsPlaybackDevice(deviceId))
                {
                    AppLogger.LogInfo($"Auto-switch candidate is not an active playback device yet. DeviceId={deviceId}, Attempt={attempt + 1}");

                    if (attempt < 4)
                    {
                        await Task.Delay(400);
                        continue;
                    }

                    return;
                }

                var currentPlaybackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
                var currentCommunicationDevice = _services.AudioDeviceService.GetDefaultCommunicationDevice();
                if (currentPlaybackDevice is not null &&
                    currentCommunicationDevice is not null &&
                    string.Equals(currentPlaybackDevice.Id, deviceId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(currentCommunicationDevice.Id, deviceId, StringComparison.OrdinalIgnoreCase))
                {
                    AppLogger.LogInfo($"Auto-switch skipped because defaults already match. DeviceId={deviceId}");
                    return;
                }

                _ = ExecuteTrackedAudioChange(() =>
                {
                    _services.AudioDeviceService.SetDefaultPlaybackAndCommunicationDevice(deviceId);
                    return true;
                });
                AppLogger.LogInfo($"Auto-switch applied. DeviceId={deviceId}, Attempt={attempt + 1}");
                return;
            }
            catch (Exception ex)
            {
                if (attempt < 4)
                {
                    AppLogger.LogException($"MainShellForm.HandleNewPlaybackDeviceConnectedAsync attempt {attempt + 1}", ex);
                    await Task.Delay(400);
                    continue;
                }

                AppLogger.LogException("MainShellForm.HandleNewPlaybackDeviceConnectedAsync", ex);
            }
        }
    }

}
