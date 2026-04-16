namespace SoundDeviceSwitcher.App.Audio;

public sealed class AudioDeviceStateApplyResult
{
    private AudioDeviceStateApplyResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }

    public string Message { get; }

    public static AudioDeviceStateApplyResult Ok(string message) => new(true, message);

    public static AudioDeviceStateApplyResult Fail(string message) => new(false, message);
}
