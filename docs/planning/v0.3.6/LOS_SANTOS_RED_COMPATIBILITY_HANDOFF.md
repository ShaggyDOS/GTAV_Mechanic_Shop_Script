# Glass Heroes × Los Santos RED — Static Compatibility Handoff
**Evidence date:** 2026-10-24  
**Workstream:** v0.3.5 QA blocker carry-forward to v0.3.6 entry gate, with v0.3.8 long-term compatibility hardening.  
**Status:** Investigation notes only. This document does not authorize gameplay fixes or claim a crash root cause.

## Verified artifacts
Owner provided these Los Santos RED v1.0.0.513 files for non-executing static inspection:
- `Los Santos RED(1).dll`: 13,359,616 bytes; SHA256 `dd0f0734f88bdf3b174a5c191b09e9a8db5801a8828efec9cc80eb326b5e466a`.
- `Los Santos RED.pdb`: 16,991,744 bytes; SHA256 `f10a19f57a14401f6f6b78c2158c7af25d3886661e24e52b1e78ea72c347df16`.
- `LosSantosRed.rar`: 46,951,363 bytes; SHA256 `5047469001d227f71d34483624fbe59b7b786ffc7d27f45bb1ba49e5e07b26b7`.

The DLL is a .NET Framework 4.8 assembly whose CodeView PDB signature (`0c5bffab-6dac-4b99-99cc-08eeb0c68d66`, age 1) matches the supplied PDB. If a crash dump belongs to this exact version, the symbols may assist debugging; they are not standalone root-cause evidence.

RAR5 archive inventory: 3,207 entries (~108 MB uncompressed), including 156 XML configs, 2,608 MP3 files and 274 PNGs. 155 XMLs under 7 MB were statically inspected; large `SaveGames.xml` not parsed. **Archived settings are not independently confirmed as the actual installed game settings.** Do not commit binary, debug symbol, audio, photo or full third-party archive material into the GH repo.

## Glass Heroes LSR location config
`LosSantosRed/Locations+_GlassHeroesBusiness.xml`:
- `Glass Heroes`; `IsEnabled=true`; `IsOwnable=false`.
- Entrance (-244.9314, -1396.76, 31.49066); heading 291.319.
- Blip/marker/directory/taxi visibility enabled, SP and MP map enabled.
- Prompt: `Inquire About Business`; range ActivateDistance=225 / ActivateCells=4; 24-hour open period.
- Could create competing markers/business prompts alongside GH; **not established as a crash cause**. Avoid assuming generic LSR business payouts enter GH cash since `IsOwnable=false`.

## Settings.xml overlap leads (ARCHIVED VALUES, not confirmed installed)
| LSR setting | Archived value | GH concern |
|---|---|---|
| WorldSettings/CleanupVehicles | true | Who can delete customer/valet vehicles? |
| WorldSettings/ExtendedVehicleCleanup | true | Lifecycle/mission-flag cleanup |
| WorldSettings/SetMissionFlagOn | true | Entity persistence ownership |
| WorldSettings/UpdateVehiclePlates | true | Preserve customer car identity and invoice linkage |
| WorldSettings/CheckAreaBeforeVehicleSpawn | true | Crowded queue / customer drop-off |
| WorldSettings/DefaultToMPMap | true | Custom MLO map/nav/streaming |
| WorldSettings/ReplaceVanillaShopKeepers | true | Receptionist/ped interactions |
| VehicleSettings/ScaleEngineDamage | true | v0.3.5 limited engine-health repair |
| VehicleSettings/ScaleEngineDamageMultiplier | 3 | Repeatable repair-verification thresholds |
| VehicleSettings/NonRoadworthyVehicleCheckDamagedTires | true | v0.3.5 one-flat-tire repair |
| VehicleSettings/NonRoadworthyEngineHealthLimit | 300 | Engine-damage diagnosis |
| VehicleSettings/PlayerEnteredPersistantVehicleLimit | 3 | Player-valeted customer car persistence |
| PoliceSpawnSettings/LocationSpawnedPedLimit | 9 | Ped budget near Strawberry |
| PoliceSpawnSettings/LocationSpawnedVehicleLimit | 9 | Vehicle budget |
| PerformanceSettings/EnableHighPerformanceMode | true | Fix config across A/B runs |
| PerformanceSettings/TaskAssignmentCheckFrequency | 500 | Units not validated |

**Do not disable or change these settings wholesale**. Investigate targeted effects using owner-approved single-variable A/B runs and backed-up XML.

## Observations from LSR DLL (static strings only)
- `Spawn Task: ERROR DELETED VEHICLE`.
- `LegalJobManager: mechanic car spawn error`.
- `A vehicle has pulled into bay {0}. Press ~y~J~s~ to check it in.`
- `LegalJobManager: mechanic repair sequence error`.
- GTA task names such as `TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE`, `TASK_VEHICLE_DRIVE_WANDER`, `TASK_LEAVE_VEHICLE`, `TASK_ENTER_VEHICLE`.

LSR contains its own mechanic workflow and several vehicle-spawn subsystems. String presence is not proof they operate near GH or touch GH-owned cars. A prior RAGE/LSR log recorded `Spawn Task: ERROR DELETED VEHICLE` at 16:27:23 before a 16:28:35 stack-overflow CTD, but the deleted car's owner and causal connection are **unknown**.

## Separate, unresolved failure classes
1. **RPH/LSR cold startup failure** (Issue #39): 2026-10-24 17:49–17:50 local RPH log records `Los Santos RED_AppDomain` instantiation then stops. `LoadingScreenMsg: Fatal error` occurred earlier during initialization but does not give a stack trace or definitive crashing module. The 17:48 GH/SHVDN log is **another earlier launch**, not proof GH started in failed RPH process.
2. **Gameplay stack overflow**: several `0xC00000FD` faults in `clr.dll` with mixed scripts, often PSRP-inclusive. A prior PSRP-free GH+LSR test exited normally after ~1h49m, but one normal run is not a compatibility guarantee or proof of PSRP responsibility.
3. **v0.3.5 valet F2 controls stuck** (Issue #39): Physical Customer Workflow OFF/LEGACY, Future Job Valet Operator MECHANIC, Player Before Receptionist LEGACY; owner reports unable to change even after GTA cold restart. This is a separate in-game blocker, not attributed to LSR.

## Development guardrails for v0.3.6
- Treat **fix/verify v0.3.5 blockers** as a pre-integration gate: valet policy menu input/guard/persistence; protected live/pending customer orders; end-to-end valet and damage flows; save/invoice/stock invariants. Run same source and config in GTA to qualify.
- Accounting must use GH's own durable finance/repair IDs and once-only settlement. Do **not** infer payments or repairs from LSR business location, LSR job messages, visible vehicle damage or third-party cleanup. If car is deleted or plate changes, retain paid customer obligations and do not duplicate payout/part debit.
- Limited damage repairs: verify actual supported repaired state (flat tire/minor engine) before acknowledging settlement; consider LSR's engine multiplier and tire roadworthiness as unproven compatibility dependencies. Preserve prior accepted quotes on reload.
- Keep heavy-mod native calls and diagnostics bounded/state-transition-only; no per-tick verbose scanning.
- Existing save and `FinanceV1` preserved; no offline/pause-time manager accrual, no script-driven finance catch-up.
- No speculative LSR or GH gameplay patch solely from static strings/config. Separate RPH/PSRP root-cause forensics from financial scope.
- Main, v0.3.3.6 protected baseline, v0.3.4 RC1/RC2, Issue #37 work, installed game and owner saves untouched.

## Controlled QA sequence
1. Record current loaded LSR XML, DLL/PDB hashes, RPH, SHVDN, GH source SHA, active mods and exact error timestamp. Back up save/config.
2. Compare RPH with no auto-loaded RPH plugins; RPH+LSR without GH; RPH+LSR+GH without PSRP; separately GH+LSR without PSRP; optionally PSRP combinations later. Use cold launches and keep other conditions unchanged.
3. Monitor GH vehicle model, plate, handle, spawned owner, mission persistence and entity valid/destroyed state at handoff/repair/return; never spawn or scan per frame just for diagnosis.
4. Validate LSR's own mechanic job separately only if player uses it; distinguish its entities from GH's. Test marker/blip overlap with a single toggle at a time.
5. For a matching crash, obtain timestamp-aligned dump, RPH log, SHVDN log and GH diagnostics; debug with supplied matching symbols. Do not use an earlier .dmp for a later exception.
6. GTA acceptance: 1–3 mechanics, multiple customer cars, valet check-in/return, supported flat tire and engine repair, blocked bay/queue, damage state, supplier/payroll invoicing, no duplicate revenue or stock consumption, paused/closed/no offline progress.

## Ownership and scope
- Issue #39: v0.3.5 valet settings + RPH/LSR startup investigation (track independently, confirm reproduction).
- Issue #38: v0.3.6 finance/cost/invoice/reporting design; **require v0.3.5 QA fixes as entry criteria**.
- v0.3.8: deeper LSR/PSRP/RPH compatibility hardening and extended mixed-stack stress.
- Provide owner with exact branch/source SHA, tests, blockers, rollback and STOP for explicit phase approval before implementation, merge or release.
