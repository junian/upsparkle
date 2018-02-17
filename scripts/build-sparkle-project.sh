#!/bin/bash

DIR=$(dirname ${0})
cd ${DIR}
git submodule init && git submodule update
cd ../external/Sparkle
xcodebuild -configuration Release -target Sparkle

