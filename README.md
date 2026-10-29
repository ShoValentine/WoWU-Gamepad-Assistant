<p align="center">
  <img src="docs/assets/wowu-logo.png" alt="WoWU" width="680">
</p>

<h1 align="center">Your adventure. A second screen.</h1>

<p align="center">
  A touchscreen companion for World of Warcraft, with a Windows Assistant that handles the setup behind the scenes.
</p>

<p align="center">
  <img alt="Development preview" src="https://img.shields.io/badge/status-development_preview-b68b38?style=flat-square">
  <img alt="Windows x64" src="https://img.shields.io/badge/Assistant-Windows_x64-536c88?style=flat-square">
  <img alt="Touchscreen companion" src="https://img.shields.io/badge/companion-1280_%C3%97_800-b68b38?style=flat-square">
</p>

<p align="center">
  <a href="#what-is-wowu">Overview</a> &nbsp;·&nbsp;
  <a href="docs/GETTING-STARTED.md">Getting started</a> &nbsp;·&nbsp;
  <a href="docs/FEATURES.md">Features & compatibility</a> &nbsp;·&nbsp;
  <a href="docs/SCREENSHOTS.md">Screenshot guide</a>
</p>

---

## What is WoWU?

WoWU brings your bags, quests, maps, spells and other everyday Warcraft controls onto a separate touchscreen. Keep the world on your main display and use a handheld, tablet or other Moonlight-capable device as your companion interface.

The **WoWU addon** provides the interface inside the game. The **Gamepad Assistant** prepares the Windows display and manages Sunshine streaming. **Moonlight** puts that companion display on your device.

<p align="center">
  <img src="docs/assets/how-it-works.svg" alt="WoW gameplay on the main screen, a separate WoWU companion interface streamed through Sunshine to a Moonlight device" width="960">
</p>

## Made for your next adventure

| | What you can do |
|---|---|
| **A dashboard that fits you** | Arrange the minimap, tracked quests, Quick Access buttons, notifications and other widgets. Save different layouts. |
| **Everyday actions, within reach** | Browse bags, read quests, turn spellbook pages and interact with supported NPC services through touch-friendly screens. |
| **Controller play** | Assign supported spells and items to trigger layers, move and scale button clusters, and use the optional compact HUD. |
| **Find your way** | Use the compass, destination guidance, quest trails and version-specific services, rares and notable-location data. |
| **A quiet desktop companion** | Start the Assistant, enable Gamepad Mode in WoW and connect through Moonlight. Display preparation and streaming run in the background. |
| **Return to normal play** | Disabling Gamepad Mode restores the gameplay layout and minimap. WoWU starts hidden when the mode is off. |

Feature availability follows the running WoW client. Forever retains its native controller system; some Classic screens use hosted Blizzard windows. [Read the complete support matrix →](docs/FEATURES.md)

## The everyday flow

1. **Start the Assistant.** It prepares the owned virtual display and managed streaming host.
2. **Launch WoW normally.** Enter the world with the matching WoWU addon enabled.
3. **Enable Gamepad Mode.** WoWU and the Assistant coordinate the viewport and companion interface.
4. **Connect with Moonlight.** Choose **WoWU Gamepad** and use **1280 × 800**. First-time pairing opens a PIN window in the Assistant.

The companion stream is configured without audio, leaving your normal game sound on the PC. Initial driver setup may ask for Windows administrator approval or a restart.

[Setup, requirements and troubleshooting →](docs/GETTING-STARTED.md)

## Which Warcraft clients?

| Client | Current development focus |
|---|---|
| **Retail** | Broad custom-module support, modern talents and professions, custom banking and controller features. |
| **WoW Forever** | Companion interface, viewport and Forever-specific systems; uses Forever's own native gamepad controls. |
| **Classic Era** | Dedicated quest, map, navigation and layout work, with native fallbacks for some modules. |
| **Mists Classic** | Dedicated talent tiers, professions, quest-map integration, Dungeon Journal, Dungeon Finder and PvP views. |
| **Other Classic branches** | Selected adapters and native fallbacks exist. They have not received a complete live acceptance pass. |

The [80-feature matrix](docs/FEATURES.md) distinguishes custom interfaces, native fallbacks, conditional support and features that are not provided. Support is not identical across clients.

## Development status

WoWU is a **private development preview**. Current reviewed development packages are **addon v0.13.2**, **Compass v0.4.4** and **Assistant v0.7.3**. Builds are supplied by the maintainer during testing.

The source currently checked into this repository is the **earlier v0.1.0 Assistant milestone**. This project overview describes the newer development builds; it does not imply that building the older snapshot produces the current app. Updating the source snapshot is a separate task from this presentation refresh.

The latest project review passed **146 current behavior/compatibility checks**, compiled **109 Lua files**, and rebuilt the Assistant without warnings. Those automated checks do not certify protected combat actions, every client UI or an actual Moonlight stream. [Review summary →](docs/PROJECT-STATUS.md)

## Feedback from testing

Found a problem? Include your **WoW client and build**, **addon/Assistant versions**, what you were doing, and any exact error text. A clean screenshot of the affected screen is especially helpful. Please omit account details, chat conversations and pairing PINs.

[Open an issue](../../issues/new/choose)

## Project and artwork

WoWU is a community project and is not affiliated with or endorsed by Blizzard Entertainment. Warcraft names and game artwork belong to their respective owners. Existing third-party notices and source records remain applicable; this documentation refresh does not introduce a new software licence.

Moonlight and Sunshine are separate projects. Sunshine is managed as a separate process by the current Assistant. The repository's original [development brief](DEVELOPMENT-BRIEF.md) and [milestone notes](MILESTONE-1.md) are historical design references.

<p align="center"><img src="docs/assets/wowu-icon.png" alt="WoWU emblem" width="64"></p>
