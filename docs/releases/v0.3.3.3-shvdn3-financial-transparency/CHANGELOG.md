# v0.3.3.3 Financial Transparency — EXPERIMENTAL

- Add Finances & Accounting hub, four non-overlapping income sources and lifetime/current/last-shift views.
- Separate cash bridge, expense payments, unpaid accruals, owner capital, other credits and purchase-basis accounting profit.
- Add atomic bounded 256-entry financial journal with identities, active-game timestamps, resulting cash and retained-history checkpoint.
- Harden supplier/mechanic/all-bill/owner synchronous save-failure rollback and record rebate/refund metadata separately.
- Label manager returning sales LIFETIME and make ProcessEconomy consume the active-game clock.
- Preserve all supplier/menu/cart/pause, repair/sales/wage/quote/pallet features and existing data. No waypoint/crash changes mixed in.
- Exact cause of reported $5,000-vs-cash session discrepancy remains unproven; no gameplay qualification claimed.
