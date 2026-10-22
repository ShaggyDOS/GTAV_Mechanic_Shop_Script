v0.3.3.1 QA R2 — EXPERIMENTAL — FINAL CHATGPT REVIEW PENDING
GTA testing NOT RUN. Stack-overflow cause UNPROVEN. Not production approved.

This is the ONE combined review package: PR #16's issues #2–#7, PR #9 crash
instrumentation, and the warehouse retry correction. Do not independently merge
it with the overlapping issue/QA PRs. No main/development merge has occurred.
Read docs/releases/v0.3.3.1-qa-r2-final-review/DEVELOPER_GUIDE.md and
FINAL_REVIEW_HANDOFF.md before considering an in-game test.

AFTER final review, install ONLY Install/scripts/GlassHeroesBusiness.2.cs into
the game scripts folder. Back up the previous source OUTSIDE the active scripts
folder. Preserve EVERY existing Save.ini and Settings.ini. The .example settings
are reference only. Do not copy the entire archive to the game. Never install a
business DLL or another source copy alongside it. Exactly ONE active Glass Heroes
script is required. VisualStudio/ contains the matching development copy only.

The historical RC1 source is a .cs.txt verification fixture; do not install it.
Existing optional visual/route metadata and rollback behavior are documented.
No new keys/defaults were added for the R2 retry or diagnostic changes.
No live business files were modified by this development task.
