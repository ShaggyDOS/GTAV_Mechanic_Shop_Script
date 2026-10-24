## Controlled Issue36 maintenance integration — owner acceptance pending

Review base: preintegration v0.3.4 c77591b; receiving branch feature/shvdn3-340-inventory-procurement. Original RC1 and main unchanged. Owner authorized four maintenance fixes from approved final handoff 2f1ebc2, not a testing release.

Cherry-picked A footer, B mechanic route clarity, C owned READY wholesale locator/explicit GPS and D canonical staff editors, retaining v0.3.4 inventory/procurement/receipt logic. Four product-source overlap hunks and one navigation-harness hunk resolved as reviewed unions; no older-source replacement or architecture change.

Combined: measured/cached footer; shared-stop versus selection terminology; one bounded owned warehouse blip/GPS respecting player waypoint; common receptionist/manager editors with contextual Back; original inventory views, mixed confirmation, live states, last outcome and atomic rollback preserved. Active Deliveries adds GPS at row 4, retains recover/cancel at 2/3 and moves Back to 5.

Actual verification: real net48/C#7.3 SHVDN3 3.6.0 Release, zero errors/two inherited warnings; 954/954 behaviors including 212 navigation, 21 static, 13 inventory structural. Two-parent preservation confirms 396 unchanged v0.3.4 members, unchanged save/config expressions/defaults and accounting/route data; only accepted locator sync added to the throttled tick path.

Review materials: docs/features/v0.3.4/issue36-integration/REVIEW.md, exact COMMIT_MAP, conflicts, changed files, combined/source-only diffs, validation, Word/PDF and future GTA checklist. Source SHA256 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120.

GTA/Windows/Enhanced/LSRED testing NOT RUN; no crash attribution. Integration is NOT yet owner-accepted. STOP for the mandatory new YES/NO before further phase work or testing-release preparation. No PR/public release/tag/merge/install created by this task.
