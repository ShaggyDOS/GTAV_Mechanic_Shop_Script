# v0.3.3.1 issue #5 — direct player handoff — EXPERIMENTAL

Issue #5; branch fix/5-direct-player-handoff to development/v0.3.3.1. Review PENDING; GTA testing NOT RUN; stack-overflow cause UNPROVEN. PR #9 is separate and unchanged.

## Changelog, changed methods and before/after

Matching VisualStudio/scripts/GlassHeroesBusiness.2.cs and Install/scripts/GlassHeroesBusiness.2.cs: BeginSupplierDriverWalk now snapshots the existing player-handoff mode (a configured parts pallet). In that mode it builds exactly one target at the current player position. Regular automatic drop-off still visits all configured driver waypoints then the fixed handoff.

Before:
```csharp
foreach (WorkBay point in _supplierDriverWaypoints)
    if (point.IsSet) _supplierWalkingNodes.Add(point.Position);
_supplierWalkingNodes.Add(_supplierHandoff.Position);
```
After (player-handoff branch):
```csharp
_supplierWalkingNodes.Add(player.Position);
```

New UpdateSupplierPlayerApproach refreshes the direct target at most every three seconds while out of handoff range, with at most fifteen repaths and a ninety-second approach timeout. Within two metres the driver waits holding the same crate until the explicit player action. A player who moves away is followed within the same bounded approach session. No waypoint teleport recovery is used for direct mode. Lost cargo, invalid/dead player, excessive distance or exhausted budget calls DeferSupplierPlayerHandoff: clean only owned visuals, leave the pending order authoritative, and wait thirty seconds before visual retry. ProcessSupplierDelivery dispatches stages 3/4 to that helper only for direct mode and checks the retry gate before stage-0 spawn. DeleteSupplierVisuals resets direct-mode session state. Full literal source.diff and actual validation are supplied.

## Save/config, financial preservation and rollback

No Save.ini/Settings.ini keys or defaults change. Existing configured pallet remains the established player-handoff selector; no new option silently changes standard drop-off. Session-only route/retry fields are not saved. CommitSupplierDelivery, stock, money, invoice and pending identity code are unchanged. Time bounds govern visuals only, never business or off-session timer progress. If a chase times out the order remains pending rather than being charged or credited again. A long pause may expire the visual navigation window after resume; the pending order survives and retries. This is not a business clock.

Install only Install/scripts/GlassHeroesBusiness.2.cs after backing up the old source outside the active scripts folder. Preserve every existing Save.ini/Settings.ini; packaged example settings are reference only. Never activate both source copies, historical RC1, or a compiled business DLL with the install source. Roll back after exiting GTA by restoring the previous source; no save migration/reset. Last user-tested rollback is v0.3.2.9a diagnostic.

## Validation scope and GTA regression: NOT RUN

Harness executes actual BeginSupplierDriverWalk, UpdateSupplierPlayerApproach and DeferSupplierPlayerHandoff with navigation/cleanup stubbed. It checks direct versus regular route construction, moving-player repath interval/budget, arrival/wait, preserved pending counters, timeout/backoff and lost-cargo recovery. It does not execute GTA pathfinding or the unchanged stock-commit ledger. Real SHVDN2 compilation is separate.

Repeat activated-SDK tests: python3 verification/run_issue_tests.py 5; python3 verification/run_static_checks.py /tmp/gh-static. Exact build and result transcripts accompany this guide.

GTA NOT RUN: three cold starts and boot delay; direct handoff with saved detour waypoints must go to player; move before/after arrival; receive the same crate once, deposit once and verify stock/invoice; standard drop-off must still follow configured stops; timeout/failure/recover/cancel and save/reload with no lost/duplicate paid order; stream far away and back; test under LS RED/heavy mods. Global >30m idle and off-session progress checks remain NOT RUN. No user test requested while away and no stability claim.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #5: 8/8 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: c4dadbc6cbfeba69d975668574ede471a9b6d0b08d8da28e7460c98644068912.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
