# v0.3.3.1 SHVDN3 repair cost coverage — EXPERIMENTAL

Issue #27; follows experimental migration PR #21 at 430a202e7b10e3f191acccb7a094723568bd14e9. Review date October 22, 2026 (America/Chicago). Separate branch feature/shvdn3-27-repair-cost-coverage. No manager supplier automation, manual parts sales, pallet layout, forensic probes or map-blip changes are included in this focused change.

## Result and accounting boundary

Before: physical customers used service base plus expertise; virtual jobs used JobRevenue, regardless of parts or labor. After: a shared quote sets total = max(existing service base + existing expertise premium, estimated consumed parts acquisition cost + estimated billable mechanic labor). The existing total is a floor, not a second surcharge. Universal, filters, brake kits and diagnostics use their actual reserved stock source; brakes reserve two units. Existing supplier procurement discounts, invoices and wage accrual remain separate and are not charged again by quoting or consuming parts.

The inventory stores counts, not purchase lots. This change explicitly uses estimates, not invented historical costs. Optional Settings.ini [Pricing] UnitCostEstimateUniversal, UnitCostEstimateFilters, UnitCostEstimateBrakes and UnitCostEstimateDiagnostics are nonnegative long amounts per unit. Missing/negative values use ceil(configured base unit cost × 1.25), conservatively covering current express acquisition before procurement discounts; default estimates are $125/$113/$200/$157. Zero is an intentional zero estimate. No Settings.ini is generated or modified by this work. Historical purchases under different settings cannot be reconstructed. Quotes and status label the basis as estimated.

Labor is ceil(current mechanic period wage × predicted billable work seconds / wage period). Wage bonus and specialty wage are included; existing expertise, specialty speed, equipment speed and remote duration are applied before snapshotting. Walking, parking, waits, idle wages, overtime caused by stalls and later configuration/wage changes are outside the billable estimate. Wages remain fully owed in the existing ledger. This is estimated service cost coverage, not a guarantee of business profit. Once assigned, price, duration and source allocation are fixed; no post-completion surcharge is introduced. Duplicate assignment cannot raise a quote twice.

## Exact source changes

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs is executable product code changed. CustomerOrder/RemoteJob hold long prices and RepairQuote snapshots; Mechanic holds its background quote. InitialiseRepairPricing, EstimatedUnitAcquisitionCost and CalculateRepairQuote implement the basis. ReserveRepairUnits, ReserveQuotedUnits, UnreservedRepairStock and TryCreateRepairQuote protect exact bucket allocations across physical, remote and background work, including shared handoff snapshots. TryConsumeQuotedSupplies and ConsumeRepairSupplies consume those sources exactly once.

AssignCustomerMechanic snapshots the quote before setting HasJob. ProcessOneCustomerRepair, ProcessEconomy and ProcessRemoteOperations check stock and credit bounds before payout; RecordRepairPricing records completed settlements in separate RepairPricing.v1.csv. EnterRemoteOperations carries the fixed quote through handoff. SpawnCustomerOrder/SpawnRoadsideWaitingCustomer use ServiceQuoteBaseline; ShowCustomerOrderStatus displays the estimate. MechanicWage/TotalMechanicPayroll and the economy wage local use long arithmetic to avoid integer wrap. See preservation.json for the complete method list and source.diff for every changed line.

Before/after core rule:

```csharp
// Before (physical assignment):
order.Price += level * 20;
if (SpecialistMatches(m, order.ServiceKind)) order.Price += 75;
// After: baseline + premium competes with cost floor, never stacks twice.
quote.Total = Math.Max(baseline + premium, partsEstimate + laborEstimate);
```

The excerpt describes the rule; source.diff contains the exact implementation, checked arithmetic and call sites.

## Data compatibility and limitations

Save() and every existing save key are unchanged; no business saves are edited by the agent. Existing stock, cash, invoices, pending wholesale DeliveryCost, saved coordinates and earned-package masks retain their established meaning. New configuration is optional and additive. The old customer/remote CSV schemas are unchanged; RepairPricing.v1.csv is a separate 13-column completion audit.

An already-promised in-memory legacy order keeps its price even if below the new estimate, and is marked LegacyPromise in the sidecar. Existing physical/remote job objects and accepted price history were never persisted by PR #21: reopening the script still cancels those runtime orders under the original lifecycle, without offline completion or deferred payout. This change does not claim to restore an accepted customer entity or price across script reload. Settled ledger totals continue using the existing save fields. This inherited persistence limitation is explicitly exposed for reviewer consideration; no destructive new restoration scheme is introduced.

OnTick is unchanged. Active unpaused game-time sampling, existing clamped deltas, startup delay/hold defaults and no offline catch-up remain intact. The tests extract production methods into deterministic stubs; save-ledger tests model persisted totals and do not prove Windows INI writes, GTA entities or accepted-order restoration.

## Installation, QA and rollback

Review first. The ZIP contains exactly one active .3.cs source and a separate net48 project linking that exact source. Do not install the whole archive into GTA. For a later owner-authorized test, back up the entire scripts/GlassHeroesBusiness data folder, Save.ini, Settings.ini and logs outside the game; disable all v2 and duplicate Business Manager sources/DLLs; copy only the experimental .3.cs into scripts. Keep the installed ScriptHookVDotNet runtime unchanged. No Save.ini or Settings.ini is bundled.

Manual GTA tests (all NOT RUN): cold launch past 30-second gate; test all three customer services, brakes dedicated/fallback and insufficient stock; compare high wages/master specialties/equipment and remote/background completions against displayed fixed estimates. Change wages mid-order and ensure no hidden surcharge. Pause/unpause for several minutes and leave/re-enter streaming range; confirm no paused/offline revenue or supplier progress. Save/reload before assignment, during repair/handoff and after payout, checking cash, all stock buckets, bills and absence of duplicated payout, while observing the documented baseline cancellation behavior for runtime customers. Check pending wholesale cargo stays unbilled until pallet deposit and the original package positions/masks are intact. Repeat keyboard/controller confirmations and check CSV column compatibility. Include cold-launch/reload and a controlled heavy-mod session after the owner's no-GH control; preserve crash evidence without attributing an unknown CLR stack overflow.

Rollback: disable this .3.cs and restore the prior experimental source/project; never run both. Optional pricing keys and sidecar CSV may remain ignored. Preserve current financial saves; restoring a pre-test backup rolls back all transactions since that backup, so do so deliberately only in an isolated test copy. No QA R2/main merge or deployment is authorized.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

117/117 extracted-behavior tests PASS (85 inherited; repair: 32). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game OnTick clock sampling and original v2 source/project are unchanged. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 1856d76ade0a9d0d00fb3b8c6d3a2a1a62da0b7e3a0ba5952bea4345609f8949. The complete unified source.diff applies to parent 430a202e7b10e3f191acccb7a094723568bd14e9 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
