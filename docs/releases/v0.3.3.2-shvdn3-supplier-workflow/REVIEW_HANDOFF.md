# v0.3.3.2 SHVDN3 Supplier Workflow — EXPERIMENTAL handoff

Baseline manager-deliveries: b777c64da7fc64241a2ac175b4013ff40cc864ec, clean and remote-verified before changes.

Linear focused review stack (no merges):
1. feature/shvdn3-nearby-delivery-range, base feature/shvdn3-24-manager-supplier-deliveries.
2. feature/shvdn3-inventory-menu-workflow, base nearby-delivery-range.
3. feature/shvdn3-mixed-supplier-orders, base inventory-menu-workflow.
4. feature/shvdn3-native-pause-compatibility, base mixed-supplier-orders (complete candidate).

Read CURRENT DEVELOPER_GUIDE.md/Word/PDF, validation.json, preservation.json, focused 01–04 diffs and full source.diff. The current guide supersedes older range/menu/cart exclusions in historical v0.3.3.1 reports. All previous feature evidence retains its original source hashes.

Review package: build/releases/GlassHeroesBusiness_v0.3.3.2_SHVDN3_supplier-workflow_EXPERIMENTAL.zip and matching manifest. One source directly linked by the v3 project; no live data/runtime DLLs/active v2 script included. Source hash and package receipt accompany this report.

Implemented: all four requested priorities plus low-stock draft fill/feedback. Deferred: supplier-order history, automatic unattended reorder. Draft cart is not persisted until submission. Do not downgrade a pending mixed order; finish/cancel or restore complete matching source/data backup. Old pending single-item orders are preserved.

GTA/Windows testing of this candidate NOT RUN. Prior owner report says manager offsite/transition works and session smooth/crash-free; it does not certify this source. Crash root cause remains unproven. Native pause display, actual UI readability and new range/route behavior require owner QA.

PR_BODIES.md has exact per-branch comparison links and prepared draft descriptions. github-pr-attempts.txt records API outcomes when available. No PR creation is claimed without a successful returned URL. All pushes are feature branches; main/QA R2/baseline are untouched. No live installation or saves/settings modified.

Focused implementation commits: ba6e110 (range), d6df28e (menus), b008850 (mixed orders), 6e66036 (native pause). A small separate followup clarifies over-limit cart preview text. The final review/package commit records the exact source hash.

All four feature branches were pushed. All four draft creation attempts returned `Post "https://api.github.com/graphql": Forbidden`; no PR was created. Use PR_BODIES.md comparison links/descriptions to create Drafts manually. Main/QA R2 and both baseline branches were checked unchanged in protected-remote-heads.txt.
