namespace SoundDeviceSwitcher.App.Audio;

public sealed class ToggleResult
{
    private ToggleResult(bool success, string message, string? switchedDeviceId)
    {
        Success = success;
        Message = message;
        SwitchedDeviceId = switchedDeviceId;
    }

    public bool Success { get; }

    public string Message { get; }

    public string? SwitchedDeviceId { get; }

    public static ToggleResult Ok(string message, string switchedDeviceId) => new(true, message, switchedDeviceId);

    public static ToggleResult Fail(string message) => new(false, message, null);
}
