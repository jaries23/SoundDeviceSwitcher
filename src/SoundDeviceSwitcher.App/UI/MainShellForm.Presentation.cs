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
            _mainProgramToggleTitleLabel.Text = _services.Localizer.Get("MainProgramToggleGroup");
            _generalTitleLabel.Text = _services.Localizer.Get("GeneralGroup");
            _automationTitleLabel.Text = _services.Localizer.Get("AutomationGroup");
            _hotkeyTitleLabel.Text = _services.Localizer.Get("HotkeyGroup");
            _toggleRulesTitleLabel.Text = _services.Localizer.Get("ToggleRulesGroup");
            _overlayTitleLabel.Text = _services.Localizer.Get("OverlayGroup");
            _profilesTitleLabel.Text = _services.Localizer.Get("ProfilesGroup");
            _settingsGeneralTabButton.Text = _services.Localizer.Get("GeneralGroup");
            _settingsAutomationTabButton.Text = _services.Localizer.Get("AutomationGroup");
            _settingsShortcutsTabButton.Text = _services.Localizer.Get("HotkeyGroup");
            _settingsToggleRulesTabButton.Text = _services.Localizer.Get("ToggleRulesGroup");
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
            _mainProgramToggleProgramsLabel.Text = _services.Localizer.Get("MainProgramToggleProgramsLabel");
            _mainProgramTogglePrimaryDeviceLabel.Text = _services.Localizer.Get("ToggleRulePrimaryDeviceLabel");
            _mainProgramToggleSecondaryDeviceLabel.Text = _services.Localizer.Get("ToggleRuleSecondaryDeviceLabel");
            _mainProgramToggleVolumeMixerCheckBox.Text = _services.Localizer.Get("ToggleRuleVolumeMixerLabel");
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
            _toggleRuleNameLabel.Text = _services.Localizer.Get("ToggleRuleNameLabel");
            _toggleRulePriorityLabel.Text = _services.Localizer.Get("ToggleRulePriorityLabel");
            _toggleRulePrimaryDeviceLabel.Text = _services.Localizer.Get("ToggleRulePrimaryDeviceLabel");
            _toggleRuleSecondaryDeviceLabel.Text = _services.Localizer.Get("ToggleRuleSecondaryDeviceLabel");
            _toggleRuleVolumeMixerCheckBox.Text = _services.Localizer.Get("ToggleRuleVolumeMixerLabel");
            _toggleRuleProgramsLabel.Text = _services.Localizer.Get("ToggleRuleProgramsLabel");
            _toggleRuleOrderHintLabel.Text = _services.Localizer.Get("ToggleRuleOrderHint");

            _toggleNowButton.Text = _services.Localizer.Get("ToggleNowButton");
            _refreshDevicesButton.Text = _services.Localizer.Get("RefreshDevicesButton");
            _openIconFolderButton.Text = _services.Localizer.Get("OpenIconFolderButton");
            _refreshIconsButton.Text = _services.Localizer.Get("RefreshIconsButton");
            _createShortcutButton.Text = _services.Localizer.Get("CreateToggleShortcutButton");
            _openProfileOverlayButton.Text = _services.Localizer.Get("OpenProfileOverlayButton");
            _statusActionButton.Text = _services.Localizer.Get("UpdateStatusButton");
            _addProfileButton.Text = _services.Localizer.Get("AddProfileButton");
            _removeProfileButton.Text = _services.Localizer.Get("RemoveProfileButton");
            _addToggleRuleButton.Text = _services.Localizer.Get("AddToggleRuleButton");
            _removeToggleRuleButton.Text = _services.Localizer.Get("RemoveToggleRuleButton");
            _addProfileProgramButton.Text = _services.Localizer.Get("AddProgramButton");
            _removeProfileProgramButton.Text = _services.Localizer.Get("RemoveProgramButton");
            _addToggleRuleProgramButton.Text = _services.Localizer.Get("AddProgramButton");
            _removeToggleRuleProgramButton.Text = _services.Localizer.Get("RemoveProgramButton");

            if (refreshIconChoices)
            {
                RefreshAvailableIcons(preserveSelections: true);
            }

            RefreshProfileLists(refreshDeviceChoices: true);
            RefreshMainProgramToggleList(refreshDeviceChoices: true);
            RefreshToggleRuleLists(refreshDeviceChoices: true);

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

    private const int WmSetRedraw = 0x000B;
    private const uint RdwInvalidate = 0x0001;
    private const uint RdwErase = 0x0004;
    private const uint RdwAllChildren = 0x0080;
    private const uint RdwUpdateNow = 0x0100;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

    private static void SuspendDrawing(Control? control)
    {
        if (control is { IsHandleCreated: true })
        {
            SendMessage(control.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
        }
    }

    private static void ResumeDrawing(Control? control)
    {
        if (control is { IsHandleCreated: true })
        {
            SendMessage(control.Handle, WmSetRedraw, (IntPtr)1, IntPtr.Zero);

            // Repaint the container and all of its children synchronously in a single
            // pass so the finished page appears at once instead of empty boxes that
            // fill in afterwards (the "ghosting" seen when redraw resumes).
            RedrawWindow(
                control.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                RdwInvalidate | RdwErase | RdwAllChildren | RdwUpdateNow);
        }
    }

    private void ApplyTheme(AppThemeMode themeMode, Control? root = null)
    {
        _activePalette = ThemeManager.Apply(root ?? this, themeMode);
        _trayNotifications.SetThemeMode(themeMode);
        _brandLabel.ForeColor = _activePalette.Text;
        _pageTitleLabel.ForeColor = _activePalette.Text;
        _statusLabel.ForeColor = _lastStatusWasError ? _activePalette.ErrorText : _activePalette.SuccessText;
        _hotkeyNoteLabel.ForeColor = _activePalette.MutedText;
        _recentSwitchUndoNoteLabel.ForeColor = _activePalette.MutedText;
        _overlayHintLabel.ForeColor = _activePalette.MutedText;
        _profileOrderHintLabel.ForeColor = _activePalette.MutedText;
        _profileAutomationHintLabel.ForeColor = _activePalette.MutedText;
        _toggleRuleOrderHintLabel.ForeColor = _activePalette.MutedText;
    }

    private string GetSettingsSectionTitle(SettingsSection section)
    {
        return section switch
        {
            SettingsSection.General => _services.Localizer.Get("GeneralGroup"),
            SettingsSection.Automation => _services.Localizer.Get("AutomationGroup"),
            SettingsSection.Shortcuts => _services.Localizer.Get("HotkeyGroup"),
            SettingsSection.ToggleRules => _services.Localizer.Get("ToggleRulesGroup"),
            SettingsSection.Overlay => _services.Localizer.Get("OverlayGroup"),
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
        SuspendDrawing(_pageHost);
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

            // Both pages are already themed; only the sidebar nav buttons change
            // selection state here, so re-theme just that strip instead of the whole form.
            ApplyTheme(GetSelectedThemeMode(), _mainNavButton.Parent);
        }
        finally
        {
            _pageHost.ResumeLayout(performLayout: true);
            ResumeDrawing(_pageHost);
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
        // Batch layout only. No paint suspension here: every card is already parented
        // (so there is no re-parent flash to hide), and cards have different heights, so
        // suppressing redraw would skip erasing the previous card's vacated area and leave
        // it showing behind the new one. A plain visibility swap erases and repaints cleanly.
        _settingsContentHost.SuspendLayout();

        try
        {
            _currentSettingsSection = section;

            if (updateRadioSelection)
            {
                _settingsGeneralTabButton.Checked = section == SettingsSection.General;
                _settingsAutomationTabButton.Checked = section == SettingsSection.Automation;
                _settingsShortcutsTabButton.Checked = section == SettingsSection.Shortcuts;
                _settingsToggleRulesTabButton.Checked = section == SettingsSection.ToggleRules;
                _settingsOverlayTabButton.Checked = section == SettingsSection.Overlay;
                _settingsProfilesTabButton.Checked = section == SettingsSection.Profiles;
            }

            // Every card is already parented to the host; switching sections only toggles
            // which one is visible. No card is re-parented, so there is no handle
            // recreation and no flash of a half-rendered frame.
            var activeCard = GetSettingsSectionCard(section);
            foreach (Control card in _settingsContentHost.Controls)
            {
                card.Visible = card == activeCard;
            }

            activeCard.BringToFront();
            UpdatePageHeader();
            _pageHost.AutoScrollPosition = Point.Empty;

            if (_shellLayout is not null)
            {
                // Only the settings tab buttons and the shown card change here, so
                // re-theme just the settings page rather than the entire form.
                ApplyTheme(GetSelectedThemeMode(), _settingsPage);
            }
        }
        finally
        {
            _settingsContentHost.ResumeLayout(performLayout: true);
        }
    }

    private Control GetSettingsSectionCard(SettingsSection section)
    {
        return section switch
        {
            SettingsSection.General => _settingsGeneralCard,
            SettingsSection.Automation => _settingsAutomationCard,
            SettingsSection.Shortcuts => _settingsHotkeyCard,
            SettingsSection.ToggleRules => _settingsToggleRulesCard,
            SettingsSection.Overlay => _settingsOverlayCard,
            SettingsSection.Profiles => _settingsProfilesCard,
            _ => _settingsGeneralCard
        };
    }

}
