# First run — v0.1.0

1. Extract the full ZIP. Run WoWU.GamepadAssistant.exe on the gaming PC.
2. Keep your existing virtual display enabled at 1280 x 800 directly beneath the
   gameplay monitor. This prototype does not change monitor arrangement.
3. Stop Offhand's addon and any external helper that resizes WoW. Launch WoW normally,
   select Windowed mode and log into the game world. Do not run viewport preview yet.
4. In the Assistant, select the WoW window, gameplay monitor and virtual monitor.
   Monitor names, sizes and desktop coordinates are shown to identify them.
5. Tick the in-world confirmation and choose Preview span. A successful span
   automatically restores after 30 seconds unless you choose Keep span.
6. Choose Keep span, then type the viewport command displayed by the Assistant.
   It includes your exact gameplay dimensions and the lower display's horizontal
   offset, so the virtual monitor need not be horizontally centred.
7. Type /wowuviewport keep in game to retain the separate addon preview too.
8. To finish: /wowuviewport off, then Restore window in the Assistant. Closing the
   Assistant also restores. Restoration returns to the window state at activation;
   if it was already spanned, that original span is restored.

Use WoWU v0.11.0-poc2 and its Compass v0.4.1. This ZIP contains only the Windows app.

If WoW rejects the requested span, the app attempts restoration and reports an
error. It does not repeatedly force the window size. If the game is running with
higher privileges, Windows may reject the operation; no automatic elevation is used.

Sunshine and Moonlight remain as currently configured. This version does not
claim streaming readiness. For milestone 2, report whether the window spans,
the shown viewport command works, and Restore returns to the previous layout.
