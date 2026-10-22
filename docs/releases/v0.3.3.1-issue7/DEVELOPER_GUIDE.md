# v0.3.3.1 issue #7 — direct mechanic job routes — EXPERIMENTAL

Issue #7; fix/7-direct-mechanic-job-routes to development/v0.3.3.1. Review PENDING; GTA testing NOT RUN; stack-overflow cause UNPROVEN. Diagnostic PR #9 remains separate and unchanged.

## Changelog, exact methods and before/after

Matching VisualStudio/scripts/GlassHeroesBusiness.2.cs and Install/scripts/GlassHeroesBusiness.2.cs: BuildRoute defaults Break-to-Bay and Bay-to-Break legs to the final destination, even when custom arrival routes exist. Optional saved service points, final approaches and per-mechanic outbound/return masks are retained and used only with explicit advanced job-route opt-in. Custom arrival routes and car pickup/return routing remain independently controlled. Arrival fallback now has braces so an unset gap cannot accidentally append the legacy checkpoint repeatedly.

Before:
```csharp
if (_customEmployeeWalkingPaths) { /* add saved job route points */ }
result.Add(_bays[slot].Position);
```
After:
```csharp
if (_customMechanicJobRoutes) { /* add existing saved job route points */ }
result.Add(_bays[slot].Position);
```

Constructor/Save add the optional per-save job-route flag. GetMenuItems/GetMenuValue/menu help and ExecuteMechanicWalkingSelection expose Advanced Break <-> Bay Routes (DIRECT default / SAVED WAYPOINTS); changing it requires a closed shop and preserves every old point/mask. Existing row numbers 0-8 stay unchanged, advanced toggle is row 9 and Back is row 10. WalkCurrentNode and retry calls pass directJob to IssueEmployeeWalkTask, so direct job legs use GTA navmesh instead of being forced straight by a custom arrival setting. Existing finite retry/blocked-state logic and HasJob remain intact. Full exact source.diff is supplied.

## Additive migration, settings and rollback

New OPTIONAL Save.ini key: [EmployeeSettings] CustomJobRoutes=false by default, including old saves. This intentionally chooses simpler direct work legs for the corrective milestone. Opt in through the new menu row to reuse all existing ServiceWaypoint, BayApproach and JobOutboundMask/JobReturnMask values. No old key, route coordinate, inventory, job, money, order or timer is deleted/reset. Settings.ini defaults and existing CustomWalkingPaths/pickup settings are unchanged. Advanced job routes operate independently of arrival mode. The toggle is saved once on deliberate input, not per-frame.

Install only Install/scripts/GlassHeroesBusiness.2.cs after preserving the prior script outside the active scripts folder. Keep all existing Save.ini/Settings.ini; packaged example settings are reference only. Never activate both source copies, historical RC1 or a compiled business DLL alongside it. Exit GTA and restore the previous script for rollback; older versions ignore the optional flag and use their former routing logic. Do not reset saves. Last user-tested rollback remains v0.3.2.9a diagnostic.

## Automated validation and pending GTA regression

Tests execute actual BuildRoute, StartRouteLeg, WalkCurrentNode, IssueEmployeeWalkTask and UpdateEmployeeActivity with captured native calls and minimal worker state. They exercise direct/advanced routes, masks and gaps, legacy fallback, navmesh choice, finite work/break transitions and bounded blocked retries without HasJob loss. Financial repair simulation and real GTA pathfinding are not executed. Actual SHVDN2 Release compilation is separate.

Repeat activated-SDK checks: python3 verification/run_issue_tests.py 7; python3 verification/run_static_checks.py /tmp/gh-static. Exact build and validation logs accompany this guide.

GTA NOT RUN: three cold launches and boot gate; old save with custom arrival/work points defaults to direct break/bay legs while retaining every coordinate/mask; complete several jobs and return to break without detours; opt in to advanced mode while closed and confirm outbound/reversed return masks and approaches; verify arrival and car pickup routes remain unchanged; induce an obstruction and verify finite retries/manual blocked state rather than respawn loops or cancelled jobs; save/reload each mode, stream workers in/out and repeat with LS RED/heavy mod stack. >30m idle and off-session delivery checks remain NOT RUN. No immediate user test requested and no crash/stability claim.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #7: 7/7 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: 085979984854d9ffbd2c299566c71ca60ddc8e1f11d2755401259d0381a4e2e5.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
