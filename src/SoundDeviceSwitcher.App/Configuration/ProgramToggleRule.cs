namespace SoundDeviceSwitcher.App.Configuration;

public sealed class ProgramToggleRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public int Priority { get; set; } = 100;

    public bool Enabled { get; set; } = true;

    public DeviceSelection PrimaryDevice { get; set; } = new();

    public DeviceSelection SecondaryDevice { get; set; } = new();

    public ProfileToggleTarget Target { get; set; } = ProfileToggleTarget.DefaultDevice;

    public List<ProfileProgramTarget> Programs { get; set; } = [];

    public ProgramToggleRule Clone()
    {
        return new ProgramToggleRule
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id,
            Name = Name,
            Priority = Priority,
            Enabled = Enabled,
            PrimaryDevice = new DeviceSelection
            {
                Id = PrimaryDevice.Id,
                Name = PrimaryDevice.Name
            },
            SecondaryDevice = new DeviceSelection
            {
                Id = SecondaryDevice.Id,
                Name = SecondaryDevice.Name
            },
            Target = Target,
            Programs = Programs.Select(program => program.Clone()).ToList()
        };
    }
}
