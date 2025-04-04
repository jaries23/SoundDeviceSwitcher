Preparation Guide (English)

1. Download NirCmd
   - Visit the official NirSoft website: https://www.nirsoft.net/utils/nircmd.html
   - Scroll to the bottom of the page and click "Download NirCmd 64-bit"
   - Extract the ZIP file to: C:\Program Files\nircmd-x64
   - Make sure the folder contains the nircmd.exe file

2. Add to System Environment Variables
   - Open the Start menu or click the search icon
   - Type “Edit the system environment variables” and press Enter
   - In the window that opens, click “Environment Variables...”
   - Under "System variables", select "Path" and click "Edit..."
   - Click "New" and enter: C:\Program Files\nircmd-x64
   - Click OK to close all windows

3. Install PowerShell Audio Module (only needed once)
   - Press Win + R, type powershell, then press Enter
   - In the PowerShell window, type and run:
     Install-Module -Name AudioDeviceCmdlets -Scope CurrentUser -Force
   - If prompted about an untrusted repository, type Y and press Enter

4. Configure Audio Device Names
   - Right-click the device.bat file and choose Rename
   - Change the file extension to .txt, then open it in Notepad
   - Under the [Settings] comment, update the device names next to earphone and speaker to match your actual device names
   - Press Ctrl + S to save and close Notepad
   - Rename the file extension back to .bat