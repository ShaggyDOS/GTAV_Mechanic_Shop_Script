# v0.3.3.1 issue #3 — ready warehouse cargo — EXPERIMENTAL

Issue #3. Branch fix/3-warehouse-ready-cargo to development/v0.3.3.1. Review PENDING. GTA testing NOT RUN. Stack-overflow cause UNPROVEN; existing diagnostic PR #9 is separate.

## Changelog, exact methods, and before/after

VisualStudio/scripts/GlassHeroesBusiness.2.cs and matching Install/scripts/GlassHeroesBusiness.2.cs: ProcessSupplierDelivery now delegates ready pickup visuals to UpdateWholesalePalletVisual without looking up an assigned vehicle. That method owns the pair, streams them within 80m when ETA is ready, uses ground placement for the pallet and WarehousePackagePosition on its resolved support, avoids duplicate live props, and limits retries to five per streaming entry with three seconds between attempts. Stream-out and DeleteSupplierVisuals/recovery reset the budget. Exhaustion requires re-entry or recovery; it never discards an order. Individual model requests retain the existing 250ms bound and fallback pallet model.

Before:
```csharp
if (playerNearWarehouse && FindAssignedWholesaleVehicle(warehouse, 24f) != null)
    SpawnWarehouseBox();
```
After:
```csharp
_supplierVisualStage = 7;
UpdateWholesalePalletVisual(); // readiness and player range only
```
TrySupplierPackageInteraction permits collecting the ready box while the vehicle is away. Actual LOAD still requires FindAssignedWholesaleVehicle near the player and the existing assigned-vehicle plate/model checks. HandleSupplierPackageInteraction now offers a reliable vehicle-independent pickup hint and explicitly explains assigned-vehicle loading. DrawWholesalePackageMarker checks readiness and player distance independently of vehicles. Loaded, carried and Express delivery crates are protected from warehouse stream-out cleanup. The full exact source.diff is included; excerpts above explain the changed gate, not literal legacy helper names.

## Save/config compatibility, install and rollback

No Save.ini or Settings.ini key/default/schema changes; no assignment, ownership, money, stock, invoice, order identity or timer changes. CommitSupplierDelivery and its once-only ledger remain unchanged. Visual retry counts are session-only and reset safely. Live business files are never part of the setup or overwritten. Install only Install/scripts/GlassHeroesBusiness.2.cs, preserving all existing Save.ini/Settings.ini files. Example settings are reference only. Do not install multiple source copies, historical RC1 or a compiled business DLL alongside it. Exit GTA and restore the previous source for rollback; no migration/reset is required. v0.3.2.9a remains the last user-tested rollback.

## Automated scope and pending GTA regression

Extracted production UpdateWholesalePalletVisual, WarehousePackagePosition and DrawWholesalePackageMarker run with native/prop stubs. Tests exercise vehicle-independent spawning/marker, readiness, support height, one live pair, stream-out/re-entry, retry limits/cooldown, and carried/loaded/Express protection. Stubs cannot establish real model/collision behavior. Source diff preserves plate/model matching and stock/invoice operations; real compilation separately checks SHVDN2 API compatibility.

Repeat with activated SDK: python3 verification/run_issue_tests.py 3; python3 verification/run_static_checks.py /tmp/gh-static. Full build commands/results are recorded in build.txt.

GTA NOT RUN: three cold launches under boot guard; drive to ready pickup with assigned vehicle over 24m away/no nearby vehicle and verify grounded pair plus marker/hint; collect the box, reject incorrect vehicle, load correct assigned vehicle once; return to shop, unload/deposit once, verify stock/invoice; recover/cancel/reload/stream repeatedly with no duplicate charges or props; test ETA across closed GTA sessions; preserve Express parcels; repeat in LS RED/heavy-mod stack. Cold-start, >30m idle, shop-distance and save/reload matrix remains NOT RUN. No stability/crash fix claim and no immediate user test requested.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #3: 9/9 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: 061701234408f3428233827c36ffbe78abc04ef58dc6b54081f444933db8164a.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
