# SoundDeviceSwitcher v2.0.0

## 🆕 v2.0.0 업데이트 내역

### ✨ 주요 변경 사항

- **스크립트 기반 도구를 독립 실행형 Windows 앱으로 전환**
  - 기존 PowerShell, batch, NirCmd 중심 흐름을 WinForms 기반 트레이 앱으로 재구성했습니다.
  - 별도 스크립트 실행 없이 앱에서 바로 장치 전환을 처리할 수 있습니다.

- **장치 전환과 설정 기능 통합**
  - 앱 안에서 장치 A / 장치 B를 직접 선택할 수 있습니다.
  - 현재 기본 장치를 기준으로 다른 장치로 자동 토글합니다.
  - 기본 재생 장치와 기본 통신 장치를 함께 전환합니다.
  - 전역 핫키와 토글 바로가기 생성을 지원합니다.

- **사용성 및 UI 개선**
  - 한국어 / 영어 UI를 지원합니다.
  - 시스템 / 라이트 / 다크 테마를 지원합니다.
  - 커스텀 트레이 토스트 알림과 사용자 아이콘 폴더 기반 아이콘 선택을 지원합니다.
  - 시작 프로그램, 시작 시 최소화, 닫을 때 트레이로 최소화 옵션을 추가했습니다.

- **배포 및 업데이트 기능 추가**
  - GitHub 릴리스 업데이트 감지를 지원합니다.
  - 포터블 `.zip` 빌드와 설치형 `setup.exe` 패키지를 함께 제공합니다.

---

## 🔄 Changelog v2.0.0

### ✨ What’s New

- **Script-Based Workflow Replaced with a Standalone Windows App**
  - The previous PowerShell, batch, and NirCmd-driven workflow was rebuilt as a WinForms tray application.
  - Device switching can now be handled directly inside the app without separate scripts.

- **Integrated Device Switching and Settings**
  - Device A and Device B can be selected directly from the app.
  - Switching automatically toggles based on the current default device.
  - The app updates both the default playback device and the default communications device.
  - Global hotkeys and toggle shortcut creation are supported.

- **Usability and UI Improvements**
  - Korean and English UI are supported.
  - System, light, and dark themes are available.
  - Custom tray toast notifications and user icon folder-based icon selection are included.
  - Start with Windows, start minimized, and minimize-to-tray options were added.

- **Release and Update Features**
  - GitHub release update detection is supported.
  - Both portable `.zip` builds and installer `setup.exe` packages are provided.
