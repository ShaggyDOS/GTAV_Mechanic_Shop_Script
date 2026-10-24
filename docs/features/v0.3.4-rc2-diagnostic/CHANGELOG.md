# v0.3.4 RC2 Diagnostic Experimental — October 24, 2026

- Recovered and checksum-verified owner RC1 source; dedicated RC2 diagnostic branch preserves RC1/main.
- Added bounded managed entry/finally-exit breadcrumbs around ten mechanic/customer-vehicle transfer scopes. Stable-state coalescing, 64 snapshot slots, 12 ENTER admissions per ten seconds, writer reentry/failure containment and one 128 KiB rotated backup.
- Original vehicle tasks, AI, routes, invoice/stock/save logic and active-unpaused progression unchanged. No new saved key, settings default or migration. Diagnostic-only; no verified crash correction.
- Actual Release: 0 errors / two inherited warnings. Full inherited 954 plus 31 new trace tests: 985/985 PASS. Static 21/21, inventory structure 13/13 and exact-restoration preservation PASS. GTA/Windows execution NOT RUN; cause UNATTRIBUTED.
- Packaged matching source/project, checksums/manifest, raw validation, source diff, Word/PDF guide, capture instructions, GTA checklist and full-data rollback reference for owner review. No merge, deployment or v0.3.5 work.
