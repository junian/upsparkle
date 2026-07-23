# Get-Libraries.ps1
# Downloads and extracts libraries listed in .gitbinmodules

$LibsDir = Join-Path $PSScriptRoot "libs"

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

Write-Host "Downloading files ..."

foreach ($Url in $Urls) {
    $FileName = [System.IO.Path]::GetFileName($Url)
    $Destination = Join-Path $LibsDir $FileName
    Write-Host "  -> $FileName"
    Invoke-WebRequest -Uri $Url -OutFile $Destination
}

Write-Host "Download finished."
Write-Host "Extracting files ..."

# Extract Sparkle (tar.xz)
$SparkleArchive = Get-ChildItem -Path $LibsDir -Filter "Sparkle*.tar.xz" | Select-Object -First 1
if ($SparkleArchive) {
    $SparkleDir = Join-Path $LibsDir "Sparkle"
    New-Item -ItemType Directory -Path $SparkleDir | Out-Null
    Write-Host "Extracting $($SparkleArchive.Name) ..."
    tar -xJf $SparkleArchive.FullName -C $SparkleDir
    Remove-Item $SparkleArchive.FullName
}

# Extract WinSparkle (zip)
$WinSparkleArchive = Get-ChildItem -Path $LibsDir -Filter "WinSparkle*.zip" | Select-Object -First 1
if ($WinSparkleArchive) {
    Write-Host "Extracting $($WinSparkleArchive.Name) ..."
    Expand-Archive -Path $WinSparkleArchive.FullName -DestinationPath $LibsDir -Force

    $WinSparkleDir = Join-Path $LibsDir "WinSparkle"
    New-Item -ItemType Directory -Path $WinSparkleDir -Force | Out-Null

    # Move architecture-specific DLLs
    $x86Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "\\\\Release\\\\WinSparkle\.dll$" -or $_.FullName -match "[/\\]Release[/\\]WinSparkle\.dll$" } |
        Where-Object { $_.FullName -notmatch "x64" -and $_.FullName -notmatch "ARM64" } | Select-Object -First 1
    if ($x86Dll) {
        Move-Item $x86Dll.FullName (Join-Path $WinSparkleDir "WinSparkle.x86.dll") -Force
    }

    $x64Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "x64[/\\]Release" } | Select-Object -First 1
    if ($x64Dll) {
        Move-Item $x64Dll.FullName (Join-Path $WinSparkleDir "WinSparkle.x86_64.dll") -Force
    }

    $arm64Dll = Get-ChildItem -Path $LibsDir -Recurse -Filter "WinSparkle.dll" |
        Where-Object { $_.FullName -match "ARM64[/\\]Release" } | Select-Object -First 1
    if ($arm64Dll) {
        Move-Item $arm64Dll.FullName (Join-Path $WinSparkleDir "WinSparkle.arm64.dll") -Force
    }

    Remove-Item $WinSparkleArchive.FullName
    Get-ChildItem -Path $LibsDir -Directory -Filter "WinSparkle-*" |
        ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
}

Write-Host "Files extraction finished. You can use them for development now."
