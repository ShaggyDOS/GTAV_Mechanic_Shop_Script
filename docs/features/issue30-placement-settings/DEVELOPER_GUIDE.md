# Glass Heroes v0.3.3.4 — Issue #30 Placement Settings — EXPERIMENTAL DEVELOPMENT REVIEW

Menu-only candidate on feature/shvdn3-placement-settings-hub. No release, merge or installation performed. GTA testing NOT RUN; Enhanced runtime compatibility NOT VERIFIED. Issue #28 remains UNATTRIBUTED.

## Baseline and source identity

Dedicated branch initially 4ac08cb521608bcdab00de0b0807bfbb85cdee7f (owner-approved handoff), based on Issue29 d980be89f387588c02596e8f1ed7732a05ede9d8. Initial source SHA256 8d6ddb213fe69105bbbbc10b6d7746f36c8851912bebfcd5ebd002e643532d6b matches the verified RC1 package and Issue29 verification report. The actually installed working source and GitHub RC1 tag were not independently verified or assumed. Fetched the explicit branch reference; origin's default fetch mapping names the older Issue29 branch, so the exact Issue30 ref was fetched separately. Original worktrees and earlier candidates preserved.

Read complete committed docs/coordination/ISSUE30_PLACEMENT_SETTINGS_CODEX_HANDOFF.md and public Issue #30. Followed both approved scopes. Code is still v0.3.3.4; no new runtime version/release declared. Startup source SHA256 identifies this revision; reused version label alone cannot identify installed code.

Product: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. Project: experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj, directly linked to this source (net48/C#7.3, ScriptHookVDotNet3 3.6.0). No new dependency.

## Independent commits and exact changes

Commit A: 8ed199e — financial menu deduplication. Remove ONLY Management's direct Expenses & Bills entry. Accounting > Expenses remains, including equipment and all invoice/payment actions. Correct action, value, description and Back indices before hub insertion. Initial Release 0 errors/two inherited warnings; focused navigation 10/10. Evidence: commit-a-build.txt and commit-a-navigation.txt.

Commit B: central hub, focused navigation/persistence checks and this development documentation; identify its exact SHA with git log on the feature branch and the review handoff. No financial algorithms changed.

Changed existing methods across both commits: ExecuteManagementSelection, ExecuteSelection, ExecuteCustomerSelection, ExecuteEmployeeSettingsSelection, GetMenuItems, GetMenuItemValue, GetSelectedDescription, GetMenuTitle, NavigateBack. New helpers: OpenPlacementMenu, ExecutePlacementSelection. MenuPage appends three enum values; original values retained. One session-only bounded-by-menu-pages return dictionary records parent page/row, removes entries when backing out and clears when entering a fresh hub/management path. It is not serialized and performs no game/native or disk work.

OpenPointEditor still captures its immediate page and selected row. PointEditor Back returns there first; shared submenu Back then consults the hub context. Old Employees/Customers placement links now open the same central hub and preserve their original parent selection. Other existing operational route/supplier/vehicle controls remain reachable. Neutral Back captions avoid naming the wrong parent on shared pages. No recursive navigation or new placement engine.

Before (Management): Employees, Customers, Inventory, Expenses, Accounting, Controller, Deposit, Withdraw, Shop, Back. Commit A: Employees, Customers, Inventory, Accounting, Controller, Deposit, Withdraw, Shop, Back. Final: Employees, Customers, Inventory, Accounting, Placement Settings, Controller, Deposit, Withdraw, Shop, Back. Finance returns to row 3; Placement to 4; Controller to 5. Deposits/withdrawals/shop remain rows 6/7/8. Existing Expenses -> Equipment remains row 7 within Expenses.

Before:
```csharp
case 3: _menuPage = MenuPage.BusinessExpenses; _selection = 0; break;
case 4: _financesFromExpenses = false; _menuPage = MenuPage.Finances; _selection = 0; break;
```
After:
```csharp
case 3: _financesFromExpenses = false; _menuPage = MenuPage.Finances; _selection = 0; break;
case 4: OpenPlacementMenu(MenuPage.PlacementSettings); break;
```
Before Customer placement shortcut directly opened CustomerPlacements; after it calls OpenPlacementMenu(PlacementSettings). The editor handlers themselves remain byte-for-byte unchanged. source.diff contains the full unified product diff against d980be8 and is verified to reproduce the candidate.

## Structured menu listing and behavior

F2 > Business Management > Placement Settings:

1. Customer Locations: drop-off parking, road spawn, reception approach, reception queue/check-in, waiting car 2/3, existing service-vehicle bay link, customer walk stops.
2. Employee Locations: break area, existing Service Bay link, receptionist desk and approach, manager workstation and approach, staff spawn/entry.
3. Service Bays: existing Bay 1/2/3 profiles. Separate vehicle parking/heading and mechanic work/animation point; roster slots and AUTO LOT/CUSTOM semantics unchanged. Assignment remains informational fixed slot, not arbitrary reassignment.
4. Delivery & Stockroom: existing Mule spawn, unloading spot and driver handoff editors; pallet storage/placement, supplier driver stops, existing Equipment & Upgrades link. Pallet ownership/capacity restrictions remain; browsing buys nothing. Equipment's financial access through Expenses also remains.
5. Walking Routes & Approaches: mechanic spawn-to-break stops, break-to-bay stops, final bay approaches, receptionist stops, manager stops, customer stops, supplier stops, existing mechanic job-route selection and legacy staff checkpoint. Receptionist/manager/customer final approaches remain in their location categories. Route modes, recovery, driving and operating toggles remain in existing settings menus.
6. Marker Display Settings: existing global visual placement-marker ON/OFF and existing color legend. No new filter, auto-lot control, rendering pass or persistent prop. AUTO LOT preview remains at its existing driving-options location.

Existing point editor: walk to the intended point, face the desired heading and select Set / Save Position + Facing. Existing show-coordinates action provides coordinates/heading; SET/unset status remains on existing lists. Existing shop-closed / active-order editing restrictions are retained. F2, keyboard/controller edges/debounce, Backspace/controller Back and native Esc/Start pause behavior unchanged.

## Save compatibility, protections and rollback

Save.ini/Settings.ini schema changes: NONE. No new save/config defaults or migration. No coordinate, heading, roster ownership, business progress, source timer, financial record or inventory modification by navigation. The new return context exists only in memory. Marker toggles retain their original visual/session semantics; a visual marker is not a saved location.

Scope check pins 355 existing methods unchanged, including constructor, OnTick/OnAborted, Save and atomic replacement/rollback, FinanceV1 loading/writing, all capture/load/save point helpers, marker rendering, actual keyboard/controller handlers, vehicle driving/teleport behavior, customer repair settlement, employee work, supplier order/receipt, storage and wages. Existing original v2 source/project hashes unchanged. New session-only fields do not add native scans, entities, background tasks, timers or per-frame work.

Inherited financial and accepted-quote tests exercise actual failure/retry/reload boundaries. New disk roundtrip uses production LoadPoint/SavePoint with a deterministic ScriptSettings file adapter across 59 point sections, including set/unset flags and headings, plus an unrelated cash sentinel. It is not an in-GTA ScriptSettings or NTFS permission test. WorkBay inline serialization and all real Save/loading call sites are additionally pinned unchanged. New editor-save dispatch test uses capture adapters; the capture/native helpers themselves are frozen, not represented as newly executed GTA natives.

Rollback for later owner QA: close GTA, preserve tested source/hash and complete post-test data/logs, restore the exact prior single script and COMPLETE matching scripts/GlassHeroesBusiness backup outside the sole save. Include journals, recovery/backup files, CSV, Save.ini and Settings.ini; never mix only an old Save.ini with a newer financial journal. This task prepares no installation archive and changes no live files. Existing transient paid customer-car reload limitation remains inherited from Issue29.

## Automated validation and reproduction

Run after activating the .NET SDK:
```sh
source /workspace/.cloud-setup/env.sh
python verification/placement_settings/verify.py --label issue30-placement-settings --profile placement --suites repair sales pallets wages reload supplier range menu cart pause focus finance startup hardening bays
```

Full inherited run completed on this exact final source. The navigation suite was then expanded with two additional disk save/default tests and rerun successfully; no product source changed between runs. Results are combined in validation.json. No redundant full rerun is claimed.

The Release build is the actual SHVDN3 net48/C#7.3 project, fresh temporary obj/output, not only the net8 adapter tests. Compiler warnings CS0169 (_selectedInvoiceMechanicSlot) and CS0649 (CustomerOrder.VehicleTaskIssued) are inherited.

Full current results and exact source hash appear in Recorded automated results below and validation.json. Evidence files include build.txt, inherited-behavior.txt, logger-tests.txt, migration-tests.txt, feature-tests.txt, navigation-tests.txt, static-tests.txt, preservation.json and StaticVerification.json. Focused cases compile verbatim production methods/case arms; unneeded GTA effects are deterministic adapters. The two inherited input fixtures only receive the session dictionary needed to compile actual NavigateBack.

## GTA acceptance checklist — NOT RUN

1. Do not install until review approval. For later QA use Legacy 1.0.3889.0 / Enhanced 3.9.0.6 with GH + Los Santos RED and a disposable COMPLETE matching backup. Compile reference 3.6.0 is not Enhanced compatibility proof. Enable exactly one Business Manager script; preserve runtime/mod versions and source hash.
2. Cold launch, default 30-second startup gate: check one instance, BOOT source SHA, no load/save errors; inventory, cash, staff, quotes, supplier order identities and all saved locations/headings unchanged. Open/close menus repeatedly without purchasing/saving: no cash/stock/journal change.
3. Management: select every row. Only Accounting has Expenses & Bills. Test Accounting -> Expenses -> Equipment, mechanic invoices, manager wages and Back. Controller/owner transfers/shop location retain correct actions and return rows; do not relocate the shop merely to test browsing.
4. Open each of six Placement categories and return with both visible Back and keyboard/controller Back. Confirm highlighted parent selection. Repeat via Customers > Placement Settings and Employees > Employee Settings > Placement Settings. Return to their original rows.
5. Customer category: all six physical points, existing bay link and all three walking stops. Employee category: break, spawn/entry, desk/workstation and approaches, bay shortcut. On disposable data set one of each type: walk, face, Save Position + Facing, inspect stored heading/coordinates, Back and reopen. Confirm clear/save restrictions while open/active orders still apply.
6. Each Bay 1/2/3: vehicle vs work point remain distinct. Edit safely with shop closed and no active order. Compare AUTO LOT and CUSTOM modes; fixed assignments/vehicle transfers ON/OFF must not change from viewing/editing unrelated points.
7. Delivery: Mule spawn/unload/handoff and three driver stops. Pallets: purchased slots only; no purchase just from browsing. Equipment: browse and return to Delivery, then use financial access and return to Expenses. Existing supplier/wholesale restrictions preserved.
8. Routes: every optional stop list and final-bay approach; legacy checkpoint and job-route selection. Back from editor -> exact list/row -> Routes -> hub. Receptionist/manager/customer approaches are in location categories. Existing route mode toggles remain operational in their previous menus.
9. Markers: toggle ON/OFF and legend, then close F2 and walk; visual preview behavior unchanged. No saved point disappears from hiding markers. Existing AUTO LOT preview independent; no new persistent display entity.
10. Keyboard/controller: Up/Down wrap, one action per Enter/Accept, held/repeated input and frontend echo, Back selection restoration. Esc and Start open native pause and hide overlay; reopen via F2 without stale hub returns. Close/reopen from a nested point, navigate through other Management sections and verify correct parents.
11. Save/reload on disposable matching data: all edited and untouched positions/headings preserved. Regress customer queue, mechanic/receptionist/manager behavior, transfer ON physical and OFF one-shot placement, exactly-once repair/supplier/parts payments, wages, nine-box pallets, mixed cart and finance views.
12. Pause during work/supplier ETA and leave GTA closed: no paused/offline catch-up. Longer GH + LS RED observation: preserve logs, exact source/runtime identity and separate crash-family evidence. No claim of Issue28/crash resolution.

## Tradeoffs and review state

This is menu consolidation, not a new placement engine or arbitrary mechanic assignment. Existing operational route and supplier settings retain familiar shortcuts where removing them would shift unrelated controls. Employees/Customers placement entrypoints are aliases to the central hub; shared category pages retain a few useful internal cross-links. Marker filtering and AUTO LOT preview duplication omitted. Equipment page intentionally includes existing purchases alongside placement; its original accounting path remains.

No in-game screenshots produced; structured menu listing above reflects source. No GTA/Windows execution, live installation, sole-save access, release, merge, protected-branch change or speculative crash fix. Draft PR attempt/compare fallback is recorded in REVIEW_HANDOFF.md.

## Recorded automated results

Genuine SHVDN3 3.6.0 net48 / C#7.3 Release: PASS, 0 errors; inherited warnings CS0169 and CS0649. SDK 8.0.426.

556/556 behavior tests PASS: 85 inherited issue/logger/migration checks plus repair 32, sales 29, pallets 25, wages 22, reload 41, supplier 37, range 40, menu 13, cart 32, pause 9, focus 8, finance 33, startup 18, hardening 9, bays 34; navigation 89. Static 21/21 PASS; focused menu-only scope/save/v2/physical-helper preservation PASS. The new navigation tests use production members/case arms with deterministic adapters, not GTA.

Candidate source SHA256: 4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71. Full diff verified to reproduce this source from d980be8.

GTA testing NOT RUN. Windows execution NOT RUN. Enhanced runtime compatibility NOT VERIFIED. Crash cause UNATTRIBUTED. No release archive, runtime deployment, save modification or merge was produced by this task.
