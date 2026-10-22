# Glass Heroes v0.3.3.1 issue #1 DIAGNOSTIC

EXPERIMENTAL. Issue #1 remains OPEN. Crash cause and crash resolution are NOT verified.
Target: GTA V Legacy Story Mode 1.0.3889.0; ScriptHookVDotNet2 2.11.6; C# 7.3; .NET Framework 4.8.
Integration target: development/v0.3.3.1. This change is isolated on fix/1-idle-crash-diagnostics.
Issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/1

## Why this change exists

The reported idle crash occurred October 22, 2026 at 00:34:33 local time, 18m06s into a session, with 0xC00000FD at clr.dll+0x19E9DD. No minidump was saved. SHVDN compilation/start success and earlier wholesale log entries do not identify a cause. Do not attribute this to PSRP, the most recent archive, or any specific method without evidence.

A Roslyn syntax review of all 272 RC1 methods found zero direct unqualified local-call cycles. This does not analyze native code, virtual dispatch, event callbacks or other scripts; it cannot rule out a runtime stack overflow. The code therefore adds diagnostics rather than claiming a fix. The GitHub API was unavailable in this cloud instance; scope and incident facts come from the checked-in AGENTS.md and milestone test plan.

## Exact files and methods

Canonical development/compilation source: VisualStudio/scripts/GlassHeroesBusiness.2.cs. Identical deployment source: Install/scripts/GlassHeroesBusiness.2.cs. The original root RC1 source and prior RC1 archive remain unchanged for reference.

- GlassHeroesBusiness constructor: correct the boot version label to identify this experimental build.
- OnTick: retain all original gating, timing, input, economy, visual calls and call order. Assign constant strings to _diagnosticTickStage before each phase; include that phase in recoverable tick exception context. Sample managed state after the existing once-per-second visual refresh.
- OnAborted: log cleanup entry with the last tick phase and log completion after the existing Save call. Abort before activation still preserves the save without issuing world cleanup or saving partial defaults.
- New DiagnosticLog.RecordTransition: record shift, remote mode, staff streaming-range flag, menu visibility, supplier stage and customer/remote order counts only when the observed state changes. Stable idle produces one initial state line, then no checkpoint heartbeat. Rapid changes are coalesced to the latest observed state at most once per ten seconds.
- New DiagnosticLog.Write: admit at most one exception log per ten seconds across all contexts, summarize suppressed errors on the next admitted exception, prevent recursive writer entry, contain writer failures and back off storage retries for thirty seconds.
- Log and new AppendDiagnosticLine: route existing log calls through that bounded writer; retain Errors.log in scripts/GlassHeroesBusiness.

Tick phase assignments use constant strings; the stable state checkpoint path performs managed comparisons only. It does not load models, scan entities, spawn props, call GTA natives or write per-frame diagnostics. New state checkpoints occur only during the existing unpaused visual refresh; they are observations, not an audit of every transition.

## Before and after

Before:
```csharp
catch (Exception ex)
{
    Log("Tick", ex);
    _clockedIn = false;
}
```

After:
```csharp
catch (Exception ex)
{
    Log("Tick / " + _diagnosticTickStage, ex);
    _clockedIn = false;
}
```

Before: Log appended every exception immediately, using local timestamps.
After: DiagnosticLog.Write rate-limits exception floods and storage failures. New timestamps are UTC and end in Z; older entries without Z retain their historical local-time meaning.

Example new checkpoint:
```text
2026-10-22T06:00:00Z | STATE shift=True remote=False staffInRange=True menu=False supplierStage=0 customers=0 remoteJobs=0 | no exception
```

The complete unified production-source delta against RC1 is supplied as source.diff, including both existing install and Visual Studio source copies. Both complete source copies are included in the package and must remain byte-identical. No gameplay fixes from issues #2-#7 are bundled here.

## Save, configuration and rollback

No Save.ini or Settings.ini key is added, removed, renamed, defaulted differently, or migrated. HoldGameplayTick=false and StartupDelaySeconds=30 remain the existing defaults, and existing user overrides remain in force. Diagnostics are enabled without new configuration. Only the log uses new wall-clock backoff; gameplay, wages, remote work, delivery timer calculations, persistence cadence and money/inventory logic are unchanged.

Installation: back up your existing script and user-owned save/settings outside the game scripts folder. Install only Install/scripts/GlassHeroesBusiness.2.cs from this package to the game's scripts folder. Preserve the existing scripts/GlassHeroesBusiness/Save.ini and Settings.ini. Example settings in the package are reference only; do not overwrite an existing file. Do not install the root historical RC1 source, the Visual Studio copy, or a compiled GlassHeroesBusiness.dll alongside the install source. There must be one active Glass Heroes script.

Rollback: exit GTA, replace the experimental .2.cs with your preserved v0.3.2.9a diagnostic script (last user-tested rollback) or prior RC1 if comparing against RC1. This change itself creates no save schema dependency. Preserve the user's saves and settings; do not reset them. RC1 was not certified stable.

## Actual automated validation

See validation.json, build.txt, diagnostics-tests.txt and StaticVerification.json beside this guide for this build's exact results, tool versions and source SHA256.

- Real .NET Framework 4.8 Release compilation with SDK 8.0.426 / MSBuild 17.11.69 and ScriptHookVDotNet2 2.11.6: PASS, zero errors. Existing CS0649 (CustomerOrder.VehicleTaskIssued) and CS0169 (_selectedInvoiceMechanicSlot) warnings remain.
- Existing static checks on the development source: 21/21 PASS. The wrapper redirects original hardcoded paths in memory and checks the reconstructed v0.3.2.9a baseline SHA256 before preserving every original assertion. StaticVerification.json's compiled=false describes that static runner alone; actual compilation is recorded separately.
- Production logger behavior under .NET 8: 9/9 PASS, including 30 simulated minutes of stable idle with one checkpoint; independent state changes; rapid-change coalescing; 10,000 alternating errors; startup/cleanup checkpoints during error throttle; storage failure/retry; reentrant writes; clock failure; and UTC/phase formatting.
- The harness extracts the production logger verbatim, changing only class accessibility for compilation. It does not load GTA or prove SHVDN callback behavior. This simulated idle test is NOT a 30-minute GTA session.
- Source-copy equality, original RC1 SHA256, package contents, and git whitespace checks: see recorded validation results.

Repeat locally after activating the cloud SDK:
```bash
source /workspace/.cloud-setup/env.sh
bash /workspace/.cloud-setup/build.sh
python3 verification/run_static_checks.py /tmp/glassheroes-static-results
python3 verification/run_diagnostics_tests.py
```

On Windows, open VisualStudio/GlassHeroesBusiness.sln with the .NET Framework 4.8 developer pack and restore its pinned NuGet dependencies before a Release build. This Windows build has NOT been run in cloud.

## GTA regression and crash forensics: ALL NOT RUN

Use docs/V0_3_3_1_TEST_PLAN.md and record PASS, FAIL or NOT RUN for each scenario. The release is not crash-qualified and is not a recommendation to replace the known working rollback.

1. Cold launch with only normal prerequisites and LS RED disabled, then repeat with LS RED and the usual heavy stack. Verify the thirty-second/player-validity gate; exercise HoldGameplayTick=true separately without overwriting other settings. Complete three cold starts, checking one script instance and boot version.
2. Idle at the shop for over thirty minutes with ordinary gameplay active. Save Errors.log and ScriptHookVDotNet.log and record mod versions, start/end times and state changes. Stable unchanged state should not produce repeated STATE lines; existing game events can still log. Repeat normal play for over thirty minutes with customer and supplier work. Automated simulated idle is not evidence for this step.
3. Start an open shift and an outstanding order, travel beyond staff/manager streaming distances and return. Check bounded state records for remote/staff transitions, no duplicate entities and unchanged inventory, balances and orders.
4. Save/reload with pending delivery, quit GTA and relaunch after a delay. Confirm stock, money and order identity persist and downtime adds no progress. Compare with RC1 behavior; no economy changes are intended here.
5. Reload/abort after activation and during the boot gate. Verify cleanup entry/completion as applicable, no duplicate entities, preservation of existing saves, and clean subsequent activation. A completion line means the abort handler reached its end; it does not certify that every internally caught cleanup operation succeeded.
6. Verify keyboard/controller parity and the remaining milestone scenarios without treating issues #2-#7 as fixed by this diagnostic build.

If another crash occurs, preserve available logs and Windows Event Viewer details and obtain a full crash dump using supported Windows crash-dump tools/settings in a separate user-approved diagnostic step. Do not alter the game install, registry, mod stack or gameconfig on the user's behalf here. Record the final observed STATE and any recoverable exception phase; these are context, not attribution.

Exception throttling applies across all error contexts, so distinct errors inside a ten-second window may be omitted; only the next admitted exception carries the suppressed count. Storage backoff also drops records while the writer is unavailable. Existing non-exception event logs retain their prior behavior.

A fatal CLR/native stack overflow may terminate before managed catch/abort handlers execute. This change neither catches that fatal condition nor writes a last-phase checkpoint every frame. Without a dump, the last STATE line cannot identify the exact failing method. Issue #1 stays open pending repeated GTA reproduction and crash forensics; do not merge to main or claim resolution from these automated results.
