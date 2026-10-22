# Codex handoff: QA R2 persistent wholesale cargo map icon — Issue #3 (P1)

WORKSTREAM: independent map-blip feature. Physical wholesale delivery is now WORKING in real GTA. Do not undo or rewrite it.

## Protected base / branch
- QA R2 base commit 7693ed2167405f475dd4d36392c5df1f257fe178; parent draft PR #17.
- Work ONLY in feature/qa-r2-wholesale-cargo-map-blip. Draft PR MUST target qa/v0.3.3.1-final-review-r2 (not main/development). Do not mix with P0 crash-forensics or modify the owner's live installation.
- Codex may be Forbidden from PR review comments; use this checked-in handoff and source.

## Correctly understood live GTA evidence
- First ready WHOLESALE shipment of 5 Service Filters successfully collected at warehouse, loaded into registered vehicle, transported, unloaded at Glass Heroes and deposited as an actual visible box on the stockroom pallet. Inventory 85 -> 90, supplier invoice 1012 -> 1376 (+364), order cleared.
- Second wholesale order for 5 Brake Kits initially showed WAITING FOR ARRIVAL. This was correct: its new delivery timer had not finished. Owner CONFIRMED when ETA expired the next warehouse package spawned normally. DO NOT claim the second-order physical spawn or ETA is broken.
- Source calls SET_NEW_WAYPOINT once at order placement; it has no owned persistent WHOLESALE package Blip. This is the REAL open UX requirement. The owner wants a clear location marker on world map and minimap continuously while a wholesale order/package is outstanding until physically deposited at the parts pallet.
- Small ground DrawMarker appears only in short range (<45m), and pallet/box models stream in only near warehouse (<80m) at ready stage7. The new map blip must be independent of those distances/models and the assigned-vehicle vicinity.

## Acceptance criteria
1. Maintain exactly ONE owned long-range, named WHOLESALE package map blip from purchase/ETA pending (at warehouse) through ready pickup at fixed warehouse, player carrying, registered vehicle loaded and driving, unloading/return to parts pallet, and only remove it after actual successful stocking. Use a recognizable supported icon/color, distinct from customer-car blips.
2. During pending ETA and ready stage7 use fixed warehouse coordinate; don't spawn physical props before ETA. During carrying stage6 follow player/carried prop; loaded stage9 follow the actual registered vehicle when safely available; unloaded at shop follow carrier until deposited. Safely fall back to last known actionable location if entity transiently unavailable. Update at throttled refresh or state change, NOT every draw frame. Distinguish missing vehicle, reload/recovery without expensive unbounded nearby vehicle scans.
3. Remove ONLY the owned blip on actual stock completion, cancellation or invalid order; reconstitute exactly one after script reload/recovery; no duplicates or stale icons. Never remove user-created waypoints, other mods' markers, or customer blips. Do not remove on mere warehouse collection or vehicle loading. Avoid orphaned blips on script abort.
4. Keep existing in-world pickup marker/hints. Do not touch ordering, prepaid migration, active unpaused ETA logic, assigned vehicle validation, inventory, accounting, invoices, or Save.ini schema. No new required settings; current hold/startup defaults remain.
5. Add targeted blip tests: no order; pending ETA; ready >80m; model unavailable; pickup; carried; load vehicle; movement; unload; deposit clears; abort/reload reconstructs exactly one; second order recreates; cancel/recover; failed entities; existing save migration; no duplicate per tick; preserved customer and user waypoints. Also rerun all inherited Release build, static/behavior/logger and financial/preservation checks.
6. Deliver matched install + VisualStudio C# source, full diff, README, MD/DOCX/PDF developer guide, detailed test logs/source hashes, single-install-source experimental ZIP and rollback. Mark new GTA tests NOT RUN. Remain DRAFT and unmerged until independent review plus user GTA qualification.

P0 crash investigation is higher priority and separate. Do not alter R2 QA baseline or experiment on the user's active game.
