# v0.3.3.1 issues #2–#7 — EXPERIMENTAL / QA ONLY

Open GlassHeroesBusiness.sln, targeting .NET Framework 4.8, C# 7.3 and ScriptHookVDotNet2 2.11.6. The project compiles scripts/GlassHeroesBusiness.2.cs, byte-identical to the Install/scripts/ copy. The historical root RC1 is a reference, not another active script.

All six independent issue branches and this isolated QA candidate have real passing Release builds. Combined static checks 21/21 and behavioral tests 49/49 pass, with two existing production compiler warnings. Windows compilation and GTA testing are NOT RUN. No crash cause or stability is confirmed; review is pending. See ../docs/releases/v0.3.3.1-qa-issues2-7/ for actual logs, before/after changes, full diff, additive save compatibility and rollback. Install one source only and preserve user Save.ini/Settings.ini.
