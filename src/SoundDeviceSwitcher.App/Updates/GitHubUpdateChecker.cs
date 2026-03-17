using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SoundDeviceSwitcher.App.Diagnostics;

namespace SoundDeviceSwitcher.App.Updates;

internal sealed class GitHubUpdateChecker
{
    private const string RepositoryOwner = "jaries23";
    private const string RepositoryName = "SoundDeviceSwitcher";
    private static readonly Uri LatestReleaseUri = new($"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest");
    private static readonly HttpClient HttpClient = CreateHttpClient();

    public GitHubUpdateChecker()
    {
        CurrentVersion = NormalizeVersion(Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0, 0));
        CurrentVersionDisplay = FormatVersion(CurrentVersion);
    }

    public Version CurrentVersion { get; }

    public string CurrentVersionDisplay { get; }

    public async Task<UpdateReleaseInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<LatestReleaseResponse>(stream, cancellationToken: cancellationToken);
            if (payload is null || payload.Draft || payload.Prerelease || string.IsNullOrWhiteSpace(payload.TagName))
            {
                return null;
            }

            if (!TryParseVersion(payload.TagName, out var latestVersion))
            {
                return null;
            }

            if (latestVersion <= CurrentVersion)
            {
                return null;
            }

            return new UpdateReleaseInfo
            {
                Version = latestVersion,
                VersionTag = payload.TagName,
                VersionDisplay = FormatVersion(latestVersion),
                HtmlUrl = string.IsNullOrWhiteSpace(payload.HtmlUrl)
                    ? $"https://github.com/{RepositoryOwner}/{RepositoryName}/releases"
                    : payload.HtmlUrl,
                PublishedAt = payload.PublishedAt
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            AppLogger.LogException("UpdateCheck", ex);
            return null;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SoundDeviceSwitcher");
        return client;
    }

    private static bool TryParseVersion(string rawVersion, out Version version)
    {
        var normalized = rawVersion.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        var suffixIndex = normalized.IndexOfAny(['-', '+', ' ']);
        if (suffixIndex >= 0)
        {
            normalized = normalized[..suffixIndex];
        }

        if (!Version.TryParse(normalized, out var parsed))
        {
            version = new Version(0, 0, 0, 0);
            return false;
        }

        version = NormalizeVersion(parsed);
        return true;
    }

    private static Version NormalizeVersion(Version version)
    {
        return new Version(
            version.Major,
            Math.Max(0, version.Minor),
            Math.Max(0, version.Build),
            version.Revision >= 0 ? version.Revision : 0);
    }

    private static string FormatVersion(Version version)
    {
        if (version.Revision > 0)
        {
            return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        return $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    private sealed class LatestReleaseResponse
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
