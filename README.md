# WoWU Gamepad Assistant

Windows 11 companion for the WoWU World of Warcraft touchscreen addon.

## Status

Planning and native viewport proof of concept. The user has confirmed world
cropping, touchscreen alignment and restoration with the Forever viewport POC.
The Windows application has not been implemented yet.

## Next: milestone 2

C# / .NET / WPF prototype using the existing virtual display:
- Detect supported WoW processes and their main windows.
- Identify the gameplay display and explicitly selected existing virtual display.
- Calculate the vertical 1280 x 800 workspace without moving unrelated displays.
- Snapshot window placement and style before explicit activation.
- Span the client area and restore on request or game exit.
- Persist recovery information for interrupted sessions.

Driver installation, addon activation handshake, Sunshine management and an
installer follow in later milestones. Never span merely on process detection.

## Requirements

- [Full development brief](DEVELOPMENT-BRIEF.md)
- [Viewport proof of concept](MILESTONE-1.md)
- [Mandatory Sunshine capture targeting](SUNSHINE-TARGETING.md)

Sunshine readiness requires verification of the actual captured virtual display;
writing output_name alone is insufficient. Never hardcode a monitor identity.

## Development

Preferred stack: C#, .NET, WPF, Windows 11. No executable is included yet.
Do not commit credentials, pairing keys, local display snapshots or user logs.
Do not bundle Sunshine or display drivers until redistribution and signing
requirements have been settled. No project redistribution licence selected yet.
