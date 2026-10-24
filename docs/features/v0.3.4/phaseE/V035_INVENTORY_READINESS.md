# v0.3.5 damaged-car inventory readiness — design only

v0.3.4 does not implement physical damage generation/restoration, new parts categories, dynamic pricing or a new repair settlement engine.

## Current contracts

The four stored buckets remain Universal Parts, Service Filters, Brake Kits and Diagnostic Supplies. `RepairQuote` owns ServiceKind, DedicatedUnits, UniversalUnits, OrderId, agreed estimates/total and billable duration. `UnreservedRepairStock` derives availability across physical/remote/autonomous/restored accepted work, deduplicating shared quotes. `TrySettleRepair` consumes supplies and posts cash/progression atomically; `ProcessOneCustomerRepair` requires the legitimate mechanic/bay/completed-work state. Current service names are Routine Service, Brake Service and Engine Diagnostics. These contracts are preserved verbatim by preservation.py.

SupplierOrderV1 remains four-line/one-pending; SupplierOutcomeV1 is a bounded terminal procurement audit, not a repair-plan or stock-lot schema. Incoming cannot be consumed or sold. Pallet visuals record earned receipts rather than current units; quantity sales do not delete boxes.

## Proposed future mappings — owner approval required

| Damage domain | Existing item relationship | Additional design/schema if approved |
|---|---|---|
| Bodywork / panels | No dedicated item; universal is not a panel counter | Stable item IDs, panel/kit unit semantics, supplier/cart lines, reserved quantities and save version. |
| Engine components | Diagnostic supplies are diagnostic consumables, not replacement engine components | Separate component definitions and diagnosis recipes; do not reinterpret existing stock. |
| Tires / wheels | No dedicated item | Decide tire vs wheel granularity, quantity, supply and persistence. |
| Brakes | Existing Brake Kits may fit | Approve kit semantics and component substitution before reuse. |
| Glass / windows | No dedicated item | Decide kit vs individual window, supported restoration and stock quantities. |
| General maintenance | Universal Parts / Service Filters | Reuse current meaning/pricing; freeze accepted requirements. |

Do not append categories to existing four-element arrays without auditing every cart, receipt, quote, reservation, retail, capacity and migration consumer. v0.3.4 makes no speculative catalog changes.

## Future relationships and sequencing

Customer order identity -> bounded versioned DamageAssessment -> accepted RepairPlan with immutable PartRequirement lines -> job-owned reservation -> independent mechanic/bay assignment -> active-time work -> durable settlement/repair authorization -> idempotent authorized restoration -> return/pickup.

The vehicle must remain damaged through collection and initial bay parking. Do not repair it on spawn, transfer, reservation or animation start. Parts must be reserved/available for the accepted plan. Final restoration occurs only after successful service.

Disk and GTA natives cannot be committed atomically. Proposed future policy: durably settle parts/finance plus repair authorization once, then apply bounded component-specific restoration; hold return and retry if natives fail, without consuming or paying twice. If the durable commit fails, preserve damage and restore accounting. This ordering, failure compensation, lost-vehicle handling and supported damage operations require separate v0.3.5 approval.

Persist order/plan identities, never entity handles. Diagnose only owned customer vehicles at transitions, not every Tick. Broad fix-everything natives may erase unrelated damage and must not replace an approved component plan. Legacy accepted quotes retain their contract; no retroactive new requirements or repricing.

## Future acceptance gates

1. Owner approves exact new items, substitutions, supplier/retail policy and storage semantics.
2. Versioned repair-plan migration defaults legacy jobs to their existing promises.
3. Damaged car stays damaged until work completion in both transfer modes.
4. Reservations prevent retail from consuming planned parts.
5. Failed save leaves stock/cash/damage unchanged; native failure after durable settlement holds return and never repeats payment/consumption.
6. Missing entities, reload, pause and long closed-game intervals do not fabricate work, parts, vehicle restoration or revenue.
7. Legacy/Enhanced/LSRED engine compatibility and actual damage APIs are validated in GTA.
