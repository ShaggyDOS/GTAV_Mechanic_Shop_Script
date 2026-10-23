# Glass Heroes Business Manager v0.3.3.1 — SHVDN3 Phase 1 EXPERIMENTAL

Issue #20 / draft PR #21. Branch: migration/qa-r2-shvdn3-prep. Source of truth: QA R2 7693ed2167405f475dd4d36392c5df1f257fe178. This is a separate source-level API port and build assessment, with automated parity checks. GTA testing: NOT RUN. Windows CLR execution: NOT RUN. Installed Enhanced runtime compatibility: NOT VERIFIED. Crash cause: UNATTRIBUTED / UNPROVEN. No merge or live installation action was performed.

## Scope and review status

The updated AGENTS.md branch addendum authorizes this migration despite the historical v2-only instructions lower in that file. The tracked CODEX_QA_R2_SHVDN3_MIGRATION.md is the implementation handoff. GitHub GraphQL and REST returned Forbidden for Issue #20/PR #21; their direct text could not be retrieved, so the tracked handoff and actual R2 source/API were used. No missing review text was invented.

The original byte-matched v2 Install/VisualStudio sources, v2 project and solution remain unchanged. PR #18 forensic instrumentation and PR #19/#22 package-blip changes are absent. The existing separate GlassHeroesV3Probe is not a Business Manager and was not reused. Main and QA R2 were not modified. No code from those experimental feature branches was merged or cherry-picked.

The new source is experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. Its class is GlassHeroesBusinessV3Experimental. The separate Visual Studio project is GlassHeroesBusiness.V3.Experimental.csproj, net48 / C# 7.3, producing GlassHeroesBusiness.V3.Experimental. It links the installation source directly instead of maintaining a second active .3.cs copy. The source ZIP therefore has exactly ONE business .3.cs and no active .2.cs. The matching .sln and project compile that same physical source. Reference fixtures end in .cs.txt and are not installable scripts.

Phase 1 delivers a real v3 compile, API map, baseline/v3 extracted regression execution, dependency assessment, source parity and review package. Phase 2 engine/save interoperability and Phase 3 GTA qualification are still pending. Compilation is not an explanation or fix for the reported stack overflow.

## Exact reference and provenance

The product pins official ScriptHookVDotNet3 NuGet 3.6.0, assembly ScriptHookVDotNet3 3.6.0.0, fetched over HTTPS from api.nuget.org. At assessment time, NuGet's official version index ended at 3.6.0. Publisher metadata names crosire and contributors and links the upstream repository. dependencies.json records the exact artifact URL, package SHA256, SHA512 sidecar match, restore source and full restored-library set. api-metadata.json records assembly versions and references from offline PE metadata, without loading/executing GTA APIs.

Restored libraries: ScriptHookVDotNet3/3.6.0, Microsoft.NETFramework.ReferenceAssemblies/1.0.3 and Microsoft.NETFramework.ReferenceAssemblies.net48/1.0.3. Framework references include mscorlib/System/Core, Drawing and Windows.Forms. The v3 API references the SHVDN core assembly 3.6.0.0; that runtime dependency is not a redistributable package supplied here. The built Business Manager references ScriptHookVDotNet3 only, with no ScriptHookVDotNet2 reference. All 363 v2 Control names/values were compared with v3, all 43 distinct native hashes used by the port matched, and the ChaletLondon font index matched.

The owner reports Enhanced runtime 3.9.0.6 installed with both API generations. Its actual binaries were not supplied or executed. Stable 3.6.0 is the officially supported compile reference chosen under the handoff, not a claim of an exact Enhanced binary match. Runtime binding/native behavior remains an owner qualification gate. Do not replace or upgrade ScriptHookVDotNet.asi, ScriptHookVDotNet2.dll or ScriptHookVDotNet3.dll. This package includes no runtime DLL or product DLL.

References: https://www.nuget.org/packages/ScriptHookVDotNet3/3.6.0 ; https://github.com/scripthookvdotnet/scripthookvdotnet ; https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Script-Migration-Guide ; https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Getting-Started . The migration-guide assembly-name warning was not treated as a method-by-method conversion recipe.

## API changes and unchanged contracts

API_MAP.md and verification/migration/api-map.json record exact symbols, methods, compiler/XML evidence, risks and tests. v3-initial-build.txt truthfully records 25 compile errors from referencing the untouched API shape; v3-api-adapted-build.txt records the remaining unavailable Rectangle name; v3-api-resolved-build.txt records the first successful API compile. v3-build.txt is the final product build. Earlier transcripts refer to the initial development source location; the final project links the single installation source.

Panel now uses GTA.UI.ContainerElement with explicit top-left positioning and the existing dimensions/1280x720 draw formulas. Label uses GTA.UI.TextElement, GTA.UI.Font.ChaletLondon and Alignment.Left instead of UIText/centered=false. Four subtitle callers use GTA.UI.Screen.ShowSubtitle with unchanged text/durations. Notify uses GTA.UI.Notification.Show(message, false) with the existing routine-notice filter. No other mod's notifications are flushed.

SyncCustomerBlip and CancelCreatedCustomer delete only their existing owned blips via Blip.Delete instead of Remove. Seven prop freeze assignments use IsPositionFrozen instead of FreezePosition, preserving every boolean value and location. This includes warehouse, vehicle attachment, carried boxes, pallet deposit, office and equipment props. No new wholesale map blip was added.

PadJustPressed and PadPressed preserve enabled AND disabled queries for groups 0 AND 2. v3 Game helpers omit the group parameter; simply switching to them would lose this R2 contract. The replacement uses the corresponding four direct native queries with explicit group and Control index. Tests exercise each path and verify the no-input path is bounded at four calls. Existing menu de-bouncing and mixed-input arbitration are unchanged.

The two timed prop-model loading paths in UpdateOfficeDeskVisuals and CreateFixedEquipmentProp now request nonblocking and wait for IsLoaded on later existing refreshes. A HashSet tracks only this instance's requested visual models. Ready requests are released after creation, including failed creation; a release failure retains ownership for retry. OnAborted releases pending requests after the pre-activation save guard. There is no new timer, thread, background poller, world scan, synchronous Request(timeout), Script.Wait or infinite loop. Office retries remain three seconds, warehouse retries retain their tested batch/cooldown, and proximity checks are unchanged. First appearance can shift to the next permitted refresh, which needs GTA QA.

The API itself may perform internal native work or waits. Loaded-model checks precede World.CreateProp, and existing ped/vehicle paths already check their models. This source audit does not prove that no runtime callback/yield can ever occur in engine code. Existing Model wrapper construction remains before the gate; API/core hashing internals are outside the source parity guarantee. No constructor world/model request was added. These observations do not attribute the crash to a method or API generation.

A managed-only startup preflight runs before business data loading. It rejects canonical v2 Business .2.cs, v2 business DLL, the historical RC1 business filename, or simultaneous canonical v3 source and v3 business DLL. It does not delete/rename any file. It cannot detect arbitrary renamed/subfolder copies, already-loaded stale instances or every external loader; the owner must verify the actual SHVDN script list in any future test. Distinct v3 class/assembly names prevent the known v2/v3 assembly-name collision, but are not authorization to enable both generations.

## Source, save and financial invariants

check_parity.py reverses only the explicit approved API/lifecycle edits and requires the ENTIRE remaining source to equal the pinned R2 fixture byte-for-byte as text. It also checks original v2 source/project/solution hashes in the repository, one shared project/install source, absence of unrelated P0/P1 features and identical GetValue/SetValue key ordering. It pins approved input/helper additions to the reviewed API map. The added behavior is exercised independently by extracted-member tests.

OnTick, ProcessEconomy, ProcessRemoteOperations, EnterRemoteOperations, LeaveRemoteOperations, Save, CommitSupplierDelivery, cancellation, recovery and assigned-vehicle lookup are byte-identical to R2. The stock/invoice commit still occurs only after physical pallet stocking, not warehouse collection or vehicle loading. Prepaid migration, existing order identity, saved package provenance, inventory, money, invoices, jobs, worker profiles/routes and placement coordinates remain unchanged. No new Save.ini/Settings.ini key or schema migration is introduced. Both generations use the same scripts/GlassHeroesBusiness data root and the same filenames; they must never run together.

The original startup defaults remain HoldGameplayTick=false and StartupDelaySeconds=30, including the existing bounded delay and valid-player/unpaused gate. There is no offline accrual. The actual OnTick gate resets wall/game samples before activating systems. Paused ticks advance no economy, wages, supplier ETA or remote jobs. Supplier ETA/remote work use active game-clock samples with R2's two-second cap and jump rejection. The employee economy retains R2's existing clamped, unpaused Tick delta; no alternative wall-clock catch-up or across-session scheduler was introduced. Diagnostic/UI throttles may use UTC as before.

These are source and deterministic behavior checks, not proof of real Windows INI/native behavior under Enhanced. Real Save.ini loading/writing, culture/value round trips and gameplay state survival are NOT RUN in GTA and remain Phase 2 QA. No private owner save/settings contents were opened, copied into the repository, changed or published.

## Before and after excerpts

Input before:
```
return Game.IsControlJustPressed(0, input) ||
    Game.IsDisabledControlJustPressed(0, input) ||
    Game.IsControlJustPressed(2, input) ||
    Game.IsDisabledControlJustPressed(2, input);
```
Input after:
```
return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_PRESSED, 0, (int)input) ||
    GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)input) ||
    GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_PRESSED, 2, (int)input) ||
    GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, (int)input);
```
Prop creation before:
```
model.Request(250);
try { if (!model.IsLoaded) return null; /* create prop */ }
finally { model.MarkAsNoLongerNeeded(); }
```
Prop creation after:
```
if (!RequestVisualModel(model)) return null;
try { /* create prop only when loaded */ }
finally { ReleaseVisualModel(model); }
```
The excerpts omit unchanged creation statements; api-source.diff is the full R2-to-v3 comparison with every source change. source.diff is the additive experimental source/project patch from R2, without replacing the preserved v2 files. Do not apply the comparison patch to the live v2 script.

## Automated execution and reproduction

The exact original v2 source was independently compiled before porting. Its inherited behavior suite has 54 corrective cases plus nine logger cases, 63 total; the nine logger cases are INCLUDED, not an additional total. The v3 source executes those same templates with only one stub property renamed (FreezePosition to IsPositionFrozen), then executes 22 new migration cases. Production member bodies are extracted verbatim. No tests were silently removed or rewritten to change business expectations. Existing static predicates remain intact; the v3 adapter normalizes only the constructor identifier in checker memory. StaticVerification compiled=false describes static analysis, while the separate real build transcript proves compilation.

Migration cases cover all grouped enabled/disabled input paths, bounded calls, canonical duplicate-generation preflight, single-source acceptance, delayed/unavailable models, bounded model ownership, creation/release failures and retry, office model requests/retry/proximity, startup hold/delay/valid player, long closed-session activation, pause, active ETA/remote jobs, game-clock jumps, R2 caps and ETA save cadence. Tick-dependent financial service calls use deterministic stubs in that harness; whole-source parity independently confirms the actual production financial bodies are unchanged. Stub tests do not execute GTA natives.

From a repository checkout with .NET 8 SDK and Python 3 available, run:
```
dotnet build VisualStudio/GlassHeroesBusiness.csproj -c Release
dotnet build experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release
python verification/run_issue_tests.py 2 3 4 5 6 7
python verification/run_diagnostics_tests.py
python verification/run_static_checks.py /tmp/gh-v2-static
python verification/run_issue_tests.py --source experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs --v3-stubs 2 3 4 5 6 7
python verification/run_diagnostics_tests.py --source experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs
python verification/run_static_checks.py /tmp/gh-v3-static --source experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs --class-name GlassHeroesBusinessV3Experimental
python verification/migration/run_tests.py
python verification/migration/check_parity.py
```
On Windows choose writable output directories instead of /tmp; Windows build execution was not performed here. In this cloud workspace, activate /workspace/.cloud-setup/env.sh first. Build logs used external temporary obj/bin paths and maxcpucount=2 to keep the repository clean. The product targets net48/C#7.3; net8 is used ONLY by the portable extracted test harnesses/offline metadata tool. Source-only packaging does not require installing a runtime update into GTA.

## Manual qualification gates — all NOT RUN

Finish the owner-controlled no-Glass-Heroes session before a new installation experiment. Record its result without assuming survival or blame. The latest reported crash was October 22 at 4:21:49 PM local, uptime 29m20s, 0xC00000FD at clr.dll+0x688D5; responsible script remains unknown. A v2 deprecation warning is not attribution evidence. Compare one variable at a time and keep private crash dumps/logs out of public GitHub.

After independent review and explicit owner approval, qualify cold starts under the existing Enhanced/SHVDN/RAGE/full-mod stack; separately record warm reloads and startup hold/delay. Verify exactly one Business Manager script is loaded. If a no-RAGE/LSPDFR control is useful, obtain owner agreement and change that variable separately. Run repeated 60–120 minute closed-shop idle/travel/full-feature sessions, comparing no-GH and accepted v2 controls. Healthy runs are evidence, never proof of root-cause correction.

Exercise F2, keyboard/controller-only and mixed input, one action per press and Back; all menu layouts/resolutions; manager remote work, mechanic routes and customer vehicles/blips; pause/long pause/close/restart with zero offline progress; warehouse >80m streaming, missing model and five-attempt exhaustion recovery; first and second wholesale ETA→ready→pickup→registered-vehicle load→drive→unload→physical pallet stock; full pallet, cancel/recover, prepaid old pending order, reload during each phase, stock/invoice identity exactly once. Verify real INI values, money, wages, invoices, jobs, inventory, saved placements and legitimate package visuals before/after reload. Confirm owned cleanup on abort/reload without touching other mods' entities or markers. Every new engine, persistence and stability result remains NOT RUN until owner evidence is supplied.

## Review package and safe rollback

The ZIP contains one installation .3.cs, the project/solution linked to it, API map, build/test/dependency metadata, full source diffs, source/hash manifest, validation JSON, MD/DOCX/PDF report, tests and noninstallable historical reference fixtures. It contains no runtime/product DLL, active v2 script, live Save.ini/Settings.ini, private crash data or nested ZIP. The external ZIP SHA256 manifest is separate to avoid circular hashes. Inspect READ_ME_FIRST.txt and validation.json before considering installation.

No deployment is authorized by the existence of this artifact. For a later approved source test, stop GTA, back up the complete business folder privately outside scripts without editing contents, and move the active v2 business source/DLL outside ALL script loader search paths. Keep Save.ini and Settings.ini in place. Copy only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs into scripts. Do not add a business DLL alongside it and do not copy reference fixtures/development tools into scripts. Do not replace SHVDN root runtime files. Check the actual loader list for renamed/stale business copies, which the canonical preflight cannot discover.

Rollback with GTA stopped: move the v3 business source/DLL outside all script loader paths and restore the exact accepted R2 .2.cs or previously accepted single business DLL. Keep the current save/settings; this port adds no saved keys and requires no downgrade. Preserve the private pre-test backup separately rather than overwriting a newer business save. Only restore data from backup through a deliberate owner decision if a real failure requires it. No installation, merge, rollback or GTA testing has been executed by Codex. PR #21 remains DRAFT for coordinator review.

## Recorded automated results

Real Release builds: v2 PASS, v3 PASS; both 0 errors and two inherited warnings (CS0169, CS0649). SDK 8.0.426; product net48 / C#7.3.

Baseline v2: 54 corrective + 9 logger = 63/63 behavior PASS; 21/21 static PASS. Migrated v3: same 63/63 inherited behavior PASS plus 22/22 migration cases = 85/85 behavior PASS; 21/21 static PASS. The nine logger cases are counted once per generation.

Whole-source R2 parity after the reviewed API reversal: PASS. Financial/clock/save bodies and key order unchanged. Original v2 sources/project/solution unchanged. Project compiles the single installation file directly. Additive source.diff applies and reproduces source/project/solution exactly. Offline metadata: 363 existing Control names/values match, all 43 distinct used native hashes match, font index matches; compiled product has no SHVDN2 reference.

Clean ZIP extraction: Release rebuild PASS (0 errors / 2 existing warnings), 21/21 static PASS, source parity PASS; all archive file hashes verified. Source, project and solution match this candidate.

Installation/source SHA256: cda7721e4bfaefa3c3a8da84afde70cd3d8c909f79bbfb19a0ac947549955445.

GTA gameplay testing: NOT RUN. Windows execution: NOT RUN. Actual Enhanced 3.9.0.6 runtime binding/engine/save behavior: NOT VERIFIED. Crash cause: UNATTRIBUTED / UNPROVEN. These automated results authorize no merge, installation or stability claim.
