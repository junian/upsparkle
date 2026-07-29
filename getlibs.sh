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

# Rename versioned archive to a fixed name so the csproj can reference it directly.
SPARKLE_ARCHIVE=$(ls libs/Sparkle*.tar.xz 2>/dev/null | head -n 1)
if [ -n "$SPARKLE_ARCHIVE" ]; then
    mv "$SPARKLE_ARCHIVE" "${RUNTIMES_DIR}/osx/native/Sparkle.tar.xz"
fi

echo "Extracting MacSparkle ..."

unzip -o libs/libMacSparkle*.zip -d libs
mv libs/libMacSparkle*.dylib "${RUNTIMES_DIR}/osx/native/libMacSparkle.dylib"
rm libs/libMacSparkle*.zip

echo "Extracting WinSparkle ..."

unzip -o libs/WinSparkle*.zip -d libs

mkdir -p "${RUNTIMES_DIR}/win-x86/native"
mkdir -p "${RUNTIMES_DIR}/win-x64/native"
mkdir -p "${RUNTIMES_DIR}/win-arm64/native"

mv libs/WinSparkle-*/Release/WinSparkle.dll          "${RUNTIMES_DIR}/win-x86/native/WinSparkle.dll"
mv libs/WinSparkle-*/x64/Release/WinSparkle.dll      "${RUNTIMES_DIR}/win-x64/native/WinSparkle.dll"
mv libs/WinSparkle-*/ARM64/Release/WinSparkle.dll    "${RUNTIMES_DIR}/win-arm64/native/WinSparkle.dll"

rm libs/WinSparkle*.zip
rm -rf libs/WinSparkle-*

echo "Files extraction finished. You can use them for development now."
