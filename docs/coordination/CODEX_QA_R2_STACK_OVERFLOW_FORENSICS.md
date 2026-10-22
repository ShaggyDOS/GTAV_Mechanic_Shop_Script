# Codex handoff: QA R2 stack-overflow forensics — Issue #1 (P0)

WORKSTREAM: forensic diagnostics and isolation; NOT an identified root-cause fix. Do not merge, touch the user's live game/saves, or claim GTA stability.

## Protected base / branch
- QA R2 baseline commit: 7693ed2167405f475dd4d36392c5df1f257fe178, draft PR #17, from branch qa/v0.3.3.1-final-review-r2.
- Work ONLY in investigation/qa-r2-stack-overflow-forensics. Draft PR MUST target QA R2, not main or development. Keep unrelated blip feature separate.
- Codex previously got Forbidden reading review comments; use this checked-in handoff and the actual source, not GitHub PR comments alone.

## Actual GTA evidence and limitations
- First cold-load CTD October 22 15:00:18 local: 0xC00000FD, clr.dll+0x5F8D10, game uptime 61 seconds. QA R2 had compiled and constructor completed around 15:00:07, with HoldGameplayTick=false and StartupDelaySeconds=30; BOOT gameplay gate passed was NOT logged. Active gameplay tick thus had not been observed. This crash cannot be attributed to live warehouse or mechanic code on available evidence.
- Warm-reload session: game booted with HoldGameplayTick=true, then user changed false and reloaded SHVDN scripts after Story Mode loaded. QA R2 passed game gate, menu/keyboard and wholesale flow worked. An earlier crash happened while RAGE Plugin Hook was still hooking, not while the user typed an RPH command.
- Later gameplay CTD October 22 15:47:16: 0xC00000FD, clr.dll+0x19C1D, game uptime 34m21s. The R2 gameplay gate passed at 15:18:28; last managed STATE log around 15:41:56; no managed exception logged. Existing logger has NO steady-idle heartbeat, so a five-minute log gap is not proof of a stalled tick. Shop was closed in that session.
- Last loaded DLC, recently read PSRP files or native stack CLR frame do not identify a culprit. Other SHVDN2/3 mods, the separate GlassHeroesV3Probe.3.cs, ScriptPluginControl.dll, RPH, LS RED/PSRP integration are hypotheses, not proven sources. Stack-overflow events at different CLR offsets may be different bugs. Do not edit Windows clr.dll.
- User has subsequently confirmed first wholesale pickup-to-deposit success and SECOND delivery ready/spawn also working after ETA. Do not assert warehouse flow caused the crash.

## Investigation deliverables
1. Independently audit actual R2 C# for synchronous recursion, reentrant handlers, unsafe constructor or OnAborted callbacks, native/model creation, SHVDN v2/v3 interactions and hot-reload duplicate instances. Provide ranked hypotheses with evidence vs unknowns; do not patch blindly.
2. Create opt-in, low-overhead crash-forensics instrumentation in this separate experimental branch: coarse state transitions, lifecycle and tick stage, and an approximately 120-second managed-only heartbeat during active operation. Disabled by default in normal operation, clear explicit QA enablement. No new GTA natives, entity scans, hot-loop I/O, synchronous blocking, per-frame writes, or exception recursion. Preserve writer backoff/throttle. If useful, add sparse pre/post spawn breadcrumbs only on real state transitions. A fatal 0xC00000FD can bypass managed logging.
3. Design A/B protocol for the owner, not execute on their live install: same mod stack, 35–40 minutes with R2 vs same stack with ONLY GlassHeroesBusiness.2.cs removed outside scripts, and optional known older baseline v0.3.2.9a. Never change multiple variables together. Warm-reload stability is not cold-start qualification.
4. Document supported private minidump/WER/native/.NET symbolized stack capture options with opt-in before Windows registry changes. Never commit private user dump files, logs or user-specific system paths into public GitHub.
5. Preserve all active-unpaused-only money, wages, jobs, supplier ETA; no wall-clock offline catch-up. Preserve all existing Save.ini / Settings.ini and existing diagnostics defaults HoldGameplayTick=false, StartupDelaySeconds=30.
6. Compile actual SHVDN2 2.11.6 / net48 / C#7.3 Release, run all inherited 63/63 behavior, 21 static, 9 logger tests, new heartbeat/exhaustion/reentry tests, source preservation; matching installation/Visual Studio source. Package complete experimental source-only ZIP, full diff, test transcripts, hashes, MD/DOCX/PDF guide, rollback.
7. Mark actual GTA gameplay test NOT RUN on any new candidate. PR stays DRAFT until independent coordinator review and user game QA. Never merge main or development without explicit user sign-off.

P0 priority: collect defensible evidence, not guess the culprit. The owner is still running QA R2 live; no deployment actions.
