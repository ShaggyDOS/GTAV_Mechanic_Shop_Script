# Glass Heroes Business Manager v0.3.4 RC2 Diagnostic Experimental

October 24, 2026. DIAGNOSTIC-ONLY — OWNER REVIEW PENDING. GTA testing: NOT RUN. Windows execution: NOT RUN. Crash cause and responsible script: UNKNOWN / UNATTRIBUTED. No crash correction is implemented or claimed. This is the owner-authorized RC2 development recovery, in the historical Issue #1 crash-investigation context; it does not begin v0.3.5.

## Recovered checkpoint and branch

- Dedicated branch: diagnostic/v0.3.4-rc2-mechanic-transitions
- Owner-verified RC1 source commit: e312d8c9500f4a990eb993b82e8e76b55711d3de
- Verified RC1 source SHA256: 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120
- Branch parent: 75e3623f608a05d1a2b59d12c243dbcdfc061358, the RC1 approval/package documentation commit. Its source checksum is identical to the specified RC1 checkpoint.
- RC2 instrumentation commit: 3b2ef09a96dda9f6d0325f08d29bc986167abe68
- RC2 source SHA256: 3475f93b67d5feb8d90ef30550b8a474642aeb95c77e76e8f0438ac21f30cdc9

The package's FINAL_HANDOFF.md/JSON records the exact final branch tip including its subsequent documentation/package-tool commit. Original RC1, feature/shvdn3-340-inventory-procurement and main are not modified. The original workspace checkouts are preserved. No merge or deployment is performed.

## Investigation and limits

The owner reports an October 24, 2026 0xC00000FD stack overflow while Mechanic 1 transported a customer car into a service bay. This temporal context motivates instrumentation; it does not identify the responsible script, native, CLR method or mod. No October 24 crash dump, complete managed/native crash stack or owner runtime log was supplied or examined in this recovery task. Earlier October 22 observations must not be treated as the same crash.

The reviewed RC1 path goes from assignment and walking to vehicle entry, optional reverse-out, BeginVehicleDrive, approach/parking, exit and repair. Return and paid-recovery paths are included because they share transfer helpers. A conservative lexical audit parsed 419 script methods and found 86 reachable from the transfer roots, with no direct cycle among those unqualified script calls. Qualified calls such as data.Save(), SDK/native implementations, CLR internals, delegates, runtime reentry and other scripts are outside that audit. It is not a semantic C# analyzer or evidence that this script is crash-free. See INVESTIGATION.json and verification/diagnostic340/investigate.py.

No task retry, recursion guard that skips gameplay, native replacement, vehicle movement correction or AI rewrite is introduced. The trace is designed to provide context for a future real dump and owner reproduction. A fatal stack overflow may terminate the process before finally blocks or any logger can run; instrumentation cannot catch or repair it.

## Exact source-level scope

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs changes in the gameplay source. The original v2 sources and both generations' project/solution files are unchanged. The v3 project still directly links the matching installation source; target net48, language C# 7.3, official ScriptHookVDotNet3 NuGet version 3.6.0.

Ten existing methods receive one diagnostic Begin call and a try/finally End scope:

1. UpdateOneCustomerOrder — per-order state before existing validation and state-machine work.
2. AssignCustomerMechanic — assignment, quote reservation/save and handoff selection context.
3. BeginVehicleWalk — original walk-route setup/task dispatch.
4. BeginEnterVehicle — original clear/enter-vehicle dispatch.
5. StartValetDeparture — original optional reverse-out or drive dispatch.
6. BeginVehicleDrive — original ability/aggressiveness/drive task dispatch.
7. UpdateVehicleApproach — original approach, park dispatch and bounded drive retry.
8. UpdateVehicleParking — original park/exit and bounded parking retry.
9. FailVehicleTransfer — existing failure/cancellation or paid-return recovery decision.
10. ProcessPaidReturnRecovery — existing safe empty-car recovery.

Their original statements, branches, return behavior, exception propagation and native ordering are byte-identical after undoing the mechanical indentation introduced by the scopes. SOURCE_EXCERPTS.md provides before/after examples, and source.diff is the complete unified source diff. Applying that diff to the pinned RC1 reproduces the RC2 source byte-for-byte. Body reindentation accounts for much of the displayed diff; it is not a gameplay rewrite.

New diagnostic-only members are CustomerVehicleTraceState, CustomerVehicleTrace, CaptureCustomerVehicleTrace, BeginCustomerVehicleTrace, EndCustomerVehicleTrace and AppendCustomerVehicleDiagnosticLine, plus one instance trace field and one static safe logger field. They capture managed order/stage/slot/handle/mode/route-index/retry information without querying entity existence, position, speed, models, tasks or the world. Entity Handle properties identify existing wrappers; diagnostic code never validates or manipulates those entities.

## Bounded logging and performance

Diagnostics begin only after the existing boot-activation gate. No constructor native calls, new Tick callback, per-frame poll, delay/yield, background thread or stack-trace capture is added. Calls run where the existing state machine already operates, normally at the existing one-second customer refresh. Existing explicit/manual recovery calls keep their prior scheduling.

There are 64 fixed snapshot slots. Stable operation/order/state at the same or lower nesting depth produces no extra log formatting, logging-clock read or I/O. A greater nesting depth can produce a bounded breadcrumb, which may help identify runtime reentry. Nesting can also be normal: assignment → walking, reverse fallback → drive, and failure → recovery are examples. An observed depth alone does not prove recursion or a fault.

At most 12 admitted ENTER records per ten-second logging window, each with a corresponding attempted EXIT. Failed ENTER writes also spend the budget. EXIT attempts for already admitted scopes can complete after the originating window. Rapid state changes are coalesced/suppressed; this is not a complete flight recorder. Successful later ENTER reports the count of budget-suppressed candidates. Diagnostic time is UTC wall-clock only for logging; no business clock uses it.

The existing unchanged DiagnosticLog supplies reentry protection, contained writer failure and a 30-second write-failure backoff. The trace adds its own writer reentry guard, contains its own errors and always balances admitted/suppressed scopes through finally on ordinary returns/exceptions. A diagnostic failure must not replace an existing gameplay exception or stop native task dispatch.

Only scripts/GlassHeroesBusiness/CustomerVehicle.RC2.log and CustomerVehicle.RC2.log.previous rotate. Each file is bounded to 128 KiB by UTF-8 size; records are capped at 2048 characters. One previous file is retained. Thus diagnostic footprint is approximately 256 KiB across two files. Existing Errors.log and source-identity logging remain unchanged. See DIAGNOSTIC_INSTRUCTIONS.md for pairing and limitations.

## Save, financial and gameplay compatibility

No Save.ini or Settings.ini key, default, schema, migration, serialized field, stored coordinate or waypoint mask changes. No existing owner data/settings were opened or modified. The file-rotation regression uses synthetic temporary sentinel files only. RC2 adds no user setting to enable/disable its always-on bounded diagnostic scopes; rollback to the exact RC1 source removes them.

The exact-restoration guard proves that removing only the new diagnostic block and ten scopes restores the entire verified RC1 source. It also checks 426 existing members remain untouched, project/v2 sources remain byte-identical, and all nine inherited fixture edits are only no-op diagnostic sink adapters with every original assertion preserved. This protects vehicle/AI behavior, native task ordering, saved route geometry, invoices/quotes, stock, manager procurement, receipts, business saves and active-unpaused game-time progression. The original Tick, startup defaults, controller arbitration and all financial/stock/save methods outside the scopes remain untouched.

Unchanged source logic is not a substitute for native/runtime QA. Synchronous bounded trace writes add some overhead and can affect timing; no engine performance measurement has been run. The owner should compare behavior/performance with the exact RC1 checkpoint using copied data.

## Actual compilation and regression results

Independent RC1 recovery run: 954/954 inherited behavior/navigation checks PASS; actual Release build PASS, 0 errors / two existing warnings. RC1 source hash matched the owner value before any instrumentation.

Final RC2: 985/985 behavior/navigation PASS, including all inherited 954 and 31 new trace cases. Navigation remains 212/212. Static checks: 21/21 PASS. Inventory structure: 13/13 PASS. Diagnostic-only exact-restoration/fixture preservation: PASS. Real clean Release compilation: 0 errors, inherited warnings CS0169 and CS0649. Build tool SDK 8.0.426; product net48/C# 7.3; SHVDN3 package 3.6.0.

The new cases extract real production trace/state/safe-writer members and five instrumented transfer methods. They exercise stable outer/nested updates, three orders, stage/retry changes, flood bounds/recovery, clock/write failures, writer reentry, nested depth, boot/null references, native ordering, early return, cancellation, original exception propagation, reverse/park/exit/retry and real temporary file rotation. Inherited accounting fixtures use no-op trace adapters; new trace tests exercise the real diagnostics separately.

Tests run on deterministic .NET 8 adapters, not GTA natives. They do not prove compatibility with GTA, SHVDN Enhanced, controller hardware, the owner's MLO/heavy mod stack or CLR crash behavior. No crash reproduction, dump capture or successful gameplay session was performed by Codex. GTA testing NOT RUN. Windows execution NOT RUN. Enhanced runtime compatibility NOT VERIFIED.

Evidence is in baseline/ and final-validation/, including commands.json, actual compiler/test output, checksums and preservation reports. The final suite ran before committing the instrumented worktree: its source_revision records the then-current RC1 documentation HEAD, while source_sha256 records the actual RC2 bytes compiled/tested. Those bytes were subsequently committed as 3b2ef09a96dda9f6d0325f08d29bc986167abe68; report/package generation checks exact equality to that commit. Reproduce in the full Git checkout with Python 3/.NET 8 tooling (cloud shell: source /workspace/.cloud-setup/env.sh):

```text
python verification/inventory340/validate.py --output /tmp/gh-rc2-review --issue36-integration --integration-approved --diagnostic-rc2
python verification/diagnostic340/investigate.py
```

The approval flag records the already accepted Issue #36 integration in RC1; it does not accept RC2 gameplay. The full preservation audit requires the Git history/protected baseline files. Product rebuild from the package requires only its matching source/project and NuGet references:

```text
dotnet build VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release
```

## Delivery and owner review

The review ZIP contains one installation script, matching Visual Studio project/solution, manifest/checksums, all new documentation and Word/PDF guide, full source diff, test sources and raw evidence. It contains no business Save.ini/Settings.ini or runtime DLL. Source retains RC1's v0.3.4 DEV header/startup wording so the original source outside diagnostics is unchanged; RC2 identity comes from this diagnostic-only package, source SHA and RC2 VEHICLE log prefix.

The separate archive receipt records independent ZIP extraction, every manifest hash and another clean product compilation. Code/regression results apply to its byte-identical source; documentation-only commits do not change gameplay qualification. Finished status is ready for owner review, not an invented stability certification or crash fix.

Follow INSTALL.md, GTA_TEST_CHECKLIST.md and ROLLBACK.md only during a later owner-run test. No live installation, save rewrite, protected-branch merge, GitHub public release or v0.3.5 work is performed by this task.
