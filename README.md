> 🔽 English instructions are available below.

## ✅ 사용 방법 (Korean)

1. **NirCmd 다운로드**  
   - NirSoft 공식 웹사이트 방문: https://www.nirsoft.net/utils/nircmd.html  
   - 페이지 맨 아래로 스크롤해 "NirCmd 64-bit" 다운로드  
   - 압축을 `C:\Program Files\nircmd-x64` 폴더에 해제  
   - 폴더 내부에 `nircmd.exe` 파일이 존재해야 함  

2. **시스템 환경 변수 등록**  
   - 시작 메뉴 또는 검색 아이콘 클릭  
   - “시스템 환경 변수 편집” 입력 후 Enter  
   - 열리는 창에서 "환경 변수(N)..." 클릭  
   - 시스템 변수(S) 목록에서 "Path" 선택 후 "편집(I)..." 클릭  
   - "새로 만들기(N)" 클릭 → `C:\Program Files\nircmd-x64` 입력  
   - 확인을 눌러 모든 창 닫기  

3. **PowerShell 오디오 모듈 설치** (최초 한 번만)  
   - `Win + R` 누르고 `powershell` 입력 후 Enter  
   - PowerShell 창에서 아래 명령어 입력 및 실행:  
     ```
     Install-Module -Name AudioDeviceCmdlets -Scope CurrentUser -Force
     ```  
   - 신뢰되지 않은 저장소에서 설치할지 묻는 경우 `Y` 입력 후 Enter  

4. **오디오 장치 이름 설정**  
   - `device.bat` 파일을 마우스 오른쪽 버튼으로 클릭하고 "이름 바꾸기" 선택  
   - 파일 확장자를 `.txt`로 변경한 뒤 메모장으로 열기  
   - `[설정]` 주석 아래의 `earphone`, `speaker` 옆 장치 이름을 본인의 장치 이름으로 수정  
   - `Ctrl + S`로 저장 후 메모장 닫기  
   - 다시 확장자를 `.bat`로 변경하여 저장  

---

## ✅ How to Use (English)

1. **Download NirCmd**  
   - Visit the official NirSoft website: https://www.nirsoft.net/utils/nircmd.html  
   - Scroll to the bottom and download "NirCmd 64-bit"  
   - Extract the archive to the folder: `C:\Program Files\nircmd-x64`  
   - Make sure the folder contains `nircmd.exe`  

2. **Add to System Environment Variables**  
   - Open the Start menu or click the search icon  
   - Type “Edit the system environment variables” and press Enter  
   - In the window that appears, click “Environment Variables...”  
   - Under the "System variables" section, select "Path" and click "Edit..."  
   - Click “New” and enter: `C:\Program Files\nircmd-x64`  
   - Click OK to close all dialogs  

3. **Install PowerShell Audio Module** (only once)  
   - Press `Win + R`, type `powershell`, then press Enter  
   - In the PowerShell window, run:  
     ```
     Install-Module -Name AudioDeviceCmdlets -Scope CurrentUser -Force
     ```  
   - If prompted about an untrusted repository, type `Y` and press Enter  

4. **Set Your Audio Device Names**  
   - Right-click the `device.bat` file and choose “Rename”  
   - Change the file extension to `.txt`, then open it in Notepad  
   - Under the `[Settings]` comment, replace the names next to `earphone` and `speaker` with your actual device names  
   - Save the file (`Ctrl + S`) and close Notepad  
   - Rename the extension back to `.bat`  
