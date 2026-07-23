#!/bin/sh

RUNTIMES_DIR="runtimes"

mkdir -p libs
rm -rf libs/*

echo "Downloading files ..."

wget -P libs -i .gitbinmodules

echo "Download finished."

echo "Extracting Sparkle ..."

mkdir -p "${RUNTIMES_DIR}/osx/native"
tar -Jxf libs/Sparkle*.tar.xz -C "${RUNTIMES_DIR}/osx/native"
rm libs/Sparkle*.tar.xz

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
