# Glass Heroes Business Manager v0.3.3.2 SHVDN3 Supplier Workflow — EXPERIMENTAL

October 23, 2026 (America/Chicago). Continuation of #24 manager deliveries and #25 Parts Sales QA, baseline b777c64da7fc64241a2ac175b4013ff40cc864ec. Four focused commits/branches form a linear review stack; no branch was merged. The installable candidate is the tip of feature/shvdn3-native-pause-compatibility and includes all four priorities. Do not install intermediate branches together or merge overlapping whole-source commits independently.

## Implemented versus deferred

Implemented: shared physical/virtual shop-distance boundary; automatic nearby driver-to-pallet receipt retained; six inventory/supplier destinations; measured nonoverlapping row columns and compact Parts Sales prices; four-item adjustable cart with preview, one supplier, ETA and invoice; safe additive line persistence and atomic placement/receipt/cancellation; native Escape/Pause/Start compatibility; low-stock draft fill and clearer transaction feedback. Deferred: persistent supplier-order history and automatic unattended reordering. Low-stock fill only prepares a draft and never purchases by itself.

Preserved: repair cost coverage #27, manual parts sales #25 and repair-reserved stock, nine-box centered stacking, manager wage payments, fixed accepted repair quotes/allocations across reload, settings and original v2 source/project. Manager LOCAL/EXPRESS receipt remains virtual while open/away, physical near the shop, and never takes an already player-carried parcel. WHOLESALE remains player-only including prepaid legacy orders, registered vehicle, warehouse collection, transportation, unloading and stocking. No invoices are created at ordering or physical arrival alone.

Owner QA evidence applies to the earlier manager-deliveries candidate: offsite receipt and moving-away transition work, nearby spawn too restrictive, Parts Sales crowds the menu edge, latest reported session smooth/crash-free. Exact deployed hash/session duration were not provided. This is useful reported gameplay evidence, not certification of this new source or crash attribution. Crash cause remains UNATTRIBUTED / UNPROVEN.

## Priority 1 — nearby delivery range

StartMuleDelivery previously required the player within 80m of the saved unloading position; virtual manager receipt started outside the configured shop away radius (default 200m, clamp 90–700m). The gap could wait indefinitely. New SupplierPhysicalRange is shared by StartMuleDelivery and SupplierPlayerAway, measured from shop location, inclusive for physical and strictly outside for virtual. There is no separate intermediate distance band. Models are requested only after that eligibility gate. The player need not approach the truck spawn. A correctly configured road spawn and unloading route near the shop are still required.

Road occupancy, player-within-5m spawn collision exclusion, async models, ten-active-second spawn retry and bounded driver stages remain. Unsafe/occupied road spawn, disabled physical mode or invalid route can defer without losing the order; this is shown through settings/status and is distinct from distance eligibility. No collision bypass, world-wide scan or extra per-frame entity loop was added. Moving away releases GH-owned supplier visuals and settles the same pending order virtually if eligible, with one stock/invoice transaction. Physical arrival still needs a live driver/crate, actual grounded pallet layout, free parcel slot and aggregate storage capacity.

```csharp
// Before: unrelated parking gate could exclude a nearby shop player.
DistanceSquared(player.Position, _supplierTruckPark.Position) <= 6400f;
// After: same shop boundary selects complementary physical/virtual policies.
SupplierPhysicalRange(DistanceSquared(player.Position, _location), _remoteDistanceMeters);
```

## Priority 2 — inventory menu workflow and fitting

Inventory & Suppliers is a hub for Inventory Overview, Build Supplier Order, Active Deliveries, Parts Sales, Stockroom & Pallets and Supplier Settings. Each has its own title, values, descriptions and Back destination. Overview retains stock totals, shared capacity, reorder advice and low-stock threshold cycling. Active Deliveries exposes status, exact line contents, recovery and two-press cancellation. Recovery keeps the same order. Stockroom/route setup preserves all prior placement and vehicle controls.

DrawMenu uses LabelColumn/FitMenuText with measured left/right budgets of 255/125 design pixels and an 11-pixel gap, scaled with the existing 0.75 layout. Values align against the inner right edge. Long strings truncate with an ellipsis; exact quantities/prices are available in preview/contents and selected help. A bounded 128-entry text-fit cache prevents repeated native width measurements every frame. Parts Sales rows display compact fixed prices; selected descriptions retain unreserved/total stock, cost estimate and one-unit confirmation semantics. Native GTA rendering/resolution appearance still requires owner QA.

```csharp
// Before: right value started near the edge with no width constraint.
Label(GetMenuItemValue(i), left + 329, rowY + 3, 0.31f, textColor);
// After: measured, right-aligned within a separate bounded column.
LabelColumn(GetMenuItemValue(i), left + width - 14, rowY + 3, 125, 0.31f, textColor, true);
```

## Priority 3 — one mixed-item supplier purchase order

Build Supplier Order: highlight Universal Parts, Service Filters, Brake Kits or Diagnostic Supplies. LEFT removes one; RIGHT or ENTER adds one; controller D-pad matches. Each quantity clamps 0–100; one submitted order totals 1–100 units and must fit actual aggregate storage plus a free parcel slot. Up/down changes row. Choose one supplier, inspect Cost / Capacity Preview, then explicitly Place Purchase Order. An over-limit draft shows OVER LIMIT instead of a misleading zero-dollar preview. Fill Low-stock Cart uses existing thresholds to prepare quantities only; Clear Cart changes no saved order or stock. Unsubmitted draft quantities are session-only and disappear on reload, as described in the menu.

TryPlaceSupplierOrder clones valid line counts and calculates the sum of existing per-line supplier/manager-adjusted prices. It saves one order identity, supplier, total, ETA and line distribution before clearing old owned visuals or setting optional wholesale GPS. On save failure it restores all order fields and manager savings and leaves the draft available. Existing quick universal orders call this same safe submit path. Changing preferred supplier does not change an already submitted order's supplier or fixed total cost.

One parcel represents the combined shipment. CommitSupplierDelivery applies all four inventory buckets through ApplySupplierInventory, one supplier invoice, one completion and one package provenance bit, and clears pending lines/order in the same existing atomic snapshot. Failed persistence restores every bucket, invoice, package count/mask and the original immutable line array; retries cannot partially receive a cart. Transactional cancellation saves refund/order clearance before entity cleanup and restores the pending cart/refund balance if saving fails. Repeated cancel/receipt sees no pending order and cannot duplicate stock, invoice or prepaid refund. A visual failure after durable receipt cannot recreate accounting; saved provenance rebuilds the parcel.

```csharp
// Before: only one kind received quantity.
if (kind < 0) _parts += quantity; else _specialStock[kind] += quantity;
// After: legacy single item or all four saved lines, in one save/rollback boundary.
ApplySupplierInventory(kind, quantity, oldLines, 1);
if (!Save()) { ApplySupplierInventory(kind, quantity, oldLines, -1); /* restore pending snapshot */ }
```

Additive Save.ini schema: [SupplierOrderV1] Version=1; Line0..Line3 = Universal/Filters/Brakes/Diagnostics; OrderTicks, Cost and Supplier bind payload to the legacy pending-order fields. Absent Version=0 or an empty cleared payload retains old pending single-item orders, including prepaid. DeliveryKind stays the first nonzero item's legacy kind and DeliveryQuantity is aggregate units. The loader rejects unsupported versions, negative/oversized counts, sum mismatch, wrong identity/cost/supplier or first-kind mismatch before Tick registration or any save. Preserve the original file on a consistency error and request review/restore matching source/data; never delete keys to bypass checks. Completed/cancelled snapshots write zero lines/identity.

Existing save values, unknown keys, Settings.ini keys and filesystem paths are retained. WriteSupplierOrderLines is added inside the existing atomic Save snapshot; LoadSupplierOrderLines runs before callback registration and repair-journal loading. TryCommitBusinessSave remains unchanged: write a copied temporary INI preserving unknown keys and atomically replace the original. Ordinary filesystem atomic-replacement assumptions apply; power-loss/media-corruption behavior is not certified. Actual Windows ScriptSettings execution remains owner QA.

Do not run older source against a pending mixed order: it cannot interpret the distribution. Complete/cancel in this candidate first, or restore the complete matching prior source/data backup with GTA closed. Accepted repair journals still use their consistency stamp and must stay with the corresponding finances/inventory. An older build that changes a pending cart leaves stale metadata that this loader rejects instead of replaying it.

## Priority 4 — native GTA pause

Escape hides the overlay and resets held-key/menu arbitration without performing Back or confirming anything. Backspace/controller Back remain submenu navigation. YieldMenuToNativePause runs before custom focus/polling; Game.IsPaused, FrontendPause and FrontendPauseAlternate hide the overlay and suppress custom accept/open actions for that tick. Both native pause actions are explicitly enabled in groups 0 and 2 while controller focus is active. Hiding stops the overlay's per-frame suppression. GTA consumes the original native input; no forced pause toggle or key injection is used. Paused F2 cannot reopen the overlay; deliberately reopen after resuming. Cart arrow actions join semantic keyboard/controller echo protection.

```csharp
// Before: Esc navigated Back and controller focus disabled every gameplay action.
if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Back) NavigateBack();
// After: Escape yields; only Backspace navigates the custom menu.
if (e.KeyCode == Keys.Escape || Game.IsPaused) { HideBusinessMenuForPause(); return; }
_nativePauseYielded = YieldMenuToNativePause(); // before custom focus and controller polling
```

The active-game Game.GameTime sample, pause gating, loading-jump rejection, supplier route clock, wages, remote jobs and 15-active-second ETA persistence are unchanged. No delivery or economy catch-up uses UTC/file age/offline time. Actual pause display with your mappings/runtime requires GTA testing; deterministic tests prove custom yielding/control-enable behavior, not engine execution.

## Exact source boundary and verification

Executable source: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs; net48/C#7.3 project links that exact one file and pins official ScriptHookVDotNet3 3.6.0. Focused 01/02/03/04/05-source.diff and stage guides/build logs cover each isolated commit. Full source.diff reconstructs this candidate from b777c64. preservation.json records the reviewed method change list; unchanged Save payload and constructor behavior are enforced except for the one cart serializer/loader call, and original v2 hashes/repair/sales/wage methods are protected. Input tests deliberately move the old Escape-as-Back expectation to Backspace and add the new native pause contract without weakening accept/repeat guards.

The actual tests compile verbatim production helpers against deterministic world/INI/control stubs; atomic file tests use temporary files. The regression run includes all 271 manager-deliveries baseline cases, range coverage, menu-fit/cache/navigation, cart load/save/receipt/cancellation faults, native pause yielding and controller-focus controls. Build, static checks, scope hashes, exact counts and source SHA are recorded below and in validation.json. No live business data is bundled or read. GTA/Windows execution of this candidate is NOT RUN.

## Installation and rollback — owner QA only

1. Exit GTA and all loaders. Back up your current .3.cs and complete scripts/GlassHeroesBusiness directory together (Save.ini, Settings.ini, journals/logs). Retain the b777c64 source/package and its matching data backup. Do not test on your sole copy of business data.
2. Extract the review ZIP outside GTA. Read this current guide/DOCX/PDF and FILES_SHA256.json/package manifest. Do not copy the whole ZIP tree into the game.
3. Copy ONLY experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs from the ZIP to GTA's scripts/GlassHeroesBusiness.3.cs. Enable only one Glass Heroes Business source/DLL; move duplicate .2.cs/.3.cs/business DLLs outside script discovery. Keep unrelated mods intact.
4. Preserve your existing Save.ini, Settings.ini and SHVDN ASI/DLLs; none are supplied. Official compile reference is SHVDN3 3.6.0. Installed Enhanced runtime compatibility remains unverified for this new candidate.
5. Cold-start, confirm one GlassHeroesBusinessV3Experimental instance, retain existing startup delay/hold settings and deliberately OPEN the business after load. Hire/retain manager, place owned pallet(s), leave enough total storage and one crate slot.
6. For rollback, exit GTA and restore the prior source PLUS complete matching data backup as one unit. Do not mix financial/cart/repair snapshots. Finish/cancel any pending mixed order before switching to older source unless restoring the full older backup. Never delete schema keys to bypass errors.

## Manual GTA test plan — NOT RUN for v0.3.3.2

Record inventory by four types, supplier bills, completed deliveries, pending contents, cash and package counts before/after each test, plus exact deployed SHA and runtime logs.

- Range: with manager/open shop, valid physical Mule route and empty capacity, place LOCAL/EXPRESS orders at approximately 80/100/150/199/200/201m from shop (use your configured boundary if changed). Physical route must start inside/boundary without walking toward spawn; virtual receipt must occur only outside. Move away during drive/walk and return; expect one receipt. Test occupied spawn separately: it should defer safely, retaining the order.
- Menus: keyboard/controller navigate every hub destination and Back row. Inspect 720p, 1080p and ultrawide, long prices/stocks and Parts Sales descriptions. Opening pages/previewing/cart edits must not move money or stock. Sell one unreserved part and verify fixed price; reserved repair stock remains unsellable.
- Mixed orders: choose all four quantities, cycle supplier, inspect sum/capacity, submit once. Test nearby physical and offsite manager LOCAL/EXPRESS receipt, reload before ETA/arrival/receipt and reload after success. Each line increments once, one combined invoice/parcel/completion. Full capacity/full crate slots block whole receipt, then retry once storage is restored. Cancel twice and verify no inventory; legacy prepaid cancellation refunds once. Failed save should retain order and balances.
- Legacy/wholesale: load an existing single-item prepaid and unpaid order from matching backup. Complete without content drift or extra payment. Mixed WHOLESALE must never manager-receive; collect at warehouse in assigned vehicle, transport, unload and manually stock. Reload at each cargo stage, then after completion; no duplicated inventory/invoice.
- Interruptions: fire manager/close shop during route, stream out/in, block path, test missing model/driver/vehicle in a disposable QA setup. Order remains recoverable and retries bounded. Existing Recover keeps all four lines; cancellation cannot create stock.
- Pause: on foot and driving, open each custom page and press Esc/controller Start. GTA native pause must display, custom overlay/control focus must disappear, no accidental purchase/sale or Back action. Wait past ETA/wage/route deadline; resume with zero paused catch-up. F2 while paused must not reopen; resume and reopen deliberately, then test Enter/Back/left-right/input repeats.
- Off-session: save pending ETA/cart/accepted repair, exit, wait and cold-launch. No offline progress. Reopen business and finish once. Regress repair coverage, fixed accepted quotes, manual sales, manager wages and nine-box layouts. Run normal travel/idle for 60–120 minutes with the full heavy-mod stack and retain diagnostic logs; no stability or crash-fix claim is made.

## Reproduce automated verification and package

Use .NET SDK 8, Python and python-docx/reportlab/pypdf; restore pinned packages normally. No live installation is needed.

```text
python verification/workflow/verify.py --label supplier-workflow --profile workflow --suites repair sales pallets wages reload supplier range menu cart pause focus
python verification/workflow/package.py supplier-workflow --parent b777c64da7fc64241a2ac175b4013ff40cc864ec
```

The source ZIP includes the linked project, current/focused guides and Word/PDF, exact source diffs, tests, hashes and historical feature evidence. All file hashes and a clean extracted Release rebuild are verified. API limitations and comparison links are recorded in REVIEW_HANDOFF.md and PR_BODIES.md. No main/QA R2 merge or live deployment is authorized or performed.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

373/373 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 25, wages: 22, reload: 41, supplier: 37, range: 40, menu: 13, cart: 32, pause: 9, focus: 8). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game clock sampling and original v2 source/project are unchanged. Supplier route timing consumes that existing active-game sample. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: de2b07413870de4e61befc517682fbc3d03afea8df5f80a5c45a583e65ee5194. The complete unified source.diff applies to parent b777c64da7fc64241a2ac175b4013ff40cc864ec and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
