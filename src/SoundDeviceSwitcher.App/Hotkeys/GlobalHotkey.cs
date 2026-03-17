using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Configuration;

namespace SoundDeviceSwitcher.App.Hotkeys;

[Flags]
internal enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
    NoRepeat = 0x4000
}

internal sealed class GlobalHotkey : IDisposable
{
    private const int HotkeyId = 0x5344;
    private readonly HotkeyMessageWindow _window = new();
    private bool _registered;

    public GlobalHotkey()
    {
        _window.HotkeyPressed += (_, _) => Pressed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Pressed;

    public bool Register(HotkeySettings hotkeySettings, out string? errorMessage)
    {
        errorMessage = null;

        if (!hotkeySettings.Enabled)
        {
            Unregister();
            return true;
        }

        Unregister();

        var modifiers = HotkeyModifiers.NoRepeat;
        if (hotkeySettings.Control)
        {
            modifiers |= HotkeyModifiers.Control;
        }

        if (hotkeySettings.Alt)
        {
            modifiers |= HotkeyModifiers.Alt;
        }

        if (hotkeySettings.Shift)
        {
            modifiers |= HotkeyModifiers.Shift;
        }

        if (hotkeySettings.WindowsKey)
        {
            modifiers |= HotkeyModifiers.Windows;
        }

        _registered = RegisterHotKey(_window.Handle, HotkeyId, (uint)modifiers, (uint)hotkeySettings.Key);
        if (_registered)
        {
            return true;
        }

        errorMessage = null;
        return false;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_window.Handle, HotkeyId);
        _registered = false;
    }

    public void Dispose()
    {
        Unregister();
        _window.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr windowHandle, int id);
}

internal sealed class HotkeyMessageWindow : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;

    public HotkeyMessageWindow()
    {
        CreateHandle(new CreateParams());
    }

    public event EventHandler? HotkeyPressed;

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == WmHotkey)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref message);
    }

    public void Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            DestroyHandle();
        }
    }
}
