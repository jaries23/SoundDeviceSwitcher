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

internal sealed class MainShellForm : Form
{
    private const int RecentAudioStateObservationDebounceMs = 250;
    private const int RecentAudioStateNotificationSuppressionMs = 900;
    private const int SurfaceCornerRadius = 0;

    private readonly AppServices _services;
    private readonly TrayNotificationService _trayNotifications;
    private readonly RoundedPanel _sidebarCard;
    private readonly RoundedPanel _headerCard;
    private readonly RoundedPanel _statusCard;
    private readonly RoundedPanel _mainDeviceCard;
    private readonly RoundedPanel _mainSwitchCard;
    private readonly RoundedPanel _settingsGeneralCard;
    private readonly RoundedPanel _settingsAutomationCard;
    private readonly RoundedPanel _settingsHotkeyCard;
    private readonly RoundedPanel _settingsOverlayCard;
    private readonly RoundedPanel _settingsProfilesCard;
    private readonly RoundedButton _menuButton;
    private readonly Panel _pageHost;
    private readonly Panel _settingsContentHost;
    private readonly TableLayoutPanel _mainPage;
    private readonly TableLayoutPanel _settingsPage;
    private readonly PillRadioButton _mainNavButton;
    private readonly PillRadioButton _settingsNavButton;
    private readonly PillRadioButton _settingsGeneralTabButton;
    private readonly PillRadioButton _settingsAutomationTabButton;
    private readonly PillRadioButton _settingsShortcutsTabButton;
    private readonly PillRadioButton _settingsOverlayTabButton;
    private readonly PillRadioButton _settingsProfilesTabButton;
    private readonly Label _brandLabel;
    private readonly Label _pageTitleLabel;
    private readonly Label _deviceCardTitleLabel;
    private readonly Label _switchCardTitleLabel;
    private readonly Label _generalTitleLabel;
    private readonly Label _automationTitleLabel;
    private readonly Label _hotkeyTitleLabel;
    private readonly Label _overlayTitleLabel;
    private readonly Label _profilesTitleLabel;
    private readonly Label _languageLabel;
    private readonly Label _themeLabel;
    private readonly CheckBox _startWithWindowsCheckBox;
    private readonly CheckBox _startMinimizedAtStartupCheckBox;
    private readonly CheckBox _minimizeToTrayOnCloseCheckBox;
    private readonly CheckBox _enableUpdateNotificationsCheckBox;
    private readonly CheckBox _syncCommunicationDeviceWithPlaybackCheckBox;
    private readonly CheckBox _autoSwitchToNewPlaybackDeviceCheckBox;
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
    private readonly PictureBox _profileIconPreview;
    private readonly ComboBox _primaryDeviceComboBox;
    private readonly ComboBox _secondaryDeviceComboBox;
    private readonly CheckBox _enableHotkeyCheckBox;
    private readonly Label _switchShortcutSectionLabel;
    private readonly Label _recentSwitchUndoSectionLabel;
    private readonly CheckBox _enableRecentSwitchUndoHotkeyCheckBox;
    private readonly Label _modifiersLabel;
    private readonly Label _keyLabel;
    private readonly Label _recentSwitchUndoModifiersLabel;
    private readonly Label _recentSwitchUndoKeyLabel;
    private readonly Label _profileNameLabel;
    private readonly Label _profileIconLabel;
    private readonly Label _profilePriorityLabel;
    private readonly Label _profileOutputDeviceLabel;
    private readonly Label _profileInputDeviceLabel;
    private readonly Label _profileProgramsLabel;
    private readonly Label _profileOrderHintLabel;
    private readonly Label _profileAutomationHintLabel;
    private readonly CheckBox _enableProfilesCheckBox;
    private readonly CheckBox _enableOverlayHotkeyCheckBox;
    private readonly CheckBox _controlCheckBox;
    private readonly CheckBox _altCheckBox;
    private readonly CheckBox _shiftCheckBox;
    private readonly CheckBox _windowsCheckBox;
    private readonly CheckBox _recentSwitchUndoControlCheckBox;
    private readonly CheckBox _recentSwitchUndoAltCheckBox;
    private readonly CheckBox _recentSwitchUndoShiftCheckBox;
    private readonly CheckBox _recentSwitchUndoWindowsCheckBox;
    private readonly CheckBox _overlayControlCheckBox;
    private readonly CheckBox _overlayAltCheckBox;
    private readonly CheckBox _overlayShiftCheckBox;
    private readonly CheckBox _overlayWindowsCheckBox;
    private readonly ComboBox _hotkeyComboBox;
    private readonly ComboBox _recentSwitchUndoHotkeyComboBox;
    private readonly ComboBox _overlayHotkeyComboBox;
    private readonly ComboBox _profileIconComboBox;
    private readonly ComboBox _profileOutputDeviceComboBox;
    private readonly ComboBox _profileInputDeviceComboBox;
    private readonly Label _hotkeyNoteLabel;
    private readonly Label _recentSwitchUndoNoteLabel;
    private readonly Label _overlayModifiersLabel;
    private readonly Label _overlayKeyLabel;
    private readonly Label _overlayShortcutSectionLabel;
    private readonly Label _overlayHeightLabel;
    private readonly Label _overlayPositionLabel;
    private readonly Label _overlayLayoutLabel;
    private readonly Label _overlayHintLabel;
    private readonly Label _statusLabel;
    private readonly RoundedButton _statusActionButton;
    private readonly RoundedButton _toggleNowButton;
    private readonly RoundedButton _refreshDevicesButton;
    private readonly RoundedButton _openIconFolderButton;
    private readonly RoundedButton _refreshIconsButton;
    private readonly RoundedButton _createShortcutButton;
    private readonly RoundedButton _openProfileOverlayButton;
    private readonly RoundedButton _addProfileButton;
    private readonly RoundedButton _removeProfileButton;
    private readonly RoundedButton _moveProfileUpButton;
    private readonly RoundedButton _moveProfileDownButton;
    private readonly RoundedButton _addProfileProgramButton;
    private readonly RoundedButton _removeProfileProgramButton;
    private readonly RadioButton _overlayHorizontalLayoutRadioButton;
    private readonly RadioButton _overlayVerticalLayoutRadioButton;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly ToolStripMenuItem _trayToggleMenuItem;
    private readonly ToolStripMenuItem _trayOpenSettingsMenuItem;
    private readonly ToolStripMenuItem _trayExitMenuItem;
    private readonly GlobalHotkey _trayHotkey;
    private readonly GlobalHotkey _recentSwitchUndoHotkey;
    private readonly GlobalHotkey _overlayHotkey;
    private readonly Icon _windowIcon;
    private readonly Icon _trayIcon;
    private readonly EventWaitHandle _restoreEvent;
    private readonly RegisteredWaitHandle _restoreWaitRegistration;
    private readonly ProcessProfileMatcher _processProfileMatcher = new();
    private readonly WinEventDelegate _foregroundChangedCallback;
    private readonly System.Windows.Forms.Timer _profileEvaluationTimer;
    private readonly System.Windows.Forms.Timer _recentAudioStateObservationTimer;
    private readonly System.Windows.Forms.Timer _recentUndoWindowTimer;
    private readonly RecentAudioStateTracker _recentAudioStateTracker = new();
    private readonly List<ProcessAudioProfile> _editableProfiles = [];
    private readonly Dictionary<ProfileOverlayAnchor, RadioButton> _overlayAnchorButtons = [];
    private readonly bool _launchedFromStartup;
    private readonly bool _launchedFromPostInstall;
    private readonly DateTime _postInstallStartedAtUtc;
    private ThemePalette _activePalette = ThemeManager.ResolvePalette(AppThemeMode.System);
    private UpdateReleaseInfo? _availableUpdate;
    private PageKind _currentPage = PageKind.Main;
    private SettingsSection _currentSettingsSection = SettingsSection.General;
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
    private bool _recentSwitchUndoHotkeySelectionChangedWhileDroppedDown;
    private bool _overlayHotkeySelectionChangedWhileDroppedDown;
    private bool _initialStateLoaded;
    private bool _postInstallCloseGuardPending;
    private bool _updateCheckInProgress;
    private bool _profileEvaluationInProgress;
    private bool _suppressProfileEditorEvents;
    private bool _suppressProfileListSelectionEvents;
    private AudioDeviceNotificationMonitor? _audioDeviceNotificationMonitor;
    private AppConfig? _cachedConfig;
    private string? _activeProcessProfileId;
    private string? _activeProcessProfilePlaybackDeviceId;
    private string? _activeProcessProfileRecordingDeviceId;
    private DateTime _ignoreObservedAudioDeviceChangesUntilUtc;
    private IntPtr _foregroundChangeHook;
    private TableLayoutPanel? _shellLayout;
    private ListBox? _profilesListBox;
    private ListBox? _profileProgramsListBox;
    private TextBox? _profileNameTextBox;
    private TextBox? _profileOrderTextBox;
    private NumericUpDown? _overlayHeightNumericUpDown;
    private ProfileOverlayForm? _profileOverlayForm;

    public MainShellForm(AppServices services, bool launchedFromStartup = false, bool launchedFromPostInstall = false)
    {
        _services = services;
        _trayNotifications = new TrayNotificationService(_services.Localizer.Get("AppName"));
        _launchedFromStartup = launchedFromStartup;
        _launchedFromPostInstall = launchedFromPostInstall;
        _postInstallStartedAtUtc = DateTime.UtcNow;
        _postInstallCloseGuardPending = launchedFromPostInstall;
        _foregroundChangedCallback = HandleForegroundChanged;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        DoubleBuffered = true;
        _allowFormVisibility = launchedFromPostInstall;
        ShowInTaskbar = launchedFromPostInstall;
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
        _recentSwitchUndoHotkey = new GlobalHotkey();
        _recentSwitchUndoHotkey.Pressed += (_, _) => UndoRecentAudioDeviceChange();
        _overlayHotkey = new GlobalHotkey();
        _overlayHotkey.Pressed += (_, _) => ToggleProfileOverlay();
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

        _sidebarCard = CreateCard("sidebar", autoSize: false, dock: DockStyle.Fill, cornerRadius: SurfaceCornerRadius);
        _headerCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _statusCard = CreateCard("soft", autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _mainDeviceCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _mainSwitchCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsGeneralCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsAutomationCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsHotkeyCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsOverlayCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsProfilesCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _sidebarCard.Padding = new Padding(16);
        _headerCard.Padding = new Padding(14, 12, 14, 12);
        _statusCard.Padding = new Padding(14, 10, 14, 10);
        _mainDeviceCard.Padding = new Padding(18);
        _mainSwitchCard.Padding = new Padding(18);
        _settingsGeneralCard.Padding = new Padding(18);
        _settingsAutomationCard.Padding = new Padding(18);
        _settingsHotkeyCard.Padding = new Padding(18);
        _settingsOverlayCard.Padding = new Padding(18);
        _settingsProfilesCard.Padding = new Padding(18);
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
        _settingsGeneralTabButton = CreateSettingsTabButton();
        _settingsGeneralTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsGeneralTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.General);
            }
        };
        _settingsAutomationTabButton = CreateSettingsTabButton();
        _settingsAutomationTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsAutomationTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.Automation);
            }
        };
        _settingsShortcutsTabButton = CreateSettingsTabButton();
        _settingsShortcutsTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsShortcutsTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.Shortcuts);
            }
        };
        _settingsOverlayTabButton = CreateSettingsTabButton();
        _settingsOverlayTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsOverlayTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.Overlay);
            }
        };
        _settingsProfilesTabButton = CreateSettingsTabButton();
        _settingsProfilesTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsProfilesTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.Profiles);
            }
        };

        _pageTitleLabel = new Label { AutoSize = true, Font = new Font("Segoe UI Semibold", 19.5F, FontStyle.Bold, GraphicsUnit.Point), Margin = new Padding(0) };

        _deviceCardTitleLabel = CreateSectionTitleLabel();
        _switchCardTitleLabel = CreateSectionTitleLabel();
        _generalTitleLabel = CreateSectionTitleLabel();
        _automationTitleLabel = CreateSectionTitleLabel();
        _hotkeyTitleLabel = CreateSectionTitleLabel();
        _overlayTitleLabel = CreateSectionTitleLabel();
        _profilesTitleLabel = CreateSectionTitleLabel();

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
        _enableUpdateNotificationsCheckBox = new CheckBox { AutoSize = true, Checked = true };
        _enableUpdateNotificationsCheckBox.CheckedChanged += (_, _) => HandleUpdateNotificationsChanged();
        _syncCommunicationDeviceWithPlaybackCheckBox = new CheckBox { AutoSize = true };
        _syncCommunicationDeviceWithPlaybackCheckBox.CheckedChanged += (_, _) => HandleCommunicationDeviceSyncChanged();
        _autoSwitchToNewPlaybackDeviceCheckBox = new CheckBox { AutoSize = true };
        _autoSwitchToNewPlaybackDeviceCheckBox.CheckedChanged += (_, _) => HandleAutoSwitchToNewPlaybackDeviceChanged();
        _deviceAIconLabel = CreateFieldLabel();
        _deviceBIconLabel = CreateFieldLabel();
        _deviceALabel = CreateFieldLabel();
        _deviceBLabel = CreateFieldLabel();
        _switchShortcutSectionLabel = CreateFieldLabel();
        _switchShortcutSectionLabel.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        _recentSwitchUndoSectionLabel = CreateFieldLabel();
        _recentSwitchUndoSectionLabel.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        _modifiersLabel = CreateFieldLabel();
        _keyLabel = CreateFieldLabel();
        _recentSwitchUndoModifiersLabel = CreateFieldLabel();
        _recentSwitchUndoKeyLabel = CreateFieldLabel();
        _profileNameLabel = CreateFieldLabel();
        _profileIconLabel = CreateFieldLabel();
        _profilePriorityLabel = CreateFieldLabel();
        _profileOutputDeviceLabel = CreateFieldLabel();
        _profileInputDeviceLabel = CreateFieldLabel();
        _profileProgramsLabel = CreateFieldLabel();
        _profileOrderHintLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 40,
            Margin = new Padding(0, 8, 0, 0),
            TextAlign = ContentAlignment.TopLeft
        };
        _profileAutomationHintLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 40,
            Margin = new Padding(0, 8, 0, 0),
            TextAlign = ContentAlignment.TopLeft
        };
        _enableProfilesCheckBox = new CheckBox
        {
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 12),
            Checked = true
        };
        _enableProfilesCheckBox.CheckedChanged += (_, _) => HandleProfilesFeatureChanged();
        _enableOverlayHotkeyCheckBox = new CheckBox
        {
            AutoSize = true
        };
        _enableOverlayHotkeyCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateOverlayHotkeyControls();
            AutoSaveCurrentSelections();
        };
        _overlayHorizontalLayoutRadioButton = CreateOverlayLayoutRadioButton();
        _overlayHorizontalLayoutRadioButton.CheckedChanged += (_, _) =>
        {
            if (_overlayHorizontalLayoutRadioButton.Checked)
            {
                PersistOverlayPresentationSelections();
            }
        };
        _overlayVerticalLayoutRadioButton = CreateOverlayLayoutRadioButton();
        _overlayVerticalLayoutRadioButton.CheckedChanged += (_, _) =>
        {
            if (_overlayVerticalLayoutRadioButton.Checked)
            {
                PersistOverlayPresentationSelections();
            }
        };
        _overlayAnchorButtons[ProfileOverlayAnchor.TopLeft] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.TopCenter] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.TopRight] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.MiddleLeft] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.Center] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.MiddleRight] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.BottomLeft] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.BottomCenter] = CreateOverlayAnchorButton();
        _overlayAnchorButtons[ProfileOverlayAnchor.BottomRight] = CreateOverlayAnchorButton();
        foreach (var button in _overlayAnchorButtons.Values)
        {
            button.CheckedChanged += (_, _) =>
            {
                if (button.Checked)
                {
                    PersistOverlayPresentationSelections();
                }
            };
        }

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
        _profileIconPreview = CreateIconPreview();
        _primaryIconComboBox = CreateIconComboBox();
        _secondaryIconComboBox = CreateIconComboBox();
        _profileIconComboBox = CreateIconComboBox();
        _primaryIconComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateDeviceIconPreview(_primaryIconComboBox, _primaryIconPreview);
            if (_secondaryIconComboBox.Items.Count == _primaryIconComboBox.Items.Count &&
                _primaryIconComboBox.SelectedIndex >= 0)
            {
                _secondaryIconComboBox.SelectedIndex = _primaryIconComboBox.SelectedIndex;
                UpdateDeviceIconPreview(_secondaryIconComboBox, _secondaryIconPreview);
            }

            AutoSaveCurrentSelections();
        };
        _secondaryIconComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateDeviceIconPreview(_secondaryIconComboBox, _secondaryIconPreview);
            if (_secondaryIconComboBox.Parent is not null)
            {
                AutoSaveCurrentSelections();
            }
        };
        _profileIconComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateDeviceIconPreview(_profileIconComboBox, _profileIconPreview);
            HandleSelectedProfileChanged();
        };

        _enableHotkeyCheckBox = new CheckBox { AutoSize = true };
        _enableHotkeyCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateHotkeyControls();
            AutoSaveCurrentSelections();
        };
        _enableRecentSwitchUndoHotkeyCheckBox = new CheckBox { AutoSize = true };
        _enableRecentSwitchUndoHotkeyCheckBox.CheckedChanged += (_, _) =>
        {
            UpdateRecentSwitchUndoHotkeyControls();
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
        _recentSwitchUndoControlCheckBox = new CheckBox { AutoSize = true, Text = "Ctrl" };
        _recentSwitchUndoControlCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _recentSwitchUndoAltCheckBox = new CheckBox { AutoSize = true, Text = "Alt" };
        _recentSwitchUndoAltCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _recentSwitchUndoShiftCheckBox = new CheckBox { AutoSize = true, Text = "Shift" };
        _recentSwitchUndoShiftCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _recentSwitchUndoWindowsCheckBox = new CheckBox { AutoSize = true, Text = "Win" };
        _recentSwitchUndoWindowsCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _overlayControlCheckBox = new CheckBox { AutoSize = true, Text = "Ctrl" };
        _overlayControlCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _overlayAltCheckBox = new CheckBox { AutoSize = true, Text = "Alt" };
        _overlayAltCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _overlayShiftCheckBox = new CheckBox { AutoSize = true, Text = "Shift" };
        _overlayShiftCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _overlayWindowsCheckBox = new CheckBox { AutoSize = true, Text = "Win" };
        _overlayWindowsCheckBox.CheckedChanged += (_, _) => AutoSaveCurrentSelections();
        _hotkeyComboBox = CreateKeyComboBox();
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
        _recentSwitchUndoHotkeyComboBox = CreateKeyComboBox();
        _recentSwitchUndoHotkeyComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (_recentSwitchUndoHotkeyComboBox.DroppedDown)
            {
                _recentSwitchUndoHotkeySelectionChangedWhileDroppedDown = true;
                return;
            }

            AutoSaveCurrentSelections();
        };
        _recentSwitchUndoHotkeyComboBox.DropDownClosed += (_, _) =>
        {
            if (!_recentSwitchUndoHotkeySelectionChangedWhileDroppedDown)
            {
                return;
            }

            _recentSwitchUndoHotkeySelectionChangedWhileDroppedDown = false;
            AutoSaveCurrentSelections();
        };
        _overlayHotkeyComboBox = CreateKeyComboBox();
        _overlayHotkeyComboBox.SelectedIndexChanged += (_, _) =>
        {
            if (_overlayHotkeyComboBox.DroppedDown)
            {
                _overlayHotkeySelectionChangedWhileDroppedDown = true;
                return;
            }

            AutoSaveCurrentSelections();
        };
        _overlayHotkeyComboBox.DropDownClosed += (_, _) =>
        {
            if (!_overlayHotkeySelectionChangedWhileDroppedDown)
            {
                return;
            }

            _overlayHotkeySelectionChangedWhileDroppedDown = false;
            AutoSaveCurrentSelections();
        };
        _profileOutputDeviceComboBox = CreateProfileDeviceComboBox();
        _profileOutputDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedProfileChanged();
        _profileInputDeviceComboBox = CreateProfileDeviceComboBox();
        _profileInputDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedProfileChanged();
        _hotkeyNoteLabel = new Label
        {
            AutoSize = false,
            Height = 52,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };
        _recentSwitchUndoNoteLabel = new Label
        {
            AutoSize = false,
            Height = 44,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };
        _overlayModifiersLabel = CreateFieldLabel();
        _overlayKeyLabel = CreateFieldLabel();
        _overlayShortcutSectionLabel = CreateFieldLabel();
        _overlayShortcutSectionLabel.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        _overlayHeightLabel = CreateFieldLabel();
        _overlayPositionLabel = CreateFieldLabel();
        _overlayLayoutLabel = CreateFieldLabel();
        _overlayHintLabel = new Label
        {
            AutoSize = false,
            Height = 44,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 0)
        };
        _profileNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4)
        };
        _profileNameTextBox.TextChanged += (_, _) => HandleSelectedProfileChanged();
        _profileOrderTextBox = new TextBox
        {
            Dock = DockStyle.Left,
            Width = 120,
            Margin = new Padding(0, 4, 0, 4),
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        _profilesListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 160,
            Margin = new Padding(0),
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        _profilesListBox.SelectedIndexChanged += (_, _) => HandleProfileSelectionChanged();
        _profileProgramsListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 92,
            Margin = new Padding(0, 4, 0, 4),
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        _profileProgramsListBox.SelectedIndexChanged += (_, _) => UpdateProfileProgramButtons();

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
        _addProfileButton = CreateActionButton("secondary", new Size(120, 38));
        _addProfileButton.Click += (_, _) => AddProfile();
        _removeProfileButton = CreateActionButton("secondary", new Size(120, 38));
        _removeProfileButton.Click += (_, _) => RemoveSelectedProfile();
        _moveProfileUpButton = CreateIconActionButton("↑");
        _moveProfileUpButton.Click += (_, _) => MoveSelectedProfile(-1);
        _moveProfileDownButton = CreateIconActionButton("↓");
        _moveProfileDownButton.Click += (_, _) => MoveSelectedProfile(1);
        _addProfileProgramButton = CreateActionButton("secondary", new Size(120, 38));
        _addProfileProgramButton.Click += (_, _) => AddProgramToSelectedProfile();
        _removeProfileProgramButton = CreateActionButton("secondary", new Size(120, 38));
        _removeProfileProgramButton.Click += (_, _) => RemoveSelectedProgramFromProfile();
        _openProfileOverlayButton = CreateActionButton("secondary", new Size(148, 40));
        _openProfileOverlayButton.Click += (_, _) => ToggleProfileOverlay();

        _pageHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 10, 0, 0)
        };
        _settingsContentHost = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Margin = new Padding(0)
        };
        ApplyDefaultHotkeyValues();

        _profileEvaluationTimer = new System.Windows.Forms.Timer
        {
            Interval = 500
        };
        _profileEvaluationTimer.Tick += (_, _) => EvaluateProcessProfiles();
        _recentAudioStateObservationTimer = new System.Windows.Forms.Timer
        {
            Interval = RecentAudioStateObservationDebounceMs
        };
        _recentAudioStateObservationTimer.Tick += (_, _) =>
        {
            _recentAudioStateObservationTimer.Stop();
            CaptureObservedAudioDeviceState();
        };
        _recentUndoWindowTimer = new System.Windows.Forms.Timer
        {
            Interval = (int)RecentAudioStateTracker.UndoWindowDuration.TotalMilliseconds
        };
        _recentUndoWindowTimer.Tick += (_, _) =>
        {
            _recentUndoWindowTimer.Stop();
            _recentAudioStateTracker.ExpireUndoIfNeeded(DateTime.UtcNow);
            UpdateRecentSwitchUndoHotkeyRegistration();
        };
        _overlayHeightNumericUpDown = new NumericUpDown
        {
            Minimum = 12,
            Maximum = 35,
            Value = 20,
            Width = 120,
            DecimalPlaces = 0,
            Increment = 1,
            Dock = DockStyle.Left,
            Margin = new Padding(0, 4, 0, 4)
        };
        _overlayHeightNumericUpDown.ValueChanged += (_, _) => PersistOverlayPresentationSelections();

        _mainPage = BuildMainPage();
        _settingsPage = BuildSettingsPage();

        Controls.Add(BuildShellLayout());

        ApplyLanguage();
        ApplyTheme(AppThemeMode.System);
        SetActivePage(PageKind.Main, updateRadioSelection: true);
        SetSidebarVisible(false);
        SetStatus(_services.Localizer.Get("InitialStatus"));

        FormClosing += (_, eventArgs) => HandleFormClosing(eventArgs);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ReleaseForegroundChangeHook();
            _restoreWaitRegistration.Unregister(null);
            _restoreEvent.Dispose();
            _profileEvaluationTimer.Dispose();
            _recentAudioStateObservationTimer.Dispose();
            _recentUndoWindowTimer.Dispose();
            _trayHotkey.Dispose();
            _recentSwitchUndoHotkey.Dispose();
            _overlayHotkey.Dispose();
            _profileOverlayForm?.Close();
            _profileOverlayForm?.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _windowIcon.Dispose();
            _trayIcon.Dispose();
            _trayMenu.Dispose();
            _audioDeviceNotificationMonitor?.Dispose();
            _trayNotifications.Dispose();
            SetPictureBoxImage(_primaryIconPreview, null);
            SetPictureBoxImage(_secondaryIconPreview, null);
            SetPictureBoxImage(_profileIconPreview, null);
        }

        base.Dispose(disposing);
    }

    protected override void OnHandleCreated(EventArgs eventArgs)
    {
        base.OnHandleCreated(eventArgs);
        EnsureForegroundChangeHook();

        if (_initialStateLoaded || IsDisposed)
        {
            return;
        }

        _initialStateLoaded = true;
        BeginInvoke(new Action(SafeLoadInitialState));
    }

    protected override void OnHandleDestroyed(EventArgs eventArgs)
    {
        ReleaseForegroundChangeHook();
        base.OnHandleDestroyed(eventArgs);
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
            Height = 46,
            MinimumSize = new Size(0, 46),
            Margin = new Padding(0, 0, 0, 8),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 16, 0),
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
        _settingsGeneralCard.Controls.Add(BuildSettingsGeneralCardContent());
        _settingsAutomationCard.Controls.Add(BuildSettingsAutomationCardContent());
        _settingsHotkeyCard.Controls.Add(BuildSettingsHotkeyCardContent());
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
        page.Controls.Add(_mainDeviceCard, 0, 0);
        page.Controls.Add(_mainSwitchCard, 0, 1);
        return page;
    }

    private TableLayoutPanel BuildSettingsPage()
    {
        _settingsGeneralCard.Margin = Padding.Empty;
        _settingsAutomationCard.Margin = Padding.Empty;
        _settingsHotkeyCard.Margin = Padding.Empty;
        _settingsOverlayCard.Margin = Padding.Empty;
        _settingsProfilesCard.Margin = Padding.Empty;

        var navigationLayout = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };
        navigationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        navigationLayout.Controls.Add(_settingsGeneralTabButton, 0, 0);
        navigationLayout.Controls.Add(_settingsAutomationTabButton, 0, 1);
        navigationLayout.Controls.Add(_settingsShortcutsTabButton, 0, 2);
        navigationLayout.Controls.Add(_settingsProfilesTabButton, 0, 3);

        var navigationCard = CreateCard("soft", autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        navigationCard.Padding = new Padding(12);
        navigationCard.Margin = new Padding(0, 0, 16, 0);
        navigationCard.Controls.Add(navigationLayout);

        var contentColumn = CreateSingleColumnLayout();
        contentColumn.Controls.Add(_settingsContentHost, 0, 0);

        var settingsShell = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };
        settingsShell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 228F));
        settingsShell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        settingsShell.Controls.Add(navigationCard, 0, 0);
        settingsShell.Controls.Add(contentColumn, 1, 0);

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

    private Control BuildSettingsOverlayCardContent()
    {
        var root = CreateSingleColumnLayout();

        var headerCard = CreateInsetCard("accent-soft");
        headerCard.Controls.Add(BuildSettingsLeadBlock(_overlayTitleLabel, _overlayHintLabel));

        var settingsCard = CreateInsetCard(margin: Padding.Empty);
        var settingsLayout = CreateTwoColumnFormLayout(leftColumnWidth: 180F);
        var actionsRow = CreateButtonRow(wrapContents: false);
        actionsRow.Margin = new Padding(0, 12, 0, 0);
        actionsRow.Controls.Add(_openProfileOverlayButton);

        settingsLayout.Controls.Add(_overlayHeightLabel, 0, 0);
        settingsLayout.Controls.Add(_overlayHeightNumericUpDown!, 1, 0);
        settingsLayout.Controls.Add(actionsRow, 1, 1);
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

        var overlayLeadBlock = BuildSettingsLeadBlock(_overlayTitleLabel, _overlayHintLabel);
        overlayLeadBlock.Margin = new Padding(0, 14, 0, 0);
        profileListLayout.Controls.Add(CreateDivider(margin: new Padding(0, 12, 0, 0)), 0, 2);
        profileListLayout.Controls.Add(overlayLeadBlock, 0, 3);
        profileListLayout.Controls.Add(
            CreateFieldGroup(_overlayPositionLabel, BuildOverlayAnchorSelector(), margin: new Padding(0, 12, 0, 0)),
            0,
            4);
        profileListLayout.Controls.Add(
            CreateFieldGroup(_overlayLayoutLabel, BuildOverlayLayoutSelector(), margin: new Padding(0, 12, 0, 0)),
            0,
            5);
        profileListLayout.Controls.Add(
            CreateFieldGroup(_overlayHeightLabel, _overlayHeightNumericUpDown!, margin: new Padding(0, 12, 0, 0)),
            0,
            6);

        var overlayActionRow = CreateButtonRow(wrapContents: false);
        overlayActionRow.Margin = new Padding(0, 10, 0, 0);
        overlayActionRow.Controls.Add(_openProfileOverlayButton);
        profileListLayout.Controls.Add(overlayActionRow, 0, 7);

        _overlayShortcutSectionLabel.Margin = new Padding(0, 14, 0, 0);
        _enableOverlayHotkeyCheckBox.Margin = new Padding(0, 6, 0, 10);
        profileListLayout.Controls.Add(CreateDivider(margin: new Padding(0, 14, 0, 0)), 0, 8);
        profileListLayout.Controls.Add(_overlayShortcutSectionLabel, 0, 9);
        profileListLayout.Controls.Add(_enableOverlayHotkeyCheckBox, 0, 10);
        profileListLayout.Controls.Add(
            BuildManualHotkeyEditor(
                _overlayModifiersLabel,
                _overlayControlCheckBox,
                _overlayAltCheckBox,
                _overlayShiftCheckBox,
                _overlayWindowsCheckBox,
                _overlayKeyLabel,
                _overlayHotkeyComboBox),
            0,
            11);
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
            _automationTitleLabel.Text = _services.Localizer.Get("AutomationGroup");
            _hotkeyTitleLabel.Text = _services.Localizer.Get("HotkeyGroup");
            _overlayTitleLabel.Text = _services.Localizer.Get("OverlayGroup");
            _profilesTitleLabel.Text = _services.Localizer.Get("ProfilesGroup");
            _settingsGeneralTabButton.Text = _services.Localizer.Get("GeneralGroup");
            _settingsAutomationTabButton.Text = _services.Localizer.Get("AutomationGroup");
            _settingsShortcutsTabButton.Text = _services.Localizer.Get("HotkeyGroup");
            _settingsOverlayTabButton.Text = _services.Localizer.Get("OverlayGroup");
            _settingsProfilesTabButton.Text = _services.Localizer.Get("ProfilesGroup");
            _enableProfilesCheckBox.Text = _services.Localizer.Get("EnableProfilesLabel");
            _profileAutomationHintLabel.Text = _services.Localizer.Get("ProfileAutomationHint");

            _languageLabel.Text = _services.Localizer.Get("LanguageLabel");
            _themeLabel.Text = _services.Localizer.Get("ThemeLabel");
            _startWithWindowsCheckBox.Text = _services.Localizer.Get("StartWithWindowsLabel");
            _startMinimizedAtStartupCheckBox.Text = _services.Localizer.Get("StartMinimizedAtStartupLabel");
            _minimizeToTrayOnCloseCheckBox.Text = _services.Localizer.Get("MinimizeToTrayOnCloseLabel");
            _enableUpdateNotificationsCheckBox.Text = _services.Localizer.Get("EnableUpdateNotificationsLabel");
            _syncCommunicationDeviceWithPlaybackCheckBox.Text = _services.Localizer.Get("SyncCommunicationDeviceWithPlaybackLabel");
            _autoSwitchToNewPlaybackDeviceCheckBox.Text = _services.Localizer.Get("AutoSwitchToNewPlaybackDeviceLabel");
            _deviceAIconLabel.Text = _services.Localizer.Get("DeviceAIconLabel");
            _deviceBIconLabel.Text = _services.Localizer.Get("DeviceBIconLabel");
            _systemThemeRadioButton.Text = _services.Localizer.Get("ThemeModeSystem");
            _lightThemeRadioButton.Text = _services.Localizer.Get("ThemeModeLight");
            _darkThemeRadioButton.Text = _services.Localizer.Get("ThemeModeDark");
            _deviceALabel.Text = _services.Localizer.Get("DeviceALabel");
            _deviceBLabel.Text = _services.Localizer.Get("DeviceBLabel");
            _enableHotkeyCheckBox.Text = _services.Localizer.Get("EnableGlobalHotkey");
            _switchShortcutSectionLabel.Text = _services.Localizer.Get("SwitchShortcutSectionLabel");
            _recentSwitchUndoSectionLabel.Text = _services.Localizer.Get("RecentUndoSectionLabel");
            _overlayShortcutSectionLabel.Text = _services.Localizer.Get("OverlayShortcutSectionLabel");
            _enableRecentSwitchUndoHotkeyCheckBox.Text = _services.Localizer.Get("EnableRecentSwitchUndoHotkeyLabel");
            _enableOverlayHotkeyCheckBox.Text = _services.Localizer.Get("EnableOverlayHotkeyLabel");
            _modifiersLabel.Text = _services.Localizer.Get("ModifiersLabel");
            _keyLabel.Text = _services.Localizer.Get("KeyLabel");
            _recentSwitchUndoModifiersLabel.Text = _services.Localizer.Get("ModifiersLabel");
            _recentSwitchUndoKeyLabel.Text = _services.Localizer.Get("KeyLabel");
            _hotkeyNoteLabel.Text = _services.Localizer.Get("HotkeyHint");
            _recentSwitchUndoNoteLabel.Text = _services.Localizer.Get("RecentSwitchUndoHint");
            _overlayModifiersLabel.Text = _services.Localizer.Get("ModifiersLabel");
            _overlayKeyLabel.Text = _services.Localizer.Get("KeyLabel");
            _overlayPositionLabel.Text = _services.Localizer.Get("OverlayPositionLabel");
            _overlayLayoutLabel.Text = _services.Localizer.Get("OverlayLayoutLabel");
            _overlayHeightLabel.Text = _services.Localizer.Get("OverlayHeightLabel");
            _overlayHintLabel.Text = _services.Localizer.Get("OverlayHint");
            _overlayHorizontalLayoutRadioButton.Text = _services.Localizer.Get("OverlayLayoutHorizontal");
            _overlayVerticalLayoutRadioButton.Text = _services.Localizer.Get("OverlayLayoutVertical");
            _profileNameLabel.Text = _services.Localizer.Get("ProfileNameLabel");
            _profileIconLabel.Text = _services.Localizer.Get("ProfileIconLabel");
            _profilePriorityLabel.Text = _services.Localizer.Get("ProfilePriorityLabel");
            _profileOutputDeviceLabel.Text = _services.Localizer.Get("ProfileOutputDeviceLabel");
            _profileInputDeviceLabel.Text = _services.Localizer.Get("ProfileInputDeviceLabel");
            _profileProgramsLabel.Text = _services.Localizer.Get("ProfileProgramsLabel");
            _profileOrderHintLabel.Text = _services.Localizer.Get("ProfileOrderHint");

            _toggleNowButton.Text = _services.Localizer.Get("ToggleNowButton");
            _refreshDevicesButton.Text = _services.Localizer.Get("RefreshDevicesButton");
            _openIconFolderButton.Text = _services.Localizer.Get("OpenIconFolderButton");
            _refreshIconsButton.Text = _services.Localizer.Get("RefreshIconsButton");
            _createShortcutButton.Text = _services.Localizer.Get("CreateToggleShortcutButton");
            _openProfileOverlayButton.Text = _services.Localizer.Get("OpenProfileOverlayButton");
            _statusActionButton.Text = _services.Localizer.Get("UpdateStatusButton");
            _addProfileButton.Text = _services.Localizer.Get("AddProfileButton");
            _removeProfileButton.Text = _services.Localizer.Get("RemoveProfileButton");
            _addProfileProgramButton.Text = _services.Localizer.Get("AddProgramButton");
            _removeProfileProgramButton.Text = _services.Localizer.Get("RemoveProgramButton");

            if (refreshIconChoices)
            {
                RefreshAvailableIcons(preserveSelections: true);
            }

            RefreshProfileLists(refreshDeviceChoices: true);

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
        _hotkeyNoteLabel.ForeColor = _activePalette.MutedText;
        _recentSwitchUndoNoteLabel.ForeColor = _activePalette.MutedText;
        _overlayHintLabel.ForeColor = _activePalette.MutedText;
        _profileOrderHintLabel.ForeColor = _activePalette.MutedText;
        _profileAutomationHintLabel.ForeColor = _activePalette.MutedText;
    }

    private string GetSettingsSectionTitle(SettingsSection section)
    {
        return section switch
        {
            SettingsSection.General => _services.Localizer.Get("GeneralGroup"),
            SettingsSection.Automation => _services.Localizer.Get("AutomationGroup"),
            SettingsSection.Shortcuts => _services.Localizer.Get("HotkeyGroup"),
            SettingsSection.Overlay => _services.Localizer.Get("ProfilesGroup"),
            SettingsSection.Profiles => _services.Localizer.Get("ProfilesGroup"),
            _ => _services.Localizer.Get("SettingsTab")
        };
    }

    private void UpdatePageHeader()
    {
        if (_currentPage == PageKind.Main)
        {
            _pageTitleLabel.Text = _services.Localizer.Get("AppName");
            return;
        }

        _pageTitleLabel.Text = $"{_services.Localizer.Get("SettingsTab")} / {GetSettingsSectionTitle(_currentSettingsSection)}";
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

    private void SetOverlayAnchorSelection(ProfileOverlayAnchor anchor)
    {
        if (!_overlayAnchorButtons.ContainsKey(anchor))
        {
            anchor = ProfileOverlayAnchor.BottomCenter;
        }

        foreach (var pair in _overlayAnchorButtons)
        {
            pair.Value.Checked = pair.Key == anchor;
        }
    }

    private ProfileOverlayAnchor GetSelectedOverlayAnchor()
    {
        foreach (var pair in _overlayAnchorButtons)
        {
            if (pair.Value.Checked)
            {
                return pair.Key;
            }
        }

        return ProfileOverlayAnchor.BottomCenter;
    }

    private void SetOverlayLayoutOrientationSelection(ProfileOverlayLayoutOrientation orientation)
    {
        _overlayHorizontalLayoutRadioButton.Checked = orientation == ProfileOverlayLayoutOrientation.Horizontal;
        _overlayVerticalLayoutRadioButton.Checked = orientation == ProfileOverlayLayoutOrientation.Vertical;
    }

    private ProfileOverlayLayoutOrientation GetSelectedOverlayLayoutOrientation()
    {
        return _overlayVerticalLayoutRadioButton.Checked
            ? ProfileOverlayLayoutOrientation.Vertical
            : ProfileOverlayLayoutOrientation.Horizontal;
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

    private void SetSettingsSection(SettingsSection section, bool updateRadioSelection = false)
    {
        _currentSettingsSection = section;

        if (updateRadioSelection)
        {
            _settingsGeneralTabButton.Checked = section == SettingsSection.General;
            _settingsAutomationTabButton.Checked = section == SettingsSection.Automation;
            _settingsShortcutsTabButton.Checked = section == SettingsSection.Shortcuts;
            _settingsOverlayTabButton.Checked = section == SettingsSection.Overlay;
            _settingsProfilesTabButton.Checked = section == SettingsSection.Profiles;
        }

        if (_settingsContentHost.Controls.Count > 0)
        {
            _settingsContentHost.Controls.Clear();
        }

        _settingsContentHost.Controls.Add(GetSettingsSectionCard(section));
        UpdatePageHeader();
        _pageHost.AutoScrollPosition = Point.Empty;

        if (_shellLayout is not null)
        {
            ApplyTheme(GetSelectedThemeMode());
        }
    }

    private Control GetSettingsSectionCard(SettingsSection section)
    {
        return section switch
        {
            SettingsSection.General => _settingsGeneralCard,
            SettingsSection.Automation => _settingsAutomationCard,
            SettingsSection.Shortcuts => _settingsHotkeyCard,
            SettingsSection.Overlay => _settingsProfilesCard,
            SettingsSection.Profiles => _settingsProfilesCard,
            _ => _settingsGeneralCard
        };
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

    private void HandleUpdateNotificationsChanged()
    {
        if (_isLoading)
        {
            return;
        }

        if (_enableUpdateNotificationsCheckBox.Checked)
        {
            BeginUpdateCheck();
        }
        else
        {
            ClearAvailableUpdate();
        }

        AutoSaveCurrentSelections();
    }

    private void HandleCommunicationDeviceSyncChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();

        if (_syncCommunicationDeviceWithPlaybackCheckBox.Checked)
        {
            SyncCommunicationDeviceToCurrentPlaybackIfEnabled(trackChange: true);
        }
    }

    private void HandleAutoSwitchToNewPlaybackDeviceChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();
    }

    private void HandleProfilesFeatureChanged()
    {
        if (_isLoading)
        {
            return;
        }

        AutoSaveCurrentSelections();
    }

    private void SafeLoadInitialState()
    {
        try
        {
            LoadInitialState();
            InitializeAudioDeviceNotificationMonitor();
            SyncCommunicationDeviceToCurrentPlaybackIfEnabled(trackChange: false);
            EvaluateProcessProfiles();
            InitializeRecentAudioStateTracking();
            BeginUpdateCheck();

            if (!_startHiddenToTrayRequested && !_isHiddenToTray)
            {
                BeginInvoke(new Action(RevealWindow));
            }

            if (_launchedFromPostInstall)
            {
                BeginInvoke(new Action(EnsureVisibleAfterPostInstall));
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
                _cachedConfig = null;
                SelectFirstTwoDevices();
                SetStatus(_services.Localizer.Get("NoSavedSettings"));
            }

            UpdateHotkeyControls();
            UpdateRecentSwitchUndoHotkeyControls();
            UpdateOverlayHotkeyControls();
            UpdateStartupControls();
            UpdateTrayHotkeyRegistration();
            UpdateRecentSwitchUndoHotkeyRegistration();
            UpdateOverlayHotkeyRegistration();
            RefreshProfileLists(refreshDeviceChoices: true);
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

            RefreshProfileLists(refreshDeviceChoices: true);
        });
    }

    private void ApplySavedConfig(AppConfig config)
    {
        _cachedConfig = config;
        _startWithWindowsCheckBox.Checked = config.StartWithWindows;
        _startMinimizedAtStartupCheckBox.Checked = config.StartMinimizedAtStartup;
        _minimizeToTrayOnCloseCheckBox.Checked = config.MinimizeToTrayOnClose;
        _enableUpdateNotificationsCheckBox.Checked = config.EnableUpdateNotifications;
        _syncCommunicationDeviceWithPlaybackCheckBox.Checked = config.SyncCommunicationDeviceWithPlayback;
        _autoSwitchToNewPlaybackDeviceCheckBox.Checked = config.AutoSwitchToNewPlaybackDevice;
        _enableProfilesCheckBox.Checked = config.EnableProfiles;
        SelectDevice(_primaryDeviceComboBox, config.PrimaryDevice.Id);
        SelectDevice(_secondaryDeviceComboBox, config.SecondaryDevice.Id);
        var iconFileName = string.IsNullOrWhiteSpace(config.NotificationIconFileName)
            ? !string.IsNullOrWhiteSpace(config.PrimaryIconFileName)
                ? config.PrimaryIconFileName
                : config.SecondaryIconFileName
            : config.NotificationIconFileName;
        SelectIcon(_primaryIconComboBox, iconFileName);
        SelectIcon(_secondaryIconComboBox, iconFileName);
        _enableHotkeyCheckBox.Checked = config.Hotkey.Enabled;
        _controlCheckBox.Checked = config.Hotkey.Control;
        _altCheckBox.Checked = config.Hotkey.Alt;
        _shiftCheckBox.Checked = config.Hotkey.Shift;
        _windowsCheckBox.Checked = config.Hotkey.WindowsKey;
        SelectKeyOption(_hotkeyComboBox, config.Hotkey.Key);
        _enableRecentSwitchUndoHotkeyCheckBox.Checked = config.RecentSwitchUndoHotkey.Enabled;
        _recentSwitchUndoControlCheckBox.Checked = config.RecentSwitchUndoHotkey.Control;
        _recentSwitchUndoAltCheckBox.Checked = config.RecentSwitchUndoHotkey.Alt;
        _recentSwitchUndoShiftCheckBox.Checked = config.RecentSwitchUndoHotkey.Shift;
        _recentSwitchUndoWindowsCheckBox.Checked = config.RecentSwitchUndoHotkey.WindowsKey;
        SelectKeyOption(_recentSwitchUndoHotkeyComboBox, config.RecentSwitchUndoHotkey.Key);
        _enableOverlayHotkeyCheckBox.Checked = config.OverlayHotkey.Enabled;
        _overlayControlCheckBox.Checked = config.OverlayHotkey.Control;
        _overlayAltCheckBox.Checked = config.OverlayHotkey.Alt;
        _overlayShiftCheckBox.Checked = config.OverlayHotkey.Shift;
        _overlayWindowsCheckBox.Checked = config.OverlayHotkey.WindowsKey;
        SelectKeyOption(_overlayHotkeyComboBox, config.OverlayHotkey.Key);
        _overlayHeightNumericUpDown!.Value = Math.Clamp(config.OverlayHeightPercent, (int)_overlayHeightNumericUpDown.Minimum, (int)_overlayHeightNumericUpDown.Maximum);
        SetOverlayAnchorSelection(config.OverlayAnchor);
        SetOverlayLayoutOrientationSelection(config.OverlayLayoutOrientation);
        _editableProfiles.Clear();
        _editableProfiles.AddRange(
            config.Profiles
                .OrderByDescending(profile => profile.Priority)
                .ThenBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
                .Select(profile =>
                {
                    var clone = profile.Clone();
                    clone.Enabled = true;
                    return clone;
                }));
        RefreshProfileLists(refreshDeviceChoices: true);
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

    private static NotificationIconCatalog.IconChoice? GetSelectedIconChoice(ComboBox comboBox)
    {
        return comboBox.SelectedItem as NotificationIconCatalog.IconChoice;
    }

    private static string GetSelectedIconFileName(ComboBox comboBox, string fallbackFileName)
    {
        return GetSelectedIconChoice(comboBox)?.FileName ?? fallbackFileName;
    }

    private static void SelectKeyOption(ComboBox comboBox, Keys key)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is KeyOption option && option.Value == key)
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static Keys GetSelectedKeyOption(ComboBox comboBox, Keys fallbackKey)
    {
        return comboBox.SelectedItem is KeyOption option ? option.Value : fallbackKey;
    }

    private void RefreshProfileLists(bool refreshDeviceChoices, string? selectedProfileId = null)
    {
        if (_profilesListBox is null)
        {
            return;
        }

        var profileIdToSelect = selectedProfileId ?? GetSelectedProfile()?.Id;
        var items = _editableProfiles
            .Select(profile => new ProfileListItem(profile, BuildProfileListLabel(profile)))
            .ToList();
        var selectedIndex = -1;

        RunWithoutAutoSave(() =>
        {
            _profilesListBox.BeginUpdate();
            _suppressProfileListSelectionEvents = true;

            try
            {
                _profilesListBox.Items.Clear();
                foreach (var item in items)
                {
                    _profilesListBox.Items.Add(item);
                }

                if (_profilesListBox.Items.Count > 0)
                {
                    selectedIndex = 0;
                    for (var index = 0; index < _profilesListBox.Items.Count; index++)
                    {
                        if (_profilesListBox.Items[index] is ProfileListItem item &&
                            string.Equals(item.Profile.Id, profileIdToSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIndex = index;
                            break;
                        }
                    }
                }

                _profilesListBox.SelectedIndex = selectedIndex;
            }
            finally
            {
                _suppressProfileListSelectionEvents = false;
                _profilesListBox.EndUpdate();
            }
        });

        UpdateProfileProgramButtons();

        UpdateSelectedProfileEditor(refreshDeviceChoices);
    }

    private string BuildProfileListLabel(ProcessAudioProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? _services.Localizer.Get("ProfileUnnamed")
            : profile.Name;
    }

    private ProcessAudioProfile? GetSelectedProfile()
    {
        return (_profilesListBox?.SelectedItem as ProfileListItem)?.Profile;
    }

    private ProfileProgramTarget? GetSelectedProfileProgram()
    {
        return (_profileProgramsListBox?.SelectedItem as ProgramListItem)?.Program;
    }

    private void UpdateSelectedProfileEditor(bool refreshDeviceChoices = true)
    {
        if (_profileNameTextBox is null ||
            _profileOrderTextBox is null ||
            _profileProgramsListBox is null)
        {
            return;
        }

        var profile = GetSelectedProfile();
        _suppressProfileEditorEvents = true;

        try
        {
            if (profile is null)
            {
                _profileNameTextBox.Text = string.Empty;
                _profileOrderTextBox.Text = string.Empty;
                _profileIconComboBox.SelectedIndex = -1;
                SetPictureBoxImage(_profileIconPreview, null);
                SetComboBoxItems(_profileOutputDeviceComboBox, BuildDeviceChoices([], new DeviceSelection()));
                SetComboBoxItems(_profileInputDeviceComboBox, BuildDeviceChoices([], new DeviceSelection()));
                SelectDeviceChoice(_profileOutputDeviceComboBox, null);
                SelectDeviceChoice(_profileInputDeviceComboBox, null);
                _profileProgramsListBox.Items.Clear();
                SetProfileEditorEnabled(false);
                return;
            }

            SetProfileEditorEnabled(true);
            _profileNameTextBox.Text = profile.Name;
            var profileOrder = _editableProfiles.FindIndex(item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase)) + 1;
            _profileOrderTextBox.Text = profileOrder > 0 ? profileOrder.ToString() : string.Empty;
            SelectIcon(_profileIconComboBox, profile.IconFileName);
            UpdateDeviceIconPreview(_profileIconComboBox, _profileIconPreview);

            if (refreshDeviceChoices)
            {
                RefreshSelectedProfileDeviceChoices(profile);
            }
            else
            {
                SelectDeviceChoice(_profileOutputDeviceComboBox, profile.PlaybackDevice.Id);
                SelectDeviceChoice(_profileInputDeviceComboBox, profile.RecordingDevice.Id);
            }

            RefreshProfileProgramList(profile);
        }
        finally
        {
            _suppressProfileEditorEvents = false;
            UpdateProfileProgramButtons();
        }
    }

    private void SetProfileEditorEnabled(bool enabled)
    {
        if (_profileNameTextBox is null || _profileOrderTextBox is null || _profileProgramsListBox is null)
        {
            return;
        }

        _profileNameTextBox.Enabled = enabled;
        _profileOrderTextBox.Enabled = enabled;
        _profileIconComboBox.Enabled = enabled && _profileIconComboBox.Items.Count > 0;
        _profileOutputDeviceComboBox.Enabled = enabled;
        _profileInputDeviceComboBox.Enabled = enabled;
        _profileProgramsListBox.Enabled = enabled;
        _removeProfileButton.Enabled = enabled;
        var selectedIndex = _profilesListBox?.SelectedIndex ?? -1;
        _moveProfileUpButton.Enabled = enabled && selectedIndex > 0;
        _moveProfileDownButton.Enabled = enabled && selectedIndex >= 0 && selectedIndex < _editableProfiles.Count - 1;
        _addProfileProgramButton.Enabled = enabled;
        _removeProfileProgramButton.Enabled = enabled && _profileProgramsListBox.SelectedItem is not null;
    }

    private void RefreshSelectedProfileDeviceChoices(ProcessAudioProfile profile)
    {
        _ = TryRepairProfileDeviceSelections(
            profile,
            persistChanges: !_isLoading && !_suppressAutoSave);

        SetComboBoxItems(
            _profileOutputDeviceComboBox,
            BuildDeviceChoices(
                _services.AudioDeviceService.GetSelectablePlaybackDevices(),
                profile.PlaybackDevice));
        SetComboBoxItems(
            _profileInputDeviceComboBox,
            BuildDeviceChoices(
                _services.AudioDeviceService.GetSelectableRecordingDevices(),
                profile.RecordingDevice));

        SelectDeviceChoice(_profileOutputDeviceComboBox, profile.PlaybackDevice.Id);
        SelectDeviceChoice(_profileInputDeviceComboBox, profile.RecordingDevice.Id);
    }

    private bool TryRepairProfileDeviceSelections(ProcessAudioProfile profile, bool persistChanges)
    {
        var repairedSelections = false;
        _ = ResolveProfileDeviceSelection(profile, isPlaybackDevice: true, out var playbackSelectionRepaired);
        repairedSelections |= playbackSelectionRepaired;
        _ = ResolveProfileDeviceSelection(profile, isPlaybackDevice: false, out var recordingSelectionRepaired);
        repairedSelections |= recordingSelectionRepaired;

        if (repairedSelections && persistChanges)
        {
            PersistCachedProfileRepairs();
        }

        return repairedSelections;
    }

    private AudioDeviceInfo? ResolveProfileDeviceSelection(
        ProcessAudioProfile profile,
        bool isPlaybackDevice,
        out bool repairedSelection)
    {
        repairedSelection = false;

        var selection = isPlaybackDevice ? profile.PlaybackDevice : profile.RecordingDevice;
        if (string.IsNullOrWhiteSpace(selection.Id))
        {
            return null;
        }

        AudioDeviceInfo? resolvedDevice;
        bool resolvedByName;
        if (isPlaybackDevice)
        {
            resolvedDevice = _services.AudioDeviceService.ResolveSelectablePlaybackDevice(selection, out resolvedByName);
        }
        else
        {
            resolvedDevice = _services.AudioDeviceService.ResolveSelectableRecordingDevice(selection, out resolvedByName);
        }

        if (!resolvedByName || resolvedDevice is null)
        {
            return resolvedDevice;
        }

        repairedSelection = UpdateProfileDeviceSelection(profile, isPlaybackDevice, resolvedDevice);
        return resolvedDevice;
    }

    private bool UpdateProfileDeviceSelection(
        ProcessAudioProfile profile,
        bool isPlaybackDevice,
        AudioDeviceInfo resolvedDevice)
    {
        var changed = UpdateDeviceSelection(
            isPlaybackDevice ? profile.PlaybackDevice : profile.RecordingDevice,
            resolvedDevice);

        var editableProfile = _editableProfiles.FirstOrDefault(
            item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (editableProfile is not null && !ReferenceEquals(editableProfile, profile))
        {
            changed |= UpdateDeviceSelection(
                isPlaybackDevice ? editableProfile.PlaybackDevice : editableProfile.RecordingDevice,
                resolvedDevice);
        }

        var cachedProfile = _cachedConfig?.Profiles.FirstOrDefault(
            item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (cachedProfile is not null &&
            !ReferenceEquals(cachedProfile, profile) &&
            !ReferenceEquals(cachedProfile, editableProfile))
        {
            changed |= UpdateDeviceSelection(
                isPlaybackDevice ? cachedProfile.PlaybackDevice : cachedProfile.RecordingDevice,
                resolvedDevice);
        }

        return changed;
    }

    private static bool UpdateDeviceSelection(DeviceSelection selection, AudioDeviceInfo resolvedDevice)
    {
        if (string.Equals(selection.Id, resolvedDevice.Id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(selection.Name, resolvedDevice.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        selection.Id = resolvedDevice.Id;
        selection.Name = resolvedDevice.Name;
        return true;
    }

    private void PersistCachedProfileRepairs()
    {
        if (_cachedConfig is null)
        {
            return;
        }

        try
        {
            _services.ConfigStore.Save(_cachedConfig);
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.PersistCachedProfileRepairs", ex);
        }
    }

    private void RefreshSelectedProfileDeviceChoicesIfSelected(string profileId)
    {
        var selectedProfile = GetSelectedProfile();
        if (selectedProfile is null ||
            !string.Equals(selectedProfile.Id, profileId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var previousSuppressProfileEditorEvents = _suppressProfileEditorEvents;
        _suppressProfileEditorEvents = true;
        try
        {
            RefreshSelectedProfileDeviceChoices(selectedProfile);
        }
        finally
        {
            _suppressProfileEditorEvents = previousSuppressProfileEditorEvents;
        }
    }

    private List<DeviceChoice> BuildDeviceChoices(
        IReadOnlyList<AudioDeviceInfo> devices,
        DeviceSelection selectedDevice)
    {
        var options = new List<DeviceChoice>
        {
            new(string.Empty, _services.Localizer.Get("ProfileDoNotChangeOption"), string.Empty)
        };

        options.AddRange(devices.Select(device => new DeviceChoice(device.Id, device.DisplayName, device.Name)));

        if (!string.IsNullOrWhiteSpace(selectedDevice.Id) &&
            options.All(option => !string.Equals(option.Id, selectedDevice.Id, StringComparison.OrdinalIgnoreCase)) &&
            options.All(option => !string.Equals(option.Label, selectedDevice.Name, StringComparison.OrdinalIgnoreCase)))
        {
            options.Add(new DeviceChoice(selectedDevice.Id, selectedDevice.Name, selectedDevice.Name));
        }

        return options;
    }

    private static void SelectDeviceChoice(ComboBox comboBox, string? deviceId)
    {
        if (comboBox.Items.Count == 0)
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            comboBox.SelectedIndex = 0;
            return;
        }

        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is DeviceChoice choice &&
                string.Equals(choice.Id, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }

        comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
    }

    private void RefreshProfileProgramList(ProcessAudioProfile profile)
    {
        if (_profileProgramsListBox is null)
        {
            return;
        }

        _profileProgramsListBox.BeginUpdate();
        _profileProgramsListBox.Items.Clear();
        foreach (var program in profile.Programs)
        {
            _profileProgramsListBox.Items.Add(new ProgramListItem(program, BuildProgramLabel(program)));
        }

        _profileProgramsListBox.EndUpdate();
    }

    private string BuildProgramLabel(ProfileProgramTarget program)
    {
        if (!string.IsNullOrWhiteSpace(program.DisplayName))
        {
            return program.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(program.ExecutableName))
        {
            return program.ExecutableName;
        }

        return program.ExecutablePath;
    }

    private void UpdateProfileProgramButtons()
    {
        if (_profileProgramsListBox is null)
        {
            return;
        }

        _removeProfileProgramButton.Enabled =
            _profileProgramsListBox.Enabled &&
            _profileProgramsListBox.SelectedItem is not null;
    }

    private void HandleProfileSelectionChanged()
    {
        if (_suppressProfileListSelectionEvents)
        {
            return;
        }

        UpdateSelectedProfileEditor();
    }

    private void HandleSelectedProfileChanged()
    {
        if (_suppressProfileEditorEvents)
        {
            return;
        }

        var profile = GetSelectedProfile();
        if (profile is null || _profileNameTextBox is null)
        {
            return;
        }

        profile.Name = _profileNameTextBox.Text.Trim();
        profile.IconFileName = NotificationIconCatalog.NormalizeFileName(
            GetSelectedIconFileName(_profileIconComboBox, AppConfig.DefaultIconFileName));
        if (_profileOutputDeviceComboBox.SelectedItem is DeviceChoice playbackChoice)
        {
            profile.PlaybackDevice.Id = playbackChoice.Id;
            profile.PlaybackDevice.Name = playbackChoice.DeviceName;
        }

        if (_profileInputDeviceComboBox.SelectedItem is DeviceChoice recordingChoice)
        {
            profile.RecordingDevice.Id = recordingChoice.Id;
            profile.RecordingDevice.Name = recordingChoice.DeviceName;
        }

        RefreshProfileLists(refreshDeviceChoices: false, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void AddProfile()
    {
        var suffix = 1;
        string name;
        do
        {
            name = _services.Localizer.Format("ProfileDefaultName", suffix++);
        }
        while (_editableProfiles.Any(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase)));

        var profile = new ProcessAudioProfile
        {
            Name = name
        };

        _editableProfiles.Add(profile);
        RefreshProfileLists(refreshDeviceChoices: true, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedProfile()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        _editableProfiles.Remove(profile);
        RefreshProfileLists(refreshDeviceChoices: true);
        AutoSaveCurrentSelections();
    }

    private void MoveSelectedProfile(int offset)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        var currentIndex = _editableProfiles.FindIndex(item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0)
        {
            return;
        }

        var targetIndex = currentIndex + offset;
        if (targetIndex < 0 || targetIndex >= _editableProfiles.Count)
        {
            return;
        }

        _editableProfiles.RemoveAt(currentIndex);
        _editableProfiles.Insert(targetIndex, profile);
        RefreshProfileLists(refreshDeviceChoices: false, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void AddProgramToSelectedProfile()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Applications (*.exe)|*.exe",
            Multiselect = true,
            Title = _services.Localizer.Get("SelectProgramsDialogTitle")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (var fileName in dialog.FileNames)
        {
            var executableName = Path.GetFileName(fileName);
            if (profile.Programs.Any(program =>
                    string.Equals(program.ExecutablePath, fileName, StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrWhiteSpace(program.ExecutablePath) &&
                     string.Equals(program.ExecutableName, executableName, StringComparison.OrdinalIgnoreCase))))
            {
                continue;
            }

            profile.Programs.Add(new ProfileProgramTarget
            {
                DisplayName = Path.GetFileNameWithoutExtension(fileName),
                ExecutableName = executableName,
                ExecutablePath = fileName
            });
        }

        RefreshProfileProgramList(profile);
        UpdateProfileProgramButtons();
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedProgramFromProfile()
    {
        var profile = GetSelectedProfile();
        var program = GetSelectedProfileProgram();
        if (profile is null || program is null)
        {
            return;
        }

        profile.Programs.Remove(program);
        RefreshProfileProgramList(profile);
        UpdateProfileProgramButtons();
        AutoSaveCurrentSelections();
    }

    private void RefreshAvailableIcons(bool preserveSelections, bool showStatus = false)
    {
        RunWithoutAutoSave(() =>
        {
            var selectedIcon = preserveSelections
                ? GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName)
                : AppConfig.DefaultIconFileName;
            var selectedProfile = GetSelectedProfile();
            var previousSuppressProfileEditorEvents = _suppressProfileEditorEvents;
            _suppressProfileEditorEvents = true;

            try
            {
                var icons = NotificationIconCatalog.GetSelectableIcons(_services.Localizer).ToList();
                SetComboBoxItems(_primaryIconComboBox, icons);
                SetComboBoxItems(_secondaryIconComboBox, icons);
                SetComboBoxItems(_profileIconComboBox, icons);

                var hasIcons = icons.Count > 0;
                _primaryIconComboBox.Enabled = hasIcons;
                _secondaryIconComboBox.Enabled = hasIcons;
                _profileIconComboBox.Enabled = hasIcons && selectedProfile is not null;

                if (!hasIcons)
                {
                    SetPictureBoxImage(_primaryIconPreview, null);
                    SetPictureBoxImage(_secondaryIconPreview, null);
                    SetPictureBoxImage(_profileIconPreview, null);

                    if (showStatus)
                    {
                        SetStatus(_services.Localizer.Get("StatusNoIconsFound"), isError: true);
                    }

                    return;
                }

                SelectIcon(_primaryIconComboBox, selectedIcon);
                SelectIcon(_secondaryIconComboBox, selectedIcon);
                SelectIcon(_profileIconComboBox, selectedProfile?.IconFileName ?? AppConfig.DefaultIconFileName);

                if (_primaryIconComboBox.SelectedIndex < 0)
                {
                    _primaryIconComboBox.SelectedIndex = _primaryIconComboBox.Items.Count > 0 ? 0 : -1;
                }

                if (_secondaryIconComboBox.SelectedIndex < 0)
                {
                    _secondaryIconComboBox.SelectedIndex = _secondaryIconComboBox.Items.Count > 0
                        ? Math.Max(_primaryIconComboBox.SelectedIndex, 0)
                        : -1;
                }

                if (_profileIconComboBox.SelectedIndex < 0)
                {
                    _profileIconComboBox.SelectedIndex = _profileIconComboBox.Items.Count > 0 ? 0 : -1;
                }

                UpdateDeviceIconPreviews();
                if (selectedProfile is null)
                {
                    _profileIconComboBox.SelectedIndex = -1;
                    SetPictureBoxImage(_profileIconPreview, null);
                }
                else
                {
                    UpdateDeviceIconPreview(_profileIconComboBox, _profileIconPreview);
                }

                if (showStatus)
                {
                    SetStatus(_services.Localizer.Format("StatusIconsRefreshed", icons.Count));
                }
            }
            finally
            {
                _suppressProfileEditorEvents = previousSuppressProfileEditorEvents;
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
        var fileName = GetSelectedIconChoice(comboBox)?.FileName;
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

    private void UpdateRecentSwitchUndoHotkeyControls()
    {
        var enabled = _enableRecentSwitchUndoHotkeyCheckBox.Checked;
        _recentSwitchUndoControlCheckBox.Enabled = enabled;
        _recentSwitchUndoAltCheckBox.Enabled = enabled;
        _recentSwitchUndoShiftCheckBox.Enabled = enabled;
        _recentSwitchUndoWindowsCheckBox.Enabled = enabled;
        _recentSwitchUndoHotkeyComboBox.Enabled = enabled;
    }

    private void UpdateOverlayHotkeyControls()
    {
        var enabled = _enableOverlayHotkeyCheckBox.Checked;
        _overlayControlCheckBox.Enabled = enabled;
        _overlayAltCheckBox.Enabled = enabled;
        _overlayShiftCheckBox.Enabled = enabled;
        _overlayWindowsCheckBox.Enabled = enabled;
        _overlayHotkeyComboBox.Enabled = enabled;
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

    private void ApplyDefaultHotkeyValues()
    {
        var defaultConfig = new AppConfig();

        RunWithoutAutoSave(() =>
        {
            _controlCheckBox.Checked = defaultConfig.Hotkey.Control;
            _altCheckBox.Checked = defaultConfig.Hotkey.Alt;
            _shiftCheckBox.Checked = defaultConfig.Hotkey.Shift;
            _windowsCheckBox.Checked = defaultConfig.Hotkey.WindowsKey;
            SelectKeyOption(_hotkeyComboBox, defaultConfig.Hotkey.Key);

            _recentSwitchUndoControlCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Control;
            _recentSwitchUndoAltCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Alt;
            _recentSwitchUndoShiftCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.Shift;
            _recentSwitchUndoWindowsCheckBox.Checked = defaultConfig.RecentSwitchUndoHotkey.WindowsKey;
            SelectKeyOption(_recentSwitchUndoHotkeyComboBox, defaultConfig.RecentSwitchUndoHotkey.Key);

            _overlayControlCheckBox.Checked = defaultConfig.OverlayHotkey.Control;
            _overlayAltCheckBox.Checked = defaultConfig.OverlayHotkey.Alt;
            _overlayShiftCheckBox.Checked = defaultConfig.OverlayHotkey.Shift;
            _overlayWindowsCheckBox.Checked = defaultConfig.OverlayHotkey.WindowsKey;
            SelectKeyOption(_overlayHotkeyComboBox, defaultConfig.OverlayHotkey.Key);
            if (_overlayHeightNumericUpDown is not null)
            {
                _overlayHeightNumericUpDown.Value = Math.Clamp(
                    defaultConfig.OverlayHeightPercent,
                    (int)_overlayHeightNumericUpDown.Minimum,
                    (int)_overlayHeightNumericUpDown.Maximum);
            }
            SetOverlayAnchorSelection(defaultConfig.OverlayAnchor);
            SetOverlayLayoutOrientationSelection(defaultConfig.OverlayLayoutOrientation);
        });
    }

    private void InitializeRecentAudioStateTracking()
    {
        try
        {
            _recentAudioStateObservationTimer.Stop();
            _recentUndoWindowTimer.Stop();
            _recentAudioStateTracker.Initialize(_services.AudioDeviceService.CaptureCurrentState());
            _ignoreObservedAudioDeviceChangesUntilUtc = DateTime.MinValue;
            UpdateRecentSwitchUndoHotkeyRegistration();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.InitializeRecentAudioStateTracking", ex);
        }
    }

    private T ExecuteTrackedAudioChange<T>(Func<T> action)
    {
        FlushPendingObservedAudioDeviceStateCapture();
        SuppressObservedAudioDeviceNotifications();

        try
        {
            return action();
        }
        finally
        {
            CaptureCurrentAudioDeviceStateAsRecentChange();
        }
    }

    private void QueueObservedAudioDeviceStateCapture()
    {
        if (IsDisposed || _isLoading || ShouldIgnoreObservedAudioDeviceNotifications())
        {
            return;
        }

        _recentAudioStateObservationTimer.Stop();
        _recentAudioStateObservationTimer.Start();
    }

    private void FlushPendingObservedAudioDeviceStateCapture()
    {
        if (!_recentAudioStateObservationTimer.Enabled)
        {
            return;
        }

        _recentAudioStateObservationTimer.Stop();
        CaptureObservedAudioDeviceState();
    }

    private void CaptureObservedAudioDeviceState()
    {
        if (IsDisposed || _isLoading || ShouldIgnoreObservedAudioDeviceNotifications())
        {
            return;
        }

        CaptureCurrentAudioDeviceStateAsRecentChange("observed", collapseIfReturningToPreviousState: true);
    }

    private void CaptureCurrentAudioDeviceStateAsRecentChange(
        string source = "internal",
        bool collapseIfReturningToPreviousState = false)
    {
        try
        {
            var currentState = _services.AudioDeviceService.CaptureCurrentState();
            var changeResult = _recentAudioStateTracker.RecordStateChange(
                currentState,
                DateTime.UtcNow,
                collapseIfReturningToPreviousState);
            if (changeResult == RecentAudioStateChangeResult.None)
            {
                return;
            }

            if (changeResult == RecentAudioStateChangeResult.Cleared)
            {
                AppLogger.LogInfo($"Recent audio state cleared. Source={source}");
                _recentUndoWindowTimer.Stop();
                UpdateRecentSwitchUndoHotkeyRegistration();
                return;
            }

            AppLogger.LogInfo($"Recent audio state updated. Source={source}");
            RestartRecentUndoWindowTimer();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.CaptureCurrentAudioDeviceStateAsRecentChange", ex);
        }
    }

    private void RestartRecentUndoWindowTimer()
    {
        _recentUndoWindowTimer.Stop();
        if (!IsRecentSwitchUndoWindowActive())
        {
            UpdateRecentSwitchUndoHotkeyRegistration();
            return;
        }

        UpdateRecentSwitchUndoHotkeyRegistration();
        _recentUndoWindowTimer.Start();
    }

    private bool IsRecentSwitchUndoWindowActive()
    {
        var undoAvailableUntilUtc = _recentAudioStateTracker.UndoAvailableUntilUtc;
        return undoAvailableUntilUtc.HasValue && DateTime.UtcNow <= undoAvailableUntilUtc.Value;
    }

    private void SuppressObservedAudioDeviceNotifications()
    {
        _recentAudioStateObservationTimer.Stop();
        _ignoreObservedAudioDeviceChangesUntilUtc = DateTime.UtcNow.AddMilliseconds(RecentAudioStateNotificationSuppressionMs);
    }

    private bool ShouldIgnoreObservedAudioDeviceNotifications()
    {
        return DateTime.UtcNow < _ignoreObservedAudioDeviceChangesUntilUtc;
    }

    private void UndoRecentAudioDeviceChange()
    {
        FlushPendingObservedAudioDeviceStateCapture();

        if (!_recentAudioStateTracker.TryGetUndoTarget(DateTime.UtcNow, out var targetState, out var availability) ||
            targetState is null)
        {
            _recentAudioStateTracker.InvalidateUndo();
            _recentUndoWindowTimer.Stop();
            UpdateRecentSwitchUndoHotkeyRegistration();

            var unavailableMessage = availability == RecentAudioUndoAvailability.Expired
                ? _services.Localizer.Get("ErrorRecentSwitchUndoExpired")
                : _services.Localizer.Get("ErrorRecentSwitchUndoUnavailable");
            SetStatus(unavailableMessage, isError: true);
            AppLogger.LogInfo($"Recent audio undo ignored. Availability={availability}");
            return;
        }

        SuppressObservedAudioDeviceNotifications();
        var result = _services.AudioDeviceService.ApplyState(targetState);
        if (result.Success)
        {
            _recentAudioStateTracker.CommitUndo(targetState, DateTime.UtcNow);
            RestartRecentUndoWindowTimer();
        }
        else
        {
            CaptureCurrentAudioDeviceStateAsRecentChange("undo-failed");
        }

        SetStatus(result.Message, isError: !result.Success);
        _trayNotifications.Show(
            result.Message,
            result.Success ? ToolTipIcon.Info : ToolTipIcon.Warning,
            2500,
            imagePath: NotificationIconCatalog.ResolvePath(_cachedConfig?.NotificationIconFileName ?? AppConfig.DefaultIconFileName));
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
        if (ShouldSuppressPostInstallClose(eventArgs))
        {
            eventArgs.Cancel = true;
            BeginInvoke(new Action(EnsureVisibleAfterPostInstall));
            return;
        }

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

    private bool ShouldSuppressPostInstallClose(FormClosingEventArgs eventArgs)
    {
        if (!_postInstallCloseGuardPending ||
            !_launchedFromPostInstall ||
            _allowClose ||
            _isHiddenToTray)
        {
            return false;
        }

        if (eventArgs.CloseReason is CloseReason.ApplicationExitCall or CloseReason.WindowsShutDown or CloseReason.TaskManagerClosing)
        {
            return false;
        }

        if (DateTime.UtcNow - _postInstallStartedAtUtc > TimeSpan.FromSeconds(5))
        {
            return false;
        }

        _postInstallCloseGuardPending = false;
        AppLogger.LogInfo($"Suppressed post-install close request. Reason={eventArgs.CloseReason}.");
        return true;
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
            ForceForeground();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }
    }

    private async void EnsureVisibleAfterPostInstall()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            if (IsDisposed)
            {
                return;
            }

            _startHiddenToTrayRequested = false;
            _isHiddenToTray = false;
            RevealWindow();

            if (attempt < 2)
            {
                await Task.Delay(500);
            }
        }
    }

    private void ForceForeground()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        ShowWindow(Handle, SwRestore);
        SetForegroundWindow(Handle);
        TopMost = true;
        TopMost = false;
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
            CloseProfileOverlay();

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
        if (config is null)
        {
            config = _cachedConfig;
        }

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

        _cachedConfig = config;

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

    private bool UpdateRecentSwitchUndoHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null)
        {
            config = _cachedConfig;
        }

        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _recentSwitchUndoHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _recentSwitchUndoHotkey.Unregister();
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config.Language);

        if (!config.RecentSwitchUndoHotkey.Enabled || !IsRecentSwitchUndoWindowActive())
        {
            _recentSwitchUndoHotkey.Unregister();
            return true;
        }

        if (_recentSwitchUndoHotkey.Register(config.RecentSwitchUndoHotkey, out var errorMessage))
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

    private bool UpdateOverlayHotkeyRegistration(AppConfig? config = null, bool showErrorNotification = false)
    {
        if (config is null)
        {
            config = _cachedConfig;
        }

        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            _overlayHotkey.Unregister();
            return false;
        }

        if (config is null)
        {
            _overlayHotkey.Unregister();
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config.Language);

        if (!config.OverlayHotkey.Enabled)
        {
            _overlayHotkey.Unregister();
            return true;
        }

        if (_overlayHotkey.Register(config.OverlayHotkey, out var errorMessage))
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

    private void ToggleProfileOverlay()
    {
        if (_profileOverlayForm is not null && !_profileOverlayForm.IsDisposed && _profileOverlayForm.Visible)
        {
            CloseProfileOverlay();
            return;
        }

        OpenProfileOverlay();
    }

    private void OpenProfileOverlay()
    {
        if (!TryGetOverlayRuntimeConfig(out var config))
        {
            return;
        }

        var profiles = BuildOverlayProfiles(config!);
        if (profiles.Count == 0)
        {
            var message = _services.Localizer.Get("ErrorNoOverlayProfiles");
            SetStatus(message, isError: true);
            _trayNotifications.Show(message, ToolTipIcon.Warning, 2500);
            return;
        }

        CloseProfileOverlay();

        _profileOverlayForm = new ProfileOverlayForm(
            profiles,
            _services.Localizer,
            config!.Theme,
            config.OverlayHeightPercent,
            config.OverlayAnchor,
            config.OverlayLayoutOrientation);
        _profileOverlayForm.ProfileSelected += HandleOverlayProfileSelected;
        _profileOverlayForm.FormClosed += HandleProfileOverlayClosed;
        _profileOverlayForm.Show();
        _profileOverlayForm.Activate();
    }

    private bool TryGetOverlayRuntimeConfig(out AppConfig? config)
    {
        var language = _languageComboBox.SelectedValue is AppLanguage selectedLanguage
            ? selectedLanguage
            : _services.Localizer.CurrentLanguage;

        config = new AppConfig
        {
            Language = language,
            Theme = GetSelectedThemeMode(),
            OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value),
            OverlayAnchor = GetSelectedOverlayAnchor(),
            OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation(),
            Profiles = BuildProfilesForConfig()
        };

        _services.Localizer.SetLanguage(language);
        return true;
    }

    private bool TryLoadOverlayConfig(out AppConfig? config)
    {
        config = _cachedConfig;
        if (config is not null)
        {
            _services.Localizer.SetLanguage(config.Language);
            return true;
        }

        if (!_services.ConfigStore.TryLoad(out config, out var errorMessage))
        {
            var message = errorMessage ?? _services.Localizer.Get("ErrorNoConfig");
            SetStatus(message, isError: true);
            _trayNotifications.Show(message, ToolTipIcon.Warning, 2500);
            return false;
        }

        _cachedConfig = config;
        _services.Localizer.SetLanguage(config!.Language);
        return true;
    }

    private List<ProcessAudioProfile> BuildOverlayProfiles(AppConfig config)
    {
        return config.Profiles
            .OrderByDescending(profile => profile.Priority)
            .Where(profile =>
                !string.IsNullOrWhiteSpace(profile.PlaybackDevice.Id) ||
                !string.IsNullOrWhiteSpace(profile.RecordingDevice.Id))
            .Select(profile =>
            {
                var clone = profile.Clone();
                clone.Enabled = true;
                return clone;
            })
            .ToList();
    }

    private void HandleOverlayProfileSelected(object? sender, ProfileOverlaySelectionEventArgs eventArgs)
    {
        if (!TryApplyProcessProfile(eventArgs.Profile, out var message, out var isError, out var devicesChanged))
        {
            if (isError && !string.IsNullOrWhiteSpace(message))
            {
                SetStatus(message, isError: true);
            }

            return;
        }

        _activeProcessProfileId = eventArgs.Profile.Id;
        _activeProcessProfilePlaybackDeviceId = eventArgs.Profile.PlaybackDevice.Id;
        _activeProcessProfileRecordingDeviceId = eventArgs.Profile.RecordingDevice.Id;

        if (devicesChanged && !string.IsNullOrWhiteSpace(message))
        {
            SetStatus(message);
            _trayNotifications.Show(
                message,
                ToolTipIcon.Info,
                2000,
                imagePath: NotificationIconCatalog.ResolvePath(_cachedConfig?.NotificationIconFileName ?? AppConfig.DefaultIconFileName));
        }
    }

    private void HandleProfileOverlayClosed(object? sender, FormClosedEventArgs eventArgs)
    {
        if (_profileOverlayForm is null)
        {
            return;
        }

        _profileOverlayForm.ProfileSelected -= HandleOverlayProfileSelected;
        _profileOverlayForm.FormClosed -= HandleProfileOverlayClosed;
        _profileOverlayForm.Dispose();
        _profileOverlayForm = null;
    }

    private void CloseProfileOverlay()
    {
        if (_profileOverlayForm is null)
        {
            return;
        }

        var overlayForm = _profileOverlayForm;
        _profileOverlayForm = null;
        overlayForm.ProfileSelected -= HandleOverlayProfileSelected;
        overlayForm.FormClosed -= HandleProfileOverlayClosed;

        if (!overlayForm.IsDisposed)
        {
            overlayForm.Close();
            overlayForm.Dispose();
        }
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

        var result = ExecuteTrackedAudioChange(() => _services.AudioDeviceService.Toggle(config));
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

        var result = ExecuteTrackedAudioChange(() => _services.AudioDeviceService.Toggle(config));
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
            _cachedConfig = config;
            _services.ShortcutManager.SyncStartupShortcut(config);
            var hotkeyRegistered = UpdateTrayHotkeyRegistration(config);
            var recentSwitchUndoHotkeyRegistered = UpdateRecentSwitchUndoHotkeyRegistration(config);
            var overlayHotkeyRegistered = UpdateOverlayHotkeyRegistration(config);
            EvaluateProcessProfiles(config);

            if (hotkeyRegistered &&
                recentSwitchUndoHotkeyRegistered &&
                overlayHotkeyRegistered &&
                !string.IsNullOrWhiteSpace(successStatus))
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
            EnableUpdateNotifications = _enableUpdateNotificationsCheckBox.Checked,
            SyncCommunicationDeviceWithPlayback = _syncCommunicationDeviceWithPlaybackCheckBox.Checked,
            AutoSwitchToNewPlaybackDevice = _autoSwitchToNewPlaybackDeviceCheckBox.Checked,
            EnableProfiles = _enableProfilesCheckBox.Checked,
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
            NotificationIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            PrimaryIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            SecondaryIconFileName = GetSelectedIconFileName(_primaryIconComboBox, AppConfig.DefaultIconFileName),
            OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value),
            OverlayAnchor = GetSelectedOverlayAnchor(),
            OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation(),
            Profiles = BuildProfilesForConfig(),
            Hotkey = new HotkeySettings
            {
                Enabled = _enableHotkeyCheckBox.Checked,
                Control = _controlCheckBox.Checked,
                Alt = _altCheckBox.Checked,
                Shift = _shiftCheckBox.Checked,
                WindowsKey = _windowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_hotkeyComboBox, Keys.F10)
            },
            RecentSwitchUndoHotkey = new HotkeySettings
            {
                Enabled = _enableRecentSwitchUndoHotkeyCheckBox.Checked,
                Control = _recentSwitchUndoControlCheckBox.Checked,
                Alt = _recentSwitchUndoAltCheckBox.Checked,
                Shift = _recentSwitchUndoShiftCheckBox.Checked,
                WindowsKey = _recentSwitchUndoWindowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_recentSwitchUndoHotkeyComboBox, Keys.Z)
            },
            OverlayHotkey = new HotkeySettings
            {
                Enabled = _enableOverlayHotkeyCheckBox.Checked,
                Control = _overlayControlCheckBox.Checked,
                Alt = _overlayAltCheckBox.Checked,
                Shift = _overlayShiftCheckBox.Checked,
                WindowsKey = _overlayWindowsCheckBox.Checked,
                Key = GetSelectedKeyOption(_overlayHotkeyComboBox, Keys.V)
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

    private void PersistOverlayPresentationSelections()
    {
        if (_isLoading || _suppressAutoSave)
        {
            return;
        }

        AppConfig? config = _cachedConfig;
        if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
        {
            return;
        }

        if (config is null)
        {
            return;
        }

        config.Language = _languageComboBox.SelectedValue is AppLanguage selectedLanguage
            ? selectedLanguage
            : _services.Localizer.CurrentLanguage;
        config.Theme = GetSelectedThemeMode();
        config.OverlayHeightPercent = _overlayHeightNumericUpDown is null ? 20 : Decimal.ToInt32(_overlayHeightNumericUpDown.Value);
        config.OverlayAnchor = GetSelectedOverlayAnchor();
        config.OverlayLayoutOrientation = GetSelectedOverlayLayoutOrientation();

        try
        {
            _services.ConfigStore.Save(config);
            _cachedConfig = config;
            SetStatus(_services.Localizer.Get("StatusSettingsSaved"));
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.PersistOverlayPresentationSelections", ex);
        }
    }

    private List<ProcessAudioProfile> BuildProfilesForConfig()
    {
        var count = _editableProfiles.Count;
        return _editableProfiles
            .Select((profile, index) =>
            {
                var clone = profile.Clone();
                clone.Priority = count - index;
                clone.Enabled = true;
                return clone;
            })
            .ToList();
    }

    private void SetStatus(string message, bool isError = false)
    {
        _lastStatusWasError = isError;
        _statusLabel.ForeColor = isError ? _activePalette.ErrorText : _activePalette.SuccessText;
        _statusLabel.Text = message;
    }

    private void EnsureForegroundChangeHook()
    {
        if (_foregroundChangeHook != IntPtr.Zero || IsDisposed)
        {
            return;
        }

        _foregroundChangeHook = SetWinEventHook(
            EventSystemForeground,
            EventSystemForeground,
            IntPtr.Zero,
            _foregroundChangedCallback,
            0,
            0,
            WinEventOutOfContext);

        if (_foregroundChangeHook == IntPtr.Zero)
        {
            AppLogger.LogInfo(
                $"Foreground hook unavailable. Falling back to timer-based profile evaluation. Win32Error={Marshal.GetLastWin32Error()}");
        }
    }

    private void ReleaseForegroundChangeHook()
    {
        if (_foregroundChangeHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWinEvent(_foregroundChangeHook);
        _foregroundChangeHook = IntPtr.Zero;
    }

    private void HandleForegroundChanged(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime)
    {
        if (IsDisposed || !IsHandleCreated || !_initialStateLoaded)
        {
            return;
        }

        BeginInvoke(new Action(() => EvaluateProcessProfiles()));
    }

    private void RefreshProfileEvaluationTimer(bool requireBackgroundMonitoring)
    {
        var shouldRun = _foregroundChangeHook == IntPtr.Zero || requireBackgroundMonitoring;
        if (shouldRun)
        {
            if (!_profileEvaluationTimer.Enabled)
            {
                _profileEvaluationTimer.Start();
            }

            return;
        }

        if (_profileEvaluationTimer.Enabled)
        {
            _profileEvaluationTimer.Stop();
        }
    }

    private void InitializeAudioDeviceNotificationMonitor()
    {
        if (_audioDeviceNotificationMonitor is not null || IsDisposed)
        {
            return;
        }

        try
        {
            _audioDeviceNotificationMonitor = new AudioDeviceNotificationMonitor();
            _audioDeviceNotificationMonitor.DefaultAudioDeviceChanged += HandleTrackedDefaultAudioDeviceChanged;
            _audioDeviceNotificationMonitor.DefaultPlaybackDeviceChanged += HandleDefaultPlaybackDeviceChanged;
            _audioDeviceNotificationMonitor.AudioDeviceAdded += HandleAudioDeviceAdded;
            _audioDeviceNotificationMonitor.Start();
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.AudioDeviceNotificationMonitor", ex);
            _audioDeviceNotificationMonitor?.Dispose();
            _audioDeviceNotificationMonitor = null;
        }
    }

    private void HandleTrackedDefaultAudioDeviceChanged(object? sender, DefaultAudioDeviceChangedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(QueueObservedAudioDeviceStateCapture));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void HandleDefaultPlaybackDeviceChanged(object? sender, DefaultPlaybackDeviceChangedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (ShouldIgnoreObservedAudioDeviceNotifications())
                {
                    return;
                }

                SyncCommunicationDeviceWithPlayback(eventArgs.DeviceId, trackChange: false);
            }));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void HandleAudioDeviceAdded(object? sender, AudioDeviceAddedEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() => _ = HandleNewPlaybackDeviceConnectedAsync(eventArgs.DeviceId)));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void SyncCommunicationDeviceToCurrentPlaybackIfEnabled(bool trackChange)
    {
        if (!_syncCommunicationDeviceWithPlaybackCheckBox.Checked)
        {
            return;
        }

        var playbackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
        if (playbackDevice is null)
        {
            return;
        }

        SyncCommunicationDeviceWithPlayback(playbackDevice.Id, trackChange);
    }

    private void SyncCommunicationDeviceWithPlayback(string playbackDeviceId, bool trackChange)
    {
        if (IsDisposed ||
            _isLoading ||
            !_syncCommunicationDeviceWithPlaybackCheckBox.Checked ||
            string.IsNullOrWhiteSpace(playbackDeviceId))
        {
            return;
        }

        try
        {
            var communicationDevice = _services.AudioDeviceService.GetDefaultCommunicationDevice();
            if (communicationDevice is not null &&
                string.Equals(communicationDevice.Id, playbackDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (trackChange)
            {
                _ = ExecuteTrackedAudioChange(() =>
                {
                    _services.AudioDeviceService.SetDefaultCommunicationDevice(playbackDeviceId);
                    return true;
                });
                return;
            }

            _services.AudioDeviceService.SetDefaultCommunicationDevice(playbackDeviceId);
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.SyncCommunicationDeviceWithPlayback", ex);
        }
    }

    private async Task HandleNewPlaybackDeviceConnectedAsync(string deviceId)
    {
        if (IsDisposed ||
            _isLoading ||
            !_autoSwitchToNewPlaybackDeviceCheckBox.Checked ||
            string.IsNullOrWhiteSpace(deviceId))
        {
            return;
        }

        AppLogger.LogInfo($"Auto-switch candidate received. DeviceId={deviceId}");

        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (!_services.AudioDeviceService.IsPlaybackDevice(deviceId))
                {
                    AppLogger.LogInfo($"Auto-switch candidate is not an active playback device yet. DeviceId={deviceId}, Attempt={attempt + 1}");

                    if (attempt < 4)
                    {
                        await Task.Delay(400);
                        continue;
                    }

                    return;
                }

                var currentPlaybackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
                var currentCommunicationDevice = _services.AudioDeviceService.GetDefaultCommunicationDevice();
                if (currentPlaybackDevice is not null &&
                    currentCommunicationDevice is not null &&
                    string.Equals(currentPlaybackDevice.Id, deviceId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(currentCommunicationDevice.Id, deviceId, StringComparison.OrdinalIgnoreCase))
                {
                    AppLogger.LogInfo($"Auto-switch skipped because defaults already match. DeviceId={deviceId}");
                    return;
                }

                _ = ExecuteTrackedAudioChange(() =>
                {
                    _services.AudioDeviceService.SetDefaultPlaybackAndCommunicationDevice(deviceId);
                    return true;
                });
                AppLogger.LogInfo($"Auto-switch applied. DeviceId={deviceId}, Attempt={attempt + 1}");
                return;
            }
            catch (Exception ex)
            {
                if (attempt < 4)
                {
                    AppLogger.LogException($"MainShellForm.HandleNewPlaybackDeviceConnectedAsync attempt {attempt + 1}", ex);
                    await Task.Delay(400);
                    continue;
                }

                AppLogger.LogException("MainShellForm.HandleNewPlaybackDeviceConnectedAsync", ex);
            }
        }
    }

    private void EvaluateProcessProfiles(AppConfig? config = null)
    {
        if (_profileEvaluationInProgress || IsDisposed)
        {
            return;
        }

        _profileEvaluationInProgress = true;

        try
        {
            if (config is null)
            {
                config = _cachedConfig;
            }

            if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            if (config is null)
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            _cachedConfig = config;
            _services.Localizer.SetLanguage(config.Language);

            if (!config.EnableProfiles)
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            var matchResult = _processProfileMatcher.FindForegroundMatch(config.Profiles);
            RefreshProfileEvaluationTimer(matchResult.ShouldMonitorBackgroundState);

            if (matchResult.BlockingProfile is not null)
            {
                ClearActiveProcessProfile();
                return;
            }

            var matchedProfile = matchResult.MatchedProfile;
            if (matchedProfile is null)
            {
                ClearActiveProcessProfile();
                return;
            }

            var playbackDeviceId = matchedProfile.PlaybackDevice.Id;
            var recordingDeviceId = matchedProfile.RecordingDevice.Id;
            if (string.Equals(_activeProcessProfileId, matchedProfile.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeProcessProfilePlaybackDeviceId, playbackDeviceId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeProcessProfileRecordingDeviceId, recordingDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!TryApplyProcessProfile(matchedProfile, out var message, out var isError, out var devicesChanged))
            {
                if (isError && !string.IsNullOrWhiteSpace(message))
                {
                    SetStatus(message, isError: true);
                }

                return;
            }

            _activeProcessProfileId = matchedProfile.Id;
            _activeProcessProfilePlaybackDeviceId = matchedProfile.PlaybackDevice.Id;
            _activeProcessProfileRecordingDeviceId = matchedProfile.RecordingDevice.Id;

            if (!devicesChanged || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            SetStatus(message);
            _trayNotifications.Show(
                message,
                ToolTipIcon.Info,
                2500,
                imagePath: NotificationIconCatalog.ResolvePath(AppConfig.DefaultIconFileName));
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.EvaluateProcessProfiles", ex);
        }
        finally
        {
            _profileEvaluationInProgress = false;
        }
    }

    private bool TryApplyProcessProfile(
        ProcessAudioProfile profile,
        out string? message,
        out bool isError,
        out bool devicesChanged)
    {
        message = null;
        isError = false;
        devicesChanged = false;

        var missingDevices = new List<string>();
        var repairedSelections = false;
        AudioDeviceInfo? targetPlaybackDevice = null;
        if (!string.IsNullOrWhiteSpace(profile.PlaybackDevice.Id))
        {
            targetPlaybackDevice = ResolveProfileDeviceSelection(
                profile,
                isPlaybackDevice: true,
                out var playbackSelectionRepaired);
            repairedSelections |= playbackSelectionRepaired;

            if (targetPlaybackDevice is null)
            {
                missingDevices.Add(GetProfileDeviceLabel(profile.PlaybackDevice));
            }
        }

        AudioDeviceInfo? targetRecordingDevice = null;
        if (!string.IsNullOrWhiteSpace(profile.RecordingDevice.Id))
        {
            targetRecordingDevice = ResolveProfileDeviceSelection(
                profile,
                isPlaybackDevice: false,
                out var recordingSelectionRepaired);
            repairedSelections |= recordingSelectionRepaired;

            if (targetRecordingDevice is null)
            {
                missingDevices.Add(GetProfileDeviceLabel(profile.RecordingDevice));
            }
        }

        if (missingDevices.Count > 0)
        {
            isError = true;
            message = _services.Localizer.Format(
                "ErrorProfileMissingDevices",
                GetProfileDisplayName(profile),
                string.Join(", ", missingDevices.Where(name => !string.IsNullOrWhiteSpace(name))));
            return false;
        }

        if (repairedSelections)
        {
            PersistCachedProfileRepairs();
            RefreshSelectedProfileDeviceChoicesIfSelected(profile.Id);
        }

        var playbackChanged = false;
        var recordingChanged = false;

        try
        {
            _ = ExecuteTrackedAudioChange(() =>
            {
                if (targetPlaybackDevice is not null)
                {
                    var currentPlaybackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
                    if (currentPlaybackDevice is null ||
                        !string.Equals(currentPlaybackDevice.Id, targetPlaybackDevice.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        _services.AudioDeviceService.SetDefaultPlaybackAndCommunicationDevice(targetPlaybackDevice.Id);
                        playbackChanged = true;
                    }
                }

                if (targetRecordingDevice is not null)
                {
                    var currentRecordingDevice = _services.AudioDeviceService.GetDefaultRecordingDevice();
                    if (currentRecordingDevice is null ||
                        !string.Equals(currentRecordingDevice.Id, targetRecordingDevice.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        _services.AudioDeviceService.SetDefaultRecordingAndCommunicationDevice(targetRecordingDevice.Id);
                        recordingChanged = true;
                    }
                }

                return true;
            });
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.TryApplyProcessProfile", ex);
            isError = true;
            message = ex.Message;
            return false;
        }

        devicesChanged = playbackChanged || recordingChanged;
        message = _services.Localizer.Format("StatusProfileApplied", GetProfileDisplayName(profile));
        return true;
    }

    private static string GetProfileDeviceLabel(DeviceSelection selection)
    {
        return string.IsNullOrWhiteSpace(selection.Name)
            ? selection.Id
            : selection.Name;
    }

    private string GetProfileDisplayName(ProcessAudioProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? _services.Localizer.Get("ProfileUnnamed")
            : profile.Name;
    }

    private void ClearActiveProcessProfile()
    {
        _activeProcessProfileId = null;
        _activeProcessProfilePlaybackDeviceId = null;
        _activeProcessProfileRecordingDeviceId = null;
    }

    private async void BeginUpdateCheck()
    {
        if (_updateCheckInProgress || !_enableUpdateNotificationsCheckBox.Checked || IsDisposed)
        {
            return;
        }

        _updateCheckInProgress = true;

        try
        {
        var updateRelease = await _services.UpdateChecker.CheckForUpdateAsync();
        if (updateRelease is null || IsDisposed || !_enableUpdateNotificationsCheckBox.Checked)
        {
            return;
        }

        ApplyAvailableUpdate(updateRelease);
        }
        finally
        {
            _updateCheckInProgress = false;
        }
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

    private void ClearAvailableUpdate()
    {
        _availableUpdate = null;
        _updateDialogShown = false;
        _statusActionButton.Visible = false;
    }

    private static List<LanguageChoice> BuildLanguageChoices()
    {
        return
        [
            new LanguageChoice(AppLanguage.English, "English"),
            new LanguageChoice(AppLanguage.Korean, "\uD55C\uAD6D\uC5B4")
        ];
    }

    private const int SwRestore = 9;
    private const uint EventSystemForeground = 0x0003;
    private const uint WinEventOutOfContext = 0x0000;

    private delegate void WinEventDelegate(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWinEventHook(
        uint eventMin,
        uint eventMax,
        IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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

    private static void SetComboBoxItems<T>(ComboBox comboBox, IReadOnlyList<T> items)
    {
        comboBox.BeginUpdate();

        try
        {
            comboBox.DataSource = null;
            comboBox.Items.Clear();

            if (items.Count > 0)
            {
                comboBox.Items.AddRange(items.Cast<object>().ToArray());
            }

            comboBox.SelectedIndex = -1;
        }
        finally
        {
            comboBox.EndUpdate();
        }
    }

    private enum PageKind
    {
        Main,
        Settings
    }

    private enum SettingsSection
    {
        General,
        Automation,
        Shortcuts,
        Overlay,
        Profiles
    }

    private sealed class DeviceChoice
    {
        public DeviceChoice(string id, string label, string deviceName)
        {
            Id = id;
            Label = label;
            DeviceName = deviceName;
        }

        public string Id { get; }

        public string Label { get; }

        public string DeviceName { get; }
    }

    private sealed class ProfileListItem
    {
        public ProfileListItem(ProcessAudioProfile profile, string label)
        {
            Profile = profile;
            Label = label;
        }

        public ProcessAudioProfile Profile { get; }

        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
    }

    private sealed class ProgramListItem
    {
        public ProgramListItem(ProfileProgramTarget program, string label)
        {
            Program = program;
            Label = label;
        }

        public ProfileProgramTarget Program { get; }

        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
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
