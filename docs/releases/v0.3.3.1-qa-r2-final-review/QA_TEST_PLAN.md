# QA R2 game test matrix — NOT RUN

Final ChatGPT review is pending. This document prepares future tests; it does not request or record a game session. Test only the one install source after final review. Preserve all user Save.ini/Settings.ini files and the previous script outside the active scripts folder.

| Scenario | Expected behavior | Status |
|---|---|---|
| Three cold launches, LS RED off/on and normal heavy stack | One active script, existing boot delay/hold/player gate, QA R2 banner | NOT RUN |
| >30m shop idle and >30m normal play | No crash/duplicate entities; stable state has no diagnostic heartbeat | NOT RUN |
| Keyboard/controller-only/mixed input, held Enter and two-stage cancellation | One logical action per press, exact Back parent/cursor, correct SET/SAVE labels | NOT RUN |
| Heading 0/90/180/270 and diagonals | Rectangle follows actual vehicle facing with front arrow; stored heading unchanged | NOT RUN |
| Existing elevated/interior MLO floors | Pallet resolves to floor, box rests on support, no new model/prop flood | NOT RUN |
| Ready wholesale with assigned vehicle >24m away | Pair/marker/hint visible independently of vehicle; collection works | NOT RUN |
| Transient warehouse model failure while staying within 80m | Automatic retry batch after cooldown, no order reset or leave/re-enter required; no duplicate pallet | NOT RUN |
| Warehouse marker while models are unavailable | Locator/hint remain; no per-frame model requests | NOT RUN |
| Wrong/correct vehicle, load/unload/deposit, recovery/cancellation | Correct ownership; one stock/invoice commit; no duplicates or lost pending order | NOT RUN |
| Direct player handoff, moving player, pause/resume and timeout | Direct bounded approach, stable box, pending order survives visual defer; regular drop-off retains waypoints | NOT RUN |
| Old save DeliveredPackageMask1..4 absent and new delivery/reload | Legacy display hidden without stock/capacity loss; new real parcel persists by provenance | NOT RUN |
| CustomJobRoutes missing/false and explicit advanced opt-in | Direct default jobs; all old points/masks retained; advanced route completion and finite blocked recovery | NOT RUN |
| Shop-distance stream-out/in, script reload/abort | Owned cleanup and no duplicate workers/props; existing jobs/orders preserved | NOT RUN |
| Save/reload with outstanding orders/jobs/bills | Money, stock, order identity and invoices preserved; optional metadata additive | NOT RUN |
| Close GTA mid-delivery and restart after delay | No off-session timer/profit/wage/manager progress | NOT RUN |
| Logging storage failure or repeated errors if encountered | Rate-bounded records; gameplay unaffected by diagnostic writer failure | NOT RUN |

Controlled model/error failures are covered in the automated harness; forcing them in GTA would require a separately reviewed diagnostic procedure. Do not alter gameconfig, registry, MLO geometry, user saves or live mod installation to manufacture a failure here.

If a fatal CLR stack overflow recurs, keep available Errors.log, ScriptHookVDotNet.log, event details, versions, elapsed time and a supported crash dump if available. No prior dump or repeated GTA resolution evidence exists. Crash cause remains UNPROVEN.
