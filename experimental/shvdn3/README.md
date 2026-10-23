# v0.3.3.3-FS1 Financial Startup Hardening — EXPERIMENTAL

Focused branch investigation/shvdn3-financial-startup-audit, from reviewed financial 0fefcf8. Seven inherited model-by-name constructors now run after the startup delay and valid unpaused player, with bounded managed retries. FinanceV1, existing save/rollback, settings and all financial-parent features are preserved. The financial/waypoint/Mule combined candidate is HELD separately.

Read [current guide](../../docs/releases/v0.3.3.3-FS1-shvdn3-startup-hardening/DEVELOPER_GUIDE.md), [investigation report](../../docs/investigations/2026-10-23-financial-startup/REPORT.md) and versioned Word/PDF/results. The dump confirms a native GTA access violation; neither that incident nor the prior CLR stack overflow is attributed or proven fixed. Pinned 3.6.0 metadata shows the inherited early native hashing path; deployed Enhanced implementation remains unverified.

One installation source: Install/scripts/GlassHeroesBusiness.3.cs linked by the net48/C#7.3 project. Back up exact source and COMPLETE matching business data/settings before owner testing. Never alternate candidate builds against modified Save.ini without restoring matching complete backup. No live files, runtime DLLs or raw crash dump are included. GTA/Windows testing NOT RUN; no protected branch merge or live installation change.
