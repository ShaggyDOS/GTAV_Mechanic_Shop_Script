# Codex handoff — Issue #30 — Central Placement Settings + finance menu dedup
Status: OWNER APPROVED FOR IMPLEMENTATION. No code edits or GTA testing have been performed on this branch at handoff creation.

Repository: `ShaggyDOS/GTAV_Mechanic_Shop_Script`
Issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/30
Approved dedicated branch: `feature/shvdn3-placement-settings-hub`
Base: `feature/shvdn3-29-service-bay-integration` at commit `d980be89f387588c02596e8f1ed7732a05ede9d8` when this branch was created.
Important: Check the owner's actually installed and working RC1 script hash against the branch source before building; a GitHub tag `v0.3.3.4-rc1` was NOT independently verified. Do not assume main is current.

## Assignment
Proceed with a minimal, reviewable **menu-only** implementation in two independent commits; no new placement technology, movement, state-machine, money, save, rendering or timer algorithms.

### Commit A — remove duplicate financial navigation
Remove the direct `Business Management > Expenses & Bills` entry ONLY. Preserve `Business Management > Finances & Accounting > Expenses & Bills` and every payment/finance/equipment action and transaction. Update the positional routing throughout the menu implementation, including:
- `GetMenuItems`, `ExecuteManagementSelection`, `NavigateBack`
- `GetMenuItemValue`, `GetSelectedDescription`, `GetMenuTitle`, `ExecuteSelection`
- Every hard-coded selection index or return position affected by deletion.
In particular preserve access to equipment purchases now reached via the Expenses submenu; do not delete, rename or alter its logic. No financial journal or report changes. Exhaustively exercise menu selection/Back, not only compilation.

### Commit B — Placement Settings hub
Add `F2 > Business Management > Placement Settings` with:
1. **Customer Locations:** drop-off, road spawn, waiting car 2/3, customer reception approach and queue, customer walking stops.
2. **Employee Locations:** staff break, spawn/entry, receptionist desk/approach, manager workstation/approach.
3. **Service Bays:** route to the existing Bay 1/2/3 profile (distinct saved vehicle position/heading and mechanic work point; no collapsing coordinates or altering mechanic slot ownership).
4. **Delivery & Stockroom:** Mule spawn, unloading spot, handoff, existing pallet points, supplier driver stops and other existing owner-editable physical points.
5. **Walking Routes & Approaches:** reuse existing mechanic/receptionist/manager/customer/supplier optional waypoint editors; retain their existing route-mode and operating toggles elsewhere.
6. **Marker Display Settings:** use existing `_showPlacementMarkers`, global color legend, optionally link existing AUTO LOT preview; no new expensive rendering or persistent objects.

**Input:** Keep the current keyboard/controller F2 point editor, walking to a point, facing the desired heading, then selecting Save Position + Facing. Preserve native pause, controller debouncing, Back navigation, return selection and saved coordinate/heading behavior. If trivial, surface saved/not-saved status, heading and coordinates using existing values and notifications; do not add a new free-camera placement/dragging mechanism.

Avoid duplicated placement editor entrypoints in Employees/Customers and other menus if straightforward to remove; preserve operation controls (employee training/hiring, customer service toggles, supplier orders, route modes). Ensure new menu navigation never gets stuck or routes to the wrong parent after `OpenPointEditor`. Do not confuse *visual placement markers* with *saved placement points*.

### Current source guide
- `experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs` on Issue #29 branch.
- `MenuPage`, `GetMenuItems`, `ExecuteSelection`, `ExecuteManagementSelection`, `NavigateBack`, `GetMenuItemValue`, `GetSelectedDescription`, `GetMenuTitle`.
- `ExecuteCustomerPlacementsSelection`, `ExecuteEmployeePlacementsSelection`, `ExecuteEmployeeSettingsSelection`, `ExecuteMarkerSelection`, `OpenPointEditor`, `DrawPlacementMarkers`, supplier/pallet/route handlers.
- Existing source has `BusinessManagement` entries `Expenses & Bills >` and `Finances & Accounting >` adjacent (selection indices 3 and 4); `NavigateBack` hard-codes several positional indices. Treat those as regression risks.
- `DrawPlacementMarkers` already renders temporary customer/staff/vehicle bay/supplier visuals when enabled; reuse existing marker logic. Automatic-lot preview is independent from regular markers.
- Issue #29 `ServiceBayProfile` already groups two saved coordinates without combining them.

### Non-negotiable protections
Preserve all Save.ini and Settings.ini keys and values, coordinates, headings, financial records, progress, manager activity, unpaused-only business clock, and customer vehicle transfer ON/OFF state machine. Preserve rest of latest financial/supplier features. No new threads, blocking waits, expensive per-frame calls or live GTA installation edits; do not touch investigation Issue #28 or perform unrelated v0.3.3.5 feature work. Do not merge or release without review.

### Verification and handoff
Build the **actual** SHVDN3 net48/C#7.3 source project and run its inherited automated feature/static suites. Add focused menu navigation tests for every entry, Back, selection restoration, controller, point-editor return target, marker toggle and financial dedup; verify no hidden actions become inaccessible and no unexpected Save calls when merely browsing. Confirm untouched saved locations/headings under load/reload and unchanged business finance results. Mark game testing **NOT RUN** until owner tests LS RED + GH.

Report baseline/feature commit SHA, changed methods, test counts, compiler warnings, save schema changes (expected NONE), unresolved UX tradeoffs, review link/draft PR (if permissions allow), and manual GTA checklist. Work only on `feature/shvdn3-placement-settings-hub`.
