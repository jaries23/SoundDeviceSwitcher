using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Hotkeys;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Profiles;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App.UI;

internal sealed partial class MainShellForm
{
    private static RoundedPanel CreateCard(string? tag = null, bool autoSize = true, DockStyle dock = DockStyle.Top, int cornerRadius = 0)
    {
        return new RoundedPanel
        {
            Tag = tag,
            AutoSize = autoSize,
            Dock = dock,
            CornerRadius = cornerRadius,
            BorderThickness = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
    }

    private static PillRadioButton CreateNavButton()
    {
        return new PillRadioButton
        {
            Tag = "nav",
            CornerRadius = 0,
            Dock = DockStyle.Fill,
            Height = 44,
            MinimumSize = new Size(0, 44),
            Margin = new Padding(0, 0, 0, 10),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 14, 0),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static PillRadioButton CreateSettingsTabButton()
    {
        return new PillRadioButton
        {
            Tag = "settings-nav",
            CornerRadius = 0,
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 42,
            MinimumSize = new Size(0, 42),
            Margin = new Padding(0, 0, 8, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(8, 0, 8, 0),
            Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static PillRadioButton CreateThemeRadioButton()
    {
        return new PillRadioButton
        {
            CornerRadius = 0,
            Size = new Size(84, 34),
            Margin = new Padding(0, 0, 8, 0),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static RadioButton CreateOverlayLayoutRadioButton()
    {
        return new RadioButton
        {
            AutoSize = true,
            Margin = new Padding(0, 2, 18, 2),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            UseVisualStyleBackColor = true
        };
    }

    private static RadioButton CreateOverlayAnchorButton()
    {
        var button = new RadioButton
        {
            AutoSize = false,
            Size = new Size(22, 22),
            Margin = new Padding(6),
            Anchor = AnchorStyles.None,
            CheckAlign = ContentAlignment.MiddleCenter,
            UseVisualStyleBackColor = true
        };
        button.Text = string.Empty;
        button.Padding = Padding.Empty;
        return button;
    }

    private static RoundedButton CreateActionButton(string role, Size minimumSize)
    {
        return new RoundedButton
        {
            Tag = role,
            CornerRadius = 0,
            MinimumSize = minimumSize,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(14, 8, 14, 8),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static RoundedButton CreateIconActionButton(string glyph)
    {
        var button = CreateActionButton("secondary", new Size(40, 40));
        button.AutoSize = false;
        button.Size = new Size(40, 40);
        button.MinimumSize = new Size(40, 40);
        button.Padding = new Padding(0);
        button.Margin = new Padding(0);
        button.Text = glyph;
        button.Font = new Font("Segoe UI Symbol", 11F, FontStyle.Bold, GraphicsUnit.Point);
        return button;
    }

    private static ComboBox CreateDeviceComboBox()
    {
        return new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            DisplayMember = nameof(AudioDeviceInfo.DisplayName),
            ValueMember = nameof(AudioDeviceInfo.Id),
            Margin = new Padding(0, 4, 0, 4)
        };
    }

    private static ComboBox CreateProfileDeviceComboBox()
    {
        return new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            DisplayMember = nameof(DeviceChoice.Label),
            ValueMember = nameof(DeviceChoice.Id),
            Margin = new Padding(0, 4, 0, 4)
        };
    }

    private static ComboBox CreateKeyComboBox()
    {
        var comboBox = new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 180,
            DisplayMember = nameof(KeyOption.Label)
        };
        comboBox.Items.AddRange(BuildKeyOptions().Cast<object>().ToArray());
        return comboBox;
    }

    private static Label CreateSectionTitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private static Label CreateFieldLabel()
    {
        return new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 0, 6),
            Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static TableLayoutPanel CreateTwoColumnFormLayout(float leftColumnWidth = 148F)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, leftColumnWidth));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static FlowLayoutPanel CreateButtonRow(bool wrapContents = true)
    {
        return new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = wrapContents,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
    }

    private static ComboBox CreateIconComboBox()
    {
        return new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            DisplayMember = nameof(NotificationIconCatalog.IconChoice.Label),
            ValueMember = nameof(NotificationIconCatalog.IconChoice.FileName),
            Margin = new Padding(0)
        };
    }

    private static PictureBox CreateIconPreview()
    {
        return new PictureBox
        {
            Size = new Size(32, 32),
            MinimumSize = new Size(32, 32),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(0, 0, 8, 0)
        };
    }

    private static Control CreateIconSelector(ComboBox comboBox, PictureBox preview)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.Controls.Add(preview, 0, 0);
        layout.Controls.Add(comboBox, 1, 0);
        return layout;
    }

    private Control BuildOverlayAnchorSelector()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 3,
            Margin = new Padding(0, 6, 0, 0),
            BackColor = Color.Transparent
        };

        for (var index = 0; index < 3; index++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        }

        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.TopLeft], 0, 0);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.TopCenter], 1, 0);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.TopRight], 2, 0);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.MiddleLeft], 0, 1);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.Center], 1, 1);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.MiddleRight], 2, 1);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.BottomLeft], 0, 2);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.BottomCenter], 1, 2);
        layout.Controls.Add(_overlayAnchorButtons[ProfileOverlayAnchor.BottomRight], 2, 2);
        return layout;
    }

    private Control BuildOverlayLayoutSelector()
    {
        var layout = CreateButtonRow(wrapContents: false);
        layout.Margin = new Padding(0, 6, 0, 0);
        layout.Controls.Add(_overlayHorizontalLayoutRadioButton);
        layout.Controls.Add(_overlayVerticalLayoutRadioButton);
        return layout;
    }

    private static TableLayoutPanel CreatePageLayout()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static TableLayoutPanel CreateSingleColumnLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static TableLayoutPanel CreateEqualWidthColumnsLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        return layout;
    }

    private static RoundedPanel CreateInsetCard(string? tag = "soft", DockStyle dock = DockStyle.Top, Padding? margin = null)
    {
        var card = CreateCard(tag, autoSize: true, dock: dock, cornerRadius: SurfaceCornerRadius);
        card.Padding = new Padding(16);
        card.Margin = margin ?? new Padding(0, 0, 0, 12);
        return card;
    }

    private static Control BuildSettingsLeadBlock(Label titleLabel, Label? noteLabel = null)
    {
        var layout = CreateSingleColumnLayout();
        titleLabel.Margin = Padding.Empty;
        layout.Controls.Add(titleLabel, 0, 0);

        if (noteLabel is not null)
        {
            noteLabel.Margin = new Padding(0, 8, 0, 0);
            layout.Controls.Add(noteLabel, 0, 1);
        }

        return layout;
    }

    private static Control CreateFieldGroup(Label label, Control input, Padding? margin = null)
    {
        var layout = CreateSingleColumnLayout();
        layout.Margin = margin ?? Padding.Empty;
        label.Margin = Padding.Empty;
        input.Margin = new Padding(0, 6, 0, 0);
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(input, 0, 1);
        return layout;
    }

    private static Panel CreateDivider(Padding? margin = null)
    {
        return new Panel
        {
            Tag = "divider",
            Height = 1,
            MinimumSize = new Size(0, 1),
            Dock = DockStyle.Top,
            Margin = margin ?? new Padding(0, 12, 0, 10)
        };
    }

    private static FlowLayoutPanel CreateModifierSelectorRow(
        CheckBox controlCheckBox,
        CheckBox altCheckBox,
        CheckBox shiftCheckBox,
        CheckBox windowsCheckBox)
    {
        var layout = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Margin = new Padding(0, 6, 0, 0),
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };

        foreach (var checkBox in new[] { controlCheckBox, altCheckBox, shiftCheckBox, windowsCheckBox })
        {
            checkBox.Margin = new Padding(0, 0, 8, 0);
            layout.Controls.Add(checkBox);
        }

        return layout;
    }

    private static Control BuildManualHotkeyEditor(
        Label modifiersLabel,
        CheckBox controlCheckBox,
        CheckBox altCheckBox,
        CheckBox shiftCheckBox,
        CheckBox windowsCheckBox,
        Label keyLabel,
        ComboBox keyComboBox)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156F));

        var modifiersPanel = CreateSingleColumnLayout();
        modifiersLabel.Margin = Padding.Empty;
        modifiersPanel.Controls.Add(modifiersLabel, 0, 0);
        modifiersPanel.Controls.Add(CreateModifierSelectorRow(controlCheckBox, altCheckBox, shiftCheckBox, windowsCheckBox), 0, 1);

        var keyPanel = CreateSingleColumnLayout();
        keyLabel.Margin = Padding.Empty;
        keyComboBox.Dock = DockStyle.Top;
        keyComboBox.Margin = new Padding(0, 6, 0, 0);
        keyPanel.Controls.Add(keyLabel, 0, 0);
        keyPanel.Controls.Add(keyComboBox, 0, 1);

        layout.Controls.Add(modifiersPanel, 0, 0);
        layout.Controls.Add(keyPanel, 1, 0);
        return layout;
    }

    private Control BuildShellLayout()
    {
        _sidebarCard.Controls.Add(BuildSidebarContent());
        _headerCard.Controls.Add(BuildHeaderContent());
        _mainDeviceCard.Controls.Add(BuildMainDeviceCardContent());
        _mainSwitchCard.Controls.Add(BuildMainSwitchCardContent());
        _mainProgramToggleCard.Controls.Add(BuildMainProgramToggleCardContent());
        _settingsGeneralCard.Controls.Add(BuildSettingsGeneralCardContent());
        _settingsAutomationCard.Controls.Add(BuildSettingsAutomationCardContent());
        _settingsHotkeyCard.Controls.Add(BuildSettingsHotkeyCardContent());
        _settingsToggleRulesCard.Controls.Add(BuildSettingsToggleRulesCardContent());
        _settingsOverlayCard.Controls.Add(BuildSettingsOverlayCardContent());
        _settingsProfilesCard.Controls.Add(BuildSettingsProfilesCardContent());
        _statusCard.Controls.Add(BuildStatusContent());

        _pageHost.Controls.Add(_settingsPage);
        _pageHost.Controls.Add(_mainPage);

        var contentColumn = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        contentColumn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        contentColumn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        contentColumn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentColumn.Controls.Add(_headerCard, 0, 0);
        contentColumn.Controls.Add(_pageHost, 0, 1);
        contentColumn.Controls.Add(_statusCard, 0, 2);

        _shellLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 1
        };
        _shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F));
        _shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _shellLayout.Controls.Add(_sidebarCard, 0, 0);
        _shellLayout.Controls.Add(contentColumn, 1, 0);

        return _shellLayout;
    }

    private Control BuildSidebarContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var brandBlock = new Panel
        {
            Dock = DockStyle.Top,
            Height = 26,
            Padding = new Padding(0)
        };
        brandBlock.Controls.Add(_brandLabel);
        _brandLabel.Location = new Point(0, 0);

        var navPanel = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            Margin = new Padding(0, 12, 0, 0)
        };
        navPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        navPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        navPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        navPanel.Controls.Add(_mainNavButton);
        navPanel.Controls.Add(_settingsNavButton);

        layout.Controls.Add(brandBlock, 0, 0);
        layout.Controls.Add(navPanel, 0, 1);
        return layout;
    }

    private Control BuildHeaderContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            Margin = new Padding(0)
        };
        titlePanel.Controls.Add(_pageTitleLabel);
        _pageTitleLabel.Location = new Point(0, 0);

        layout.Controls.Add(_menuButton, 0, 0);
        layout.Controls.Add(titlePanel, 1, 0);
        return layout;
    }

    private TableLayoutPanel BuildMainPage()
    {
        var page = CreatePageLayout();
        _mainSwitchCard.Margin = new Padding(0, 0, 8, 0);
        _mainDeviceCard.Margin = new Padding(8, 0, 0, 0);
        _mainProgramToggleCard.Margin = new Padding(0, 12, 0, 0);

        var dashboard = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };
        dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        dashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        dashboard.Controls.Add(_mainSwitchCard, 0, 0);
        dashboard.Controls.Add(_mainDeviceCard, 1, 0);

        page.Controls.Add(dashboard, 0, 0);
        page.Controls.Add(_mainProgramToggleCard, 0, 1);
        return page;
    }

    private TableLayoutPanel BuildSettingsPage()
    {
        _settingsGeneralCard.Margin = Padding.Empty;
        _settingsAutomationCard.Margin = Padding.Empty;
        _settingsHotkeyCard.Margin = Padding.Empty;
        _settingsToggleRulesCard.Margin = Padding.Empty;
        _settingsOverlayCard.Margin = Padding.Empty;
        _settingsProfilesCard.Margin = Padding.Empty;

        var navigationLayout = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 6,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };
        navigationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var index = 0; index < 6; index++)
        {
            navigationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        foreach (var button in new[]
        {
            _settingsGeneralTabButton,
            _settingsAutomationTabButton,
            _settingsShortcutsTabButton,
            _settingsToggleRulesTabButton,
            _settingsOverlayTabButton,
            _settingsProfilesTabButton
        })
        {
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 14, 0);
            button.Margin = new Padding(0, 0, 0, 8);
            button.Dock = DockStyle.Fill;
        }

        navigationLayout.Controls.Add(_settingsGeneralTabButton, 0, 0);
        navigationLayout.Controls.Add(_settingsAutomationTabButton, 0, 1);
        navigationLayout.Controls.Add(_settingsShortcutsTabButton, 0, 2);
        navigationLayout.Controls.Add(_settingsToggleRulesTabButton, 0, 3);
        navigationLayout.Controls.Add(_settingsOverlayTabButton, 0, 4);
        navigationLayout.Controls.Add(_settingsProfilesTabButton, 0, 5);

        var navigationCard = CreateCard("soft", autoSize: true, dock: DockStyle.Fill, cornerRadius: SurfaceCornerRadius);
        navigationCard.Padding = new Padding(10);
        navigationCard.Margin = new Padding(0, 0, 12, 0);
        navigationCard.MinimumSize = new Size(180, 0);
        navigationCard.Controls.Add(navigationLayout);

        var settingsShell = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };
        settingsShell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 192F));
        settingsShell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        settingsShell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        settingsShell.Controls.Add(navigationCard, 0, 0);
        settingsShell.Controls.Add(_settingsContentHost, 1, 0);

        var page = CreatePageLayout();
        page.Controls.Add(settingsShell, 0, 0);
        SetSettingsSection(SettingsSection.General, updateRadioSelection: true);
        return page;
    }

    private Control BuildMainDeviceCardContent()
    {
        var layout = CreateTwoColumnFormLayout();
        layout.Controls.Add(_deviceCardTitleLabel, 0, 0);
        layout.SetColumnSpan(_deviceCardTitleLabel, 2);
        layout.Controls.Add(_deviceALabel, 0, 1);
        layout.Controls.Add(_primaryDeviceComboBox, 1, 1);
        layout.Controls.Add(_deviceBLabel, 0, 2);
        layout.Controls.Add(_secondaryDeviceComboBox, 1, 2);

        var actionRow = CreateButtonRow();
        actionRow.Margin = new Padding(0, 12, 0, 0);
        actionRow.Controls.Add(_refreshDevicesButton);
        layout.Controls.Add(actionRow, 0, 3);
        layout.SetColumnSpan(actionRow, 2);

        return layout;
    }

    private Control BuildMainSwitchCardContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        _toggleNowButton.Dock = DockStyle.Top;
        _toggleNowButton.Margin = new Padding(0, 8, 0, 10);
        _toggleNowButton.MinimumSize = new Size(0, 48);
        _toggleNowButton.Height = 48;

        layout.Controls.Add(_switchCardTitleLabel, 0, 0);
        layout.Controls.Add(_toggleNowButton, 0, 1);
        layout.Controls.Add(_createShortcutButton, 0, 2);
        return layout;
    }

    private Control BuildMainProgramToggleCardContent()
    {
        var root = CreateSingleColumnLayout();
        root.Controls.Add(_mainProgramToggleTitleLabel, 0, 0);

        var content = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var listLayout = CreateSingleColumnLayout();
        listLayout.Margin = new Padding(0, 0, 12, 0);
        _mainProgramToggleProgramsLabel.Margin = Padding.Empty;
        listLayout.Controls.Add(_mainProgramToggleProgramsLabel, 0, 0);
        listLayout.Controls.Add(_mainProgramToggleProgramsListBox!, 0, 1);

        var editorLayout = CreateSingleColumnLayout();
        editorLayout.Margin = new Padding(12, 0, 0, 0);
        var deviceGrid = CreateEqualWidthColumnsLayout();
        deviceGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        deviceGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        deviceGrid.Controls.Add(
            CreateFieldGroup(
                _mainProgramTogglePrimaryDeviceLabel,
                _mainProgramTogglePrimaryDeviceComboBox,
                margin: new Padding(0, 0, 6, 10)),
            0,
            0);
        deviceGrid.Controls.Add(
            CreateFieldGroup(
                _mainProgramToggleSecondaryDeviceLabel,
                _mainProgramToggleSecondaryDeviceComboBox,
                margin: new Padding(6, 0, 0, 10)),
            1,
            0);
        deviceGrid.Controls.Add(_mainProgramToggleVolumeMixerCheckBox, 0, 1);
        deviceGrid.SetColumnSpan(_mainProgramToggleVolumeMixerCheckBox, 2);

        editorLayout.Controls.Add(deviceGrid, 0, 0);
        content.Controls.Add(listLayout, 0, 0);
        content.Controls.Add(editorLayout, 1, 0);

        root.Controls.Add(content, 0, 1);
        return root;
    }

    private Control BuildSettingsGeneralCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_generalTitleLabel));

        var topGrid = CreateEqualWidthColumnsLayout();
        topGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var themeOptionsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Color.Transparent
        };
        themeOptionsPanel.Controls.Add(_systemThemeRadioButton);
        themeOptionsPanel.Controls.Add(_lightThemeRadioButton);
        themeOptionsPanel.Controls.Add(_darkThemeRadioButton);

        var appearanceCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(0, 0, 6, 12));
        var appearanceLayout = CreateSingleColumnLayout();
        _languageLabel.Margin = Padding.Empty;
        _languageComboBox.Margin = new Padding(0, 6, 0, 0);
        _themeLabel.Margin = new Padding(0, 14, 0, 0);
        themeOptionsPanel.Margin = new Padding(0, 6, 0, 0);
        appearanceLayout.Controls.Add(_languageLabel, 0, 0);
        appearanceLayout.Controls.Add(_languageComboBox, 0, 1);
        appearanceLayout.Controls.Add(_themeLabel, 0, 2);
        appearanceLayout.Controls.Add(themeOptionsPanel, 0, 3);
        appearanceCard.Controls.Add(appearanceLayout);

        _startWithWindowsCheckBox.Margin = new Padding(0, 0, 0, 8);
        _startMinimizedAtStartupCheckBox.Margin = new Padding(0, 0, 0, 8);
        _minimizeToTrayOnCloseCheckBox.Margin = new Padding(0, 0, 0, 8);
        _enableUpdateNotificationsCheckBox.Margin = Padding.Empty;

        var behaviorCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(6, 0, 0, 12));
        var behaviorLayout = CreateSingleColumnLayout();
        behaviorLayout.Controls.Add(_startWithWindowsCheckBox, 0, 0);
        behaviorLayout.Controls.Add(_startMinimizedAtStartupCheckBox, 0, 1);
        behaviorLayout.Controls.Add(_minimizeToTrayOnCloseCheckBox, 0, 2);
        behaviorLayout.Controls.Add(_enableUpdateNotificationsCheckBox, 0, 3);
        behaviorCard.Controls.Add(behaviorLayout);

        var primaryIconSelector = CreateIconSelector(_primaryIconComboBox, _primaryIconPreview);
        var iconCard = CreateInsetCard(margin: Padding.Empty);
        var iconLayout = CreateTwoColumnFormLayout(leftColumnWidth: 112F);
        var actionRow = CreateButtonRow();
        actionRow.Margin = new Padding(0, 12, 0, 0);
        actionRow.Controls.Add(_openIconFolderButton);
        actionRow.Controls.Add(_refreshIconsButton);

        iconLayout.Controls.Add(_deviceAIconLabel, 0, 0);
        iconLayout.Controls.Add(primaryIconSelector, 1, 0);
        iconLayout.Controls.Add(actionRow, 0, 1);
        iconLayout.SetColumnSpan(actionRow, 2);
        iconCard.Controls.Add(iconLayout);

        topGrid.Controls.Add(appearanceCard, 0, 0);
        topGrid.Controls.Add(behaviorCard, 1, 0);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(topGrid, 0, 1);
        root.Controls.Add(iconCard, 0, 2);
        return root;
    }

    private Control BuildSettingsAutomationCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_automationTitleLabel));

        _syncCommunicationDeviceWithPlaybackCheckBox.Margin = new Padding(0, 0, 0, 10);
        _autoSwitchToNewPlaybackDeviceCheckBox.Margin = Padding.Empty;

        var deviceRulesCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(0, 0, 6, 0));
        var deviceRulesLayout = CreateSingleColumnLayout();
        deviceRulesLayout.Controls.Add(_syncCommunicationDeviceWithPlaybackCheckBox, 0, 0);
        deviceRulesLayout.Controls.Add(_autoSwitchToNewPlaybackDeviceCheckBox, 0, 1);
        deviceRulesCard.Controls.Add(deviceRulesLayout);

        _enableProfilesCheckBox.Margin = new Padding(0, 0, 0, 8);
        _profileAutomationHintLabel.Margin = Padding.Empty;

        var profileAutomationCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(6, 0, 0, 0));
        var profileAutomationLayout = CreateSingleColumnLayout();
        profileAutomationLayout.Controls.Add(_enableProfilesCheckBox, 0, 0);
        profileAutomationLayout.Controls.Add(_profileAutomationHintLabel, 0, 1);
        profileAutomationCard.Controls.Add(profileAutomationLayout);

        root.Controls.Add(headerCard, 0, 0);

        var contentGrid = CreateEqualWidthColumnsLayout();
        contentGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentGrid.Controls.Add(deviceRulesCard, 0, 0);
        contentGrid.Controls.Add(profileAutomationCard, 1, 0);

        root.Controls.Add(contentGrid, 0, 1);
        return root;
    }

    private Control BuildSettingsHotkeyCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_hotkeyTitleLabel, _hotkeyNoteLabel));

        _switchShortcutSectionLabel.Margin = Padding.Empty;
        _enableHotkeyCheckBox.Margin = new Padding(0, 8, 0, 10);
        _recentSwitchUndoSectionLabel.Margin = Padding.Empty;
        _enableRecentSwitchUndoHotkeyCheckBox.Margin = new Padding(0, 6, 0, 10);

        var settingsCard = CreateInsetCard(margin: Padding.Empty);
        var settingsLayout = CreateSingleColumnLayout();
        settingsLayout.Controls.Add(_switchShortcutSectionLabel, 0, 0);
        settingsLayout.Controls.Add(_enableHotkeyCheckBox, 0, 1);
        settingsLayout.Controls.Add(
            BuildManualHotkeyEditor(
                _modifiersLabel,
                _controlCheckBox,
                _altCheckBox,
                _shiftCheckBox,
                _windowsCheckBox,
                _keyLabel,
                _hotkeyComboBox),
            0,
            2);
        settingsLayout.Controls.Add(CreateDivider(), 0, 3);
        settingsLayout.Controls.Add(_recentSwitchUndoSectionLabel, 0, 4);
        settingsLayout.Controls.Add(_enableRecentSwitchUndoHotkeyCheckBox, 0, 5);
        settingsLayout.Controls.Add(
            BuildManualHotkeyEditor(
                _recentSwitchUndoModifiersLabel,
                _recentSwitchUndoControlCheckBox,
                _recentSwitchUndoAltCheckBox,
                _recentSwitchUndoShiftCheckBox,
                _recentSwitchUndoWindowsCheckBox,
                _recentSwitchUndoKeyLabel,
                _recentSwitchUndoHotkeyComboBox),
            0,
            6);
        settingsCard.Controls.Add(settingsLayout);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(settingsCard, 0, 1);
        return root;
    }

    private Control BuildSettingsToggleRulesCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_toggleRulesTitleLabel));

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var ruleListCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(0, 0, 6, 0));
        var ruleListLayout = CreateSingleColumnLayout();

        var listAndMoveLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        listAndMoveLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        listAndMoveLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        listAndMoveLayout.Controls.Add(_toggleRulesListBox!, 0, 0);

        var moveButtonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(8, 0, 0, 0)
        };
        moveButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        moveButtonsPanel.Controls.Add(_moveToggleRuleUpButton, 0, 0);
        moveButtonsPanel.Controls.Add(_moveToggleRuleDownButton, 0, 1);
        _moveToggleRuleUpButton.Margin = new Padding(0, 0, 0, 8);
        _moveToggleRuleDownButton.Margin = new Padding(0);
        listAndMoveLayout.Controls.Add(moveButtonsPanel, 1, 0);

        ruleListLayout.Controls.Add(listAndMoveLayout, 0, 0);

        var ruleButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };
        ruleButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        ruleButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        ruleButtons.Controls.Add(_addToggleRuleButton, 0, 0);
        ruleButtons.Controls.Add(_removeToggleRuleButton, 1, 0);
        ruleListLayout.Controls.Add(ruleButtons, 0, 1);
        ruleListCard.Controls.Add(ruleListLayout);

        var editorCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(6, 0, 0, 0));
        var editorLayout = CreateSingleColumnLayout();

        var topFieldsGrid = CreateEqualWidthColumnsLayout();
        topFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topFieldsGrid.Controls.Add(CreateFieldGroup(_toggleRuleNameLabel, _toggleRuleNameTextBox!, margin: new Padding(0, 0, 6, 10)), 0, 0);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_toggleRulePriorityLabel, _toggleRuleOrderTextBox!, margin: new Padding(6, 0, 0, 10)), 1, 0);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_toggleRulePrimaryDeviceLabel, _toggleRulePrimaryDeviceComboBox, margin: new Padding(0, 0, 6, 10)), 0, 1);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_toggleRuleSecondaryDeviceLabel, _toggleRuleSecondaryDeviceComboBox, margin: new Padding(6, 0, 0, 10)), 1, 1);
        topFieldsGrid.Controls.Add(_toggleRuleVolumeMixerCheckBox, 0, 2);
        topFieldsGrid.SetColumnSpan(_toggleRuleVolumeMixerCheckBox, 2);

        var programsLayout = CreateSingleColumnLayout();
        programsLayout.Margin = new Padding(0);
        _toggleRuleProgramsLabel.Margin = Padding.Empty;
        _toggleRuleProgramsListBox!.Margin = new Padding(0, 6, 0, 0);
        programsLayout.Controls.Add(_toggleRuleProgramsLabel, 0, 0);
        programsLayout.Controls.Add(_toggleRuleProgramsListBox!, 0, 1);

        var programButtons = CreateButtonRow();
        programButtons.Margin = new Padding(0, 8, 0, 0);
        programButtons.Controls.Add(_addToggleRuleProgramButton);
        programButtons.Controls.Add(_removeToggleRuleProgramButton);
        programsLayout.Controls.Add(programButtons, 0, 2);

        editorLayout.Controls.Add(topFieldsGrid, 0, 0);
        editorLayout.Controls.Add(programsLayout, 0, 1);
        editorLayout.Controls.Add(_toggleRuleOrderHintLabel, 0, 2);
        editorCard.Controls.Add(editorLayout);

        content.Controls.Add(ruleListCard, 0, 0);
        content.Controls.Add(editorCard, 1, 0);
        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(content, 0, 1);

        return root;
    }

    private Control BuildSettingsOverlayCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_overlayTitleLabel, _overlayHintLabel));

        var settingsCard = CreateInsetCard(margin: Padding.Empty);
        var settingsLayout = CreateSingleColumnLayout();

        var placementGrid = CreateEqualWidthColumnsLayout();
        placementGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        placementGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        placementGrid.Controls.Add(
            CreateFieldGroup(_overlayPositionLabel, BuildOverlayAnchorSelector(), margin: new Padding(0, 0, 6, 12)),
            0,
            0);
        placementGrid.Controls.Add(
            CreateFieldGroup(_overlayLayoutLabel, BuildOverlayLayoutSelector(), margin: new Padding(6, 0, 0, 12)),
            1,
            0);
        placementGrid.Controls.Add(
            CreateFieldGroup(_overlayHeightLabel, _overlayHeightNumericUpDown!, margin: new Padding(0, 0, 6, 0)),
            0,
            1);
        placementGrid.SetColumnSpan(placementGrid.GetControlFromPosition(0, 1)!, 2);

        var actionsRow = CreateButtonRow(wrapContents: false);
        actionsRow.Margin = new Padding(0, 12, 0, 0);
        actionsRow.Controls.Add(_openProfileOverlayButton);

        _overlayShortcutSectionLabel.Margin = new Padding(0, 18, 0, 0);
        _enableOverlayHotkeyCheckBox.Margin = new Padding(0, 8, 0, 10);

        settingsLayout.Controls.Add(placementGrid, 0, 0);
        settingsLayout.Controls.Add(actionsRow, 0, 1);
        settingsLayout.Controls.Add(CreateDivider(margin: new Padding(0, 18, 0, 12)), 0, 2);
        settingsLayout.Controls.Add(_overlayShortcutSectionLabel, 0, 3);
        settingsLayout.Controls.Add(_enableOverlayHotkeyCheckBox, 0, 4);
        settingsLayout.Controls.Add(
            BuildManualHotkeyEditor(
                _overlayModifiersLabel,
                _overlayControlCheckBox,
                _overlayAltCheckBox,
                _overlayShiftCheckBox,
                _overlayWindowsCheckBox,
                _overlayKeyLabel,
                _overlayHotkeyComboBox),
            0,
            5);
        settingsCard.Controls.Add(settingsLayout);

        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(settingsCard, 0, 1);
        return root;
    }

    private Control BuildSettingsProfilesCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_profilesTitleLabel));

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var profileListCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(0, 0, 6, 0));

        var profileListLayout = CreateSingleColumnLayout();

        var listAndMoveLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        listAndMoveLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        listAndMoveLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        listAndMoveLayout.Controls.Add(_profilesListBox!, 0, 0);

        var moveButtonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(8, 0, 0, 0)
        };
        moveButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        moveButtonsPanel.Controls.Add(_moveProfileUpButton, 0, 0);
        moveButtonsPanel.Controls.Add(_moveProfileDownButton, 0, 1);
        _moveProfileUpButton.Margin = new Padding(0, 0, 0, 8);
        _moveProfileDownButton.Margin = new Padding(0);
        listAndMoveLayout.Controls.Add(moveButtonsPanel, 1, 0);

        profileListLayout.Controls.Add(listAndMoveLayout, 0, 0);

        var profileButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };
        profileButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        profileButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        profileButtons.Controls.Add(_addProfileButton, 0, 0);
        profileButtons.Controls.Add(_removeProfileButton, 1, 0);
        profileListLayout.Controls.Add(profileButtons, 0, 1);

        profileListCard.Controls.Add(profileListLayout);

        var editorCard = CreateInsetCard(dock: DockStyle.Fill, margin: new Padding(6, 0, 0, 0));

        var editorLayout = CreateSingleColumnLayout();

        var topFieldsGrid = CreateEqualWidthColumnsLayout();
        topFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        topFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var profileIconSelector = CreateIconSelector(_profileIconComboBox, _profileIconPreview);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_profileNameLabel, _profileNameTextBox!, margin: new Padding(0, 0, 6, 10)), 0, 0);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_profileIconLabel, profileIconSelector, margin: new Padding(6, 0, 0, 10)), 1, 0);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_profileOutputDeviceLabel, _profileOutputDeviceComboBox, margin: new Padding(0, 0, 6, 0)), 0, 1);
        topFieldsGrid.Controls.Add(CreateFieldGroup(_profileInputDeviceLabel, _profileInputDeviceComboBox, margin: new Padding(6, 0, 0, 0)), 1, 1);

        var lowerFieldsGrid = CreateEqualWidthColumnsLayout();
        lowerFieldsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var programsLayout = CreateSingleColumnLayout();
        programsLayout.Margin = new Padding(6, 0, 0, 0);
        _profileProgramsLabel.Margin = Padding.Empty;
        _profileProgramsListBox!.Margin = new Padding(0, 6, 0, 0);
        programsLayout.Controls.Add(_profileProgramsLabel, 0, 0);
        programsLayout.Controls.Add(_profileProgramsListBox!, 0, 1);

        var programButtons = CreateButtonRow();
        programButtons.Margin = new Padding(0, 8, 0, 0);
        programButtons.Controls.Add(_addProfileProgramButton);
        programButtons.Controls.Add(_removeProfileProgramButton);
        programsLayout.Controls.Add(programButtons, 0, 2);

        lowerFieldsGrid.Controls.Add(CreateFieldGroup(_profilePriorityLabel, _profileOrderTextBox!, margin: new Padding(0, 0, 6, 0)), 0, 0);
        lowerFieldsGrid.Controls.Add(programsLayout, 1, 0);

        editorLayout.Controls.Add(topFieldsGrid, 0, 0);
        editorLayout.Controls.Add(lowerFieldsGrid, 0, 1);
        editorLayout.Controls.Add(_profileOrderHintLabel, 0, 2);
        editorCard.Controls.Add(editorLayout);

        content.Controls.Add(profileListCard, 0, 0);
        content.Controls.Add(editorCard, 1, 0);
        root.Controls.Add(headerCard, 0, 0);
        root.Controls.Add(content, 0, 1);

        return root;
    }

    private Control BuildStatusContent()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.Controls.Add(_statusLabel, 0, 0);
        layout.Controls.Add(_statusActionButton, 1, 0);
        return layout;
    }

}
