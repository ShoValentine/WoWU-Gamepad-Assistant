# Project status

Reviewed development packages, 29 October 2026:

| Component | Version |
|---|---|
| WoWU addon | 0.13.2 |
| WoWU Compass | 0.4.4 |
| Windows Gamepad Assistant | 0.7.3 |

These are the current development packages supplied by the maintainer. The repository's original Assistant source snapshot is v0.1.0; it has not been synchronized as part of this documentation/artwork update. There is no claim that a current downloadable GitHub release exists.

## Latest verification

- 146 current behavior/compatibility checks passed, including client/module gates, minimap lifecycle, controller held-action release, touch scrolling and Assistant parsers/geometry.
- All 109 current addon/Compass Lua files compiled with Lua 5.1; 12 restricted controller snippets also passed static parsing.
- Assistant Release build/publish completed without warnings or errors.
- Fourteen manifests, 91 image resources and 59,361 imported location records were audited.
- Nine historical harnesses are outdated as an unchanged suite; their failures were documented rather than counted as successes. Reusable touch assertions were separately exercised against the current source.

This is **automated/static development verification**. It is not a live test of every client, driver, protected action or network stream. The latest review did not launch the game/Assistant, change displays or run Sunshine/Moonlight.

## Corrections from that review

- Restored access to WoWU's own notification-history page on Classic Era, TBC and Wrath.
- Made the Assistant ignore non-positive and out-of-range companion request IDs instead of repeatedly throwing on malformed data.

## Coverage limits

- Forever keeps its native gamepad implementation; WoWU's newer independent pad HUD and fishing possession are separate systems.
- The custom Retail finder does not yet contain the dedicated Mists PvP pages.
- The custom split-pane bank/Warband interface is Retail-only; legacy clients use native bank interactions.
- Other Classic branches have adapters and fallbacks, not a full live acceptance record or every Era/Mists layout feature.
- Native proc, cursor, bank, profession, journal and queue features depend on the APIs and permissions that the particular client exposes.
- Known rare/treasure coordinates do not prove a live spawn, and the catalogues are not exhaustive.
- Modern AH caches are query-driven; last-observed prices are not guaranteed live prices or a legacy full-realm scan.
- Sunshine capture initialization is checked; decoded Moonlight receiver frames are not automatically verified.

See the [complete feature matrix](FEATURES.md) and [CSV](FEATURES.csv) for the implementation route per client.
