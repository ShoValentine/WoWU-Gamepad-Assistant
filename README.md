# WoWU Gamepad Assistant

Windows 11 x64 companion for the WoWU touchscreen addon. C# / .NET 10 / WPF.

## v0.1.0: milestone 2 prototype

Implemented: running WoW discovery; active Windows display enumeration; explicit
monitor selection; overlap-safe vertical-span calculation; borderless window
placement; exact client-area verification; 30-second preview; Keep/Restore;
restoration on game exit, geometry changes and app close; local crash recovery.
No activation on process detection. No driver, display topology, Sunshine or
SavedVariables changes. The user must already be in Windowed mode in Forever.
Client type is confirmed by the user, not inferred securely from executable names.

The existing 1280 x 800 virtual monitor must already sit directly below, entirely
within the gameplay monitor width. The app never identifies a monitor as owned
merely because of its resolution. You explicitly select it for this prototype.

## Run

Extract the entire application ZIP, then run WoWU.GamepadAssistant.exe.
Requires Microsoft .NET 10 Desktop Runtime x64 (already present on the initial
development PC). This is a framework-dependent, unsigned prototype, not an installer.

See [FIRST-RUN.md](FIRST-RUN.md) for activation and restoration steps.

## Build

Install the .NET 10 SDK for Windows x64. From the repository root:

```powershell
dotnet publish src/WoWU.GamepadAssistant/WoWU.GamepadAssistant.csproj -c Release -r win-x64 --self-contained false -o artifacts/app -p:RestoreConfigFile=NuGet.Config
```

No third-party NuGet packages. NuGet.Config clears package feeds for this build.
Compilation completed; the executable has not been launched or live-tested.
No simulated checks were run. Real client-area verification in the application
is part of the requested operating behaviour, not a separate simulated test.

## Recovery

The original process identity, HWND, styles, placement and rectangle are written
before mutation to %LOCALAPPDATA%/WoWU.GamepadAssistant/recovery.json.
An unfinished session is restored on the next app launch, only if process start
time and window ownership match. Errors retain the recovery record and block
new activation. A destroyed window needs no restoration. Displays are never
changed, so no display-topology restoration is needed in milestone 2.

Only one Assistant instance runs per Windows login session. Closing the window
restores the game; tray behaviour is deferred. Exit/logout cinematic sync is not
yet automatic: stop the addon viewport and restore before returning to character
selection. Do not run another window manager against the game simultaneously.

## Roadmap

Next: addon activation handshake and confirmed geometry handoff. Later: driver
installation/ownership, topology automation, Sunshine capture verification,
Moonlight pairing, tray UI, installer and compatibility work.

- [Development brief](DEVELOPMENT-BRIEF.md)
- [Viewport proof of concept](MILESTONE-1.md)
- [Mandatory Sunshine targeting](SUNSHINE-TARGETING.md)

Sunshine readiness requires verification of the actual captured virtual display,
not just a config write. Never hardcode a monitor identity.

## References

- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos
- https://learn.microsoft.com/en-us/windows/win32/hidpi/setting-the-default-dpi-awareness-for-a-process

No Sunshine or driver binaries are bundled. Redistribution licensing/signing must
be settled before an installer. No project redistribution licence selected yet.
