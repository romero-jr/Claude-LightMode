# ClaudeLight for Windows

Windows port of ClaudeLight — a system tray indicator for Anthropic API peak hours.

## Features
- 🟢 Green dot = off-peak (full rate limits)
- 🔴 Red dot = peak hours (reduced limits)
- Countdown to next status change
- Current Pacific Time display
- Optional balloon notification when off-peak starts
- Launch at Login via registry
- Zero third-party dependencies — pure C# / .NET 8 WinForms

## Peak Schedule
**Mon–Fri, 5:00 AM – 11:00 AM PT**  
Weekends are always green.

## Requirements
- Windows 10 or 11
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
  (or build self-contained to bundle the runtime — see below)

## Build

```
# Requires .NET 8 SDK
build.bat

# Output: .\build\ClaudeLight.exe
```

### Self-contained build (no runtime required on target machine)
```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./build
```
Note: self-contained adds ~60 MB to the executable.

## Project Structure

```
ClaudeLight/
├── Program.cs                  # Entry point (STAThread)
├── TrayApplicationContext.cs   # NotifyIcon + menu + timer (≈ AppDelegate + MenuBarExtra)
├── PeakHoursService.cs         # Peak/off-peak logic + events (≈ PeakHoursService.swift)
├── TrayIconRenderer.cs         # Draws sparkle icon as System.Drawing.Icon
├── LaunchAtLoginService.cs     # HKCU registry launch-at-login
├── NativeMethods.cs            # P/Invoke for DestroyIcon
└── build.bat                   # One-click build script
```

## macOS → Windows mapping

| macOS (Swift)                  | Windows (C#)                        |
|-------------------------------|--------------------------------------|
| `MenuBarExtra`                | `NotifyIcon` + `ContextMenuStrip`   |
| `NSImage` CGContext drawing   | `System.Drawing` + `Graphics`       |
| `UNUserNotificationCenter`    | `NotifyIcon.ShowBalloonTip`         |
| `SMAppService` launch at login| Registry `HKCU\...\Run`             |
| `Timer` + `RunLoop.common`    | `System.Windows.Forms.Timer`        |
| `@Published` + `StateChanged` | `event EventHandler`                |
