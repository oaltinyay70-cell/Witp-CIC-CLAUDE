<#
.SYNOPSIS
    Builds, publishes, and packages releases for ROCHEFORT 6000 for GitHub.

.DESCRIPTION
    Compiles both:
      1. Standalone / Self-Contained single-file Windows x64 binary (runs standalone without requiring .NET 10 runtime)
      2. Framework-Dependent single-file Windows x64 binary (ultra-compact, requires .NET 10 runtime)
    Packages both into release zip archives with documentation, icon assets, telemetry text files, and computes SHA256 checksums.

.PARAMETER Version
    The release version tag (e.g., '1.0.0' or 'v1.0.0'). Defaults to '1.0.0'.
#>

[CmdletBinding()]
param(
    [string]$Version = "1.0.0",
    [int]$BuildNumber = 0
)

$ErrorActionPreference = "Stop"

$cleanVersion = $Version.TrimStart('v')
$tagName = "v$cleanVersion"
$rootDir = $PSScriptRoot
$publishDir = Join-Path $rootDir "publish"
$releaseDir = Join-Path $rootDir "release"
$distDir = Join-Path $rootDir "dist"
$frameworkDepPublishDir = Join-Path $publishDir "framework-dependent"
$standalonePublishDir = Join-Path $publishDir "standalone"

# Read Phase from AppVersion.cs
$appVersionFile = Join-Path $rootDir "Core\Common\AppVersion.cs"
$phase = "alpha4"
if (Test-Path $appVersionFile) {
    $content = Get-Content $appVersionFile -Raw
    if ($content -match 'public const string Phase = "([^"]+)";') {
        $phase = $matches[1]
    }
}

# Auto-detect latest build number from dist/ for this phase if not explicitly specified
if ($BuildNumber -le 0) {
    $latestNum = 0
    if (Test-Path $distDir) {
        $filter = "witp-cic-rochefort-$phase-*.exe"
        $regex = "^witp-cic-rochefort-" + [regex]::Escape($phase) + "-(\d+)\.exe$"
        $existing = Get-ChildItem -Path $distDir -Filter $filter | ForEach-Object {
            if ($_.Name -match $regex) {
                [int]$matches[1]
            }
        }
        if ($existing) {
            $latestNum = ($existing | Measure-Object -Maximum).Maximum
        }
    }
    $BuildNumber = $latestNum + 1
}

$buildArtifactName = "witp-cic-rochefort-$phase-$BuildNumber.exe"
$versionTag = "v-$phase-build$BuildNumber"

# Keep AppVersion.cs synchronized with the determined build number
if (Test-Path $appVersionFile) {
    $content = Get-Content $appVersionFile -Raw
    $newContent = [System.Text.RegularExpressions.Regex]::Replace($content, 'public const int BuildNumber = \d+;', "public const int BuildNumber = $BuildNumber;")
    if ($content -ne $newContent) {
        [System.IO.File]::WriteAllText($appVersionFile, $newContent, [System.Text.Encoding]::UTF8)
        Write-Host "Synchronized AppVersion.cs with BuildNumber = $BuildNumber ($versionTag)" -ForegroundColor DarkGray
    }
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  ROCHEFORT 6000 Release Builder - $versionTag ($buildArtifactName)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Clean previous directories and terminate running instances
Write-Host "`n[1/5] Preparing output directories and terminating running instances..." -ForegroundColor Yellow
try {
    taskkill /F /IM witp-cic-rochefort* /T 2>$null
    taskkill /F /IM ROCHEFORT* /T 2>$null
} catch {}

if (Test-Path $publishDir) { Remove-Item -Path $publishDir -Recurse -Force }
if (Test-Path $releaseDir) { Remove-Item -Path $releaseDir -Recurse -Force }

New-Item -ItemType Directory -Force -Path $frameworkDepPublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $standalonePublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

$dotnetExe = "dotnet"
if (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") {
    $dotnetExe = "$env:USERPROFILE\.dotnet\dotnet.exe"
}

# 1. Publish Standalone (Self-Contained)
Write-Host "`n[2/5] Publishing Standalone (Self-Contained) win-x64 Release..." -ForegroundColor Green
& $dotnetExe publish "$rootDir\ROCHEFORT6000.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:Version=$cleanVersion `
    -p:AssemblyVersion="$cleanVersion.0" `
    -p:FileVersion="$cleanVersion.0" `
    -o $standalonePublishDir

# 2. Publish Framework-Dependent
Write-Host "`n[3/5] Publishing Framework-Dependent win-x64 Release..." -ForegroundColor Green
& $dotnetExe publish "$rootDir\ROCHEFORT6000.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$cleanVersion `
    -p:AssemblyVersion="$cleanVersion.0" `
    -p:FileVersion="$cleanVersion.0" `
    -o $frameworkDepPublishDir

# Copy documentation, assets, and text folder into publish folders
Write-Host "`n[4/5] Copying branding, documentation, and telemetry assets..." -ForegroundColor Green
$assets = @("README.md", "LICENSE", "icon.png", "icon.ico")
foreach ($asset in $assets) {
    $src = Join-Path $rootDir $asset
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $frameworkDepPublishDir -Force
        Copy-Item -Path $src -Destination $standalonePublishDir -Force
    }
}

$textSrc = Join-Path $rootDir "text"
if (Test-Path $textSrc) {
    Copy-Item -Path $textSrc -Destination $frameworkDepPublishDir -Recurse -Force
    Copy-Item -Path $textSrc -Destination $standalonePublishDir -Recurse -Force
}

# 3. Create Zip Packages
Write-Host "`n[5/5] Compressing Release Archives & Computing Checksums..." -ForegroundColor Green
$frameworkDepZip = Join-Path $releaseDir "ROCHEFORT6000-$tagName-win-x64-FrameworkDependent.zip"
$standaloneZip = Join-Path $releaseDir "ROCHEFORT6000-$tagName-win-x64-Standalone.zip"

Compress-Archive -Path "$frameworkDepPublishDir\*" -DestinationPath $frameworkDepZip -Force
Compress-Archive -Path "$standalonePublishDir\*" -DestinationPath $standaloneZip -Force

# Locate the published executable and copy to versioned build artifact
$sourceExe = Get-ChildItem -Path $standalonePublishDir -Filter "*.exe" | Select-Object -First 1
if ($sourceExe) {
    Copy-Item -Path $sourceExe.FullName -Destination "$releaseDir\$buildArtifactName" -Force
    $distDir = Join-Path $rootDir "dist"
    New-Item -ItemType Directory -Force -Path $distDir | Out-Null
    Copy-Item -Path $sourceExe.FullName -Destination "$distDir\$buildArtifactName" -Force
}
if (Test-Path $textSrc) {
    Copy-Item -Path $textSrc -Destination $releaseDir -Recurse -Force
}

# Generate SHA256 Checksums
$checksumFile = Join-Path $releaseDir "SHA256SUMS.txt"
if (Test-Path $checksumFile) { Remove-Item $checksumFile -Force }

Get-ChildItem -Path "$releaseDir\*.zip", "$releaseDir\*.exe" | ForEach-Object {
    $hash = (Get-FileHash -Path $_.FullName -Algorithm SHA256).Hash.ToLower()
    $line = "$hash  $($_.Name)"
    $line | Out-File -FilePath $checksumFile -Append -Encoding ascii
    Write-Host "  $line" -ForegroundColor Gray
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "  Full Release Build Complete!" -ForegroundColor Cyan
Write-Host "  Publish Folders:" -ForegroundColor White
Write-Host "    - $frameworkDepPublishDir" -ForegroundColor Gray
Write-Host "    - $standalonePublishDir" -ForegroundColor Gray
Write-Host "  Release Archives ($releaseDir):" -ForegroundColor White
Get-ChildItem -Path $releaseDir | ForEach-Object {
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "    - $($_.Name) ($sizeMB MB)" -ForegroundColor Yellow
}
Write-Host "==========================================================" -ForegroundColor Cyan

