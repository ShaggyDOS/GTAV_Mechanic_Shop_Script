# Glass Heroes v0.3.3.2-WP1 waypoint-editor diagnostics — EXPERIMENTAL

October 23, 2026. Independent investigation/shvdn3-waypoint-editor-diagnostics branch/worktree based on preserved 422baa756ce16f6af370278105731de2e82abdd3. No finance changes or merges. Includes the verified supplier/menu/cart/pause candidate by ancestry; it does not replace those branches or establish the crashed deployment identity. Current code is reviewable instrumentation, NOT a proven crash fix.

## Session investigation record

User-reported crash: October 23, 2026 at 12:41:09 AM, 41m08s gameplay, editing business waypoints; 0xC00000FD STACK_OVERFLOW at clr.dll+0x688D5. User reports successful compilation/start of one GlassHeroesBusiness.3.cs on SHVDN Enhanced 3.9.0.6. Errors(10).log startup family: v0.3.3.1 SHVDN3 ECONOMY + PALLETS + WAGES + SAVED QUOTES EXPERIMENTAL (#27, #25). That label was reused. Exact deployed Git commit: UNVERIFIED. Exact deployed source SHA256: UNVERIFIED. Do not attribute aad2dff, 422baa7 or later feature hashes to this crash without the actual session source. The hash in this package identifies ONLY this new diagnostic candidate.

Raw matching Crash Analyzer, ScriptHookVDotNet, RPH and Glass Heroes logs have NOT been received/read in this task; statements above are user-provided summaries. User reports no named responsible SHVDN script or fatal managed exception, with earlier customer/mechanic parking failures. These are correlation leads, not causal stacks. A CLR stack overflow can terminate before ordinary catch/log execution, so an Errors.log ending without a managed fatal exception does not exonerate or implicate this script.

Recurring investigation: previous reports included 0xC00000FD in clr.dll at offsets +0x19E9DD/+0x19F47B, with no matching managed call stacks establishing a script cause. The new +0x688D5 remains a module/offset observation. Different offsets, waypoint editing and prior parking failures do not establish identical call chains. Compare exact runtime/module versions, session clocks, deployed source hash and managed/native stacks. Do not substitute the earlier unrelated 0xE06D7363 startup/spdlog incident or its dump for this session's overflow.

## Source audit and actual findings

Audited current preserved source, not an identified deployment. OnKeyDown/PollController -> ExecuteSelection -> ExecutePointEditorSelection dispatches one ConfigureWorkBay/ConfigureVehicleParkingBay/ConfigureArrivalPoint/ConfigureRoutePoint action. OpenPointEditor assigns managed editor state and return page/row. NavigateBack assigns the parent menu. Save -> TryCommitBusinessSave -> ScriptSettings snapshot -> atomic file replacement has no call back to editor entry/capture. GetMenuItems/GetMenuTitle/descriptions render managed data, not save or coordinate capture. Tick does not repeatedly invoke waypoint save. The only subscriptions are ordinary Tick/key/abort handlers; no point-changed, save-completed or filesystem callback installs an editor cycle. No direct source-level recursive editor/save call chain was found.

Two defensive boundaries were absent: nested OpenPointEditor could overwrite its parent with PointEditor (a self-return navigation state, not demonstrated stack recursion), and editor action/save callbacks had no explicit reentry guard. New guards decline nested entry/action/save without recursively recovering or writing a second save. Existing semantic keyboard/controller arbitration remains the first duplicate-input defense. This is defensive instrumentation/hardening, not proof either path occurred in the crash.

Placement reads player position/heading at explicit action boundaries. It can dismiss GH-owned workers/receptionist and clear owned pallet visuals; it does not scan the entire world, teleport player vehicles, spawn new actors in coordinate capture or perform a blocking native wait. Existing vehicle placement checks the current customer order; route edits/clear have existing closed-shop/proximity restrictions. Entity validity may change during native work and ordinary exceptions are logged by the existing input handler; missing global dump stacks prevent attribution. Earlier parking failures concern asynchronous driving/task paths, whose temporal relationship to placement must be established from matching logs.

## Implemented bounded diagnostics

New WaypointTrace uses an injected clock/writer, reentry guard, exception containment and maximum 24 events per ten-second UTC window, with suppressed count on the next emitted event. Payload caps at 512 characters. No Tick heartbeat, world scan, extra coordinate/native query or repeated save is added by logging. TracePointEditor reads only managed label/menu/selection/sequence, cached game-time sample, customer count and supplier stage. Existing DiagnosticLog writer retains disk-error backoff and recursion containment. Diagnostic wall time throttles logging only; every economy/delivery clock and OnTick remains byte-identical to baseline.

Events: entry and rejected nested entry; action-begin; capture-begin/capture-complete with the already captured coordinate; save-begin/save-committed/save-failed/save-error; action-exit/error-recovery; Back/key-close/controller-close/native-pause exit. Each editor entry gets a managed sequence number. SaveEditorPoint delegates exactly once to the existing Save on a normal action. Existing old UI success text/failure-memory semantics are not broadly rewritten; use save-committed versus save-failed as the durability evidence. No saved point coordinates/defaults, schema, suppliers, financial counters, native routing tasks or business data are changed by diagnostics.

One startup-only identity event hashes the conventional scripts/GlassHeroesBusiness.3.cs file, capped at 2 MiB. Missing/oversized/unreadable file logs UNVERIFIED. This is a startup file hash at that path, not a claim about an unknown past deployment or a compiled module if the source changed or was loaded from another name/path. There is no per-Tick file hashing. Keep the exact deployed source file alongside logs for future comparison.

Exact executable file: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. Changed: constructor identity call; OpenPointEditor, ExecutePointEditorSelection and four Configure methods; NavigateBack, OnKeyDown, PollController and HideBusinessMenuForPause for explicit exit checkpoints. New helpers/class: WaypointTrace, TracePointEditor, SaveEditorPoint and LogWaypointBuildIdentity. Save/OnTick and all financial/supplier/repair/sales/wage methods remain unchanged, enforced by preservation.json/source.diff and protected v2 hashes.

```csharp
// Before: nested entry could replace parent with PointEditor; save had no scope trace.
_editingPoint = point;
Save();
// After: nested editor entry is rejected, and existing save is bracketed once.
if (_pointEditorBusy || _menuPage == MenuPage.PointEditor) { TracePointEditor("entry-reentry-blocked"); return; }
TracePointEditor("save-begin"); bool saved = Save();
TracePointEditor(saved ? "save-committed" : "save-failed");
```

Full source.diff reproduces this exact diagnostic source from 422baa7. No patch was applied to financial-transparency or supplier feature branches.

## Owner installation and test procedure — NOT RUN

Do not overwrite a sole copy of the experimental build or business data. Exit GTA; back up the exact installed .3.cs and complete scripts/GlassHeroesBusiness directory together, including Save.ini/Settings.ini and logs. Calculate SHA256 of that preserved source if it is the session file; keep unidentified files marked UNVERIFIED rather than infer from startup labels. On Windows: Get-FileHash -Algorithm SHA256 .\scripts\GlassHeroesBusiness.3.cs.

Review this ZIP outside GTA. Install ONLY experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs to GTA scripts/GlassHeroesBusiness.3.cs if you choose to test this independent diagnostic candidate. Never load duplicate/v2+v3 Business Managers. Retain existing runtime ASI/DLLs, game files, saved coordinates/settings and full mod stack. Do not install this alongside the independent finance candidate; compare separately on matching source/data backups. Roll back with GTA closed by restoring the full matching source/data backup. Mixed-order rollback rules remain: complete/cancel pending mixed orders before older source, or restore matching full backup.

Cold launch; retain startup identity line, SHVDN/RPH/Crash Analyzer logs and Errors.log. Open each employee/customer/vehicle/supplier/pallet point editor, set/reposition/clear/view/back; test Enter/controller duplicate echo, held keys, repeated entry, native pause and invalid/unavailable player conditions in a disposable QA session. Verify one capture/save outcome per intentional action, correct parent/row, no changed stored coordinates on a read-only action, and bounded logs during repeated input. Cause ordinary save failure in a disposable test folder only and inspect save-failed/recovery checkpoints; never damage live data. Confirm repeated successful saves/reload preserve business, accepted repair promises, inventory, cart and waypoints. Test stream-out/in, shop distance and full heavy-mod travel/idle 60–120 minutes; this is not stability certification.

For crash correlation, provide matching session Crash Analyzer, SHVDN, RPH and Glass Heroes Errors logs plus actual deployed source if retained; a matching managed/native dump with thread stacks is strongest evidence. Align timestamps/time zones (12:41:09 AM timezone not independently verified; assume owner local America/Chicago only for an initial timeline), last WAYPOINT phase and earlier parking messages. A last checkpoint identifies the last completed boundary, not a proven faulting method.

## Reproduction

```text
python verification/waypoint/verify.py --label waypoint-audit --profile waypoint --suites repair sales pallets wages reload supplier range menu cart pause focus waypoint
python verification/waypoint/package.py waypoint-audit --parent 422baa756ce16f6af370278105731de2e82abdd3
```

Standalone tests use deterministic managed stubs and fault injection; no GTA/native crash reproduction or Windows execution was performed. All unchanged inherited features, 21 static predicates and method/hash boundaries are checked. The linked net48/C#7.3 SHVDN3 3.6.0 project builds the one installation source; clean extracted rebuild and package hashes accompany this guide. Installed Enhanced compatibility and crash root cause remain UNVERIFIED/UNPROVEN. No live installation, save/settings, main or QA R2 branch was modified.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

386/386 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 25, wages: 22, reload: 41, supplier: 37, range: 40, menu: 13, cart: 32, pause: 9, focus: 8, waypoint: 13). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game clock sampling and original v2 source/project are unchanged. Supplier route timing consumes that existing active-game sample. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 50bf63c16ec06fb9300dbc13246b3dda009411398ff812654a8d5163341702be. The complete unified source.diff applies to parent 422baa756ce16f6af370278105731de2e82abdd3 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
