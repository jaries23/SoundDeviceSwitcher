@echo off
setlocal

:: 관리자 권한 확인
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo 🔒 관리자 권한이 필요합니다. 다시 관리자 권한으로 실행 중...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: 현재 경로에 있는 Install_Modules.ps1 실행
set "psScript=%~dp0Install_Modules.ps1"

echo 🚀 PowerShell 스크립트를 실행하는 중...
powershell -NoProfile -ExecutionPolicy Bypass -File "%psScript%"

echo.
pause
