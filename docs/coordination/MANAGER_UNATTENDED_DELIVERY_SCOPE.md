# Manager unattended supplier delivery — implementation scope (#24)

Status: DESIGN/SOURCE AUDIT ONLY. No gameplay code changed, build not run, GTA tests not run. Not an installable release.
Baseline: migration/qa-r2-shvdn3-prep commit 430a202e7b10e3f191acccb7a094723568bd14e9 (draft #21).
This branch must remain separate from main, development, QA #17, crash #18, wholesale #19/#22 and migration #21.

## Confirmed current behavior
Audited experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs at the pinned commit.
- OnTick lines 883–889 derives remote mode from clocked-in + manager tier >=0 + placed shop + player beyond configured remote distance.
- Lines 895–900 decrement and periodically save DeliveryRemainingGameSeconds using activeGameSeconds. Line 921 invokes ProcessSupplierDelivery on throttled visual refresh.
- ProcessSupplierDelivery lines 7030–7049 gates ETA, keeps wholesale warehouse/cargo collection separate, and otherwise permits legacy nonphysical CommitSupplierDelivery.
- Lines 7063–7066 prioritize direct-player approach. Stage 4 lines 7129–7134 waits for collection if a parts pallet is set; no manager-specific receipt branch exists.
- TrySupplierPackageInteraction lines 7617–7651 credits stock/invoice only at pallet deposit, then transfers owned parcel visual and sends truck away.
- CommitSupplierDelivery lines 6973–7008 guards no-order/quantity/capacity, updates inventory, package provenance, invoice, completion count, clears order then calls Save().
This is the unattended gap. Do not label it the source of the reported CLR overflow.

## Policy and state transitions to implement
Initially restrict automation to local/express suppliers (1/2). Wholesale supplier 0 retains physical pickup, registered vehicle and deposit rules, including prepaid legacy cases until explicitly audited.
1. Pending ETA: do nothing until remaining active game seconds reaches zero.
2. Manager employed + clocked in + placed shop + valid live player outside existing remote-distance boundary: simulate receipt without spawning driver/truck/package. Select a valid owned pallet slot with crate capacity plus aggregate part capacity. Commit through one shared guarded path and record delivered provenance for later nearby visuals.
3. Nearby manager-controlled receipt: retain physical truck parking/unload; route driver to an available configured pallet, bypass player chase/handoff for this policy. At confirmed arrival, detach/place parcel and commit once. Never teleport a box through unknown collision or require the player to accept it.
4. Without eligible manager, preserve manual player handoff and existing regular delivery behavior. Display which receipt policy is active and why an order is waiting.
5. Reevaluate before terminal receipt: approaching/leaving boundary, manager removal or clock-out may change policy; do not steal a player-carried parcel. A carried/vehicle-loaded shipment remains under its existing physical ownership path.
6. Near-to-far transition may release only GH-owned visuals and continue original order virtually; far-to-near may spawn only if order remains uncommitted. No stock changes from entity cleanup.
7. Missing pallet/capacity/model, blocked route, dead/missing driver, stream-out: keep original order and use bounded retry. No unlimited native scans, recursive route restarts or per-frame logging. Recheck capacity immediately before commit.
8. Pause/loading/script reload: zero progress. Use the existing activeGameSeconds sample for every NEW route dwell, arrival, timeout and retry budget. Existing supplier stage DateTime timestamps must not be reused to count unattended progress across pause. Keep UTC order ticks as identity/legacy migration only, never elapsed ETA.

## Exactly-once and persistence gate
Extract a single receipt operation shared by manual, remote and nearby receipt. Snapshot order identity before visual work and revalidate at commit; no second order or cancellation can reuse it. Commit inventory, invoice (zero additional charge for prepaid), completion count, masks and order clearance together.
The current comment claiming one persist operation does not establish durable atomicity: CommitSupplierDelivery mutates memory before Save(). Audit Save() exception handling and ScriptSettings write guarantees; require a success-reporting persistence boundary. On failed save, restore the prior in-memory snapshot or retain a guarded retry transaction; never refund or invoice again.
Prove restart behavior for failures before write, during write and after durable write but before visual cleanup. If persistence cannot meet this guarantee, leave implementation blocked at this gate rather than claiming exactly once. Any journal/receipt ID must be additive, backward compatible and included with business data in one durable snapshot; avoid two independent writes that create an ambiguity window.

## Preservation and implementation boundaries
Modify experimental v3 only; keep any corresponding project source/reference consistent. Preserve all saved keys/paths and unknown keys, stock, balances, invoices, wages, settings, customers, vehicles and manager records. Do not migrate live saves, modify runtime ASIs/DLLs, or publish an installable ZIP as part of this scope. Legacy v2 source unchanged.
Audit Save/load and malformed input before adding metadata. Old saves must load with safe defaults and no retroactive income or automatic wall-clock catch-up.

## Required acceptance matrix
| Case | Required result |
| --- | --- |
| Local + express, manager away, ETA >0/at zero | No early receipt; exactly one stock/invoice receipt at zero |
| No manager / clocked out / unplaced shop | No manager simulation |
| Wholesale ready/carried/loaded/unloaded | Existing pickup and vehicle validation unchanged |
| Nearby regular + direct-player setting | Manager policy drops at pallet without chase; manual mode remains usable |
| Paused for longer than ETA/retry/arrival dwell | No progress, no stock/invoice/timeout during pause |
| Exit game, wait, reload | Remaining active time preserved; no off-session progress |
| Capacity full, crate slots full, pallet missing | Original order retained; retry succeeds once after capacity restored |
| Prepaid legacy delivery | No second payment; cancellation refund at most once |
| Boundary crossed each way during each physical stage | One order, no duplicate entities/parcel/receipt |
| Player already carrying or wholesale vehicle loaded | Automation cannot steal or independently credit shipment |
| Manager fired / clock-out during route | Policy reevaluated, order retained |
| Driver death, entity deletion, model unavailable, blocked route | Bounded recovery, no lost paid order or unbounded loops |
| Cancel before/after receipt; repeated interaction/tick | No duplicate refund, receipt or invoice |
| Persistence failure + reload at each commit boundary | Correct prior-or-committed snapshot, exactly-once result |
| Stream out/in after success; visual setter throws | Owned visuals recover independently of accounting |
| Legacy save round-trip | All original and unknown keys preserved; other saves untouched |

Validation required after implementation: net48/C#7.3 SHVDN3 build, inherited behavior/static/diagnostics/parity checks, meaningful fault-injection receipt tests, matched Install/project source checks and save preservation diff. Then opt-in GTA local/express/offsite/nearby/pause/reload/streaming QA. Existing branch PASS results are not evidence for future edits.
Related: #1, #5, #20, #21, #24. No merge or live-install changes authorized.


## Oct 22 follow-up: 21:01:53 local crash and SHVDN3 load evidence

Reviewed cached attachments from the referenced chat (10 accessible files; preview says 11, so one attachment is unavailable). Raw logs/dump and personal save paths are not uploaded here.

- Report_2026-10-22_21-01-57.txt: Legacy 1.0.3889.0, abnormal exit 0xC00000FD STACK_OVERFLOW, WER clr.dll 4.8.9310.0 +0x19F47B, ~1h20m uptime.
- CrashAnalyzer(5).log: process began 19:41:32; exit recorded 21:01:53.419, exitCalled=0, crashState=0; last observed stack-overflow SEH ~18.954 seconds earlier. Thus exit/report time is not necessarily first-fault time. No matching 21:01 dump is supplied.
- SHVDN log: Enhanced 3.9.0.6, GlassHeroesBusiness.3.cs successfully compiled 19:56:30, resolved API 3.9.0, GlassHeroesBusinessV3Experimental started 19:56:32. This is positive v3 compilation/start evidence (~65m before exit), NOT crash attribution, exact deployed-source identity, or proof migration fixes the overflow. Log has no named managed failure after startup.
- RPH log confirms Los Santos RED and its organisation add-on loaded in this session; other SHVDN scripts include PSRP and PropertyManager. Deprecated v2 scripts remain in the environment. No one script is identified by the CLR module alone.
- Separate 19:35:59 startup failure: 0xE06D7363, KERNELBASE +0x6055C, spdlog_ex; supplied report's stack includes GTAV.PedPropLimitAdjuster.asi frames. Its 19:36 dump belongs to that earlier failure, not 21:01. Logger/permissions are a hypothesis for the earlier failure, not a demonstrated cause for the later overflow.
- ReShade contains shader include/compile errors and reference-count warnings; RPH has deleted-vehicle task errors. OpenIV's last archive and PSRP's recent file activity are chronology, not causal stacks. CargoOperations log contains repeated session banners; hashes.ini is a model lookup list, not hashes of deployed GH source.
- User waiting for express delivery is useful reproduction context. It does not prove delivery recursion. Clearing folder read-only coinciding with successful launch does not establish the later crash's cause.

Next evidence: matching managed/native dump with thread stacks at overflow; exact installed GH source SHA256 and GH diagnostic log (not present); repeat matched GH-on/GH-off cold sessions with same other mods, idle and express/local delivery, record pause/offsite transitions. Do not change the user's live install automatically. No fatal-stack-overflow catch claim, no per-tick disk logging. Root cause remains unproven.

Related: #20 / #21 (v3), #18 (forensics). Manager delivery work is a separate behavior change, not a crash cure.