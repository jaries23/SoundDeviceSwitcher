using SoundDeviceSwitcher.App.Theming;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class TrayNotificationService : IDisposable
{
    private string _title;
    private AppThemeMode _themeMode = AppThemeMode.System;

    public TrayNotificationService(string title)
    {
        _title = title;
    }

    public void SetTitle(string title)
    {
        _title = title;
    }

    public void SetThemeMode(AppThemeMode themeMode)
    {
        _themeMode = themeMode;
    }

    public void Show(string message, ToolTipIcon icon, int durationMilliseconds = 2500, string? imagePath = null)
    {
        ToastNotificationManager.Show(_title, message, icon, durationMilliseconds, imagePath, _themeMode);
    }

    public void Dispose()
    {
    }

    public static void ShowTransient(
        string title,
        string message,
        ToolTipIcon icon,
        int durationMilliseconds = 2500,
        string? imagePath = null,
        AppThemeMode themeMode = AppThemeMode.System)
    {
        using var toast = new ToastNotificationForm(
            title,
            message,
            icon,
            durationMilliseconds,
            imagePath ?? NotificationIconCatalog.GetSystemImagePath(icon),
            themeMode);
        using var context = new ToastApplicationContext(toast);
        Application.Run(context);
    }

    private sealed class ToastApplicationContext : ApplicationContext
    {
        private readonly ToastNotificationForm _toast;

        public ToastApplicationContext(ToastNotificationForm toast)
        {
            _toast = toast;
            _toast.FormClosed += HandleToastClosed;
            _toast.Show();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toast.FormClosed -= HandleToastClosed;
            }

            base.Dispose(disposing);
        }

        private void HandleToastClosed(object? sender, FormClosedEventArgs e)
        {
            ExitThread();
        }
    }
}
