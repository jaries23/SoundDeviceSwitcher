#define MyAppId "{{F61E10E4-29A5-4B8F-9F27-0B7D39F0A951}"
#define MyAppName "SoundDeviceSwitcher"
#define MyAppPublisher "jaries23"
#define MyAppUrl "https://github.com/jaries23/SoundDeviceSwitcher"

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#ifndef MyPublishDir
  #define MyPublishDir "..\artifacts\publish\win-x64"
#endif

#ifndef MyOutputDir
  #define MyOutputDir "..\artifacts\release"
#endif

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}
AppUpdatesURL={#MyAppUrl}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
OutputDir={#MyOutputDir}
OutputBaseFilename=setup
SetupIconFile=..\src\SoundDeviceSwitcher.App\assets\icons\default.ico
UninstallDisplayIcon={app}\SoundDeviceSwitcher.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}\{#MyAppName}"; Filename: "{app}\SoundDeviceSwitcher.exe"; WorkingDir: "{app}"
Name: "{autoprograms}\{#MyAppName}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\SoundDeviceSwitcher.exe"; Tasks: desktopicon; WorkingDir: "{app}"

[Run]
Filename: "{app}\SoundDeviceSwitcher.exe"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
