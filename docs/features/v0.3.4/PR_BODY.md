> Historical preintegration c77591b snapshot. Current combined source and pending approval: [Issue36 integration review](issue36-integration/REVIEW.md).

## Inventory visibility and supplier transaction integrity

Addresses #35; coordinates with #24 and #25. Base: `feature/shvdn3-336-service-recovery-business-polish` (`c8617ae`), not main. Keep this review draft until owner GTA acceptance.

Inventory Overview now explains on-hand, reserved, free and incoming stock, estimated costs, reorder advice and the independent unit/parcel capacity limits. Supplier carts require a separate review and explicit confirmation; Back retains the draft. Existing supplier cycling and prices remain.

Receipt paths now require order identity and commit all mixed stock, supplier liability, earned provenance, pending-clear and one last receipt/cancellation record through the existing atomic save. Driver exit/rear progression is bounded and requires the driver on foot/at rear. Wholesale cargo checkpoints, recovery and pallet purchases roll back failed saves. Existing nearby/offsite manager LOCAL/EXPRESS behavior uses the shared commit; WHOLESALE stays player-only.

Validation: real net48/C#7.3 SHVDN3 3.6.0 Release, zero errors/two inherited warnings; 862/862 behavior checks (184 navigation included), 21/21 static, 13/13 inventory structural, pinned preservation checks. Finance integration covers $5,000 remote revenue with/without payments, receipt accrual versus cash settlement, failed saves and reload. GTA/Enhanced/LSRED validation NOT RUN.

Review materials: DEVELOPER_GUIDE.md, versioned Word/PDF reports, CHANGELOG.md, GTA_TEST_CHECKLIST.md, NAVIGATION_MATRIX.md, source.diff, final-validation/ and SHA256 manifest under docs/features/v0.3.4. The exact REV2 proposal and phase reconciliation are committed there too.

Additive SupplierOutcomeV1 is optional last-outcome metadata, not a new stock ledger. Existing keys, FinanceV1, RepairPromisesV1, reservations, wages/progression and placement coordinates/headings retain their meanings. No new pricing/categories, physical damaged-car repair, multi-order queue, crash fix or release package. v0.3.5 readiness is design only.

Known limits: one pending order/last outcome; historical outcomes unknown; earned parcel slots remain consumed after stock use; nearby manager needs configured physical route; wholesale carry reload uses existing warehouse recovery; physical native/pathfinding behavior and actual notice wrapping require GTA QA. Source SHA256: `fbc8f7f963bbffa3e31c097bf34250fef5807d28dd5de3b7d75463146beb3251`.

Owner authorized development through all phases to PR readiness. No main/RC1 changes, maintenance backport, merge, tag, RC package, live installation or owner-data changes.
