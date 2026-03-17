using SoundDeviceSwitcher.App.Configuration;

namespace SoundDeviceSwitcher.App.Hotkeys;

internal static class HotkeyFormatter
{
    public static string Format(HotkeySettings hotkeySettings)
    {
        var parts = new List<string>();

        if (hotkeySettings.Control)
        {
            parts.Add("Ctrl");
        }

        if (hotkeySettings.Alt)
        {
            parts.Add("Alt");
        }

        if (hotkeySettings.Shift)
        {
            parts.Add("Shift");
        }

        if (hotkeySettings.WindowsKey)
        {
            parts.Add("Win");
        }

        parts.Add(FormatKey(hotkeySettings.Key));
        return string.Join(" + ", parts);
    }

    public static string FormatKey(Keys key)
    {
        if (key is >= Keys.D0 and <= Keys.D9)
        {
            return ((char)('0' + (key - Keys.D0))).ToString();
        }

        return key.ToString();
    }
}
