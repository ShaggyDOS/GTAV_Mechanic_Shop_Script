# Glass Heroes Business Manager v0.3.3.6 — EXPERIMENTAL DEVELOPMENT REVIEW

Issues #33 (P0 service recovery) and #34 (business/UI/demand). Source-verified candidate, not a released RC or stable build. GTA/Windows execution NOT RUN; Enhanced 3.9.0.6 compatibility NOT VERIFIED. No merge, tag, release, installation, live save change or Issue28 crash attribution.

## Evidence and baseline

Initial authorized feature branch feature/shvdn3-336-service-recovery-business-polish at 52749f7 contains only the handoff above approved .5 source commit 22bff0990bdc771215ebcb587e8426059df14336. Its C# SHA256 exactly matched 741c97730856adad11cd98d985e41d4eb325c9788af5ceebeee965a0eb64125f before edits. Read full committed CODEX_V0_3_3_6_OWNER_QA_HANDOFF.md and public Issues33/34. PR32/.5 and older candidate worktrees are protected, not merged or modified.

Original Errors log and owner Save.ini were NOT supplied in this workspace for .6. Embedded issue/handoff evidence reports: .5 GH+LS RED session without crash/managed exception; 16 saved-waypoint and 5 emergency walking recoveries, 3 ambiguous-direction mechanic parking failures, 14 queued parking failures, one direct arrival timeout, four normal departures and some repair-revenue entries. This is reported evidence, not a claim we read private full files or reproduced GTA.

Source confirms unset selected work nodes are skipped; mask8 selects point4, so the reported unset ServiceWaypoint4 produces no selected work stop. It does NOT independently explain pickup: pickup reverses ArrivalWaypoints, not work masks. Reported 17m/15m/16m/17m reverse pickup legs and Bay3 ~0.39m work/car separation are geometry hypotheses needing GTA checks. No saved path sorting, mask migration or coordinate rewrite occurs.

Source confirms paid and unpaid parking failures shared cancellation/message; Employee Status could count a stale working phase without a live repair. Those mechanisms are corrected/instrumented. Neither source tests nor the prior crash-free .5 session prove .6 fixes every navigation/animation failure or historical crash.

## Focused commits and verification plan

P0 was implemented and tested before independent business features:

- 9889842: paid/unpaid direction handling, one parking retry, retained paid recovery, bounded driver exit, logical worker/return ownership, queue retry, mask warnings/USE-UNSET and initial accurate status. Initial gate 701/701 (683 inherited + 18 new), 21 static, actual Release 0 errors/two inherited warnings. Final evidence below supersedes initial source hashes.
- ae6dc03: owner amount/review/confirmation and removed direct cash shortcuts. Inherited finance33/startup18 plus new owner13.
- c5a9886: quantity retail draft/review/atomic batch, compatible audit and unchanged pricing. Original one-unit helper remains as a wrapper; inherited sale/repair suites plus new retail tests.
- e1f1339: employee snapshot menu, Settings.ini controller precedence, obsolete controller menu removal, row-specific help and input/navigation tests.
- 7db0764: GTA-hour-weighted active-time demand, new demand13.
- aca343b: NPC destination clearance and invalid-speed holds for paid recovery; normal OFF flags preserved.
- dcde623: negative unit-estimate guard and clearer workstation/demand labels.
- 8833abe: manual recovery restricted to physical handoffs; original OFF safe pickup remains automatic. Final recovery21.
- fc1d986: full staff summary uses the same refreshed/live-validated snapshot as the menu, eliminating stale receptionist status; status15.
- Final test/report commit: full evidence, fixture compatibility, 67-menu/392-footer matrix, source diff/hash and versioned DOCX/PDF; exact SHA in branch log/completion handoff.

No speculative new scheduler, placement engine, path reordering, background task or completed-car serialization. Each new recovery/transaction/demand condition has focused deterministic tests. Native engine behavior remains the owner acceptance gate.

## Phase 1 — reliability and diagnostics

### Routes, collection and workstation access

Five physical ServiceWaypoint positions remain SHARED geometry; JobOutboundMask/JobReturnMask remain per-mechanic selections. EffectiveWorkRouteMask intersects selected bits with saved flags, without modifying either. WorkRouteStatus shows selected/effective/unset masks; selected-but-unset rows show USE / UNSET. WarnEmptyWorkRoute emits one warning per slot/direction/mask combination in a bounded session set, only when building a route. A zero effective selection explicitly explains the inherited final-approach/target fallback; it does not silently select points1/2/3.

Mechanic route entries say Workstation Access Stops, not a misleading first walk to bay. Physical flow remains Break -> Customer Car (reverse Arrival walk) -> Service Bay (drive/park) -> Workstation (walk/repair) -> Drop-off (drive/park) -> Break (saved return walk). Existing arrival routes are not reordered. ON and OFF remain distinct; no occupied-car teleport fallback on unpaid arrival.

### Parking and paid recovery

UpdateVehicleParking retries native parking ONCE after the existing parking timeout plus15 seconds. It logs direction/order/Paid for genuine retry/failure only. Persistent unpaid bay failure uses existing cancellation/accepted quote recovery, no premature repair payment. Persistent paid pickup failure enters session PaidReturnRecovery instead of canceling a good paid return. Approach/driver-loss and return enter/exit failures follow the same direction-aware policy.

FailVehicleTransfer logs ID, direction, Paid, stage, slot, elapsed, target/actual position, squared distance and speed when available. Original generic .5 messages cannot retrospectively prove direction. No every-tick log or additional world enumeration.

ProcessPaidReturnRecovery is eligible at most once per five GTA game seconds on the existing ~one-second visual cadence. Require a valid, stopped finite-speed car. Ask the assigned driver to leave at most twice, then HOLD if still inside. A later update must prove exit; another vehicle containing the worker is not touched. Validate empty driver seat/passengers, player not inside/near destination, destination vehicle/NPC clearance and existing return priority before the existing one-shot pickup path revalidates and moves the car. Unsafe conditions retain paid state; no extra settlement or invoice. No forced actor deletion.

ReleaseRecoveredMechanic frees the physical worker once after pickup placement and starts the existing return-to-break route. RecoveryWorkerReleased prevents replay. MechanicOnVehicleTask protects held recovery from competing employee animation/tasks. NeedsDropoffSoon keeps paid return ahead of new intake. Manual Vehicle Driving Options row7 resets bounded physical return attempts only; ignores unpaid/already-departing/normal OFF orders. OFF pickup continues its original automatic retry.

Ordinary OFF destination flags remain byte-identical after normalizing the conditional paid-recovery ped flag. Paid recovery explicitly checks NPCs as well as vehicles and the player. Moving/non-finite/negative speed never counts as stopped. A nearby actor can therefore HOLD recovery indefinitely until safe; no unsafe timeout cancellation is added.

### Queue and status

A staged queue car gets one parking retry on the SAME entity, ID and FIFO reservation before original cleanup/retained-request behavior. The successful .5 three-customer queue, independent bay assignment and parts reservations remain protected. Repeated native parking cannot repair an undriveable MLO point; actual geometry needs owner correction.

MechanicServiceStatus differentiates startup/closed/paused/remote, worker unavailable, pickup/parking stage, paid return, vehicle absent/outside bay, workstation wait, valid repair and autonomous job with a quote. ShowEmployeeStatus and the explicit snapshot page provide slot/name/specialty/wages and route masks; recovery notices alone do not claim work. Work/car proximity warning is diagnostic only. No coordinates or progression change.

## Phase 2 — financial/UI features

### Owner capital transfers

Management has seven rows: Employees, Customers, Inventory, Accounting, Placement, Shop Location, Back. Owner transfers are solely under Accounting. Cycle session amounts $1k/$10k/$100k, open separate deposit/withdraw review, then Confirm. Merely viewing/selecting amounts/report period/back never moves money.

TransferPlayerMoney validates the selected amount, existing Story-ped restriction, wallet/business funds, wallet integer limit and long owner-counter/cash limits. Original Save/wallet rollback remains, including notification if an external mod prevents wallet restoration. Deposits/withdrawals remain capital, not revenue/expense. Selection state is not a new saved setting.

### Retail sale draft and atomic batch

Parts Sales has four quantity rows, Review, Clear Draft and Back. Left/Right or Enter adjusts 0–100 per category; shared input arbitration/debounce remains. Review snapshots quantities and shows stock/unchanged unit-cost estimates/proceeds. Confirm rechecks CURRENT unreserved stock and ledger limits.

TrySellParts validates all lines, negative prices and overflow before mutation. One Save applies all stock/cash/gross retail/unit counters, with full rollback if the save fails. One-frame repeat guard remains. Successful CSV audit preserves the original six columns and emits one row per nonempty category, after commit only. Pending supplier cargo and promised repairs are never sold. No retail markup/margin change; zero estimate remains valid as before. Safe UI total formatting reports overflow instead of throwing while drawing. TrySellPart remains a one-unit wrapper for inherited entry-point invariants.

### Employee/controller/menu help

Employee Status is explicitly a SNAPSHOT: three worker rows, receptionist, manager, Refresh/Full Status and Back. Native worker/car checks happen on open/refresh/detail selection; drawing reads cached status strings. Reception readiness requires a live worker; manager distinguishes remote simulation, absent stream and workstation route. Working is not inferred from HasJob/PhaseWorking alone.

ControllerSettings enum/page/action/navigation is removed; controller runtime mappings, debouncing, native pause and walk/look handling remain. ResolveControllerEnabled makes Settings.ini [Gamepad] Enabled authoritative when present; missing key falls back to legacy Save.ini/default. Existing save key is still written through unchanged Save, not deleted. No forced disable, new key or Settings.ini write from browsing.

Every highlighted row gets contextual text or the existing precise row help with the selected label. All67 pages and392 row footers compile/exercise through adapters; NAVIGATION_MATRIX.md and MENU_FOOTERS.md provide complete evidence. Back restores hub/role/editor/review parent and row. Existing wrapping/right-column fit/cache remain bounded; actual controller hardware and text truncation need GTA QA.

### GTA-hour demand

AdvanceCustomerDemand adds only clamped sampled active gameplay seconds under boot/open/arrivals/not-paused/not-remote gates. It is the ONLY new managed call added to OnTick after existing economy processing; original clock sampling/order is source-pinned. QueueDueCustomer reads GTA GET_CLOCK_HOURS once on the existing visual cadence, not during constructor or every frame. No UTC deadline or offline catch-up loop.

Existing SecondsBetweenCustomers is the base (30–900): 00–05 x4; 06–08 x0.85; 09–17 x0.70; 18–20 x0.90; 21–23 x2, ceiling and bounded20–3600 active seconds. At base150: 600,128,105,135,300 seconds respectively. GTA hour weights REAL ACTIVE gameplay seconds; it is not a GTA-minute timer. Session elapsed resets across reload and shop/service transitions; new request creation only is weighted. Manual scheduling bypasses demand wait within capacity, and paused manual entry is rejected. No pending/accepted/paid request deletion, revenue or wage change.

## Code, compatibility and preservation

Authoritative source/project remain experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs and existing directly linked net48/C#7.3 project. No new runtime dependency. Full source.diff against22bff09 is verified to reconstruct exact final source; SOURCE_SHA256.txt identifies it.

Preservation.json lists all changed/added methods and sole removed ExecuteControllerSelection. 332 existing methods remain byte-identical. Extra exact assertions freeze Save, FinanceV1 serializers/commit, TrySettleRepair, RecordCustomerInvoice, ProcessOneCustomerRepair, AssignCustomerMechanic, stock reservation, OnAborted, all point capture/load/save/render helpers, manager remote operations, ProcessEconomy, supplier simulation/receipt, cancellation and teleport retry clock. Constructor delta is only .6 BOOT label plus controller precedence; no early game native. OnTick delta normalizes exactly to the original after removing the managed demand call. Original OFF clearance is unchanged; only paid-recovery destination peds are additionally checked.

Save.ini/Settings.ini schema changes: NONE. Existing positions, headings, masks, staff XP, funds/journal/inventory/accepted quotes/supplier liabilities preserved. New recovery, demand and UI draft fields are session-only. Added CustomerStage value is appended and not serialized as a pending movement. Removed MenuPage value affects only unsaved UI state. Original v2 source/project hashes unchanged. .5 branch, PR32, main/QA and older worktrees remain protected.

Before: parking timeout canceled both directions using the same message. After: one retry -> unpaid existing promise recovery OR paid retained exit/empty-car recovery. Before: Owner Transfers hardcoded1000 immediately; after: amount + review + guarded Confirm. Before: one retail unit immediately; after: reviewed quantities/all-line commit. Before: stale Save.ini controller flag overrode config; after: explicit config wins, legacy fallback remains. Before: new requests used UTC deadline; after: sampled active elapsed plus GTA-hour weighting. No broad redesign.

## Before / after code excerpts

Paid vs unpaid parking (full context in source.diff):
```csharp
// Before: same cancellation for bay and paid pickup.
else if (order.SecondsInStage > _parkingTimeoutSeconds + 15)
    CancelCustomerOrder("Mechanic failed to park at saved point; revise heading", true);
// After: one bounded retry, then direction-specific handling.
else FailVehicleTransfer(order, atBay, "Parking failed at saved point; review heading/clearance");
// Within FailVehicleTransfer:
if (order.Paid && !toBay)
{
    order.PaidReturnRecovery = true;
    order.Stage = CustomerStage.PaidReturnRecovery;
}
```

Route mask handling preserves the loop and data:
```csharp
// Before: unset selected nodes are silently skipped.
if (_serviceRoute[i].IsSet && (routeMask & (1 << i)) != 0)
    result.Add(_serviceRoute[i].Position);
// After: warn once at route construction, then retain the same loop.
if (slot < _mechanics.Count) WarnEmptyWorkRoute(slot, false);
```

Controller precedence and owner amount:
```csharp
// Before:
_controllerEnabled = data.GetValue("Gamepad", "Enabled", _controllerEnabled);
// After:
_controllerEnabled = ResolveControllerEnabled(_configuration, data, _controllerEnabled);
// Before wallet mutation used literal 1000. After validation:
int amount = _ownerTransferAmount;
if (amount != 1000 && amount != 10000 && amount != 100000) return;
```

Retail and demand boundaries:
```csharp
// Before: single-unit entry mutated one bucket then saved.
// After: the one-unit entry delegates to the all-line transaction.
int[] quantities = new int[4]; quantities[kind + 1] = 1;
return TrySellParts(quantities);
// Before QueueDueCustomer compared now with a UTC deadline.
// After: only active elapsed can reach a GTA-hour-weighted request interval.
int hour = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOCK_HOURS);
_customerDemandIntervalSeconds = CustomerDemandInterval(hour, _customerIntervalSeconds);
if (_customerDemandElapsed < _customerDemandIntervalSeconds) return;
_customerDemandElapsed = 0;
```

Status boundary:
```csharp
// Before: PhaseWorking alone increased the working aggregate.
if (employee.ArrivalPhase == PhaseWorking) working++;
// After: row status requires live worker/car/assignment and actual stage gates.
_employeeStatusRows[i] = MechanicServiceStatus(i);
```

## Actual tests and test boundaries

Full exact-source final run: actual Release0 errors/two inherited warnings (CS0169 unused _selectedInvoiceMechanicSlot, CS0649 CustomerOrder.VehicleTaskIssued). 762/762 behaviors: 85 original issue/logger/migration plus inherited feature/queue/financial tests, 180 navigation, recovery21, owner13, retail15, status15, demand13. Static21/21; scope/preservation PASS. All counts come from actual successful runs; earlier failed runs were fixture compile gaps, not silently counted passes.

Inherited migration/hardening adapters now include the actual managed demand helper; wage fixture adds a name for status rows. Single-sale fixtures include the real new batch core, input fixtures add the real retail quantity helper, and one old route-geometry fixture stubs its new warning side effect while focused recovery tests compile it verbatim. Obsolete controller/direct-transfer navigation cases were replaced with new confirmation/config cases, not treated as runtime UI coverage. Focused tests use actual extracted production members/case arms with deterministic native/entity/movement/settings adapters and temporary files. They do NOT run GTA navigation/parking, animation, hardware, NTFS permission semantics or Enhanced runtime.

Reproduce after SDK activation:
```sh
source /workspace/.cloud-setup/env.sh
python verification/workflow336/verify.py --label v0.3.3.6-final --profile workflow336 --suites repair sales pallets wages reload supplier range menu cart pause focus finance startup hardening bays workflow335 recovery336 owner336 retail336 status336 demand336
```

## Remaining limits and next gate

MLO paths, car/work geometry, driver native exit and animation must be verified in GTA. An unsafe or unresponsive occupied paid car holds after two exit attempts until manually retried/cleared; nearby target NPCs deliberately block it. No actor is forcibly deleted to manufacture safety. Unpaid bay failure remains cancellation/promise recovery after bounded retry, not unsafe occupied-car warp.

Paid cars and unaccepted roadside entities remain session transient after reload/abort, inherited from .5. Ledger remains paid; completed-car storage/persistence is NOT introduced. Destroyed/removed paid entities cannot be physically restored. Original best-effort CSV logging can fail independently of committed financial state; do not treat a missing CSV line as proof of a second payment.

Employee status is an explicit refresh snapshot, not a live overlay. Long footer text can truncate to the existing two-line footer; source/report contains full wording. Automatic demand first arrivals may wait longer than the former short startup deadline, especially overnight; manual scheduling is available for testing. Physical creation retry uses its existing separate active-game backoff.

No original private log/save was fabricated, no crash cause proven, and no .6 GTA result claimed. Owner should first validate P0 service/mask/return behavior on GH+LS RED, then UI/transactions/demand and incremental other mods. The checklist includes the reported mask8/unset4 and Bay3 layout suspicion without forcing a setup change. A source warning is not proof the native service workflow now succeeds in every MLO.

## Review, rollback and package plan

See GTA_TEST_CHECKLIST.md for the detailed manual test sheet and complete paired-backup rollback. Keep .5 exact source and COMPLETE data/journals/backup files. Never alternate modified sole Save.ini between independent candidates. No install/rollback performed by Codex.

Prepare draft PR against feature/shvdn3-335-customer-repair-workflow, linking Issues33/34 and this guide, source diff, fresh logs, complete menu evidence and versioned reports. Preserve PR32 draft/unmerged. After source review, a separate authorization can request a single-source v0.3.3.6 RC1 archive; this milestone creates NO archive, tag or release and begins no next feature work.

## Recorded automated results

Genuine SHVDN3 3.6.0 net48 / C#7.3 Release: PASS, 0 errors; inherited warnings CS0169 and CS0649. SDK 8.0.426.

762/762 behavior tests PASS: 85 inherited issue/logger/migration checks plus repair 32, sales 29, pallets 25, wages 22, reload 41, supplier 37, range 40, menu 13, cart 32, pause 9, focus 8, finance 33, startup 18, hardening 9, bays 34, workflow335 38, recovery336 21, owner336 13, retail336 15, status336 15, demand336 13; navigation 180. Static 21/21 PASS; focused scope/save/v2/clock/settlement/transfer-helper preservation PASS. The new navigation tests use production members/case arms with deterministic adapters, not GTA.

Candidate source SHA256: 7fff30978e2dc4c3235656f6ca6865506d5cea53387b7eb6d83654d71c8a9d6b. Full diff verified to reproduce this source from 22bff09.

GTA testing NOT RUN. Windows execution NOT RUN. Enhanced runtime compatibility NOT VERIFIED. Crash cause UNATTRIBUTED. No release archive, runtime deployment, save modification or merge was produced by this task.
