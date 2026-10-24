# v0.3.3.6 Issue #36 maintenance — EXPERIMENTAL / ACCEPTANCE PENDING

Isolated branch `maintenance/v0.3.3.6-issue36`, based on pinned RC1 `c8617aed5cda492214bbbe1907042bcafbfa0dc1`. Four focused fixes: readable footer wrapping, clear mechanic route terminology, owned READY physical-wholesale locator/explicit GPS, and canonical staff-route menus. [Developer guide](../../docs/features/v0.3.3.6-issue36/DEVELOPER_GUIDE.md), [actual verification](../../docs/features/v0.3.3.6-issue36/D-canonical-roles/validation.json), [integration handoff](../../docs/features/v0.3.3.6-issue36/INTEGRATION_HANDOFF.md), versioned Word/PDF and full source diff accompany the review package. Actual Release 0 errors/two existing warnings; 854/854 behavior/navigation and 21/21 static PASS. GTA testing NOT RUN. Gate 1 YES; gate 2 PENDING owner YES/NO. No installation, data rewrite, merge or v0.3.4 integration. The separate v0.3.4 instance handles integration after acceptance. Earlier milestone context follows.

# v0.3.3.6 — service recovery and business polish — EXPERIMENTAL REVIEW

Issues #33/#34 on dedicated feature branch; [developer guide](../../docs/features/v0.3.3.6/DEVELOPER_GUIDE.md), [GTA checklist](../../docs/features/v0.3.3.6/GTA_TEST_CHECKLIST.md), [menu matrix](../../docs/features/v0.3.3.6/NAVIGATION_MATRIX.md), [all row footers](../../docs/features/v0.3.3.6/MENU_FOOTERS.md). Actual Release/tests, source diff/hash and Word/PDF included. No release/install; GTA/Enhanced validation pending. Preserve .5 backup and draft PR32. Earlier milestone context follows.

# v0.3.3.5 — Issue #31 development review — EXPERIMENTAL

Role-based walking routes, Mechanic Break Area label, focused menu Back/customer workflow fixes. [Developer report](../../docs/features/v0.3.3.5-issue31/DEVELOPER_GUIDE.md), [65-page menu audit](../../docs/features/v0.3.3.5-issue31/NAVIGATION_MATRIX.md), [GTA checklist](../../docs/features/v0.3.3.5-issue31/GTA_TEST_CHECKLIST.md). Actual build/tests and Word/PDF included. No release or installation; GTA/Enhanced compatibility pending.

Previous milestone context follows.

# v0.3.3.4 — Issue #29 Service Bay Integration — EXPERIMENTAL

Development branch feature/shvdn3-29-service-bay-integration, from FS1 12fbbf7. OFF now uses one-shot customer vehicle placement to a clear assigned bay, existing mechanic repair/payment, then clear pickup placement. ON driving helpers are unchanged. Service Bay profiles group existing distinct car/work positions; roster-slot assignments and all save/settings keys remain.

Read [development report](../../docs/features/v0.3.3.4-issue29-service-bays/DEVELOPER_GUIDE.md), [review handoff](../../docs/features/v0.3.3.4-issue29-service-bays/REVIEW_HANDOFF.md), Word/PDF and actual validation before reviewing/testing. One installation source links directly into the net48/C#7.3 project. This task publishes no release and performs no installation or merge.

GTA/Enhanced validation PENDING. Preserve exact source and COMPLETE matching data/settings backups; do not alternate candidates on a modified sole save. No runtime DLL, live save or settings changes. Issue #28 remains independent and unproven; held waypoint/Mule work is preserved separately.

Issue #30 menu-only review on `feature/shvdn3-placement-settings-hub`: [development guide and GTA checklist](../../docs/features/issue30-placement-settings/DEVELOPER_GUIDE.md), with versioned Word/PDF, actual validation and full source diff. Financial deduplication and centralized Placement Settings are separate commits; no release or installation is prepared. Saved-point editors, transfer/gameplay and accounting logic remain unchanged.
