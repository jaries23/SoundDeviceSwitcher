## 🆕 v2.1.0 업데이트 내역

이번 `v2.1.0`은 `v2.0.1`을 기준으로 장치 전환 자동화와 프로필 기능이 크게 확장된 업데이트입니다.

### ✨ 주요 변경 사항

- **프로그램 프로필 기반 자동 전환 추가**
  - 프로그램별로 출력 장치와 입력 장치를 지정하는 프로필을 만들 수 있습니다.
  - 현재 활성 프로그램을 기준으로 맞는 프로필을 자동 적용합니다.
  - 프로필 목록 순서가 우선순위로 동작하며, 더 높은 우선순위 프로필이 실행 중이면 낮은 우선순위 프로필 적용을 막습니다.

- **프로필 오버레이 추가**
  - 프로필 목록을 오버레이로 띄워서 빠르게 선택할 수 있습니다.
  - 오버레이 단축키, 위치, 방향, 높이를 설정할 수 있습니다.

- **새 장치 연결 시 자동 전환**
  - 새 재생 장치가 연결되면 자동으로 해당 장치로 전환하는 옵션이 추가되었습니다.

- **기본 장치 변경 시 기본 통신 장치 연동**
  - 기본 재생 장치를 바꿀 때 기본 통신 장치도 함께 맞추는 옵션이 추가되었습니다.

- **최근 전환 되돌리기**
  - 최근 오디오 장치 변경을 5초 안에 되돌리는 전역 단축키가 추가되었습니다.

- **장치 선택 안정성 개선**
  - 프로필 장치 선택 목록에서 연결 해제된 장치도 유지되도록 확장되었습니다.
  - 저장된 장치 ID가 바뀐 경우에도, 가능한 상황에서는 이름 기준으로 장치 선택을 자동 복구하도록 보완했습니다.

- **설정 화면 및 단축키 확장**
  - 설정 화면이 일반, 자동화, 단축키, 오버레이, 프로필 섹션으로 확장되었습니다.
  - 기존 전환 단축키 외에, 프로필 오버레이 호출 단축키와 최근 전환 되돌리기 단축키가 추가되었습니다.
  - 프로필별 아이콘, 프로그램 목록, 출력/입력 장치 편집 UI가 추가되었습니다.

---

## 🔄 Changelog v2.1.0

`v2.1.0` is a major expansion on top of `v2.0.1`, adding automation workflows and profile-based device management.

### ✨ What’s New

- **Program Profile Automation Added**
  - You can create per-app profiles with dedicated output and input devices.
  - The app detects the foreground program and applies the matching profile automatically.
  - Profile order now acts as priority, so higher-priority running apps can block lower-priority profile changes.

- **Profile Overlay Added**
  - A profile overlay lets you open and choose a profile quickly.
  - The overlay supports its own shortcut plus configurable position, layout, and height.

- **Automatic Switching on New Device Connection**
  - You can now automatically switch to a newly connected playback device.

- **Playback/Communications Device Sync**
  - You can now keep the default communications device aligned with the selected default playback device.

- **Recent Switch Undo**
  - A new global shortcut can undo the most recent audio device change within 5 seconds.

- **Improved Device Selection Reliability**
  - Profile device pickers now keep disconnected devices in the selection flow.
  - When saved device IDs become stale, the app can repair selections by device name when possible.

- **Expanded Settings and Shortcuts**
  - Settings are now organized into General, Automation, Shortcuts, Overlay, and Profiles sections.
  - This release adds a profile overlay shortcut and a recent-switch undo shortcut.
  - Profile-specific icons, program targeting, and output/input device editing were added to the UI.
