# Phase 0 validation — actual rerun

Tested existing source at `9ef7befac9822c722eb488b7d6683433ce80f9b3`, unchanged by this phase.

- Genuine net48 / C# 7.3 SHVDN3 3.6.0 Release: PASS, zero errors, two inherited warnings CS0169 and CS0649.
- Inherited behavior: **762/762 PASS** = 54 corrective + 9 logger + 22 migration + 497 feature + 180 production navigation checks.
- Static: **21/21 PASS**.
- Existing inventory source/structure checks: **13/13 PASS**, separate from behavior counts.
- Fourteen economy, save, reservation, receipt, time and order methods compared verbatim against RC1: unchanged (see validation.json).
- New inventory projection behavior tests: NOT ADDED; requires Phase A approval.
- GTA/runtime/LSRED testing: NOT RUN here; owner RC1 evaluation ongoing.

Captured commands and outputs accompany this report. Adapter tests do not execute the real GTA engine. The static tool reports `compiled:false` because it does not compile; build.txt separately records the successful actual Release compilation. The inventory structural tool's footer says compile not run by that tool; it is not the aggregate status.

The first orchestration attempt could not locate dotnet on PATH; it did not run a compiler. After loading the existing managed SDK environment, all listed checks were run successfully. No SDK or game runtime change was made.

Historical PHASE_A_REVIEW.md remains an accurate record of what had not run at its writing; these results supersede its NOT RUN statements for the existing source only. They do not complete Phase A acceptance or authorize B.
