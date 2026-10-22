# PR19 wholesale package blip — EXPERIMENTAL

Draft PR #19 adds one persistent WHOLESALE package map icon on QA R2. Physical first and second wholesale deliveries were owner-confirmed on R2. This new candidate has GTA testing NOT RUN; crash cause UNPROVEN. No PR18 code, save/settings changes, merge or deployment. See [developer guide](../docs/releases/v0.3.3.1-qa-wholesale-blip-pr19/DEVELOPER_GUIDE.md).

# v0.3.3.1 QA R2 — EXPERIMENTAL / FINAL REVIEW PENDING

Open GlassHeroesBusiness.sln targeting .NET Framework 4.8, C# 7.3 and ScriptHookVDotNet2 2.11.6. The project compiles scripts/GlassHeroesBusiness.2.cs, byte-identical to the Install/scripts/ source. R2 explicitly combines PR #9 diagnostics, PR #16's #2–#7 corrections and the renewing warehouse retry gate. Do not independently merge overlapping issue and combined PR sources.

Real R2 Release build: 0 errors, 2 existing warnings. Automated results: 63/63 behavioral cases including nine diagnostic tests, 21/21 original static checks, preservation/provenance guards PASS. GTA testing NOT RUN; Windows build NOT RUN; crash cause UNPROVEN. See ../docs/releases/v0.3.3.1-qa-r2-final-review/ for full diffs, exact heads, actual logs, versioned Word/PDF guide, compatibility/rollback and final-review handoff. Install exactly one source after review and preserve every user Save.ini/Settings.ini. Never activate this development copy or a business DLL alongside the install source.
