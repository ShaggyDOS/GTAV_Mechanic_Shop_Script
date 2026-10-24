# Glass Heroes Business v0.3.3.6 — Issue #36 maintenance review

EXPERIMENTAL — PENDING OWNER ACCEPTANCE. Report date: October 24, 2026. GTA testing: NOT RUN. Windows execution: NOT RUN. This is a review candidate, not an accepted release or a crash fix. The stack-overflow cause remains UNATTRIBUTED.

## Identity and approvals

Branch: maintenance/v0.3.3.6-issue36

Pinned RC1 base: c8617aed5cda492214bbbe1907042bcafbfa0dc1

Final source/four-fix checkpoint: e4860da7accc386ab4b0d36c9c85b0cc4c55f7b7

Source SHA256: 7ba7b6021eead8f1e972b92a382a477045df083ea76b0148b84124e8725ef2ca

The package's FINAL_HANDOFF.md and FINAL_HANDOFF.json record the exact final branch tip including the subsequent documentation commit. This tracked guide names the code checkpoint to avoid a self-referential documentation commit hash.

Gate 1: owner YES in the current conversation, before implementation. APPROVALS.md supersedes the historical awaiting-approval wording in APPROVED_PLAN.md. Gate 2: PENDING owner YES/NO after reviewing this candidate. No acceptance, merge, deployment, tag or v0.3.4 integration has been performed.

Individual fix commits, in order:

```
4ad2fc4c344bafff0c098ef17ce06ff2145affb8 fix(v0.3.3.6): fit and cache readable menu footer lines (#36 A)
15467753fc4a7d376e10d57e1d9ab649e5e6a7aa fix(v0.3.3.6): distinguish shared mechanic stops from route selections (#36 B)
21d865c50493bbc233baed6135ffe18f241bbed1 fix(v0.3.3.6): own ready wholesale map locator and explicit GPS (#36 C)
e4860da7accc386ab4b0d36c9c85b0cc4c55f7b7 fix(v0.3.3.6): share canonical staff route editors (#36 D)
```

## A — footer descriptions

The previous fixed-character split could overflow proportional-font text, leave a single trailing word and crowd the control hints. WrapMenuDescription now uses measured font widths and a bounded cached LayoutMenuDescription/FittingMenuWords helper. It balances two fitting word lines, normalizes whitespace/newline tokens and uses a word-boundary ellipsis if two lines are insufficient. Oversized input and tokens are bounded safely. DrawMenu preserves the 420-pixel panel, 119-pixel footer, font, colours, rows and hint/counter positions. Description baselines are spaced 23 design pixels; single-line help is centered vertically. The generic Back hint is shorter.

Placement uses the existing SHVDN 1280x720 reference space at MenuScale; no resolution query or wrapping calculations occur for an unchanged cached description. Cache capacity is 128; input is bounded to 1024 characters and 96 words; fitting uses binary search. GTA font/native rendering, safe-zone, 720p/1080p and overlay readability still need owner observation. No screenshots were inspected by Codex.

## B — mechanic terminology

Shared Entry Corridor means the existing physical entry path, also used by the existing optional car pickup/return walking toggle. Shared Work Stops means the five shared workstation-access nodes. Workstation Approaches identifies each separate mechanic final approach. Selections by Mechanic / To Workstation / From Workstation identify per-worker masks selecting those shared work stops. These do not rename or move the underlying saved data.

MechanicRouteCounts reports selected, physically saved and unset counts. MechanicRoutePreview displays the existing ascending outbound and descending return order, with the approach/target; DIRECT mode marks custom selections inactive. MechanicRouteHeading fits direction-first text without hiding direction. Preview is read-only and never calls BuildRoute, Save or pathfinding. The 16 route cases include 6144 mask/saved-mask/direction/worker combinations; these combinations are inside those cases, not 6144 additional tests. AI, traversal order, geometry, bays and waypoint masks are unchanged.

## C — READY physical wholesale locator

After startup, SyncWholesalePickupBlip runs in the existing active, unpaused one-second supplier refresh. An actual wholesale order with stock, finished ETA, stage 7, a valid existing warehouse point and no carried/loaded/unloaded cargo owns one long-range yellow Package blip named Glass Heroes — Wholesale Parts Pickup. It remains useful outside the prop streaming radius; no actor, model or distance scan is required. Transit and manager deliveries do not receive this locator.

Active Deliveries adds Set Warehouse GPS at row 4, before Back. Existing action rows 0–3 keep their behavior. GPS is explicit and READY-only: it enables the owned blip's ShowRoute without setting, deleting or restoring the player's waypoint. A later changed player waypoint yields the owned route at the next active check. Ordering/READY do not automatically seize GPS. Existing post-collection return-to-shop navigation is unchanged.

Three creation attempts are spaced five active seconds apart, followed by a 30-second cooldown and recovery window. Logging/setup messages are rate limited to 30 active seconds. Valid handles are reused; partial configuration is cleaned. Failed deletion retains ownership and blocks duplicates, with at most two delete attempts per synchronization. Successful collection/loading, cancellation, recovery, abort and invalid/completed order state clean only the owned locator. The READY saved order reconstructs the locator after the existing startup gate; GPS choice is session-only and starts off after reload. The native engine can still fail to create/delete a blip; adapters test these failures but do not prove GTA cleanup behavior.

Collection remains entirely player-operated. No new function bills, stocks inventory, collects cargo, moves staff or saves business data. Existing depot pickup point, world marker, physical props and accounting remain unchanged. Missing/nonfinite locations get setup help rather than invented coordinates.

## D — one staff-route editor per role

Placement Settings > Walking Routes, role-management and Employee Walking Paths shortcuts now open the existing ReceptionWalkingPaths or ManagerWalkingPaths canonical page. Each retains walking mode, five saved stops, final approach and Back. Redundant visible wrapper steps are bypassed; internal enum aliases are retained for compatibility. Existing OpenPlacementMenu context records the real caller/row and the child-list return, replacing stale menu context. NavigateBack and PointEditor themselves remain unchanged.

Receptionist Walk Route and Manager Walk Route titles/help identify the same settings from every shortcut. Mode guards, Save/Clear actions and coordinates/headings are unchanged. The navigation suite retains its original 180 checks with only approved destinations/row expectations updated and adds 28 cases for role origins, all stops/approaches, explicit save, modes, stale reopen context, keyboard/controller/mixed input and pause.

## Exact source-level scope and data compatibility

The only gameplay source changed is experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. The Visual Studio project still links this exact install source; project/solution and original v2 sources are byte-identical to RC1. SOURCE_EXCERPTS.md and source.diff contain before/after evidence and the complete unified source diff; applying it to the pinned base reproduced the source byte-for-byte.

Changed existing members:

CancelSupplierOrderWithConfirmation, DrawMenu, EditingPointPurpose, ExecuteEmployeePathsSelection, ExecuteManagerManagement, ExecuteManagerWalkingSelection, ExecuteMechanicWalkingSelection, ExecutePlacementSelection, ExecutePointListSelection, ExecuteReceptionWalkingSelection, ExecuteReceptionistManagement, ExecuteRouteDirectionSelection, ExecuteRoutePointSelection, ExecuteSelection, GetMenuItemValue, GetMenuItems, GetMenuTitle, GetPageDescription, GetSelectedDescription, OnAborted, OnTick, RecoverSupplierShipment, SetSupplierPickupWaypoint, ShowSupplierDeliveryStatus, TryPlaceSupplierOrder, TrySupplierPackageInteraction, WrapMenuDescription.

New members:

DeleteWholesalePickupBlip, FittingMenuWords, LayoutMenuDescription, MechanicRouteCounts, MechanicRouteHeading, MechanicRoutePreview, NotePickupLocatorFailure, PickupWaypointChanged, PickupWaypointSnapshot, ReadPickupWaypoint, SyncWholesalePickupBlip, WholesalePickupLocationValid, WholesalePickupLocatorEligible.

The guard confirms 376 existing members and all other existing root declarations/defaults are unchanged. Only approved presentation caches and owned-locator session fields are added. All GetValue/SetValue expressions are identical. Save.ini and Settings.ini have zero new keys, defaults, schema changes, migrations, coordinate rewrites or mask resets. No existing user save/settings file was read or modified. Synthetic data is used only by tests. Financial transactions, inventory transfers, pending-order identity, invoicing, employee AI, route geometry, active-unpaused clocks, constructor/startup defaults and controller arbitration remain unchanged. The guard also checks every allowed locator hook is outside the protected transaction logic.

## Recorded compilation and regression results

Actual clean Release compilation: PASS, 0 errors, two inherited warnings CS0169 and CS0649. SDK 8.0.426; .NET Framework 4.8; C# 7.3; official ScriptHookVDotNet3 package pinned to 3.6.0. This is real product compilation, not a stub-only build. Warning lines are in D-canonical-roles/build.txt.

Behavior/navigation: 854/854 PASS, compared with independently reproduced RC1 baseline 762/762. Static: 21/21 PASS. Approved-scope/preservation guard: PASS. New cases: footer36 15, routes36 16, locator36 33, plus 28 navigation cases. Final navigation: 208/208. Inherited issue/logger/migration checks: 85. Other feature suites: repair 32, sales 29, pallets 25, wages 22, reload 41, supplier 37, range 40, menu 13, cart 32, pause 9, focus 8, finance 33, startup 18, hardening 9, bays 34, workflow335 38.

Behavior tests extract actual production members/case arms and compile/run them against deterministic adapters on .NET 8. They cover accounting/clock/input/native-failure behavior but do not execute GTA natives or prove real GPS rendering, controller hardware, collision, MLO navigation or crash stability. Static checks and preservation are separate from the 854 behavior count. Every stage's actual logs are retained; D-canonical-roles/validation.json is the final source's result. No identical gameplay suite rerun was needed for documentation-only changes.

Reproduce from the Git branch with Python 3 and .NET SDK 8 available (configured cloud shell: source /workspace/.cloud-setup/env.sh):

```
python verification/maintenance36/verify.py --stage reviewer-rerun --extra footer36 routes36 locator36
```

The complete verifier requires the pinned Git base and historical validation/protected files. The review ZIP includes source/project, tests, historical baseline text, evidence and diffs; clean product rebuild does not require Git history. Product build:

```
dotnet build experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release
```

A .NET 8 SDK supplies build tooling and test execution; the product remains net48. The package extraction receipt records a separate clean product build from the archive. No compiled DLL is supplied for simultaneous installation with the source script.

## Review package, installation and rollback

This package is prepared for code review before acceptance. It has one active GlassHeroesBusiness.3.cs under experimental/shvdn3/Install/scripts and the matching linked Visual Studio project, documentation, tests and actual logs. No Save.ini, Settings.ini or alternate runtime script/DLL is included. Historical source fixtures are text, not installable scripts. Do not copy the entire archive into GTA.

No installation is performed or authorized by this report. If the owner later accepts and authorizes testing: close GTA; back up the exact currently tested source and complete matching Glass Heroes data/settings directory together; use a disposable copy of the data for first tests; install only the reviewed .3.cs using the owner's existing SHVDN3 setup. Ensure only one Business Manager generation runs; keep older .2.cs scripts or compiled Business Manager DLLs outside the active scripts folder. Do not overwrite saved data/settings with examples. Record archive/source hashes and runtime/dependency/mod-stack versions.

Rollback with GTA closed: restore the previously tested source and its complete matching data/settings backup together. Keep copies of both pre-test and post-test data for investigation; never reset or merge sole live saves to roll back code. Source does not add a migration, but gameplay transactions during testing can change balances/inventory normally. Report any missing shipment, double invoice, changed coordinate or pause/offline advancement immediately with copies and logs.

## Integration ownership

The separate v0.3.4 instance owns integration only after explicit owner gate 2 YES. This branch contains no v0.3.4 features. Do not overwrite its newer monolithic source with this RC1-based file. Review the four deltas against its current source, resolve shared menu/wholesale overlaps deliberately, rerun its complete build/regressions and obtain its review. Test fixture adapter changes preserve prior assertions and must accompany the corresponding hooks if needed. Do not merge overlapping historical QA/issue PR sets independently. This instance has not changed or merged main, RC1, QA R2, v0.3.4 or live GTA.
