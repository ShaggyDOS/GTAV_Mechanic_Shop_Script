# Glass Heroes v0.3.3.5 / Issue #31 — EXPERIMENTAL DEVELOPMENT REVIEW

Authorized customer/repair workflow and final menu-polish candidate. No release, merge or installation performed. GTA/Windows execution NOT RUN; Enhanced runtime compatibility NOT VERIFIED; historical crashes remain UNATTRIBUTED.

## Baseline, owner evidence and branch

Work only on feature/shvdn3-335-customer-repair-workflow. Fetched exact remote branch at 1582ae8443704cc35926723f11dee8ef912d44be (authorization/handoff docs). Product source matched Issue30 branch commit 53205ad36aa398ebf6c9cb6626ea0d21ad135c0b, SHA256 4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71. Read complete committed implementation handoff and public Issue31 body.

Owner explicitly confirmed using this approved repository baseline while the installed RC2 hash remains unverified, with no conflicting evidence/additional regressions reported. Did not infer deployed identity from a tag. RC1, RC2, Issue29/30, financial/WP1/FS1/integration worktrees, Issue28, main and QA R2 preserved. Original v2 code/project hashes unchanged. No new milestone beyond .5 begun.

Product source: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. Existing project directly links this source; net48/C#7.3, official ScriptHookVDotNet3 3.6.0. No dependency changes. Header/BOOT identify .5; SHA256, not reused labels, identifies exact code.

## Focused implementation commits

- 5cf4321: visible Mechanic Break Area rename and role-based walking routes, existing point/mask handlers reused. Initial navigation 100/100 and Release 0 errors/two inherited warnings.
- 854c9d9: demonstrated finance/supplier Back-path corrections; 65-page menu audit. Focused navigation then 166/166.
- da14ab5: FIFO survival, usable bay eligibility, entity-creation retry backoff and destroyed staged vehicle validation. Initial focused workflow 16/16.
- 4eff687: assignment ownership and invalid-entity pre-settlement guard, including physical ON destroyed-car cleanup; focused workflow then 36/36.
- 8dfa965: existing on-demand customer status names mechanic/bay and clarifies wait; header aligned. Workflow 38/38.
- Final verification/documentation commit: exact SHA reported by branch log and completion response. Contains actual full-run evidence, expanded navigation, source-preservation and Word/PDF report; no additional product change.

## Confirmed defects and fixes

1. Owner-reported break label and flat route discovery: corrected consistently in visible placement/editor/legend/help. `_breakArea`, EmployeeBreakArea and all XYZ/headings untouched. Walking Routes & Approaches now has Mechanics, Receptionist, Manager, Customers, Supplier Driver. Existing central hub still has six categories.
2. Financial Reports opened from Expenses ignored its return flag and went to Management. Back now restores Expenses row 8; direct Accounting still returns to Management row 3. No financial actions/math changed.
3. Pallet Storage entered from Supplier Settings fell back to Inventory. Capture its calling context so Back restores Supplier Settings row 4. Existing pallet buying/capacity/receipt logic unchanged. Correct stale overdue-wage notice to Accounting > Expenses.
4. When staged Car 2 disappeared and Car 3 survived, the next visual update could spawn a newer Car 2, which dispatch preferred. Do not refill Car 2 while a surviving Car 3 exists; dispatch retains the older live reservation/vehicle. Existing promotion, pending-request decrement and one-drop-off checks remain.
5. Spawn failure set `_nextCustomerAt`, but pending dispatch did not consult that scheduling value, allowing creation attempts each visual second. Add session-only GTA-time backoff shared by direct/staged creation: 30s after creation failure, 120s after invalid direct model. Guard before spawning/model requests; retain requests and preserve safety/occupancy gates. Model streaming and ordinary obstruction are not redesigned. No thread, blocking wait or persisted retry timestamp.
6. Intake accepted a free worker without requiring a configured usable logical bay. CustomerBayAvailable now validates configured work/car target and existing order ownership; CanStartNextIntake uses it. Existing second intake during another repair remains permitted when drop-off, receptionist, free worker/bay and stock permit. Paid return priority/intake exclusivity unchanged.
7. Physical ON assignments did not protect coincident saved bay targets, and OFF settlement freed HasJob before its paid vehicle left the bay. Reuse CustomerBayAvailable in assignment: reserve worker slot and targets within conservative 4m until payment/return stages free them. Prefer existing specialists; keep original ON all-custom-car-bays-unset legacy fallback. This is logical ownership, not a new world scan or assignment framework.
8. Visual validation rejected destroyed vehicles only for OFF mode; economy can run before visual cleanup and physical settlement lacked a corresponding vehicle/customer guard. Reject missing/dead vehicle/customer before any work/completion credit and use existing cancellation/promise preservation. Paid retry protection, parts consumption, quote save and financial settlement remain unchanged.

Source-confirmed behavior is not proof these defects caused a particular owner's GTA symptom. No CLR or startup crash attribution follows from this work.

## State transitions and unchanged behavior

FIFO: lost oldest entity -> existing validation cleans owned entities/retains request -> surviving third stays oldest -> no newer first-slot replacement -> existing activation consumes one request and claims drop-off. Temporary blockers retain pending work. Staged route timeouts remain bounded; no unnecessary recirculation or queued-entity recreation added.

Intake: existing active count, reserved stock, drop-off/return ownership, worker-at-break and receptionist gates remain. New logical bay gate only excludes unusable/reserved choices. Up to three active customer slots and two staged cars remain bounded. One/two/three staff tests exercise independent assignments; one staffed bay serializes safely.

Assignment: freeze/commit accepted quote before movement, rollback failed save and reserve exactly one worker/bay. ON retains walk/enter/drive/park/exit/return tasks and legacy fallback. OFF retains one-shot placement/heading, existing employee animation/work, settlement and pickup, including occupancy/player safety, partial-heading retry and bounded queries. No transfer helper or driving algorithm changed.

Completion: invalid vehicle/customer -> existing cancellation and unpaid promise preservation. Otherwise existing arrival/parking/work gates -> TrySettleRepair exactly once -> saved paid state/inventory/cash -> original invoice/notification -> clear pickup or retained paid wait. No second repair from blocked pickup. Cancellation logic and native entity cleanup ownership unchanged. Invoice CSV remains inherited best-effort logging; FinanceV1/committed state is authoritative if CSV writing fails.

Missing worker recovery, physical movement timeouts and route recovery were audited and exercised through deterministic state adapters. No speculative new driving recovery, forced occupied-car teleport or persistent completed-car serialization was added. A deleted worker can still defer while original respawn/route policy applies; actual GTA task recovery remains manual acceptance.

Before:
```csharp
CustomerOrder next = _stagedCustomerOrder ?? _stagedThirdCustomerOrder;
// A failed Car 2 could be refilled before this existing selection.
```
After: existing selection is unchanged; SpawnRoadsideWaitingCustomer refuses a new first slot while a surviving third reservation exists.

Before: CanStartNextIntake only required a free at-break worker; assignment only checked HasJob/configuration. After: both consult bounded CustomerBayAvailable, with per-order ownership and saved-target overlap checks.

Before: physical destroyed cars could pass visual/settlement gates. After:
```csharp
if (order.Vehicle == null || !order.Vehicle.Exists() || order.Vehicle.IsDead ||
    order.Customer == null || !order.Customer.Exists() || order.Customer.IsDead)
{ CancelCustomerOrder("Customer entity unavailable before repair settlement", true); return; }
```

Full source.diff is against the exact approved Issue30 product and is verified to reconstruct the final source.

## Menu organization and complete audit

Mechanics role links: existing five arrival points; five SHARED work points; three bay approaches; MechanicRouteRoster and each worker's outbound/return masks; legacy checkpoint; existing Walking Mode & Recovery page. It does not create fifteen independent physical points. Receptionist/manager link their existing stops, final approach and mode pages. Customer/supplier roles link their existing stops and approach/handoff. PointEditor uses the same walk/facing capture, restrictions, saved status and coordinates/heading view.

Existing `_placementMenuReturns` provides nested page/selection return. Native keyboard/controller handlers, debouncing, phone/input suppression, menu rendering/scrolling, native Esc/Start pause and actual point editor remain unchanged. Added enum pages are appended. Shared modes/settings are links, not duplicated state. Operational supplier/customer/employee controls remain where expected.

NAVIGATION_MATRIX.md lists ALL 65 pages, labels/titles, default Back target/row and contextual parent overrides. The Word/PDF appendix includes the matrix. It distinguishes neutral-context shape/Back tests from contextual role/editor/controller tests. Per-page tests do not automatically select purchase/destructive actions; inherited action suites and source review cover unchanged transactional handlers. Actual hardware, scrolling visibility and safe controller walking/look need owner GTA validation.

## Modified methods and preservation evidence

Menu methods: GetMenuItems, GetMenuItemValue, GetMenuTitle, GetSelectedDescription, ExecutePlacementSelection, ExecuteSelection, NavigateBack, ExecuteEmployeePlacementsSelection, ExecuteMarkerSelection, ExecuteSupplierDeliverySetup.

Workflow/status: CanStartNextIntake, AssignCustomerMechanic, SpawnCustomerOrder, SpawnRoadsideWaitingCustomer, ValidateStagedCustomer, UpdateOneCustomerOrder, ProcessOneCustomerRepair, ShowCustomerOrderStatus. Constructor BOOT label only; ProcessEconomy notification string only. New helpers: CustomerBayAvailable, CustomerSpawnRetryReady, DeferCustomerSpawn. Session-only retry fields; no entity fields persisted.

Preservation pins 346 existing methods unchanged. Additional exact assertions prove constructor label-only and economy notification-only deltas. Freeze Save, FinanceV1 load/write, TryCommitBusinessSave, TrySettleRepair, RecordCustomerInvoice, OnTick/OnAborted, all point capture/load/save/render helpers, remote operations, supplier order/receipt, cancellation, driving and one-shot teleport helpers. Existing approximately one-second customer visual cadence unchanged; new ownership loops bounded by three slots. No new world enumeration, expensive Tick work, spawned type, thread or recursive callback.

Save.ini/Settings.ini schema changes: NONE. No location/key/default reset; EmployeeBreakArea unchanged. Finance, inventory, mixed supplier orders, manual retail, wages, pallets, staff XP and saved accepted quotes protected by preservation and inherited tests. Source fixtures are verification inputs only, not install scripts.

## Actual validation and limits

Final exact-source full run passed 671/671 (467 inherited + 166 menu + 38 workflow), then the navigation suite added all three mechanics x two directions x DIRECT/CUSTOM configuration variants and reran 178/178 with NO further product changes. Combined total is 683/683. The log/validation records these actual runs; no redundant second full run is claimed after test-only expansion.

Focused workflow compiles production intake/staging/spawn/assignment/order-state/cancel/settlement logic against deterministic native/entity/movement adapters and temporary-file quote saves. It tests physical timeout state dispatch, not GTA driving AI. Existing 34 bay tests also exercise actual one-shot placement logic through native adapters; 41 quote tests cover save failure, abort, restart and no offline progress. New menu serializers roundtrip 59 placement sections with flags/XYZ/headings and unrelated cash sentinel, through a ScriptSettings file adapter. All real serializers are additionally source-pinned.

The actual Release project compiles net48/C#7.3 against SHVDN3 3.6.0 using fresh temporary obj/output. Warnings are inherited CS0169 (_selectedInvoiceMechanicSlot unused) and CS0649 (CustomerOrder.VehicleTaskIssued never assigned). This is not engine testing and does not certify Enhanced 3.9.0.6, LS RED interoperability or stability.

Reproduce:
```sh
source /workspace/.cloud-setup/env.sh
python verification/workflow335/verify.py --label v0.3.3.5-issue31 --profile workflow335 --suites repair sales pallets wages reload supplier range menu cart pause focus finance startup hardening bays workflow335
```

## Known limitations, hypotheses and next test

- Paid departure/pickup vehicles and unaccepted queue requests remain session transient, as in the baseline. Reload can clean a paid car while preserving its paid cash state. No persistent completed-car design was introduced.
- Logical saved-target spacing uses conservative 4m clearance; dense MLO bays may defer. Physical collision/pathfinding remains engine-dependent. Misconfigured points still need correction.
- A failed oldest staged entity cannot preserve that physical customer's identity; its pending request survives, and the surviving older live reservation gets priority over a replacement. No durable unaccepted queue identity added.
- Creation cooldown uses session GTA game time and paused/boot guards, not wall-clock offline recovery. Existing model-request/streaming behavior otherwise remains; a model that never loads is not automatically substituted.
- Physical lost-worker/parking/enter/exit recovery and manager/streaming transitions retain original policy. Source audits/tests do not prove repeated idle simulation failures or historic crashes resolved.
- Some original operational shortcuts remain intentionally; all roles share the same existing editors. Treat new owner menu reports as .5 stabilization issues before a larger milestone, not proof that all runtime UI defects are eliminated.

Next test: owner source-verified GH + LS RED extended session using GTA_TEST_CHECKLIST.md, first all role/edit/Back/controller paths, then parallel queue/repair ON/OFF and save/reload. Record IDs, source/runtime hash and exact paths for defects. Owner chooses duration; no invented stability threshold.

## GTA checklist, rollback and package readiness

GTA_TEST_CHECKLIST.md is the detailed check sheet, including every menu/role, each worker/direction, one/two/three staff and queues, FIFO loss case, native input, pause/restart, transactions and extended observation. Restore COMPLETE matching source/data/journal backups for rollback; never alternate independent builds against modified sole Save.ini. No live installation accessed.

After code review and explicit release approval only, a proposed v0.3.3.5-rc1 package can contain this exact single .3.cs, matching optional project, checksum/manifest, installer-free guides and reports. No archive, tag, release or installation is created by this milestone. No main/QA/Issue28 merge or next feature work.

## Recorded automated results

Genuine SHVDN3 3.6.0 net48 / C#7.3 Release: PASS, 0 errors; inherited warnings CS0169 and CS0649. SDK 8.0.426.

683/683 behavior tests PASS: 85 inherited issue/logger/migration checks plus repair 32, sales 29, pallets 25, wages 22, reload 41, supplier 37, range 40, menu 13, cart 32, pause 9, focus 8, finance 33, startup 18, hardening 9, bays 34, workflow335 38; navigation 178. Static 21/21 PASS; focused scope/save/v2/clock/settlement/transfer-helper preservation PASS. The new navigation tests use production members/case arms with deterministic adapters, not GTA.

Candidate source SHA256: 741c97730856adad11cd98d985e41d4eb325c9788af5ceebeee965a0eb64125f. Full diff verified to reproduce this source from 53205ad.

GTA testing NOT RUN. Windows execution NOT RUN. Enhanced runtime compatibility NOT VERIFIED. Crash cause UNATTRIBUTED. No release archive, runtime deployment, save modification or merge was produced by this task.
