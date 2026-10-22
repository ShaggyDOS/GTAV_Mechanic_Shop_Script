# v0.3.3.1 issue #6 — hide legacy stock displays — EXPERIMENTAL

Issue #6; fix/6-hide-legacy-stock-boxes to development/v0.3.3.1. Review PENDING. GTA testing NOT RUN. Crash cause UNPROVEN. Diagnostic PR #9 is separate and unchanged. Warehouse correction #3 is independent.

## Changelog and exact methods

Matching VisualStudio/scripts/GlassHeroesBusiness.2.cs and Install/scripts/GlassHeroesBusiness.2.cs now separate obsolete _storedCrateProps ownership from indexed _deliveredPackageProps. DeleteLegacyStockroomDisplayBoxes removes only the obsolete Glass Heroes-owned list. It never touches active carrier/Express/warehouse crates or world/MLO entities. DeleteStoragePalletVisuals performs normal owned delivered-parcel cleanup on teardown or stream-out, preserving their saved reconstruction metadata.

UpdateSupplierPalletVisual stops synthesizing old floating decorative boxes from every historical BoxCount. It maintains the real pallet, recreates only slots with delivery provenance, and keeps already existing real delivered props when the display becomes dirty. Missing models use five attempts per stream entry and three-second spacing. TrySupplierPackageInteraction transfers the same newly deposited physical box into the delivered array. StoredBoxPosition retains its actual grounded support calculation.

Before:
```csharp
for (int b = 0; b < _storedDeliveryCrates[i]; b++)
    CreateDisplayBox();
```
After (actual filter in the same slot loop):
```csharp
if ((_deliveredPackageMasks[i] & (1 << b)) == 0) continue;
if (_deliveredPackageProps[index] != null && _deliveredPackageProps[index].Exists()) continue;
```
Complete literal source.diff accompanies this explanatory excerpt. Constructor and Save load/write additive visual provenance; CommitSupplierDelivery records the slot bit in the same existing save as a successful deposit. Financial accounting and capacity counts are preserved.

## Additive save migration and rollback

New OPTIONAL Save.ini keys: [Storage] DeliveredPackageMask1 through DeliveredPackageMask4. Default 0. Each low nine-bit mask identifies actual parcel slots delivered by this build. CompatiblePackageVisualMask clamps masks to existing BoxCount and nine slots. Existing BoxCount, stock, capacity, jobs, pending orders, money and invoices are NEVER removed, reset or converted. Settings.ini and its defaults are unchanged.

Old saves contain only BoxCount and cannot distinguish a legacy synthesized display from the source of an older delivery. Therefore historical untagged reconstruction is hidden by default; this suppresses the old stock-display visuals the user chose to remove. All historical inventory/capacity records remain. New deliveries in this build keep the original deposited physical prop and save its provenance so streaming/reload recreates it correctly. Active pending/Express/warehouse packages are unaffected. This does not infer provenance for parcels delivered under an earlier build. It is an explicit visual migration, not loss of stock. Downgrading to RC1 ignores the optional masks and restores RC1's historical display rendering; returning to this build filters them again. Older-version deliveries lack new provenance.

Install only Install/scripts/GlassHeroesBusiness.2.cs after backing up the previous source outside the game scripts folder. Preserve all existing Save.ini/Settings.ini. Example settings are reference only. Never activate both source copies, historical RC1 or a compiled business DLL alongside this source. Roll back after exiting GTA by restoring the previous source; do not reset saves or strip unknown keys. Last user-tested rollback is v0.3.2.9a diagnostic.

## Automated scope and pending GTA regression

Tests execute the production cleanup, renderer, grounded position, migration helper and CommitSupplierDelivery with game/native persistence stubbed. They check old-save defaults, invalid masks, no historical decorative reconstruction, protection of real parcel ownership, one stock/invoice commit, dirty refresh identity, stream/reload reconstruction and bounded failures. Real net48/SHVDN2 compilation is separately recorded. Stubbed persistence does not prove the game file I/O system.

Repeat activated-SDK checks: python3 verification/run_issue_tests.py 6; python3 verification/run_static_checks.py /tmp/gh-static. Exact compilation commands and results accompany the guide.

GTA NOT RUN: three cold starts/boot hold; existing stockroom legacy display absent without inventory/capacity loss; place a new Express parcel and ensure its live prop stays correctly grounded through the next refresh; reload/stream and verify only provenance-tagged parcels return; wholesale pickup package/pallet and carried/vehicle cargo remain intact; deposit once, reload, verify invoice/stock once; edit pallet position, recover/cancel, save/reload, test full capacity; repeat with LS RED/heavy mods and >30m idle. Global timer/off-session and lifecycle matrix remains NOT RUN. No screenshot is required and no immediate user test requested.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #6: 8/8 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: 801e14c3b75f9a304e1cb5ecf62eb27719aa852ab8e696c7940dd7776ca841cb.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
