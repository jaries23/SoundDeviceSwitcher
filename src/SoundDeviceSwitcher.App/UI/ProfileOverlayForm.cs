using System.Drawing.Drawing2D;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class ProfileOverlayForm : Form
{
    private const int OverlayScreenMargin = 28;
    private const int OverlaySurfacePadding = 20;
    private const int OverlayButtonGap = 14;
    private const int BaseButtonWidth = 190;
    private const int BaseButtonHeight = 144;
    private const int MaxButtonWidth = 220;
    private const int MaxButtonHeight = 160;
    private const int MinButtonWidth = 24;
    private const int MinButtonHeight = 18;
    private readonly ThemePalette _palette;
    private readonly LocalizationService _localizer;
    private readonly RoundedPanel _surface;
    private readonly TableLayoutPanel _profileGrid;
    private readonly IReadOnlyList<ProcessAudioProfile> _profiles;
    private readonly List<RoundedButton> _profileButtons = [];
    private readonly Dictionary<RoundedButton, Image> _profileButtonImages = [];
    private readonly int _overlayHeightPercent;
    private readonly ProfileOverlayAnchor _overlayAnchor;
    private readonly ProfileOverlayLayoutOrientation _overlayLayoutOrientation;

    public ProfileOverlayForm(
        IReadOnlyList<ProcessAudioProfile> profiles,
        LocalizationService localizer,
        AppThemeMode themeMode,
        int overlayHeightPercent,
        ProfileOverlayAnchor overlayAnchor,
        ProfileOverlayLayoutOrientation overlayLayoutOrientation)
    {
        _profiles = profiles;
        _localizer = localizer;
        _palette = ThemeManager.ResolvePalette(themeMode);
        _overlayHeightPercent = Math.Clamp(overlayHeightPercent, 12, 35);
        _overlayAnchor = overlayAnchor;
        _overlayLayoutOrientation = overlayLayoutOrientation;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        KeyPreview = true;
        Padding = new Padding(0);
        BackColor = _palette.AppBackground;
        Opacity = 0.97D;

        _surface = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 0,
            BorderThickness = 1,
            BorderColor = _palette.Border,
            FillColor = _palette.AccentSurface,
            Padding = new Padding(OverlaySurfacePadding),
            Margin = new Padding(0)
        };

        _profileGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Margin = new Padding(0),
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };

        foreach (var profile in _profiles)
        {
            var button = CreateProfileButton(profile, _profileButtons.Count);
            _profileButtons.Add(button);
        }

        _surface.Controls.Add(_profileGrid);
        Controls.Add(_surface);

        Load += (_, _) => ApplyOverlayBounds();
        Shown += (_, _) =>
        {
            if (_profileButtons.Count > 0)
            {
                _profileButtons[0].Focus();
            }
        };
        KeyDown += HandleOverlayKeyDown;
        Deactivate += (_, _) => Close();
    }

    public event EventHandler<ProfileOverlaySelectionEventArgs>? ProfileSelected;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var image in _profileButtonImages.Values)
            {
                image.Dispose();
            }

            _profileButtonImages.Clear();
        }

        base.Dispose(disposing);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRoundedRegion();
    }

    private RoundedButton CreateProfileButton(ProcessAudioProfile profile, int index)
    {
        var button = new RoundedButton
        {
            AutoSize = false,
            Size = new Size(BaseButtonWidth, BaseButtonHeight),
            Margin = new Padding(0, 0, OverlayButtonGap, OverlayButtonGap),
            Padding = new Padding(16, 16, 16, 14),
            CornerRadius = 0,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point),
            TextAlign = ContentAlignment.BottomCenter,
            TextImageRelation = TextImageRelation.ImageAboveText,
            ImageAlign = ContentAlignment.MiddleCenter,
            BackColor = _palette.Surface,
            ForeColor = _palette.Text,
            Tag = profile
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = _palette.Border;
        button.FlatAppearance.MouseDownBackColor = _palette.MutedSurface;
        button.FlatAppearance.MouseOverBackColor = _palette.MutedSurface;
        button.Text = GetProfileName(profile);
        button.Click += (_, _) => SelectProfile(profile);
        return button;
    }

    private string GetProfileName(ProcessAudioProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? _localizer.Get("ProfileUnnamed")
            : profile.Name;
    }

    private Image? CreateProfileButtonImage(string? iconFileName, int size)
    {
        var path = NotificationIconCatalog.ResolvePath(
            string.IsNullOrWhiteSpace(iconFileName)
                ? AppConfig.DefaultIconFileName
                : NotificationIconCatalog.NormalizeFileName(iconFileName))
            ?? NotificationIconCatalog.ResolvePath(AppConfig.DefaultIconFileName);
        using var source = NotificationIconCatalog.LoadImage(path);
        return source is null ? null : ResizeImage(source, size, size);
    }

    private void ApplyOverlayBounds()
    {
        var workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
        var layout = CalculateGridLayout(workingArea);

        ApplyGridLayout(layout);

        Size = new Size(layout.OuterWidth, layout.OuterHeight);
        Location = ResolveOverlayLocation(workingArea, Size, _overlayAnchor);

        UpdateRoundedRegion();
    }

    private OverlayGridLayout CalculateGridLayout(Rectangle workingArea)
    {
        var count = Math.Max(1, _profileButtons.Count);
        var maxOuterWidth = Math.Max(260, workingArea.Width - (OverlayScreenMargin * 2));
        var maxOuterHeight = Math.Max(220, workingArea.Height - (OverlayScreenMargin * 2));
        var maxContentWidth = Math.Max(120, maxOuterWidth - _surface.Padding.Horizontal);
        var maxContentHeight = Math.Max(120, maxOuterHeight - _surface.Padding.Vertical);
        var preferredContentHeight = Math.Clamp(
            (int)Math.Round(workingArea.Height * (_overlayHeightPercent / 100D)) - _surface.Padding.Vertical,
            160,
            maxContentHeight);

        return _overlayLayoutOrientation == ProfileOverlayLayoutOrientation.Vertical
            ? BuildVerticalLayout(count, maxContentWidth, maxContentHeight, preferredContentHeight)
            : BuildHorizontalLayout(count, maxContentWidth, maxContentHeight, preferredContentHeight);
    }

    private OverlayGridLayout BuildVerticalLayout(int count, int maxContentWidth, int maxContentHeight, int preferredContentHeight)
    {
        var requiredContentHeight = (count * BaseButtonHeight) + (Math.Max(0, count - 1) * OverlayButtonGap);
        var allowedContentHeight = Math.Min(maxContentHeight, Math.Max(preferredContentHeight, requiredContentHeight));
        var scale = Math.Min(
            1D,
            Math.Min(
                maxContentWidth / (double)BaseButtonWidth,
                allowedContentHeight / (double)Math.Max(1, requiredContentHeight)));

        return CreateLinearLayout(count, isVerticalLayout: true, scale, maxContentWidth, maxContentHeight);
    }

    private OverlayGridLayout BuildHorizontalLayout(int count, int maxContentWidth, int maxContentHeight, int preferredContentHeight)
    {
        var requiredContentWidth = (count * BaseButtonWidth) + (Math.Max(0, count - 1) * OverlayButtonGap);
        var allowedContentHeight = Math.Min(maxContentHeight, Math.Max(preferredContentHeight, BaseButtonHeight));
        var scale = Math.Min(
            1D,
            Math.Min(
                maxContentWidth / (double)Math.Max(1, requiredContentWidth),
                allowedContentHeight / (double)BaseButtonHeight));

        return CreateLinearLayout(count, isVerticalLayout: false, scale, maxContentWidth, maxContentHeight);
    }

    private OverlayGridLayout CreateLinearLayout(int count, bool isVerticalLayout, double scale, int maxContentWidth, int maxContentHeight)
    {
        var buttonWidth = Math.Clamp((int)Math.Floor(BaseButtonWidth * scale), MinButtonWidth, MaxButtonWidth);
        var buttonHeight = Math.Clamp((int)Math.Floor(BaseButtonHeight * scale), MinButtonHeight, MaxButtonHeight);
        var gaps = Math.Max(0, count - 1) * OverlayButtonGap;

        if (isVerticalLayout)
        {
            var maxButtonHeight = Math.Max(MinButtonHeight, (maxContentHeight - gaps) / Math.Max(1, count));
            buttonHeight = Math.Min(buttonHeight, maxButtonHeight);
            buttonWidth = Math.Min(maxContentWidth, Math.Clamp(
                (int)Math.Floor(buttonHeight * (BaseButtonWidth / (double)BaseButtonHeight)),
                MinButtonWidth,
                MaxButtonWidth));
        }
        else
        {
            var maxButtonWidth = Math.Max(MinButtonWidth, (maxContentWidth - gaps) / Math.Max(1, count));
            buttonWidth = Math.Min(buttonWidth, maxButtonWidth);
            buttonHeight = Math.Min(maxContentHeight, Math.Clamp(
                (int)Math.Floor(buttonWidth * (BaseButtonHeight / (double)BaseButtonWidth)),
                MinButtonHeight,
                MaxButtonHeight));
        }

        var columns = isVerticalLayout ? 1 : count;
        var rows = isVerticalLayout ? count : 1;
        var contentWidth = isVerticalLayout
            ? buttonWidth
            : (count * buttonWidth) + gaps;
        var contentHeight = isVerticalLayout
            ? (count * buttonHeight) + gaps
            : buttonHeight;

        return new OverlayGridLayout(
            columns,
            rows,
            buttonWidth,
            buttonHeight,
            contentWidth,
            contentHeight,
            contentWidth + _surface.Padding.Horizontal,
            contentHeight + _surface.Padding.Vertical);
    }

    private void ApplyGridLayout(OverlayGridLayout layout)
    {
        _profileGrid.SuspendLayout();

        try
        {
            _profileGrid.Controls.Clear();
            _profileGrid.ColumnStyles.Clear();
            _profileGrid.RowStyles.Clear();
            _profileGrid.ColumnCount = layout.Columns;
            _profileGrid.RowCount = layout.Rows;

            for (var column = 0; column < layout.Columns; column++)
            {
                var width = layout.ButtonWidth;
                if (_overlayLayoutOrientation == ProfileOverlayLayoutOrientation.Horizontal && column < layout.Columns - 1)
                {
                    width += OverlayButtonGap;
                }

                _profileGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, width));
            }

            for (var row = 0; row < layout.Rows; row++)
            {
                var height = layout.ButtonHeight;
                if (_overlayLayoutOrientation == ProfileOverlayLayoutOrientation.Vertical && row < layout.Rows - 1)
                {
                    height += OverlayButtonGap;
                }

                _profileGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            }

            for (var index = 0; index < _profileButtons.Count; index++)
            {
                var button = _profileButtons[index];
                button.Width = layout.ButtonWidth;
                button.Height = layout.ButtonHeight;
                button.Dock = DockStyle.Fill;
                button.Margin = Padding.Empty;
                button.Padding = new Padding(
                    Math.Max(5, layout.ButtonWidth / 12),
                    Math.Max(5, layout.ButtonHeight / 12),
                    Math.Max(5, layout.ButtonWidth / 12),
                    Math.Max(5, layout.ButtonHeight / 12));

                var profile = (ProcessAudioProfile)button.Tag!;
                var iconSize = Math.Clamp(Math.Min(layout.ButtonWidth - 14, layout.ButtonHeight - 32), 12, 78);
                UpdateProfileButtonImage(button, profile.IconFileName, iconSize);

                var (column, row) = ResolveGridPosition(index, layout);
                _profileGrid.Controls.Add(button, column, row);
            }
        }
        finally
        {
            _profileGrid.ResumeLayout(performLayout: true);
        }
    }

    private (int Column, int Row) ResolveGridPosition(int index, OverlayGridLayout layout)
    {
        return _overlayLayoutOrientation == ProfileOverlayLayoutOrientation.Vertical
            ? (0, index)
            : (index, 0);
    }

    private void UpdateProfileButtonImage(RoundedButton button, string? iconFileName, int iconSize)
    {
        if (_profileButtonImages.TryGetValue(button, out var existingImage))
        {
            existingImage.Dispose();
            _profileButtonImages.Remove(button);
        }

        var image = CreateProfileButtonImage(iconFileName, iconSize);
        button.Image = image;
        if (image is not null)
        {
            _profileButtonImages[button] = image;
        }
    }

    private static Point ResolveOverlayLocation(Rectangle workingArea, Size overlaySize, ProfileOverlayAnchor anchor)
    {
        var left = workingArea.Left + OverlayScreenMargin;
        var centerX = workingArea.Left + ((workingArea.Width - overlaySize.Width) / 2);
        var right = workingArea.Right - overlaySize.Width - OverlayScreenMargin;
        var top = workingArea.Top + OverlayScreenMargin;
        var middleY = workingArea.Top + ((workingArea.Height - overlaySize.Height) / 2);
        var bottom = workingArea.Bottom - overlaySize.Height - OverlayScreenMargin;

        var x = anchor switch
        {
            ProfileOverlayAnchor.TopLeft or ProfileOverlayAnchor.MiddleLeft or ProfileOverlayAnchor.BottomLeft => left,
            ProfileOverlayAnchor.TopCenter or ProfileOverlayAnchor.Center or ProfileOverlayAnchor.BottomCenter => centerX,
            _ => right
        };

        var y = anchor switch
        {
            ProfileOverlayAnchor.TopLeft or ProfileOverlayAnchor.TopCenter or ProfileOverlayAnchor.TopRight => top,
            ProfileOverlayAnchor.MiddleLeft or ProfileOverlayAnchor.Center or ProfileOverlayAnchor.MiddleRight => middleY,
            _ => bottom
        };

        return new Point(Math.Max(workingArea.Left, x), Math.Max(workingArea.Top, y));
    }

    private void HandleOverlayKeyDown(object? sender, KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyCode == Keys.Escape)
        {
            Close();
            return;
        }

        var number = eventArgs.KeyCode switch
        {
            >= Keys.D1 and <= Keys.D9 => eventArgs.KeyCode - Keys.D1,
            >= Keys.NumPad1 and <= Keys.NumPad9 => eventArgs.KeyCode - Keys.NumPad1,
            _ => -1
        };
        if (number >= 0 && number < _profiles.Count)
        {
            SelectProfile(_profiles[number]);
        }
    }

    private void SelectProfile(ProcessAudioProfile profile)
    {
        ProfileSelected?.Invoke(this, new ProfileOverlaySelectionEventArgs(profile));
        Close();
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

    private static Bitmap ResizeImage(Image source, int width, int height)
    {
        var bitmap = new Bitmap(width, height);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var scale = Math.Min((width - 4D) / source.Width, (height - 4D) / source.Height);
        var drawWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
        var drawHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
        var x = (width - drawWidth) / 2;
        var y = (height - drawHeight) / 2;

        graphics.DrawImage(source, new Rectangle(x, y, drawWidth, drawHeight));
        return bitmap;
    }
}

internal readonly record struct OverlayGridLayout(
    int Columns,
    int Rows,
    int ButtonWidth,
    int ButtonHeight,
    int ContentWidth,
    int ContentHeight,
    int OuterWidth,
    int OuterHeight);

internal sealed class ProfileOverlaySelectionEventArgs : EventArgs
{
    public ProfileOverlaySelectionEventArgs(ProcessAudioProfile profile)
    {
        Profile = profile;
    }

    public ProcessAudioProfile Profile { get; }
}
