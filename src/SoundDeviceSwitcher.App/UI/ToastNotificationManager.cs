using SoundDeviceSwitcher.App.Theming;

namespace SoundDeviceSwitcher.App.UI;

internal static class ToastNotificationManager
{
    private static readonly object SyncRoot = new();
    private static ToastNotificationForm? _currentToast;

    public static void Show(
        string title,
        string message,
        ToolTipIcon icon,
        int durationMilliseconds = 2500,
        string? imagePath = null,
        AppThemeMode themeMode = AppThemeMode.System)
    {
        lock (SyncRoot)
        {
            ReplaceCurrentToast();

            _currentToast = new ToastNotificationForm(
                title,
                message,
                icon,
                durationMilliseconds,
                imagePath ?? NotificationIconCatalog.GetSystemImagePath(icon),
                themeMode);
            _currentToast.FormClosed += HandleToastClosed;
            _currentToast.Show();
        }
    }

    private static void ReplaceCurrentToast()
    {
        if (_currentToast is null)
        {
            return;
        }

        var toast = _currentToast;
        _currentToast = null;
        toast.FormClosed -= HandleToastClosed;

        if (!toast.IsDisposed)
        {
            toast.CloseImmediately();
            toast.Dispose();
        }
    }

    private static void HandleToastClosed(object? sender, FormClosedEventArgs e)
    {
        lock (SyncRoot)
        {
            if (ReferenceEquals(_currentToast, sender))
            {
                _currentToast = null;
            }
        }
    }
}
