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
unzip -o libs/WinSparkle*.zip -d libs
rm libs/WinSparkle*.zip
ls libs/ | grep WinSparkle | xargs -I '{}' mv libs/{} libs/WinSparkle

echo "Files extraction finished. You can use them for development now."
