# Glass Heroes v0.3.3.1 — RC1 Stack-Overflow Static Audit (Phase 1)

**Date:** 2026-10-22
**Tracking issue:** [#1 — 0xC00000FD idle crash](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/1)
**Source inspected:** `GlassHeroesBusiness_v0.3.3.0_RC1.2.cs` at Git blob `73f804351875fbfd100146b98c1054a6b14acf17`
**Status:** STATIC REVIEW ONLY; NO CHANGE TO GAMEPLAY SOURCE. Root cause **not** identified.

## Ground truth: crash report and user tests
- 2026-10-22 00:34:33 local: GTA V Legacy 1.0.3889.0 experienced `0xC00000FD` STACK_OVERFLOW at `clr.dll+0x19E9DD` after 18 min 6 s, while idle near Glass Heroes.
- ScriptHookVDotNet log: `GlassHeroesBusiness.2.cs` compiled and initialized; no managed exception is logged for the crash.
- Glass Heroes own log: successful manager and delivery events up to ~71 seconds before crash; last event is **not** proof of cause.
- Los Santos RED and RAGE Plugin Hook were active in this session, along with numerous other plugins.
- Crash Analyzer did **not** save a minidump. Do not invent the stack trace or assign the fault to last loaded archive/PSRP.
- Earlier v0.3.2.9a diagnostic build had a reported successful session alongside ~73 scripts/plugins. It is a rollback **candidate**, not a statistically verified crash-free control.

## RC1 code observations and source locations
- `OnTick` begins around L829. Its `Interval = 0` registration is around L813: tick handler is eligible to run once per frame.
- Boot guard around L835–L846 delays game natives until a real-world buffer and player-valid/unpaused check have passed.
- L862–L880: remote operations, economy and supplier time logic; **no per-session catch-up** for supplier delivery countdown. Economy uses clamped `DateTime.UtcNow` seconds while playing, with `Game.IsPaused` check. Review for pause/loading edge cases separately; not evidence of the stack overflow.
- L887–L900: physical employee/customer/supplier visual lifecycle refresh is throttled to approximately once per second.
- L913–L916: outer tick exception handler writes a log entry and disables the shop shift. If a persistent exception occurs every subsequent frame, disk logging could become excessive; design a **rate-limited** exception logger. No evidence yet that this happened in the reported session.
- L3476–L3493: mechanic stream-in/out and staffing, including persistence of captured employee posts. Inspect for large-scale save writes during repeated boundary crossings.
- L3716–L3779: route recovery has explicit retry/teleport budgets. Repeated path retries/cleanup still merit integration tests.
- L4194–L4259: manager AI has a bounded retry condition and a guarded exception path; no direct manager self-call was visible.
- L6482–L6520: controller polling interacts with menu code; separate [issue #2](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/2). Do not touch while Codex is working on it.

## Mechanical scan limitations
A superficial lexical scan of the ~7,730-line C# file found approximately 271 private/public method declarations, no clear direct self-call in those parsed methods and no `while(...)` loop. An initial method-call graph appeared to have a cycle connecting menu input and `OnTick`, but it was a **false positive**: the `OnTick()` string was in a comment attached to `DisablePhoneWhileMenuOpen`. This does **not** prove absence of indirect recursion, event-handler reentrancy, native-hook interaction, stack growth, reflection, compiler-generated calls, or bugs in other mods. Do not use the scan as a correctness certificate.

## Proposed focused crash diagnostics, separate from Codex's menu work
1. Add a low-overhead **in-memory** subsystem state snapshot tracking stage, order/customer counts, worker counts, manager state, and last major transition (no extra per-frame disk writes).
2. Emit diagnostic checkpoints **only** when subsystem state changes, on a slow heartbeat while clocked in (e.g. 60 s or more), and on genuine caught errors with rate limiting; avoid a log event inside every worker/tick iteration.
3. Check that teardown cannot recursively trigger itself through callback/event paths and cleanup is idempotent on reload.
4. In regression tests, compare RC1, v0.3.2.9a and the candidate under matched mod loads and actions; separate baseline A/B from code changes.
5. Do not modify vendor, gameconfig, RAGE, or other mods without evidence.
6. Request optional Windows crash dump capture only if problem reproduces; current reported session has no dump.

## User-in-game acceptance tests (not yet run)
- Three full GTA cold starts (not script-controller reloads), first no RAGE and then with LS RED and the usual 70+ scripts.
- Idle at shop for 30+ minutes while employees, manager and customer flow are active; compare with RC1 and known-working rollback.
- A separate 30+ minute active session using supplier deliveries, customer interactions, streaming out and back, restart with pending order.
- Review both `ScriptHookVDotNet.log` and `scripts/GlassHeroesBusiness/Errors.log` from the **same** session; record differences even for successful runs.

## Version/documentation policy
No patch is attached to this Phase 1 audit. Once code is altered, supply complete source and Visual Studio project, unified diff, before/after method excerpts, actual compile/static-test evidence, save compatibility/rollback notes, and versioned **DOCX + PDF** developer guide before labeling any build test-ready. Preserve the successful v0.3.2.9a backup and keep `main` unchanged.
