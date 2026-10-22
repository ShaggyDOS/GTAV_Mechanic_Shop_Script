# PR #16 external coordination review — supplied by user in this chat

GitHub GraphQL and REST review retrieval returned Forbidden. The user supplied the following feedback directly; PR source heads were independently fetched with native Git.

> First external coordination review of combined QA candidate: The source includes all six corrective subsystems. Codex's branch artifacts report a real Release compilation (0 errors), 49/49 extracted-behavior tests, 21/21 static checks and source-copy equality. These are promising automated results, not GTA gameplay qualification. Thank you for preserving main and the live saves.
>
> Two follow-ups before presenting a unified crash-investigation build for user stress-testing:
> 1. Crash instrumentation is absent from this QA branch. Issue #1 logger remains in PR #9. With the previous CLR stack overflow at ~18 minutes idle, this candidate might still crash without the extra diagnostic context. Decide whether to combine the vetted diagnostic changes (and rerun compilation/tests) into a separate integrated QA branch before asking the user to stress-test; do not silently cherry-pick or call a merged untested build verified.
> 2. Warehouse finite retry exhaustion: UpdateWholesalePalletVisual() increments _warehouseVisualAttempts and returns permanently after five attempts if the player stays within the 80m streaming radius, even if both props are absent. Consider a bounded time-based retry window (e.g., reset attempts after an interval) without per-tick model requests, or a clear recovery instruction. This is relevant to the original missing-warehouse-package report. Please add a targeted exhaustion/retry test before claiming the warehouse issue fully resolved.
>
> Known limitations / UX to verify in GTA: Controller-only input and mixed keyboard/gamepad, heading marker orientation at all cardinal directions, existing MLO ground elevation, Save.ini schema addition DeliveredPackageMask1..4 and CustomJobRoutes, and player-handoff after game pause. Protect financial/inventory semantics across save/reload.
>
> All 7 PRs (#10–#16) are draft, development/v0.3.3.1 and main have not been merged. As issue PRs and the QA PR touch overlapping GlassHeroesBusiness.2.cs, do not merge both sets independently. Prefer final validated integrated candidate after code review and real GTA tests.
