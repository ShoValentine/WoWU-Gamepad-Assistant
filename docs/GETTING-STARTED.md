# Getting started with WoWU

This guide describes the current development packages. The repository's original v0.1.0 source snapshot has a different manual setup flow; it does not build the current automatic Assistant.

## What you need

- A Windows x64 PC running a compatible WoW client. Windows 11 is the current desktop target.
- The matching WoWU and WoWU_Compass addon folders, plus the current Gamepad Assistant package supplied by the maintainer.
- The .NET 10 Desktop Runtime x64 for the framework-dependent Assistant.
- A Moonlight-capable device and a suitable local network. Touchscreen behavior depends on the device and Moonlight client.

## Install the development build

1. Close the previous Assistant. Extract the **entire** new Assistant package into its own folder.
2. Replace the `WoWU` and `WoWU_Compass` folders in your chosen client's `Interface/AddOns` directory.
3. Load WoW with the addons enabled. If this is the first installation, enter the world and reload once so WoWU creates its saved data.
4. Start `WoWU.GamepadAssistant.exe`. Let the splash complete; its status message explains anything that needs attention.

The current Assistant prepares its owned virtual display at 1280 × 800, positions it below the gameplay monitor, and manages its Sunshine process. Initial driver installation or enablement can require administrator approval or a Windows restart. Avoid running a competing standalone Sunshine host at the same time.

## Connect

1. Enter the world, then enable **Gamepad Mode** using WoWU's in-game button or settings.
2. Allow the reload/connection transition to finish.
3. In Moonlight, add the PC and request pairing if needed. Enter Moonlight's four-digit PIN in the Assistant popup.
4. Connect to **WoWU Gamepad** at **1280 × 800**.

There is no manual WoWU.lua file picker in the current Assistant. It discovers eligible addon saves from the recognized game installation. When several WoW clients are open, close the extra clients so it can select one unambiguously.

## Finish a session

Disable Gamepad Mode to restore normal gameplay layout and the minimap. Closing the Assistant stops its managed streaming host, attempts to restore the owned game window/display position and disables the owned virtual driver. Recovery records are retained when cleanup needs attention.

## Common things to check

| Symptom | Check |
|---|---|
| Assistant is waiting for WoWU | Confirm the addon is installed in the **running client's** AddOns folder. Enter the world and reload once after first install. |
| Assistant cannot choose a game | Close additional WoW clients. Renamed executables are recognized through metadata/title/install structure rather than one exact filename. |
| Display setup needs attention | Read the status message. Finish any Windows-requested driver restart before retrying. |
| Another Sunshine host is detected | Close the competing standalone host before starting the managed one. |
| Pairing window does not appear | Check that Moonlight is requesting a fresh pairing and the Assistant host is ready. Do not include the PIN in a bug report. |
| Held casting does not repeat | Enable Hold to Cast on a supported client and put the spell on an existing action bar; a hidden bar works. Items and macros do not repeat. Forever uses its own controller implementation. |
| A feature is missing in Classic | Check the [support matrix](FEATURES.md). Some modules are native hosted fallbacks or require APIs that client does not expose. |

## Useful in-game commands

- `/wowu` — WoWU command help.
- `/wowuclient` — identify the running WoW version/interface.
- `/wowushow` and `/wowuhide` — manually show/hide the interface.
- `/wowupad status` — WoWU controller preset status on supported clients.

Check in-game help for the exact commands available in your build. Do not edit live SavedVariables to force a connection.

## If you report a problem

Include the WoW version/build, addon version, Assistant version, exact error text and the steps needed to reproduce it. Current Assistant diagnostics are kept under `%LOCALAPPDATA%/WoWU.GamepadAssistant`, including startup and cleanup logs. Review logs for personal information before sharing them.
