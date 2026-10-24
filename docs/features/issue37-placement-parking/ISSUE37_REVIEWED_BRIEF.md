## Owner GTA QA request — v0.3.4 RC2 (Oct 24, 2026)

**Scope/proposal only — owner implementation YES/NO still required.** The owner provided screenshots of (a) per-mechanic `TO WORKSTATION / Luis Moreno` selection, and (b) `MECHANIC WALKING` menu showing Custom MLO, shared entry corridor and work stops. Current RC2 diagnostic/crash investigation must remain isolated. Do not change RC1, maintenance, main, live saves, or active RC2 diagnostics while designing.

### A. Customer drop-off must NOT require facing/heading (P1 gameplay)
**Owner observation:** mechanics circle around the customer drop-off rectangular pavement area attempting to approach the saved facing; returning repaired cars should be allowed from either orientation.

**Source confirmed** in `experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs` at `diagnostic/v0.3.4-rc2-mechanic-transitions`:
- `UpdateOneCustomerOrder()` driving intake issues `TASK_VEHICLE_PARK` using `_customerDropoff.Heading` inside 14 m, even though later acceptance is proximity/speed-based.
- `VehicleDestination(toBay=false)` returns `_customerDropoff`; `UpdateVehicleApproach()` issues `TASK_VEHICLE_PARK` using `dest.Heading` when close for both bay and drop-off. `UpdateVehicleParking()` accepts within 4 m and speed <1.5, but a timed retry again uses `dest.Heading`.
- `MarkParkingSquare(_customerDropoff)` draws a directional rectangle with a forward arrow using heading; this is presentation, not the parking logic itself.

**Design:** separate direction-free customer handoff zone from orientation-sensitive *service bay* and queue parking. For drop-off on BOTH incoming customer intake and mechanic repaired-car return, target a safe position, stop within validated clearance/radius and complete handoff at low speed without `TASK_VEHICLE_PARK` forcing a saved heading. Retain no-overlap/occupancy guards, drive timeouts, blocked-road recovery, one-way/road navigation safety, customer/worker exit handling, paid-return recovery, finances, reservations and single-settlement. Do not teleport through MLO objects or force an exit on active traffic. Bay parking retains optional facing. Draw customer drop-off as an orientation-neutral zone (no arrow) and label saved heading as unused for drop-off if kept for back-compat. Add deterministic tests for both ingress and return, either heading, occupied zone, no-progress, stop/exit, and timeouts.

### B. Clarify mechanic walking routing/stop terminology and selected physical points (P1 UX)
Current menu rows: `Mechanic Walking Mode`, `Car Walks: Entry Corridor`, `Shared Entry Corridor`, `Shared Work Stops`, `Workstation Approaches`, `Legacy Staff Checkpoint (MLO)`, `Selections by Mechanic`, etc. Per-mechanic direction currently shows `TO WORKSTATION / Name` and `Shared Work Stop N` with `SKIP / SET` or `SKIP / UNSET`.

Rename/explain without changing existing route behavior/masks:
- `Shared Entry Corridor`: common Staff Spawn -> Break path (5 configurable stops), used reversed for mechanic pickup walk and forward after returning customer car only if `Car Walks: Entry Corridor` reuse is ON.
- `Shared Work Stops`: five physical Break <-> Service Bay walking stops shared by mechanics, selected separately for `TO BAY` vs `FROM BAY` using per-mechanic masks (not car-driving tasks).
- `Workstation Approaches`: final pedestrian approach by workstation/bay, separate from parked vehicle bay coordinates.
- `Legacy Staff Checkpoint`: advanced legacy point, not implicitly a pickup waypoint.
- Distinguish `SAVED PHYSICAL POINT` from `USED BY THIS MECHANIC`; `SET/UNSET` and `USE/SKIP` should not appear as ambiguous fused state. When highlighted, optionally emphasize ONLY the selected shared physical point; no change to saved coordinates or masks unless explicitly edited.

### C. Placement marker display management (P1 UX/performance)
Current `PlacementMarkers` menu has only global ON/OFF + color legend; `DrawPlacementMarkers` draws every category when global ON within 80 m, while `_autoLotPreview` still draws independently; saving a point forcibly re-enables global marker visibility. This results in massive overlapping marker clutter in MLO.

Provide `OFF / FOCUSED (selected/editor) / CUSTOM / ALL` modes with independent category switches: customer/queue+dropoff; mechanic work/entry/walking; service vehicle bays; receptionist/manager; supplier/stockroom. Allow quick Hide All, Show Current Category and restore last custom selection. Emphasize active selected point and give a simple on-demand identity legend/label or diagnostic identifier. Preserve existing `_autoLotPreview` as an explicit separate toggle, but make Hide All truly hide previews or clearly distinguish optional previews. Do not automatically override user visibility choice upon save; offer temporary preview for the newly saved point. Render only short-range, filtered markers; no excessive GTA native calls per tick; markers are cosmetic and never alter AI.

### D. Safe reset of saved placements (P1 destructive operation, separate approval)
Add `Placement Settings > Reset & Restore Placements` with category resets and a deliberate `Reset All Saved Placements` option. Show count/affected groups and require two-stage YES/NO confirmation. **Require shop CLOSED, no active/staged/paid-return customer orders, no pending physical supplier delivery or cargo, and no active remote jobs; do not interrupt customer promises.** Save complete consistent backup before any bulk mutation; provide documented rollback and a verified save/commit-or-rollback pathway. Never erase the business ledger, cash, stock, employee profiles, purchased equipment, customer billing, invoices, earned pallet capacity, or fixed world supplier warehouse pickup. Distinguish clearing editable spatial coordinates (including optional shop anchor explicitly) from changing mechanic route masks/modes (preserve unless separately approved). Reset only geometry selected by the owner; avoid orphaned prop/pallet visuals and invalid worker/parking references.

### Acceptance and staging
1. Produce source-level design and preview UI hierarchy first, STOP for owner YES/NO before code.
2. Implement in a new isolated UX/placement branch only after approval. Do not merge with ongoing RC2 crash forensics. A changes vehicle arrival logic; B/C presentation; D destructive settings action needing dedicated safety tests.
3. Clean net48 SHVDN3 build, full inherited regressions, new negative-path tests (double-save, lost paid return, occupancy, path masks, reset while live), save/rollback checks, keyboard/controller, custom vs auto lot mode, static perf.
4. GTA acceptance: road/driveway arrival from opposite headings, mechanic customer return and intake, service bay oriented parking unchanged, no stale markers or leaked state, shop closed reset with backups. Report all as NOT RUN until owner performs game test.
5. Report exact base/branch/SHA/diff and STOP for second owner YES/NO. Preserve RC1 and live installation.

**Related:** #30 centralized Placement Settings; #33 mechanic route/parking issues; #36 route clarity/marker context.