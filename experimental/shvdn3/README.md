# v0.3.3.3 SHVDN3 Financial Transparency — EXPERIMENTAL

Current branch feature/shvdn3-financial-transparency, based on preserved supplier-workflow 422baa7. All prior supplier range/menu/cart/pause, repair coverage, retail sales, wages, fixed quote persistence and pallet geometry remain. Independent waypoint crash diagnostics are not included.

Read [current guide](../../docs/releases/v0.3.3.3-shvdn3-financial-transparency/DEVELOPER_GUIDE.md), [handoff](../../docs/releases/v0.3.3.3-shvdn3-financial-transparency/REVIEW_HANDOFF.md), Word/PDF and actual validation. Income splits never add remote twice; lifetime/shift reports distinguish sales, cash, accruals, owner transfers and purchase-basis profit. New atomic reporting history starts at upgrade, retaining 256 rows plus opening checkpoint/aggregates; historical chronology is not invented.

One installation source: Install/scripts/GlassHeroesBusiness.3.cs, directly linked by net48/C#7.3 project with official SHVDN3 3.6.0. No live save/settings/runtime DLLs included. Preserve exact source plus complete matching business data; never enable duplicate/v2+v3 instances or delete journal keys. Older-source changes can stale the audit checkpoint; restore matching full backup for rollback. Owner wallet/business file cannot promise hard-crash atomicity across game saves.

GTA/Windows candidate testing NOT RUN, Enhanced runtime binding unverified, crash cause unproven. Deployed source identity for October 23 crash remains UNVERIFIED. No protected-branch merge or live installation/data change.
