# v0.3.3.1 SHVDN3 complete review candidate — EXPERIMENTAL

- Includes the four focused #27 repair coverage, #25 manual sales, nine-box pallet placement and individual manager wage changes.
- Isolated #27 reload followup: bounded accepted quote/stock allocation/remaining-work journal, data-only startup and normal physical/remote/background restoration after reopening.
- Atomic all-route settlement rolls back money, stock, paid/job flags, counters and XP on save failure; completion audit follows commit, with game-time-bounded retries.
- Pending accepted stock stays reserved before NPC streaming. Graceful abort/entity loss preserves accepted data without stale entity handles or offline progress.
- Journal-to-ledger consistency stamp fails closed on stale older-build financial/stock changes; invalid/duplicate/unsupported records preserve the original save and stop startup before callbacks.
- Build/tests and clean extracted-package compile are recorded with current SHA. GTA testing NOT RUN, Windows/Enhanced runtime unverified, crash cause UNATTRIBUTED. Main/QA R2/live saves/settings/GTA untouched; no manager supplier automation or merges.
