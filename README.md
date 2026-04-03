# ClaudeLight for Windows

Windows port of ClaudeLight — a system tray indicator for Anthropic API peak hours.

## Features
- 🟢 Green dot = off-peak (full rate limits)
- 🔴 Red dot = peak hours (reduced limits)
- Countdown to next status change
- Current Eastern Time display
- Optional balloon notification when off-peak starts
- Launch at Login via registry
- Zero third-party dependencies — pure C# / .NET 8 WinForms

## Peak Schedule
**Mon–Fri, 8:00 AM – 2:00 PM ET** (5:00 AM – 11:00 AM PT)  
Weekends are always green.

## Requirements
- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — required to build

**Don't have .NET 8?** Install it via winget:
```powershell
winget install Microsoft.DotNet.SDK.8
```
Then close and reopen PowerShell before building.

## Quick Start

```powershell
git clone https://github.com/romero-jr/Claude-LightMode.git
cd Claude-LightMode
.\build.bat
.\build\ClaudeLight.exe
```

The app runs in the system tray (bottom-right corner of the taskbar). No window will open.

## Usage
- **Left or right-click** the tray icon to see status, countdown, and options
- **Notify when off-peak starts** — enables a Windows notification when peak hours end
- **Launch at Login** — starts ClaudeLight automatically when Windows boots

## Build

```powershell
.\build.bat
# Output: .\build\ClaudeLight.exe
```

### Self-contained build (no .NET runtime required on target machine)
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./build
```
Note: self-contained adds ~60 MB to the executable.

## Project Structure

```
ClaudeLight/
├── Program.cs                  # Entry point (STAThread)
├── TrayApplicationContext.cs   # NotifyIcon + menu + timer
├── PeakHoursService.cs         # Peak/off-peak logic + events
├── TrayIconRenderer.cs         # Draws sparkle icon as System.Drawing.Icon
├── LaunchAtLoginService.cs     # HKCU registry launch-at-login
├── NativeMethods.cs            # P/Invoke for DestroyIcon
├── build.bat                   # One-click build script
└── README.md
```
