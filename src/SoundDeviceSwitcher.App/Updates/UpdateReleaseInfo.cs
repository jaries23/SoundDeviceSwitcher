namespace SoundDeviceSwitcher.App.Updates;

internal sealed class UpdateReleaseInfo
{
    public required string VersionTag { get; init; }

    public required string VersionDisplay { get; init; }

    public required Version Version { get; init; }

    public required string HtmlUrl { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
}
