#!/bin/sh

RUNTIMES_DIR="runtimes"

mkdir -p libs
rm -rf libs/*

echo "Downloading files ..."

wget -P libs -i .gitbinmodules

echo "Download finished."

echo "Extracting Sparkle ..."

mkdir -p "${RUNTIMES_DIR}/osx/native"
tar -Jxf libs/Sparkle*.tar.xz -C "libs"
mv libs/Sparkle.framework "${RUNTIMES_DIR}/osx/native/."
rm libs/Sparkle*.tar.xz

# Re-pack only Sparkle.framework into a new Sparkle.tar.xz with symlinks preserved.
# Using a relative path inside the archive so it extracts as Sparkle.framework/
echo "Repacking Sparkle.framework -> runtimes/osx/native/Sparkle.tar.xz ..."
tar -cJf "${RUNTIMES_DIR}/osx/native/Sparkle.tar.xz" \
    -C "${RUNTIMES_DIR}/osx/native" \
    Sparkle.framework

echo "Extracting MacSparkle ..."

unzip -o libs/libMacSparkle*.zip -d libs
mv libs/libMacSparkle*.dylib "${RUNTIMES_DIR}/osx/native/libMacSparkle.dylib"
rm libs/libMacSparkle*.zip

echo "Extracting WinSparkle ..."

unzip -o libs/WinSparkle*.zip -d libs

mkdir -p "${RUNTIMES_DIR}/win-x86/native"
mkdir -p "${RUNTIMES_DIR}/win-x64/native"
mkdir -p "${RUNTIMES_DIR}/win-arm64/native"

mv libs/WinSparkle-*/Win32/Release/WinSparkle.dll    "${RUNTIMES_DIR}/win-x86/native/WinSparkle.dll"
mv libs/WinSparkle-*/x64/Release/WinSparkle.dll      "${RUNTIMES_DIR}/win-x64/native/WinSparkle.dll"
mv libs/WinSparkle-*/ARM64/Release/WinSparkle.dll    "${RUNTIMES_DIR}/win-arm64/native/WinSparkle.dll"

rm libs/WinSparkle*.zip
rm -rf libs/WinSparkle-*

echo "Files extraction finished. You can use them for development now."
