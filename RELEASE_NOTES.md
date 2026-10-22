# PR19 wholesale package blip — EXPERIMENTAL

Draft PR #19 adds one persistent WHOLESALE package map icon on QA R2. Physical first and second wholesale deliveries were owner-confirmed on R2. This new candidate has GTA testing NOT RUN; crash cause UNPROVEN. No PR18 code, save/settings changes, merge or deployment. See [developer guide](docs/releases/v0.3.3.1-qa-wholesale-blip-pr19/DEVELOPER_GUIDE.md).

# v0.3.3.1 QA R2 — EXPERIMENTAL / FINAL CHATGPT REVIEW PENDING

Address PR #16 feedback: replace permanent warehouse retry exhaustion with bounded renewing batches (five attempts, three-second spacing, thirty-second cooldown). Retain successful pallet/box identity, ready marker, ownership and pending financial state. Explicitly integrate PR #9 diagnostics from 4727bdabdf67efd07810f5a62e5d1fdb202464e3 into PR #16's combined #2–#7 source; identify R2 in the boot banner. No new save/config keys/defaults relative to R1.

Actual R2 Release compilation PASS: 0 errors, 2 existing warnings. Corrective behavior 54/54, diagnostics 9/9 (63/63 total), original static assertions 21/21 and business/timer/provenance preservation guards PASS. A prior-source stationary-retry regression failure is recorded separately. Source copies and applicable diffs are verified. Live business saves/settings, main/development and existing PR branches remain untouched.

ONE complete final-review source ZIP and SHA256 manifest: build/releases/GlassHeroesBusiness_v0.3.3.1_qa-r2-final-review_EXPERIMENTAL.zip. Updated MD/DOCX/PDF, actual logs, provenance, retry resolution and manual matrix: docs/releases/v0.3.3.1-qa-r2-final-review/. Review this combined source once; do not independently merge overlapping PRs #9–#16. GTA testing NOT RUN; Windows build NOT RUN; stack-overflow cause UNPROVEN. This prepares final review, not an in-game test or production release.

---

# v0.3.3.1 issues #2–#7 — EXPERIMENTAL / QA ONLY

Review-pending isolated candidate: input parity, ready warehouse cargo, direct player handoff, legacy display cleanup with real parcel preservation, GTA heading/front arrow and direct default mechanic job routes. Six issue branches remain separate. No main or integration merge. Existing issue #1 diagnostics/audit are excluded and unchanged; stack-overflow cause UNPROVEN.

Actual combined Release build PASS (2 existing warnings), 21/21 static checks, 49/49 behavior tests and core source-preservation guards PASS. GTA testing NOT RUN; Windows build NOT RUN. No live business saves/settings touched. Additive visual/route save keys and old-save defaults are documented in docs/releases/v0.3.3.1-qa-issues2-7/DEVELOPER_GUIDE.md. Complete QA ZIP and SHA256 manifest: build/releases/. Draft PR creation blocked by GitHub API Forbidden; exact branch heads, prepared descriptions and error records are in REVIEW_HANDOFF.md / PR_STATUS.json.

---

# Changelog: 0.3.2.9a -> 0.3.3.0 RC1

1. Visual cargo positioning: warehouse pallet, stockroom box base sourced from physical pallet.
2. New manager route states and five configurable points plus final approach.
3. Manager submenu reorganized into Management -> Profile/Hiring/Walking.
4. Manager workstation and approach moved to Employee Placement Settings.
5. Keyboard point SET feedback and marker visibility revised; editor ON state reflects saved marker toggles.
6. One-pending-order supplier recovery and double-confirm cancellation.
7. Supplier delivery ETA now in active gameplay seconds with additive save migration.
8. Removed side effect from manager training quote value display.
9. Preserved startup diagnostic hold and 30-second boot gate from stable 0.3.2.9a.

Static checks: 21 of 21 passed. No compiler or GTA execution performed.
