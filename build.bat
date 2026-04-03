@echo off
REM Build ClaudeLight for Windows
REM Requires .NET 8 SDK: https://dotnet.microsoft.com/download

echo Building ClaudeLight...

dotnet publish ClaudeLightWindows.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained false ^
  -p:PublishSingleFile=true ^
  -p:PublishReadyToRun=true ^
  -o ./build

echo.
echo Done. Output: .\build\ClaudeLight.exe
pause
