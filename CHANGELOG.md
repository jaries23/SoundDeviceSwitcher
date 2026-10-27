# Changelog

이 프로젝트의 모든 주요 변경 사항은 이 파일에 기록됩니다.
All notable changes to this project are documented in this file.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를 따르며, 이 프로젝트는 [유의적 버전(SemVer)](https://semver.org/lang/ko/)을 따릅니다.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.2.1] - 2026-10-27

### Fixed

- **탭 전환 잔상·겹침·깜빡임 제거 / Smoother tab switching** — 설정 섹션을 전환할 때 이전 카드가 뒤에 겹쳐 보이거나, 내용이 채워지기 전 반쯤 그려진 빈 영역(잔상)이 먼저 나타나던 현상을 없앴습니다. 카드를 매번 다시 붙이는 대신 미리 올려두고 보이기만 전환하며, 테마 적용 범위를 바뀐 영역으로 좁히고, 섹션 호스트를 합성 패널로 그려 한 프레임에 완성된 화면이 나타납니다. Fixed the previous section card showing behind the new one, and the half-drawn empty skeleton that appeared before content filled in, when switching settings sections. Cards are now mounted once and toggled by visibility instead of being re-parented, theme re-apply is scoped to the changed area, and the section host composites its children into a single finished frame.

## [2.2.0] - 2026-10-27

### Added

- **프로그램별 토글 규칙 · 앱별 오디오 라우팅 / Program-specific toggle rules and per-app routing** — 특정 프로그램을 두 출력 장치 사이에서 토글하는 규칙을 만들고, 해당 프로그램의 오디오만 지정한 장치로 보냅니다. 볼륨 믹서 토글을 쓰면 포그라운드로 잡힌 프로그램만 전환됩니다. Create rules that toggle a specific program between two output devices and route only that program's audio to the chosen device; with volume-mixer toggle enabled, only the foreground-matched program is switched.
- **실행 중인 프로그램 선택 / Running-program picker** — 프로그램을 추가할 때 파일을 탐색하는 대신 현재 실행 중인(창이 있는) 프로그램 목록에서 선택합니다. 토글 규칙과 프로필 모두에 적용됩니다. When adding a program, pick from the list of currently running windowed programs instead of browsing for a file; this applies to both toggle rules and profiles.

### Changed

- **기본-통신 장치 연동 확장 / Default–communications sync extended** — 기존 재생 기준 통신 장치 연동을 녹음(입력) 장치에도 동일하게 적용하고, 옵션 이름을 "기본-통신 장치 연동"으로 바꿨습니다. The communications-default sync now also aligns the recording (capture) communications default with the current default recording device, and the option was renamed to "default–communications sync."
- **MainShellForm 분할 리팩터링 / MainShellForm refactor split** — 6,111줄 단일 폼을 책임별 14개 partial 클래스 파일로 분리했습니다(동작 변화 없음). Split the single 6,111-line form into 14 feature-based partial class files with no behavior change.

## [2.1.0] - 2026-04-16

### Added

- **프로그램 프로필 자동 전환 / Program profile automation** — 프로그램별 출력·입력 장치 프로필을 만들고, 포그라운드 프로그램에 맞는 프로필을 자동 적용합니다. 목록 순서가 우선순위로 동작하여, 더 높은 우선순위 프로그램이 실행 중이면 낮은 우선순위 프로필 적용을 막습니다. Create per-app output/input device profiles; the app applies the matching profile for the foreground program, with list order acting as priority so higher-priority running apps block lower-priority changes.
- **프로필 오버레이 / Profile overlay** — 프로필을 오버레이로 띄워 빠르게 선택하며, 단축키·위치·방향·높이를 설정할 수 있습니다. Open and pick a profile quickly from an overlay with a configurable shortcut, position, layout, and height.
- **새 장치 연결 시 자동 전환 / Auto-switch on new device** — 새 재생 장치가 연결되면 자동으로 해당 장치로 전환하는 옵션. Optionally switch automatically to a newly connected playback device.
- **기본 통신 장치 연동 / Playback–communications sync** — 기본 재생 장치를 바꿀 때 기본 통신 장치도 함께 맞추는 옵션. Optionally keep the default communications device aligned with the default playback device.
- **최근 전환 되돌리기 / Recent switch undo** — 최근 오디오 장치 변경을 5초 안에 되돌리는 전역 단축키. A global shortcut to undo the most recent audio device change within 5 seconds.
- **설정·단축키 확장 / Expanded settings and shortcuts** — 설정이 일반·자동화·단축키·오버레이·프로필 섹션으로 확장되고, 프로필 오버레이 호출 단축키와 되돌리기 단축키, 프로필별 아이콘·프로그램 목록·입출력 장치 편집 UI가 추가되었습니다. Settings expanded into General, Automation, Shortcuts, Overlay, and Profiles sections, adding overlay and undo shortcuts plus per-profile icon, program-target, and input/output device editing.

### Changed

- **장치 선택 안정성 개선 / Device selection reliability** — 프로필 장치 목록에서 연결 해제된 장치도 유지하고, 저장된 장치 ID가 바뀌어도 가능한 경우 이름 기준으로 선택을 자동 복구합니다. Profile device pickers keep disconnected devices in the selection flow, and stale device IDs are repaired by device name when possible.

## [2.0.1] - 2026-03-17

### Added

- **팝업 토스트 알림 사운드 / Popup toast notification sound** — 앱 내부 토스트 표시 시 알림 유형(일반·경고·오류)에 따라 서로 다른 Windows 시스템 사운드를 재생합니다. In-app toasts play a Windows system sound, varying by normal/warning/error type.
- **업데이트 알림 토글 / Update notification toggle** — 설정에서 업데이트 알림을 켜고 끌 수 있으며, 끄면 자동 확인과 업데이트 팝업이 모두 비활성화됩니다. Turn update notifications on or off in Settings; disabling suppresses automatic checks and update popups.

### Changed

- **안정성 보완 / Stability** — 사운드 재생에 실패해도 앱을 중단하지 않고 로그만 남깁니다. Sound playback failures are logged without interrupting the app.

## [2.0.0] - 2026-03-17

### Added

- **독립 실행형 Windows 앱 / Standalone Windows app** — 기존 PowerShell·batch·NirCmd 스크립트 흐름을 WinForms 트레이 앱으로 재구성하여, 별도 스크립트 없이 앱에서 바로 장치를 전환합니다. Rebuilt the former PowerShell/batch/NirCmd workflow as a WinForms tray app; device switching now happens in the app without separate scripts.
- **장치 전환·설정 통합 / Integrated switching and settings** — 앱에서 장치 A/B를 선택하고 현재 기본 장치를 기준으로 토글하며, 기본 재생 장치와 기본 통신 장치를 함께 바꿉니다. 전역 핫키와 토글 바로가기 생성을 지원합니다. Select Device A/B in-app, toggle based on the current default device, update both playback and communications defaults, and create global hotkeys and toggle shortcuts.
- **사용성·UI / Usability and UI** — 한국어·영어 UI, 시스템·라이트·다크 테마, 커스텀 트레이 토스트와 아이콘 폴더 기반 아이콘 선택, 시작 프로그램·시작 시 최소화·닫을 때 트레이로 최소화 옵션. Korean/English UI, system/light/dark themes, custom tray toasts with icon-folder-based selection, and start-with-Windows / start-minimized / minimize-to-tray options.
- **배포·업데이트 / Release and updates** — GitHub 릴리스 업데이트 감지, 포터블 `.zip` 빌드와 설치형 `setup.exe` 패키지 제공. GitHub release update detection, with both portable `.zip` builds and installer `setup.exe` packages.

## [1.1] - 2025-04-06

- 앱 이전의 스크립트(PowerShell·NirCmd) 기반 버전입니다. 상세 릴리스 노트는 보존되어 있지 않습니다. Pre-app, script-based (PowerShell/NirCmd) version; detailed release notes are not preserved.

## [1.0] - 2025-04-04

- 최초 스크립트 기반 릴리스. Initial script-based release.

[Unreleased]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v2.2.0...HEAD
[2.2.0]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v2.0.1...v2.1.0
[2.0.1]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v1.1...v2.0.0
[1.1]: https://github.com/jaries23/SoundDeviceSwitcher/compare/v1.0...v1.1
[1.0]: https://github.com/jaries23/SoundDeviceSwitcher/releases/tag/v1.0
