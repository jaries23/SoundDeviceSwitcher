using System.Windows.Forms;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Theming;

namespace SoundDeviceSwitcher.App.Configuration;

public sealed class AppConfig
{
    public const string DefaultIconFileName = "default.ico";

    public AppLanguage Language { get; set; } = AppLanguage.English;

    public AppThemeMode Theme { get; set; } = AppThemeMode.System;

    public bool StartWithWindows { get; set; }

    public bool StartMinimizedAtStartup { get; set; }

    public bool MinimizeToTrayOnClose { get; set; }

    public DeviceSelection PrimaryDevice { get; set; } = new();

    public DeviceSelection SecondaryDevice { get; set; } = new();

    public string PrimaryIconFileName { get; set; } = DefaultIconFileName;

    public string SecondaryIconFileName { get; set; } = DefaultIconFileName;

    public HotkeySettings Hotkey { get; set; } = new();
}

public sealed class DeviceSelection
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class HotkeySettings
{
    public bool Enabled { get; set; }

    public bool Control { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public bool WindowsKey { get; set; }

    public Keys Key { get; set; } = Keys.F10;
}
