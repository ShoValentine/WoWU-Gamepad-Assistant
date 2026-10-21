# Mandatory Sunshine targeting contract

Status: design requirement, not implemented. 21 October 2026.

Never declare streaming ready from a successful config write alone.

1. Detect the installed Sunshine binary/version, service, config path and active sessions.
   Do not launch a second host or restart an unrelated active stream.
2. Persist the driver/device ownership established when WoWU creates its display.
   Match that identity through Windows display-device/DisplayConfig records; never
   identify ownership by resolution, monitor number, friendly name or first match.
3. Enumerate current topology for each activation and after display changes. Resolve
   the owned target's active adapter/target IDs, monitor device path, current GDI
   source name and physical desktop bounds. Driver recreation requires reconciling
   ownership; ambiguity is an error, never a fallback to the primary monitor.
4. Confirm the owned target is enabled at 1280 x 800. Check the rectangle below the
   gameplay display does not overlap any unrelated monitor. Do not rearrange them.
5. Use a version-specific Sunshine adapter. Modern builds document a device_id from
   Sunshine's display inventory; older builds document a DXGI output name. Match
   Sunshine's own inventory to the resolved Windows target. A PnP ID or adapter
   GUID is not interchangeable with Sunshine's capture ID. Unknown versions or
   ambiguous inventory must block activation.
6. Record original config bytes and the exact keys being changed, with a recovery
   journal written before mutation. Update output_name only plus separately agreed
   session settings. Preserve unrelated settings, credentials and app entries.
   Restore with conflict detection if settings changed during the session.
7. Apply via the supported version-specific mechanism; discover service/instance
   ownership before restarting. Prefer isolated config for a managed host, but
   isolated config does not eliminate port/service conflicts.
8. Re-read effective config and Sunshine's recognized target and capture geometry.
   Distinguish Prepared from Capture verified. Require evidence from an actual
   capture/stream, not merely the enumerated device list or a host desktop screenshot.
   Prototype a receiver/probe which checks a per-session visual challenge on the
   virtual display against decoded frames and validates the encoded 1280 x 800 size.
   A verification session must not expose another display to the remote client.
   Availability of such a gated probe is a milestone-5 feasibility requirement.
9. Only announce Ready after capture evidence matches ownership, target and size.
   On mismatch stop the WoWU-owned stream, undo changed settings and show recovery.
10. During a session, topology changes invalidate verification. Suspend the managed
    stream before resolving and verifying again; never allow capture fallback.

Moonlight chooses stream parameters too: host resolution alone does not establish
encoded stream size. Pairing/device input and requested resolution must be checked.
If installed Sunshine cannot prevent wrong-display fallback, do not enable that
integration; use a supported managed version or report it unsupported.

Official references consulted:
https://docs.lizardbyte.dev/projects/sunshine/latest/md_docs_2configuration.html#output_name
https://docs.lizardbyte.dev/projects/sunshine/v0.21.0/about/advanced_usage.html

Milestone order remains viewport POC, Windows geometry, handshake, virtual display,
Sunshine, installer, compatibility. No driver, service or display mutations yet.
