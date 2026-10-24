> Historical initial-slice report. For current build/tests and completed scope see DEVELOPER_GUIDE.md and final-validation/validation.json. Its original NOT RUN statements describe the state when authored, not the current branch.

# Glass Heroes v0.3.4 — Implementation Review (Phase A)

**Status:** Source change committed, NOT compiled or GTA-tested. No RC package or merge.

**Dev branch:** `feature/shvdn3-340-inventory-procurement`  
**Parent:** v0.3.3.6 `c8617aed5cda492214bbbe1907042bcafbfa0dc1`  
**Parent source SHA256:** `7fff30978e2dc4c3235656f6ca6865506d5cea53387b7eb6d83654d71c8a9d6b`  
**Issue:** [#35](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/35)

## Evidence and scope

The owner explicitly authorized implementation while personally testing v0.3.3.6 RC1. The revised Codex planning proposal lives only at `/workspace/review-notes/v0.3.4-architecture/PREPRODUCTION_PROPOSAL_REV2.md`; it was **not accessible** through the GitHub connector or conversation files at this point. Codex should add that exact proposal to the repo and reconcile it against the owner roadmap **before modifying supplier receipt or manager simulation**. Do not attribute unreviewed design details to that document.

This initial source slice adds **read-only inventory visibility** through existing F2 > Business Management > Inventory & Suppliers > Inventory Overview actions. It intentionally reuses all eight existing row positions and does not change player controls, currency, stock counts, ordering or save keys.

### Modified production file

`experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs`

- `PendingSupplierInventory()`: derives incoming counts from an outstanding **mixed** SupplierOrderV1 lines array or the existing **legacy** single-kind order, treating `_deliveryKind == -2` as none. No incoming units are deposited into actual stock.
- `OnHandInventory()`: reads and clamps the four stock counters to nonnegative display quantities.
- `InventoryAvailabilityRow()`: formats on-hand, estimated reserved, actual available (from the existing reservation ledger), and incoming counts. Reserved display is `max(0, onHand-free)` by item.
- `ShowInventoryCategoryDetail(int)`: displays a focused on-demand snapshot, estimated (not exact historical) unit acquisition cost, shared capacity and supplier liability.
- `ShowDetailedInventory()`: four-category on-demand snapshot.
- `MenuPage.InventoryOverview` execution now distinguishes category indices 0–3, capacity summary index 4, reorder advice 5, threshold 6, Back 7. Names, values, action order and item count are unchanged.
- Inventory context help clarifies pending cargo is **not usable** and selecting a row does **not** purchase/sell.

### Non-goals and compatibility

- No `Save.ini` or `Settings.ini` schema changes; no new fields to serialize.
- No supplier/wholesale, delivery, manager offsite, manager physical receipt, invoice or finance settlement changes.
- No new parts categories or physical car damage/repair code (deferred to v0.3.5).
- No extra world scans, GTA natives or native writes. The existing `UnreservedRepairStock(null)` reservation traversal runs **only on selecting** inventory detail, not on Tick. Existing menu value rendering stays unchanged.

## Required next verification

**NOT RUN**: .NET Framework 4.8 SHVDN3 Release compilation, inherited 762 behavior/21 static regressions, in-game GTA acceptance. This branch has **not** been installed, used with a real Save.ini, or approved as a test release.

Before RC review:
1. Publish Codex's REV2 proposal to a GitHub document and compare line by line; present owner any new pricing/category/receipt-policy decisions.
2. Run `python verification/inventory340/verify.py` for focused structural checks.
3. Run official existing SHVDN3 net48 build, 762 inherited tests, controller navigation and finance/preservation checks.
4. Add genuine behavior tests exercising actual source methods through the existing adapter/harness (legacy and mixed order; no order; reserved/unreserved four categories).
5. Owner QA: compare each item with current Save.ini, active orders, accepted physical/remote repair reservations, delivered-but-not-deposited cargo, open warehouse pickup, and Parts Sales. Verify browsing/clicking never changes stock, wallet, business cash, supplier invoices or delivery identity.
6. Preserve a matching complete v0.3.3.6 rollback copy; **no RC1 release/installation/main merge** without separate owner approval.

## Proposed follow-on commits (not implemented)

- Phase B: supplier cart/active order clarity, independently reviewed.
- Phase C: attended Mule, wholesale and pallet visual/route consistency.
- Phase D: manager local/express unattended receipt with one guarded stock/invoice path. **Wholesale is always physically collected by the player.**
- Phase E: source-verified inventory preparation for future damaged-car job/part mappings, no physical restoration implementation.
- Regression, documentation, draft PR and owner GTA testing only after each gate.

This is an isolated development start, **not v0.3.4 feature completeness**.
