# SoundDeviceSwitcher

SoundDeviceSwitcher is a Windows tray app for switching between two playback devices.

## Highlights

- Select two playback devices in the app
- Toggle between them automatically based on the current default device
- Set both the default playback device and default communications device
- Optional global hotkey
- Tray notifications with theme support
- English and Korean UI
- Start with Windows, start minimized, and minimize-to-tray options
- GitHub release update check

## Release Assets

- `setup.exe`: installer build
- `SoundDeviceSwitcher-v2.0.0-win-x64.zip`: portable build
- `SHA256SUMS.txt`: SHA-256 hashes for release files

## Build

```powershell
dotnet build src\SoundDeviceSwitcher.App\SoundDeviceSwitcher.App.csproj
```

## Publish Release Assets

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Publish-Release.ps1
```

This produces release files under `artifacts/release/`.
