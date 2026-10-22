# v0.3.3.1 issues #2–#7 — EXPERIMENTAL QA CANDIDATE

REVIEW PENDING. GTA testing NOT RUN. Stack-overflow cause UNPROVEN. No stability, production approval, crash-resolution or in-game validation claim is made.

This is an isolated candidate assembled on qa/v0.3.3.1-review-pending from six independently tested issue branches. It is prepared for review as requested, not merged into development/v0.3.3.1 or main. The handoff's integration/review gate remains outstanding: review the individual issue changes before treating this archive as an approved combined tester build. No immediate user test is demanded while the user is away.

## Included changes and exact source

Target: GTA V Legacy Story Mode 1.0.3889.0; ScriptHookVDotNet2 2.11.6; C# 7.3; .NET Framework 4.8. The single install source is Install/scripts/GlassHeroesBusiness.2.cs. It matches VisualStudio/scripts/GlassHeroesBusiness.2.cs byte-for-byte. The root RC1 source and existing archives remain historical reference files.

- #2: shared semantic keyboard/frontend input gate for Enter, Back, navigation and menu open. Holds survive submenu changes; mapped echoes in either order are suppressed; controller-only cadence remains. Correct Back rows and point-editor labels. Changed OnKeyDown/OnKeyUp/PollController/NavigateBack/GetMenuValue plus MenuInputGate and label helpers.
- #3: ready warehouse pallet/box/marker and pickup instructions are independent of assigned-vehicle proximity. Correct plate/model vehicle still required to LOAD. Bound visual retries and preserve carried/vehicle/Express cargo. Changed UpdateWholesalePalletVisual, CreateWarehousePallet, DrawWholesalePackageMarker, ProcessSupplierDelivery, interaction/hint gates and cleanup.
- #5: existing player-handoff mode bypasses supplier waypoints, approaches the current player directly with bounded repathing, holds the same box and waits for explicit pickup. Standard drop-off retains saved waypoints. Changed BeginSupplierDriverWalk, UpdateSupplierPlayerApproach, DeferSupplierPlayerHandoff, ProcessSupplierDelivery and owned cleanup.
- #6: hide historical untagged stock-display box reconstruction, retain real new parcels with separate indexed ownership and additive visual provenance. Preserve inventory/capacity and active packages. Changed UpdateSupplierPalletVisual, DeleteStoragePalletVisuals, legacy cleanup, constructor/Save, CommitSupplierDelivery's visual slot metadata and deposit ownership transfer. StoredBoxPosition's grounded support math is preserved.
- #4: correct GTA forward axis (-sin heading, cos heading), draw a front arrow with the car outline. Changed MarkParkingSquare and new ParkingOutlineVertices; saved headings and draw-distance/toggles stay unchanged.
- #7: default work legs directly to bay/break using navmesh; optional saved paths/masks/approaches remain via explicit advanced opt-in. Changed BuildRoute, walking-task selection/retry arguments and menu/load/save flag; sparse arrival gaps no longer duplicate the legacy checkpoint. Finite retry/blocked state and HasJob remain intact.

Not included: PR #9 issue #1 logger changes, PR #8 documentation audit, SHVDN3 migration, new economy features, or fixes to the still-unproven stack overflow. Those existing branches were not edited. Each original issue package contains that issue alone; do not stack install source files from multiple archives.

## Before and after examples; complete diff

Before #2: keyboard Enter and frontend Accept independently executed selection. After: both use the semantic input gate, including a held key across submenu changes.
```csharp
else if (PadJustPressed(_controllerAccept) && _menuInput.TryController(2, now.Ticks))
```
Before #3: ready box spawning required an assigned vehicle within 24m. After: ready cargo streams by player range, while existing correct-vehicle LOAD checks remain.
Before #5: player handoff visited fixed driver waypoints. After: direct mode targets player.Position; standard mode retains the waypoint loop.
Before #6: every historical BoxCount generated decorative props. After: only provenance-tagged delivered slots reconstruct, and existing real delivered props are reused.
Before #4: forward X used +sin heading. After: -sin heading with an unambiguous forward arrow.
Before #7: enabling custom arrivals also enabled job-route detours. After: direct job routes default independently; existing saved points are used only with advanced opt-in.

Full source.diff applies to the recorded integration baseline and reproduces both complete candidate sources. Each issue's separate developer guide and changelog provides literal source changes, method names, compatibility details and its own recorded results.

## Save compatibility and deliberate migrations

No live business Save.ini or Settings.ini was read, overwritten, reset or shipped as an active save by these development commands. Existing business money, inventory, jobs, pending identities, payment and delivery timer keys remain. Preservation guards compare eleven core methods against the integration baseline, including OnTick and business/remote economy/timing functions, and verify that the financial commit only gains visual provenance. These are source checks, not proof of game I/O.

Additive visual keys: [Storage] DeliveredPackageMask1..4, default 0, low nine bits clamped to each existing BoxCount. Old saves cannot distinguish original package provenance from synthesized stock displays. Untagged historical display reconstruction is intentionally hidden, with all BoxCount/stock/capacity data preserved. New deliveries keep their same live parcel and persist its provenance for stream/reload. Older-build deliveries do not gain inferred provenance. This is documented visual behavior, not a stock conversion.

Additive route key: [EmployeeSettings] CustomJobRoutes=false by default, including existing saves. This intentionally chooses direct Break/Bay legs. Enable Advanced Break <-> Bay Routes while the shop is closed to reuse all old service waypoints, final approaches and outbound/return masks. No old route coordinates/masks are erased. Existing arrival and pickup settings remain independent. Settings.ini has no new key requirement.

HoldGameplayTick=false and StartupDelaySeconds=30 remain existing defaults and existing overrides are honored. All business and supplier progress remains governed by the original active-game logic. New wall-clock bounds govern input arbitration or visual retry only. A paused chase may time out its visuals after resume; its paid pending order survives. No closed-session catch-up or per-frame save is introduced.

## Safe installation and rollback (after review)

Back up the prior script outside the active game scripts folder. Copy ONLY Install/scripts/GlassHeroesBusiness.2.cs to the game's scripts folder. Preserve all existing scripts/GlassHeroesBusiness/Save.ini and Settings.ini. Packaged example settings are REFERENCE ONLY and must not replace an existing settings file. Do not copy the whole archive to the game; do not activate the historical RC1 source, Visual Studio source copy, or a compiled business DLL alongside the install source. Exactly one active Glass Heroes script is required.

To roll back: exit GTA, restore the preserved script, keep existing save/settings and optional unknown keys. Older versions ignore new keys and may show old decorative rendering/routing again. No save reset is required. The last user-tested rollback is v0.3.2.9a diagnostic; RC1 was never certified stable.

## Automated validation scope

Actual Release compilation targets net48 and pinned SHVDN2 2.11.6 with the installed SDK. Original static assertions run on this combined source through path redirection; the reconstructed previous baseline is SHA256-checked. The six isolated harnesses execute extracted production callbacks/helpers against game/native stubs. Their 49 behavioral cases cover input parity and holds, warehouse streaming/retries, direct/regular handoff, provenance/stock commit, heading geometry, direct/advanced work legs and bounded lifecycle behavior. Stubbed runtime actions are not GTA tests or real persistence proofs.

validation.json, build.txt, static-checks.txt, StaticVerification.json, behavior-tests.txt, preservation.json and preservation-checks.txt contain actual outcomes. Per-issue results describe their independent source; the combined source SHA256 below is the candidate to review. The package manifest verifies archive CRC, source equality, guide content and archive SHA256.

Repeat after activating SDK:
```bash
source /workspace/.cloud-setup/env.sh
python3 verification/run_issue_tests.py 2 3 5 6 4 7
python3 verification/run_static_checks.py /tmp/gh-static
python3 verification/check_preservation.py /tmp/gh-preservation.json
```
Run the recorded dotnet build command with writable external intermediate/output paths. A Windows build and GTA runtime remain NOT RUN.

## GTA test plan — every scenario NOT RUN

After issue reviews, the first tester session should prioritize three cold launches, then >30 minutes of shop idle with LS RED and the normal heavy mod stack. Preserve session logs, mod versions, save backups and crash evidence. The 49 synthetic cases are not a 30-minute GTA session.

Then test keyboard Enter/Back once and held through nested menus and confirmation stages; controller-only and mixed controls; point SET/SAVE labels and Back cursor restoration. Test warehouse-ready cargo with assigned vehicle far away, wrong/correct vehicle loading, then collect/load/unload/deposit once. Compare ordinary saved-waypoint drop-off with direct player handoff, moving-player/timeouts and recovery. Verify parking front arrow and long axis at 0/90/180/270 and diagonal headings. Check no old floating stock displays, correct new Express delivery placement, provenance persistence across reload and unchanged stock/capacity. Complete several mechanic jobs in direct mode and advanced opt-in, including obstructions and car pickup/arrival independence.

Repeat shop-distance streaming, script reload/abort, save/reload with pending jobs/orders, recover/cancel, and a closed-GTA delay to verify no offline progress or duplicate money/stock/invoices. Use the full docs/V0_3_3_1_TEST_PLAN.md matrix. Capture a supported crash dump if the idle overflow recurs; do not attribute the last checkpoint to a root cause. No main merge or production release is authorized by these artifacts.

## Branches and draft PR status

All six issue branches are pushed. GitHub API returned Forbidden for every separate draft-PR creation attempt; no new PRs were created. REVIEW_HANDOFF.md and PR_STATUS.json list exact heads and branch links; PR_DRAFTS/ contains ready descriptions and captured errors. Existing PR #9 and PR #8 remain separate. The inherited BOOT text still names the earlier diagnostic baseline; identify this candidate by its source header and SHA256 manifest rather than that pre-existing banner.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #2: 10/10 behavioral tests PASS; Issue #3: 9/9 behavioral tests PASS; Issue #5: 8/8 behavioral tests PASS; Issue #6: 8/8 behavioral tests PASS; Issue #4: 7/7 behavioral tests PASS; Issue #7: 7/7 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: c280bf5adaee22bb174302505a02e11896ffc56c366a64906473c92c5595a813.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
