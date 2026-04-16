namespace SoundDeviceSwitcher.App.Configuration;

public sealed class ProcessAudioProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string IconFileName { get; set; } = AppConfig.DefaultIconFileName;

    public int Priority { get; set; } = 100;

    public bool Enabled { get; set; } = true;

    public DeviceSelection PlaybackDevice { get; set; } = new();

    public DeviceSelection RecordingDevice { get; set; } = new();

    public List<ProfileProgramTarget> Programs { get; set; } = [];

    public ProcessAudioProfile Clone()
    {
        return new ProcessAudioProfile
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id,
            Name = Name,
            IconFileName = string.IsNullOrWhiteSpace(IconFileName)
                ? AppConfig.DefaultIconFileName
                : IconFileName,
            Priority = Priority,
            Enabled = Enabled,
            PlaybackDevice = new DeviceSelection
            {
                Id = PlaybackDevice.Id,
                Name = PlaybackDevice.Name
            },
            RecordingDevice = new DeviceSelection
            {
                Id = RecordingDevice.Id,
                Name = RecordingDevice.Name
            },
            Programs = Programs.Select(program => program.Clone()).ToList()
        };
    }
}

public sealed class ProfileProgramTarget
{
    public string DisplayName { get; set; } = string.Empty;

    public string ExecutableName { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public ProfileProgramTarget Clone()
    {
        return new ProfileProgramTarget
        {
            DisplayName = DisplayName,
            ExecutableName = ExecutableName,
            ExecutablePath = ExecutablePath
        };
    }
}
