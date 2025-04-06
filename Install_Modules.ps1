# 실행 정책 설정 및 TLS 1.2 강제 적용
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned -Force
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# NuGet 공급자 설치
Install-PackageProvider -Name NuGet -Force -Scope CurrentUser

# AudioDeviceCmdlets 설치
if (-not (Get-Module -ListAvailable -Name AudioDeviceCmdlets)) {
    Write-Host "🎧 AudioDeviceCmdlets 설치 중..."
    Install-Module -Name AudioDeviceCmdlets -Scope CurrentUser -Force
} else {
    Write-Host "✅ AudioDeviceCmdlets 이미 설치됨"
}

# BurntToast 설치
if (-not (Get-Module -ListAvailable -Name BurntToast)) {
    Write-Host "🔔 BurntToast 설치 중..."
    Install-Module -Name BurntToast -Scope CurrentUser -Force
} else {
    Write-Host "✅ BurntToast 이미 설치됨"
}

Write-Host "`n🎉 모든 준비 완료! 이제 배치 스크립트에서 오디오 전환과 알림을 사용할 수 있습니다." -ForegroundColor Green
Read-Host -Prompt "👋 설치가 완료되었습니다! 창을 닫으려면 Enter 키를 누르세요"
