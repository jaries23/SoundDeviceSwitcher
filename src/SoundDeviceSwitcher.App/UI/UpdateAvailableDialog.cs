using System.Diagnostics;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class UpdateAvailableDialog : Form
{
    private readonly LocalizationService _localizer;
    private readonly UpdateReleaseInfo _releaseInfo;
    private readonly ThemePalette _palette;
    private readonly Icon _windowIcon;
    private readonly RoundedPanel _surface;
    private readonly Label _warningGlyphLabel;
    private readonly Label _titleLabel;
    private readonly Label _messageLabel;
    private readonly Label _currentVersionLabel;
    private readonly Label _latestVersionLabel;
    private readonly Label _publishedAtLabel;
    private readonly RoundedButton _openButton;
    private readonly RoundedButton _laterButton;

    public UpdateAvailableDialog(
        LocalizationService localizer,
        UpdateReleaseInfo releaseInfo,
        string currentVersionDisplay,
        AppThemeMode themeMode)
    {
        _localizer = localizer;
        _releaseInfo = releaseInfo;
        _palette = ThemeManager.ResolvePalette(themeMode);

        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(448, 248);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        Text = _localizer.Get("UpdateDialogWindowTitle");
        _windowIcon = NotificationIconCatalog.CreateTrayIcon();
        Icon = _windowIcon;
        ShowIcon = true;

        _surface = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 0,
            BorderThickness = 1,
            Padding = new Padding(18),
            Margin = new Padding(0)
        };

        _warningGlyphLabel = new Label
        {
            AutoSize = false,
            Size = new Size(52, 52),
            Font = new Font("Segoe UI Semibold", 26F, FontStyle.Bold, GraphicsUnit.Point),
            Text = "!",
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0)
        };
        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(0, 2, 0, 0),
            Text = _localizer.Get("UpdateDialogTitle")
        };
        _messageLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 42,
            Margin = new Padding(0, 14, 0, 0),
            Text = _localizer.Format("UpdateDialogMessage", releaseInfo.VersionDisplay)
        };
        _currentVersionLabel = CreateDetailLabel(_localizer.Format("UpdateDialogCurrentVersion", currentVersionDisplay));
        _latestVersionLabel = CreateDetailLabel(_localizer.Format("UpdateDialogLatestVersion", releaseInfo.VersionDisplay));
        _publishedAtLabel = CreateDetailLabel(
            releaseInfo.PublishedAt is null
                ? _localizer.Get("UpdateDialogPublishedUnknown")
                : _localizer.Format("UpdateDialogPublishedAt", releaseInfo.PublishedAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")));

        _openButton = CreateActionButton("primary", new Size(156, 38));
        _openButton.Text = _localizer.Get("UpdateDialogOpenButton");
        _openButton.Click += (_, _) => OpenReleasePage();

        _laterButton = CreateActionButton("secondary", new Size(110, 38));
        _laterButton.Text = _localizer.Get("UpdateDialogLaterButton");
        _laterButton.Click += (_, _) => Close();

        _surface.Controls.Add(BuildContent());
        Controls.Add(_surface);

        ApplyTheme();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _windowIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildContent()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.Controls.Add(_warningGlyphLabel, 0, 0);
        header.Controls.Add(_titleLabel, 1, 0);

        var detailLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        detailLayout.Controls.Add(_messageLabel, 0, 0);
        detailLayout.Controls.Add(_currentVersionLabel, 0, 1);
        detailLayout.Controls.Add(_latestVersionLabel, 0, 2);
        detailLayout.Controls.Add(_publishedAtLabel, 0, 3);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 16, 0, 0),
            BackColor = Color.Transparent
        };
        buttonRow.Controls.Add(_laterButton);
        buttonRow.Controls.Add(_openButton);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(detailLayout, 0, 1);
        root.Controls.Add(buttonRow, 0, 2);
        return root;
    }

    private void ApplyTheme()
    {
        BackColor = _palette.AppBackground;
        _surface.FillColor = _palette.Surface;
        _surface.BorderColor = _palette.Accent;
        _surface.BackColor = _palette.AppBackground;
        _warningGlyphLabel.ForeColor = _palette.ErrorText;
        _warningGlyphLabel.BackColor = _palette.InputBackground;
        _titleLabel.ForeColor = _palette.Text;
        _messageLabel.ForeColor = _palette.Text;
        _currentVersionLabel.ForeColor = _palette.MutedText;
        _latestVersionLabel.ForeColor = _palette.MutedText;
        _publishedAtLabel.ForeColor = _palette.MutedText;

        ApplyButtonTheme(_openButton, isPrimary: true);
        ApplyButtonTheme(_laterButton, isPrimary: false);
    }

    private static Label CreateDetailLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 8, 0, 0),
            Text = text
        };
    }

    private static RoundedButton CreateActionButton(string role, Size minimumSize)
    {
        return new RoundedButton
        {
            Tag = role,
            CornerRadius = 0,
            MinimumSize = minimumSize,
            Margin = new Padding(8, 0, 0, 0),
            Padding = new Padding(14, 8, 14, 8),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private void ApplyButtonTheme(RoundedButton button, bool isPrimary)
    {
        button.FlatAppearance.BorderSize = 1;
        if (isPrimary)
        {
            button.BackColor = _palette.Accent;
            button.ForeColor = _palette.AccentText;
            button.FlatAppearance.BorderColor = _palette.Accent;
            return;
        }

        button.BackColor = _palette.MutedSurface;
        button.ForeColor = _palette.Text;
        button.FlatAppearance.BorderColor = _palette.Border;
    }

    private void OpenReleasePage()
    {
        try
        {
            Process.Start(new ProcessStartInfo(_releaseInfo.HtmlUrl)
            {
                UseShellExecute = true
            });

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, _localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
