# v0.3.3.1 issue #2 — menu input parity — EXPERIMENTAL

Issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/2
Branch: fix/2-menu-input-parity. Target: development/v0.3.3.1. Review PENDING. GTA testing NOT RUN. The stack-overflow cause remains UNPROVEN; PR #9 diagnostics are not bundled or changed here.

## Changelog and exact changes

VisualStudio/scripts/GlassHeroesBusiness.2.cs and Install/scripts/GlassHeroesBusiness.2.cs match exactly. Original root RC1 and existing archives remain historical reference files.

OnKeyDown, OnKeyUp and PollController now share MenuInputGate for semantic Up/Down/Accept/Back/Open actions. A keyboard hold remains latched through submenu changes and cannot trigger a second mapped frontend action; keyboard-first and controller-first echoes are suppressed for 250ms across sources. Each key release ends its hold and suppresses a delayed frontend release echo. A fresh keyboard press remains eligible without a same-device cooldown. Escape/Back aliases are counted independently. Existing controller-only 180ms polling cadence and tested bindings are unchanged. No input loop writes files.

NavigateBack restores Arrival, Service and Approach rows 2/3/4, and service vehicle bays to row 6 under Customer Placements or row 4 under Driving Options. Existing editor return page and cursor are preserved. GetMenuValue calls PointEditorActionLabel so unset, update/save, clear and marker state agree with stored state.

## Before / after excerpts

Before:
```csharp
else if (PadJustPressed(_controllerAccept))
{ ExecuteSelection(); }
```
After:
```csharp
else if (PadJustPressed(_controllerAccept) && _menuInput.TryController(2, now.Ticks))
{ ExecuteSelection(); }
```
Before: Arrival/Service/Approach returned to rows 1/2/3, and service vehicle bays always returned to row 4. After: correct rows 2/3/4 and parent-specific row 6 or 4. Full source.diff is included and checked against both original sources.

## Save/config compatibility and install/rollback

No Save.ini or Settings.ini keys or defaults change. No business timer, money, stock, order, job or saved heading changes. Existing HoldGameplayTick and StartupDelaySeconds remain unchanged. The echo window is input arbitration only; it never advances gameplay or offline progress. A deliberate immediate switch between devices for the same action inside 250ms may be suppressed.

Install only Install/scripts/GlassHeroesBusiness.2.cs into the game's scripts folder after backing up the previous script outside that folder. Preserve all existing Save.ini and Settings.ini files. Packaged example settings are reference only. Never install both source copies, the historical RC1 source, or a compiled business DLL alongside the active source. Roll back by exiting GTA and restoring the previous source; no save migration/reset is needed. The last user-tested rollback remains v0.3.2.9a diagnostic, not a stability certification for RC1.

## Validation and pending GTA regression

The harness executes actual OnKeyDown, OnKeyUp, PollController, NavigateBack, OpenPointEditor, input gate and label methods with GTA/control calls stubbed. Its selection executor counts logical actions; game purchases and natives are not executed. It covers keyboard/controller/mixed order, hold/release, Back aliases, boot gate, nested editor, cursor and labels. Source-only compilation is separate from harness compilation.

Repeat cloud checks after activating SDK: python3 verification/run_issue_tests.py 2; python3 verification/run_static_checks.py /tmp/gh-static; dotnet build VisualStudio/GlassHeroesBusiness.csproj with external intermediate/output paths as recorded in build.txt.

GTA NOT RUN: three cold launches, boot gate/hold setting, Enter once and held through purchase/cancel confirmation, Escape/Back through nested editor and each corrected parent, controller-only regression, mixed inputs, saved marker labels after SET/reload, shop-distance streaming, save/reload with pending orders and heavy mod stack including LS RED. Test before production use; no user test is demanded while away. Global milestone cold-start, >30m idle and save/reload matrix remains NOT RUN.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #2: 10/10 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: 7c83b91b3b732485c92101e561b623f842c460c15b6a43efbe28e529a6abe885.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
