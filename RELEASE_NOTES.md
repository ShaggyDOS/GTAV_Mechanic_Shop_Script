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
