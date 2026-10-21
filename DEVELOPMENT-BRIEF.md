# WoWU Gamepad Assistant
## Complete Development Brief and Technical Specification

**Project:** WoWU (World of Warcraft Touchscreen Companion)  
**New application:** WoWU Gamepad Assistant  
**Primary platform:** Windows 11  
**Primary game target:** World of Warcraft Forever  
**Future targets:** World of Warcraft Classic and Retail  
**Status:** Planning and proof-of-concept stage

---

# 1. PROJECT OBJECTIVE

We need to develop a Windows application called **WoWU Gamepad Assistant**.

This application will work alongside our existing WoWU World of Warcraft addon.

The goal is to make WoWU's second-screen touchscreen experience accessible to ordinary users without requiring them to understand or manually configure:

- Virtual display drivers.
- Windows extended desktop arrangements.
- Multi-monitor window spanning.
- World of Warcraft viewport manipulation.
- Sunshine streaming host configuration.
- Virtual monitor resolutions and coordinates.
- Complicated external addon dependencies.

**The application must completely eliminate WoWU's dependency on Offhand.**

Our existing setup already works using Offhand, a virtual monitor, Sunshine, Moonlight and WoWU.

We are not inventing a new display architecture. We want to reproduce and automate that proven arrangement using our own Windows application and native WoWU viewport functionality.

The long-term goal is:

**Install WoWU Gamepad Assistant, install the WoWU addon, launch WoW normally, activate Gamepad Mode in-game, and connect a touchscreen through Moonlight.**

Everything complicated should happen automatically.

---

# 2. END-USER EXPERIENCE

This is the intended installation and activation process.

## Step 1: Install WoWU Gamepad Assistant

The user downloads and installs our Windows application.

The installer should handle the necessary components, including:

- WoWU Gamepad Assistant.
- Virtual display driver installation and configuration.
- Sunshine streaming host integration.
- Any required application dependencies.

The exact packaging strategy will depend on the licences, signing requirements and redistribution permissions of these components.

The user should not need to install and configure a virtual display driver manually.

## Step 2: Install the WoWU Addon

The user downloads the WoWU addon through CurseForge and installs it normally.

The addon contains the existing WoWU functionality, including the Dashboard, modules, touchscreen controls and the new native viewport-management functionality.

## Step 3: Launch Gamepad Assistant

The application opens with a deliberately minimal interface.

Initial status:

**WoWU Gamepad Assistant Running.**  
**Please Launch World of Warcraft.**

The Assistant begins monitoring for supported World of Warcraft processes.

There should be no requirement to launch WoW through the Assistant.

The player may launch WoW normally through their preferred launcher.

## Step 4: Launch World of Warcraft

The player launches WoW.

At this stage:

- WoW appears normally on the primary gameplay monitor.
- Login screens remain correctly displayed.
- Character selection remains correctly displayed.
- No window spanning occurs yet.
- The player can log in without connecting their handheld or tablet.

This is extremely important.

Our current Offhand-based implementation spans the display early, which can make login screens and cinematics difficult to see or interact with.

We want to avoid that problem.

## Step 5: Enter the Game World

Once the player enters the game world, the WoWU addon displays a prompt:

**Enable WoWU Gamepad Mode?**

Buttons:

- Yes
- Not Now

The prompt should appear at a suitable time after character login, not repeatedly during loading screens.

Selecting Not Now leaves WoW functioning normally.

Selecting Yes begins Gamepad Mode activation.

The user should also be able to activate or disable Gamepad Mode manually through WoWU Settings.

## Step 6: Activate Gamepad Mode

When the player selects Yes:

1. WoWU records an activation request using a SavedVariables bridge.
2. WoWU triggers a UI reload if required.
3. The Windows Assistant detects the activation request.
4. The Assistant activates the virtual display.
5. It configures Windows extended desktop positioning.
6. It positions and resizes WoW across the combined gameplay and WoWU display areas.
7. WoWU applies its native gameplay viewport.
8. Sunshine is configured to stream the virtual WoWU display.
9. The Assistant reports that Gamepad Mode is ready.

The technical implementation must ensure that the Windows window arrangement is ready before the addon applies viewport settings.

A second synchronisation step or reload may be necessary.

The user should not have to perform manual window-positioning operations.

## Step 7: Connect Through Moonlight

The player opens Moonlight on their handheld, tablet or phone.

They connect to the WoWU streaming host.

Moonlight displays the WoWU interface from the virtual 1280×800 display.

Touch input is passed back to the game through the supported streaming input system.

Initial Moonlight installation and pairing may still be necessary.

After first-time setup, reconnecting should be straightforward.

## Step 8: Exit World of Warcraft

When WoW closes, the Assistant should:

- Detect the game process terminating.
- Restore the original Windows display configuration.
- Restore any relevant window-management settings.
- Deactivate the WoWU virtual display when appropriate.
- Keep unrelated displays and applications unaffected.
- Return to its standby state.

Status returns to:

**WoWU Gamepad Assistant Running.**  
**Please Launch World of Warcraft.**

---

# 3. APPLICATION TECHNOLOGY

Our preferred implementation is:

**Language:** C#  
**Framework:** .NET  
**Desktop interface:** WPF  
**Target OS:** Windows 11

The application should be lightweight and run quietly while WoW is active.

A minimal window with status information is preferred over a complicated control panel.

Suggested internal components:

- Display Manager.
- Virtual Display Manager.
- WoW Process Monitor.
- WoW Window Manager.
- WoWU Addon Bridge.
- Sunshine Streaming Manager.
- Session State Manager.
- Recovery Manager.

These should be developed as separate logical components.

---

# 4. VIRTUAL DISPLAY MANAGEMENT

The Assistant must be able to provide a virtual monitor with a default resolution of:

**1280 × 800**

This is the primary intended WoWU touchscreen resolution.

We should investigate using an existing Windows Indirect Display Driver rather than writing a display driver ourselves.

Any selected driver must be appropriate for redistribution, properly licensed, and compatible with Windows driver-signing requirements.

Do not require users to disable Windows driver security.

## Driver behaviour

The Assistant should:

1. Detect whether a compatible virtual display driver is already installed.
2. Install it with appropriate user permission if required.
3. Detect or create the WoWU virtual monitor.
4. Configure its resolution to 1280×800.
5. Enable it as an extended desktop display.
6. Identify it reliably, without confusing it with existing physical monitors.
7. Deactivate or restore its previous state when the session ends.

Where possible, avoid modifying an existing virtual display that belongs to another application.

We should not automatically remove or overwrite the user's existing drivers.

---

# 5. WINDOWS MONITOR ARRANGEMENT

Our chosen default arrangement is **vertical spanning**.

The virtual WoWU display is positioned directly beneath the selected primary gameplay monitor.

This is important because many players, including our existing test system, already have other monitors positioned to the left and right.

We should not assume those positions are available.

## Example

Main gameplay monitor:

2560 × 1440

WoWU virtual monitor:

1280 × 800

The virtual monitor is positioned immediately below the gameplay monitor, preferably centred horizontally beneath it.

The resulting WoW window client area is:

**2560 × 2240**

The combined window height is calculated as:

`GameplayHeight + 800`

The combined window width is normally the gameplay monitor's width, assuming that width is at least 1280 pixels and the virtual display lies completely underneath it.

## Display arrangement example

```text
       GAMEPLAY MONITOR
        2560 x 1440
┌────────────────────────────┐
│                            │
│      WORLD OF WARCRAFT     │
│                            │
│      3D GAME VIEWPORT      │
│                            │
└────────────────────────────┘
       ┌──────────────┐
       │              │
       │     WoWU     │
       │              │
       │  1280 x 800  │
       └──────────────┘
```

The lower-left and lower-right regions of the rectangular WoW window are unused desktop space.

These regions are not covered by physical monitors.

The Companion must calculate actual Windows monitor coordinates rather than assuming that the gameplay monitor starts at `(0,0)`.

Some users have additional monitors with negative desktop coordinates.

## Important requirements

- Do not rearrange unrelated monitors.
- Respect existing Windows display settings.
- Identify the selected gameplay monitor reliably.
- Account for Windows scaling and DPI.
- Handle differing monitor resolutions.
- Account for display orientation.
- Preserve the user's preferred refresh rate where possible.
- Save original display topology before making changes.

The virtual monitor should not necessarily become the Windows primary display.

---

# 6. WORLD OF WARCRAFT WINDOW MANAGEMENT

The Assistant must automatically manage WoW's window geometry.

We have experimented with virtual ultrawide monitors and modifications to `Config.wtf`.

Our experiments established that WoW can render on a 3840×1440 virtual display, but changing resolution settings alone did not achieve the desired dual-screen arrangement.

Attempting to force a 3840×1440 fullscreen resolution on the main monitor caused WoW to return to its normal fullscreen-windowed presentation.

We therefore decided to pursue **windowed-mode spanning**.

This is already proven to work in our current Offhand setup.

## Required behaviour

The Assistant should:

1. Detect the WoW process and its main window.
2. Wait until the game window is fully initialised.
3. Record the original window style, size and position.
4. Switch to the required windowed presentation when Gamepad Mode is activated.
5. Position the window across the gameplay monitor and virtual monitor.
6. Apply the calculated window client-area dimensions.
7. Keep the game visually immersive, with minimal unnecessary window borders.
8. Avoid repeatedly resizing or repositioning the window once configured.

The user should not have to drag or resize WoW manually.

## Critical detail

The **client area** must match the calculated rendering dimensions.

Windows window borders and title bars must not unexpectedly reduce the usable rendering area.

We may use native Windows window-management APIs to achieve this.

However, we must test whether custom window styles and positioning remain compatible with WoW Forever.

Avoid repeatedly fighting the game's own window management.

---

# 7. NATIVE WOWU VIEWPORT MANAGEMENT

This is one of the most important parts of the project.

**WoWU must no longer require Offhand.**

We need to implement the viewport behaviour currently provided by Offhand directly within the WoWU addon.

The Windows Assistant handles the overall window geometry.

WoWU handles the in-game rendering viewport.

## Example

Combined WoW window:

2560 × 2240

Gameplay viewport:

2560 × 1440

WoWU region:

1280 × 800

The game's 3D world should render only within the gameplay viewport.

WoWU should appear within the virtual monitor's region below it.

## Responsibilities of the WoWU addon

- Initialise the native viewport system.
- Receive or load the confirmed display geometry.
- Restrict the 3D world to the gameplay area.
- Maintain correct UI scaling and positioning.
- Position WoWU within the virtual screen region.
- Keep tooltips and touch coordinates aligned.
- Restore the original game viewport when Gamepad Mode is disabled.
- Handle UI reloads.
- Avoid regressions in existing modules.

We must test interactions with:

- Character preview windows.
- Collections previews.
- Maps.
- Fullscreen UI panels.
- Cinematics.
- Loading screens.
- Combat lockdown.
- Resolution changes.
- UI reloads.

We should implement and test the viewport subsystem independently before integrating it with the full Windows Assistant.

---

# 8. ADDON-TO-CLIENT COMMUNICATION

We have discussed using WoW SavedVariables as a simple communication mechanism.

WoW normally writes SavedVariables when the UI reloads, the player logs out or the game exits.

Our proposal is to create a dedicated SavedVariables table.

Suggested structure:

```lua
WoWUClientBridge = {
    request = 0,
    requestId = 43,

    viewportX = 2560,
    viewportY = 1440,

    gamepadX = 1280,
    gamepadY = 800,

    mode = "vertical"
}
```

## Field definitions

`request`

- `0` = Disable or deactivate Gamepad Mode.
- `1` = Activate Gamepad Mode.

`requestId`

A sequential identifier used to distinguish new requests from previously handled requests.

`viewportX`

Gameplay viewport width.

`viewportY`

Gameplay viewport height.

`gamepadX`

WoWU display width.

`gamepadY`

WoWU display height.

`mode`

Display arrangement type, initially `"vertical"`.

## Proposed activation communication

1. Player clicks Enable Gamepad Mode.
2. WoWU sets `request = 1`.
3. WoWU increments `requestId`.
4. WoWU triggers a UI reload.
5. SavedVariables are written to disk.
6. Windows Assistant detects the updated request.
7. Assistant processes the request only once.
8. Assistant configures the display and WoW window.
9. WoWU applies the viewport once the correct geometry is available.

## Important limitation

The Assistant can read SavedVariables from disk, but changes written by the Assistant will not automatically appear inside the already-running addon.

WoW also owns its SavedVariables files while running and may overwrite external changes.

Therefore, we should not rely on editing live SavedVariables as our primary client-to-addon communication method.

A more reliable approach may involve:

- Addon-to-client requests through SavedVariables.
- A separate generated configuration Lua file for confirmed display geometry.
- Loading that configuration during a controlled UI reload.
- Request IDs and acknowledgement states to prevent repeated operations.

The exact handshake must be prototyped and tested.

We should avoid resetting requests prematurely in a way that causes the Assistant to miss them.

WoW Forever's SavedVariables persistence must be verified against the current client build.

---

# 9. SUNSHINE INTEGRATION

We want the Windows Assistant to include Sunshine streaming functionality.

The user should not need to separately install and manually configure Sunshine.

Sunshine is open-source software, and we should investigate distributing a compatible version alongside our application.

**Preferred approach: bundle and manage Sunshine rather than directly merge its source code into the C# application.**

This reduces engineering complexity and allows Sunshine to remain a separately maintained component.

The distribution must comply with GPL-3.0 and applicable third-party licence obligations.

## Sunshine responsibilities

The Assistant should:

- Detect an existing Sunshine installation.
- Avoid conflicting with existing Sunshine services.
- Start or manage a compatible Sunshine instance where appropriate.
- Configure the correct virtual display as the streaming target.
- Preserve the user's existing Sunshine configuration.
- Make WoWU available as a Moonlight streaming destination.
- Ensure the streaming resolution matches 1280×800.
- Support native touchscreen input where the client and host permit it.
- Provide connection status to the Assistant.

The virtual monitor should be identified using its actual Windows display identity.

Do not assume the virtual monitor always has the same monitor number.

## Moonlight compatibility

We want to retain compatibility with as many Moonlight-capable devices as possible, including:

- Lenovo Legion Go.
- Steam Deck.
- Windows handheld PCs.
- Android tablets.
- Android phones.
- iPads.
- Other compatible Moonlight clients.

Touchscreen functionality and input modes may vary by device.

We should test representative client types rather than assume all Moonlight devices behave identically.

## Initial pairing

The first Moonlight connection will generally require pairing.

The Assistant should provide clear pairing instructions and, if supported, an easy way to identify the host.

Subsequent connections should be straightforward.

There should be no requirement for users to configure virtual monitor capture settings manually.

---

# 10. MINIMAL WINDOWS APPLICATION INTERFACE

The Assistant should not become a complicated launcher.

The user specifically prefers a small application that remains open and displays its current status.

The player launches WoW normally.

We do not need a large Launch WoW button.

## Suggested application statuses

**Waiting**

"WoWU Gamepad Assistant Running."  
"Please Launch World of Warcraft."

**Game detected**

"World of Warcraft Detected."  
"Waiting for Gamepad Mode activation."

**Configuring**

"Preparing your WoWU display."

**Active**

"WoWU Gamepad Mode Active."  
"Connect using Moonlight."

**Restoring**

"Restoring your original display configuration."

**Error**

"WoWU could not configure your display."

Provide a short, understandable explanation and a safe recovery option if configuration fails.

## Background operation

The Assistant should be capable of minimising to the Windows system tray.

Possible tray actions:

- Show status.
- Restore displays.
- Open settings.
- Exit Assistant.

Closing the main window should have clearly defined behaviour so users do not accidentally terminate an active WoWU session.

---

# 11. LOGIN SCREENS, CHARACTER SELECTION AND CINEMATICS

This is a critical usability requirement.

Our current Offhand implementation can cause login screens and cutscenes to extend across the combined display arrangement.

That means some content may be inaccessible or incorrectly displayed unless the touchscreen is connected.

We want to avoid this.

## Before character login

WoW must operate normally on the gameplay monitor.

Do not activate window spanning merely because the WoW process starts.

Wait until the player explicitly activates Gamepad Mode from inside the game.

## During gameplay

The spanned window and restricted viewport are active.

WoWU is presented on the virtual monitor.

## During cinematics

Investigate ways to keep cinematic content visible on the main monitor.

Possible approaches:

- Temporarily adjust the viewport.
- Temporarily change UI framing.
- Respond to relevant cinematic events.

The preferred approach is to avoid frequently resizing the entire Windows game window.

Not all WoW cinematics use identical mechanisms, so this requires testing.

## Character logout

When the player logs out to character selection, the preferred behaviour is to return to the normal gameplay monitor presentation.

If this cannot be detected reliably through the chosen communication system, investigate a safe fallback.

## Game exit

The Windows Assistant must restore the original configuration automatically.

---

# 12. SESSION MANAGEMENT AND RECOVERY

The Assistant must safely manage the complete session lifecycle.

## Before activation

Record:

- Existing monitor topology.
- Relevant virtual display state.
- WoW window position.
- WoW window dimensions.
- WoW window style.
- Relevant streaming configuration if modified.

## On successful activation

Mark the session active.

Keep sufficient information to restore the original state later.

## On normal shutdown

Restore the original configuration.

## On WoW crash

Detect process termination and perform the same cleanup.

## On Assistant crash

Maintain enough recovery information that the next launch can detect an unfinished session and offer or perform safe recovery.

## Essential safeguards

- Never disable unrelated displays.
- Never overwrite unrelated Sunshine settings without permission.
- Never assume monitor positions are fixed.
- Never leave the user's desktop unusable.
- Provide an accessible Restore Displays control.
- Handle disconnected or unavailable virtual displays.
- Avoid applying window changes repeatedly.
- Avoid accidental activation when unsupported WoW clients are running.
- Ensure installation and removal do not leave unwanted virtual displays or services active.

Restoration should be transactional where practical: if part of setup fails, undo the changes already made.

---

# 13. FUTURE INPUT MANAGEMENT

We previously discussed restricting the physical mouse cursor to the main gameplay monitor.

The intended behaviour is:

- Physical mouse remains within the gameplay monitor.
- Moonlight touchscreen interactions continue to operate WoWU on the virtual display.

Windows provides cursor-confinement functionality, but applying global cursor restrictions may interfere with touch input if the streaming client emulates mouse movements.

**This feature is deliberately deferred.**

Do not prioritise it during initial development.

We will investigate it only after the core WoWU Gamepad Assistant is operational.

---

# 14. DEVELOPMENT ROADMAP

Develop this application incrementally.

## Milestone 1: Native WoWU Viewport Proof of Concept

Goal: Eliminate the Offhand addon dependency.

- Implement a minimal native viewport manager inside WoWU.
- Use the existing working multi-monitor layout.
- Disable Offhand for testing.
- Confirm the gameplay viewport stays on the main monitor.
- Confirm the WoWU interface remains on the virtual display.
- Verify UI interaction and restoration.

**Success criterion:** WoWU operates correctly in the existing spanned environment without Offhand.

## Milestone 2: Basic Windows Companion

Goal: Reproduce the existing arrangement automatically.

Build a simple C# application that can:

- Detect the gameplay monitor.
- Detect an existing 1280×800 virtual monitor.
- Calculate desktop coordinates.
- Detect the running WoW process.
- Position and resize the WoW window.
- Restore the original window state.

Use the existing installed virtual display driver during testing.

Do not introduce automated driver installation yet.

## Milestone 3: Addon Communication

Goal: Activate Gamepad Mode from inside WoW.

Implement:

- In-game activation prompt.
- SavedVariables request bridge.
- Request ID handling.
- Window activation.
- Confirmed geometry handoff.
- Viewport activation.
- Deactivation and restoration.

The user should need only one in-game confirmation.

## Milestone 4: Virtual Display Automation

Goal: Eliminate manual virtual monitor configuration.

Implement:

- Driver detection.
- Safe driver installation.
- Virtual monitor creation or activation.
- 1280×800 resolution configuration.
- Automatic positioning below the gameplay display.
- Safe restoration and cleanup.

## Milestone 5: Sunshine Integration

Goal: Eliminate separate Sunshine setup.

Implement:

- Bundled or managed Sunshine installation.
- Compatible existing-installation detection.
- Streaming host management.
- Virtual display selection.
- Moonlight pairing workflow.
- Touch input verification.

## Milestone 6: Installation and User Experience

Goal: Make everything accessible to non-technical users.

Implement:

- A single Windows installer.
- Minimal status interface.
- Automatic WoW detection.
- Clear connection status.
- System tray behaviour.
- Error handling.
- Safe recovery.
- Uninstallation cleanup.

## Milestone 7: Compatibility Testing

Test against:

- Different gameplay monitor resolutions.
- 1080p and 1440p configurations.
- Multiple physical monitors.
- Unusual monitor arrangements.
- Different Windows scaling settings.
- Different refresh rates.
- Existing Sunshine installations.
- Different Moonlight receiving devices.
- Different supported WoW clients.

Forever remains the initial development target.

---

# 15. WHAT WE HAVE ALREADY PROVEN

The existing WoWU setup already demonstrates that:

1. WoW can be spanned across the main monitor and a virtual monitor.
2. Offhand can successfully restrict the 3D game viewport to the gameplay monitor.
3. WoWU can operate on a 1280×800 second-screen region.
4. Sunshine and Moonlight can stream the virtual display.
5. Touchscreen interactions can work across the streaming connection.
6. A virtual 3840×1440 monitor can be created and captured through OBS.
7. WoW can render at 3840×1440 on that virtual monitor.
8. Changing `Config.wtf` alone does not reliably produce the desired window arrangement.

These experiments provide useful proof of the underlying concept.

However, they do not yet prove that we can remove Offhand, automatically manage window geometry or achieve the intended SavedVariables handshake.

Those are the next engineering tasks.

---

# 16. IMPORTANT DESIGN DECISIONS

These decisions have already been made and should be preserved.

**1. No Offhand dependency.**

WoWU must implement its own viewport management.

**2. Keep Sunshine/Moonlight.**

We want wide compatibility with existing handhelds, tablets and phones.

**3. Bundle streaming functionality into the Windows Assistant.**

The user should not be required to manually install and configure Sunshine.

**4. Use a virtual 1280×800 display.**

This is the default WoWU display size.

**5. Use vertical spanning by default.**

Place the virtual display below the gameplay monitor, rather than assuming the left or right positions are free.

**6. The player launches WoW normally.**

The Assistant is not a game launcher.

**7. Do not span before the player enters the game.**

Login and character selection should remain normal.

**8. Require in-game confirmation.**

The addon presents an Enable WoWU Gamepad Mode prompt.

**9. Use a minimal Windows interface.**

Status text is sufficient for normal operation.

**10. Restore everything after WoW closes.**

Do not leave Windows display arrangements altered unnecessarily.

**11. Defer mouse confinement.**

Investigate it after the core application works.

**12. Preserve all existing WoWU features.**

Do not redesign the existing addon or disrupt working modules as part of this project.

---

# 17. IMMEDIATE NEXT TASK

Before building the complete Windows application, investigate and prototype the **native WoWU viewport manager**.

This is the most important technical dependency.

The first proof of concept should answer:

**Can WoWU reproduce the existing Offhand viewport arrangement while Offhand is completely disabled?**

Use the existing working vertical display configuration for this test.

Do not modify the user's current monitor setup unnecessarily.

Once native viewport management is proven, proceed to the Windows display and window-management prototype.

We should validate the core technical assumptions before investing significant effort in the installer, streaming integration or user interface.

---

# FINAL VISION

WoWU Gamepad Assistant should make our existing second-screen setup feel like a native feature of World of Warcraft.

The end user should not need to understand Offhand, virtual monitors, Windows display coordinates, viewport geometry, Sunshine configuration or window spanning.

Their experience should be:

**Install the Assistant.**  
**Install the WoWU addon.**  
**Launch the Assistant.**  
**Launch World of Warcraft normally.**  
**Enter the game.**  
**Click "Enable WoWU Gamepad Mode".**  
**Connect the handheld or tablet through Moonlight.**  
**Enjoy WoWU.**

When they exit WoW, everything returns to normal.

The Windows Assistant handles the operating system, virtual displays and streaming.

The WoWU addon handles the game viewport and touchscreen interface.

Moonlight handles the connection to the player's device.

**One cohesive experience, without Offhand, designed to be usable by players who have never configured a virtual display or streaming host before.**
