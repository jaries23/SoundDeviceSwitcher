@echo off
setlocal

:: 🎧🔊 오디오 장치 토글 스크립트
:: 스피커와 이어폰을 번갈아가며 기본 장치 및 기본 통신 장치로 설정

:: [설정] 토글할 오디오 장치 지정
set earphone=Final VR2000
set speaker=Marshall Willen

set e_found=
set s_found=
set current=0

set SCRIPT_DIR=%~dp0
set ASSET_DIR=%SCRIPT_DIR%assets

:: [1단계] 장치 연결 유무 확인
for /f "delims=" %%i in ('powershell -Command "Import-Module AudioDeviceCmdlets; Get-AudioDevice -List | Select-Object -ExpandProperty Name | ForEach-Object { $_.Split('(')[0].Trim() }"') do (
    if "%%i"=="%earphone%" set e_found=1
    if "%%i"=="%speaker%" set s_found=1
)

:: [2단계] 현재 기본 설정 장치 확인
for /f "delims=" %%i in ('powershell -Command "Import-Module AudioDeviceCmdlets; Get-AudioDevice -List | Where-Object { $_.Type -eq 'Playback' -and $_.Default -eq $true } | Select-Object -ExpandProperty Name | ForEach-Object { $_.Split('(')[0].Trim() }"') do (
    if "%%i"=="%earphone%" set current=1
)

:: [3단계] 기본 장치 변경 (이어폰 ↔ 스피커)
if defined e_found if defined s_found (
    if %current%==1 (
        :: 현재 장치가 이어폰이면 스피커로 변경
        nircmd setdefaultsounddevice "%speaker%" 1
        nircmd setdefaultsounddevice "%speaker%" 2
        powershell -Command "Import-Module BurntToast; New-BurntToastNotification -Text 'The device has been switched.', '%speaker%' -AppLogo '%ASSET_DIR:\=\\%\\icon_speaker.png' -UniqueIdentifier 'DeviceToggle'"
    ) else (
        :: 현재 장치가 스피커라면 이어폰으로 변경
        nircmd setdefaultsounddevice "%earphone%" 1
        nircmd setdefaultsounddevice "%earphone%" 2
        powershell -Command "Import-Module BurntToast; New-BurntToastNotification -Text 'The device has been switched.', '%earphone%' -AppLogo '%ASSET_DIR:\=\\%\\icon_earphone.png' -UniqueIdentifier 'DeviceToggle'"
    )
) else (
    :: 장치가 감지되지 않을 경우 알림 표시
    powershell -Command "Import-Module BurntToast; New-BurntToastNotification -Text 'The device is unreachable.', 'Please check your device connection or ensure the device is powered on.' -AppLogo '%ASSET_DIR:\=\\%\\icon_warning.png' -UniqueIdentifier 'DeviceToggle'"
)

endlocal
