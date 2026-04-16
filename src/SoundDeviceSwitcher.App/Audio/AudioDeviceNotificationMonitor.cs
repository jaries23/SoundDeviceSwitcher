using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Diagnostics;

namespace SoundDeviceSwitcher.App.Audio;

internal sealed class AudioDeviceNotificationMonitor : IDisposable
{
    private IMMDeviceEnumerator? _enumerator;
    private NotificationClient? _notificationClient;

    public event EventHandler<DefaultAudioDeviceChangedEventArgs>? DefaultAudioDeviceChanged;

    public event EventHandler<DefaultPlaybackDeviceChangedEventArgs>? DefaultPlaybackDeviceChanged;

    public event EventHandler<AudioDeviceAddedEventArgs>? AudioDeviceAdded;

    public void Start()
    {
        if (_enumerator is not null)
        {
            return;
        }

        var enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
        var notificationClient = new NotificationClient(HandleDefaultDeviceChanged, HandleDeviceAdded, HandleDeviceStateChanged);
        var registrationResult = enumerator.RegisterEndpointNotificationCallback(notificationClient);
        if (registrationResult < 0)
        {
            Marshal.ThrowExceptionForHR(registrationResult);
        }

        _enumerator = enumerator;
        _notificationClient = notificationClient;
    }

    public void Dispose()
    {
        if (_enumerator is not null && _notificationClient is not null)
        {
            _ = _enumerator.UnregisterEndpointNotificationCallback(_notificationClient);
        }

        if (_enumerator is not null && Marshal.IsComObject(_enumerator))
        {
            Marshal.ReleaseComObject(_enumerator);
        }

        _notificationClient = null;
        _enumerator = null;
    }

    private void HandleDefaultDeviceChanged(EDataFlow flow, ERole role, string? deviceId)
    {
        AppLogger.LogInfo($"Audio notification: default device changed. Flow={flow}, Role={role}, DeviceId={deviceId ?? "<null>"}");

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        if (IsTrackedDefaultDevice(flow, role))
        {
            DefaultAudioDeviceChanged?.Invoke(this, new DefaultAudioDeviceChangedEventArgs(flow, role, deviceId));
        }

        if (flow == EDataFlow.Render && role != ERole.Communications)
        {
            DefaultPlaybackDeviceChanged?.Invoke(this, new DefaultPlaybackDeviceChangedEventArgs(deviceId));
        }
    }

    private void HandleDeviceAdded(string? deviceId)
    {
        AppLogger.LogInfo($"Audio notification: device added. DeviceId={deviceId ?? "<null>"}");

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        AudioDeviceAdded?.Invoke(this, new AudioDeviceAddedEventArgs(deviceId));
    }

    private void HandleDeviceStateChanged(string? deviceId, DeviceState newState)
    {
        AppLogger.LogInfo($"Audio notification: device state changed. DeviceId={deviceId ?? "<null>"}, State={newState}");

        if (string.IsNullOrWhiteSpace(deviceId) ||
            (newState & DeviceState.Active) == 0)
        {
            return;
        }

        AudioDeviceAdded?.Invoke(this, new AudioDeviceAddedEventArgs(deviceId));
    }

    private static bool IsTrackedDefaultDevice(EDataFlow flow, ERole role)
    {
        return flow switch
        {
            EDataFlow.Render => role == ERole.Console || role == ERole.Multimedia || role == ERole.Communications,
            EDataFlow.Capture => role == ERole.Console || role == ERole.Multimedia,
            _ => false
        };
    }

    [ClassInterface(ClassInterfaceType.None)]
    private sealed class NotificationClient : IMMNotificationClient
    {
        private readonly Action<EDataFlow, ERole, string?> _onDefaultDeviceChanged;
        private readonly Action<string?> _onDeviceAdded;
        private readonly Action<string?, DeviceState> _onDeviceStateChanged;

        public NotificationClient(
            Action<EDataFlow, ERole, string?> onDefaultDeviceChanged,
            Action<string?> onDeviceAdded,
            Action<string?, DeviceState> onDeviceStateChanged)
        {
            _onDefaultDeviceChanged = onDefaultDeviceChanged;
            _onDeviceAdded = onDeviceAdded;
            _onDeviceStateChanged = onDeviceStateChanged;
        }

        public int OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            try
            {
                _onDeviceStateChanged(deviceId, newState);
            }
            catch (Exception ex)
            {
                AppLogger.LogException("AudioDeviceNotificationMonitor.OnDeviceStateChanged", ex);
            }

            return 0;
        }

        public int OnDeviceAdded(string deviceId)
        {
            try
            {
                _onDeviceAdded(deviceId);
            }
            catch (Exception ex)
            {
                AppLogger.LogException("AudioDeviceNotificationMonitor.OnDeviceAdded", ex);
            }

            return 0;
        }

        public int OnDeviceRemoved(string deviceId)
        {
            return 0;
        }

        public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string defaultDeviceId)
        {
            try
            {
                _onDefaultDeviceChanged(flow, role, defaultDeviceId);
            }
            catch (Exception ex)
            {
                AppLogger.LogException("AudioDeviceNotificationMonitor.OnDefaultDeviceChanged", ex);
            }

            return 0;
        }

        public int OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            return 0;
        }
    }
}

internal sealed class DefaultAudioDeviceChangedEventArgs : EventArgs
{
    public DefaultAudioDeviceChangedEventArgs(EDataFlow flow, ERole role, string deviceId)
    {
        Flow = flow;
        Role = role;
        DeviceId = deviceId;
    }

    public EDataFlow Flow { get; }

    public ERole Role { get; }

    public string DeviceId { get; }
}

internal sealed class DefaultPlaybackDeviceChangedEventArgs : EventArgs
{
    public DefaultPlaybackDeviceChangedEventArgs(string deviceId)
    {
        DeviceId = deviceId;
    }

    public string DeviceId { get; }
}

internal sealed class AudioDeviceAddedEventArgs : EventArgs
{
    public AudioDeviceAddedEventArgs(string deviceId)
    {
        DeviceId = deviceId;
    }

    public string DeviceId { get; }
}
