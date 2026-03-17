using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;

namespace SoundDeviceSwitcher.App.Shell;

public sealed class ShortcutManager
{
    private readonly LocalizationService _localizer;
    private const string StartupShortcutFileName = "SoundDeviceSwitcher.lnk";
    private const string LegacyBackgroundStartupShortcutFileName = "SoundDeviceSwitcher Background.lnk";

    public ShortcutManager(LocalizationService localizer)
    {
        _localizer = localizer;
    }

    public string StartupShortcutPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            StartupShortcutFileName);

    public string LegacyBackgroundStartupShortcutPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            LegacyBackgroundStartupShortcutFileName);

    public void SyncStartupShortcut(AppConfig config)
    {
        DeleteShortcut(LegacyBackgroundStartupShortcutPath);

        if (config.StartWithWindows)
        {
            CreateShortcut(
                StartupShortcutPath,
                Application.ExecutablePath,
                "--startup",
                _localizer.Get("ShortcutStartupDescription"));
            return;
        }

        DeleteShortcut(StartupShortcutPath);
    }

    public void CreateToggleShortcut(string path)
    {
        CreateShortcut(
            path,
            Application.ExecutablePath,
            "--toggle",
            _localizer.Get("ShortcutToggleDescription"));
    }

    public void CreateShortcut(string path, string targetPath, string arguments, string description)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows Script Host is not available on this machine.");

        object? shell = null;
        object? shortcut = null;

        try
        {
            shell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("Could not create the Windows Script Host shell object.");

            dynamic dynamicShell = shell;
            shortcut = dynamicShell.CreateShortcut(path);

            dynamic dynamicShortcut = shortcut;
            dynamicShortcut.TargetPath = targetPath;
            dynamicShortcut.Arguments = arguments;
            dynamicShortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            dynamicShortcut.Description = description;
            dynamicShortcut.Save();
        }
        finally
        {
            if (shortcut is not null && Marshal.IsComObject(shortcut))
            {
                Marshal.FinalReleaseComObject(shortcut);
            }

            if (shell is not null && Marshal.IsComObject(shell))
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }

    private static void DeleteShortcut(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
