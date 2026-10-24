# v0.3.4 GTA acceptance checklist — NOT RUN

For a future separately approved experimental test package. Do not install from these instructions without that approval. No RC prepared by this development task.

## Controlled setup and backup

- [ ] Close GTA. Copy the installed script and complete scripts/GlassHeroesBusiness directory to a dated backup outside scripts. Include Save.ini, Settings.ini and all companion files; FinanceV1 is embedded in Save.ini.
- [ ] Preserve working v0.3.3.6 RC1 and v0.3.3.5 rollback artifacts. Use a disposable full data copy; never alternate builds against the same modified sole save.
- [ ] Record GTA Legacy 1.0.3889.0, SHVDN Enhanced 3.9.0.6, LSRED/mod versions/settings and backup identity. Keep other mods constant for comparison.
- [ ] Verify installed source SHA256 fbc8f7f963bbffa3e31c097bf34250fef5807d28dd5de3b7d75463146beb3251 and startup DEV identity. Install exactly one Glass Heroes script, with older versions/DLLs outside script loading paths. No extra reference/runtime files.
- [ ] Cold launch and confirm existing gameplay startup gate, no script reload/initialization/save errors. Net48 compilation does not certify Enhanced runtime compatibility.

## Inventory and input

- [ ] F2 > Business Management > Inventory & Suppliers > Inventory Overview: enter each category; compare on-hand/reserved/free/incoming against a copied save and accepted physical/remote/autonomous jobs.
- [ ] Verify reserved universal fallback is counted once; incoming cargo cannot satisfy repair or retail availability.
- [ ] Inspect cost estimates, threshold and reorder advice. Browsing changes no cash, stock, pending identity or liabilities and places no order.
- [ ] Storage summary shows unit use/free/projected headroom and parcel slots. Nine small historical deliveries can fill slots even with units free; sales/repairs do not erase earned boxes.
- [ ] Read notices at actual UI scale; report wrapping/clipping with source identity. Check every changed row footer.
- [ ] Keyboard and controller quantity controls, wrap, review, confirm and Back. Back from supplier review restores cart row 6 without purchase. Esc/Start hides overlay and opens native pause without transaction.

## Purchase confirmation and accounting

- [ ] Select each supplier; compare configured ETA, current quote and requirements. Quantities/price formulas remain unchanged.
- [ ] Build a four-line order, open review, Back/edit/review, then confirm once. One pending identity, one ETA, no receipt stock or unpaid-invoice cash debit at placement.
- [ ] Held Enter/Accept cannot create duplicate orders; second pending order is rejected.
- [ ] Cancel unpaid order: no stock/invoice/refund. For an existing legacy prepaid fixture, cancel refunds once. Never fabricate a prepaid live order by editing sole data.
- [ ] If another order replaces an old completed/cancelled one, cancellation requires confirmation for the new identity.
- [ ] Verify receipt increments every mixed bucket once, one parcel provenance slot and one supplier liability unless prepaid. Reload and inspect Order / Last Receipt and current pending identity separately.
- [ ] Last receipt says invoice accrued, not paid; actual payment under Finances & Accounting reduces cash/liability once. Compare opening + inflows - outflows = closing.
- [ ] Observe remote sales over a controlled shift: with $5,000 new remote revenue and no outflows expect +$5,000 cash; with payments expect the corresponding net increase. Remote revenue must not be added twice to customer totals.

## Physical and manager delivery

- [ ] LOCAL then EXPRESS with manager absent: arrival, parking, confirmed driver exit, rear approach/unload, attached driver box, handoff, player carry, saved pallet deposit, receipt notification and departure.
- [ ] Block driver exit safely: one fallback attempt, then recovery after 40 active seconds. Order stays pending, no stock/invoice posted.
- [ ] Block rear access: no early box solely from elapsed time; recovery at 45 active seconds. Missing driver/vehicle/crate/model leaves original order recoverable.
- [ ] Hire manager/open shop, stay nearby: driver follows saved stops and deposits automatically at valid pallet. Status distinguishes manager drop from player pickup.
- [ ] Move across the near/away boundary both directions during arrival/walking; no dead range or duplicate receipt. Player-carried package cannot be auto-received offsite.
- [ ] Away, manager hired/open: virtual LOCAL/EXPRESS receipt after active ETA with capacity. Closed/no manager never receives through manager simulation.
- [ ] Nearby missing physical route, full pallet/unit limit, no parcel slot or absent pallet support: clear hold, no duplicate invoice and recoverable order.

## Wholesale, persistence and regression

- [ ] WHOLESALE requires assigned vehicle; player warehouse pickup, load, transport, unload, deposit. Manager never collects or stocks warehouse cargo remotely.
- [ ] Reload at ETA, warehouse carry, vehicle-loaded and player-unloaded checkpoints. Existing carried/unloaded reload recovery may require collecting at warehouse again; original order/cost stays, no receipt until stocking.
- [ ] Test failed-save behavior only in a controlled disposable environment; do not alter live permissions/attributes. Failed cargo/pallet/receipt actions retain prior cash/stock/liability/order and remain retryable.
- [ ] Reload after successful receipt: outcome/stock/invoice remain, old order cannot receive again. Missing cosmetic prop never authorizes another credit.
- [ ] Pause longer than ETA; close GTA for a long interval and reload. No paused/offline income, wages, delivery or virtual progress. GTA clock changes alone cannot create catch-up.
- [ ] Run 1/2/3 mechanic queues, accepted repair quotes, physical transfers ON/teleport mode OFF, wages, owner transfers, parts retail and all saved role paths/points/headings unchanged.
- [ ] Extended Glass Heroes + LSRED session: bounded entities/retries, no growing orphaned trucks/boxes, performance comparable to baseline. Record exact uptime and activity.
- [ ] If crash occurs, preserve matching dump/analyzer/SHVDN/RPH/Glass Heroes logs and installed hash. Keep CLR overflow, startup AV and Read-only/EUP symptoms separate; do not attribute based on last logged action.
- [ ] Prove rollback with GTA closed: archive full candidate data, restore matching complete baseline directory + script, verify exactly one script and baseline hash.
