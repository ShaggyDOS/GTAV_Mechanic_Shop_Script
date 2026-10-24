# Codex implementation handoff — Glass Heroes Business Manager v0.3.3.5

**Owner authorization:** YES — proceed with development after this handoff. The owner has finished providing current notes, wants **v0.3.3.5 to be the final focused menu-polish milestone** before feature development, and intends a longer GTA playtest of the resulting .5 candidate. No live installation or release publication is authorized yet.

- Repository: `ShaggyDOS/GTAV_Mechanic_Shop_Script`
- Development issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/31
- Development branch: `feature/shvdn3-335-customer-repair-workflow`
- Baseline branch: `feature/shvdn3-placement-settings-hub` (Issue #30 / tested-menu candidate)
- Expected baseline **C# source SHA256** from Issue #30 verification: `4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71`
- Product source: `experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs`
- Project: `experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj`
- GTA target: Legacy `1.0.3889.0`, SHVDN Enhanced `3.9.0.6`, initial GTA testing with Los Santos RED plus Glass Heroes only.

## 0. Starting conditions and source baseline (mandatory)

Fetch the latest exact refs, inspect Issue #31 and this document, and compare candidate code against `feature/shvdn3-placement-settings-hub`. First inspect any owner-reported RC2 packaging / installation evidence. The `v0.3.3.4-rc2` Git tag was not independently found in repository comparison; **do not assume that an unverified release tag is the code installed in GTA**. Confirm the branch C# source hash and seek the user's actual installed RC2 script hash / report if available; if an installed version mismatch is discovered, pause and report the discrepancy before implementing gameplay modifications. Otherwise, use the verified Issue #30 source baseline as specified.

No edits to `main`, Issue #29, Issue #30, the active owner game installation, saves, or Issue #28 crash investigation. Preserve the working v0.3.3.4 code and RC2 rollback snapshot. No claims that prior crashes are fixed.

## 1. v0.3.3.5 scope and implementation order

This version is **customer/repair workflow polish PLUS final menu/navigation polish**, not a new tablet, accounting system, placement engine or AI rewrite. Work in separately reviewable commits; isolate menu changes from customer workflow changes.

### Commit A — owner-requested menu corrections (highest priority)
1. Rename the visible placement **Staff Break Area** to **Mechanic Break Area** consistently in **Placement Settings > Employee Locations**, PointEditor title, marker/help/status text where relevant. Keep `_breakArea`, `EmployeeBreakArea` saved key, all XYZ/heading and behavior unchanged. Do not rename a save section or reset player placement.
2. Under `Placement Settings > Walking Routes & Approaches`, group routes by role into appropriate **Mechanics**, **Receptionist**, **Manager**, **Customers**, **Supplier Driver** submenus. Within **Mechanics** provide clear links to:
   - Spawn → Mechanic Break Area: existing 0–5 `_arrivalRoute` waypoint editors
   - Mechanic Break Area → Service Bays: existing 0–5 `_serviceRoute` editors
   - Final Bay Approaches: existing 3 `_bayApproaches` points
   - Job Waypoint Selection by Mechanic: existing `MechanicRouteRoster` and each staff member's independent outbound/return waypoint masks
   - Legacy Mechanic Checkpoint: existing `_walkCheckpoint` editor
   Keep walking-mode, safe stuck-route and transport toggles accessible; if desirable, use an additional navigational shortcut to `MechanicWalkingPaths` rather than copying controls.
3. IMPORTANT data model: `_serviceRoute` uses **five shared saved physical positions**, while `Mechanic.JobOutboundMask` and `JobReturnMask` select which of those positions an individual mechanic uses. Do **not** invent 15 independent physical points or rewrite masks/route semantics.
4. Every navigation path and visible Back row must return to the correct parent and highlighted option, including PointEditor nested within a role, and controller vs keyboard. Review `_placementMenuReturns` context, `NavigateBack`, `OpenPlacementMenu`, `GetMenuItems`, `GetMenuItemValue`, `ExecutePlacementSelection`, `ExecuteSelection`, `GetSelectedDescription`, `GetMenuTitle` and existing route handlers.

### Commit B — final menu navigation audit and focused fixes
Treat v0.3.3.5 as the menu-cleanup completion gate. Review **all** reachable F2 menus (not just route grouping):
- Business Management; Accounting > Expenses & Bills (no duplicate management Expenses link), reports/owner transfers, equipment and purchasing.
- Placement Settings six categories, all customer/employee/bay/supplier positions and saved/unset status; menu captions and help descriptions consistent.
- Employee mechanics/receptionist/manager profiles, walking modes and routes, customer queue/vehicle-drive settings, inventory/supplier and controller options.
- Scroll/wrapping, highlighted selection restoration, closing/reopening F2, native Esc/Start pause, keyboard/controller accept/back, held-key debouncing, safe controller-only walk/look while menu open.
- No wrong action caused by positional indices; no hidden or unreachable point editor; no financial purchase from viewing a menu; no unwanted Save.ini/Settings.ini writes when merely browsing.
- Fix only demonstrated navigation defects, mismatched titles/descriptions or clear usability errors. Do not add a complex redesign, new permanent rendering/UI props, unrelated financial changes, or new save keys. Provide a complete navigation matrix in the report.

### Commit C — customer queue and parallel service correctness
Current source has `_activeCustomerOrders`, `_stagedCustomerOrder`, `_stagedThirdCustomerOrder`, `UpdateCustomerVisuals`, `CanStartNextIntake`, `NeedsDropoffSoon`, `IntakeInProgress`, `ActivateRoadsideWaitingCustomer`, `SpawnRoadsideWaitingCustomer`, `UpdateStagedQueueTravel`, `ValidateStagedCustomer`, `CanReturnCustomerCar`.

Requirements:
- Customers 2 and 3 stage roadside safely and enter drop-off/check-in in FIFO order when it is clear.
- Customer 2 must be able to check in while Customer 1 is **still being repaired** if the drop-off, receptionist and *another staffed usable service bay* are free. Do not wait for the full first order to finish just because `_activeCustomerOrders` is nonempty.
- One live arrival/vehicle return may occupy the shared drop-off at a time. Preserve paid returning vehicle priority and avoid head-on overlap or intake starvation/deadlock.
- Never double-dispatch queued reservations, double-book bays or mechanics, consume reserved stock twice, create infinite pending requests, or spin/recreate staged entities each tick. A staged failure should preserve the pending request where appropriate.
- Determine actual faults or missing behavioral tests before changing established safeguards; simplify rather than rewrite.

### Commit D — mechanic, transfer and repair completion reliability
Audit `AssignCustomerMechanic`, `ProcessCustomerRepair`, `ProcessOneCustomerRepair`, `TrySettleRepair`, `RecordCustomerInvoice`, `ReleaseCustomerForPickup`, `CancelCustomerOrder`, `ProcessTeleportOrder`, `TeleportTargetAvailable`, vehicle movement / parking helper methods.

- Preserve v0.3.3.4 ON physical mechanic walking, driving, parking and returning paths, and OFF one-shot vehicle TP to bay and pickup. **Never teleport player/NPC-occupied vehicles**.
- Mechanics may have one job per worker and one car per bay. Repair animation is at a separate saved work point from the car position; do not shorten gates that wait for the worker to arrive.
- A complete repair settles once, consumes the correct parts, records one invoice, returns the car when the pickup is free and lets the customer depart.
- Preserve accepted quote/reload identity and save-failure rollback; no duplicate financial credit on retries, no lost unpaid promises or ghost payments.
- Missing/destroyed entities, failed enter/exit/parking, blocked return, abort/reload and temporary collision obstructions should clean up or safely defer according to ownership and current policy. Existing Issue #29 limitation for already-paid transient cars after reload must be documented, not silently re-engineered.
- Only implement *confirmed* bounded recovery changes (with tests). No repeated native driving tasks per Tick, recursive retry, polling loops, blocking waits, unbounded logging or new background worker. Keep heavy mod stacks (LS RED/LSPDFR/etc.) in mind.

### Commit E — tests, docs, packaging readiness (not release publication)
- Compile actual SHVDN3 project using net48/C#7.3; run all inherited behavior, integration, menu, pause, finance, supplier/pallet, repair, queue, bay, startup and static tests. Count genuine passes and inherited warnings accurately.
- Add focused route-submenu navigation/PointEditor Back/controller tests; test `EmployeeBreakArea` persists identically despite display rename; test masks are per-mechanic while physical waypoints remain shared.
- Test customer 1/2/3 and 1/2/3-mechanic scenarios, FIFO dispatch, return priority, occupied bay/drop-off, quote save/reload, stock reservation, no duplicate repair settlement, physical ON, teleport OFF, pause/abort/restart, failed spawning and failed movement; verify no offline catch-up.
- Prove source-preservation for unmodified finance/manager/supplier/placement systems and existing save schema (expected no changes). Do not mislabel deterministic harness runs as GTA in-game testing.
- Prepare developer guide and DOCX/PDF documentation, concise changelog and user-facing manual GTA test checklist. Offer an experimental `v0.3.3.5-rc1` package plan **after code review/authorization**, not an automatic live release or install.

## 2. Release/test expectation
Owner intends a **longer real-game test session** with v0.3.3.5 specifically to surface remaining menu problems. Prepare a practical GTA check sheet with:
- Startup, exactly one .3.cs script loaded, installed source checksum and matching backup of full business data/journals;
- Navigate every F2/Controller/menu branch, all five Walking Route role groups, each mechanic route-selection pair, PointEditor edit/save/back/reload;
- Queue 1/2/3 and repair flow with 1/2/3 mechanics, ON and OFF transfer, pickup priority, accounting invoices;
- 1+ extended session with LS RED and GH baseline, recording symptoms and reproducible menu-selection paths and logs;
- Game-pause and closed-session nonaccrual verification, complete rollback instructions.
Do not invent a fixed stability duration or assert Issue #28 solved; owner determines test duration.

## 3. Reporting / stop gates
1. Confirm and report base branch/commit and baseline file SHA256; if installed RC2 identity conflicts, **stop before code edits and ask for the correct base**.
2. Implement focused commits only on `feature/shvdn3-335-customer-repair-workflow`.
3. Provide change log and static review, builds/tests evidence and exact case counts, risks, save/serialization compatibility, summary of confirmed vs hypothesized defects, GitHub comparison/draft PR if available.
4. **Do not merge**, overwrite live GTA files, publish a release, modify Issue #28 or branch into the next feature milestone without owner approval.
5. Treat any new owner reports during the .5 extended QA as defects to triage and propose for **this .5 stabilization milestone** before declaring the menu complete; do not silently expand scope beyond menu issues/customer lifecycle.
