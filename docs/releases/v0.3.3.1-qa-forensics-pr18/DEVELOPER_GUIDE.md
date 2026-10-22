# v0.3.3.1 PR18 forensic candidate — EXPERIMENTAL

Review candidate for draft PR #18, investigation/qa-r2-stack-overflow-forensics. Target is protected QA R2 7693ed2167405f475dd4d36392c5df1f257fe178. No P1 blip code is included. Review P0 before P1. No branch merge, deployment, business-save editing or live configuration changes were performed. GTA testing of this candidate: NOT RUN. Stack-overflow cause: UNPROVEN.

## Evidence and attribution

The owner reports October 22 at 4:21:49 PM local, game uptime 29 minutes 20 seconds, 0xC00000FD at clr.dll+0x688D5. No corresponding private dump or symbolized managed stack was available for this investigation. This is user-confirmed incident metadata, not a locally reproduced failure. The responsible script remains unknown.

The tracked handoff also records cold-load 15:00:18 (61 seconds, clr.dll+0x5F8D10; no observed gameplay-gate passage) and gameplay 15:47:16 (34m21s, clr.dll+0x19C1D; gate passed, shop closed). Their different offsets do not establish a common cause. R2's transition-only logger has no steady-idle heartbeat, so its five-minute log gap does not prove a stalled tick. Warm reload is distinct from cold loading. RPH hooking, last-read DLC/PSRP files, CLR frames and proximity in time are not attribution evidence.

The owner confirmed the first wholesale pickup/load/unload/deposit and stock/invoice update, and the second delivery becoming ready and spawning after ETA. This candidate deliberately preserves that logic. Those selected R2 observations do not qualify this new build or prove overall stability.

## Independent R2 source audit and ranked investigation hypotheses

The Roslyn tool under verification/forensic-audit parses the actual R2 source, not a claimed upstream version. Its transcript is r2-source-audit.txt. It examines unqualified, this-qualified and class-qualified local calls across 288 methods and found zero cycles. A separate conservative name-only inspection produced two apparent self cycles, manually resolved: Notify calls GTA.UI.Notify; Save calls GTA.ScriptSettings.Save on the local data object. Neither expression calls this script's same-named method. This is syntax analysis, not a semantic proof or a scan of other installed mods.

Investigation order, not probability or a diagnosis:

1. Obtain a symbolized native/managed crash stack and controlled isolation. A CLR stack-overflow exception is confirmed, but no repeated managed frame sequence identifies a script. The pre-gate crash makes active Glass Heroes worker/warehouse execution an unsupported explanation for that incident. Other SHVDN2/3 scripts, ScriptPluginControl, the separate V3 probe, RPH and LS RED/PSRP are isolation hypotheses; none is convicted or cleared.
2. Check runtime callback/reentry and hot-reload lifetime. R2 subscribes Tick, KeyDown, KeyUp and Aborted once per instance; it has no explicit calls to its own OnTick/OnAborted, script-created timer, thread, or task. Runtime/native/event reentry and overlapping instances are unknown without runtime evidence. New per-instance forensic IDs help identify overlap but a startup line alone is not proof of duplicate activity. PR9's existing writer rejects synchronous writer reentry and backs off failures. No speculative gameplay reentry guard was added.
3. Review post-gate native/model operations if stacks implicate them. Staff, customer and supplier creates occur after the startup gate through the once-per-second visual path. Most model requests are nonblocking. Existing office/equipment helpers use Request(250), potentially yielding/waiting; no recursive call chain was found. This can affect latency and reload interactions but is not demonstrated to cause overflow. Their behavior was preserved, including the tested warehouse batch retry/cooldown.
4. Review startup and abort if evidence points there. R2 constructor loads managed settings/save data, normalizes records and subscribes events. Audit found no explicit world/native creation in that constructor. After its UTC delay it validates the player in OnTick. Abort before activation returns without Save; after activation it cleans owned entities/models and saves, using existing native operations. A fatal overflow may bypass Aborted entirely. No cleanup/accounting change was justified by available evidence.

SHVDN2 dependency remains 2.11.6, net48, C# 7.3, .2.cs. No V3 API, assembly binding, clr.dll, plugin or game-install edits. External modules were not supplied for source audit. Native implementation, reflection, indirect delegates and cross-script callback graphs remain outside the tool's coverage.

## Before and after: exact instrumentation scope

Before: normal Errors.log includes constructor, activation, abort, managed exception phase and throttled changed-state checkpoints; unchanged idle emits nothing. After: all of that remains. Optional ForensicProbe adds a separate Forensics.log stream with a random per-instance identifier, lifecycle snapshots, at most one changed-state observation per ten active seconds and a heartbeat every 120 active, unpaused seconds. Heartbeat records completed ticks, completed phase and cached managed shift/menu/supplier/customer/job state. The first state snapshot can appear immediately; subsequent steady heartbeats require active time. This is sampled evidence, not a complete per-stage or native stack trace. The last completed phase cannot locate an eventual fatal instruction.

Exact changed existing methods: constructor (optional probe setup), OnTick (activation breadcrumb and completed-tick observation using existing activeGameSeconds), OnAborted (best-effort entry/completion breadcrumbs). New nested ForensicProbe handles bounded scheduling only. PR9 DiagnosticLog, AppendDiagnosticLine and Log are byte-identical to their vetted versions. All 282 private R2 methods, including wholesale, finances, Save and recovery, are hash-checked after removing only the explicit new Tick/Abort hooks. The constructor's optional setting is checked separately by additive-key and original native-constructor checks. Full source.diff contains all changes to both active sources and applies exactly to R2.

No additional GTA native, entity scan, model request, financial clock, saved key or Save.ini schema change. The existing startup defaults remain HoldGameplayTick=false and StartupDelaySeconds=30. No wall-clock catch-up is added. ForensicsEnabled is an optional Settings.ini read only; this build never writes it.

## Explicit QA enablement and delivery guarantees

Default: disabled. In the owner's existing [Diagnostics] section, manually add ForensicsEnabled = true only when deliberately choosing this forensic candidate; reload scripts to apply. Keep existing HoldGameplayTick and StartupDelaySeconds values. Do not replace Settings.ini with the reference example. Missing or false disables the new stream, including background scheduling and file creation. Changing the setting requires reload. Normal Errors.log behavior remains as before.

New I/O runs in a ThreadPool callback, never synchronously in the new gameplay instrumentation. Each instance admits at most ONE outstanding immutable snapshot; busy writers drop subsequent events and report the drop count on the next admitted snapshot. The worker touches strings, UTC and filesystem only, never GTA entities, script state, settings or saves. It owns its own unchanged DiagnosticLog with the existing 30-second write-failure backoff. Errors are swallowed without recursive logging; exception throttling in normal Errors.log remains unchanged. No dedicated poller, wait, lock or flush is added to OnTick/OnAborted. Existing normal logging remains synchronous as inherited; it was not redesigned here.

Snapshots can be lost on fatal overflow, domain unload, shutdown, queue refusal, full disk or process termination. Abort completion can be dropped if abort entry is still queued. There is no guaranteed flush or catchable StackOverflowException. Instance IDs, completion counters and UTC file timestamps are correlation aids, not proof of the culprit. Snapshot content is captured on the script thread; the timestamp is write time and can lag under load. Forensics.log appends across reloads; rotate it privately while GTA is stopped if needed, preserving evidence first.

## Owner-controlled isolation protocol — NOT RUN

Back up the complete business folder outside the live scripts tree before testing, without editing any contents. Keep copies of saves/settings and record their hashes privately. Preserve all other mods, versions, diagnostics settings, SHVDN generation, plugins, test route, shop state and workload between comparisons. Record local timezone, UTC correlation, cold/warm start, game gate, exact source hash and mod inventory privately. Do not remove the V3 probe or another mod in the same comparison.

A: run the accepted unchanged QA R2 on the same stack for 35–40 minutes from a true cold start. B: stop GTA and move ONLY GlassHeroesBusiness.2.cs outside every scripts/search directory, leave saves/settings and other mods alone, then repeat a cold start and the same 35–40 minute workload. Restore the exact source afterward while GTA is stopped. Verify SHVDN's script list confirms which script loaded. Do not leave a renamed .cs or compiled copy in scripts. Alternate/repeat comparable runs; one survival interval does not establish absence of the bug. A B-side crash weakens an exclusive attribution to this script; survival alone does not prove it is responsible.

A separate comparison may replace only the Glass Heroes source with this PR18 candidate and explicitly enable its one optional forensic setting. Record this deliberate instrumentation change and start a fresh session; do not silently label it R2. If approved and useful, test the known v0.3.2.9a source in another isolated comparison, retaining a separate backup because old/new schema compatibility is not a reason to overwrite current saves. Warm script reloads are an additional scenario, never cold-start qualification. No rollback or restore has been executed by Codex.

Also schedule normal cold boot; delayed gate; HoldGameplayTick=true hold/reload; pause/resume; closed idle; open shop/remote jobs; menu keyboard/controller; 80m warehouse stream boundary; bounded missing-model recovery; two sequential wholesale deliveries; reload with pending/loaded cargo; and owned-entity abort cleanup. Verify money, wages, jobs, ETA, invoices, stock and saved routes against the backed-up scenario. Every new in-game item remains NOT RUN.

## Private crash capture — owner opt-in required

Keep logs and dumps private; dumps can contain account/session data, paths and unrelated process memory. Do not upload them to public GitHub or commit raw logs, dumps, mod lists or user-specific paths. This repository contains synthetic tests and source audit output only.

Start with Event Viewer Application Error and Windows Error Reporting entries plus SHVDN/RPH logs and both Glass Heroes logs, captured from the same session. Preserve timestamps, exception code, module version/offset and uptime; redact copies before sharing. Absence of a managed exception is expected for some fatal stack overflows.

If the owner explicitly opts in, Windows WER LocalDumps supports a per-executable GTA5.exe configuration under HKLM Software Microsoft Windows Windows Error Reporting LocalDumps, with a private user-chosen DumpFolder, bounded DumpCount and DumpType=1 for minidump. Registry changes require administrator authority and explicit owner approval; none were performed or scripted here. Export any existing per-app values before changing them and restore those exact values afterward rather than deleting shared WER configuration. Minidumps may omit managed stack data; a separately approved full dump may be required. Microsoft documentation: https://learn.microsoft.com/windows/win32/wer/collecting-user-mode-dumps .

Alternative: Microsoft ProcDump can wait for GTA5.exe and capture an unhandled exception dump, with explicit filtering for C00000FD where the installed version supports it; consult procDump -? for that version before choosing arguments. Attaching a debugger can affect timing/RPH and should be a separate documented capture run, never a claimed equivalent A/B run. Do not broadly capture first-chance exceptions or change multiple diagnostic tools at once. https://learn.microsoft.com/sysinternals/downloads/procdump .

A reviewer can open a private dump in matching-bitness WinDbg, load Microsoft symbols, run !analyze -v and inspect native stacks. For .NET Framework load matching CLR SOS with .loadby sos clr, inspect !threads, !clrstack -a and !dumpstack on the faulting thread. Preserve the repeated-frame sequence, loaded-module versions and script/assembly identity. Exact commands depend on dump completeness and CLR version; no usable symbolized stack was obtained here. If SOS cannot load or required memory is missing, capture the diagnostic error and request a better dump privately. Do not edit Windows clr.dll or infer ownership from its offset alone.

## Installation package, rollback and review

Source-only ZIP contains exactly the matching Install/scripts and VisualStudio/scripts .2.cs copies, VS project, full source diff, tests, MD/DOCX/PDF guide, audit and actual build transcripts. No DLL, live Save.ini, live Settings.ini, raw crash evidence or older active business script is included. Inspect the external manifest SHA256 before use. This is for ChatGPT code review first; PR #18 remains DRAFT, no user deployment performed.

After approval, stop GTA, retain an external backup, and replace only the selected business .2.cs with Install/scripts/GlassHeroesBusiness.2.cs. Do not install the VS copy alongside it. Keep live Save.ini/Settings.ini. To roll back while GTA is stopped, restore the exact accepted QA R2 source; remove only the manually added optional ForensicsEnabled entry or set it false. Preserve forensic evidence privately. No save downgrade is required by this change. Keep PR18 and PR19 separate; do not merge both or deploy a combined build without review and renewed validation.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Suite #2: 10/10 behavioral tests PASS; Suite #3: 14/14 behavioral tests PASS; Suite #5: 8/8 behavioral tests PASS; Suite #6: 8/8 behavioral tests PASS; Suite #4: 7/7 behavioral tests PASS; Suite #7: 7/7 behavioral tests PASS; Suite #18: 11/11 behavioral tests PASS.

PR #9 diagnostic logger tests: 9/9 PASS; total behavioral tests: 74/74 PASS.

Install and Visual Studio sources have equal SHA256: 9c7d37c0263ea3b13753e635455f1d6af9d74b14c65677ef2bdada242bf3a298.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
