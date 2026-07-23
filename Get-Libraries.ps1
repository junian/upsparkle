# Get-Libraries.ps1
# Downloads and extracts libraries listed in .gitbinmodules

param(
    [switch]$IncludeSparkle = $false
)

$LibsDir      = Join-Path $PSScriptRoot "libs"
$RuntimesDir  = Join-Path $PSScriptRoot "runtimes"

# Clear libs directory contents, preserving .gitkeep
if (Test-Path $LibsDir) {
    Get-ChildItem -Path $LibsDir -Exclude ".gitkeep" |
        ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
} else {
    New-Item -ItemType Directory -Path $LibsDir | Out-Null
}

# Read URLs from .gitbinmodules
$ModulesFile = Join-Path $PSScriptRoot ".gitbinmodules"
$Urls = Get-Content $ModulesFile | Where-Object { $_.Trim() -ne "" }

if (-not $IncludeSparkle) {
    $Urls = $Urls | Where-Object { $_ -notmatch "Sparkle" }
}

Write-Host "Downloading files ..."

foreach ($Url in $Urls) {
    $FileName    = [System.IO.Path]::GetFileName($Url)
    $Destination = Join-Path $LibsDir $FileName
    Write-Host "  -> $FileName"
    Invoke-WebRequest -Uri $Url -OutFile $Destination
}

Write-Host "Download finished."
Write-Host "Extracting files ..."

# --- Sparkle (tar.xz) -> runtimes/osx/native/ ---
if ($IncludeSparkle) {
    $SparkleArchive = Get-ChildItem -Path $LibsDir -Filter "Sparkle*.tar.xz" | Select-Object -First 1
    if ($SparkleArchive) {
        $OsxNativeDir = Join-Path $RuntimesDir "osx\native"
        New-Item -ItemType Directory -Path $OsxNativeDir -Force | Out-Null
        Write-Host "Extracting $($SparkleArchive.Name) -> runtimes/osx/native/ ..."
        tar -xJf $SparkleArchive.FullName -C $OsxNativeDir
        Remove-Item $SparkleArchive.FullName
    }
}

# --- WinSparkle (zip) -> runtimes/win-{arch}/native/ ---
$WinSparkleArchive = Get-ChildItem -Path $LibsDir -Filter "WinSparkle*.zip" | Select-Object -First 1
if ($WinSparkleArchive) {
    Write-Host "Extracting $($WinSparkleArchive.Name) ..."
    Expand-Archive -Path $WinSparkleArchive.FullName -DestinationPath $LibsDir -Force

    $x86Dir   = Join-Path $RuntimesDir "win-x86\native"
    $x64Dir   = Join-Path $RuntimesDir "win-x64\native"
    $arm64Dir = Join-Path $RuntimesDir "win-arm64\native"

    New-Item -ItemType Directory -Path $x86Dir   -Force | Out-Null
    New-Item -ItemType Directory -Path $x64Dir   -Force | Out-Null
    New-Item -ItemType Directory -Path $arm64Dir -Force | Out-Null

    # x86 — Release\WinSparkle.dll (no x64/ARM64 in path)
    $x86Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "[/\\]Release[/\\]WinSparkle\.dll$" } |
        Where-Object { $_.FullName -notmatch "x64" -and $_.FullName -notmatch "ARM64" } |
        Select-Object -First 1
    if ($x86Dll) {
        Move-Item $x86Dll.FullName (Join-Path $x86Dir "WinSparkle.dll") -Force
        Write-Host "  -> runtimes/win-x86/native/WinSparkle.dll"
    }

    # x64 — x64\Release\WinSparkle.dll
    $x64Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "x64[/\\]Release" } |
        Select-Object -First 1
    if ($x64Dll) {
        Move-Item $x64Dll.FullName (Join-Path $x64Dir "WinSparkle.dll") -Force
        Write-Host "  -> runtimes/win-x64/native/WinSparkle.dll"
    }

    # arm64 — ARM64\Release\WinSparkle.dll
    $arm64Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "ARM64[/\\]Release" } |
        Select-Object -First 1
    if ($arm64Dll) {
        Move-Item $arm64Dll.FullName (Join-Path $arm64Dir "WinSparkle.dll") -Force
        Write-Host "  -> runtimes/win-arm64/native/WinSparkle.dll"
    }

    Remove-Item $WinSparkleArchive.FullName
    Get-ChildItem -Path $LibsDir -Directory -Filter "WinSparkle-*" |
        ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
}

Write-Host "Files extraction finished. You can use them for development now."
