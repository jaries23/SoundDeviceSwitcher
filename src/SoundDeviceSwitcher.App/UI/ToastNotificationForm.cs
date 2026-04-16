using System.Media;
using System.Drawing.Drawing2D;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class ToastNotificationForm : Form
{
    private const int NoActivateExtendedStyle = 0x08000000;
    private readonly System.Windows.Forms.Timer _closeTimer;
    private readonly RoundedPanel _surface;
    private readonly Image? _notificationImage;
    private readonly ThemePalette _palette;
    private readonly ToolTipIcon _icon;
    private bool _displayPrepared;

    public ToastNotificationForm(
        string title,
        string message,
        ToolTipIcon icon,
        int durationMilliseconds,
        string? imagePath = null,
        AppThemeMode themeMode = AppThemeMode.System)
    {
        _palette = ThemeManager.ResolvePalette(themeMode);

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        DoubleBuffered = true;
        Size = new Size(384, 124);
        BackColor = _palette.AppBackground;
        Padding = new Padding(0);
        Opacity = 0D;
        Location = new Point(-10000, -10000);
        _icon = icon;

        _surface = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 0,
            BorderThickness = 1,
            Padding = new Padding(16, 14, 16, 14),
            Margin = new Padding(0)
        };

        _notificationImage = NotificationIconCatalog.LoadImage(imagePath);

        Controls.Add(_surface);
        _surface.Controls.Add(BuildContent(title, message, icon));
        ApplyPalette(icon);
        PrepareForDisplay();

        _closeTimer = new System.Windows.Forms.Timer
        {
            Interval = Math.Max(durationMilliseconds, 1200)
        };
        _closeTimer.Tick += (_, _) => CloseImmediately();

        Load += (_, _) => PrepareForDisplay();
        Shown += (_, _) =>
        {
            PrepareForDisplay();
            Opacity = 1D;
            TryPlaySound(_icon);
            _closeTimer.Start();
        };
    }

    protected override bool ShowWithoutActivation => true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notificationImage?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= NoActivateExtendedStyle;
            return createParams;
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRoundedRegion();

        if (!Visible)
        {
            PositionToast();
        }
    }

    public void CloseImmediately()
    {
        _closeTimer.Stop();

        if (!IsDisposed)
        {
            Close();
        }
    }

    private Control BuildContent(string title, string message, ToolTipIcon icon)
    {
        var iconColor = ResolveAccentColor(icon);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        Control iconBox = _notificationImage is null
            ? new Label
            {
                Dock = DockStyle.Fill,
                Text = ResolveGlyph(icon),
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold, GraphicsUnit.Point),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = iconColor,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 14, 0)
            }
            : new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = _notificationImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 14, 0)
            };

        var textLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = _palette.Text,
            BackColor = Color.Transparent,
            Text = title,
            Margin = new Padding(0)
        };

        var messageLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(274, 0),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = _palette.MutedText,
            BackColor = Color.Transparent,
            Text = message,
            Margin = new Padding(0, 6, 0, 0)
        };

        textLayout.Controls.Add(titleLabel, 0, 0);
        textLayout.Controls.Add(messageLabel, 0, 1);

        layout.Controls.Add(iconBox, 0, 0);
        layout.Controls.Add(textLayout, 1, 0);

        return layout;
    }

    private void ApplyPalette(ToolTipIcon icon)
    {
        _surface.FillColor = _palette.Surface;
        _surface.BorderColor = ResolveAccentColor(icon);
        _surface.BackColor = _palette.AppBackground;
    }

    private void PrepareForDisplay()
    {
        if (_displayPrepared || Width <= 0 || Height <= 0)
        {
            return;
        }

        SuspendLayout();

        try
        {
            PositionToast();
            UpdateRoundedRegion();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }

        _displayPrepared = true;
    }

    private void PositionToast()
    {
        var workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        Location = new Point(
            workingArea.Right - Width - 18,
            workingArea.Bottom - Height - 18);
    }

    private void UpdateRoundedRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using var path = CreateRoundedPath(ClientRectangle, 0);
        Region = new Region(path);
    }

    private static string ResolveGlyph(ToolTipIcon icon)
    {
        return icon switch
        {
            ToolTipIcon.Warning => "!",
            ToolTipIcon.Error => "X",
            _ => "\u2022"
        };
    }

    private Color ResolveAccentColor(ToolTipIcon icon)
    {
        return icon switch
        {
            ToolTipIcon.Warning => _palette.MutedText,
            ToolTipIcon.Error => _palette.ErrorText,
            _ => _palette.Accent
        };
    }

    private static void TryPlaySound(ToolTipIcon icon)
    {
        try
        {
            var sound = icon switch
            {
                ToolTipIcon.Warning => SystemSounds.Exclamation,
                ToolTipIcon.Error => SystemSounds.Hand,
                _ => SystemSounds.Asterisk
            };

            sound.Play();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("Toast sound playback", ex);
        }
    }

    private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
