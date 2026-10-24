# v0.3.4 RC2 Diagnostic Experimental — implementation checkpoint

Owner-authorized RC2 recovery scope, October 24, 2026; historical crash-investigation Issue #1 context. This is diagnostic-only. No crash correction is implemented or claimed; the responsible script and stack-overflow cause remain unknown. GTA testing NOT RUN.

Recovered source commit e312d8c9500f4a990eb993b82e8e76b55711d3de matches owner SHA256 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120. Approval-only RC1 tip 75e3623f608a05d1a2b59d12c243dbcdfc061358 has identical source and is the branch parent. Neither checkpoint was changed.

Ten existing methods have diagnostic entry/finally-exit scopes; preservation.json lists exact methods and proves removing those scopes plus the added diagnostic block restores the full RC1 source byte-for-byte. Thus original tasks, routes, accounting, inventory, save expressions/defaults and business clocks are preserved. No new native query/task or world scan. Only two new diagnostic file paths may be written; no save/settings changes. Full source diff, excerpts, Word/PDF, diagnostic interpretation, GTA checks and rollback instructions are prepared in the documentation commit before owner review.

Actual clean net48/C#7.3 Release against SHVDN3 3.6.0: 0 errors / two inherited warnings. Reproduced RC1: 954/954 behavior/navigation. RC2: 985/985 (including 31 new trace tests), 21/21 static, 13/13 inventory structural, exact-restoration preservation PASS. Existing tests receive no-op sink adapters only; original assertions are byte-preserved. Real GTA/native behavior and crash reproduction NOT RUN. Detailed logs: baseline/ and final-validation/.
