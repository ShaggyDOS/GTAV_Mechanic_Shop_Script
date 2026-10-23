# Phase 1 review handoff — Issue #20 / draft PR #21

Candidate branch: migration/qa-r2-shvdn3-prep. Base: QA R2 7693ed2167405f475dd4d36392c5df1f257fe178. No merge or runtime installation. Review the final tree, not the initial API-assessment commit (which deliberately records failed compiler evidence).

The one .3.cs installation source is compiled by the separate v3 project via a Link. The v2 sources/project/solution remain unchanged. Source/class/assembly identity is distinct from v2 and from GlassHeroesV3Probe. A readonly canonical filename preflight rejects common duplicate-generation installs, but renamed/subfolder or already-loaded copies still require owner inspection.

Real Release compilation passed against official stable ScriptHookVDotNet3 3.6.0 / net48 / C#7.3. No SHVDN2 reference appears in the resulting product metadata. V2 baseline: 63/63 behavior and 21/21 static. V3: 63/63 inherited behavior plus 22/22 new migration cases (85 total), 21/21 static. Nine logger cases are included once in each inherited total. A clean ZIP extraction rebuilt successfully and passed source parity and the original 21 static checks.

Review API_MAP.md and api-source.diff for UI, customer blip cleanup, frozen-position properties and grouped input mapping. Review RequestVisualModel/ReleaseVisualModel/ReleasePendingVisualModels for nonblocking retries, retained failed-release ownership and abort cleanup. Whole-source reversal enforces that only the reviewed API/lifecycle changes differ; actual OnTick, finance/remote/Save and stock/invoice commit bodies remain byte-identical to R2. No save/config schema/default change, no offline progress and no PR18/19/22 additions.

Open qualification questions: UI position/alignment and hint behavior in the real game; stream-in/retry timing after timed requests were removed; engine callback/abort behavior; actual Save.ini/Settings.ini round trips; existing Enhanced 3.9.0.6 binding and full-stack operation. The installed binaries were not supplied or executed. The compile reference does not authorize a runtime replacement. See the full report for one-variable testing and safe rollback; finish the no-GH control first.

GTA testing NOT RUN. Crash cause UNATTRIBUTED / UNPROVEN. The source ZIP and Word/PDF report are for independent review, with installation deferred until owner approval. Preserve all live business saves/settings. Keep PR #21 DRAFT and leave QA R2/main unchanged.
