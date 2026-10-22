# v0.3.3.1 PR19 wholesale package blip — EXPERIMENTAL

Independent review candidate on feature/qa-r2-wholesale-cargo-map-blip, draft PR #19 targeting qa/v0.3.3.1-final-review-r2. Protected R2 base: 7693ed2167405f475dd4d36392c5df1f257fe178. No PR18 forensic code or changes to normal diagnostics. GTA testing of this candidate: NOT RUN. Overall stack-overflow cause remains UNPROVEN. No merge, deployment or live save/settings edits performed.

## Confirmed behavior and actual problem

Owner-confirmed R2 gameplay: first wholesale delivery of five Service Filters was collected, loaded into the registered vehicle, transported, unloaded and stocked as a real pallet box. Inventory increased 85 to 90; supplier invoice increased 1012 to 1376, exactly 364; order cleared. The next five Brake Kits package spawned normally when its ETA expired. Physical spawning and the second ETA are not diagnosed as broken.

The missing feature was continuous map guidance: R2 sets a user waypoint at purchase, draws a nearby ground marker below 45m and streams ready warehouse props inside 80m. It had no owned persistent package map icon. This candidate adds that icon independently of props, range and financial fulfillment.

## Before and after / exact methods

Before: one purchase waypoint can be replaced by the player and no package icon follows cargo. After: one owned orange Package sprite named WHOLESALE package appears at the fixed warehouse while ETA is pending and when ready, follows the carrier after pickup, follows a safely resolved registered vehicle after loading, follows the carrier after unloading, and disappears when successful stocking clears the order. Existing hints, warehouse ground marker, streaming distances and waypoint calls remain unchanged. The feature never adds/removes a user waypoint or modifies customer blips.

OnTick has one new UpdateWholesalePackageBlip(now) call inside its existing once-per-second visual refresh, after wholesale visuals. OnAborted calls DeleteWholesalePackageBlip, which does no native work when no handle exists. Constructor only changes the experimental log label; there is no new startup native or save parsing. Three new helpers implement update, position resolution and owned cleanup. Matching source.diff includes both active source copies and reproduces them exactly from R2.

All 282 existing private R2 methods are checked by immutable hashes after removing only those two exact new visual lifecycle hooks. This includes BuyInventory, ProcessSupplierDelivery, TrySupplierPackageInteraction, CommitSupplierDelivery, CancelSupplierOrderWithConfirmation, RecoverSupplierShipment, Save, active-unpaused ETA, validation, warehouse retry/exhaustion cooldown, stock provenance, jobs and financial methods. The PR9 logger region and other diagnostic methods remain byte-identical after the approved hooks are removed. Original 21 static checks are retained, alongside full regression suites. New helpers are also checked against disallowed save/commit/waypoint/scan/model-request calls. Preservation checks are source guards, not a claim of executing real GTA persistence.

## Lifecycle and position behavior

No valid wholesale order means no icon. A valid order uses the existing kind, supplier, positive quantity, fixed warehouse and order identity; no new persisted fields are read/written. Pending ETA creates only a map icon, not a pallet or box. Ready stage7 remains visible on world map/minimap from beyond 80m with missing models. Coordinates are refreshed at the existing approximately one-second visual cadence, never in DrawMarker or on each draw frame.

During stage6 carrying/unloaded, a valid player position is used. During loaded stage9, the helper retains an independent reference to a validated registered vehicle even if the pre-existing nearby visual cache clears. Identity uses the existing unchanged MatchesWholesaleVehicle helper, with plate validation no more than every three seconds per refresh cycle; movement updates approximately once per second. Up to three candidate plate checks may occur in a validation cycle: retained, existing cached and player's current vehicle. A wrong plate/model is rejected. No new World.GetNearbyVehicles or unbounded/global entity scan is added; the original cargo visual resolver remains unchanged. An owned attached cargo prop is a fallback when vehicle resolution is unavailable.

Transient missing entities leave the icon at its last observed actionable coordinate in this script instance. The in-memory coordinate is intentionally not added to Save.ini. On a fresh reload, the initial fallback is the fixed warehouse until the existing cargo resolver or the player's registered current vehicle supplies a safe location. Approach the assigned vehicle or use the existing Recover Pending Shipment command if the loaded entity is gone; recovery resets logical cargo to warehouse without stocking or invoicing. It is not an icon claim that the lost vehicle physically sits at the warehouse. This fallback requires in-game UX review.

Collection, loading and unloading retain the SAME icon. Failed deposit, missing pallet or full capacity leave the pending order and icon intact. Successful deposit, cancellation, invalid/no order or a switch to non-wholesale removes only the owned icon on the next successful visual refresh (normally within one active second). Recovery retains that icon and relocates it to warehouse; a subsequent order resets the order identity and location. Reload/abort cleanup followed by reconstruction creates exactly one active owned icon in normal operation.

New visual failures are caught locally, so they do not close the shop or mutate the order. Creation/configuration failures back off for ten seconds. If a setter fails after creation, the same owned handle is retained and configured on retry rather than leaked. Failed cleanup retains the reference for another attempt, with backoff. An abort makes a best-effort immediate owned removal; a native failure or fatal process crash cannot guarantee a final callback. No delete-all-blips operation, route activation or waypoint-removal native exists in the new code.

## Defaults, saves and compatibility

SHVDN2 2.11.6 / .NET Framework 4.8 / C# 7.3 / .2.cs remain unchanged. Package sprite and Orange color compile against the real pinned API. Customer markers remain yellow and short-range; this package marker is long-range and explicitly named. Actual visual appearance still requires GTA validation.

No required or optional setting is added by PR19. HoldGameplayTick=false and StartupDelaySeconds=30 remain the existing missing-key defaults. Existing settings are preserved. Save.ini schema, prepaid migration, registered model/plate, order identity, pending ETA and finance/inventory semantics remain as R2. New fields are session-only owned-visual state. No new disk write or financial timer exists in the feature. Owner data has not been read, installed, edited or restored during this work.

## Automated coverage and limitations

New extracted-production-member tests exercise no order; pending ETA; ready beyond 80m with no props; ETA expiration; collection and moving carrier; registered loaded vehicle and movement; clearing the nearby visual cache; unloading; successful deposit lifecycle; failed/full deposit; abort before/after activation; reload reconstruction; second order; cancellation and foreign markers; recovery; missing vehicle/carrier; loaded reload; wrong plate/model; owned cargo fallback; old saves with no new fields; repeated refresh without duplicates; lost handles; creation/configuration/cleanup failure and recovery; invalid/non-wholesale cleanup.

These are deterministic stubbed-API behavior tests, not execution of GTA native rendering or a full financial persistence simulation. All inherited 63 behavior tests include the nine existing PR9 logger tests; they are counted once. Original 21 static checks, actual Release build, matching source hashes, source-diff application and financial preservation guards run separately. Check recorded results and raw transcripts below.

## Owner test plan — NOT RUN

After independent review and explicit selection of this candidate, test cold start and delayed activation; hold before gameplay; warm reload with pending, carried and loaded cargo; pause/resume without ETA catch-up; map and minimap at distances greater than 80m; ready props with temporary model failure; pickup and carrying; load only the registered vehicle; drive far from warehouse/shop; leave/re-enter vehicle; unload and walk to the pallet; failed/full deposit then successful stocking; cancel, recover and second order; missing/streamed vehicle; abort/reload; foreign customer icons and a manually replaced user waypoint. Confirm exactly one named orange package icon, reasonable one-second following and correct fallback behavior. Verify the ground marker and existing interaction hints remain usable. Cold and warm runs are separate scenarios.

For each complete order compare stock, supplier invoice, funds, wages and ETA against expected values before/after save and reload. Preserve the tested first/second wholesale flow and active-unpaused-only progression. Do not infer stability from automated tests or from the prior selected R2 successes. Crash investigation has priority; the latest owner report is October 22 at 4:21:49 PM local, uptime 29m20s, 0xC00000FD at clr.dll+0x688D5, responsible script unknown. This P1 feature is not a crash fix and contains no P0 additions.

## Review package, installation and rollback

Complete source-only ZIP contains matching Install/scripts/GlassHeroesBusiness.2.cs and VisualStudio/scripts/GlassHeroesBusiness.2.cs, VS project, full source diff, tests, transcripts, source hashes and this guide in MD/DOCX/PDF. READ_ME_FIRST.txt identifies the single installation source. No DLL, active business Save.ini, active Settings.ini, private dump, raw user crash log, old active business script or nested ZIP is shipped. Verify the separate manifest SHA256 before copying anything. PR #19 stays DRAFT; no merge to R2, development or main.

For review now, inspect the diff, validation.json and guide. If later approved for use, stop GTA, back up the existing complete business folder outside scripts without editing it, then replace only the business script with Install/scripts/GlassHeroesBusiness.2.cs. Keep saves/settings intact. Do not copy the Visual Studio development source into scripts alongside the install source. Do not install PR18 and PR19 together or combine their code without separate reviewed integration and revalidation.

Rollback while GTA is stopped: restore the exact previously accepted QA R2 business source. This feature adds no saved keys to remove, requires no save downgrade and leaves no new setting behind. Restart after replacement; ordinary abort should remove its icon before unload. Preserve crash evidence privately if any test fails. No installation, rollback or real game test has been executed by Codex.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Suite #2: 10/10 behavioral tests PASS; Suite #3: 14/14 behavioral tests PASS; Suite #5: 8/8 behavioral tests PASS; Suite #6: 8/8 behavioral tests PASS; Suite #4: 7/7 behavioral tests PASS; Suite #7: 7/7 behavioral tests PASS; Suite #19: 27/27 behavioral tests PASS.

PR #9 diagnostic logger tests: 9/9 PASS; total behavioral tests: 90/90 PASS.

Install and Visual Studio sources have equal SHA256: 17e15dec2b6fe40e772dd0b3cc8dda31168fd792c51ab18b104148b87270fc7d.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
