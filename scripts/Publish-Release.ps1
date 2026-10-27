[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipInstaller,
    [switch]$SkipSigning,
    [string]$CertificateThumbprint,
    [string]$PfxPath,
    [string]$PfxPassword,
    [string]$TimestampUrl,
    [string]$FileDigestAlgorithm = "SHA256",
    [string]$TimestampDigestAlgorithm = "SHA256",
    [switch]$UseMachineCertificateStore
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

function Get-SignToolPath {
    $command = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($null -ne $command)
    {
        return $command.Source
    }

    $kitsRoots = @(
        (Join-Path ${env:ProgramFiles(x86)} "Windows Kits\\10\\bin"),
        (Join-Path $env:ProgramFiles "Windows Kits\\10\\bin")
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path $_) }

    foreach ($kitsRoot in $kitsRoots)
    {
        $candidates = Get-ChildItem -Path $kitsRoot -Filter signtool.exe -Recurse -ErrorAction SilentlyContinue |
            Sort-Object @{
                Expression = { $_.FullName -match '\\x64\\signtool\.exe$' }
                Descending = $true
            }, @{
                Expression = { $_.FullName }
                Descending = $true
            }

        if ($candidates.Count -gt 0)
        {
            return $candidates[0].FullName
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

function Get-ConfiguredValue {
    param(
        [string]$Value,
        [Parameter(Mandatory = $true)]
        [string]$EnvironmentVariableName
    )

    if (-not [string]::IsNullOrWhiteSpace($Value))
    {
        return $Value
    }

    $environmentValue = [Environment]::GetEnvironmentVariable($EnvironmentVariableName)
    if ([string]::IsNullOrWhiteSpace($environmentValue))
    {
        return $null
    }

    return $environmentValue
}

function Get-CodeSigningConfiguration {
    param(
        [switch]$SkipSigning,
        [string]$CertificateThumbprint,
        [string]$PfxPath,
        [string]$PfxPassword,
        [string]$TimestampUrl,
        [string]$FileDigestAlgorithm,
        [string]$TimestampDigestAlgorithm,
        [switch]$UseMachineCertificateStore
    )

    if ($SkipSigning)
    {
        return $null
    }

    $resolvedThumbprint = Get-ConfiguredValue -Value $CertificateThumbprint -EnvironmentVariableName "SDS_SIGN_CERT_THUMBPRINT"
    $resolvedPfxPath = Get-ConfiguredValue -Value $PfxPath -EnvironmentVariableName "SDS_SIGN_PFX_PATH"
    $resolvedTimestampUrl = Get-ConfiguredValue -Value $TimestampUrl -EnvironmentVariableName "SDS_SIGN_TIMESTAMP_URL"
    $resolvedPfxPassword = Get-ConfiguredValue -Value $PfxPassword -EnvironmentVariableName "SDS_SIGN_PFX_PASSWORD"

    if ([string]::IsNullOrWhiteSpace($resolvedThumbprint) -and [string]::IsNullOrWhiteSpace($resolvedPfxPath))
    {
        return $null
    }

    if (-not [string]::IsNullOrWhiteSpace($resolvedThumbprint) -and -not [string]::IsNullOrWhiteSpace($resolvedPfxPath))
    {
        throw "Specify either CertificateThumbprint/SDS_SIGN_CERT_THUMBPRINT or PfxPath/SDS_SIGN_PFX_PATH, but not both."
    }

    $signToolPath = Get-SignToolPath
    if ($null -eq $signToolPath)
    {
        throw "signtool.exe was not found. Install the Windows SDK or make signtool.exe available on PATH before enabling code signing."
    }

    if (-not [string]::IsNullOrWhiteSpace($resolvedPfxPath))
    {
        $resolvedPfxPath = (Resolve-Path -LiteralPath $resolvedPfxPath).Path
        return [pscustomobject]@{
            Mode = "Pfx"
            SignToolPath = $signToolPath
            PfxPath = $resolvedPfxPath
            PfxPassword = $resolvedPfxPassword
            TimestampUrl = $resolvedTimestampUrl
            FileDigestAlgorithm = $FileDigestAlgorithm
            TimestampDigestAlgorithm = $TimestampDigestAlgorithm
            Description = "PFX file"
        }
    }

    $resolvedThumbprint = ($resolvedThumbprint -replace '\s', '').ToUpperInvariant()
    if ($resolvedThumbprint -notmatch '^[A-F0-9]+$')
    {
        throw "CertificateThumbprint must be a hexadecimal thumbprint."
    }

    return [pscustomobject]@{
        Mode = "CertificateStore"
        SignToolPath = $signToolPath
        Thumbprint = $resolvedThumbprint
        UseMachineCertificateStore = [bool]$UseMachineCertificateStore
        TimestampUrl = $resolvedTimestampUrl
        FileDigestAlgorithm = $FileDigestAlgorithm
        TimestampDigestAlgorithm = $TimestampDigestAlgorithm
        Description = if ($UseMachineCertificateStore) { "certificate store (LocalMachine)" } else { "certificate store (CurrentUser)" }
    }
}

function Invoke-CodeSigning {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Configuration,
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    if (-not (Test-Path -LiteralPath $FilePath))
    {
        throw "The file to sign was not found: $FilePath"
    }

    $arguments = @(
        "sign",
        "/fd", $Configuration.FileDigestAlgorithm
    )

    if (-not [string]::IsNullOrWhiteSpace($Configuration.TimestampUrl))
    {
        $arguments += @(
            "/tr", $Configuration.TimestampUrl,
            "/td", $Configuration.TimestampDigestAlgorithm
        )
    }
    else
    {
        Write-Warning "Signing $FilePath without a timestamp URL. The signature may become invalid after the certificate expires."
    }

    if ($Configuration.Mode -eq "Pfx")
    {
        $arguments += @("/f", $Configuration.PfxPath)
        if ($null -ne $Configuration.PfxPassword)
        {
            $arguments += @("/p", $Configuration.PfxPassword)
        }
    }
    else
    {
        if ($Configuration.UseMachineCertificateStore)
        {
            $arguments += "/sm"
        }

        $arguments += @("/sha1", $Configuration.Thumbprint)
    }

    $arguments += $FilePath

    Write-Host "Signing $FilePath"
    & $Configuration.SignToolPath @arguments
    if ($LASTEXITCODE -ne 0)
    {
        throw "signtool.exe failed while signing $FilePath with exit code $LASTEXITCODE."
    }

    $signature = Get-AuthenticodeSignature -FilePath $FilePath
    if ($signature.Status -eq [System.Management.Automation.SignatureStatus]::NotSigned -or
        $signature.Status -eq [System.Management.Automation.SignatureStatus]::HashMismatch)
    {
        throw "The generated signature for $FilePath is not usable. Status: $($signature.Status)."
    }

    if ($signature.Status -ne [System.Management.Automation.SignatureStatus]::Valid)
    {
        Write-Warning "The signature for $FilePath was created, but Windows reports status '$($signature.Status)'."
    }
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
$codeSigningConfiguration = Get-CodeSigningConfiguration `
    -SkipSigning:$SkipSigning `
    -CertificateThumbprint $CertificateThumbprint `
    -PfxPath $PfxPath `
    -PfxPassword $PfxPassword `
    -TimestampUrl $TimestampUrl `
    -FileDigestAlgorithm $FileDigestAlgorithm `
    -TimestampDigestAlgorithm $TimestampDigestAlgorithm `
    -UseMachineCertificateStore:$UseMachineCertificateStore

New-CleanDirectory -Path $publishDir
New-CleanDirectory -Path $portableStageDir
New-CleanDirectory -Path $releaseDir

if ($null -eq $codeSigningConfiguration)
{
    Write-Host "Code signing disabled."
}
else
{
    Write-Host "Code signing enabled via $($codeSigningConfiguration.Description)."
}

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

if ($null -ne $codeSigningConfiguration)
{
    Invoke-CodeSigning -Configuration $codeSigningConfiguration -FilePath $publishedExePath
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

        if ($null -ne $codeSigningConfiguration)
        {
            Invoke-CodeSigning -Configuration $codeSigningConfiguration -FilePath $setupPath
        }

        $assetPaths.Add($setupPath)
    }
}

Write-Host ""
Write-Host "Release artifacts:"
foreach ($assetPath in $assetPaths)
{
    Write-Host " - $assetPath"
}
