# v0.3.3.5 / Issue #31 — owner GTA check sheet — NOT RUN

Target: GTA V Legacy 1.0.3889.0, SHVDN Enhanced 3.9.0.6, Glass Heroes + Los Santos RED first. Compilation against official SHVDN3 3.6.0 does not certify Enhanced compatibility. Owner determines extended-session duration; this is not a fixed stability test or Issue28 resolution.

## Before a future authorized test installation

- [ ] Review this candidate first; no release or installation is performed by this task. Preserve the exact working RC2 script and COMPLETE matching scripts/GlassHeroesBusiness directory outside scripts (Save.ini, Settings.ini, journals, CSV, backups/recovery files and logs). Keep a sole-original backup untouched and use disposable QA copies.
- [ ] Record installed source SHA256, GTA/runtime/mod versions and selected transfer/walking/parking modes. Exactly one Business Manager .3.cs or compiled equivalent, never both. Default 30-second active-game startup gate; confirm one instance, correct .5 BOOT/source hash and no initialization/save errors.
- [ ] Compare all preexisting cash, financial records, stock, pending suppliers/quotes, staff progression/assignments, points and headings before any edit. Do not alternate candidate source on modified data without restoring its complete matching backup.

## Menus and placement

- [ ] Use NAVIGATION_MATRIX.md to visit all 65 pages and every safe view row. Exercise keyboard and controller, Up/Down wrapping/scroll, Accept once/held-repeat/frontend echo, visible Back and Backspace/controller Back, close/reopen F2, Esc and Start native pause. Walk/look safely with controller during point placement; watch for input suppression, clipping, wrong action or stale parent.
- [ ] Management has only Accounting > Expenses & Bills. Reports opened from Expenses return to Expenses row 8. Equipment/invoices/wage/supplier settlement remain accessible; browsing never purchases/pays/writes unexpectedly. Owner deposit/withdraw and controller/shop rows map correctly; do not move shop just to browse.
- [ ] Placement Settings has six categories. Walking Routes has Mechanics, Receptionist, Manager, Customers and Supplier Driver. Check all role Back rows and exact highlighted parent selections. Old Customers/Employees hub shortcuts still return to their original rows.
- [ ] Employee Locations says Mechanic Break Area; editor and legend agree. Existing position/heading survives save/reload under EmployeeBreakArea. Do not reset it. Receptionist and manager destinations remain separate.
- [ ] Mechanics: all five arrival and five shared work stops, three final bay approaches, legacy checkpoint, existing Walking Mode & Recovery link. Job Waypoint Selection: Mechanics 1/2/3 each outbound and return; toggling one mask changes only that mechanic/direction. There are FIVE shared physical work points, not fifteen independent coordinates. Test DIRECT/CUSTOM modes without editing unrelated points.
- [ ] Receptionist/manager roles: five stops, final approach, existing walking-mode link. Customers/supplier roles: three stops and reception approach/handoff. Save Position + Facing on disposable data, Show Saved Coordinates, Back to exact list/role/row, reload. No role edits another role's route.
- [ ] Service Bays 1/2/3: separate mechanic animation vs car coordinates/headings, fixed assignments, AUTO LOT/CUSTOM. Existing closed-shop/active-order editing restrictions remain. Marker display ON/OFF changes visibility only. Existing AUTO LOT preview independent.
- [ ] Delivery & Stockroom: supplier spawn/unload/handoff, owned pallets and driver stops. Pallets opened from Supplier Settings return there, not Inventory. Equipment from hub returns there; equipment from Expenses returns to Expenses. Purchases remain explicit existing actions.

## Queue, assignments and vehicles

- [ ] One customer/one mechanic: full intake, accepted quote, work arrival/animation, one payment/parts consumption, pickup and departure. Repeat with two then three mechanics, all staffed usable bays configured.
- [ ] Three queued customers/one staffed bay: roadside Cars 2/3 survive while first is serviced; enter FIFO. With three mechanics, Customer 2 checks in while Customer 1 still repairs, only when drop-off/receptionist/another usable staffed bay are free. Each worker owns one order; distinct saved targets must be clear.
- [ ] Obstruct drop-off/pickup and coincident/occupied bay targets: no double-booking, head-on return or premature check-in. Returning paid car gets priority; clearing obstruction resumes. Conservative 4m logical bay separation may defer tightly spaced MLO bays; report actual coordinates/mode.
- [ ] Transfers ON: walk to car, enter, drive/park, exit, work animation, return drive/customer pickup. Temporarily impede safe entry/exit/parking on disposable QA; existing timeouts cancel/defer without occupied-car teleport or continuous tasks. Misconfigured MLO points require owner correction, not unsafe forced movement.
- [ ] Transfers OFF: one safe bay placement and one safe pickup placement; no occupied car/player teleport, no repeated position write. Block pickup after payment: retain paid state, no second repair/invoice; resume once free.
- [ ] On disposable data, remove/destroy queued/active vehicles or lose a mechanic/customer: safe cancellation/defer and accepted quote retained where applicable, no ghost payment. If staged Car 2 disappears but Car 3 survives, no newer replacement jumps ahead. Preserve request count.
- [ ] If entity creation fails naturally, observe retained request and bounded retries (30 active-game seconds; invalid direct model 120 seconds). Do not deliberately corrupt the main setup. Timeouts already bound route failure; new cooldown is only for spawn/model failure, not endless fresh customer creation.
- [ ] Use Customer Status on demand: assigned mechanic/bay, other live order stages, waiting state; no per-frame spam. Record IDs/stages, quote and before/after cash/stock/invoice for each unexpected condition.

## Persistence, economy and extended session

- [ ] Reload/abort during check-in, accepted assignment and repair with COMPLETE disposable matching data. Accepted quotes/remaining work recover; no old entity handles or stale movement command. Failed-save settlement must remain unpaid and retry once after save access recovers; do not damage a live save to test failure injection.
- [ ] Reload during paid blocked pickup: no duplicate payment. Paid session cars remain transient and may be cleaned up, as in Issue29; completed-car serialization is NOT implemented. Unaccepted roadside requests/entities are also session transient; do not infer durable order acceptance before a quote save.
- [ ] Pause pending work/ETA, wait, resume: no paused accrual. Close GTA and restart: no offline catch-up. Regress remote manager jobs, $5,000 revenue reconciliation with/without settled expenses, unpaid wage/supplier accrual versus actual cash payments, retail sales, mixed supply orders, wholesale player-only and nine-box pallets.
- [ ] Conduct the owner's longer GH + LS RED session. Record duration and exact menu selection path for each defect; capture SHVDN/RPH/Errors logs and dump with source hash. Keep gameplay CLR overflow, startup AV and EUP/file-access findings separate. Additional menu reports stay in this .5 stabilization milestone for triage before larger feature development.

## Rollback

Close GTA; preserve the entire tested source/data/log set separately. Remove .5 and restore the exact prior single RC2 script AND its COMPLETE matching data directory. Do not restore only Save.ini over another journal or leave duplicate scripts. Full rollback discards test-session progress; preserve the untouched original backup. No rollback/deployment command was executed against a live installation by Codex.
