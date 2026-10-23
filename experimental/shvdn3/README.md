# v0.3.3.1 SHVDN3 complete economy and pallet candidate — EXPERIMENTAL

Final review head: `feature/shvdn3-repair-quote-reload`. Linear dependency stack: repair cost coverage #27 first, manual parts sales #25 second, separate nine-box pallet placement, separate manager wage payment, then isolated accepted-quote reload correction #27. No branches were merged; QA R2, development, main, PR #21 foundation and original v2 sources/project remain unchanged. Manager supplier automation and forensic/blip work are excluded.

`Install/scripts/GlassHeroesBusiness.3.cs` is the ONE installation source. The separate `VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj` links it directly. Product: net48/C#7.3, official pinned ScriptHookVDotNet3 3.6.0; installed Enhanced 3.9.0.6 compatibility NOT VERIFIED.

Read the [CURRENT developer guide](../../docs/releases/v0.3.3.1-shvdn3-saved-quotes/DEVELOPER_GUIDE.md), [review handoff](../../docs/releases/v0.3.3.1-shvdn3-saved-quotes/REVIEW_HANDOFF.md), matching versioned Word/PDF report and recorded validation. Earlier feature guides/packages are historical snapshots with their own hashes; their accepted-order reload gap is superseded here. The [Phase 1 API map](../../docs/releases/v0.3.3.1-shvdn3-phase1/API_MAP.md) describes the foundation.

Accepted quotes/allocations/remaining active work now persist in a bounded additive journal, with atomic one-time settlement. Back up source and full matching data together: older builds ignore pending journals, and changing finances/stock there can intentionally block stale journal replay on return. Read rollback/failure instructions; do not delete keys or mix financial snapshots.

GTA testing NOT RUN; Windows execution NOT RUN; crash cause UNATTRIBUTED / UNPROVEN. Tests use deterministic world/INI stubs, not engine gameplay qualification. Review before any owner-authorized test. Never run duplicate/v2+v3 Business Manager instances or replace installed SHVDN runtime files. Source packages contain no live Save.ini, Settings.ini, active v2 source or runtime DLL. No installation, merge or protected-branch push was performed.
