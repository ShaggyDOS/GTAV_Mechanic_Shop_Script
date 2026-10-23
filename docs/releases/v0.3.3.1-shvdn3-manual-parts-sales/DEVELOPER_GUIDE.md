# v0.3.3.1 SHVDN3 manual parts sales — EXPERIMENTAL

Issue #25; separate branch feature/shvdn3-25-manual-parts-sales, explicitly stacked on repair coverage 2532afe (#27), itself based on PR #21. October 22, 2026 (America/Chicago). This stack permits review of the shared reservation/cost helpers before retail; it is not a merge or approval of the pricing branch. Manager supplier deliveries, pallet layout and wage-menu changes remain separate.

## Behavior and finance

Business Management → Inventory & Suppliers → Parts Sales has four explicit one-unit actions: Universal, Service Filter, Brake Kit and Diagnostic Supply. Each row previews available/unreserved quantity out of total stock, fixed estimated unit price and availability. One fresh confirm sells one unit from the selected bucket. Existing keyboard/controller debouncing remains intact, with an additional Game.GameTime same-frame transaction guard. Merely opening or drawing the menu never sells stock.

Prices use the exact #27 acquisition estimate without employee skill, specialty, manager or receptionist retail markup: default $125/$113/$200/$157, with optional [Pricing] UnitCostEstimate* overrides described in the repair guide. This is a fixed estimate, not a claimed historical purchase lot valuation. No passive customers, NPCs, missions, remote income or timers were added. Explicit sales work while the shop is closed and away from the shop, but only after gameplay activation and while GTA is unpaused. Pending/incoming/cargo quantities are not eligible.

UnreservedRepairStock protects dedicated-first/fallback allocations for accepted physical, background and remote repairs. Unassigned customer requests do not reserve stock, and a paid returning car does not reserve again. Selling excess stock leaves those exact accepted sources intact. A successful transaction decrements one selected unit and credits Cash, Revenue, PartsSalesRevenue and PartsSalesUnitsSold once. Customer repair revenue, job counts, supplier invoices, procurement expenses, outstanding wages and payroll spending are untouched. Historic earned delivery-box visuals/masks are not removed or reinterpreted by a sale.

## Atomic persistence and compatibility

New additive Save.ini [Business] PartsSalesRevenue and PartsSalesUnitsSold are long values defaulting to zero for old saves. No existing key or value expression was removed. The source's Save method now returns a success flag while existing callers remain valid. It loads a same-directory copy of the old INI, writes the original entire snapshot plus these two keys, and atomically replaces the target file only after ScriptSettings.Save succeeds. Unknown keys survive that copy/load/write cycle. A new save is committed by a same-directory move. There is no non-atomic destructive fallback. The unique owned temp is cleaned on failure.

TrySellPart takes a RAM snapshot, applies the unit/cash/revenue transaction once and calls Save. If the durable commit fails, it restores stock and every changed sales amount and reports cancellation, with no success notice or audit row. Monetary bounds are checked before mutation. Crash before replacement leaves the prior whole snapshot; crash after replacement leaves the completed whole snapshot. Disk writes occur only at existing save sites and explicit manual actions, not during row rendering or per-frame placement.

PartsSales.v1.csv is a new six-column audit (UTC, Kind, Units, UnitEstimate, Total, GameTimeMs), appended after commit; audit failure logs an error but does not repeat a committed sale. Existing customer/remote CSV formats are unchanged. Actual Windows ScriptSettings/NTFS execution remains NOT RUN; the production atomic helper was fault-tested on synthetic temporary files. The pre-existing customer-object cancellation-on-reload limitation documented under #27 is unchanged.

## Exact changes and before/after

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs changes executable product code. MenuPage, NavigateBack, GetMenuItems, GetMenuItemValue, GetSelectedDescription, GetMenuTitle, ExecuteSelection and ExecuteInventorySelection connect the new page. PartsSalePreview and TrySellPart implement the explicit transaction. Constructor loads the two optional additive counters; ShowDetailedInventory reports retail separately. Save and TryCommitBusinessSave implement the reviewed atomic snapshot boundary. The full method boundary is in preservation.json, full unified source.diff is relative to 2532afe.

```csharp
// Before: no manual sale action; saves wrote directly to Save.ini.
ScriptSettings data = ScriptSettings.Load(SaveFile);
data.Save();
// After: selected stocked unit, separately tracked sales, commit or rollback.
if (kind < 0) _parts--; else _specialStock[kind]--;
_cash += price; _revenue += price; _partsSalesRevenue += price;
if (!Save()) { /* restore the captured stock and money */ return false; }
```

Source.diff provides exact surrounding checks and the complete snapshot implementation. OnTick, all repair helpers, pending delivery costs, supplier receipt formulas, stored pallet coordinates and original v2 files remain byte-for-byte unchanged relative to the parent.

## Review, installation and rollback

The ZIP is a review-only source package, containing one .3.cs and the separate net48 project linking it. Never install the whole archive. Only after owner authorization, back up the full business data folder and disable every duplicate/v2 Business Manager source or DLL before copying this one experimental .3.cs. Keep installed SHVDN runtime files, Save.ini and Settings.ini intact. No save/settings/runtime DLL is bundled or modified by the agent.

Manual GTA regression (all NOT RUN): cold launch and reload, all four manual actions with held Enter/controller A and mixed input, sold-out and last-reserved stock, brake fallback reservations, background and manager remote jobs in progress, pending local/express/wholesale shipment and loaded player cargo. Confirm invoice costs stay owed after a sale and paid repair cars do not reserve twice. Pause/unpause and test closed-shop/offsite manual sales: paused/closed-game elapsed time must never sell or progress work. Save/reload after one sale and verify exactly one stock decrement and one cash/revenue/counter increment. Test an isolated unwritable test-data target to verify visible cancellation and no ledger change. Stream out/in and check earned delivery boxes remain unchanged in this PR. Repeat in the heavy-mod environment after no-GH control, preserving any crash evidence without attribution.

Rollback by disabling this experimental source and restoring the repair-only source/project, never enabling both. Preserve financial saves; the two additive retail counters are ignored by older builds, while Cash/Revenue/stock already reflect committed sales. Restoring an earlier save discards subsequent transactions and must only be deliberate on an isolated test copy. No merge into QA R2 or main and no live installation changes.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

146/146 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game OnTick clock sampling and original v2 source/project are unchanged. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 72b054679ecaf20c712812759b9af2268060fb6f59141641b774d05f6f18c87d. The complete unified source.diff applies to parent 2532afe and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
