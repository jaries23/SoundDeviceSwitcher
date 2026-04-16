using System.Drawing;
using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;

namespace SoundDeviceSwitcher.App.UI;

internal static class NotificationIconCatalog
{
    private static readonly HashSet<string> LegacyBuiltInFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "default.png",
        "icon_speaker.png",
        "icon_earphone.png",
        "icon_warning.png"
    };

    private static readonly string[] SupportedExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif",
        ".ico"
    ];

    public static string BuiltInIconDirectoryPath =>
        Path.Combine(AppContext.BaseDirectory, "assets", "icons");

    public static string UserIconDirectoryPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SoundDeviceSwitcher",
            "assets",
            "icons");

    public static IReadOnlyList<IconChoice> GetSelectableIcons(LocalizationService localizer)
    {
        var availableIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        EnsureUserIconFolderInitialized();
        AddIconsFromDirectory(availableIcons, BuiltInIconDirectoryPath);
        AddIconsFromDirectory(availableIcons, UserIconDirectoryPath);

        return availableIcons
            .Select(pair => pair.Value)
            .OrderBy(GetSortOrder)
            .ThenBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
            .Select(path => new IconChoice(
                Path.GetFileName(path),
                GetDisplayName(Path.GetFileName(path), localizer),
                path))
            .ToList();
    }

    public static string? GetWarningIconPath()
    {
        return ResolvePath(AppConfig.DefaultIconFileName);
    }

    public static string? GetSystemImagePath(ToolTipIcon icon)
    {
        return icon switch
        {
            ToolTipIcon.Warning or ToolTipIcon.Error => GetWarningIconPath(),
            _ => null
        };
    }

    public static string? ResolvePath(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var safeFileName = NormalizeFileName(fileName);
        var userPath = Path.Combine(UserIconDirectoryPath, safeFileName);
        if (File.Exists(userPath))
        {
            return userPath;
        }

        var builtInPath = Path.Combine(BuiltInIconDirectoryPath, safeFileName);
        return File.Exists(builtInPath) ? builtInPath : null;
    }

    public static string? ResolveToggleImagePath(AppConfig config, ToggleResult result)
    {
        if (!result.Success)
        {
            return GetWarningIconPath();
        }

        var iconFileName = string.IsNullOrWhiteSpace(config.NotificationIconFileName)
            ? !string.IsNullOrWhiteSpace(config.PrimaryIconFileName)
                ? config.PrimaryIconFileName
                : config.SecondaryIconFileName
            : config.NotificationIconFileName;
        return ResolvePath(iconFileName);
    }

    public static Image? LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            if (string.Equals(Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase))
            {
                using var icon = new Icon(path);
                return icon.ToBitmap();
            }

            using var stream = File.OpenRead(path);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
        catch
        {
            return null;
        }
    }

    public static Icon CreateTrayIcon(string? fileName = null)
    {
        var path = ResolvePath(fileName ?? AppConfig.DefaultIconFileName);
        var icon = LoadTrayIconFromPath(path);
        return icon ?? (Icon)SystemIcons.Application.Clone();
    }

    private static int GetSortOrder(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.ToLowerInvariant() switch
        {
            AppConfig.DefaultIconFileName => 0,
            _ => 1
        };
    }

    private static void AddIconsFromDirectory(IDictionary<string, string> destination, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(directoryPath))
        {
            if (!SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var fileName = NormalizeFileName(Path.GetFileName(path));
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            destination[fileName] = path;
        }
    }

    private static string GetDisplayName(string fileName, LocalizationService localizer)
    {
        return fileName.ToLowerInvariant() switch
        {
            AppConfig.DefaultIconFileName => localizer.Get("IconDefault"),
            _ => Path.GetFileNameWithoutExtension(fileName)
        };
    }

    public static string NormalizeFileName(string? fileName)
    {
        var safeFileName = Path.GetFileName(fileName ?? string.Empty);
        return LegacyBuiltInFileNames.Contains(safeFileName)
            ? AppConfig.DefaultIconFileName
            : safeFileName;
    }

    public static void EnsureUserIconFolderInitialized()
    {
        Directory.CreateDirectory(UserIconDirectoryPath);

        var legacyDefaultPath = Path.Combine(UserIconDirectoryPath, "default.png");
        var userDefaultPath = Path.Combine(UserIconDirectoryPath, AppConfig.DefaultIconFileName);
        if (File.Exists(legacyDefaultPath) && !File.Exists(userDefaultPath))
        {
            File.Move(legacyDefaultPath, userDefaultPath);
        }
        else if (File.Exists(legacyDefaultPath))
        {
            File.Delete(legacyDefaultPath);
        }

        if (File.Exists(userDefaultPath))
        {
            return;
        }

        var builtInDefaultPath = Path.Combine(BuiltInIconDirectoryPath, AppConfig.DefaultIconFileName);
        if (File.Exists(builtInDefaultPath))
        {
            File.Copy(builtInDefaultPath, userDefaultPath, overwrite: false);
        }
    }

    private static Icon? LoadTrayIconFromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        try
        {
            if (string.Equals(Path.GetExtension(path), ".ico", StringComparison.OrdinalIgnoreCase))
            {
                using var fileIcon = new Icon(path);
                return (Icon)fileIcon.Clone();
            }

            using var image = LoadImage(path);
            if (image is null)
            {
                return null;
            }

            using var bitmap = new Bitmap(image, new Size(32, 32));
            var iconHandle = bitmap.GetHicon();

            try
            {
                using var handleIcon = Icon.FromHandle(iconHandle);
                return (Icon)handleIcon.Clone();
            }
            finally
            {
                DestroyIcon(iconHandle);
            }
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    internal sealed class IconChoice
    {
        public IconChoice(string fileName, string label, string fullPath)
        {
            FileName = fileName;
            Label = label;
            FullPath = fullPath;
        }

        public string FileName { get; }

        public string Label { get; }

        public string FullPath { get; }
    }
}
