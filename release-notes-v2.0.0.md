# SoundDeviceSwitcher v2.0.0

스크립트 기반 구조를 정리하고, 독립 실행형 Windows 앱으로 전환한 첫 릴리즈입니다.

This release replaces the previous script-based workflow with a standalone Windows app.

## 주요 변경점

- PowerShell, batch, NirCmd 중심 구조를 WinForms 트레이 앱으로 재구성
- 앱 안에서 장치 A / 장치 B 직접 선택 가능
- 현재 기본 장치를 기준으로 다른 장치로 자동 토글
- 기본 재생 장치와 기본 통신 장치를 함께 전환
- 전역 핫키 선택 지원
- 한국어 / 영어 UI 지원
- 시스템 / 라이트 / 다크 테마 지원
- 커스텀 트레이 토스트 알림 추가
- 시작 프로그램, 시작 시 최소화, 닫을 때 트레이로 최소화 지원
- 사용자 아이콘 폴더 기반 아이콘 선택 지원
- GitHub 릴리즈 업데이트 감지 지원
- `.zip` 포터블 빌드와 설치형 `setup.exe` 패키지 추가

## Included Assets

- `SoundDeviceSwitcher-v2.0.0-setup.exe`
- `SoundDeviceSwitcher-v2.0.0-win-x64.zip`
