# Glass Heroes v0.3.3.1 QA R2 — EXPERIMENTAL

FINAL CHATGPT REVIEW PENDING. GTA testing NOT RUN. Windows build NOT RUN. Stack-overflow cause UNPROVEN. This is a combined source candidate for final review before any in-game testing, not a production or stability qualification.

## Scope, provenance and branch policy

Branch: qa/v0.3.3.1-final-review-r2. Base: PR #16 source head 2bca18a8f4b345befbab5a2be2b9645eb228e0c2. Explicitly integrate crash instrumentation from PR #9 source commit 4727bdabdf67efd07810f5a62e5d1fdb202464e3 and correct the warehouse retry lockout identified in the supplied PR #16 review. integration.json records exact heads and diagnostic hashes. PR16_REVIEW_FEEDBACK.md preserves the user-supplied review; direct GitHub review reads returned Forbidden, while native Git fetch verified PR heads.

Only PR #9's production diagnostic changes and matching tests are brought into this candidate; older package/README changes from that branch are not blindly merged. The version-header conflict was resolved explicitly and the constructor banner now identifies QA R2. The logger region, OnTick, OnAborted, AppendDiagnosticLine and Log are hash-checked against PR #9. All earlier #2–#7 corrective source remains included.

Existing main, development and PR branches are not changed or merged. PRs #9–#16 overlap this candidate's monolithic source. Review ONE final combined source; do not independently merge the same changes through both the issue PRs and this candidate. The coordinator must choose one eventual integration path after review and real GTA qualification. No in-game test is requested by this development task.

## Target and exact source files

Target GTA V Legacy Story Mode 1.0.3889.0, ScriptHookVDotNet2 2.11.6, C# 7.3 and .NET Framework 4.8. Keep ScriptHookV/SHVDN prerequisites in the user's established game setup; no game installation, DLC, gameconfig, mod stack or live save was changed here.

Install/scripts/GlassHeroesBusiness.2.cs is the ONLY installation source. VisualStudio/scripts/GlassHeroesBusiness.2.cs is its byte-identical development copy, compiled by VisualStudio/GlassHeroesBusiness.csproj and solution. The final ZIP contains no compiled business DLL, nested older source ZIP or live Save.ini/Settings.ini. Historical RC1 is verification/baselines/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs.txt for static baseline checks only, never another active script. The checkout's original RC1 and older archives remain unchanged historical artifacts.

## Warehouse retry correction — issue #3 / PR #16

Previous UpdateWholesalePalletVisual permanently returned after five failed attempts while the player remained in range. The retained pair could remain missing even after models became available. A targeted test first failed against that actual old method with stationary recovery still blocked after retry budget; warehouse-regression-before-fix.txt records that expected regression failure, not a final-suite failure.

Before:
```csharp
if (_warehouseVisualAttempts >= 5 || now < _nextWarehouseVisualAttempt) return;
_warehouseVisualAttempts++;
_nextWarehouseVisualAttempt = now.AddSeconds(3);
```

After, UpdateWholesalePalletVisual delegates only its missing-object retry gate to new TryBeginWarehouseVisualAttempt:
```csharp
if (now < _nextWarehouseVisualAttempt) return false;
if (_warehouseVisualAttempts >= 5) _warehouseVisualAttempts = 0;
_warehouseVisualAttempts++;
_nextWarehouseVisualAttempt = now.AddSeconds(_warehouseVisualAttempts >= 5 ? 30 : 3);
return true;
```

Each batch admits at most five refresh attempts, with three seconds between attempts and thirty seconds after the fifth. With continuous failure the first attempts occur at t=0,3,6,9,12 seconds; the next batch starts no earlier than t=42. No request occurs during cooldown, or while both live props exist. Only missing objects are retried, so an already grounded pallet survives a late box-model recovery. Streaming cleanup still safely resets the session state. Requests run only in the existing visual refresh after gameplay activation; there is no per-frame request loop, new service or model-loading thread. Existing bounded individual model requests and fallback pallet are unchanged.

The ready marker/hint remains independent of vehicles and model availability. Correct assigned vehicle identity is still required when loading cargo. Carried, vehicle-attached and non-wholesale crates remain protected. No retry resets an order, issues an invoice, changes ownership, credits stock or advances business/delivery time. This corrects the automated retry defect; real GTA warehouse rendering and MLO collision remain NOT RUN.

## PR #9 crash diagnostics — issue #1

Changed constructor: identify the R2 build in the boot banner. Added managed _diagnosticTickStage, constant phase assignments in OnTick, and that phase in recoverable error context. The existing startup hold/delay/player-validity gate remains. OnTick samples shift, remote mode, staff-range flag, menu state, supplier stage and customer/remote order counts after the original throttled visual refresh. OnAborted logs entry/last phase and completion around the existing cleanup/save path.

Before:
```csharp
Log("Tick", ex);
```
After:
```csharp
Log("Tick / " + _diagnosticTickStage, ex);
```

DiagnosticLog.RecordTransition records changed observed state at most once per ten seconds; stable idle produces one initial state record, then no checkpoint heartbeat. Rapid transitions coalesce, so this is not a full event audit. DiagnosticLog.Write admits at most one exception record per ten seconds across contexts, reports suppressed count on the next admitted error, prevents writer reentry and backs off storage failures for thirty seconds. AppendDiagnosticLine writes the existing Errors.log path; Log delegates to the single recorder. New timestamps are UTC and end in Z; old unmarked records keep their historical local meaning.

No new world natives, per-frame file writes or business progress are introduced by instrumentation. Core diagnostic code is unchanged from PR #9 and the nine real-logger behavior cases run against this combined source. Different exceptions inside the throttle window may lose details; failed storage drops records during backoff. Existing non-exception event logs keep their prior behavior. A fatal CLR/native stack overflow may terminate before managed catch/abort code runs; these diagnostics neither catch it nor establish its cause. The reported 0xC00000FD at clr.dll+0x19E9DD around 18m06s idle remains unresolved without independent crash forensics.

## Retained corrective behavior — issues #2–#7

- #2: OnKeyDown/OnKeyUp/PollController share semantic input latches. Held Enter cannot satisfy another confirmation through a mapped frontend echo or submenu change. Back restores the correct parent rows and editor cursor; SET/SAVE labels reflect stored state. Controller-only cadence remains; deliberate cross-device switching inside 250ms may be suppressed.
- #3: ready warehouse pair/marker/hint and collection stream independently of the assigned vehicle; actual LOAD retains plate/model checks. The renewed retry policy above replaces the old lifetime attempt cap.
- #5: BeginSupplierDriverWalk and bounded UpdateSupplierPlayerApproach target the current player directly in established player-handoff mode. Standard drop-off keeps configured stops. Arrival waits with the same box. Excessive distance, missing cargo, fifteen repaths or ninety-second approach expiry defer owned visuals and preserve the pending order with thirty-second retry delay. A game pause may expire the visual chase after resume; financial progress is not changed.
- #6: obsolete _storedCrateProps ownership is separate from indexed _deliveredPackageProps. Historical untagged stock-display reconstruction is hidden. Real new deposited parcels are retained during dirty refresh and reconstructed only by saved provenance. Cleanup does not delete MLO/world entities, active Express cargo or warehouse pickup props.
- #4: ParkingOutlineVertices uses GTA forward (-sin heading, cos heading), with cardinal/diagonal checks and a clear front arrow. MarkParkingSquare keeps caller distance/toggle bounds and never changes saved heading.
- #7: BuildRoute defaults work legs directly to bay/break destinations via navmesh. Existing points/masks/approaches remain for explicit advanced opt-in, independent of arrival/car-pickup mode. Sparse arrival gaps no longer duplicate the old checkpoint. Bounded activity retry/blocked state retains HasJob.

Exact changed methods and every production delta are in source.diff against integration and changes_from_PR16.diff against the previous combined candidate. The original PR #9 source patch is included separately as provenance; apply the final complete source or final diff, not multiple overlapping patches.

## Save/config preservation and rollback

R2 adds NO new save/config key or default relative to PR #16. Existing optional [Storage] DeliveredPackageMask1..4 (default 0; low nine bits clamped to BoxCount) and [EmployeeSettings] CustomJobRoutes=false remain. Old untagged display reconstruction is intentionally hidden, but historical BoxCount, stock, capacity, jobs, pending orders, money and invoices remain authoritative. New deliveries keep their physical parcel and provenance through stream/reload. Provenance is not invented for older-build deliveries.

Direct work routes are the existing corrective default; enable Advanced Break <-> Bay Routes while closed to reuse old service points, approaches and masks. No old route coordinates or masks are deleted. Settings.ini gains no required key. HoldGameplayTick=false and StartupDelaySeconds=30 remain defaults and existing overrides remain in force. Input/log/visual cooldown wall clocks never create offline earnings, wages, manager work or supplier progress.

Preservation guards verify nine core financial/timer/identity methods byte-for-byte against historical RC1, normalize only PR #9's documented instrumentation out of OnTick/OnAborted before comparing their gameplay logic, and compare actual diagnostic implementation hashes to PR #9. CommitSupplierDelivery's financial operations are unchanged; its pre-existing R1 visual-provenance addition is separately checked. All old literal save/config keys remain, with only the two previously documented additive key prefixes. These are source/behavior checks, not proof of GTA file persistence.

After final review, back up the prior script OUTSIDE the active scripts folder. Install ONLY Install/scripts/GlassHeroesBusiness.2.cs into the game's scripts folder. Preserve EVERY existing scripts/GlassHeroesBusiness/Save.ini and Settings.ini. The packaged .example file is reference only. Do not copy the entire archive or the Visual Studio copy into the game, and do not install a compiled business DLL alongside the source. Exactly ONE active Glass Heroes script is required.

Rollback after exiting GTA by restoring the preserved prior source and keeping existing saves/settings and unknown optional keys. Older versions ignore the extra keys and may restore old decorative rendering/routing behavior. Never reset saves as part of rollback. The last user-tested fallback remains v0.3.2.9a diagnostic; neither RC1 nor R2 is certified stable.

## Automated checks and repeatability

The final pipeline runs actual net48 Release compilation against SHVDN2 2.11.6, original 21 static assertions, all six corrective harnesses, PR #9's nine logger tests, and source-preservation/provenance guards. The warehouse suite now has fourteen cases; other corrective counts remain 10/7/8/8/7 for issues #2/#4/#5/#6/#7. All final outcomes are recorded below and in validation.json, build.txt, behavior-tests.txt, diagnostics-tests.txt, StaticVerification.json and preservation.json. The earlier expected regression failure is labeled separately.

Harnesses compile extracted production callbacks/helpers with GTA-facing APIs stubbed. Logger tests include 30 simulated minutes of stable state, exception flood, disk failure/backoff and reentry. Simulation is not a thirty-minute GTA session. StaticVerification.json's compiled=false describes the static runner alone; actual application compilation is recorded separately. No skipped/zero-test result is counted as a pass.

From an activated cloud SDK checkout:
```bash
source /workspace/.cloud-setup/env.sh
python3 verification/prepare_experimental_package.py --label qa-r2-final-review --issues 2 3 5 6 4 7 --include-diagnostics --final-review
```

For isolated verification, run run_issue_tests.py with 2 3 5 6 4 7, run_diagnostics_tests.py, run_static_checks.py with an external results directory, and check_preservation.py. The installed checksum-verified SDK is used through the cloud activation script. On Windows, open the included solution with the .NET Framework 4.8 developer pack and restore pinned NuGet dependencies; Windows compilation remains NOT RUN. The package includes a non-active RC1 .txt fixture so static/preservation tests can resolve their baseline after extraction. Regenerating Git diffs/packages requires the checkout and recorded refs.

## Final review and GTA matrix — every game scenario NOT RUN

ChatGPT should review the feedback resolutions, full diffs, PR #9 provenance, real test transcripts, save migration/rollback, archive manifest and one-active-script instructions before authorizing an in-game test. FINAL_REVIEW_HANDOFF.md records the overlapping PR policy and exact heads.

QA_TEST_PLAN.md preserves three cold starts, >30m idle with LS RED/heavy mods, input parity and nested confirmation, cardinal/diagonal marker alignment, MLO ground elevation, warehouse transient-failure recovery, wrong/correct vehicle loading, direct versus ordinary handoff including pause, old/new visual masks, custom-route migration, shop streaming, teardown, save/reload and no off-session progress. All remain NOT RUN. Capture logs and a supported crash dump if the idle stack overflow recurs; the final checkpoint is context, not proof of causation. No main/development merge or user stress-test begins in this task.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #2: 10/10 behavioral tests PASS; Issue #3: 14/14 behavioral tests PASS; Issue #5: 8/8 behavioral tests PASS; Issue #6: 8/8 behavioral tests PASS; Issue #4: 7/7 behavioral tests PASS; Issue #7: 7/7 behavioral tests PASS.

PR #9 diagnostic logger tests: 9/9 PASS; total behavioral tests: 63/63 PASS.

Install and Visual Studio sources have equal SHA256: 72f9e58d51a54584a1141a67740a2c4154647f65624c5107b39fdb8530315445.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
