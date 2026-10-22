# Glass Heroes v0.3.3.1 — corrective milestone plan

**Status:** planning / experimental development; no fix is considered verified.
**Integration branch:** `development/v0.3.3.1`. **Production baseline:** `main` is unchanged.
**Known working rollback:** v0.3.2.9a, one reported successful session with Los Santos RED + approximately 73 other scripts, not a release qualification.

## Issue tracker (priority order)
| Priority | Issue | Required behavior |
|---|---|---|
| P0 | [#1 Idle stack-overflow](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/1) | Diagnose safely and instrument without expensive logs or invented root cause |
| P0 | [#2 Keyboard enter/back](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/2) | One action per press, correct parent menu and cursor restoration |
| P1 | [#3 Warehouse pallet and package](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/3) | Pickup shipment visible when ready, independently of assigned vehicle range |
| P1 | [#4 Parking outline heading](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/4) | Geometry aligns with saved player heading, visible front arrow |
| P1 | [#5 Driver handoff routing](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/5) | Direct route to player for handoff, saved stops for standard delivery |
| P1 | [#6 Legacy stockroom box removal](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/6) | Hide floating legacy pallet-box visuals without altering the correctly placed newly delivered packages |
| P2 | [#7 Break-to-bay mechanic paths](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/7) | Simpler default routes, preserved settings and optional advanced paths |

Direct issue URLs use `https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/NUMBER`.

## User-reported behavior to preserve
- v0.3.2.9a successful gameplay after `[Diagnostics] HoldGameplayTick=false` and `StartupDelaySeconds=30`: manager hired, wholesale box pickup and deposit succeeded in an earlier trial.
- RC1: Express package placement succeeded; warehouse GPS waypoint set but no pickup pallet, box or local marker.
- Controller menu currently behaves well; keyboard Enter can duplicate actions and Back can move to the wrong page.
- Delivery-to-player must go directly to player; route stops remain for regular supplier drop-off.
- Break -> Bay mechanic route points are unnecessary for normal shop operation.
- User opted to remove floating legacy stockroom pallet-box visuals for now; no screenshot required. Newly delivered packages were placed correctly and must be preserved.

## Test matrix (record PASS / FAIL / NOT RUN and attach session logs)
| ID | Scenario | Required outcome | Current |
|---|---|---|---|
| B01 | Cold start GTA with RC candidate, LS RED disabled | GTA reaches Story Mode, menu usable after boot gate | NOT RUN |
| B02 | Cold start GTA with RC candidate, LS RED enabled and normal heavy mod load | Same; no duplicate script entities | NOT RUN |
| B03 | Three consecutive cold starts with no script-controller reloads | All three loads cleanly | NOT RUN |
| S01 | Idle at Glass Heroes for >30 minutes after startup | No crash, runaway entity counts or frequent disk log writes | NOT RUN |
| S02 | Play normally for >30 minutes; repeat pickup/delivery, streaming in/out | No crash, duplicate props, inventory or pay | NOT RUN |
| K01 | Keyboard Enter once in shop and nested menu | Exactly one action; no repeat purchases/training | NOT RUN |
| K02 | Keyboard Escape/Back from nested submenu and point editor | Exact parent page and expected selection | NOT RUN |
| K03 | Controller with keyboard attached | Controller remains reliable; no ghost keyboard actions | NOT RUN |
| W01 | Wholesale pending/ready with no nearby assigned vehicle | Warehouse box, pallet and local marker visible when ready | NOT RUN |
| W02 | Wholesale interaction with correct/incorrect nearby vehicle | Only correct vehicle may load; order credited exactly once | NOT RUN |
| W03 | Recover/cancel then reload/re-enter streaming range | No duplicate packages, charges or stock | NOT RUN |
| D01 | Delivery driver mode: hand directly to player | Driver approaches player, avoids configured waypoints | NOT RUN |
| D02 | Delivery driver mode: normal drop-off | Configured waypoints remain active | NOT RUN |
| P01 | Save bay heading north/east/south/west | Rectangle lines up with vehicle facing and front is clearly marked | NOT RUN |
| P02 | At shop, stream in/out and deliver a new package | Legacy floating display boxes absent; newly delivered package remains correctly placed after reload | NOT RUN |
| M01 | Mechanics return from break to bay in default mode | No spurious multi-waypoint detours or stuck loops | NOT RUN |
| T01 | Close GTA mid-delivery and restart | Supplier timer has NOT advanced from wall-clock downtime | NOT RUN |
| R01 | Script reload/quit with active peds, props and orders | Clean ownership disposal and restore, no duplication | NOT RUN |

## Release criteria
- Static checks and **actual SHVDN2 compilation** pass; record exact build tool and output.
- Critical P0 issues receive focused review and repeatable test results.
- Regression tests executed in GTA, not inferred from compile success.
- Developer Guide (.docx + .pdf), install source, Visual Studio source, unified diff, versioned changelog, migration/rollback and known limitations supplied together.
- User tests release candidate. **Do not merge into main until approved.**

## Crash forensics
Oct 22, 2026 at 00:34:33 local: 0xC00000FD stack overflow `clr.dll+0x19E9DD`, 18m06s after start, during shop idle. SHVDN shows compilation/start success and no managed exception. Business log shows previous successful wholesale state transitions, not a cause. No crash dump was saved. Keep this as open pending A/B reproduction, not a resolved diagnosis.
