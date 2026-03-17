using System.Diagnostics;
using System.Drawing;
using System.Threading;
using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Hotkeys;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class MainShellForm : Form
{
    private readonly AppServices _services;
    private readonly TrayNotificationService _trayNotifications;
    private readonly RoundedPanel _sidebarCard;
    private readonly RoundedPanel _headerCard;
    private readonly RoundedPanel _statusCard;
    private readonly RoundedPanel _mainDeviceCard;
    private readonly RoundedPanel _mainSwitchCard;
    private readonly RoundedPanel _settingsGeneralCard;
    private readonly RoundedPanel _settingsHotkeyCard;
    private readonly RoundedButton _menuButton;
    private readonly Panel _pageHost;
    private readonly TableLayoutPanel _mainPage;
    private readonly TableLayoutPanel _settingsPage;
    private readonly PillRadioButton _mainNavButton;
    private readonly PillRadioButton _settingsNavButton;
    private readonly Label _brandLabel;
    private readonly Label _pageTitleLabel;
    private readonly Label _deviceCardTitleLabel;
    private readonly Label _switchCardTitleLabel;
    private readonly Label _generalTitleLabel;
    private readonly Label _hotkeyTitleLabel;
    private readonly Label _languageLabel;
    private readonly Label _themeLabel;
    private readonly CheckBox _startWithWindowsCheckBox;
    private readonly CheckBox _startMinimizedAtStartupCheckBox;
    private readonly CheckBox _minimizeToTrayOnCloseCheckBox;
    private readonly Label _deviceAIconLabel;
    private readonly Label _deviceBIconLabel;
    private readonly ComboBox _languageComboBox;
    private readonly ComboBox _primaryIconComboBox;
    private readonly ComboBox _secondaryIconComboBox;
    private readonly PillRadioButton _systemThemeRadioButton;
    private readonly PillRadioButton _lightThemeRadioButton;
    private readonly PillRadioButton _darkThemeRadioButton;
    private readonly Label _deviceALabel;
    private readonly Label _deviceBLabel;
    private readonly PictureBox _primaryIconPreview;
    private readonly PictureBox _secondaryIconPreview;
    private readonly ComboBox _primaryDeviceComboBox;
    private readonly ComboBox _secondaryDeviceComboBox;
    private readonly CheckBox _enableHotkeyCheckBox;
    private readonly Label _modifiersLabel;
    private readonly Label _keyLabel;
    private readonly CheckBox _controlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _windowsCheckBox;
    private readonly ComboBox _hotkeyComboBox;
    private readonly Label _hotkeyNoteLabel;
    private readonly Label _statusLabel;
    private readonly RoundedButton _statusActionButton;
    private readonly RoundedButton _toggleNowButton;
    private readonly RoundedButton _refreshDevicesButton;
    private readonly RoundedButton _openIconFolderButton;
    private readonly RoundedButton _refreshIconsButton;
    private readonly RoundedButton _createShortcutButton;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly ToolStripMenuItem _trayToggleMenuItem;
    private readonly ToolStripMenuItem _trayOpenSettingsMenuItem;
    private readonly ToolStripMenuItem _trayExitMenuItem;
    private readonly GlobalHotkey _trayHotkey;
    private readonly Icon _windowIcon;
    private readonly Icon _trayIcon;
    private readonly EventWaitHandle _restoreEvent;
    private readonly RegisteredWaitHandle _restoreWaitRegistration;
    private readonly bool _launchedFromStartup;
    private ThemePalette _activePalette = ThemeManager.ResolvePalette(AppThemeMode.System);
    private UpdateReleaseInfo? _availableUpdate;
    private PageKind _currentPage = PageKind.Main;
    private bool _isLoading;
    private bool _lastStatusWasError;
    private bool _allowClose;
    private bool _isHiddenToTray;
    private bool _sidebarVisible;
    private bool _suppressAutoSave;
    private bool _startHiddenToTrayRequested;
    private bool _updateDialogShown;
    private bool _allowFormVisibility;
    private bool _hotkeySelectionChangedWhileDroppedDown;
    private TableLayoutPanel? _shellLayout;

    public MainShellForm(AppServices services, bool launchedFromStartup = false)
    {
        _services = services;
        _trayNotifications = new TrayNotificationService(_services.Localizer.Get("AppName"));
        _launchedFromStartup = launchedFromStartup;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        DoubleBuffered = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 620);
        ClientSize = new Size(980, 660);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        MaximizeBox = false;
        _windowIcon = NotificationIconCatalog.CreateTrayIcon();
        Icon = _windowIcon;
        ShowIcon = true;

        _trayToggleMenuItem = new ToolStripMenuItem();
        _trayToggleMenuItem.Click += (_, _) => ToggleUsingSavedSettings(showFailureDialog: false);
        _trayOpenSettingsMenuItem = new ToolStripMenuItem();
        _trayOpenSettingsMenuItem.Click += (_, _) => RestoreFromTray();
        _trayExitMenuItem = new ToolStripMenuItem();
        _trayExitMenuItem.Click += (_, _) => ExitFromTray();
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.AddRange([_trayToggleMenuItem, _trayOpenSettingsMenuItem, _trayExitMenuItem]);
        _trayIcon = NotificationIconCatalog.CreateTrayIcon();
        _notifyIcon = new NotifyIcon
        {
            Visible = true,
            Icon = _trayIcon,
            ContextMenuStrip = _trayMenu,
            Text = _services.Localizer.Get("AppName")
        };
        _notifyIcon.DoubleClick += (_, _) => RestoreFromTray();
        _trayHotkey = new GlobalHotkey();
        _trayHotkey.Pressed += (_, _) => ToggleUsingSavedSettings(showFailureDialog: false);
        _restoreEvent = MainInstanceManager.CreateRestoreEvent();
        _restoreWaitRegistration = ThreadPool.RegisterWaitForSingleObject(
            _restoreEvent,
            (_, _) =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(HandleRestoreRequest));
                }
            },
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);

        _sidebarCard = CreateCard("sidebar", autoSize: false, dock: DockStyle.Fill, cornerRadius: 0);
        _headerCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _statusCard = CreateCard("soft", autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _mainDeviceCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _mainSwitchCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _settingsGeneralCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _settingsHotkeyCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: 0);
        _sidebarCard.Padding = new Padding(14);
        _headerCard.Padding = new Padding(10);
        _statusCard.Padding = new Padding(10, 8, 10, 8);
        _mainDeviceCard.Padding = new Padding(14);
        _mainSwitchCard.Padding = new Padding(14);
        _settingsGeneralCard.Padding = new Padding(14);
        _settingsHotkeyCard.Padding = new Padding(14);
        _menuButton = CreateActionButton("secondary", new Size(40, 40));
        _menuButton.AutoSize = false;
        _menuButton.Size = new Size(40, 40);
        _menuButton.Margin = new Padding(0);
        _menuButton.Text = "\uE700";
        _menuButton.Font = new Font("Segoe MDL2 Assets", 15F, FontStyle.Regular, GraphicsUnit.Point);
        _menuButton.Click += (_, _) => SetSidebarVisible(!_sidebarVisible);

        _brandLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 13.5F, FontStyle.Bold, GraphicsUnit.Point)
        };

        _mainNavButton = CreateNavButton();
        _mainNavButton.CheckedChanged += (_, _) =>
        {
            if (_mainNavButton.Checked)
            {
                NavigateToPage(PageKind.Main);
            }
        };

        _settingsNavButton = CreateNavButton();
        _settingsNavButton.CheckedChanged += (_, _) =>
        {
            if (_settingsNavButton.Checked)
            {
                NavigateToPage(PageKind.Settings);
            }
        };

        _pageTitleLabel = new Label { AutoSize = true, Font = new Font("Segoe UI Semibold", 18.5F, FontStyle.Bold, GraphicsUnit.Point), Margin = new Padding(0) };

        _deviceCardTitleLabel = CreateSectionTitleLabel();
        _switchCardTitleLabel = CreateSectionTitleLabel();
        _generalTitleLabel = CreateSectionTitleLabel();
        _hotkeyTitleLabel = CreateSectionTitleLabel();

        _languageLabel = CreateFieldLabel();
        _themeLabel = CreateFieldLabel();
        _startWithWindowsCheckBox = new CheckBox { AutoSize = true };
        _startWithWindowsCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateStartupControls();
            AutoSaveCurrentSelections();
        };
        _startMinimizedAtStartupCheckBox = new CheckBox { AutoSize = true };
        _startMinimizedAtStartupCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _minimizeToTrayOnCloseCheckBox = new CheckBox { AutoSize = true };
        _minimizeToTrayOnCloseCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _deviceAIconLabel = CreateFieldLabel();
        _deviceBIconLabel = CreateFieldLabel();
        _deviceALabel = CreateFieldLabel();
        _deviceBLabel = CreateFieldLabel();
        _modifiersLabel = CreateFieldLabel();
        _keyLabel = CreateFieldLabel();

        _languageComboBox = new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            DisplayMember = nameof(LanguageChoice.Label),
            ValueMember = nameof(LanguageChoice.Language),
            DataSource = BuildLanguageChoices()
        };
        _languageComboBox.SelectedIndexChanged += (_, _) => HandleLanguageChanged();

        _systemThemeRadioButton = CreateThemeRadioButton();
        _systemThemeRadioButton.CheckedChanged += (_, _) =>
        {
            if (_systemThemeRadioButton.Checked)
            {
                HandleThemeChanged(AppThemeMode.System);
            }
        };

        _lightThemeRadioButton = CreateThemeRadioButton();
        _lightThemeRadioButton.CheckedChanged += (_, _) =>
        {
            if (_lightThemeRadioButton.Checked)
            {
                HandleThemeChanged(AppThemeMode.Light);
            }
        };

        _darkThemeRadioButton = CreateThemeRadioButton();
        _darkThemeRadioButton.CheckedChanged += (_, _) =>
        {
            if (_darkThemeRadioButton.Checked)
            {
                HandleThemeChanged(AppThemeMode.Dark);
            }
        };

        _primaryDeviceComboBox = CreateDeviceComboBox();
        _primaryDeviceComboBox.SelectedIndexChanged += (_, _) => AutoSaveCurrentSelections();
        _secondaryDeviceComboBox = CreateDeviceComboBox();
        _secondaryDeviceComboBox.SelectedIndexChanged += (_, _) => AutoSaveCurrentSelections();
        _primaryIconPreview = CreateIconPreview();
        _secondaryIconPreview = CreateIconPreview();
        _primaryIconComboBox = CreateIconComboBox();
        _secondaryIconComboBox = CreateIconComboBox();
        _primaryIconComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateDeviceIconPreview(_primaryIconComboBox, _primaryIconPreview);
            AutoSaveCurrentSelections();
        };
        _secondaryIconComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateDeviceIconPreview(_secondaryIconComboBox, _secondaryIconPreview);
            AutoSaveCurrentSelections();
        };

        _enableHotkeyCheckBox = new CheckBox { AutoSize = true };
        _enableHotkeyCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateHotkeyControls();
            AutoSaveCurrentSelections();
        };
        _controlCheckBox = new CheckBox { AutoSize = true, Text = "Ctrl" };
        _controlCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _altCheckBox = new CheckBox { AutoSize = true, Text = "Alt" };
        _altCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _shiftCheckBox = new CheckBox { AutoSize = true, Text = "Shift" };
        _shiftCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _windowsCheckBox = new CheckBox { AutoSize = true, Text = "Win" };
        _windowsCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _hotkeyComboBox = new NoWheelComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 180,
            DisplayMember = nameof(KeyOption.Label),
            ValueMember = nameof(KeyOption.Value),
            DataSource = BuildKeyOptions()
        };
        _hotkeyComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (_hotkeyComboBox.DroppedDown)
            {
                _hotkeySelectionChangedWhileDroppedDown = true;
                return;
            }

            AutoSaveCurrentSelections();
        };
        _hotkeyComboBox.DropDownClosed += (_, _) =>
        {
            if (!_hotkeySelectionChangedWhileDroppedDown)
            {
                return;
            }

            _hotkeySelectionChangedWhileDroppedDown = false;
            AutoSaveCurrentSelections();
        };
        _hotkeyNoteLabel = new Label
        {
            AutoSize = false,
            Height = 44,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };

        _statusLabel = new Label
        {
            AutoSize = false,
            Height = 24,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _statusActionButton = CreateActionButton("secondary", new Size(132, 34));
        _statusActionButton.Visible = false;
        _statusActionButton.Margin = new Padding(12, 0, 0, 0);
        _statusActionButton.Click += (_, _) => ShowAvailableUpdateDialog();

        _toggleNowButton = CreateActionButton("primary", new Size(180, 44));
        _toggleNowButton.Click += (_, _) => ToggleUsingCurrentSelections();
        _refreshDevicesButton = CreateActionButton("secondary", new Size(148, 40));
        _refreshDevicesButton.Click += (_, _) => RefreshDeviceLists(preserveSelections: true);
        _openIconFolderButton = CreateActionButton("secondary", new Size(148, 40));
        _openIconFolderButton.Click += (_, _) => OpenIconFolder();
        _refreshIconsButton = CreateActionButton("secondary", new Size(148, 40));
        _refreshIconsButton.Click += (_, _) => RefreshAvailableIcons(preserveSelections: true, showStatus: true);
        _createShortcutButton = CreateActionButton("secondary", new Size(180, 40));
        _createShortcutButton.Click += (_, _) => CreateToggleShortcut();

        _pageHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 8, 0, 0)
        };

        _mainPage = BuildMainPage();
        _settingsPage = BuildSettingsPage();

        Controls.Add(BuildShellLayout());

        ApplyLanguage();
        ApplyTheme(AppThemeMode.System);
        SetActivePage(PageKind.Main, updateRadioSelection: true);
        SetSidebarVisible(false);
        SetStatus(_services.Localizer.Get("InitialStatus"));

        Load += (_, _) => SafeLoadInitialState();
        FormClosing += (_, eventArgs) => HandleFormClosing(eventArgs);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _restoreWaitRegistration.Unregister(null);
            _restoreEvent.Dispose();
            _trayHotkey.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _windowIcon.Dispose();
            _trayIcon.Dispose();
            _trayMenu.Dispose();
            _trayNotifications.Dispose();
            SetPictureBoxImage(_primaryIconPreview, null);
            SetPictureBoxImage(_secondaryIconPreview, null);
        }

        base.Dispose(disposing);
    }

    protected override void SetVisibleCore(bool value)
    {
        if (!_allowFormVisibility)
        {
            value = false;

            if (!IsHandleCreated)
            {
                CreateHandle();
            }
        }

        base.SetVisibleCore(value);
    }

    private static RoundedPanel CreateCard(string? tag = null, bool autoSize = true, DockStyle dock = DockStyle.Top, int cornerRadius = 0)
    {
        return new RoundedPanel
        {
            Tag = tag,
            AutoSize = autoSize,
            Dock = dock,
            CornerRadius = cornerRadius,
            BorderThickness = 1,
            Margin = new Padding(0, 0, 0, 10)
        };
    }

    private static PillRadioButton CreateNavButton()
    {
        return new PillRadioButton
        {
            Tag = "nav",
            CornerRadius = 0,
            Dock = DockStyle.Fill,
            Height = 40,
            MinimumSize = new Size(0, 40),
            Margin = new Padding(0, 0, 0, 8),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 12, 0),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private static PillRadioButton CreateThemeRadioButton()
    {
        return new PillRadioButton
        {
            CornerRadius = 0,
            Size = new Size(136, 34),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        };
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

    private static Label CreateSectionTitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
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
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
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

    private Control BuildShellLayout()
    {
        _sidebarCard.Controls.Add(BuildSidebarContent());
        _headerCard.Controls.Add(BuildHeaderContent());
        _mainDeviceCard.Controls.Add(BuildMainDeviceCardContent());
        _mainSwitchCard.Controls.Add(BuildMainSwitchCardContent());
        _settingsGeneralCard.Controls.Add(BuildSettingsGeneralCardContent());
        _settingsHotkeyCard.Controls.Add(BuildSettingsHotkeyCardContent());
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
            Padding = new Padding(12),
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
        page.Controls.Add(_mainDeviceCard, 0, 0);
        page.Controls.Add(_mainSwitchCard, 0, 1);
        return page;
    }

    private TableLayoutPanel BuildSettingsPage()
    {
        var page = CreatePageLayout();
        page.Controls.Add(_settingsGeneralCard, 0, 0);
        page.Controls.Add(_settingsHotkeyCard, 0, 1);
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

        var actionRow = CreateButtonRow();
        actionRow.Margin = new Padding(0, 12, 0, 0);
        actionRow.Controls.Add(_toggleNowButton);
        actionRow.Controls.Add(_createShortcutButton);

        layout.Controls.Add(_switchCardTitleLabel, 0, 0);
        layout.Controls.Add(actionRow, 0, 1);
        return layout;
    }

    private Control BuildSettingsGeneralCardContent()
    {
        var layout = CreateTwoColumnFormLayout();

        var themeOptionsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Margin = new Padding(0)
        };
        themeOptionsPanel.Controls.Add(_systemThemeRadioButton);
        themeOptionsPanel.Controls.Add(_lightThemeRadioButton);
        themeOptionsPanel.Controls.Add(_darkThemeRadioButton);
        var primaryIconSelector = CreateIconSelector(_primaryIconComboBox, _primaryIconPreview);
        var secondaryIconSelector = CreateIconSelector(_secondaryIconComboBox, _secondaryIconPreview);
        var actionRow = CreateButtonRow();
        actionRow.Margin = new Padding(0, 12, 0, 0);
        actionRow.Controls.Add(_openIconFolderButton);
        actionRow.Controls.Add(_refreshIconsButton);

        layout.Controls.Add(_generalTitleLabel, 0, 0);
        layout.SetColumnSpan(_generalTitleLabel, 2);
        layout.Controls.Add(_languageLabel, 0, 1);
        layout.Controls.Add(_languageComboBox, 1, 1);
        layout.Controls.Add(_themeLabel, 0, 2);
        layout.Controls.Add(themeOptionsPanel, 1, 2);
        layout.Controls.Add(_startWithWindowsCheckBox, 0, 3);
        layout.SetColumnSpan(_startWithWindowsCheckBox, 2);
        layout.Controls.Add(_startMinimizedAtStartupCheckBox, 0, 4);
        layout.SetColumnSpan(_startMinimizedAtStartupCheckBox, 2);
        layout.Controls.Add(_minimizeToTrayOnCloseCheckBox, 0, 5);
        layout.SetColumnSpan(_minimizeToTrayOnCloseCheckBox, 2);
        layout.Controls.Add(_deviceAIconLabel, 0, 6);
        layout.Controls.Add(primaryIconSelector, 1, 6);
        layout.Controls.Add(_deviceBIconLabel, 0, 7);
        layout.Controls.Add(secondaryIconSelector, 1, 7);
        layout.Controls.Add(actionRow, 0, 8);
        layout.SetColumnSpan(actionRow, 2);

        return layout;
    }

    private Control BuildSettingsHotkeyCardContent()
    {
        var layout = CreateTwoColumnFormLayout();

        var modifiersPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = true,
            Margin = new Padding(0)
        };
        modifiersPanel.Controls.Add(_controlCheckBox);
        modifiersPanel.Controls.Add(_altCheckBox);
        modifiersPanel.Controls.Add(_shiftCheckBox);
        modifiersPanel.Controls.Add(_windowsCheckBox);

        layout.Controls.Add(_hotkeyTitleLabel, 0, 0);
        layout.SetColumnSpan(_hotkeyTitleLabel, 2);
        layout.Controls.Add(_enableHotkeyCheckBox, 0, 1);
        layout.SetColumnSpan(_enableHotkeyCheckBox, 2);
        layout.Controls.Add(_modifiersLabel, 0, 2);
        layout.Controls.Add(modifiersPanel, 1, 2);
        layout.Controls.Add(_keyLabel, 0, 3);
        layout.Controls.Add(_hotkeyComboBox, 1, 3);
        layout.Controls.Add(_hotkeyNoteLabel, 0, 4);
        layout.SetColumnSpan(_hotkeyNoteLabel, 2);

        return layout;
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

    private void ApplyLanguage(bool refreshIconChoices = true)
    {
        RunWithoutAutoSave(() =>
        {
            Text = _services.Localizer.Get("AppName");
            ApplyTrayText();

            _brandLabel.Text = _services.Localizer.Get("SidebarMenuTitle");
            _mainNavButton.Text = _services.Localizer.Get("MainTab");
            _settingsNavButton.Text = _services.Localizer.Get("SettingsTab");

            _deviceCardTitleLabel.Text = _services.Localizer.Get("DevicesGroup");
            _switchCardTitleLabel.Text = _services.Localizer.Get("ActionsGroup");
            _generalTitleLabel.Text = _services.Localizer.Get("GeneralGroup");
            _hotkeyTitleLabel.Text = _services.Localizer.Get("HotkeyGroup");

            _languageLabel.Text = _services.Localizer.Get("LanguageLabel");
            _themeLabel.Text = _services.Localizer.Get("ThemeLabel");
            _startWithWindowsCheckBox.Text = _services.Localizer.Get("StartWithWindowsLabel");
            _startMinimizedAtStartupCheckBox.Text = _services.Localizer.Get("StartMinimizedAtStartupLabel");
            _minimizeToTrayOnCloseCheckBox.Text = _services.Localizer.Get("MinimizeToTrayOnCloseLabel");
            _deviceAIconLabel.Text = _services.Localizer.Get("DeviceAIconLabel");
            _deviceBIconLabel.Text = _services.Localizer.Get("DeviceBIconLabel");
            _systemThemeRadioButton.Text = _services.Localizer.Get("ThemeModeSystem");
            _lightThemeRadioButton.Text = _services.Localizer.Get("ThemeModeLight");
            _darkThemeRadioButton.Text = _services.Localizer.Get("ThemeModeDark");
            _deviceALabel.Text = _services.Localizer.Get("DeviceALabel");
            _deviceBLabel.Text = _services.Localizer.Get("DeviceBLabel");
            _enableHotkeyCheckBox.Text = _services.Localizer.Get("EnableGlobalHotkey");
            _modifiersLabel.Text = _services.Localizer.Get("ModifiersLabel");
            _keyLabel.Text = _services.Localizer.Get("KeyLabel");
            _hotkeyNoteLabel.Text = _services.Localizer.Get("HotkeyHint");

            _toggleNowButton.Text = _services.Localizer.Get("ToggleNowButton");
            _refreshDevicesButton.Text = _services.Localizer.Get("RefreshDevicesButton");
            _openIconFolderButton.Text = _services.Localizer.Get("OpenIconFolderButton");
            _refreshIconsButton.Text = _services.Localizer.Get("RefreshIconsButton");
            _createShortcutButton.Text = _services.Localizer.Get("CreateToggleShortcutButton");
            _statusActionButton.Text = _services.Localizer.Get("UpdateStatusButton");

            if (refreshIconChoices)
            {
                RefreshAvailableIcons(preserveSelections: true);
            }

            if (_availableUpdate is not null)
            {
                _statusActionButton.Visible = true;
            }

            UpdatePageHeader();
            PerformLayout();
        });
    }

    private void ApplyTrayText()
    {
        var appName = _services.Localizer.Get("AppName");
        _trayNotifications.SetTitle(appName);
        _trayToggleMenuItem.Text = _services.Localizer.Get("MenuToggleNow");
        _trayOpenSettingsMenuItem.Text = _services.Localizer.Get("MenuOpenSettings");
        _trayExitMenuItem.Text = _services.Localizer.Get("MenuExit");
        _notifyIcon.Text = appName;
    }

    private void ApplyTheme(AppThemeMode themeMode)
    {
        _activePalette = ThemeManager.Apply(this, themeMode);
        _trayNotifications.SetThemeMode(themeMode);
        _brandLabel.ForeColor = _activePalette.Text;
        _pageTitleLabel.ForeColor = _activePalette.Text;
        _statusLabel.ForeColor = _lastStatusWasError ? _activePalette.ErrorText : _activePalette.SuccessText;
    }

    private void UpdatePageHeader()
    {
        if (_currentPage == PageKind.Main)
        {
            _pageTitleLabel.Text = _services.Localizer.Get("AppName");
            return;
        }

        _pageTitleLabel.Text = _services.Localizer.Get("SettingsTab");
    }

    private void SetSidebarVisible(bool visible)
    {
        if (_shellLayout is null)
        {
            _sidebarVisible = visible;
            return;
        }

        if (_sidebarVisible == visible && _sidebarCard.Visible == visible)
        {
            return;
        }

        _sidebarVisible = visible;
        _shellLayout.SuspendLayout();

        try
        {
            _sidebarCard.Visible = visible;
            _shellLayout.ColumnStyles[0].Width = visible ? 176F : 0F;
        }
        finally
        {
            _shellLayout.ResumeLayout(performLayout: true);
        }
    }

    private void SetThemeSelection(AppThemeMode themeMode)
    {
        _systemThemeRadioButton.Checked = themeMode == AppThemeMode.System;
        _lightThemeRadioButton.Checked = themeMode == AppThemeMode.Light;
        _darkThemeRadioButton.Checked = themeMode == AppThemeMode.Dark;
    }

    private AppThemeMode GetSelectedThemeMode()
    {
        if (_lightThemeRadioButton.Checked)
        {
            return AppThemeMode.Light;
        }

        if (_darkThemeRadioButton.Checked)
        {
            return AppThemeMode.Dark;
        }

        return AppThemeMode.System;
    }

    private void SetActivePage(PageKind page, bool updateRadioSelection = false)
    {
        _pageHost.SuspendLayout();

        try
        {
            _currentPage = page;
            _mainPage.Visible = page == PageKind.Main;
            _settingsPage.Visible = page == PageKind.Settings;

            if (updateRadioSelection)
            {
                _mainNavButton.Checked = page == PageKind.Main;
                _settingsNavButton.Checked = page == PageKind.Settings;
            }

            if (page == PageKind.Main)
            {
                _mainPage.BringToFront();
            }
            else
            {
                _settingsPage.BringToFront();
            }

            _pageHost.AutoScrollPosition = Point.Empty;
            UpdatePageHeader();
            ApplyTheme(GetSelectedThemeMode());
        }
        finally
        {
            _pageHost.ResumeLayout(performLayout: true);
        }
    }

    private void NavigateToPage(PageKind page)
    {
        if (_shellLayout is not null)
        {
            _shellLayout.SuspendLayout();
        }

        try
        {
            SetActivePage(page);
            SetSidebarVisible(false);
        }
        finally
        {
            if (_shellLayout is not null)
            {
                _shellLayout.ResumeLayout(performLayout: true);
            }
        }
    }

    private void HandleLanguageChanged()
    {
        if (_isLoading || _languageComboBox.SelectedValue is not AppLanguage language)
        {
            return;
        }

        _services.Localizer.SetLanguage(language);
        ApplyLanguage();
        ApplyTheme(GetSelectedThemeMode());
        AutoSaveCurrentSelections(_services.Localizer.Get("StatusLanguageChanged"));
    }

    private void HandleThemeChanged(AppThemeMode themeMode)
    {
        if (_isLoading)
        {
            return;
        }

        ApplyTheme(themeMode);
        AutoSaveCurrentSelections(_services.Localizer.Get("StatusThemeChanged"));
    }

    private void SafeLoadInitialState()
    {
        try
        {
            LoadInitialState();
            BeginUpdateCheck();

            if (!_startHiddenToTrayRequested && !_isHiddenToTray)
            {
                BeginInvoke(new Action(RevealWindow));
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.Load", ex);
            var message =
                _services.Localizer.Get("ErrorAppCouldNotLoad") +
                "\n\n" +
                ex.Message +
                $"\n\nLog file: {AppLogger.LatestLogPath}";
            RevealWindow();
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadInitialState()
    {
        _isLoading = true;

        try
        {
            _languageComboBox.SelectedValue = _services.Localizer.CurrentLanguage;
            SetThemeSelection(AppThemeMode.System);
            ApplyLanguage();
            ApplyTheme(AppThemeMode.System);
            RefreshDeviceLists(preserveSelections: false);
            RefreshAvailableIcons(preserveSelections: false);

            if (_services.ConfigStore.TryLoad(out var config, out _))
            {
                _services.Localizer.SetLanguage(config!.Language);
                _languageComboBox.SelectedValue = config.Language;
                ApplyLanguage();
                SetThemeSelection(config.Theme);
                ApplyTheme(config.Theme);
                ApplySavedConfig(config);
                SetStatus(_services.Localizer.Get("LoadedSavedSettings"));

                if (_launchedFromStartup && config.StartMinimizedAtStartup)
                {
                    WindowState = FormWindowState.Minimized;
                    _startHiddenToTrayRequested = true;
                    BeginInvoke(new Action(() => HideToTray(showNotification: false)));
                }
            }
            else
            {
                SelectFirstTwoDevices();
                SetStatus(_services.Localizer.Get("NoSavedSettings"));
            }

            UpdateHotkeyControls();
            UpdateStartupControls();
            UpdateTrayHotkeyRegistration();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void RefreshDeviceLists(bool preserveSelections)
    {
        RunWithoutAutoSave(() =>
        {
            var selectedPrimaryId = preserveSelections ? _primaryDeviceComboBox.SelectedValue as string : null;
            var selectedSecondaryId = preserveSelections ? _secondaryDeviceComboBox.SelectedValue as string : null;

            var devices = _services.AudioDeviceService.GetPlaybackDevices().ToList();
            _primaryDeviceComboBox.DataSource = devices.ToList();
            _secondaryDeviceComboBox.DataSource = devices.ToList();

            if (!string.IsNullOrWhiteSpace(selectedPrimaryId))
            {
                SelectDevice(_primaryDeviceComboBox, selectedPrimaryId);
            }

            if (!string.IsNullOrWhiteSpace(selectedSecondaryId))
            {
                SelectDevice(_secondaryDeviceComboBox, selectedSecondaryId);
            }

            if (_primaryDeviceComboBox.SelectedIndex < 0 || _secondaryDeviceComboBox.SelectedIndex < 0)
            {
                SelectFirstTwoDevices();
            }
        });
    }

    private void ApplySavedConfig(AppConfig config)
    {
        _startWithWindowsCheckBox.Checked = config.StartWithWindows;
        _startMinimizedAtStartupCheckBox.Checked = config.StartMinimizedAtStartup;
        _minimizeToTrayOnCloseCheckBox.Checked = config.MinimizeToTrayOnClose;
        SelectDevice(_primaryDeviceComboBox, config.PrimaryDevice.Id);
        SelectDevice(_secondaryDeviceComboBox, config.SecondaryDevice.Id);
        SelectIcon(_primaryIconComboBox, config.PrimaryIconFileName);
        SelectIcon(_secondaryIconComboBox, config.SecondaryIconFileName);
        _enableHotkeyCheckBox.Checked = config.Hotkey.Enabled;
        _controlCheckBox.Checked = config.Hotkey.Control;
        _altCheckBox.Checked = config.Hotkey.Alt;
        _shiftCheckBox.Checked = config.Hotkey.Shift;
        _windowsCheckBox.Checked = config.Hotkey.WindowsKey;
        _hotkeyComboBox.SelectedValue = config.Hotkey.Key;
    }

    private void SelectFirstTwoDevices()
    {
        if (_primaryDeviceComboBox.Items.Count > 0)
        {
            _primaryDeviceComboBox.SelectedIndex = 0;
        }

        if (_secondaryDeviceComboBox.Items.Count > 1)
        {
            _secondaryDeviceComboBox.SelectedIndex = 1;
        }
        else if (_secondaryDeviceComboBox.Items.Count > 0)
        {
            _secondaryDeviceComboBox.SelectedIndex = 0;
        }
    }

    private static void SelectDevice(ComboBox comboBox, string deviceId)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is AudioDeviceInfo device &&
                string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static void SelectIcon(ComboBox comboBox, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var normalizedFileName = NotificationIconCatalog.NormalizeFileName(fileName);

        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is NotificationIconCatalog.IconChoice choice &&
                string.Equals(choice.FileName, normalizedFileName, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private void RefreshAvailableIcons(bool preserveSelections, bool showStatus = false)
    {
        RunWithoutAutoSave(() =>
        {
            var selectedPrimary = preserveSelections
                ? _primaryIconComboBox.SelectedValue as string
                : AppConfig.DefaultIconFileName;
            var selectedSecondary = preserveSelections
                ? _secondaryIconComboBox.SelectedValue as string
                : AppConfig.DefaultIconFileName;

            var icons = NotificationIconCatalog.GetSelectableIcons(_services.Localizer).ToList();
            _primaryIconComboBox.DataSource = icons.ToList();
            _secondaryIconComboBox.DataSource = icons.ToList();

            var hasIcons = icons.Count > 0;
            _primaryIconComboBox.Enabled = hasIcons;
            _secondaryIconComboBox.Enabled = hasIcons;

            if (!hasIcons)
            {
                SetPictureBoxImage(_primaryIconPreview, null);
                SetPictureBoxImage(_secondaryIconPreview, null);

                if (showStatus)
                {
                    SetStatus(_services.Localizer.Get("StatusNoIconsFound"), isError: true);
                }

                return;
            }

            SelectIcon(_primaryIconComboBox, selectedPrimary);
            SelectIcon(_secondaryIconComboBox, selectedSecondary);

            if (_primaryIconComboBox.SelectedIndex < 0)
            {
                _primaryIconComboBox.SelectedIndex = 0;
            }

            if (_secondaryIconComboBox.SelectedIndex < 0)
            {
                _secondaryIconComboBox.SelectedIndex = Math.Min(1, _secondaryIconComboBox.Items.Count - 1);
            }

            UpdateDeviceIconPreviews();

            if (showStatus)
            {
                SetStatus(_services.Localizer.Format("StatusIconsRefreshed", icons.Count));
            }
        });
    }

    private void OpenIconFolder()
    {
        try
        {
            NotificationIconCatalog.EnsureUserIconFolderInitialized();

            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{NotificationIconCatalog.UserIconDirectoryPath}\"")
            {
                UseShellExecute = true
            });

            SetStatus(_services.Localizer.Get("StatusOpenedIconFolder"));
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateDeviceIconPreviews()
    {
        UpdateDeviceIconPreview(_primaryIconComboBox, _primaryIconPreview);
        UpdateDeviceIconPreview(_secondaryIconComboBox, _secondaryIconPreview);
    }

    private static void UpdateDeviceIconPreview(ComboBox comboBox, PictureBox previewBox)
    {
        var fileName = comboBox.SelectedValue as string;
        var path = NotificationIconCatalog.ResolvePath(fileName);
        SetPictureBoxImage(previewBox, NotificationIconCatalog.LoadImage(path));
    }

    private static void SetPictureBoxImage(PictureBox pictureBox, Image? image)
    {
        var previousImage = pictureBox.Image;
        pictureBox.Image = image;
        previousImage?.Dispose();
    }

    private void UpdateHotkeyControls()
    {
        var enabled = _enableHotkeyCheckBox.Checked;
        _controlCheckBox.Enabled = enabled;
        _altCheckBox.Enabled = enabled;
        _shiftCheckBox.Enabled = enabled;
        _windowsCheckBox.Enabled = enabled;
        _hotkeyComboBox.Enabled = enabled;
    }

    private void UpdateStartupControls()
    {
        _startMinimizedAtStartupCheckBox.Enabled = _startWithWindowsCheckBox.Checked;
    }

    private void AutoSaveCurrentSelections(string? successStatus = null)
    {
        if (_isLoading || _suppressAutoSave)
        {
            return;
        }

        PersistCurrentSelections(
            showValidationUi: false,
            showExceptionUi: false,
            successStatus ?? _services.Localizer.Get("StatusSettingsSaved"));
    }

    private void RunWithoutAutoSave(Action action)
    {
        var previousState = _suppressAutoSave;
        _suppressAutoSave = true;

        try
        {
            action();
        }
        finally
        {
            _suppressAutoSave = previousState;
        }
    }

    private void HandleRestoreRequest()
    {
        if (IsDisposed)
        {
            return;
        }

        if (_isHiddenToTray)
        {
            RestoreFromTray();
            return;
        }

        if (!Visible)
        {
            Show();
        }

        RevealWindow();
    }

    private void HandleFormClosing(FormClosingEventArgs eventArgs)
    {
        if (_allowClose || _isHiddenToTray)
        {
            if (_allowClose)
            {
                PrepareForHiddenState();
            }

            return;
        }

        if (eventArgs.CloseReason is CloseReason.ApplicationExitCall or CloseReason.WindowsShutDown or CloseReason.TaskManagerClosing)
        {
            PrepareForHiddenState();
            return;
        }

        if (!_minimizeToTrayOnCloseCheckBox.Checked)
        {
            PrepareForHiddenState();
            return;
        }

        PrepareForHiddenState();
        eventArgs.Cancel = true;
        HideToTray(showNotification: true);
    }

    private void HideToTray(bool showNotification)
    {
        if (_isHiddenToTray)
        {
            return;
        }

        _startHiddenToTrayRequested = false;
        _isHiddenToTray = true;
        PrepareForHiddenState();
        UpdateTrayHotkeyRegistration(showErrorNotification: showNotification);

        if (showNotification)
        {
            _trayNotifications.Show(
                _services.Localizer.Get("StatusMinimizedToTray"),
                ToolTipIcon.Info,
                imagePath: NotificationIconCatalog.ResolvePath(AppConfig.DefaultIconFileName));
        }
    }

    private void RestoreFromTray()
    {
        if (!_isHiddenToTray)
        {
            RevealWindow();
            return;
        }

        _isHiddenToTray = false;
        RevealWindow();
        UpdateTrayHotkeyRegistration();

        if (_availableUpdate is not null && !_updateDialogShown)
        {
            _updateDialogShown = true;
            BeginInvoke(new Action(ShowAvailableUpdateDialog));
        }
    }

    private void ExitFromTray()
    {
        _allowClose = true;
        _notifyIcon.Visible = false;
        PrepareForHiddenState();
        Close();
    }

    private void RevealWindow()
    {
        if (IsDisposed)
        {
            return;
        }

        SuspendLayout();

        try
        {
            _allowFormVisibility = true;
            ShowInTaskbar = true;

            if (!Visible)
            {
                Show();
            }

            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            BringToFront();
            Activate();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }
    }

    private void PrepareForHiddenState()
    {
        if (IsDisposed)
        {
            return;
        }

        SuspendLayout();

        try
        {
            _allowFormVisibility = false;
            ShowInTaskbar = false;

            if (Visible)
            {
                Hide();
            }
        }
        finally
        {
            ResumeLayout(performLayout: false);
        }
    }

    private bool UpdateTrayHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _trayHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _trayHotkey.Unregister();
            return false;
        }

        _services.Localizer.SetLanguage(config.Language);
        ApplyTrayText();
        _trayNotifications.SetThemeMode(config.Theme);

        if (!config.Hotkey.Enabled)
        {
            _trayHotkey.Unregister();
            return true;
        }

        if (_trayHotkey.Register(config.Hotkey, out var errorMessage))
        {
            return true;
        }

        SetStatus(errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"), isError: true);

        if (showErrorNotification)
        {
            _trayNotifications.Show(
                errorMessage ?? _services.Localizer.Get("ErrorGlobalHotkeyUnavailable"),
                ToolTipIcon.Warning,
                3000);
        }

        return false;
    }

    private void ToggleUsingSavedSettings(bool showFailureDialog)
    {
        if (!_services.ConfigStore.TryLoad(out var config, out var errorMessage))
        {
            var message = errorMessage ?? _services.Localizer.Get("NotifyNoConfig");
            _trayNotifications.Show(message, ToolTipIcon.Warning, 3000);

            if (showFailureDialog && !_isHiddenToTray)
            {
                MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return;
        }

        _services.Localizer.SetLanguage(config!.Language);
        ApplyLanguage(refreshIconChoices: false);
        ApplyTheme(config.Theme);

        var result = _services.AudioDeviceService.Toggle(config);
        if (!_isHiddenToTray)
        {
            SetStatus(result.Message, isError: !result.Success);
        }

        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            imagePath: NotificationIconCatalog.ResolveToggleImagePath(config, result));

        if (!result.Success && showFailureDialog && !_isHiddenToTray)
        {
            MessageBox.Show(this, result.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ToggleUsingCurrentSelections()
    {
        if (!TryBuildConfigFromCurrentSelections(out var config))
        {
            return;
        }

        _services.Localizer.SetLanguage(config!.Language);
        ApplyLanguage(refreshIconChoices: false);
        ApplyTheme(config.Theme);

        var result = _services.AudioDeviceService.Toggle(config);
        SetStatus(result.Message, isError: !result.Success);
        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            imagePath: NotificationIconCatalog.ResolveToggleImagePath(config, result));

        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CreateToggleShortcut()
    {
        if (!PersistCurrentSelections(showValidationUi: true, showExceptionUi: true))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Windows Shortcut (*.lnk)|*.lnk",
            FileName = "SoundDeviceSwitcher Toggle.lnk",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _services.ShortcutManager.CreateToggleShortcut(dialog.FileName);
            SetStatus(_services.Localizer.Format("StatusCreatedShortcut", dialog.FileName));
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);
            MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool PersistCurrentSelections(bool showValidationUi, bool showExceptionUi, string? successStatus = null)
    {
        if (!TryBuildConfigFromCurrentSelections(out var config, showValidationUi))
        {
            return false;
        }

        try
        {
            _services.Localizer.SetLanguage(config!.Language);
            ApplyTrayText();
            _trayNotifications.SetThemeMode(config.Theme);
            _services.ConfigStore.Save(config);
            _services.ShortcutManager.SyncStartupShortcut(config);
            var hotkeyRegistered = UpdateTrayHotkeyRegistration(config);

            if (hotkeyRegistered && !string.IsNullOrWhiteSpace(successStatus))
            {
                SetStatus(successStatus);
            }

            return true;
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, isError: true);

            if (showExceptionUi)
            {
                MessageBox.Show(this, ex.Message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
    }

    private bool TryBuildConfigFromCurrentSelections(out AppConfig? config, bool showValidationUi = true)
    {
        config = null;

        if (_primaryDeviceComboBox.SelectedItem is not AudioDeviceInfo primaryDevice ||
            _secondaryDeviceComboBox.SelectedItem is not AudioDeviceInfo secondaryDevice)
        {
            var message = _services.Localizer.Get("ErrorNeedTwoPlaybackDevices");
            SetStatus(message, isError: true);

            if (showValidationUi)
            {
                MessageBox.Show(this, message, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        config = new AppConfig
        {
            Language = _languageComboBox.SelectedValue is AppLanguage language ? language : _services.Localizer.CurrentLanguage,
            Theme = GetSelectedThemeMode(),
            StartWithWindows = _startWithWindowsCheckBox.Checked,
            StartMinimizedAtStartup = _startWithWindowsCheckBox.Checked && _startMinimizedAtStartupCheckBox.Checked,
            MinimizeToTrayOnClose = _minimizeToTrayOnCloseCheckBox.Checked,
            PrimaryDevice = new DeviceSelection
            {
                Id = primaryDevice.Id,
                Name = primaryDevice.Name
            },
            SecondaryDevice = new DeviceSelection
            {
                Id = secondaryDevice.Id,
                Name = secondaryDevice.Name
            },
            PrimaryIconFileName = _primaryIconComboBox.SelectedValue as string ?? AppConfig.DefaultIconFileName,
            SecondaryIconFileName = _secondaryIconComboBox.SelectedValue as string ?? AppConfig.DefaultIconFileName,
            Hotkey = new HotkeySettings
            {
                Enabled = _enableHotkeyCheckBox.Checked,
                Control = _controlCheckBox.Checked,
                Alt = _altCheckBox.Checked,
                Shift = _shiftCheckBox.Checked,
                WindowsKey = _windowsCheckBox.Checked,
                Key = _hotkeyComboBox.SelectedValue is Keys key ? key : Keys.F10
            }
        };

        _services.Localizer.SetLanguage(config.Language);

        if (!_services.ConfigStore.Validate(config, out var errorMessage))
        {
            SetStatus(errorMessage!, isError: true);

            if (showValidationUi)
            {
                MessageBox.Show(this, errorMessage!, _services.Localizer.Get("AppName"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            config = null;
            return false;
        }

        return true;
    }

    private void SetStatus(string message, bool isError = false)
    {
        _lastStatusWasError = isError;
        _statusLabel.ForeColor = isError ? _activePalette.ErrorText : _activePalette.SuccessText;
        _statusLabel.Text = message;
    }

    private async void BeginUpdateCheck()
    {
        var updateRelease = await _services.UpdateChecker.CheckForUpdateAsync();
        if (updateRelease is null || IsDisposed)
        {
            return;
        }

        ApplyAvailableUpdate(updateRelease);
    }

    private void ApplyAvailableUpdate(UpdateReleaseInfo updateRelease)
    {
        _availableUpdate = updateRelease;
        _statusActionButton.Visible = true;
        _statusActionButton.Text = _services.Localizer.Get("UpdateStatusButton");
        SetStatus(_services.Localizer.Format("StatusUpdateAvailable", updateRelease.VersionDisplay));

        if (_updateDialogShown ||
            _startHiddenToTrayRequested ||
            _isHiddenToTray ||
            !Visible ||
            !ShowInTaskbar ||
            WindowState == FormWindowState.Minimized)
        {
            return;
        }

        _updateDialogShown = true;
        BeginInvoke(new Action(ShowAvailableUpdateDialog));
    }

    private void ShowAvailableUpdateDialog()
    {
        if (_availableUpdate is null || IsDisposed)
        {
            return;
        }

        using var dialog = new UpdateAvailableDialog(
            _services.Localizer,
            _availableUpdate,
            _services.UpdateChecker.CurrentVersionDisplay,
            GetSelectedThemeMode());
        dialog.ShowDialog(this);
    }

    private static List<LanguageChoice> BuildLanguageChoices()
    {
        return
        [
            new LanguageChoice(AppLanguage.English, "English"),
            new LanguageChoice(AppLanguage.Korean, "\uD55C\uAD6D\uC5B4")
        ];
    }

    private static List<KeyOption> BuildKeyOptions()
    {
        var options = new List<KeyOption>();

        for (var key = Keys.F1; key <= Keys.F24; key++)
        {
            options.Add(new KeyOption(key, HotkeyFormatter.FormatKey(key)));
        }

        for (var key = Keys.A; key <= Keys.Z; key++)
        {
            options.Add(new KeyOption(key, HotkeyFormatter.FormatKey(key)));
        }

        for (var key = Keys.D0; key <= Keys.D9; key++)
        {
            options.Add(new KeyOption(key, HotkeyFormatter.FormatKey(key)));
        }

        return options;
    }

    private enum PageKind
    {
        Main,
        Settings
    }

    private sealed class LanguageChoice
    {
        public LanguageChoice(AppLanguage language, string label)
        {
            Language = language;
            Label = label;
        }

        public AppLanguage Language { get; }

        public string Label { get; }
    }

    private sealed class KeyOption
    {
        public KeyOption(Keys value, string label)
        {
            Value = value;
            Label = label;
        }

        public Keys Value { get; }

        public string Label { get; }
    }
}
