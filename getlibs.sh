#!/bin/sh

mkdir -p libs
rm -rf libs/*

echo "Downloading files ..."

wget -P libs -i .gitbinmodules

echo "Download finished."

echo "Extracting files ..."

mkdir -p libs/Sparkle
tar -jxvf libs/Sparkle*.tar.bz2 -C libs/Sparkle
rm libs/Sparkle*.tar.bz2

echo "Download and extract WinSparkle"

unzip -o libs/WinSparkle*.zip -d libs

mkdir -p libs/WinSparkle
mv libs/WinSparkle-*/Release/WinSparkle.dll libs/WinSparkle/WinSparkle.x86.dll
mv libs/WinSparkle-*/x64/Release/WinSparkle.dll libs/WinSparkle/WinSparkle.x86_64.dll
mv libs/WinSparkle-*/ARM64/Release/WinSparkle.dll libs/WinSparkle/WinSparkle.arm64.dll

rm libs/WinSparkle*.zip
rm -rf libs/WinSparkle-*

echo "Files extraction finished. You can use them for development now."
