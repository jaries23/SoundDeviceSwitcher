using System.Diagnostics;
using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Configuration;

namespace SoundDeviceSwitcher.App.Profiles;

internal sealed class ProcessProfileMatcher
{
    public ProcessProfileMatchResult FindForegroundMatch(IEnumerable<ProcessAudioProfile> profiles)
    {
        var orderedProfiles = profiles
            .Where(IsEligibleProfile)
            .ToList();

        if (orderedProfiles.Count == 0)
        {
            return ProcessProfileMatchResult.NoMatch;
        }

        var foregroundProcess = TryGetForegroundProcess();
        if (foregroundProcess is null)
        {
            return ProcessProfileMatchResult.NoMatch;
        }

        var matchedProfileIndex = orderedProfiles.FindIndex(
            profile => profile.Programs.Any(program => Matches(program, foregroundProcess)));
        if (matchedProfileIndex < 0)
        {
            return ProcessProfileMatchResult.NoMatch;
        }

        var matchedProfile = orderedProfiles[matchedProfileIndex];
        if (matchedProfileIndex == 0)
        {
            return ProcessProfileMatchResult.Match(matchedProfile, shouldMonitorBackgroundState: false);
        }

        var processNames = GetRunningProcessNames();
        var processPathCache = new RunningProcessPathCache();

        for (var index = 0; index < matchedProfileIndex; index++)
        {
            var higherPriorityProfile = orderedProfiles[index];
            if (higherPriorityProfile.Programs.Any(program => IsRunning(program, processNames, processPathCache)))
            {
                return ProcessProfileMatchResult.Blocked(matchedProfile, higherPriorityProfile);
            }
        }

        return ProcessProfileMatchResult.Match(matchedProfile, shouldMonitorBackgroundState: true);
    }

    private static bool IsEligibleProfile(ProcessAudioProfile profile)
    {
        return profile.Programs.Count > 0 &&
               (!string.IsNullOrWhiteSpace(profile.PlaybackDevice.Id) ||
                !string.IsNullOrWhiteSpace(profile.RecordingDevice.Id));
    }

    private static HashSet<string> GetRunningProcessNames()
    {
        var processNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(process.ProcessName))
                    {
                        processNames.Add($"{process.ProcessName}.exe");
                    }
                }
                catch
                {
                }
            }
        }

        return processNames;
    }

    private static bool Matches(
        ProfileProgramTarget program,
        ForegroundProcessInfo foregroundProcess)
    {
        if (!string.IsNullOrWhiteSpace(program.ExecutableName) &&
            string.Equals(program.ExecutableName, foregroundProcess.ExecutableName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(program.ExecutablePath))
        {
            var foregroundExecutablePath = foregroundProcess.GetExecutablePath();
            if (string.IsNullOrWhiteSpace(foregroundExecutablePath))
            {
                return false;
            }

            try
            {
                if (string.Equals(
                    Path.GetFullPath(program.ExecutablePath),
                    foregroundExecutablePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
            }
        }

        return false;
    }

    private static bool IsRunning(
        ProfileProgramTarget program,
        IReadOnlySet<string> processNames,
        RunningProcessPathCache processPathCache)
    {
        if (!string.IsNullOrWhiteSpace(program.ExecutableName) &&
            processNames.Contains(program.ExecutableName))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(program.ExecutablePath) &&
               processPathCache.IsRunning(program.ExecutablePath);
    }

    private static ForegroundProcessInfo? TryGetForegroundProcess()
    {
        var windowHandle = GetForegroundWindow();
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(windowHandle, out var processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            using var process = Process.GetProcessById(unchecked((int)processId));
            if (string.IsNullOrWhiteSpace(process.ProcessName))
            {
                return null;
            }

            return new ForegroundProcessInfo(
                processId: unchecked((int)processId),
                executableName: $"{process.ProcessName}.exe");
        }
        catch
        {
            return null;
        }
    }

    internal static string? TryGetExecutablePath(Process process)
    {
        try
        {
            var executablePath = process.MainModule?.FileName;
            return string.IsNullOrWhiteSpace(executablePath)
                ? null
                : Path.GetFullPath(executablePath);
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}

internal sealed class ProcessProfileMatchResult
{
    private ProcessProfileMatchResult(
        ProcessAudioProfile? matchedProfile,
        ProcessAudioProfile? blockingProfile,
        bool shouldMonitorBackgroundState)
    {
        MatchedProfile = matchedProfile;
        BlockingProfile = blockingProfile;
        ShouldMonitorBackgroundState = shouldMonitorBackgroundState;
    }

    public static ProcessProfileMatchResult NoMatch { get; } = new(null, null, shouldMonitorBackgroundState: false);

    public ProcessAudioProfile? MatchedProfile { get; }

    public ProcessAudioProfile? BlockingProfile { get; }

    public bool ShouldMonitorBackgroundState { get; }

    public static ProcessProfileMatchResult Match(ProcessAudioProfile matchedProfile, bool shouldMonitorBackgroundState)
    {
        return new ProcessProfileMatchResult(matchedProfile, null, shouldMonitorBackgroundState);
    }

    public static ProcessProfileMatchResult Blocked(ProcessAudioProfile matchedProfile, ProcessAudioProfile blockingProfile)
    {
        return new ProcessProfileMatchResult(matchedProfile, blockingProfile, shouldMonitorBackgroundState: true);
    }
}

internal sealed class ForegroundProcessInfo
{
    private readonly int _processId;
    private string? _executablePath;
    private bool _pathResolved;

    public ForegroundProcessInfo(int processId, string executableName)
    {
        _processId = processId;
        ExecutableName = executableName;
    }

    public string ExecutableName { get; }

    public string? GetExecutablePath()
    {
        if (_pathResolved)
        {
            return _executablePath;
        }

        try
        {
            using var process = Process.GetProcessById(_processId);
            _executablePath = ProcessProfileMatcher.TryGetExecutablePath(process);
        }
        catch
        {
            _executablePath = null;
        }
        finally
        {
            _pathResolved = true;
        }

        return _executablePath;
    }
}

internal sealed class RunningProcessPathCache
{
    private readonly Dictionary<string, IReadOnlySet<string>> _pathsByProcessName = new(StringComparer.OrdinalIgnoreCase);

    public bool IsRunning(string executablePath)
    {
        string normalizedPath;

        try
        {
            normalizedPath = Path.GetFullPath(executablePath);
        }
        catch
        {
            return false;
        }

        var processName = Path.GetFileNameWithoutExtension(normalizedPath);
        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        return GetPaths(processName).Contains(normalizedPath);
    }

    private IReadOnlySet<string> GetPaths(string processName)
    {
        if (_pathsByProcessName.TryGetValue(processName, out var paths))
        {
            return paths;
        }

        var discoveredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var process in Process.GetProcessesByName(processName))
        {
            using (process)
            {
                var executablePath = ProcessProfileMatcher.TryGetExecutablePath(process);
                if (!string.IsNullOrWhiteSpace(executablePath))
                {
                    discoveredPaths.Add(executablePath);
                }
            }
        }

        _pathsByProcessName[processName] = discoveredPaths;
        return discoveredPaths;
    }
}
