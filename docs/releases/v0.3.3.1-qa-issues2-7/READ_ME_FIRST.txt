v0.3.3.1 issues #2-#7 — EXPERIMENTAL / QA ONLY / REVIEW PENDING
GTA testing NOT RUN. Stack-overflow cause UNPROVEN. No production approval.

This candidate is isolated from main and the development integration branch.
Review the individual issue changes first. Read the versioned developer guide
and REVIEW_HANDOFF.md under docs/releases/v0.3.3.1-qa-issues2-7/.

After review, install ONLY Install/scripts/GlassHeroesBusiness.2.cs.
Back up the old script outside the active scripts folder. Preserve ALL existing
Save.ini and Settings.ini files. The packaged .example settings are reference
only. Do not copy the whole ZIP into the game. Do not install a compiled business
DLL, the historical root RC1 source, or the Visual Studio copy alongside it.
Exactly ONE active Glass Heroes script is required.

Additive optional save metadata is documented: DeliveredPackageMask1..4 hides
old untagged decorative displays while preserving stock/capacity; CustomJobRoutes
is false by default and retains all old points/masks for explicit advanced use.
No live user business files were modified by this development task.
