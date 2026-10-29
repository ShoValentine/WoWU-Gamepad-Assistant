# WoWU feature and client support

Reviewed 29 October 2026. Addon **v0.13.2**, Compass **v0.4.4**, Assistant **v0.7.3**.

This is a source-audited implementation matrix, not certification that every feature works in every live client. Retail, Forever, Classic Era and Mists Classic have project testing history. TBC, Wrath and Cata have adapters/guards and some fallback windows but no complete live acceptance pass recorded in this review.

**Custom** = WoWU implementation; **Native** = Blizzard/Forever UI, controls or hosted fallback; **Conditional** = requires the relevant client API, learned ability, service or unlock; **Limited** = only part of the requested custom experience exists; **No** = not supplied by this version/client route. A conditional status does not create a feature absent from that expansion.

Core columns use: Retail / WoW Forever / Classic Era (including Era-like Anniversary interfaces) / Mists of Pandaria Classic. The CSV also includes TBC, Wrath and Cataclysm Classic columns. Version detection follows the running interface/API, not the executable filename.

## Companion

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Automatic enable/disable handoff and viewport | Custom | Custom | Custom | Custom | Assistant must recognize the installation and own the virtual display; in-world geometry must match. |
| Windowed transition and native window-mode restoration | Conditional | Conditional | Conditional | Conditional | Uses client CVars plus the Windows Assistant; server/client restrictions can differ. |
| Automatic gameplay UI scale and custom scale slider | Custom | Custom | Custom | Custom | Original scale restored on exit; separate from dashboard/controller scaling. |
| Convert current native Edit Mode layout | Custom | Custom | Native | Native | Retail/Forever use native Edit Mode conversion; Era/Mists apply the saved Classic preset. Automatic repositioning is disabled for other Classic builds. |
| Keep viewport after reload and map transitions | Custom | Custom | Custom | Custom | Implemented resume/recovery paths; real reconnect timing is client-dependent. |
| Hide WoWU outside gamepad mode; show/hide commands | Custom | Custom | Custom | Custom | Manual /wowushow remains available; normal startup is hidden. |
| Restore minimap, decorations and native controls | Custom | Custom | Custom | Custom | Mode owns the map only while viewport active. Positions, masks, visibility and collected addon controls restore. |

## Dashboard

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Three saved editable widget layouts | Custom | Custom | Custom | Custom | Move/resize, choose widgets and save per-profile settings; secure changes wait for combat to end. |
| Minimap with inset zone name | Custom | Custom | Custom | Custom | Square mask while docked; original mask restored afterward. |
| Clock, calendar, tracking, zoom and addon-controls widget | Conditional | Conditional | Conditional | Conditional | Native controls appear where the client provides them; no calendar is invented for Era. Horizontal/vertical arrangement supported. |
| Quest tracker widget | Custom | Custom | Custom | Custom | Era/Mists use independent WoWU watches; other clients use their quest/watch APIs. |
| Quick Access grid, single buttons and multiple instances | Custom | Custom | Custom | Custom | Items/spells/mounts; row/column sizing, centered single button and press-and-hold reorder. Actual actions require native support and user input. |
| Damage/healing meter widget | Conditional | Conditional | Conditional | Conditional | Requires native C_DamageMeter; no independent combat-log parser. Missing API means no readings. |
| Dungeon Hub widget | Conditional | Conditional | Conditional | Conditional | Shared presentation; meter, threat and instance information require the corresponding native APIs. |
| XP, money, bag-space and durability widgets | Custom | Custom | Custom | Custom | Read-only information; unavailable/max-level XP is handled separately. |
| Emotes and keyboard widget/buttons | Custom | Custom | Custom | Custom | Keyboard writes to supported edit fields; sending/confirming remains a user action. |
| Notifications hidden when idle | Custom | Custom | Custom | Custom | Alert card is hidden with no current alert; full history remains accessible through Adventure. |

## Input

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Independent four-layer controller assignments | Custom | Native | Custom | Custom | No trigger, LT, RT, LT+RT. Forever retains its own native controller implementation. Other Classic controller APIs need validation. |
| Compact eight-button controller HUD | Custom | Native | Custom | Custom | WoWU HUD layers occupy one pair. Forever uses its own HUD rather than WoWU Compact Layout. |
| Controller cluster/pair edit mode and scaling | Custom | Native | Custom | Custom | Four-button diamonds or eight-button pairs; changes outside combat. Forever uses its native controls. |
| Pressed-button illumination and refined button frames | Custom | Native | Custom | Custom | Cosmetic WoWU HUD feedback uses mapped physical input. Native Forever styling is separate. |
| Yellow animated proc borders | Conditional | Native | Conditional | Conditional | Only when the client exposes public native proc signals. No guessed procs from buffs or cooldowns; Era generally lacks these signals. |
| Hold to Cast repeated casting | Conditional | Native | Conditional | Conditional | Off by default. Requires native held-casting APIs/CVar and spell on an existing action bar, even a hidden bar. Items/macros stay single-press. Forever uses native behavior. |
| LB/RB targeting layers, friendly/enemy cycling and Back | Custom | Native | Custom | Custom | WoWU secure shoulder layers for Retail/Classic; Forever native bindings are not replaced. Combat restrictions apply. |
| Ground-targeted spell aiming | Conditional | Native | Conditional | Conditional | Native cursor/free-look APIs and secure click emulation required. Cannot certify targeting in a mock environment. |
| Vehicle/possessed action replacement | Conditional | Native | Conditional | Conditional | Requires matching native action-page APIs. Pet/stance/vehicle UI differs by client. |
| Specific Skyriding button mapping | Custom | No | No | No | RT+A ascent, RT+X forward, RT+Y surge, RT+B halt; LT+A Second Wind. Other layers preserved. |

## Spells and gear

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Spellbook with paging, swipes and click inspector | Custom | Custom | Custom | Custom | Uses private Classic API adapter where needed. No drag-to-action-bar requirement. |
| +Pad and +Dashboard assignment actions | Custom | Limited | Custom | Custom | Dashboard pins are shared. +Pad requires the WoWU controller preset; Forever native pad-assignment flow is separate. |
| Item inspector, equipped comparison and concise stat changes | Conditional | Conditional | Conditional | Conditional | Equipment comparisons only for gear; statistics and equipped slots depend on native data. |
| Character model, equipment, statistics and currencies | Conditional | Conditional | Conditional | Conditional | Model/stat data and supported currency/reputation APIs vary by client. |

## Bags and trading

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Inventory grid, adjustable icon sizes and item actions | Custom | Custom | Custom | Custom | Use/equip/split/drop/protection through hardware-driven native actions; swipe gestures avoid dragging. |
| Bag cleanup/sorting | Native | Conditional | Custom | Custom | Classic uses WoWU one-move-at-a-time sorter. Retail uses native cleanup. Forever follows its supported cleanup API. |
| Bank split view and selected-item transfers | Custom | No | No | No | Custom banking is explicitly Retail-only. Other clients retain native banking rather than this custom two-panel transfer UI. |
| Native bank-window docking fallback | No | Conditional | Native | Native | Legacy/native bank interaction remains available; this is not the Retail custom transfer interface. |
| Reagent deposits and purchasable tabs in custom bank | Conditional | No | No | No | Retail custom bank features depend on supported native reagent/tab APIs. Legacy bank slot purchases use native windows instead. |
| Warband bank and shared-money deposits | Conditional | No | No | No | Retail only, subject to current bank/tab APIs and account unlocks. |
| Merchant catalogue, buy/sell/buyback and repair | Custom | Custom | Custom | Custom | Requires open merchant; purchasing still needs a user action. |
| Mailbox in bags, attachments, money and keyboard | Conditional | Conditional | Conditional | Conditional | Requires native mail APIs and open mailbox; COD/confirmation restrictions remain. |
| Custom Auction House browse, categories, buy, sell and owned auctions | Conditional | Conditional | Conditional | Conditional | Retail/Forever use modern AH when available; Classic uses the legacy backend. Native permissions and commodity differences remain. |
| AH scan cache and tooltip prices | Conditional | Conditional | Conditional | Conditional | Legacy full scans are throttled by the client; modern AH uses eligible queries/results rather than a guaranteed whole-realm GetAll scan. Prices are last-seen observations per realm/client. |

## Quests and navigation

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Parchment quest journal, objectives and rewards | Custom | Custom | Custom | Custom | Uses private legacy quest adapter where necessary. |
| Track/untrack, Navigate, Show on Map and confirmed Abandon | Conditional | Conditional | Conditional | Conditional | Quest position/area navigation needs native quest POIs or a supported database. Abandon uses a confirmation. |
| World map and touch navigation | Custom | Native | Custom | Custom | Retail map chrome removed; Era/Mists custom art and navigation paths. Other Classic clients use generic native-map fallback. |
| Mists numbered quest pins and highlighted objective areas | No | No | No | Custom | Mists-specific POI selection links to WoWU quest tracking. |
| Compass headings and destination indicators | Conditional | Conditional | Conditional | Conditional | Uses camera-facing API where available; direct navigation needs public position data. Player-facing fallback may apply if the client lacks camera data. |
| Quest, destination and corpse trails | Conditional | Conditional | Conditional | Conditional | Direct guidance only: does not calculate road/pathfinding routes. Classic quest locations are strongest for the dedicated Era/Mists adapters. |
| Services, rares, treasures and notable-place catalogues | Custom | Custom | Custom | Custom | 59,361 guarded coordinate records across seven profiles. Coverage is source-dependent; not a claim to every known live spawn/location. No gathering-node import. |
| Map filters, swipe destination list and destination details | Custom | Custom | Custom | Custom | Services/Places menus, Rares, Saved, Explored and search; source markers/availability depend on profile. |
| Saved map positions/pins and explored-location log | Conditional | Conditional | Conditional | Conditional | Personal observations; not a complete historical exploration database. Requires native map positioning. |

## Talents

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Talent selection and specialization UI | Custom | Custom | Native | Custom | Retail custom class/spec trees; Mists custom six-tier one-per-row selector and specialization previews. Earlier Classic docks native talents. Forever uses its custom trees. |
| Retail specialization cards and activation | Custom | No | No | No | Retail free-spec switching follows native restrictions and available API. |
| Forever Legacy trees | No | Custom | No | No | Explicitly Forever-only. |

## Professions

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Profession cards and recipe crafting screen | Custom | Custom | Native | Custom | Retail/Forever current profession UI; Mists legacy recipe adapter. Earlier Classic docks native profession windows. |
| Mining opens Smelting; gathering cards are noninteractive | Custom | Custom | Limited | Custom | Profession/API-specific; pre-Mists native hosting does not share every custom-card refinement. |
| Cooking Fire action in custom cooking view | Conditional | Conditional | No | Conditional | Custom cooking view offers learned Cooking Fire; earlier Classic profession hosting does not provide this WoWU button. |
| Disenchant, Milling and Prospecting item tools | Conditional | Conditional | Conditional | Conditional | Requires learned profession/spell and an eligible selected item. Milling/Prospecting are unavailable in versions that lack those professions. |
| Clickable reagent inspector, source hints and AH prices | Custom | Custom | Limited | Custom | Source hints/examples are not exhaustive. Prices need a local scan; pre-Mists native profession windows do not share every custom reagent control. |
| Fishing Companion session/catch view | Custom | Limited | Limited | Custom | Retail/Mists custom entry. Earlier Classic profession routing is native; Forever fishing possession is not wired to its own controller preset. |
| Retail Cast/Reel In button | Conditional | No | No | No | Cast uses Fishing; Reel In uses the native interact binding, requiring client interaction settings/bobber eligibility. |
| Fishing control possession and stick remapping | No | No | Conditional | Custom | Mists uses A cast, X reel, temporary left-stick cursor and movement lock. Requires WoWU controller mode plus device remap APIs; earlier Classic lacks a dedicated profession entry. |

## Collections

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Mount and companion collections | Conditional | Conditional | Conditional | Conditional | Era/TBC use inventory/learned companion fallback; later clients use available journals. Actual summon uses secure actions. |
| Appearance browser with paged model previews and zoom | Conditional | Conditional | No | No | Retail/Forever transmog APIs; Classic collection implementation does not expose appearances. Only visible-page metadata/model work is requested. |
| Toys browser, large icon, inspector and Use Toy | Conditional | Conditional | No | No | Requires toy-journal APIs. Classic collection tabs do not expose this toy browser. |

## NPC interactions

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Integrated NPC dialogue, quest offers, progress and rewards | Conditional | Conditional | Conditional | Conditional | Possesses the current module content while the interaction is open, with native close/back behavior. |
| Trainer and other NPC-window docking | Conditional | Conditional | Conditional | Conditional | Uses interaction-manager enums where available, legacy show/hide events for Classic. Protected/unsupported windows may require native use. |
| Transmogrifier docking | Conditional | Conditional | No | Conditional | Only if the client supplies a transmogrifier window; standard Era/TBC/Wrath do not supply this service. |
| Trading Post docking | Conditional | No | No | No | Retail PerksProgram frame only; native purchase/cart controls remain native. |

## Notifications

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Adventure alerts, larger readable log and history | Custom | Custom | Custom | Custom | History access corrected for Era/TBC/Wrath in v0.13.2. User option gating and per-character bounded history. |
| Mail, bag space, repair, talent and upgrade reminders | Conditional | Conditional | Conditional | Conditional | Only when corresponding game APIs/events and enabled notification categories supply data. Upgrade hints do not auto-equip. |
| Queue, group, summon, resurrection and ready-check prompts | Conditional | Conditional | Conditional | Conditional | Actual native prompts are positioned on WoWU; acceptance remains a user action. Queueing systems differ by expansion. |
| Auction, crafting completion, fishing lure and calendar notices | Conditional | Conditional | Conditional | Conditional | API/event-dependent. Calendar reminders need a calendar; crafting notices need successful craft events. |

## Social

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Custom social/chat UI and keyboard | Conditional | Conditional | Conditional | Conditional | Friends, guild, chat, invite and target actions only where their native APIs are available; network/account services are client-dependent. |

## Other modules

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Achievements | Custom | Conditional | No | Custom | Classic module visibility starts at Wrath. Forever availability follows its actual achievement API. |
| Custom Dungeon Journal / Adventure Guide | Custom | No | No | Custom | Named Dungeon Journal in Mists; Adventure Guide in Retail. Cata uses native hosting. Forever tile is currently hidden, despite bundled Compendium code/data. |
| Dungeon Finder, raids, roles and queue controls | Custom | No | No | Custom | Mists/Retail custom views; progression Classic uses native hosting. Forever retains its own native systems. |
| PvP pages inside Dungeon Finder | No | No | No | Custom | Dedicated Mists Casual/Rated/War Games paths. The Retail custom finder currently has no PvP tab; earlier progression uses native hosting. |
| Housing Dashboard and Shop shortcuts | Conditional | No | No | No | Retail shortcuts open Blizzard windows; no custom commerce implementation. Housing requires a client build with the feature. |

## Quality of life

| Feature | Retail | Forever | Era | Mists | Details |
|---|---|---|---|---|---|
| Floating loot pickup feed and fast loot | Conditional | Conditional | Conditional | Conditional | Optional existing QoL feed; not the removed duplicate loot-feed module. Native loot information is required. |
| Optional auto-sell, repair and mail collection | Conditional | Conditional | Conditional | Conditional | Opt-in bounded convenience actions; native permissions, protected items, COD and purchase confirmations are respected. |

## Assistant support

The Windows x64 Assistant is shared across clients. It recognizes Warcraft product metadata or a Warcraft-style executable/title plus Interface/WTF folders, rather than one fixed executable filename. It still requires a detectable game window and an installed addon with the companion configuration file. Multiple clients are deliberately not auto-selected.

| Feature | Status / limit |
|---|---|
| Branded minimal main window; fading splash and stage quotes | Implemented; WPF builds on .NET 10 Windows x64. |
| Automatic virtual display install/re-enable, 1280×800 setup and placement | Implemented for the owned driver. Windows administrator approval, driver compatibility or a reboot can still be required. |
| Disable owned display and restore window/topology on close | Implemented with recovery journals; unexpected termination or OS failure cannot guarantee immediate restoration. |
| Automatic WoWU saved-data discovery and companion handoff | Implemented. No SavedVariables Lua is executed; generated CompanionConfig is written to the addon installation. |
| Integrated Sunshine lifecycle | Assistant-owned separate process; pinned engine download, owned-display checks, log capture diagnostics and process job cleanup. |
| Automatic Moonlight PIN-entry popup | Implemented; remote device requests pairing and user enters its PIN. |
| Audio-free companion stream | Generated Sunshine configuration disables audio and Steam audio-driver installation. |
| Display identity / topology change handling | Detects changed topology and stops streaming/restores the game session; restart can be required. |
| Stream-receiver verification | Capture initialization dimensions/offset are checked; decoded Moonlight receiver frames are not automatically verified. |
| macOS / Linux / ARM Assistant | Not provided by the current Windows x64 package. |

## Coverage limits worth keeping visible

- Forever uses its native controller system. WoWU Compact Layout, independent +Pad capture and Hold to Cast are not applied to that native HUD by this addon.
- There is no WoWU fishing-possession integration for Forever’s native controller preset. Era/TBC/Wrath/Cata profession routes also do not expose every Mists/Retail custom-card feature.
- Location catalogues are substantial but source-dependent. Rare coordinates are known locations, not proof of a live spawn; imported POIs are not a claim of exhaustive coverage.
- Retail/modern AH data is query-driven and may be cached incrementally. It is not the same guarantee as a legacy full-GetAll auction scan.
- Controller combat permissions, cursor targeting, native UI embedding and faction/level restrictions require tests on the actual client.
- All price and source hints are observations/examples and may be stale or incomplete.

The source column in the CSV names the implementation files for each row. The accompanying PROJECT-STATUS.md summarizes development verification and remaining acceptance work.
