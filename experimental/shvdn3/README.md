# v0.3.3.1 SHVDN3 Phase 1 — EXPERIMENTAL / REVIEW FIRST

Issue #20, draft PR #21. Based only on QA R2 7693ed2; no PR18/19/22 feature integration. The original v2 project/sources remain intact.

`Install/scripts/GlassHeroesBusiness.3.cs` is the ONE installation source. The separate `VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj` and solution link that exact file. Product: net48, C#7.3, official ScriptHookVDotNet3 3.6.0. Distinct class/assembly names avoid v2 name collisions. The installed Enhanced 3.9.0.6 runtime has not been executed or certified here.

See [developer report](../../docs/releases/v0.3.3.1-shvdn3-phase1/DEVELOPER_GUIDE.md) and [API map](../../docs/releases/v0.3.3.1-shvdn3-phase1/API_MAP.md). Test/build logs, source diff, hashes and Word/PDF report accompany the review package.

GTA testing NOT RUN. Crash cause UNATTRIBUTED. No deployment or merge. Finish the no-GH control and obtain owner approval before testing. Keep saves/settings intact, never enable both Business generations, and never replace the installed SHVDN runtime files. A canonical filename preflight catches common duplicates but cannot detect all renamed copies.
