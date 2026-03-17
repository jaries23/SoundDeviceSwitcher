using System.Globalization;

namespace SoundDeviceSwitcher.App.Localization;

public sealed class LocalizationService
{
    private readonly Dictionary<string, string> _english = new(StringComparer.Ordinal)
    {
        ["AppName"] = "SoundDeviceSwitcher",
        ["SidebarMenuTitle"] = "Menu",
        ["MainTab"] = "Main",
        ["SettingsTab"] = "Settings",
        ["ActionsGroup"] = "Quick actions",
        ["GeneralGroup"] = "General",
        ["LanguageLabel"] = "Language",
        ["ThemeLabel"] = "Theme",
        ["StartWithWindowsLabel"] = "Start at Windows startup",
        ["StartMinimizedAtStartupLabel"] = "Start minimized at system startup",
        ["MinimizeToTrayOnCloseLabel"] = "Minimize to tray when closing",
        ["EnableUpdateNotificationsLabel"] = "Notify about app updates",
        ["DeviceAIconLabel"] = "Device A icon",
        ["DeviceBIconLabel"] = "Device B icon",
        ["ThemeModeSystem"] = "System",
        ["ThemeModeLight"] = "Light",
        ["ThemeModeDark"] = "Dark",
        ["DevicesGroup"] = "Devices",
        ["DeviceALabel"] = "Device A",
        ["DeviceBLabel"] = "Device B",
        ["HotkeyGroup"] = "Hotkey (optional)",
        ["EnableGlobalHotkey"] = "Enable global hotkey",
        ["ModifiersLabel"] = "Modifiers",
        ["KeyLabel"] = "Key",
        ["HotkeyHint"] = "Global hotkeys work while the app is running, including when it is hidden in the tray.",
        ["ToggleNowButton"] = "Toggle now",
        ["RefreshDevicesButton"] = "Refresh devices",
        ["OpenIconFolderButton"] = "Open icon folder",
        ["RefreshIconsButton"] = "Refresh icons",
        ["CreateToggleShortcutButton"] = "Create toggle shortcut",
        ["InitialStatus"] = "Choose two playback devices. Changes are saved automatically.",
        ["LoadedSavedSettings"] = "Loaded your saved settings.",
        ["NoSavedSettings"] = "No saved settings yet. Choose two devices to get started.",
        ["StatusSettingsSaved"] = "Settings saved.",
        ["StatusCreatedShortcut"] = "Created toggle shortcut: {0}",
        ["StatusLanguageChanged"] = "Language updated.",
        ["StatusThemeChanged"] = "Theme updated.",
        ["StatusUpdateAvailable"] = "A new update is available: {0}",
        ["StatusMinimizedToTray"] = "The app is still running in the system tray.",
        ["StatusOpenedIconFolder"] = "Opened the icon folder.",
        ["StatusIconsRefreshed"] = "Reloaded {0} icon files from the built-in and custom icon folders.",
        ["StatusNoIconsFound"] = "No selectable icons were found in the built-in or custom icon folders.",
        ["ErrorNeedTwoPlaybackDevices"] = "Connect at least two playback devices to continue.",
        ["ErrorPrimarySecondaryDifferent"] = "Device A and Device B must be different.",
        ["ErrorEnableModifier"] = "Enable at least one modifier key for the global hotkey.",
        ["ErrorNoConfig"] = "No saved configuration was found. Open the app and choose your device settings first.",
        ["ErrorCouldNotLoadConfig"] = "Could not load the saved configuration: {0}",
        ["ErrorEmptyConfig"] = "The saved configuration file is empty.",
        ["ErrorCouldNotReadAudioDevices"] = "Windows audio devices could not be read. HRESULT: {0}. {1}",
        ["ErrorAppCouldNotLoad"] = "The app could not finish loading.",
        ["ErrorGlobalHotkeyUnavailable"] = "The global hotkey could not be registered. Another app may already be using it.",
        ["MessageConfigureBeforeToggle"] = "Configure the app before running toggle mode.",
        ["ToggleMissingDevices"] = "Both configured devices must be connected before switching. Missing: {0}.",
        ["ToggleSwitched"] = "Switched to {0}.",
        ["ToggleFailed"] = "Could not switch devices: {0}",
        ["MenuToggleNow"] = "Toggle now",
        ["MenuOpenSettings"] = "Open settings",
        ["MenuExit"] = "Exit",
        ["NotifyNoConfig"] = "No saved configuration was found.",
        ["IconDefault"] = "Default",
        ["UpdateStatusButton"] = "View update",
        ["UpdateDialogWindowTitle"] = "Update available",
        ["UpdateDialogTitle"] = "A new update is available",
        ["UpdateDialogMessage"] = "Version {0} has been published. Open the release page to download the latest build.",
        ["UpdateDialogCurrentVersion"] = "Current version: {0}",
        ["UpdateDialogLatestVersion"] = "Latest version: {0}",
        ["UpdateDialogPublishedAt"] = "Published: {0}",
        ["UpdateDialogPublishedUnknown"] = "Published date: unavailable",
        ["UpdateDialogOpenButton"] = "Open download page",
        ["UpdateDialogLaterButton"] = "Later",
        ["ShortcutStartupDescription"] = "Launch SoundDeviceSwitcher at sign-in.",
        ["ShortcutToggleDescription"] = "Toggle between the configured sound devices."
    };

    private readonly Dictionary<string, string> _korean = new(StringComparer.Ordinal)
    {
        ["AppName"] = "SoundDeviceSwitcher",
        ["SidebarMenuTitle"] = "메뉴",
        ["MainTab"] = "메인",
        ["SettingsTab"] = "설정",
        ["ActionsGroup"] = "빠른 작업",
        ["GeneralGroup"] = "일반",
        ["LanguageLabel"] = "언어",
        ["ThemeLabel"] = "테마",
        ["StartWithWindowsLabel"] = "Windows 시작 시 실행",
        ["StartMinimizedAtStartupLabel"] = "시스템 시작 시 최소화해서 실행",
        ["MinimizeToTrayOnCloseLabel"] = "닫을 때 트레이로 최소화",
        ["EnableUpdateNotificationsLabel"] = "앱 업데이트 알림 사용",
        ["DeviceAIconLabel"] = "장치 A 아이콘",
        ["DeviceBIconLabel"] = "장치 B 아이콘",
        ["ThemeModeSystem"] = "시스템",
        ["ThemeModeLight"] = "라이트",
        ["ThemeModeDark"] = "다크",
        ["DevicesGroup"] = "장치",
        ["DeviceALabel"] = "장치 A",
        ["DeviceBLabel"] = "장치 B",
        ["HotkeyGroup"] = "핫키 (선택)",
        ["EnableGlobalHotkey"] = "전역 핫키 사용",
        ["ModifiersLabel"] = "조합 키",
        ["KeyLabel"] = "키",
        ["HotkeyHint"] = "전역 핫키는 앱이 실행 중일 때 동작하며, 트레이에 숨겨져 있어도 사용할 수 있습니다.",
        ["ToggleNowButton"] = "지금 전환",
        ["RefreshDevicesButton"] = "장치 새로고침",
        ["OpenIconFolderButton"] = "아이콘 폴더 열기",
        ["RefreshIconsButton"] = "아이콘 새로고침",
        ["CreateToggleShortcutButton"] = "토글 바로가기 만들기",
        ["InitialStatus"] = "재생 장치 두 개를 선택하세요. 변경 내용은 자동으로 저장됩니다.",
        ["LoadedSavedSettings"] = "저장된 설정을 불러왔습니다.",
        ["NoSavedSettings"] = "저장된 설정이 없습니다. 장치 두 개를 선택해서 시작하세요.",
        ["StatusSettingsSaved"] = "설정을 저장했습니다.",
        ["StatusCreatedShortcut"] = "토글 바로가기를 만들었습니다: {0}",
        ["StatusLanguageChanged"] = "언어를 바꿨습니다.",
        ["StatusThemeChanged"] = "테마를 바꿨습니다.",
        ["StatusUpdateAvailable"] = "새 업데이트가 있습니다: {0}",
        ["StatusMinimizedToTray"] = "앱이 시스템 트레이에서 계속 실행 중입니다.",
        ["StatusOpenedIconFolder"] = "아이콘 폴더를 열었습니다.",
        ["StatusIconsRefreshed"] = "기본 및 사용자 아이콘 폴더에서 아이콘 {0}개를 다시 불러왔습니다.",
        ["StatusNoIconsFound"] = "기본 또는 사용자 아이콘 폴더에서 선택할 수 있는 아이콘을 찾지 못했습니다.",
        ["ErrorNeedTwoPlaybackDevices"] = "계속하려면 재생 장치 두 개 이상이 연결되어 있어야 합니다.",
        ["ErrorPrimarySecondaryDifferent"] = "장치 A와 장치 B는 서로 달라야 합니다.",
        ["ErrorEnableModifier"] = "전역 핫키를 사용하려면 보조 키를 하나 이상 선택하세요.",
        ["ErrorNoConfig"] = "저장된 설정이 없습니다. 앱을 열어 장치를 먼저 설정하세요.",
        ["ErrorCouldNotLoadConfig"] = "저장된 설정을 불러오지 못했습니다: {0}",
        ["ErrorEmptyConfig"] = "저장된 설정 파일이 비어 있습니다.",
        ["ErrorCouldNotReadAudioDevices"] = "Windows 오디오 장치를 읽지 못했습니다. HRESULT: {0}. {1}",
        ["ErrorAppCouldNotLoad"] = "앱을 정상적으로 불러오지 못했습니다.",
        ["ErrorGlobalHotkeyUnavailable"] = "전역 핫키를 등록하지 못했습니다. 다른 앱이 이미 사용 중일 수 있습니다.",
        ["MessageConfigureBeforeToggle"] = "토글 모드를 사용하기 전에 앱을 먼저 설정하세요.",
        ["ToggleMissingDevices"] = "설정한 두 장치가 모두 연결되어 있어야 전환할 수 있습니다. 누락: {0}.",
        ["ToggleSwitched"] = "{0}(으)로 전환했습니다.",
        ["ToggleFailed"] = "장치 전환 실패: {0}",
        ["MenuToggleNow"] = "지금 전환",
        ["MenuOpenSettings"] = "설정 열기",
        ["MenuExit"] = "종료",
        ["NotifyNoConfig"] = "저장된 설정이 없습니다.",
        ["IconDefault"] = "기본",
        ["UpdateStatusButton"] = "업데이트 보기",
        ["UpdateDialogWindowTitle"] = "업데이트 안내",
        ["UpdateDialogTitle"] = "새 업데이트가 있습니다",
        ["UpdateDialogMessage"] = "버전 {0}이(가) 배포되었습니다. 최신 빌드를 받으려면 릴리스 페이지를 여세요.",
        ["UpdateDialogCurrentVersion"] = "현재 버전: {0}",
        ["UpdateDialogLatestVersion"] = "최신 버전: {0}",
        ["UpdateDialogPublishedAt"] = "배포 시각: {0}",
        ["UpdateDialogPublishedUnknown"] = "배포 시각: 확인할 수 없음",
        ["UpdateDialogOpenButton"] = "다운로드 페이지 열기",
        ["UpdateDialogLaterButton"] = "나중에",
        ["ShortcutStartupDescription"] = "로그인 후 SoundDeviceSwitcher를 실행합니다.",
        ["ShortcutToggleDescription"] = "설정한 사운드 장치 사이를 전환합니다."
    };

    public LocalizationService()
    {
        CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase)
            ? AppLanguage.Korean
            : AppLanguage.English;
    }

    public AppLanguage CurrentLanguage { get; private set; }

    public void SetLanguage(AppLanguage language)
    {
        CurrentLanguage = language;
    }

    public string Get(string key)
    {
        var dictionary = CurrentLanguage == AppLanguage.Korean ? _korean : _english;
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        return _english.TryGetValue(key, out var fallback) ? fallback : key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(Get(key), args);
    }
}
