namespace SoundDeviceSwitcher.App.Audio;

public sealed class AudioDeviceState
{
    public AudioDeviceState(
        AudioEndpointState? playbackDevice,
        AudioEndpointState? recordingDevice,
        AudioEndpointState? communicationDevice)
    {
        PlaybackDevice = playbackDevice;
        RecordingDevice = recordingDevice;
        CommunicationDevice = communicationDevice;
    }

    public AudioEndpointState? PlaybackDevice { get; }

    public AudioEndpointState? RecordingDevice { get; }

    public AudioEndpointState? CommunicationDevice { get; }

    public bool Matches(AudioDeviceState? other)
    {
        if (other is null)
        {
            return false;
        }

        return IsSameDevice(PlaybackDevice, other.PlaybackDevice) &&
               IsSameDevice(RecordingDevice, other.RecordingDevice) &&
               IsSameDevice(CommunicationDevice, other.CommunicationDevice);
    }

    public static bool IsSameDevice(AudioEndpointState? left, AudioEndpointState? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        return string.Equals(left.DeviceId, right.DeviceId, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class AudioEndpointState
{
    public AudioEndpointState(string deviceId, string deviceName)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
    }

    public string DeviceId { get; }

    public string DeviceName { get; }

    public string DisplayName => string.IsNullOrWhiteSpace(DeviceName) ? DeviceId : DeviceName;
}
