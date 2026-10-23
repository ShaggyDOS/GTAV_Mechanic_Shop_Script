# v0.3.3.1 SHVDN3 repair coverage — EXPERIMENTAL

- Issue #27: fixed shared quotes for physical, background and remote repair paths; max(existing total, estimated reserved parts plus billable labor).
- Protect dedicated/universal stock allocations and one-time settlement; retain existing in-memory legacy promises; checked monetary bounds.
- Add optional explicit acquisition estimates and a separate versioned pricing audit. Existing save payload and supplier/wage expenses are unchanged.
- Automated results are in validation.json. GTA testing NOT RUN; crash attribution remains unproven. Accepted runtime orders still have the baseline cancellation-on-reload limitation.
