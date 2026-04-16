using Microsoft.Win32;
using SoundDeviceSwitcher.App.UI.Controls;

namespace SoundDeviceSwitcher.App.Theming;

internal static class ThemeManager
{
    private static readonly ThemePalette LightPalette = new(
        AppBackground: Color.FromArgb(242, 242, 242),
        Surface: Color.FromArgb(255, 255, 255),
        MutedSurface: Color.FromArgb(236, 236, 236),
        SidebarSurface: Color.FromArgb(229, 229, 229),
        AccentSurface: Color.FromArgb(232, 232, 232),
        Border: Color.FromArgb(196, 196, 196),
        Text: Color.FromArgb(32, 32, 32),
        MutedText: Color.FromArgb(102, 102, 102),
        Accent: Color.FromArgb(74, 74, 74),
        AccentText: Color.White,
        InputBackground: Color.FromArgb(247, 247, 247),
        InputText: Color.FromArgb(32, 32, 32),
        SuccessText: Color.FromArgb(74, 74, 74),
        ErrorText: Color.FromArgb(92, 92, 92));

    private static readonly ThemePalette DarkPalette = new(
        AppBackground: Color.FromArgb(18, 18, 18),
        Surface: Color.FromArgb(28, 28, 28),
        MutedSurface: Color.FromArgb(38, 38, 38),
        SidebarSurface: Color.FromArgb(14, 14, 14),
        AccentSurface: Color.FromArgb(44, 44, 44),
        Border: Color.FromArgb(72, 72, 72),
        Text: Color.FromArgb(248, 248, 248),
        MutedText: Color.FromArgb(190, 190, 190),
        Accent: Color.FromArgb(110, 110, 110),
        AccentText: Color.FromArgb(255, 255, 255),
        InputBackground: Color.FromArgb(22, 22, 22),
        InputText: Color.FromArgb(248, 248, 248),
        SuccessText: Color.FromArgb(236, 236, 236),
        ErrorText: Color.FromArgb(200, 200, 200));

    public static ThemePalette Apply(Control root, AppThemeMode mode)
    {
        var palette = ResolvePalette(mode);
        ApplyRecursive(root, palette, isRoot: true);
        return palette;
    }

    public static ThemePalette ResolvePalette(AppThemeMode mode)
    {
        var effectiveMode = mode == AppThemeMode.System ? DetectSystemTheme() : mode;
        return effectiveMode == AppThemeMode.Dark ? DarkPalette : LightPalette;
    }

    private static void ApplyRecursive(Control control, ThemePalette palette, bool isRoot = false)
    {
        switch (control)
        {
            case Form form:
                form.BackColor = palette.AppBackground;
                form.ForeColor = palette.Text;
                break;
            case RoundedPanel roundedPanel:
                roundedPanel.FillColor = ResolvePanelFill(roundedPanel, palette);
                roundedPanel.BorderColor = ResolvePanelBorder(roundedPanel, palette);
                roundedPanel.BackColor = palette.AppBackground;
                roundedPanel.ForeColor = palette.Text;
                break;
            case TableLayoutPanel tableLayoutPanel:
                tableLayoutPanel.BackColor = ResolveContainerBackColor(tableLayoutPanel, palette, isRoot);
                tableLayoutPanel.ForeColor = palette.Text;
                break;
            case FlowLayoutPanel flowLayoutPanel:
                flowLayoutPanel.BackColor = ResolveContainerBackColor(flowLayoutPanel, palette);
                flowLayoutPanel.ForeColor = palette.Text;
                break;
            case Panel panel:
                panel.BackColor = string.Equals(panel.Tag as string, "divider", StringComparison.Ordinal)
                    ? palette.Border
                    : ResolveContainerBackColor(panel, palette);
                panel.ForeColor = palette.Text;
                break;
            case Label label:
                label.BackColor = label.Parent?.BackColor ?? palette.Surface;
                label.ForeColor = palette.Text;
                break;
            case CheckBox checkBox:
                checkBox.BackColor = checkBox.Parent?.BackColor ?? palette.Surface;
                checkBox.ForeColor = palette.Text;
                break;
            case PillRadioButton pillRadioButton:
                var tag = pillRadioButton.Tag as string;
                var isNavigation = string.Equals(tag, "nav", StringComparison.Ordinal);
                var isSettingsNavigation = string.Equals(tag, "settings-nav", StringComparison.Ordinal);
                var defaultSurface = isNavigation
                    ? palette.SidebarSurface
                    : isSettingsNavigation
                        ? palette.Surface
                        : palette.MutedSurface;
                var hoverSurface = isSettingsNavigation
                    ? palette.AccentSurface
                    : palette.MutedSurface;
                pillRadioButton.BackColor = pillRadioButton.Checked
                    ? palette.Accent
                    : defaultSurface;
                pillRadioButton.ForeColor = pillRadioButton.Checked ? palette.AccentText : palette.Text;
                pillRadioButton.FlatAppearance.BorderColor = pillRadioButton.Checked
                    ? palette.Accent
                    : palette.Border;
                pillRadioButton.FlatAppearance.CheckedBackColor = palette.Accent;
                pillRadioButton.FlatAppearance.MouseDownBackColor = pillRadioButton.Checked ? palette.Accent : hoverSurface;
                pillRadioButton.FlatAppearance.MouseOverBackColor = pillRadioButton.Checked ? palette.Accent : hoverSurface;
                break;
            case RadioButton radioButton:
                radioButton.BackColor = radioButton.Parent?.BackColor ?? palette.Surface;
                radioButton.ForeColor = palette.Text;
                break;
            case ComboBox comboBox:
                comboBox.BackColor = palette.InputBackground;
                comboBox.ForeColor = palette.InputText;
                comboBox.FlatStyle = FlatStyle.Flat;
                break;
            case TextBox textBox:
                textBox.BackColor = palette.InputBackground;
                textBox.ForeColor = palette.InputText;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;
            case NumericUpDown numericUpDown:
                numericUpDown.BackColor = palette.InputBackground;
                numericUpDown.ForeColor = palette.InputText;
                numericUpDown.BorderStyle = BorderStyle.FixedSingle;
                break;
            case ListBox listBox:
                listBox.BackColor = palette.InputBackground;
                listBox.ForeColor = palette.InputText;
                listBox.BorderStyle = BorderStyle.FixedSingle;
                break;
            case PictureBox pictureBox:
                pictureBox.BackColor = palette.InputBackground;
                break;
            case RoundedButton roundedButton:
                roundedButton.FlatAppearance.BorderSize = 1;
                if (string.Equals(roundedButton.Tag as string, "primary", StringComparison.Ordinal))
                {
                    roundedButton.BackColor = palette.Accent;
                    roundedButton.ForeColor = palette.AccentText;
                    roundedButton.FlatAppearance.BorderColor = palette.Accent;
                }
                else
                {
                    roundedButton.BackColor = palette.MutedSurface;
                    roundedButton.ForeColor = palette.Text;
                    roundedButton.FlatAppearance.BorderColor = palette.Border;
                }
                break;
        }

        foreach (Control child in control.Controls)
        {
            ApplyRecursive(child, palette);
        }
    }

    private static Color ResolveContainerBackColor(Control control, ThemePalette palette, bool isRoot = false)
    {
        if (isRoot || control.Parent is null || control.Parent is Form)
        {
            return palette.AppBackground;
        }

        if (control.Parent is RoundedPanel roundedPanel)
        {
            return ResolvePanelFill(roundedPanel, palette);
        }

        return control.Parent.BackColor;
    }

    private static Color ResolvePanelFill(RoundedPanel roundedPanel, ThemePalette palette)
    {
        return (roundedPanel.Tag as string) switch
        {
            "sidebar" => palette.SidebarSurface,
            "soft" => palette.MutedSurface,
            "accent-soft" => palette.AccentSurface,
            _ => palette.Surface
        };
    }

    private static Color ResolvePanelBorder(RoundedPanel roundedPanel, ThemePalette palette)
    {
        return (roundedPanel.Tag as string) switch
        {
            "accent-soft" => palette.Accent,
            _ => palette.Border
        };
    }

    private static AppThemeMode DetectSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0
                ? AppThemeMode.Dark
                : AppThemeMode.Light;
        }
        catch
        {
            return AppThemeMode.Light;
        }
    }
}

internal sealed record ThemePalette(
    Color AppBackground,
    Color Surface,
    Color MutedSurface,
    Color SidebarSurface,
    Color AccentSurface,
    Color Border,
    Color Text,
    Color MutedText,
    Color Accent,
    Color AccentText,
    Color InputBackground,
    Color InputText,
    Color SuccessText,
    Color ErrorText);
