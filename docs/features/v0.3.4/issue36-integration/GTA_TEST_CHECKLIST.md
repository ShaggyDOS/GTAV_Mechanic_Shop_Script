# Combined Issue36 + v0.3.4 GTA acceptance — NOT RUN

These are future tests after explicit owner authorization. No testing release or live installation is prepared by this integration.

## Controlled baseline and rollback

- [ ] Close GTA and preserve exact installed script/hash plus a complete scripts/GlassHeroesBusiness directory backup, including Save.ini, Settings.ini, finance journal data in Save.ini and every companion file present.
- [ ] Use a disposable complete data copy. Do not alternate maintenance, preintegration v0.3.4 and combined scripts against the same modified sole Save.ini.
- [ ] Record Legacy 1.0.3889.0, Enhanced 3.9.0.6, LSRED and other mod versions/configuration; exactly one Glass Heroes script. Keep protected RC1 and preintegration c77591b available.
- [ ] Combined source hash: 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120. Confirm startup DEV identity/source hash and existing startup gate with no reload/initialization/save errors.

## Footer / mechanic clarity / canonical staff menus

- [ ] At 720p and 1080p, inspect long and short footers, spacing, natural-word wrap/ellipsis and control hints. Repeat with usual HUD overlays. Unchanged highlighted help should not stutter or repeatedly query native widths.
- [ ] Inspect inventory category, supplier review and six-row Active Deliveries footers; retain accurate confirmation/receipt/GPS/Back descriptions.
- [ ] Mechanic menus distinguish shared entry corridor (spawn/break and optional customer-car pickup/return), shared work stops (workstation access), per-mechanic direction selections and final approaches.
- [ ] Compare selected/saved/unset counts and route previews with existing route masks and SET/UNSET points. No stop is silently enabled, moved, reordered or cleared by browsing.
- [ ] Enter receptionist and manager routes through both role management and Placement Settings. Confirm the same modes, five saved stops and approach. Explicit edits persist only the chosen values.
- [ ] Keyboard/controller selection, held Enter/Accept, child editor Back, parent Back, F2 reopen and Esc/Start native pause preserve calling rows and never duplicate saves/actions.

## Wholesale locator and original physical mission

- [ ] Before ETA / for LOCAL or EXPRESS, no wholesale pickup blip or GPS route.
- [ ] READY uncollected wholesale cargo gets one named long-range package blip at the exact saved warehouse point even far away. Physical package/world marker stays distance-streamed.
- [ ] Active Deliveries row 4 Set Warehouse GPS enables only the owned route. Existing player waypoint stays unchanged; changing/clearing it yields the supplier route. No automatic waypoint takeover when ordering.
- [ ] Recover/cancel/Back remain rows 2/3/5. Row 1 remains Order / Last Receipt, not an older contents-only label; row 0 retains v0.3.4 live state.
- [ ] Player pickup/load removes locator; loaded/unloaded cargo cannot regain warehouse guidance. Save failure retains checkpoint ownership and no stock/invoice posting.
- [ ] Missing location gives setup help, no invented coordinates. Failed blip creation/deletion, repeated status/GPS requests and external blip removal remain bounded and never delete unrelated blips.
- [ ] Reload with READY pending cargo recreates one owned locator after the normal gate. Carried/unloaded reload follows existing original-order warehouse recovery. Abort cleans owned locator. WHOLESALE remains entirely player-operated.

## Full v0.3.4 and financial/save regression

- [ ] Inventory on-hand/reserved/free/incoming/cost and unit-versus-parcel capacity remain accurate. Incoming cannot satisfy sale/repair reservations; earned boxes are retained after use/sales.
- [ ] Mixed cart -> Review -> Back/edit -> Confirm creates one order/ETA; supplier/quote/capacity revalidated; held input cannot duplicate it.
- [ ] Nearby physical and offsite manager LOCAL/EXPRESS receipt, capacity hold, player carrying priority and boundary transitions preserve identity and stock/invoice exactly once.
- [ ] Failed cargo/pallet/receipt save in a disposable controlled environment restores prior state. Never change live attributes/permissions to induce failure.
- [ ] Final receipt saves mixed inventory/liability/provenance/outcome once. Reload cannot repeat it; last receipt stays distinct from a newer pending order.
- [ ] Supplier receipt accrues liability without cash payment; prepaid receipt does not bill twice; unpaid cancellation has no refund; actual invoice payment reduces cash/liability once.
- [ ] Reconcile $5,000 remote sales with no outflows (+$5,000 cash) and with payments (net increase after actual cash outflows). Remote subset is not counted twice.
- [ ] Menus, all employee paths/masks/headings, mechanic queues, service bays, vehicle transfer modes, repair quotes/settlement, inventory, wages/progression and FinanceV1 remain intact.
- [ ] Pause beyond ETA, long closed-game interval and reload: zero paused/offline income, wages, supplier or virtual progress. Extended LSRED session has no growing locator/entity/task count or performance regression.
- [ ] Record crashes with exact hash, time, dump and matching logs. CLR overflow, native startup AV and Read-only/EUP symptoms remain separate; no crash cause is established by this integration.
- [ ] With GTA closed, restore the complete matching baseline script and data directory to prove rollback. Archive the candidate data separately; never restore only an old Save.ini into mismatched companion files.
