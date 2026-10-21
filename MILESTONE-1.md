WoWU v0.11.0-poc1 + Compass v0.4.1 - Native viewport proof of concept

This is an experimental milestone-1 build, not the finished Gamepad Assistant.
It changes no Windows display settings, drivers, services or Sunshine config.
The existing v0.10.3 package remains available for normal use.

Setup for this proof of concept:
1. Keep your existing vertical display layout and external window-span mechanism.
   This addon cannot create or resize the Windows span by itself.
2. Disable the Offhand ADDON and reload. Do not remove your virtual display driver.
3. Your existing window client area must already span the gameplay display plus
   the lower 1280 x 800 monitor. Enter the game world, outside combat.
4. Run /wowuviewport status to see the current render dimensions.
5. Example only: for a 2560 x 1440 gameplay monitor and a centred lower display:
   /wowuviewport preview 2560 1440
   Expected current render area is 2560 x 2240. Use your actual gameplay dimensions.
   For a non-centred lower monitor, append its horizontal pixel offset relative
   to the gameplay monitor's left edge, e.g. /wowuviewport preview 2560 1440 640.
6. The preview reverts after 30 seconds. Run /wowuviewport keep to keep it for this
   session, or /wowuviewport off to restore immediately. Restoration waits if in combat.

The addon checks render dimensions and refuses an active Offhand addon. It cannot
independently detect Windows monitor positions: geometry must match the existing
span. Only layouts with the lower display entirely beneath the gameplay width
are supported in this prototype. Nothing activates automatically on login.

Implemented:
- WorldFrame crop, temporary pixel-aligned WoWU placement, compass gameplay bounds.
- Snapshot/restoration of viewport and WoWU frame anchors/scale/visibility.
- Preview timeout; explicit stop; recovery after interrupted preview.
- Attempt restoration on logout, loading transitions, resolution/UI-scale changes
  and cinematic/movie start. No automatic reactivation afterward.
- Prevent dragging/rescaling WoWU while the preview is active.

Not yet implemented/proven:
- General Blizzard HUD/UIParent confinement and full-screen panel/cinematic framing.
- C# Assistant, window management, driver automation, SavedVariables handshake,
  automatic Sunshine selection or capture verification, installation or pairing.
- Native viewport behaviour on the running Forever client is not yet verified.
- If Offhand currently owns your Windows span, a separate window-spanning step
  remains necessary until milestone 2; this build does not replace that component.

No simulated tests or live game/display changes were performed.
Report whether world cropping, lower-screen WoWU touch input, compass placement
and restoration work, plus your /wowuviewport status output if they do not.
