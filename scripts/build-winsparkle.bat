cd ../external/winsparkle
nuget restore
msbuild WinSparkle-2015.sln /t:WinSparkle /p:Configuration="Release" /p:Platform="Win32"
msbuild WinSparkle-2015.sln /t:WinSparkle /p:Configuration="Release" /p:Platform="x64"

