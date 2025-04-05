# 🎧 SoundDeviceSwitcher

> 🔽 English instructions are available below.

Windows에서 두 개의 오디오 장치(예: 이어폰과 스피커)를 **스마트하게 전환**할 수 있는 배치 스크립트입니다.

단순한 토글이 아닌, 다음과 같은 **지능적인 기능**을 제공합니다:

- ✅ **두 장치가 모두 연결된 경우에만 전환 실행**
- 🔁 현재 기본 장치를 확인한 뒤 **다른 장치로 자동 전환**
- 🔊 **기본 재생 장치**와 **기본 통신 장치**를 동시에 설정
- 🔔 전환 후 트레이 알림으로 상태 표시
- ⚡ Stream Deck, AutoHotKey 등 **단축키 도구와 연동** 용이

> 💡 장치가 하나라도 연결되어 있지 않으면, 스크립트는 전환을 취소하고 경고 메시지를 표시합니다.

---

## ✅ 사용 방법 (Korean)

### 1. 파일 다운로드
- GitHub에서 **초록색 Code 버튼** 클릭  
- **Download ZIP** 선택  
- 압축 파일을 해제한 후, 폴더에 다음 파일들이 있는지 확인:
  - `device.bat`
  - `device.vbs`
  - `Install_Modules.ps1`

### 2. PowerShell 모듈 설치 (최초 1회만)
- `Install_Modules.ps1` 파일을 **마우스 오른쪽 클릭 → PowerShell로 실행(R)**  
- 아래 작업이 자동으로 실행됩니다:
  - ✅ 실행 정책을 `RemoteSigned`로 설정  
  - 🎧 `AudioDeviceCmdlets` 모듈 설치 (이미 설치된 경우 생략)  
  - 🔔 `BurntToast` 모듈 설치 (이미 설치된 경우 생략)
- 설치가 완료되면 다음과 같은 메시지가 표시됩니다:
  ```
  🎉 모든 준비 완료! 이제 배치 스크립트에서 오디오 전환과 알림을 사용할 수 있습니다.
  👋 설치가 완료되었습니다! 창을 닫으려면 Enter 키를 누르세요
  ```
- > ⏳ **설치 후 창이 자동으로 닫히지 않으니, Enter 키를 눌러 수동으로 닫아주세요.**

### 3. NirCmd 다운로드
- 공식 사이트: [https://www.nirsoft.net/utils/nircmd.html](https://www.nirsoft.net/utils/nircmd.html)  
- 페이지 하단에서 **“NirCmd 64-bit”** 다운로드  
- 압축 해제 경로: `C:\Program Files\nircmd-x64`  
- 해당 폴더 안에 `nircmd.exe` 파일이 있는지 확인

### 4. 시스템 환경 변수 등록
- 시작 메뉴에서 **“시스템 환경 변수 편집”** 검색 후 실행  
- **"환경 변수(N)..."** 클릭  
- **시스템 변수**에서 `Path` 선택 → **"편집(I)..."** 클릭  
- **"새로 만들기(N)"** 클릭 → `C:\Program Files\nircmd-x64` 입력  
- 모든 창을 **"확인"**으로 닫기

### 5. 오디오 장치 이름 설정
- `device.bat` 파일의 확장자를 `.txt`로 변경하여 메모장에서 열기  
- `[설정]` 아래에 `earphone`, `speaker` 항목을 본인의 장치 이름으로 수정  
- 저장 후 확장자를 `.bat`로 다시 변경

### 6. 스크립트 실행
- 폴더 안의 `device.vbs` 파일을 더블 클릭  
- 실행창 없이 조용히 오디오 장치가 전환됨  
- **Stream Deck**, **AutoHotKey** 등에서 이 `device.vbs` 파일을 실행하도록 연결하면 더 편리하게 사용 가능

---

## ✅ How to Use (English)

**SoundDeviceSwitcher** is a batch script for **smartly toggling** between two audio devices on Windows (e.g., headphones and speakers).

It’s more than just a toggle—this script includes intelligent features:

- ✅ **Only switches if both devices are connected**
- 🔁 Automatically detects the current default device and switches to the other
- 🔊 Sets both the **Default Playback Device** and **Default Communication Device**
- 🔔 Displays a tray notification after switching
- ⚡ Easily integrates with hotkey tools like Stream Deck or AutoHotKey

> 💡 If either device is not connected, the script will cancel the operation and show a warning instead of switching.

---

### 1. Download Files
- Click the **green Code button** on GitHub  
- Select **Download ZIP**  
- Extract the ZIP file and make sure the folder contains:
  - `device.bat`
  - `device.vbs`
  - `Install_Modules.ps1`

### 2. Install PowerShell Modules (one-time setup)
- Right-click `Install_Modules.ps1` and select **Run with PowerShell**  
- The following will be performed automatically:
  - ✅ Set execution policy to `RemoteSigned`  
  - 🎧 Install `AudioDeviceCmdlets` module (skipped if already installed)  
  - 🔔 Install `BurntToast` module (skipped if already installed)
- Once complete, you'll see:
  ```
  🎉 All set! You can now use the batch script for audio switching and notifications.
  👋 Installation completed! Press Enter to close this window.
  ```
- > ⏳ **The window won’t close automatically—please press Enter to exit manually.**

### 3. Download NirCmd
- Official site: [https://www.nirsoft.net/utils/nircmd.html](https://www.nirsoft.net/utils/nircmd.html)  
- Scroll to the bottom and download **“NirCmd 64-bit”**  
- Extract to: `C:\Program Files\nircmd-x64`  
- Ensure `nircmd.exe` is present in that folder

### 4. Add to System Environment Variables
- Open the Start menu and search for **“Edit the system environment variables”**  
- Click **“Environment Variables...”**  
- In the **System variables** section, select `Path` → click **“Edit...”**  
- Click **“New”** → add `C:\Program Files\nircmd-x64`  
- Click **OK** to close all windows

### 5. Set Your Audio Device Names
- Rename `device.bat` to `.txt` and open it in Notepad  
- Under the **[Settings]** section, change the `earphone` and `speaker` entries to match your actual device names  
- Save and rename the file extension back to `.bat`

### 6. Run the Script
- Double-click the `device.vbs` file in the folder  
- It will switch audio devices silently without opening any command windows  
- You can link the `device.vbs` file to tools like **Stream Deck** or **AutoHotKey** for quick access
