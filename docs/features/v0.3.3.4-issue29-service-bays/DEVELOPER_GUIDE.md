# Glass Heroes v0.3.3.4 — Issue #29 Service Bay Integration — EXPERIMENTAL

Development review only. No release has been published, installed or merged. GTA validation PENDING. This feature does not resolve or attribute Issue #28, the CLR stack overflow, native startup AV or EUP file-access symptom.

## A. Reviewed baseline and preserved work

Selected base: investigation/shvdn3-financial-startup-audit at 12fbbf7 (full identity in BASELINE_REVIEW.json), source SHA256 0b14e8f9402007912b8316ecfc345a7ff9043ff866072d0429f00cc6ee76109b. It is the latest pushed FS1 source; its last commit adds QA notes without changing the FS1 implementation. The feature branch is feature/shvdn3-29-service-bay-integration. A separate integration/shvdn3-v0.3.3.4 baseline pointer starts at the same commit so a draft need not target or modify the investigation branch.

Fetched remote heads and reviewed main e13e977 (old v2 scaffold), experimental/shvdn3-economy-pallets-wages-review aad2dff, financial 0fefcf8, FS1 12fbbf7, Issue #29 and existing supplier/financial/FS1 handoff notes. GitHub API access has previously been Forbidden; public Issue #29 was read. Public PR listing attempts returned no usable PR details and a later request timed out; do not claim a complete live PR audit. A partial public read of PR #26 identified it as the earlier Scope/Manager unattended delivery review; its full live metadata could not be verified. Existing prepared PR descriptions and source commit ancestry were reviewed; no merge was inferred from a draft or pushed branch.

FS1 includes repair coverage (#27), manual retail (#25), wages, nine-box pallets, saved quotes, supplier range/workflow/mixed carts, native pause compatibility and FinanceV1. All remain. The independent WP1 branch 7deff80 and held local financial/waypoint/Mule integration a5c70f2 are preserved; their unmerged diagnostic and delivery changes are deliberately outside Issue #29, not overwritten or described as included. The separate crash investigation branch is unchanged.

Original v2 source/project, main, protected QA R2 and live GTA/data remain unchanged. Recent GH + LS RED success is encouraging owner evidence, not a demonstrated crash fix.

## B. Source changes and state transitions

Product source: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs, linked directly by the existing VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj (net48/C#7.3, pinned official SHVDN3 3.6.0). No additional runtime dependency.

Changed existing methods: AssignCustomerMechanic; ProcessOneCustomerRepair; UpdateOneCustomerOrder; NeedsDropoffSoon; IntakeInProgress; GetMenuItems; GetMenuItemValue; GetSelectedDescription; GetMenuTitle; ExecuteSelection; NavigateBack; constructor BOOT label only. New helpers: TeleportAttemptDue, TeleportTargetAvailable, ProcessTeleportOrder, OpenServiceBayProfile, ExecuteServiceBayProfile. CustomerOrder gains session-only flags/timestamp; CustomerStage appends two states. MenuPage gains a profile page; those enum values are not save keys.

Transfers ON: retain the existing assignment/quote commit, mechanic pickup walk/entry, driving/parking, mechanic exit/repair, return driving and customer pickup. Source-preservation checks pin every physical driving helper unchanged and verify the physical customer switch apart from the guarded OFF hook/invalid-vehicle check. Existing ON behavior when a custom vehicle bay is missing still uses its legacy warning/fallback; this issue does not redesign it.

Transfers OFF:

1. Existing check-in reaches WaitingForMechanic. At most once per five GTA game seconds, select a free existing mechanic slot with a configured work position and clear vehicle target. In CUSTOM mode use its saved VehicleParkingBay; AUTO LOT retains the existing computed workstation offset and heading. No hidden default coordinate or location reset.
2. Snapshot target position/heading and elect teleport mode for the order. Save the accepted fixed quote using the existing transaction before any vehicle movement. If save fails, undo mechanic reservation/accepted assignment; no movement or charge.
3. TeleportWaitingBay retains the order until entity, occupant and destination checks pass. Then stop velocity, write position and configured heading once, and enter Repairing. If heading fails after a successful position write, remember that partial transition and retry heading without repeating the position write.
4. Existing employee activity walks to its SEPARATE work point and starts WORLD_HUMAN_VEHICLE_MECHANIC. Existing repair accounting waits for PhaseWorking. At the completed-work boundary, OFF mode verifies successful placement and a live car still within the existing bay tolerance; failed checks are throttled, not scanned every frame. No ghost-car payment.
5. Existing TrySettleRepair commits exactly once and retains its rollback behavior. The paid order enters TeleportWaitingPickup, not a second repair. Existing pickup ownership/first-serviced priority and destination checks must permit return.
6. Move to the configured customer drop-off/pickup position and heading once, then call the unchanged customer pickup route. A navigation callback failure after movement retries navigation without moving/paying again.

No timer-driven teleport every Tick, model requests, new entity type, background worker, blocking wait or recursive retry. Ordinary transition handling is on the existing approximately one-second visual update. Expensive clearance checks/movement are eligible at most once per five GTA game seconds per order. Waiting can continue until the owner clears a persistent obstruction; there is no cancellation deadline that discards a good pending order. One-shot waiting feedback avoids per-frame notification spam.

### Safety and occupancy

Validate entity existence/destruction and finite coordinates/heading. Prohibit movement if the player is in that car, its driver seat is occupied or it has passengers (including NPCs from other scripts). Require the player to stay at least 3 metres from the target. Check other tracked cars and unpaid reserved bay targets with conservative 4-metre center clearance, plus one existing-style local IS_POSITION_OCCUPIED vehicle query. Pickup also uses existing CanReturnCustomerCar checks. No broad collision search or world enumeration is added.

These conservative clearances can defer tight adjacent MLO bays until the earlier car clears. Correct saved positions, mode, headings and free paths remain necessary. Engine-native placement is not guaranteed collision-free merely because tests pass. Missing/destroyed cars use existing cancellation/accepted-quote preservation; no stale movement command is queued for a future entity.

### Logical Service Bay configuration

Both Employees > Employee Settings > Placements > Service Bays and existing customer vehicle-bay entry points open a common Bay 1/2/3 profile. It offers Set Vehicle Position (CUSTOM), Set Mechanic Work Position, Mechanic Assignment and View Current Configuration. Reuses existing point editors, keyboard/controller selection and Back navigation.

Coordinates remain distinct. CUSTOM editing does not silently switch AUTO LOT off: the page explicitly explains which existing mode is active. Mechanic assignment remains the existing roster SLOT, preserving progression, wages and job ownership. The assignment action explains the current slot and points to existing Mechanics management; arbitrary staff-to-bay reassignment/reordering is NOT added. View-only actions do not save, charge or alter locations. User-facing consolidation is implemented without a new positioning model or relative-coordinate migration.

## C. Compatibility and limitations

Zero Save.ini or Settings.ini key additions/removals/renames. Existing mechanic bay, VehicleParkingBay1..3, headings, assignments, finances, inventory, equipment and supplier values load/write unchanged. New wait/one-shot fields are session-only. Existing RepairPromisesV1 preserves unpaid accepted quote identity, pricing and remaining work across reload; fresh entities receive fresh transitions after check-in. No teleport/native handle is serialized. Existing abort cleanup remains unchanged.

Paid customer/departure vehicles remain transient as before; this issue does not introduce persisted completed-car storage after abort/reload. A paid blocked pickup must not pay again, but abort/reload may clean up that session's already-paid vehicle as in the parent. Test this limitation explicitly in disposable QA. Missing entities can trigger the existing cancel/resume path rather than restoring a particular physical car.

FinanceV1 serializers, Save, TryCommitBusinessSave, TrySettleRepair, supplier accounting, FS1 readiness, OnTick/OnAborted, economy and remote work are pinned unchanged by preservation checks. Active unpaused timing, quote rollback and zero offline economic catch-up are inherited. No speculative dispatch-backoff, deletion-retry, crash or permission fix from Issue #28 is included.

### Before / after

Before assignment:
```csharp
order.VehicleHandoff = _vehicleTransfersEnabled && order.ResolvedParkingBay.IsSet;
order.Stage = order.VehicleHandoff ? CustomerStage.MechanicWalkingToPickup : CustomerStage.Repairing;
```
With transfers OFF that entered repairs while the car stayed at drop-off. Now the OFF election snapshots its existing vehicle target, commits the accepted quote, enters TeleportWaitingBay, and only successful placement enters Repairing. ON retains its original path.

Before completion, a non-handoff order immediately called ReleaseCustomerForPickup. Now a teleport order first enters TeleportWaitingPickup; the existing ownership/occupancy rules and successful placement must precede that call. Settlement itself is unchanged. Full source.diff reproduces the exact candidate from base 12fbbf7; preservation.json lists the full reviewed method boundary.

## D. Verification and test interpretation

Actual current results are recorded in validation.json, build.txt, feature-tests.txt and preservation.json. Product Release targets net48/C#7.3 against ScriptHookVDotNet3 3.6.0. Inherited compiler warnings remain CS0169 and CS0649. Compilation does not prove Enhanced 3.9.0.6 compatibility or GTA stability.

Focused tests compile verbatim production assignment, transfer, quote/payment, queue ownership and employee-animation logic against deterministic entity/native adapters. They include real temporary-file accepted-quote save/reload and failure-before-replace. They do not run GTA collision, movement, animation rendering or driving AI. Legacy accounting fixtures explicitly use ON with the prior missing-bay legacy path, keeping their accounting focus; the new OFF suite exercises actual new helpers instead of masking their behavior. Input/pause harnesses gain only the new menu-parent fields required to compile actual Back navigation.

Coverage includes ON dispatch/unchanged driving helpers; OFF custom and AUTO targets/headings; one-shot movement; employee arrival and animation call; paid pickup; occupied bay/return priority/multiple orders; player/driver/passenger occupancy; missing/deleted/destroyed vehicles; invalid coordinates; partial position/heading failure; callback failure; pause; accepted-save failure; failed settlement rollback/retry; saved-point preservation; quote reload without stale commands; profile menu entry, separate editor targets and no view-side financial effects.

### GTA acceptance checklist — NOT RUN

1. GTA closed: back up exact installed source and COMPLETE matching business data/settings/logs outside scripts. Preserve the sole original save; use disposable matching copies. Never alternate independent financial/WP1/FS1/Issue29 sources on a modified Save.ini without restoring its complete matching backup. No runtime DLL/config change is part of this task.
2. Review this branch and hash before deciding to test; no release/install was performed. If testing source manually, enable exactly one matching Business Manager .3.cs, retain runtime/dependencies and allow FS1's default startup delay. Check source-hash BOOT lines, one instance, saved positions/headings, cash, inventory, suppliers, staff/wages and accepted quotes.
3. ON, valid CUSTOM bays: check in, mechanic walks/enters/drives/parks/exits, walks to its work point, animates and repairs, then returns/drives and customer departs. Repeat AUTO LOT. No new teleport should occur. Verify the existing ON missing-bay warning if that configuration is used.
4. OFF, CUSTOM: set car and work points separately for each slot. Keep player on foot at least 3m from target; verify one car placement and heading, mechanic working BESIDE it, one repair payment, one pickup placement/heading and existing customer departure. Watch that a car does not move again each second/frame. Repeat AUTO LOT with its existing mode/offset; custom point changes must not silently override AUTO.
5. Occupy the assigned bay with another car; verify retained pending order, bounded feedback, alternate available bay or deferred assignment. Clear it and wait at least five active seconds. Test coincident bay points and two/three orders with staged queue cars. No shared target; first serviced return retains priority; waiting intake does not steal pickup.
6. Put the player in the customer car before inbound placement and before paid pickup. No teleport. Exit/clear the target, then verify safe resumption. Repeat with another NPC occupant if safely reproducible. Park a car at pickup; payment occurs once, car stays at bay until pickup clears. Ensure no double notification/charge after waiting.
7. Using disposable QA only, remove/destroy a customer car before assignment, during wait and at repair completion. Verify safe cancellation/defer, no ghost payment and accepted quote retained where applicable. Do not assume a cleared customer entity restores the same physical car.
8. Reload before accepted assignment, in TeleportWaitingBay and during active repair. Verify exact saved quote/remaining work is recovered with fresh entities and no old teleport command/handle. Reload after payment while pickup is blocked: verify no additional income; note inherited paid transient-car cleanup. Abort/reload must not leave a delayed movement operation.
9. Exercise Service Bay profiles from both entry paths; set/read separate points, inspect roster-slot assignment, navigate Back through point editors using keyboard/controller, and open native pause with Esc/Start while menu is open. No cash/stock/save change from viewing configuration.
10. Pause during pending transfer/repair/supplier ETA; resume after waiting. Close GTA/relaunch with a pending accepted quote. Verify no paused/offline economic progress or double settlement. Test retail sales, manager wages, mixed supplier receipt, pallets and current/lifetime finance views as inherited regressions.
11. Use the current GH + LS RED baseline first with other conditions fixed; then owner's gradual script reintroduction. Record exact source/runtime hashes, configured positions/mode, logs and active duration. Longer stability and Issue #28 attribution remain separate; report any new crash with its own exception and matching evidence.

## E. GitHub / review status

Feature: feature/shvdn3-29-service-bay-integration. Target: integration/shvdn3-v0.3.3.4 (baseline-only fork of 12fbbf7). Draft creation result is recorded in github-pr-attempt.txt; no successful PR is claimed without its URL. Use PR_BODY.md and the comparison link in REVIEW_HANDOFF.md if API access is denied. No main/QA/investigation branch merge or release publication.

## F. Next steps

Owner GTA acceptance and source review are required before considering a merge. No other v0.3 roadmap milestone, arbitrary roster reassignment, tablet, held waypoint/Mule merge, Issue #28 fix or live installation work was begun.

## Recorded automated results

Genuine SHVDN3 3.6.0 net48 / C#7.3 Release: PASS, 0 errors; inherited warnings CS0169 and CS0649. SDK 8.0.426.

467/467 behavior tests PASS: 85 inherited issue/logger/migration checks plus repair 32, sales 29, pallets 25, wages 22, reload 41, supplier 37, range 40, menu 13, cart 32, pause 9, focus 8, finance 33, startup 18, hardening 9, bays 34. Static 21/21 PASS; focused scope/save/v2/physical-helper preservation PASS. The 34 new bay tests use production members with deterministic adapters, not GTA.

Candidate source SHA256: 8d6ddb213fe69105bbbbc10b6d7746f36c8851912bebfcd5ebd002e643532d6b. Full diff verified to reproduce this source from 12fbbf7.

GTA testing NOT RUN. Windows execution NOT RUN. Enhanced runtime compatibility NOT VERIFIED. Crash cause UNATTRIBUTED. No release archive, runtime deployment, save modification or merge was produced by this task.
