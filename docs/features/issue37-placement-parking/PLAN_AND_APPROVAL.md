# Issue #37 implementation plan and owner authorization

October 24, 2026. Initial assignment required proposal/YES-NO. Owner then explicitly pre-approved implementation of A–D in the current chat and requested notification when ready for testing. This satisfies implementation gate 1. Final acceptance gate 2 remains PENDING owner YES/NO after the review package. No live GTA installation/testing is authorized or performed by Codex.

Branch: feature/shvdn3-37-placement-parking. Parent: 75e3623f608a05d1a2b59d12c243dbcdfc061358, verified RC1 source e312d8c9500f4a990eb993b82e8e76b55711d3de / SHA256 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120. RC2 diagnostic branch e7998af4db57b2a337f0e3e8ea3deb7a51478924 remains isolated; no diagnostic source is copied/cherry-picked into this feature. main, RC1, maintenance and live data remain untouched. This is Issue #37 experimental development, not v0.3.5 or a crash fix.

## A — direction-free drop-off

Replace the customer-intake TASK_VEHICLE_PARK and both mechanic-return parking/retry paths with a shared position-only slow approach (existing careful drive native, no saved heading). Complete handoff only within the existing 4m acceptance radius, at finite speed below 1.5m/s, after two low-speed observations and existing zone-occupancy checks. At most one no-progress retry, existing intake/parking/transfer timeout limits, no forced acceleration/exit/teleport. Paid return failures still enter the existing recovery path. Service-bay and queued-car directional parking retain exact logic/heading. Empty-car pickup teleport retains existing safety/idempotence but does not rotate to drop-off heading. Saved heading is retained for backward compatibility/initial non-driving spawn, not a handoff requirement. Draw a neutral zone instead of the directional drop-off rectangle.

Affected: UpdateOneCustomerOrder (only intake parking arms), UpdateVehicleApproach/UpdateVehicleParking (drop-off branches), TeleportTargetAvailable/ProcessTeleportOrder (pickup heading only), DrawPlacementMarkers and new bounded handoff helpers/session fields. Service bay, settlement and other state-machine arms frozen.

## B — mechanic walking hierarchy

Placement Settings > Walking Routes > Mechanics:
- Staff Spawn -> Break (5 physical stops)
- Break <-> Service Bay (5 physical stops)
- Final Walking Approaches (per mechanic work position)
- Stops Used by Each Mechanic > mechanic > To Bay / From Bay > Use/Skip each stop
- Advanced: Legacy Checkpoint
- Walking Options (existing toggles)

Mechanic Walking shortcuts use the same physical editors and existing masks. Values show USE/SKIP only; descriptions independently state physical point SAVED/NOT SAVED and DIRECT/CUSTOM activity. Selected physical point is highlighted by the marker policy in lists, per-mechanic selections and PointEditor. No geometry/mask/toggle mutation on selection/preview.

Affected: menu item/value/title/descriptions, MechanicRouteHeading/Counts/Preview, point labels. BuildRoute, node order, EffectiveWorkRouteMask and original mask mutation remain frozen.

## C — marker model

Marker Display > Hide All / Focused / Custom / Show All; five independent categories: Customers & Queue, Mechanics & Walking, Service Vehicle Bays, Receptionist & Manager (including equipment position display), Supplier & Stockroom. Keep last custom mask when changing modes; Show Current Category and Restore Custom shortcuts. Auto-lot preview stays an explicit separate toggle but Hide All gates every placement/auto preview. Focused resolves the actual selected physical point, never an invented coordinate. Saved but inactive custom paths can be inspected without enabling AI modes. Rendering remains short range and cosmetic, with a bounded registry and no world searches/model requests.

Save/clear actions never force visibility ON. PointEditor links to Marker Display. New UI-only preferences use a separate PlacementDisplay.v1.ini, not business Save.ini/Settings.ini; safe default Hide All, mask all five categories. No per-frame file I/O. Existing unknown settings/data remain.

## D — reset, backup and restore

Placement Settings > Reset & Restore Placements:
- individual Customers, Mechanics, Service Vehicle Bays, Receptionist/Manager, Supplier/Stockroom, Equipment Positions
- Full Reset, with optional Shop Anchor inclusion default OFF
- Restore latest verified placement backup, with scope selection and optional anchor

Every operation requires two distinct confirmation pages, default NO; recheck eligibility at both stages and immediately before commit. Expire/cancel confirmation on menu exit/context change/pause/timeout. Require boot active, unpaused, shop CLOSED, no active/staged/paid-return customer, queued request, saved repair promise, mechanic/background job, remote job/mode, supplier order/carry/cargo or supplier vehicle/driver. No operation cancels obligations to gain eligibility. For Supplier/full reset, require zero stock before clearing pallet positions; restore must not reduce usable capacity below current stock. Preserve purchased pallet count/earned upgrades/delivered masks; cleared empty pallets must be placed again without repurchase.

Use explicit geometry whitelist by category, never whole-section deletion. Keep route modes/masks, staff/personnel/wages, finances/quotes/receipts, stock, supplier identities, equipment ownership, display preferences and fixed world warehouse pickup. Clear affected saved resume posts so closed-shop visuals cannot restore stale destinations. Equipment placement flags may clear; ownership stays.

Flush consistent normal save, create complete bounded/verifiable backup of the data directory and exact installed source outside script-loading paths (Documents/GlassHeroes/PlacementBackups), write manifest/hashes, verify copies. Any backup error/oversize/reparse path aborts the mutation. Stage an INI geometry-only overlay on the current save, preserve unknown keys/comments and unrelated bytes, verify, compare current-save hash, commit by same-directory atomic replace. Only after successful commit apply typed geometry in memory and clean owned visuals/caches; on failure leave original memory/geometry/file intact. In-game restore overlays verified backup geometry onto CURRENT data, after backing up current state; never restores old cash/stock/ledger. Full-data recovery is manual with GTA closed. No automatic destructive rollback after a successful atomic commit; normal restart loads the durable new geometry if interrupted during visual cleanup.

## Verification and integration

Separate focused A/B/C/D commits plus review documentation. Actual net48/C#7.3 SHVDN3 3.6.0 Release build, full inherited RC1 954 behavior/navigation suite, 21 static/13 structural checks, new native-order/heading/occupancy/timeout tests, marker mode/focus/preference/controller tests, exhaustive masks, reset lifecycle/backup/manifest/unknown-key/CAS/commit/restore/capacity/failure tests using synthetic files only. Versioned Word/PDF, changelog, full source diff, matching source/project ZIP and exact branch/final SHA/individual commits/file list.

Likely conflicts: A edits RC2-instrumented UpdateOneCustomerOrder, UpdateVehicleApproach and UpdateVehicleParking. B/C overlap Issue36 menu/marker helpers. D adds geometry overlay/menus while preserving Save/ledger implementation. Do not merge this into RC2 to resolve conflicts. A future independently authorized integration must preserve scopes deliberately and rerun its full tests. GTA testing remains NOT RUN; second owner YES/NO required before accepting this experimental candidate.
