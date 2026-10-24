# Glass Heroes v0.3.4 — Inventory, Procurement & Stockroom Operations

## Review status and identities

Development candidate for PR review. EXPERIMENTAL — GTA TESTING NOT RUN. This is not an RC package, certified stable build, crash fix or installation approval.

Branch: `feature/shvdn3-340-inventory-procurement`.

Audited RC1 parent: `c8617aed5cda492214bbbe1907042bcafbfa0dc1` (owner RC1 runtime evaluation ongoing).

Source implementation commit: `1ded7ce2d0c35a7d9ddf9b0b652c37fbb7e53573`.

Final test-adapter commit: `050725963961a5b8a0a3468372dbd64b94002870`.

Source SHA256: `fbc8f7f963bbffa3e31c097bf34250fef5807d28dd5de3b7d75463146beb3251`.

Source: `experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs`.

Matching project: `experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj`. It still compiles the install source directly; no duplicate script copy was introduced.

The owner first required YES/NO gates, approved Phase 0, then explicitly authorized work through all phases until ready for PR. That later authorization permits this development sequence. Merge, RC packaging, tags, public release, installation and owner-data modification remain outside this work.

## Implemented behavior

### A: inventory visibility

Inventory Overview retains its eight rows and existing keyboard/controller route. Category details show on-hand, repair-reserved, free and incoming quantities; existing estimated unit acquisition cost; and reorder advice. A compact four-category snapshot labels its columns. Shared storage reports placed-pallet unit usage, free units, incoming/projected headroom, free historical parcel slots and a capacity hold reason.

Reservations still come from UnreservedRepairStock, including physical jobs, remote jobs, autonomous workers and restored accepted promises with shared-quote deduplication. Incoming never becomes usable stock. Cost labels are estimates, not historical lot valuation or cost of goods sold.

Advice uses free stock plus incoming against the existing threshold, with the existing universal target at least five. It does not edit the cart or place an order. The existing Fill Low-stock Cart action keeps its original behavior; this milestone does not silently rewrite purchasing policy to match the new advice.

All projections are on demand. Long display totals use wide arithmetic and negative legacy counts are clamped only in the display; saved stock is not changed by browsing.

### B: purchasing and order visibility

Build Supplier Order retains its four quantity controls and supplier cycling. Its comparison/preview action shows existing supplier lead times, requirements and current cart quote. Review Purchase Order opens a separate three-row page: details, confirm, Back.

Review snapshots quantities, supplier and quote without saving. Confirm requires unchanged supplier/quote and runs the existing capacity, one-pending-order and wholesale-vehicle checks. Back and failed placement retain the draft. Successful placement creates one pending mixed order, one ETA and an invoice only after stocking. No new prices, margins or expense formula.

Active Deliveries retains five rows. Its right column shows a short current state once ETA is ready. Requested notices include the full pending identity and explain holds. Order / Last Receipt shows current lines when pending, otherwise the durable last terminal record. Cancellation's second press is bound to the current order identity.

### C: receipt durability and physical recovery

Every CommitSupplierDelivery call supplies an expected identity. Zero is a valid legacy identity and no longer bypasses comparison. Player/physical manager cargo uses its captured identity; synchronous virtual/direct receipt passes the current pending identity. Wholesale cannot finalize before its manual unload checkpoint.

Before mutation, the receipt validates order form, bounded quantities, frozen cost, mixed total, arithmetic, unit/parcel capacity and selected pallet. It applies all lines, supplier liability, earned provenance and terminal outcome, clears pending state and saves one complete snapshot. Save failure restores every changed accounting/order/outcome field. Notifications, earned-prop rebuild and departure follow durable success; retry cannot re-post a cleared order.

Driver exit issues one ordinary leave task after four active seconds, one fallback after 25, and holds/retries the original order after 40 if still blocked. Rear unloading and driver box creation require a valid on-foot driver. The box is not granted simply because 16 seconds elapsed before reaching the rear; the existing 45-second rear-stage recovery remains. No occupied vehicle is teleported by this change.

Wholesale load/unload checkpoints and manual recovery now check save success and roll back flags before finalizing visual ownership. Pallet purchase rolls back cash/spend/ownership on failure. Existing reload policy remains: unloaded/carried wholesale cargo is recovered at the warehouse, retaining the original order; no free receipt or automatic manager collection.

### D: manager integration

Existing eligibility remains open shop + employed manager + placed shop, LOCAL/EXPRESS only. Nearby physical delivery and offsite virtual receipt share the same guarded commit. The existing distance predicate remains unchanged, so this milestone does not add a physical/virtual dead range. Player-carried cargo has priority and cannot be stolen by virtual receipt.

Manager pallet receipt rejects stale identity before native/pallet work. Live labels distinguish manager drop from player pickup. On-demand explanations cover ETA, player ownership, capacity, warehouse requirements, nearby physical configuration, driver exit/rear deadlines and streaming/save retry. No new manager-ped or wage gate; offsite simulation does not require an entity to stream.

### E: integration and future readiness

Tests combine receipt liabilities with actual FinanceV1 observation and payments. Remote sales remain a subset of customer revenue, not an extra amount to add again. A $5,000 remote sales increase without outflows increases cash by $5,000. Invoice accrual at receipt does not debit cash. Actual bill payment is a separate outflow.

The v0.3.5 readiness document maps existing and proposed damage parts and describes future diagnosis, immutable requirements, reservations and idempotent repair application. No new item category, damage generation, physical restoration or speculative catalog is implemented.

## Source map and before/after examples

Use method names in the pinned source; line numbers can shift across commits. Full unified diff is included as source.diff.

| Methods / data | Change |
|---|---|
| PendingSupplierInventory, OnHandInventory, InventoryAvailabilityRow | Existing Phase A projection retained/refined. |
| InventoryStorageRows, InventoryReorderSuggestion | Shared unit/parcel forecast and advice, no writes. |
| ShowInventoryCategoryDetail, ShowDetailedInventory, ShowReorderAdvice | Read-only notices and compact labels. |
| PrepareSupplierOrderReview, ShowSupplierPurchaseReview, ExecuteSupplierOrderReview | Frozen preview and separate explicit confirmation. |
| GetMenuItems/Value/Title, GetSelectedDescription, NavigateBack, ExecuteSelection | One new review page; existing row count/order otherwise retained. |
| SupplierLiveState, SupplierReceiptBlocker, MuleStageName | Current-state visibility and explanations, no new Tick monitor. |
| CommitSupplierDelivery, TryCancelSupplierOrder | Mandatory identity, checked posting, outcome in same transaction and rollback. |
| UpdateSupplierDriverExit, BeginSupplierRearUnload, GiveBoxToDriver | Bounded exit recovery and on-foot/rear validation. |
| TryCommitWholesaleCargo, TryResetSupplierCargo, TryBuyStoragePallet | Explicit checkpoint/purchase save-or-rollback. |
| SupplierOutcome; CaptureSupplierOutcome; LoadSupplierOutcome; WriteSupplierOutcome; ShowLastSupplierOutcome | Bounded optional last terminal record. |
| Save / constructor | One additive writer / pure metadata read plus new DEV label; existing payload/native startup sequence preserved. |

Before: optional identity treated zero as “skip validation.”

```csharp
CommitSupplierDelivery(int palletSlot = -1, long expectedOrderTicks = 0L)
// reject only when expectedOrderTicks != 0 and does not match
```

After:

```csharp
CommitSupplierDelivery(int palletSlot, long expectedOrderTicks)
// reject whenever expectedOrderTicks != _deliveryDueUtcTicks
```

Before: blocked exit submitted fallback then immediately began rear unloading. After: submit the fallback once, verify on-foot state on a subsequent active step, otherwise defer at the deadline. Rear proximity, not elapsed time alone, authorizes box creation.

Before: cargo flags / pallet purchase called Save without checking its result. After: helpers snapshot changed fields, save once, restore them on false, then let the caller finalize visual state.

## Receipt and finance state model

Cart -> Review -> durable Pending -> active ETA -> physical / eligible virtual / player wholesale cargo -> validated receipt -> one atomic save -> Received -> visual departure.

Blocked/missing entities -> held original order -> bounded retry. Failed receipt save -> original pending order and prior outcome restored. Cancel -> durable pending-clear + optional prepaid refund + last cancellation. Paid invoice -> separate actual cash outflow.

The saved receipt is authoritative for finalization; a truck, crate, menu notification or best-effort CSV is not evidence to post stock again. Supplier order IDs and FinanceV1 transaction IDs remain separate namespaces; no guessed foreign key is written.

## Persistence and backward compatibility

Only additive SupplierOutcomeV1 keys are introduced. Version 1 stores State (0 absent, 1 received, 2 cancelled), OrderTicks, Cost, Invoice, ActiveMs, Supplier, Pallet, Prepaid and Line0-Line3. Pallet defaults to -1 for no physical slot. Absent or invalid optional history means unknown history, not fabricated receipt. It does not invalidate a valid pending legacy order.

History stores one last terminal outcome, not an unbounded order archive. ActiveMs comes from the existing active-game clock; OrderTicks remains opaque identity and never drives ETA. Existing SupplierOrderV1, legacy Purchasing fields, FinanceV1, RepairPromisesV1, stock, settings, wages/progression and placement keys keep their meanings. No settings key is added.

The complete Save snapshot differs only by WriteSupplierOutcome; the established temporary-write/atomic-replacement helper is unchanged. Full source preservation checks enforce this and protect original v2 source/projects, finance/quote types and 361 unchanged methods.

A receipt accrues an unpaid supplier liability unless prepaid; it is not a cash payment. Prepaid cancellation refunds once; unpaid cancellation has no refund. New history does not invent previously completed order identities or acquisition lots.

## Bounds, clocks and crash safety

Keep one pending mixed purchase order, four categories, four pallets, nine historical parcel slots per pallet, one supplier truck/driver/in-flight box and existing model/visual retry bounds. No new models, worker threads, frame scans or per-Tick logging.

OnTick, active/unpaused time sampling, manager income, demand, wages and saved remaining ETA are unchanged. Offline and paused progress remain zero. Constructor changes are metadata reading and startup label only, with source SHA256 identification and gameplay gate preserved.

Clock and persistence assertions are automated evidence, not actual GTA execution. CLR stack overflow, native startup access violation and Read-only/EUP behavior remain separate investigations and UNATTRIBUTED. No crash fix or new stability claim.

## Actual verification

Release: net48/C# 7.3 against official SHVDN3 3.6.0, zero errors, inherited warnings CS0169 and CS0649.

862/862 behavior checks, including 184 production navigation checks. 21/21 static checks and 13/13 inventory structural checks, counted separately. New behavior suites: inventory 30, procurement review 12, receipt/checkpoints 32, manager 13, finance integration 9. Existing substantive assertions retained; legacy adapters gained the new mandatory receipt dependencies/identity parameter.

The original baseline had 762 behavior checks. The new total adds 96 new suite cases plus four navigation cases (new page and three targeted input scenarios). Adapter tests execute extracted production methods with deterministic world/file adapters under .NET 8; the full script separately compiles under net48/C#7.3. They do not execute GTA pathfinding, native attachment, actual game-process interruption or the installed Enhanced runtime.

Run in the managed workspace:

```sh
source /workspace/.cloud-setup/env.sh
python verification/inventory340/validate.py --output /tmp/gh340-review-validation
```

The environment activation is specific to this workspace; other machines need the .NET SDK, net48 reference assemblies and pinned NuGet dependency. Commands, logs, source hashes and preservation results accompany this guide.

## Known limitations and deferred work

- Owner RC1 runtime evaluation and v0.3.4 GTA acceptance remain pending.
- One pending order and one last outcome; no multi-order queue/history archive.
- Supplier cycling retained; no new full supplier catalog screen or dynamic quote engine.
- Nearby manager auto-drop still requires configured physical Mule route; no new per-order attended override.
- Earned boxes remain after sales/consumption, so parcel slots can block receipt with unit headroom. No silent visual removal/rotation.
- Wholesale carry reload uses existing warehouse recovery, so the owner may need to repeat transport; it does not recreate inventory or invoices.
- Notification wrapping at actual resolution/UI scale and physical route recovery need GTA confirmation.
- No new damage categories, COGS/lot valuation, retail markup, automatic buying, customer delivery missions, manager wholesale, tablet or physical damaged-car repairs.

## Review, future testing and rollback

Review against feature/shvdn3-336-service-recovery-business-polish, not main. Keep the working RC1 and prior v0.3.3.5 baseline available. No merge or release is performed.

When an experimental test package is separately approved: close GTA; back up the matching script and the entire scripts/GlassHeroesBusiness directory, including Save.ini, Settings.ini, FinanceV1 data in Save.ini and every companion CSV/journal/profile file present. Keep backups outside scripts. Install exactly one GlassHeroesBusiness.3.cs; move other Glass Heroes .2.cs/.3.cs/DLL versions out of all script search locations. Do not install reference/dependency DLLs just because the project compiled with them.

Use a disposable complete data copy and the owner Legacy 1.0.3889.0 / Enhanced 3.9.0.6 / LSRED setup. Follow GTA_TEST_CHECKLIST.md and record installed hash/startup identity. Do not alternate experimental/baseline scripts against the same modified sole Save.ini.

Rollback: close GTA, archive the complete experimental data for evidence, remove its script, restore the matching full baseline business directory and baseline script, verify hashes and exactly one script, then launch. Unknown-key preservation does not prove old builds understand new receipt metadata; restoring only an old Save.ini is insufficient.
