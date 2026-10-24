# v0.3.4 development changelog — experimental, GTA untested

- Inventory details show on-hand, reserved, free, incoming, estimated cost and non-purchasing reorder advice.
- Shared unit capacity and historical parcel slots shown separately; earned boxes preserved.
- Added explicit mixed supplier purchase review/confirmation with frozen quote/supplier validation and Back retaining draft.
- Added compact live order state, full pending identity and durable last receipt/cancellation.
- Receipt requires captured identity, validates all mixed quantities/capacity/arithmetic, and saves outcome with stock/liability/pending-clear atomically.
- Driver exit is confirmed before rear unload; retries bounded; rear proximity required before driver box creation.
- Wholesale load/unload/recovery and pallet purchases roll back failed saves.
- Existing manager LOCAL/EXPRESS nearby/offsite receipt integrated with shared guarded commit; wholesale remains player-only.
- Added 96 behavior cases and four navigation cases; full total 862 passing, plus 21 static and 13 inventory structural checks.
- Documented v0.3.5 damage-parts relationships only; no new items or physical restoration.
- Existing prices, financial engine, repair reservations/settlement, placements, employee progression and active-unpaused clocks preserved.

No merge, tag, release package, public release or live installation. Crash investigations remain separate and unresolved.
