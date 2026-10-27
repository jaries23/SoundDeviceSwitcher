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

internal sealed partial class MainShellForm : Form
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
    private readonly RoundedPanel _mainProgramToggleCard;
    private readonly RoundedPanel _settingsGeneralCard;
    private readonly RoundedPanel _settingsAutomationCard;
    private readonly RoundedPanel _settingsHotkeyCard;
    private readonly RoundedPanel _settingsToggleRulesCard;
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
    private readonly PillRadioButton _settingsToggleRulesTabButton;
    private readonly PillRadioButton _settingsOverlayTabButton;
    private readonly PillRadioButton _settingsProfilesTabButton;
    private readonly Label _brandLabel;
    private readonly Label _pageTitleLabel;
    private readonly Label _deviceCardTitleLabel;
    private readonly Label _switchCardTitleLabel;
    private readonly Label _mainProgramToggleTitleLabel;
    private readonly Label _generalTitleLabel;
    private readonly Label _automationTitleLabel;
    private readonly Label _hotkeyTitleLabel;
    private readonly Label _toggleRulesTitleLabel;
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
    private readonly Label _mainProgramToggleProgramsLabel;
    private readonly Label _mainProgramTogglePrimaryDeviceLabel;
    private readonly Label _mainProgramToggleSecondaryDeviceLabel;
    private readonly Label _toggleRuleNameLabel;
    private readonly Label _toggleRulePriorityLabel;
    private readonly Label _toggleRulePrimaryDeviceLabel;
    private readonly Label _toggleRuleSecondaryDeviceLabel;
    private readonly Label _toggleRuleProgramsLabel;
    private readonly Label _toggleRuleOrderHintLabel;
    private readonly CheckBox _enableProfilesCheckBox;
    private readonly CheckBox _mainProgramToggleVolumeMixerCheckBox;
    private readonly CheckBox _toggleRuleVolumeMixerCheckBox;
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
    private readonly ComboBox _mainProgramTogglePrimaryDeviceComboBox;
    private readonly ComboBox _mainProgramToggleSecondaryDeviceComboBox;
    private readonly ComboBox _toggleRulePrimaryDeviceComboBox;
    private readonly ComboBox _toggleRuleSecondaryDeviceComboBox;
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
    private readonly RoundedButton _addToggleRuleButton;
    private readonly RoundedButton _removeToggleRuleButton;
    private readonly RoundedButton _moveToggleRuleUpButton;
    private readonly RoundedButton _moveToggleRuleDownButton;
    private readonly RoundedButton _addToggleRuleProgramButton;
    private readonly RoundedButton _removeToggleRuleProgramButton;
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
    private readonly List<ProgramToggleRule> _editableToggleRules = [];
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
    private bool _suppressMainProgramToggleEditorEvents;
    private bool _suppressMainProgramToggleListSelectionEvents;
    private bool _suppressProfileEditorEvents;
    private bool _suppressProfileListSelectionEvents;
    private bool _suppressToggleRuleEditorEvents;
    private bool _suppressToggleRuleListSelectionEvents;
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
    private ListBox? _mainProgramToggleProgramsListBox;
    private ListBox? _toggleRulesListBox;
    private ListBox? _toggleRuleProgramsListBox;
    private TextBox? _profileNameTextBox;
    private TextBox? _profileOrderTextBox;
    private TextBox? _toggleRuleNameTextBox;
    private TextBox? _toggleRuleOrderTextBox;
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
        _mainProgramToggleCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsGeneralCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsAutomationCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsHotkeyCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsToggleRulesCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsOverlayCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _settingsProfilesCard = CreateCard(autoSize: true, dock: DockStyle.Top, cornerRadius: SurfaceCornerRadius);
        _sidebarCard.Padding = new Padding(16);
        _headerCard.Padding = new Padding(14, 12, 14, 12);
        _statusCard.Padding = new Padding(14, 10, 14, 10);
        _mainDeviceCard.Padding = new Padding(18);
        _mainSwitchCard.Padding = new Padding(18);
        _mainProgramToggleCard.Padding = new Padding(18);
        _settingsGeneralCard.Padding = new Padding(18);
        _settingsAutomationCard.Padding = new Padding(18);
        _settingsHotkeyCard.Padding = new Padding(18);
        _settingsToggleRulesCard.Padding = new Padding(18);
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
        _settingsToggleRulesTabButton = CreateSettingsTabButton();
        _settingsToggleRulesTabButton.CheckedChanged += (_, _) =>
        {
            if (_settingsToggleRulesTabButton.Checked)
            {
                SetSettingsSection(SettingsSection.ToggleRules);
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
        _mainProgramToggleTitleLabel = CreateSectionTitleLabel();
        _generalTitleLabel = CreateSectionTitleLabel();
        _automationTitleLabel = CreateSectionTitleLabel();
        _hotkeyTitleLabel = CreateSectionTitleLabel();
        _toggleRulesTitleLabel = CreateSectionTitleLabel();
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
        _mainProgramToggleProgramsLabel = CreateFieldLabel();
        _mainProgramTogglePrimaryDeviceLabel = CreateFieldLabel();
        _mainProgramToggleSecondaryDeviceLabel = CreateFieldLabel();
        _toggleRuleNameLabel = CreateFieldLabel();
        _toggleRulePriorityLabel = CreateFieldLabel();
        _toggleRulePrimaryDeviceLabel = CreateFieldLabel();
        _toggleRuleSecondaryDeviceLabel = CreateFieldLabel();
        _toggleRuleProgramsLabel = CreateFieldLabel();
        _toggleRuleOrderHintLabel = new Label
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
        _mainProgramToggleVolumeMixerCheckBox = new CheckBox
        {
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };
        _mainProgramToggleVolumeMixerCheckBox.CheckedChanged += (_, _) => HandleSelectedMainProgramToggleChanged();
        _toggleRuleVolumeMixerCheckBox = new CheckBox
        {
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };
        _toggleRuleVolumeMixerCheckBox.CheckedChanged += (_, _) => HandleSelectedToggleRuleChanged();
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
        _mainProgramTogglePrimaryDeviceComboBox = CreateProfileDeviceComboBox();
        _mainProgramTogglePrimaryDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedMainProgramToggleChanged();
        _mainProgramToggleSecondaryDeviceComboBox = CreateProfileDeviceComboBox();
        _mainProgramToggleSecondaryDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedMainProgramToggleChanged();
        _toggleRulePrimaryDeviceComboBox = CreateProfileDeviceComboBox();
        _toggleRulePrimaryDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedToggleRuleChanged();
        _toggleRuleSecondaryDeviceComboBox = CreateProfileDeviceComboBox();
        _toggleRuleSecondaryDeviceComboBox.SelectedIndexChanged += (_, _) => HandleSelectedToggleRuleChanged();
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
        _mainProgramToggleProgramsListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 136,
            Margin = new Padding(0, 6, 0, 0),
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        _mainProgramToggleProgramsListBox.SelectedIndexChanged += (_, _) => HandleMainProgramToggleSelectionChanged();
        _toggleRuleNameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 4)
        };
        _toggleRuleNameTextBox.TextChanged += (_, _) => HandleSelectedToggleRuleChanged();
        _toggleRuleOrderTextBox = new TextBox
        {
            Dock = DockStyle.Left,
            Width = 120,
            Margin = new Padding(0, 4, 0, 4),
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        _toggleRulesListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 160,
            Margin = new Padding(0),
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        _toggleRulesListBox.SelectedIndexChanged += (_, _) => HandleToggleRuleSelectionChanged();
        _toggleRuleProgramsListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Height = 92,
            Margin = new Padding(0, 4, 0, 4),
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle
        };
        _toggleRuleProgramsListBox.SelectedIndexChanged += (_, _) => UpdateToggleRuleProgramButtons();

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
        _addToggleRuleButton = CreateActionButton("secondary", new Size(120, 38));
        _addToggleRuleButton.Click += (_, _) => AddToggleRule();
        _removeToggleRuleButton = CreateActionButton("secondary", new Size(120, 38));
        _removeToggleRuleButton.Click += (_, _) => RemoveSelectedToggleRule();
        _moveToggleRuleUpButton = CreateIconActionButton("↑");
        _moveToggleRuleUpButton.Click += (_, _) => MoveSelectedToggleRule(-1);
        _moveToggleRuleDownButton = CreateIconActionButton("↓");
        _moveToggleRuleDownButton.Click += (_, _) => MoveSelectedToggleRule(1);
        _addToggleRuleProgramButton = CreateActionButton("secondary", new Size(120, 38));
        _addToggleRuleProgramButton.Click += (_, _) => AddProgramToSelectedToggleRule();
        _removeToggleRuleProgramButton = CreateActionButton("secondary", new Size(120, 38));
        _removeToggleRuleProgramButton.Click += (_, _) => RemoveSelectedProgramFromToggleRule();
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
        ToggleRules,
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

    private sealed class ToggleRuleListItem
    {
        public ToggleRuleListItem(ProgramToggleRule rule, string label)
        {
            Rule = rule;
            Label = label;
        }

        public ProgramToggleRule Rule { get; }

        public string Label { get; }

        public override string ToString()
        {
            return Label;
        }
    }

    private sealed class MainProgramToggleListItem
    {
        public MainProgramToggleListItem(ProgramToggleRule rule, ProfileProgramTarget program, string label)
        {
            Rule = rule;
            Program = program;
            ProgramKey = !string.IsNullOrWhiteSpace(program.ExecutablePath)
                ? program.ExecutablePath
                : !string.IsNullOrWhiteSpace(program.ExecutableName)
                    ? program.ExecutableName
                    : program.DisplayName;
            Label = label;
        }

        public ProgramToggleRule Rule { get; }

        public ProfileProgramTarget Program { get; }

        public string ProgramKey { get; }

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
