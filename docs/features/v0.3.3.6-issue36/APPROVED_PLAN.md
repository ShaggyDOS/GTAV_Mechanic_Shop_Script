# v0.3.3.6 Issue #36 maintenance proposal — AWAITING OWNER YES/NO

Date: October 24, 2026 (America/Chicago). Assignment: PLAN AND REPORT ONLY. No product code, active RC1, saves, settings, live installation, or v0.3.4 branch was modified. No maintenance branch, commit, PR, build or release was created.

Issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/36

## Source and evidence

Review base is the owner's tested v0.3.3.6 RC1 commit c8617aed5cda492214bbbe1907042bcafbfa0dc1, not the older v0.3.3.1 checkout currently in the workspace. Its sole SHVDN3 installation source is experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs (SHA256 7fff30978e2dc4c3235656f6ca6865506d5cea53387b7eb6d83654d71c8a9d6b). The linked project is experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj.

I read Issue #36, that exact source, the branch AGENTS.md, v0.3.3.6 handoff and recorded validation/test workflow. Issue #36 describes screenshots IMG_0750.jpeg through IMG_0744.jpeg; those images are not present here and were not visually inspected. The findings below distinguish source-confirmed behavior from screenshot observations reported in the issue. Screenshots from different saves/sessions are not used to infer economic defects.

The checked-in RC1 validation reports a real net48/C#7.3 build against ScriptHookVDotNet3 3.6.0, 762 behavior cases including 180 navigation cases, and 21 static checks. Those are historical results, not tests rerun during this planning assignment. No new compilation, regression run or GTA test was performed.

After owner YES only, create a separate worktree/maintenance branch, proposed name maintenance/v0.3.3.6-issue36, from the exact RC1 commit. Editing that isolated copy must not move or overwrite the RC1 branch/source snapshot. Keep feature/shvdn3-340-inventory-procurement (observed head dcc38f58aa2a31f1eae4030a902c15281d934d60), main and other accepted snapshots untouched. Do not merge or cherry-pick maintenance into v0.3.4 before owner acceptance.

## A — Footer wrapping/readability (P1)

Source-confirmed: WrapMenuDescription at line 2247 uses a 53-character split, may hard-split a word, then cuts the second line with an ellipsis. DrawMenu at 2263 uses a 420-design-pixel panel and a 119-pixel footer, font scale 0.30, description offsets +10/+40 and hint offset +86. Character count does not establish rendered pixel fit. Existing FitMenuText already uses SHVDN3 ChaletLondon width measurements and a bounded 128-entry cache for row columns; it need not be changed globally.

Proposed behavior:

- Preserve panel position/width/scale, header, colours, row heights, selection appearance and footer control hints. This is a footer correction, not another redesign.
- Wrap within the actual description content width (panel width minus left/right padding), using the same font, font scale and MenuScale as drawing. Normalize whitespace and honour natural word boundaries; do not leave an extra renderer-generated wrapped fragment below an explicitly wrapped line.
- Keep two deliberate description lines. Prefer balanced, useful line lengths; use consistent font-appropriate leading within the current footer help band and retain separation from the hints/page counter. Avoid the current oversized fixed gap.
- Shorten the affected row-specific descriptions where useful. Fit both lines before resorting to a word-boundary ellipsis on the final line. A single unbreakable token that cannot fit gets a safe ellipsis rather than a midword fragment or overflow; required route/action meaning must remain in the shortened help.
- Cache layouts by description, available width, font/scale and relevant resolution/layout identity. Measure only on a cache miss and keep cache size/work bounded. Repeated drawing of an unchanged selection performs zero additional native width measurements. No per-frame layout search, unbounded cache, world scan or file I/O.

Exact product scope: replace WrapMenuDescription policy/signature as needed; adjust only description layout/calls in DrawMenu; shorten relevant GetSelectedDescription/GetPageDescription entries. Add small bounded footer-layout/cache helpers/session-only fields. Reuse current text primitives and width API. Do not globally change Label, LabelColumn, Panel, CompactX/CompactY, menu branding or input handling.

Acceptance: both lines fit, use natural breaks, have readable spacing and do not collide with hints/counter at 720p/1080p. Empty/one-line/two-line/long/format-code/long-token inputs are safe; unchanged selections reuse the cache. Actual font/overlay appearance still needs owner GTA screenshots after implementation.

## B — Mechanic route terminology and mapping (P1)

The route systems are already distinct in code:

| Physical data | Existing use | Proposed UI vocabulary |
|---|---|---|
| _arrivalRoute[0..4] | Spawn to break, and when _pickupUsesArrivalExit is enabled: reverse for break-to-customer-car, forward for drop-off-to-break | Shared Entry Corridor / Pickup & Return Corridor |
| _serviceRoute[0..4] | Five shared physical workstation-access nodes | Shared Work Stops 1–5 |
| Mechanic.JobOutboundMask / JobReturnMask | Per-mechanic selection of those shared work stops; outbound increasing indices, return decreasing indices | To Workstation / From Workstation — stop selection |
| _bayApproaches[0..2] | Separate final approach for each assigned bay/workstation | Workstation Final Approach (Bay N) |
| _bays vs _vehicleParkingBays | Mechanic animation/work position versus car parking | Keep distinct workstation and vehicle-parking terminology |

The pickup corridor toggle is independent of workstation mask selection; do not imply that workstation masks control the initial car pickup or vehicle driving. Describe each mode gate accurately rather than labelling every corridor use as dependent on the same CUSTOM toggle.

Proposed presentation:

- Mechanic/direction pages identify the mechanic and To/From Workstation; fit long names within the existing header rather than widening the menu.
- Each mask row names Shared Work Stop N. Its value clearly separates USE/SKIP (selection) from SET/UNSET (physical position), using compact column-fitting words.
- Direction summaries show selected count, saved-and-selected count and selected-but-unset count. Separately report whether the custom work-route mode is active; a saved selection is not necessarily currently traversed in DIRECT mode.
- Provide a read-only numbered route summary in the existing summary action, with outbound/reverse order and the existing approach/target. Use the actual saved nodes and mask intersection. Do not call BuildRoute for a menu preview: that method also issues diagnostic warnings. Use a pure formatter over existing fields/EffectiveWorkRouteMask.
- Point-editor labels explicitly identify Shared Work Stop N, shared corridor stops and bay-specific approaches. Explain that enabling an unset point does not create coordinates.
- Browsing, summaries and previews never call Save. Existing explicit mask toggles and explicit Set/Clear point actions retain their current meaning and closed-shop/active-order safeguards.

Exact product scope: relevant arms/strings of GetMenuItems, GetMenuItemValue, GetMenuTitle, GetSelectedDescription, GetPageDescription and EditingPointPurpose; labels only in ExecutePointListSelection; summary text in ExecuteRouteDirectionSelection; post-toggle wording only in ExecuteRoutePointSelection and relevant notifications in ExecuteMechanicWalkingSelection. Add pure display/count/summary helpers if needed, reusing EffectiveWorkRouteMask without changing its result. ExecuteRouteRosterSelection already sets the mechanic slot and need not change its behavior.

Frozen: BuildRoute, BuildMechanicPickupExitRoute, BuildMechanicReturnFromDropoffRoute, route ordering, masks and bit positions, saved coordinates/headings, bay assignment, movement/recovery/native tasks, valet driving/parking and paid-car handling. No automatic selection, point creation, reordering or data migration. Keep the existing XOR/all/none mutation and Save logic in ExecuteRoutePointSelection unchanged.

Acceptance: all 32 mask values against sparse/set/unset shared points display the correct selected/effective/missing relationship; both directions and all mechanics map to the same five physical nodes. Route output remains identical to RC1 for identical inputs. No customer/worker movement changes are permitted to make labels appear correct.

## C — READY wholesale pickup map blip and explicit GPS (P1)

Source-confirmed: SetSupplierPickupWaypoint at 9292 calls SET_NEW_WAYPOINT once after purchase. DrawWholesalePackageMarker at 7751 shows only a stage-7 ground marker within 45 m. SyncCustomerBlip creates customer-vehicle blips, not a wholesale locator. The RC1 constructor currently initializes the existing warehouse WorkBay from its fixed depot constants; this maintenance must consume that existing runtime location unchanged, not alter its saved/default-location policy or invent another coordinate.

Recommended policy for approval:

- Automatically show ONE named long-range map blip, Glass Heroes — Wholesale Parts Pickup, only for an actual outstanding wholesale shipment with positive quantity, ETA finished, stage 7, a valid configured warehouse point, and no carried/loaded/unloaded cargo. No blip during transit, for local/express orders, or after collection. Its presence does not depend on prop/model streaming, player distance, shop opening or manager availability.
- Use the warehouse point already held by RC1. Missing/nonfinite location suppresses creation and gives a clear, rate-limited setup/status message; do not write a fallback coordinate. Do not put coordinates/private addresses in new logs.
- Use existing SHVDN3 World.CreateBlip, BlipSprite.Package, Name, IsShortRange=false and ShowRoute APIs (confirmed present in the pinned API metadata). Own one handle per pending order identity and position. Reuse it while valid, recreate only when necessary, and bound failures/retries with a cooldown and rate-limited error logging.
- GPS is EXPLICIT: append Set Warehouse GPS before Back in Inventory & Suppliers > Active Deliveries, preserving existing status/contents/recover/cancel action indices. Enable route drawing only for this owned ready-pickup blip after that action. Do not auto-overwrite the player's waypoint when ordering or becoming READY.
- Preserve the player's waypoint itself. Snapshot its handle/presence/position at explicit GPS selection using World.WaypointBlip, and yield/disable the owned route if the player subsequently changes their waypoint. Check only at the existing throttled update rate while this route is active. Do not use ground-search waypoint APIs, restore old waypoints, clear all routes, or refresh a global waypoint every Tick.
- Remove the owned blip/route on successful collection/loading, successful cancellation, invalid/completed order, recovery reset, location change and abort. Recreate at most one from a still-valid READY saved order after the normal post-load gameplay gate. A paused frame does not create/advance work; existing locator visibility may remain in the pause map.
- Keep map lifecycle independent from pallet/prop streaming. Do not tie locator destruction to DeleteSupplierVisuals or distance-only cleanup, which could cause flicker/recreation when an actor streams out. Keep the current proximity-limited world marker.
- Blip/GPS functions never collect cargo, move vehicles/peds, alter ETA or issue stock/invoices. Manager automation cannot perform wholesale collection. Existing post-collection return-to-shop navigation/physical interaction remains outside this change except owned-locator cleanup.

Proposed new helpers/session fields: pure WholesalePickupLocatorEligible; SyncWholesalePickupBlip; DeleteWholesalePickupBlip; a small session-only waypoint snapshot/changed check; owned Blip, bound order identity/location, route-request flag and bounded retry state. No new saved key or setting.

Exact existing call sites allowed:

| Method | Permitted change |
|---|---|
| OnTick | One synchronizer call after ProcessSupplierDelivery in the existing approximately one-second refresh block, post-boot and pause-gated; no changes to clock sampling or progression |
| TryPlaceSupplierOrder | Replace only the successful post-save automatic waypoint side effect with owned-locator reset; preserve the entire financial transaction/rollback body |
| SetSupplierPickupWaypoint | Reuse/refactor as the explicit ready-only owned-GPS action; remove its unsolicited global-waypoint behavior |
| ExecuteSelection: ActiveDeliveries arm | Dispatch the appended explicit GPS action; retain existing actions/back |
| GetMenuItems/GetMenuItemValue/GetSelectedDescription | Add the action, readiness/unavailable value and help |
| TrySupplierPackageInteraction | Owned-locator cleanup only after successful wholesale collection/loading; no transfer/accounting edits |
| RecoverSupplierShipment | Reset only locator/session routing alongside existing order recovery; next valid READY sync may recreate it |
| CancelSupplierOrderWithConfirmation | Cleanup only after successful committed cancellation; failed/cancel-first-press leaves valid locator intact |
| OnAborted | Delete only this owned locator/route after existing pre-activation guard |
| ShowSupplierDeliveryStatus | Ready/GPS/setup help only; no coordinate dump or bookkeeping changes |

Frozen: ProcessSupplierDelivery business/stage logic, CommitSupplierDelivery, TryCancelSupplierOrder transaction, ApplySupplierInventory, supplier pricing, Save payload/schema, cargo/vehicle policy, DrawWholesalePackageMarker, SyncCustomerBlip and generic DeleteSupplierVisuals. Do not import an old wholesale-blip branch wholesale or touch v0.3.4 procurement.

Acceptance: before ETA/READY none; READY anywhere on map exactly one; repeated sync no duplicate native creation; loaded/carrying/cancelled/completed none; failed collection/cancellation retains locator; recovery/load recreates only from a valid original order. Model failures and distance do not erase the map locator. Only explicit GPS enables the owned route; changing the player's waypoint yields it, and teardown never clears unrelated waypoints/blips/routes. Test failed creation/deletion and retry bounds. Cash, stock, invoices, identity and delivery clock must be unchanged by every locator-only operation.

## D — One canonical receptionist/manager route editor (P2)

Source-confirmed: ReceptionPlacementRoutes and ManagerPlacementRoutes duplicate access to the already-existing ReceptionWalkingPaths and ManagerWalkingPaths pages. OpenPlacementMenu already records caller page/selection in _placementMenuReturns; NavigateBack consumes that context before legacy role flags. This provides the existing mechanism to retain sensible Back navigation.

Proposed navigation:

- Placement Settings > Walking Routes > Receptionist goes straight to the existing ReceptionWalkingPaths canonical page.
- Placement Settings > Walking Routes > Manager goes straight to ManagerWalkingPaths.
- Employees > Receptionist/Manager > Walking Paths and Employee Settings > Employee Walking Paths shortcuts lead to those SAME canonical pages.
- Each canonical page keeps Walking Mode, five role-specific stops, final approach and Back. There is one editor/state per role, not a copied configuration.
- Use OpenPlacementMenu consistently for canonical entry and child stop lists so the current caller/row overwrites stale return contexts. PointEditor continues using its existing return fields. Back from a stop/approach returns to the canonical page; Back there returns to the actual originating menu/row, including after repeated visits or closing/reopening F2 from another entry path.
- Bypass/retire the redundant visible wrapper screens. Internal enum aliases can remain for compatibility; no reason to renumber serialized concepts or replace any saved route data. Preserve fallback navigation paths where still used.

Exact product scope: role destinations in ExecutePlacementSelection; walking-path shortcuts in ExecuteReceptionistManagement, ExecuteManagerManagement and ExecuteEmployeePathsSelection; child-list navigation in ExecuteReceptionWalkingSelection/ExecuteManagerWalkingSelection; related GetMenuItems/GetMenuTitle/GetSelectedDescription/GetPageDescription arms and, only where stale role-return arms require it, NavigateBack. Reuse OpenPlacementMenu/_placementMenuReturns; no global input rewrite. Mode toggles, explicit Save/clear behavior and all role path fields remain unchanged.

Frozen: receptionist/manager movement, UpdateVisualReceptionist, UpdateVisualManager, saved _receptionRoute/_managerRoute and final approaches, CustomReceptionistPaths/CustomManagerPaths semantics, _receptionPathsFromManagement/_managerPathsFromManagement persistence (these are runtime context, not new INI settings), hiring/wages and placement guards. No settings are lost because a redundant menu is removed.

Acceptance: test both entry points and legacy shortcuts for both roles; all five points, mode toggle, final approach, Save/Clear, Back, reopen, cancelled editor and paused/native menu handoff. Navigation alone produces no data write. Keyboard/controller and mixed-input debouncing remain intact.

## Protected source/data contract

Executable edits are limited to the isolated maintenance copy of the single SHVDN3 source and the method/arm allowlist above. Add focused tests and maintenance docs under new issue-specific paths; leave the linked project/runtime target intact unless only its isolated test-output configuration needs adjustment. No second active business script, no installation, no tag or release.

Keep existing Save/Settings paths, INI key names/defaults, LoadPoint/SavePoint and complete save value expressions unchanged. All route masks/points/heading data, finances, active-unpaused clocks, accepted repair persistence, customer paid recovery, remote business, retail transactions, supplier order/invoice/cargo semantics and controller settings precedence are protected. Tests compare hashes/method bodies or normalized ASTs, and synthetic save snapshots, before/after. Browsing/rendering/new blip/GPS actions must not call Save.

Four focused commits after YES: A footer; B route vocabulary/read-only mapping; C owned locator/GPS; D canonical staff navigation. Keep each independently reviewable. If implementation reveals a required edit outside the allowlist, report it for scope approval rather than slipping an AI/financial/data change into this maintenance.

## Verification and acceptance gates

After implementation only:

1. Real Release compilation: existing net48/C#7.3 project, pinned ScriptHookVDotNet3 3.6.0, report actual warnings/errors.
2. Run the complete existing behavior/logger/migration/finance/supplier/pause/recovery/startup/retail/demand/navigation suites. Retain RC1 coverage, update only approved changed-label/navigation expectations, and report actual counts rather than recycling RC1's 762-case claim.
3. Add focused footer measurement/cache/ellipsis tests; mechanic mask-to-shared-node and read-only-summary tests; locator readiness/ownership/retry/waypoint/cleanup tests; and full canonical-entry/Back/controller navigation tests. Repeated UI/locator operations must not alter saved financial or placement state.
4. Assert RC1 movement/route builders and transaction bodies are unchanged except the precisely allowed presentation/locator call-site additions. Verify zero schema migration and no new per-frame I/O/scans/native width queries. Scope validation output must go to a new maintenance report directory: the RC1 workflow336 verifier hardcodes docs/features/v0.3.3.6, so do not overwrite those baseline artifacts when adapting it.
5. Prepare a review-only matching-source/project ZIP, full focused diff, hashes, changelog, versioned Markdown/DOCX/PDF developer report, navigation matrix, rollback and owner GTA check sheet. Mark every unavailable test NOT RUN. Do not install it.
6. Owner GTA QA: 720p/1080p, same layout scale/HUD overlays, longest affected footer rows, both staff entry paths, all mechanic directions and sparse saved masks, keyboard/controller/mixed input/native pause; wholesale ETA→READY→collect→load→unload→stock, cancellation failure/recovery, leave/re-enter streaming range, ready order save/reload/abort, existing waypoint changes and missing locations/model failure. Compare the same save/session, not cash totals from unrelated screenshots. Verify no regression in the owner's working mechanic movement and vehicle handoff.

FIRST approval (now): owner YES/NO to this exact maintenance plan, source base, four focused scopes and explicit-only owned GPS policy. STOP before coding until YES.

SECOND approval (after implementation/testing report): owner YES/NO before the maintenance build is accepted. Passing compilation/automated tests is not owner acceptance or GTA qualification. The separate v0.3.4 branch remains untouched until this owner acceptance; the separate v0.3.4 development instance will perform integration after owner acceptance. This maintenance instance will not merge, cherry-pick, push to, or otherwise modify the v0.3.4 branch, including after acceptance. Main and live GTA remain unchanged.

## Coordination handoff contract — owner requirement

The final implemented/accepted handoff must contain the exact maintenance branch name, full final commit SHA, ordered individual A/B/C/D fix commit SHAs and any focused correction commits, exact changed-file list against the pinned RC1 base, actual build/regression results and log locations, source/package hashes, save-compatibility/rollback notes and the recorded status of BOTH owner YES/NO gates. See INTEGRATION_HANDOFF_TEMPLATE.md for the required fields; unknown implementation values are explicitly pending, not fabricated.

The v0.3.4 development instance owns integration after owner acceptance. This instance must not modify or merge into that branch. Record dependencies and overlap for its reviewer: menu methods and supplier UI occur in the shared mono-script, so integrate reviewed maintenance deltas rather than replacing the newer inventory-development source wholesale. Keep all v0.3.4 features out of the maintenance commits. Delivery of a handoff or passing automated checks does not substitute for the second owner acceptance.

## Current status

Plan complete; implementation NOT STARTED. New build/regression/GTA testing NOT RUN. No source or save changes, no branch creation, no commits/pushes, no v0.3.4 integration. Awaiting owner YES/NO as explicitly required by the current assignment and Issue #36; this is not an inferred approval rule from a skill.
