# SoundDeviceSwitcher

## 한국어

SoundDeviceSwitcher는 Windows에서 두 개의 재생 장치를 빠르게 전환하기 위한 트레이 앱입니다.

### 주요 기능

- 앱 안에서 장치 A / 장치 B 직접 선택
- 현재 기본 장치를 기준으로 다른 장치로 자동 전환
- 기본 재생 장치와 기본 통신 장치를 함께 변경
- 장치 재연결시 해당 장치로 자동 전환
- 선택형 전역 핫키 지원
- 트레이 알림 및 테마 연동
- 한국어 / 영어 UI 지원
- 시작 프로그램, 시작 시 최소화, 닫을 때 트레이로 최소화 지원
- GitHub 릴리즈 업데이트 확인

### 릴리즈 파일

- `SoundDeviceSwitcher-v2.1.0-setup.exe`: 설치형 빌드
- `SoundDeviceSwitcher-v2.1.0-win-x64.zip`: 포터블 빌드

### 빌드

```powershell
dotnet build src\SoundDeviceSwitcher.App\SoundDeviceSwitcher.App.csproj
```

### 릴리즈 패키지 생성

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Publish-Release.ps1
```

생성 결과는 `artifacts/release/` 아래에 저장됩니다.

## English

SoundDeviceSwitcher is a standalone Windows tray app that lets you configure two playback devices and switch between them from the app, the tray, a global hotkey, or a toggle shortcut. It detects the current default playback device, switches to the other configured device, updates both the default playback and default communications device, and shows a notification with the result.

### Highlights

- Select device A and device B directly in the app
- Toggle automatically based on the current default device
- Set both the default playback device and default communications device
- Optional global hotkey support
- Tray notifications with theme support
- Korean and English UI
- Start with Windows, start minimized, and minimize-to-tray options
- GitHub release update check

### Release Assets

- `SoundDeviceSwitcher-v2.1.0-setup.exe`: installer build
- `SoundDeviceSwitcher-v2.1.0-win-x64.zip`: portable build

### Build

```powershell
dotnet build src\SoundDeviceSwitcher.App\SoundDeviceSwitcher.App.csproj
```

### Publish Release Assets

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Publish-Release.ps1
```

The generated files are written to `artifacts/release/`.
