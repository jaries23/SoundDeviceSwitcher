[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipInstaller
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ProjectVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    [xml]$projectXml = Get-Content -Path $ProjectPath
    $versionNode = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($versionNode))
    {
        throw "Could not find a <Version> element in $ProjectPath."
    }

    return $versionNode.Trim()
}

function Get-InnoSetupCompiler {
    $command = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($null -ne $command)
    {
        return $command.Source
    }

    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\\ISCC.exe")
    )

    foreach ($candidate in $candidates)
    {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path $candidate))
        {
            return $candidate
        }
    }

    return $null
}

function New-CleanDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (Test-Path $Path)
    {
        Remove-Item -Path $Path -Recurse -Force
    }

    New-Item -Path $Path -ItemType Directory | Out-Null
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..")).Path
$projectPath = Join-Path $repoRoot "src\\SoundDeviceSwitcher.App\\SoundDeviceSwitcher.App.csproj"
$installerScriptPath = Join-Path $repoRoot "installer\\SoundDeviceSwitcher.iss"
$projectAssetDir = Join-Path $repoRoot "src\\SoundDeviceSwitcher.App\\assets"

$version = Get-ProjectVersion -ProjectPath $projectPath
$artifactRoot = Join-Path $repoRoot "artifacts"
$publishDir = Join-Path (Join-Path $artifactRoot "publish") $RuntimeIdentifier
$stagingRoot = Join-Path $artifactRoot "staging"
$portableFolderName = "SoundDeviceSwitcher-v{0}-{1}" -f $version, $RuntimeIdentifier
$portableStageDir = Join-Path $stagingRoot $portableFolderName
$releaseDir = Join-Path $artifactRoot ("release\\v{0}" -f $version)
$zipPath = Join-Path $releaseDir ("{0}.zip" -f $portableFolderName)
$setupPath = Join-Path $releaseDir ("SoundDeviceSwitcher-v{0}-setup.exe" -f $version)
$hashPath = Join-Path $releaseDir "SHA256SUMS.txt"

New-CleanDirectory -Path $publishDir
New-CleanDirectory -Path $portableStageDir
New-CleanDirectory -Path $releaseDir

& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $RuntimeIdentifier `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    /p:PublishTrimmed=false `
    /p:PublishReadyToRun=false `
    --output $publishDir

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$publishedExePath = Join-Path $publishDir "SoundDeviceSwitcher.exe"
if (-not (Test-Path $publishedExePath))
{
    throw "Expected published executable was not found: $publishedExePath"
}

if (Test-Path $projectAssetDir)
{
    Copy-Item -Path $projectAssetDir -Destination $publishDir -Recurse -Force
}

Copy-Item -Path (Join-Path $publishDir "*") -Destination $portableStageDir -Recurse -Force
Compress-Archive -Path $portableStageDir -DestinationPath $zipPath -CompressionLevel Optimal

$assetPaths = [System.Collections.Generic.List[string]]::new()
$assetPaths.Add($zipPath)

if (-not $SkipInstaller)
{
    $isccPath = Get-InnoSetupCompiler
    if ($null -eq $isccPath)
    {
        Write-Warning "Inno Setup compiler was not found. The zip package was created, but setup.exe was skipped."
    }
    else
    {
        & $isccPath `
            "/DMyAppVersion=$version" `
            "/DMyPublishDir=$publishDir" `
            "/DMyOutputDir=$releaseDir" `
            $installerScriptPath

        if (-not (Test-Path $setupPath))
        {
            throw "The installer build finished without creating the expected setup executable: $setupPath"
        }

        $assetPaths.Add($setupPath)
    }
}

$hashLines = foreach ($assetPath in $assetPaths)
{
    $hash = Get-FileHash -Path $assetPath -Algorithm SHA256
    "{0}  {1}" -f $hash.Hash.ToLowerInvariant(), (Split-Path $assetPath -Leaf)
}

Set-Content -Path $hashPath -Value $hashLines
$assetPaths.Add($hashPath)

Write-Host ""
Write-Host "Release artifacts:"
foreach ($assetPath in $assetPaths)
{
    Write-Host " - $assetPath"
}
