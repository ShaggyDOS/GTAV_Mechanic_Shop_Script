# Complete SHVDN3 review candidate — EXPERIMENTAL

FINAL head: `experimental/shvdn3-economy-pallets-wages-review`. This branch extends the four focused feature branches by ancestry, with an isolated Issue #27 accepted-quote persistence correction. No branch merge was performed. Earlier package limitations are historical and superseded by this current source/guide. Review each focused delta once in dependency order; do not independently merge overlapping whole-source candidates.

| Order | Branch | Base | Recorded behavior cases |
|---|---|---|---|
| 1 | feature/shvdn3-27-repair-cost-coverage | migration/qa-r2-shvdn3-prep / 430a202 | 117 |
| 2 | feature/shvdn3-25-manual-parts-sales | repair coverage / 2532afe | 146 |
| 3 | feature/shvdn3-pallet-box-stacking | manual sales / 7a7afe7 | 169 |
| 4 | feature/shvdn3-manager-wage-payment | pallet layout / 00b2d4e | 191 |
| 5 | feature/shvdn3-repair-quote-reload | manager wages / c05930f | 232 |
| 6 | experimental/shvdn3-economy-pallets-wages-review | saved quotes / 604c622 | 234 |

Final actual Release compilation: net48/C#7.3, official ScriptHookVDotNet3 3.6.0, 0 errors/two inherited warnings (CS0169/CS0649). Behavior: 85 inherited + 32 repair + 29 sales + 25 geometry + 22 wages + 41 reload = 234. Static 21/21. Scope guards verify existing save expressions, integration ordering, OnTick and protected v2 code; all pass. Synthetic INI adapter/file tests do not certify Windows SHVDN/GTA save behavior.

The new quote journal is additive and bounded: fixed prices, exact allocations and remaining work survive save/reload/close/abort, no saved entity handles or file-age catch-up. Physical customers return through existing guarded arrival, other jobs through existing mode/manager rules after reopening. Atomic settlement rolls back all affected state on save failure. The final placement-only correction gives a fresh bounded visual budget after a successful deposit, preventing a previously exhausted budget from skipping partial-layer repositioning. Two additional tests cover overlap and request-rate safety without financial changes. A consistency stamp rejects stale journals after older-build money/stock changes, preserving the original file rather than replaying payable work. Read the CURRENT developer guide's schema, failure and rollback rules.

GitHub draft creation returned `Post "https://api.github.com/graphql": Forbidden`. No new PR is claimed created. Branches and exact PR_BODY.md files are prepared; the comparison links below use the correct review bases and should remain Draft when created through an available UI/client.

1. [Repair #27](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/migration/qa-r2-shvdn3-prep...feature/shvdn3-27-repair-cost-coverage?expand=1)
2. [Manual sales #25](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-27-repair-cost-coverage...feature/shvdn3-25-manual-parts-sales?expand=1)
3. [Pallet layout](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-25-manual-parts-sales...feature/shvdn3-pallet-box-stacking?expand=1)
4. [Manager wages](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-pallet-box-stacking...feature/shvdn3-manager-wage-payment?expand=1)
5. [Accepted quote reload correction #27](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-manager-wage-payment...feature/shvdn3-repair-quote-reload?expand=1)
6. [Final pallet-refresh delta](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-repair-quote-reload...experimental/shvdn3-economy-pallets-wages-review?expand=1)
7. [Complete source comparison from PR #21](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/migration/qa-r2-shvdn3-prep...experimental/shvdn3-economy-pallets-wages-review?expand=1)

Protected remote heads checked unchanged after feature work: main e13e977744f373a218d02215a193878bf95db252; development/v0.3.3.1 86919e7648d55dc80295fa19b68560988fac41e0; QA R2 7693ed2167405f475dd4d36392c5df1f257fe178; PR #21 foundation 430a202e7b10e3f191acccb7a094723568bd14e9. Original v2 install/VS sources, project and solution retain their recorded hashes. No live installation, saves or settings were read or changed.

Complete archive: build/releases/GlassHeroesBusiness_v0.3.3.1_SHVDN3_final-review_EXPERIMENTAL.zip, matching manifest and package-receipt.json. One .3.cs linked by the separate project; all focused guides/Word/PDF reports, current tests/logs and source diffs/hashes. No active v2 source, live Save.ini/Settings.ini or runtime DLL is bundled. Clean extraction rebuilds the real product.

GTA gameplay/stress testing NOT RUN. Windows execution NOT RUN. Installed Enhanced 3.9.0.6 compatibility NOT VERIFIED. Actual model/collision/MLO support and tall fallback realism require owner QA. Past never-saved quotes cannot be reconstructed, and hard-crash unsaved progress may need repeating. Stack-overflow cause UNATTRIBUTED / UNPROVEN; no crash-fix claim. Manager supplier deliveries, forensic and blip work stay separate. Review first; no deployment or main/QA R2 merge is authorized.
