# v0.3.4 Issue #37 Placement & Parking Experimental — review pending

Isolated branch `feature/shvdn3-37-placement-parking` implements owner-pre-approved A–D from verified RC1 without RC2 instrumentation: direction-free customer handoff, plain walking coordinates versus mechanic use, persistent filtered markers, and protected geometry reset/restore. [Developer guide](../../docs/features/issue37-placement-parking/DEVELOPER_GUIDE.md), [actual final validation](../../docs/features/issue37-placement-parking/final-validation/validation.json), Word/PDF, full source diff and isolated testing/rollback accompany the package. Actual Release 0 errors/two inherited warnings; 1057/1057 behavior/navigation, 21/21 static, 13/13 structural and preservation PASS. Final acceptance PENDING owner YES/NO; GTA testing NOT RUN. Active RC2/stable RC1, main and live data untouched; no merge or crash-fix claim. Earlier milestone context follows.

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
