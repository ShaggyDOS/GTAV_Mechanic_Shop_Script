# v0.3.3.1 SHVDN3 economy, pallets and wages — EXPERIMENTAL / REVIEW FIRST

This experimental stack starts at PR #21 (`430a202`) and contains four focused branches in order: repair cost coverage (#27), manual parts sales (#25), bounds-based nine-box pallet placement, and the Expenses manager wage action. No branches were merged. QA R2, development, main and the original v2 sources/project remain unchanged. Manager supplier automation, separate forensic work and map-blip changes are excluded.

`Install/scripts/GlassHeroesBusiness.3.cs` is the ONE installation source. The separate `VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj` links that exact file. Product: net48, C#7.3, official pinned ScriptHookVDotNet3 3.6.0. Installed Enhanced 3.9.0.6 compatibility is NOT VERIFIED.

See the [complete developer guide](../../docs/releases/v0.3.3.1-shvdn3-manager-wages/DEVELOPER_GUIDE.md), [review handoff](../../docs/releases/v0.3.3.1-shvdn3-manager-wages/REVIEW_HANDOFF.md), versioned Word/PDF report and recorded validation. Focused earlier release guides are historical snapshots with their own source hashes. The [Phase 1 API map](../../docs/releases/v0.3.3.1-shvdn3-phase1/API_MAP.md) describes the foundation.

GTA testing NOT RUN. Windows execution NOT RUN. Crash cause UNATTRIBUTED / UNPROVEN. Accepted runtime customer/quote restoration on reload remains an explicit foundation limitation and Issue #27 review gap. Automated tests do not establish runtime stability, model collision correctness or real save restoration.

Review before any owner-authorized test. Back up saves/settings outside the game, never enable duplicate/v2+v3 Business Manager instances, and keep installed SHVDN runtime files unchanged. The source package includes no live saves, settings or runtime DLLs. No live installation, protected-branch merge or deployment was performed.
