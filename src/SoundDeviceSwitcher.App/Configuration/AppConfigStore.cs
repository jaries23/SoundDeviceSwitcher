using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.UI;

namespace SoundDeviceSwitcher.App.Configuration;

public sealed class AppConfigStore
{
    private readonly LocalizationService _localizer;

    public AppConfigStore(LocalizationService localizer)
    {
        _localizer = localizer;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string ConfigDirectoryPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SoundDeviceSwitcher");

    public string ConfigFilePath => Path.Combine(ConfigDirectoryPath, "settings.json");

    public void Save(AppConfig config)
    {
        Directory.CreateDirectory(ConfigDirectoryPath);
        var json = JsonSerializer.Serialize(config, SerializerOptions);
        File.WriteAllText(ConfigFilePath, json);
    }

    public bool TryLoad(out AppConfig? config, out string? errorMessage)
    {
        config = null;
        errorMessage = null;

        if (!File.Exists(ConfigFilePath))
        {
            errorMessage = _localizer.Get("ErrorNoConfig");
            return false;
        }

        try
        {
            config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigFilePath), SerializerOptions);
        }
        catch (Exception ex)
        {
            errorMessage = _localizer.Format("ErrorCouldNotLoadConfig", ex.Message);
            return false;
        }

        if (config is null)
        {
            errorMessage = _localizer.Get("ErrorEmptyConfig");
            return false;
        }

        config.Profiles ??= [];
        config.OverlayHotkey ??= new HotkeySettings
        {
            Alt = true,
            Key = Keys.V
        };
        config.RecentSwitchUndoHotkey ??= new HotkeySettings
        {
            Control = true,
            Key = Keys.Z
        };
        config.OverlayHeightPercent = Math.Clamp(config.OverlayHeightPercent, 12, 35);
        if (!Enum.IsDefined(config.OverlayAnchor))
        {
            config.OverlayAnchor = ProfileOverlayAnchor.BottomCenter;
        }

        if (!Enum.IsDefined(config.OverlayLayoutOrientation))
        {
            config.OverlayLayoutOrientation = ProfileOverlayLayoutOrientation.Horizontal;
        }
        config.NotificationIconFileName = NotificationIconCatalog.NormalizeFileName(
            string.IsNullOrWhiteSpace(config.NotificationIconFileName)
                ? !string.IsNullOrWhiteSpace(config.PrimaryIconFileName)
                    ? config.PrimaryIconFileName
                    : config.SecondaryIconFileName
                : config.NotificationIconFileName);
        config.PrimaryIconFileName = config.NotificationIconFileName;
        config.SecondaryIconFileName = config.NotificationIconFileName;
        foreach (var profile in config.Profiles)
        {
            profile.IconFileName = NotificationIconCatalog.NormalizeFileName(
                string.IsNullOrWhiteSpace(profile.IconFileName)
                    ? AppConfig.DefaultIconFileName
                    : profile.IconFileName);
        }

        _localizer.SetLanguage(config.Language);

        if (!Validate(config, out errorMessage))
        {
            config = null;
            return false;
        }

        return true;
    }

    public bool Validate(AppConfig config, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(config.PrimaryDevice.Id) ||
            string.IsNullOrWhiteSpace(config.SecondaryDevice.Id))
        {
            errorMessage = _localizer.Get("ErrorNeedTwoPlaybackDevices");
            return false;
        }

        if (string.Equals(config.PrimaryDevice.Id, config.SecondaryDevice.Id, StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = _localizer.Get("ErrorPrimarySecondaryDifferent");
            return false;
        }

        if (!ValidateHotkey(config.Hotkey, out errorMessage) ||
            !ValidateHotkey(config.OverlayHotkey, out errorMessage) ||
            !ValidateHotkey(config.RecentSwitchUndoHotkey, out errorMessage))
        {
            return false;
        }

        return true;
    }

    private bool ValidateHotkey(HotkeySettings hotkey, out string? errorMessage)
    {
        errorMessage = null;

        if (!hotkey.Enabled)
        {
            return true;
        }

        if (!hotkey.Control &&
            !hotkey.Alt &&
            !hotkey.Shift &&
            !hotkey.WindowsKey)
        {
            errorMessage = _localizer.Get("ErrorEnableModifier");
            return false;
        }

        return true;
    }
}
