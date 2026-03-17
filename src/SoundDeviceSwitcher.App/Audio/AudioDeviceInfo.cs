namespace SoundDeviceSwitcher.App.Audio;

public sealed class AudioDeviceInfo
{
    public AudioDeviceInfo(string id, string name, string displayName)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
    }

    public string Id { get; }

    public string Name { get; }

    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
