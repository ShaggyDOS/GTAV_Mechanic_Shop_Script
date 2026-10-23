# v0.3.3.1 SHVDN3 Manager-Assisted Supplier Deliveries — EXPERIMENTAL

Issue #24 / PR #26 implementation scope. New isolated branch: feature/shvdn3-24-manager-supplier-deliveries. Parent aad2dff55866efc2a17dadcfbf4738b6931b6992 from experimental/shvdn3-economy-pallets-wages-review. No merge, reset, live installation or save/settings modification. The earlier scope-only PR is not claimed updated. This candidate supersedes the prior guide only for supplier receipt and route timing; all prior features remain included.

## Implemented behavior

LOCAL (supplier 1) and EXPRESS (2) retain their existing active-game ETA. An employed manager, OPEN business and placed shop permit automatic receipt when a valid player is farther from the shop than Management/AwayDistanceMeters (existing default 200m, configured clamp 90–700m). No driver, truck or package is spawned for virtual receipt. A valid owned pallet with fewer than nine recorded crates and enough aggregate parts capacity is required. First eligible owned pallet is selected. Capacity is 25 parts per placed pallet, shared across all stock types, independent of nine physical crate slots. On successful virtual receipt, provenance is saved and its bounded visual budget resets for reconstruction when the player returns.

Near the shop, manager deliveries use the existing physical Mule route, truck parking, driver exit/rear unload and configured walking waypoints. The final destination becomes the selected storage pallet, bypassing player chase. Receipt requires the driver within 2m, a live driver/crate, a grounded pallet prop and safe box layout. Stock, supplier invoice, completion count, package mask/count and cleared order are saved together before crate cleanup or departure. Pallet visuals reconstruct the deposited parcel from the durable provenance. The player does not need to collect it. Explicit player collection remains available if they choose to intercept; an already player-carried package stays on its manual deposit path.

WHOLESALE (0), including legacy prepaid orders, cannot enter manager simulation or automatic delivery fallback. It retains warehouse pickup, assigned vehicle, transportation, unloading and player pallet stocking. A missing warehouse point retains the order. Prepaid receipts never add a second invoice and now obey capacity checks like other receipts.

No manager or CLOSED/unplaced business disables manager receipt. Existing direct player handoff remains. Near a manager-operated shop, disabled physical Mule mode or incomplete route retains the shipment; configure the physical delivery route or travel beyond the away boundary for virtual receipt. Supplier status shows manager virtual receipt, manager pallet drop or player handoff and pallet/capacity waiting. Check Delivery Settings if physical receipt waits with free storage.

## Timing, recovery and performance

OnTick's existing Game.GameTime sample, pause gate, 30-second jump rejection and two-second cap are unchanged. A session-only supplier route clock advances by exactly activeGameSeconds; ProcessSupplierDelivery consumes it instead of UTC. All route stages, departure timeouts, player approach deadlines and new spawn/save retry budgets therefore freeze during pause and never accrue from file age, UTC or a closed game. Manual supplier action timestamps use this same route clock. Paused manual package interaction returns without mutation. Reload starts a new route clock and rebuilds visuals from the saved pending order/remaining ETA; it does not resume entity handles. Existing 15-active-second ETA checkpoints remain: an abrupt crash can repeat unsaved ETA, never catch up offline.

Spawn/model/nearby-vehicle checks occur at most once per ten active seconds, within the existing 80m unloading-bay spawn gate. Manager-away receipt does no world scan. Routes run in the existing roughly one-second visual refresh, with bounded stage deadlines and fixed-size pallet/waypoint loops. Missing/dead entities, obstructed manager walking routes, invalid grounded layout, capacity loss and manager removal defer the original order with a 30-active-second retry. Manager walking recovery does not teleport a parcel through collision. Rear box creation has a 45-active-second attempt window. Exceptions back off instead of restarting on the next frame. Existing owned-entity cleanup remains separate from accounting.

Crossing near-to-away releases only GH-owned supplier visuals and settles the same eligible pending LOCAL/EXPRESS order virtually. Away-to-near cannot respawn a committed shipment. A player-carried or wholesale vehicle-owned package is never independently credited by the manager. A manager hired during a manual driver approach reroutes that driver to the pallet; removal or clock-out during a manager route defers to a fresh policy evaluation.

## Transaction and save compatibility

Save.ini and Settings.ini use the same scripts/GlassHeroesBusiness paths. No save/config keys, defaults or serialization expressions are added, removed or changed. New fields (supplier route clock and selected pallet) are session-only. Existing DeliveryDueUtcTicks is an identity, never elapsed time. Physical manager receipt revalidates that identity before settlement.

CommitSupplierDelivery now snapshots and restores every affected memory field if Save returns false: order kind/quantity/cost/identity/prepayment/ETA, timer checkpoint, wholesale cargo flags, stock, invoices, completion count, crate count, mask and visual dirty state. Existing TryCommitBusinessSave copies unknown keys into a temporary INI, writes the full business snapshot and atomically replaces the original. Thus a failed write retains the old durable order; a successful write includes both stock/invoice and order clearance. Repeated tick/interaction sees no pending order and cannot credit again. A failure after durable commit during visual cleanup cannot recreate an invoice; saved provenance recreates the visual. Normal filesystem atomic replacement assumptions apply; power loss/storage corruption is not certified.

Accepted repair quotes, fixed costs/allocations, manual parts sales, manager wages, nine-box geometry, clock/startup guards and protected v2 sources/project remain preserved. The accepted-quote journal and ledger stamp still bind the full business snapshot. Older builds may ignore that journal; do not mix saves or delete keys to bypass consistency errors.

## Exact source boundary and before/after

Executable file: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs, linked directly by the separate v3 csproj. Changed methods: OnTick (route clock plumbing and supplier exception backoff); ShowSupplierDeliveryStatus (receipt policy); DeleteSupplierVisuals (clear session pallet); BeginSupplierDriverWalk (manager destination); CommitSupplierDelivery (identity/capacity and rollback); ProcessSupplierDelivery (policy, bounded retries and wholesale exclusion); TrySupplierPackageInteraction (pause gate and route timestamps); HandleSupplierPackageInteraction (interrupted player carry uses the same route clock). New helpers: ManagerSupplierEligible, SupplierPlayerAway, AvailableSupplierPallet and TryManagerSupplierDeposit. Full exact unified source.diff and method-level preservation.json accompany this report. Save/constructor/repair/wages/sales and v2 code remain unchanged.

```csharp
// BEFORE: terminal mutation was finalized without checking durability.
Save();
// AFTER: restore the complete pre-receipt state if atomic saving fails.
if (!Save()) { /* restore stock, bills, provenance and original order */ return false; }
// BEFORE: physical driver waited for player pickup at configured pallet.
_supplierDirectToPlayer = _supplierPartsPallet.IsSet;
// AFTER: eligible manager route ends at the first available owned pallet.
_managerSupplierPallet = ManagerSupplierEligible() ? AvailableSupplierPallet() : -1;
// BEFORE: supplier route deadlines were driven by UTC.
ProcessSupplierDelivery(now);
// AFTER: route elapsed time derives only from the existing unpaused game sample.
_supplierGameplayNow = _supplierGameplayNow.AddSeconds(activeGameSeconds);
ProcessSupplierDelivery(_supplierGameplayNow);
```

Excerpts illustrate the boundary; source.diff contains the exact rollback and integration code. Tests compile verbatim production members against deterministic world stubs, with actual temporary-file atomic snapshot tests. They do not simulate native GTA navigation, Windows ScriptSettings execution or the installed Enhanced runtime.

## Installation and rollback — owner testing only

1. Exit GTA and its loaders completely. Back up the current Business Manager source plus the complete scripts/GlassHeroesBusiness data directory together, including Save.ini and Settings.ini. Keep the baseline aad2dff source/package with its matching data backup.
2. Extract the review ZIP to a review folder OUTSIDE GTA. Do not copy the entire ZIP tree into the game. Review FILES_SHA256.json, package manifest and this report first.
3. Copy ONLY experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs from the archive into your GTA scripts/GlassHeroesBusiness.3.cs. Remove/disable any other active Glass Heroes Business .2.cs, duplicate .3.cs or compiled Business DLL from script discovery; preserve them outside the active scripts directory. Keep unrelated scripts untouched.
4. Do not overwrite Save.ini, Settings.ini, ScriptHookVDotNet runtime DLLs/ASI, gameconfig or other mods. This archive includes none of those live/runtime files. Use the installed SHVDN3 environment; compilation reference is official 3.6.0, while installed Enhanced 3.9.0.6 compatibility still needs owner testing.
5. Cold-start GTA. Confirm only GlassHeroesBusinessV3Experimental starts. Retain existing HoldGameplayTick=false and StartupDelaySeconds=30 unless deliberately testing startup hold. Open the business deliberately after each load; loading does not automatically open it.
6. For rollback, exit GTA. Restore the prior source AND its complete matching business-data backup as one unit. Do not combine a historical Save.ini with newer journal or financial files, or run two Business Managers. Never erase journal keys to force startup.

## Manual GTA acceptance matrix — NOT RUN

For each test, record starting/ending inventory, Supplier invoices due, completed delivery count, pending order and package count. Use LOCAL and EXPRESS separately, with enough aggregate storage and a free crate slot. Pay other business bills as normal so unpaid staff do not confound the test.

1. Cold launch with the existing save, hire/retain a manager, OPEN business, place owned pallet(s). Order LOCAL, leave beyond configured AwayDistanceMeters until ETA reaches zero. Verify one stock increment, one invoice and one crate after returning; no duplicate on later ticks/reload. Repeat EXPRESS and special stock. Check closed/no-manager/unplaced controls do not simulate receipt.
2. Near the shop, enable Physical Mule delivery and configure road spawn, unloading parking, handoff and walking stops in Inventory Management > Supplier Delivery Settings. Park player safely within 80m of unloading bay and away from truck spawn. Order LOCAL/EXPRESS. Observe truck park, driver unload/walk to pallet, automatic deposit and departure without pressing interact. Verify stock/invoice only after actual arrival. Intercept a separate shipment manually and confirm one ordinary carry/deposit.
3. Pause with ETA remaining and while the driver is en route for longer than its stage timeout. Unpause: no ETA catch-up, no route timeout caused by pause. Exit/relaunch with pending ETA and wait outside GTA: saved remaining ETA/order persists with no offline receipt. Reopen business intentionally; complete once. Reload after successful receipt and verify no second stock/invoice.
4. Fill aggregate parts capacity or all nine crate slots / unset a pallet. Confirm pending order remains without billing. Restore storage/placement, retry and verify one receipt. Test legacy prepaid order on a backed-up test save: capacity still required, no additional invoice.
5. Cross the away boundary during driving/walking, then return. Fire manager/close business during walking. Block route safely, delete a driver/vehicle with owner debugging tools, or use a missing model in a disposable test configuration. Verify bounded retry, preserved order and no duplicate entities/accounting. Remove obstacles/configure route and complete it.
6. WHOLESALE: with manager present and away, wait through ETA. Verify warehouse pickup only, no manager receipt. Register vehicle, collect/load, transport, unload and manually stock. Reload at each cargo stage. Repeat reload after stock; no duplicate stock/invoice.
7. Preserve/regress repair coverage #27, manual parts sales #25, manager wage payments, accepted quote save/reload and nine-box centered stacking. Stream away/return after virtual receipts; verify correct package provenance and no legacy floating display boxes.
8. Full heavy-mod session: cold starts, normal travel, repeated deliveries, menu/input parity and 60–120 minutes idle/operations. Capture exact deployed source hash and diagnostic logs for any fault. Stack overflow remains UNATTRIBUTED; this change is not claimed to fix it.

## Reproduce automated checks

With .NET SDK 8 and Python dependencies python-docx/reportlab/pypdf available, from repository root:

```text
python verification/economy/verify_candidate.py --label manager-deliveries --profile supplier --suites repair sales pallets wages reload supplier
python verification/economy/package_candidate.py manager-deliveries --parent aad2dff55866efc2a17dadcfbf4738b6931b6992
```

The package contains source, linked net48/C#7.3 project, verification scripts/baselines, current guide Word/PDF, historical feature guides and actual logs. Clean extraction is rebuilt independently and file hashes verified. Review status remains pending. GTA/Windows tests are NOT RUN; native pathing, collision/MLO support and actual runtime binding remain limitations.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

271/271 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 25, wages: 22, reload: 41, supplier: 37). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game clock sampling and original v2 source/project are unchanged. Supplier route timing consumes that existing active-game sample. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 36df493478adc3e52ddd2043af1036e486662ddeebb2370401daddf6f2abc56f. The complete unified source.diff applies to parent aad2dff55866efc2a17dadcfbf4738b6931b6992 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
