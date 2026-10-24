# v0.3.4 Phase 0 — baseline and bug-fix coordination

Prepared 2026-10-24. Phase 0 only. The owner's current Development Authorization supersedes the earlier broad authorization and any proposal text suggesting work may proceed through multiple phases. Explicit YES is required at every gate. No response means no approval.

## Verified boundaries

- Protected RC1 branch: `feature/shvdn3-336-service-recovery-business-polish`.
- Approved source reference: `c8617aed5cda492214bbbe1907042bcafbfa0dc1`.
- RC1 source SHA256: `7fff30978e2dc4c3235656f6ca6865506d5cea53387b7eb6d83654d71c8a9d6b` independently recomputed from Git content.
- Existing isolated development branch: `feature/shvdn3-340-inventory-procurement`.
- Fetched remote head before this phase: `9ef7befac9822c722eb488b7d6683433ce80f9b3`.
- Existing commits above RC1: `06565ea` (inventory display), `b46029f` (review notes), `9ef7bef` (structural checks). All preserved without rewriting.
- Current development source SHA256: `6b366ad7fe28a9bdd2418bbe48b7dd6f3b175a86a6e5f15bf0b31b31991e6b8f`. This differs legitimately from RC1 because the existing Phase A source slice was already present; it is not a claim that RC1 was modified.

The original worktree remains on c8617ae. The development branch is checked out separately at `/workspace/GlassHeroes-340`. No new gameplay source edits were made during Phase 0.

## Owner QA results reviewed

Latest authorization reports positive menu appearance/responsiveness, with full v0.3.3.6 gameplay/stability testing still ongoing. That is the available current RC1 evidence. No new matching RC1 crash logs, installed hash confirmation or completed acceptance session were supplied with this authorization. Earlier v0.3.3.5 crash-free gameplay does not prove v0.3.3.6 stability.

RC1 remains an approved source reference under owner evaluation, not a certified stable build. Subsequent findings require the maintenance procedure below. Phase A approval does not imply acceptance for release or permission to modify RC1.

## Categorized bug-fix and investigation register

| Track | Evidence / version | Current disposition |
|---|---|---|
| New RC1 mechanic, queue, parking or staff failures | None newly supplied in current QA update | Await specific reproduction/logs; do not assume old symptoms persist or are fixed. |
| #33 mechanic route-mask, service and return parking failures | Issue evidence identifies exact v0.3.3.5 source/session | v0.3.3.6 contains recovery/diagnostic work and automated tests; owner RC1 verification pending. Not a newly confirmed RC1 bug. |
| #34 menus, retail, employee status, demand | Original v0.3.3.5 scope; implemented in RC1 | Positive current menu feedback; broader runtime acceptance remains pending. |
| #24, #5 supplier driver/handoff and manager receipt | Historical scope/source risks; current code already has manager paths | Preserve and investigate in separately approved B/C/D phases. No maintenance patch inferred. |
| #25, #27 retail and repair costs | Existing accepted functionality and automated regressions | Preserve; no new defect evidence supplied. |
| #1/#28 recurring CLR stack overflow | Earlier experimental incidents, attribution unresolved | Separate forensic track. No RC1 crash fix claim or speculative source change. |
| Startup native access violation | Earlier financial source with different verified hash | Separate crash family; not attributed to RC1 or inventory display. |
| Read-only/EUP behavior | Owner-observed file-access symptom | Separate controlled attributes/permission investigation; no live attribute changes. |
| Existing inventory display slice | Source establishes read-only additions; test coverage initially structural only | Phase A verification/refinement remains gated; see reconciliation below. |

The public open-issue listing and #28/#33/#34/#35 were reviewed on 2026-10-24. `OPEN_ISSUES_SNAPSHOT.json` records the visible listing. Issue-open status is not proof of a current-version defect. The snapshot is not an assertion of exhaustive hidden/paginated discussion coverage.

Other listed issues (#2/#3/#4/#6/#7/#20/#23/#29/#30/#31) cover historical input, placement, warehouse, migration, map and prior workflow scope. Preserve their existing systems and require version-matched evidence before classifying a maintenance regression.

## Maintenance coordination

No maintenance source change is proposed for implementation now; none has been integrated. There is no newly accepted maintenance build to carry forward.

For each new owner RC1 report, record source/hash, runtime/mod configuration, complete backup identity, reproduction, expected/actual behavior, timestamps and logs. Establish whether it occurs in the RC1 baseline or only the development candidate. A confirmed RC1 fix requires a separate proposal and explicit owner approval before source changes. Test it, obtain owner acceptance, then apply only the accepted correction to the isolated development branch with a reviewed diff, save compatibility and regression checks. Never merge unfinished maintenance work automatically.

## REV2 proposal publication and reconciliation

`../PREPRODUCTION_PROPOSAL_REV2.md` is an exact byte-for-byte copy of the previously delivered REV2 proposal (SHA256 `0f6d799e5c4fb9935ced2e79060d956ab8b821c21b511fc51cfa9ccc25f46f11`). Preserve it as the historical design; this report supplies the controlling workflow/scope reconciliation.

| REV2 area | Current source / decision |
|---|---|
| Four stock categories, quote reservations, one pending mixed order | Present. Keep existing counters, SupplierOrderV1 and UnreservedRepairStock semantics. |
| Inventory projection | Existing Phase A methods PendingSupplierInventory, OnHandInventory, InventoryAvailabilityRow, ShowInventoryCategoryDetail and revised ShowDetailedInventory provide on-demand views. Not a new ledger. |
| Incoming / estimated cost / shared units | Existing slice shows these without stock mutation. No schema changes. |
| Dual unit/parcel capacity and richer recommendations | Not implemented by this slice. Review in Phase A; do not label the whole proposal implemented. |
| Existing menu layout | Eight Inventory Overview rows and existing controls retained. Full proposed hierarchy is future B scope, not Phase 0 authorization. |
| Supplier review, durable terminal outcome, identity refinements | Not implemented by Phase A. Defer to explicit B/C approval; proposed additive keys are not authorized merely by committing REV2. |
| Driver exit / intermediate checkpoint rollback concerns | Source-audit candidates, not new RC1 reproductions. No fix attempted; test before claims and implement only in approved scope. |
| Manager receipt | Already exists; future D integrates/refines it, not a second receipt engine. No new wage/ped gating inferred. |
| Future damage items / repair application contract | Design only in E. No new counters, categories, recipes or physical restoration in v0.3.4. |
| Commit plan F / RC preparation | Superseded by current explicit gates: E approval permits final review/verification; final review separately asks permission to prepare the RC1 package. Neither authorizes tag/release/install/merge. |

Phase A coverage gaps to review only after YES: behavior tests executing production projection methods, readable presentation/possible long snapshot lines, negative/corrupt inputs without rewriting saves, consistent capacity explanation and reservation-aware recommendations. The current `PendingSupplierInventory` returns zero for a non-null array of unexpected length; persisted loader validation should be tested, not bypassed by guessing legacy cargo. Current low-stock advice uses existing stock semantics; no unapproved reservation/pricing changes are introduced here.

## Validation and risks

See `VALIDATION.md` and captured command logs for actual rerun results. Tests exercise the existing development source, not an owner GTA session. No new Phase A behavior tests or gameplay refinements were implemented in Phase 0.

Remaining risks: owner RC1 runtime results pending; initial inventory slice may need Phase A corrections; no GTA native/choreography proof from offline tests; proposed metadata/receipt policy remain unapproved; crash investigations remain unattributed. Source refs in REV2 are pinned to RC1 and can shift in development source; identify methods by name when reconciling.

## Next authorized action after review

If the owner answers YES, Phase A may verify/refine only inventory visibility and its tests/documentation. Do not redesign purchasing, delivery settlement or manager eligibility during A. If NO, keep Phase 0 open, address specified coordination/documentation corrections and resubmit. Silence is not approval.

**PHASE 0 COMPLETE — APPROVAL REQUIRED**

**YES:** Approve this phase and authorize Codex to proceed to Phase A.

**NO:** Do not proceed. Address the requested corrections and resubmit this phase for review.

**ShaggyOS Dev Team: Phase 0 review complete. Do you approve proceeding to Phase A? YES / NO**
