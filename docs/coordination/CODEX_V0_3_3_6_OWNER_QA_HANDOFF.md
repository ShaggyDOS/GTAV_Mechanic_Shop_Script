# Codex handoff — Glass Heroes v0.3.3.6, owner v0.3.3.5 actual-GTA evidence

The owner explicitly authorized beginning next development while separately testing additional scripts. **P0 service recovery is the first gate.** The saved RC1 is not stable enough in service flow to be treated as a finalized reliable release, despite no crashes with GH + Los Santos RED. Treat menu/economy improvements as independent follow-up commits, and report defects before any release. Never merge to main without owner approval.

- Source base: exact `22bff0990bdc771215ebcb587e8426059df14336` (`feature/shvdn3-335-customer-repair-workflow`); expected source SHA256 `741c97730856adad11cd98d985e41d4eb325c9788af5ceebeee965a0eb64125f`.
- New feature branch: `feature/shvdn3-336-service-recovery-business-polish`. **The only current branch change is this handoff document.**
- P0 issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/33
- Next UX/realism feature issue: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/34
- Existing v.5 draft PR: https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/pull/32 — preserve, do NOT merge.
- Historical crash investigation Issue #28 remains independent; no crash cause proven by this test.

## First priority: P0 source + runtime diagnosis, then focused correction

Owner's GTA v.5 log: `Errors(20261024-044451).log`, plus owner `Save.ini` were inspected by ChatGPT **outside GitHub**. They are not automatically present in the Codex workspace. Do not fabricate that they have been copied. Minimal grounded observations are embedded in Issues #33–34, without uploading private full save data.

**v.5 segment** begins at 2026-10-24T03:42:09Z (matching exact source SHA), gameplay gate 03:42:39Z, extends to 04:43:32Z; 91 v.5 log lines. In that segment: no managed exception, 16 saved-waypoint recoveries, 5 emergency walking recoveries, 3 `Mechanic failed to park at saved point` cancellations, 14 `Queued car unable to park; retaining pending customer` events, 1 direct drop-off parking timeout, 4 `Customer departed` cleanups. Multi-order counts reached 3; repair-income records indicate some jobs completed, not an absolute lack of settlement. Note `Shop closed` cancellations were user-generated and normal.

**Save configuration reveals important potential causes**:
- EmployeeSettings: CustomWalkingPaths=True, CustomJobRoutes=True, PickupUseReverseArrivalWaypoints=True, route recovery/teleport=True.
- ALL THREE mechanics use JobOutboundMask=8, which selects only work point 4. ServiceWaypoint4.Set=False. Points 1,2,3 are saved but NONE is selected on outbound job route. BuildRoute currently silently skips absent enabled waypoint.
- Reverse physical pickup route from Break → Arrival4 → Arrival3 → Arrival2 → CustomerDropoff contains 17.05m, 15.44m, 15.62m, 17.26m legs and doubles back, plausibly obstructed in custom Strawberry MLO. Actual pickup uses ArrivalWaypoints (not ServiceWaypoint masks), so do **not** claim mask mismatch independently explains all parking/pickup failures.
- Work position at service Bay3 is about 0.39m from saved car center (vs ~1.45m for bays 1/2), possibly obstructing mechanic repair animation. Do not silently rewrite any coordinates.
- The three current `Mechanic failed to park at saved point` errors use the same message for toBay and return-toDropoff: direction and Paid status **cannot be reconstructed** from these log entries; instrument before attributing.

Implementation goals after source diagnosis:
1. Clear, on-demand stage diagnostics: why mechanic is not assigned, which route mask is effective (and unset points), actual state `MechanicWalkingToPickup`, car/worker validity, bay occupancy, paused/offsite status. Separate progress of pedestrian collection, valet parking at bay, walking to workstation, mechanic animation/repair, return of paid car. Add rate-limited **direction-specific** logging and order IDs for genuine failures, not spam.
2. Correct the menus to describe full physical ON workflow `Break → Customer Car (walk) → Service Bay (drive) → Workstation (walk) → Drop-off (drive) → Break`; relevant waypoint editors for each leg, not a misleading `Break→Bay` walking label implying mechanics first walk to bay during car pickup.
3. Address no-useful-waypoint mask by diagnostic warning, safe explicit fallback or proposed migration with owner's confirmation: do not reset masks/INI keys or invent path points; verify preserving five SHARED physical work nodes and mechanic-specific outbound/return masks.
4. Prevent paid-order cancellation on return parking failure: bounded retries and if physically safe, **first exit the driver** then teleport only an **empty, clear car** to a clear drop-off with passenger/player/collision checks, preserve paid state and exactly-once invoice. If unsafe, HOLD the paid car until safe; do not teleport an occupied vehicle or settle twice. Unpaid bay parking failure requires separate safe handling, no premature work/revenue.
5. Queue-car parking retry should remain bounded and not silently reorder request FIFO; verify tight MLO/other scripts compatibility.
6. Add deterministic tests for all supported failure modes, both drive ON and teleport OFF, 1–3 staff, 3 queued, paid drop-off obstruction, physical worker blocked, absent selected waypoint, manual override, paused session and accepted-quote persistence.

## Second priority: owner-requested usability enhancements

As specified in Issue #34, small separate commits and test groups:
- Business Management: remove Deposit and Withdraw shortcuts, use existing Finances > Owner Cash Transfers with `1,000/10,000/100,000` selector and no more hardcoded $1,000 transfer; ensure wallet limit, non-Story-ped restriction, atomic financial Save rollback and tests.
- Parts Sales: use existing Build Supplier Order as menu model, allowing quantities and review/confirm preview with exact available unreserved stock and totals; preserve existing unit-cost-estimate pricing by default, do not silently add markups.
- Employee Status: on-demand accurate staff/bay/assignment/route/active job details; don't infer Working merely from `HasJob` or a recovery marker.
- Controller configuration: remove controller settings *menu* safely, retain runtime input and **Settings.ini** as authoritative (current source also reads Save.ini Gamepad.Enabled, so fix precedence/migration intentionally). No silent controller disable.
- Description footer: highlight-specific text for all reachable rows, especially Management, Employees, Customers, Inventory, Finances and nested placement; restore indices/back and controller tests.
- GTA **in-game clock** demand weighting: daytime peaks, quiet overnight including ~02:00, but preserve manual customer scheduling, customer queue capacity, accepted work and once-only stock/revenue. Never accrue while GTA closed or paused, and avoid offline wall-clock catch-up. Weighted scheduling should adjust ONLY creation of new requests, not delete existing work.

## Release/testing/approval gates
- Protect exact current v.5 RC candidate and corresponding full business-data backup; never touch actual owner GTA installation or uploaded user Save.ini.
- Commit P0 fix/diagnostic portion independently from menu/feature commits, with test evidence and review diff. If runtime reproduction cannot be deterministically established, do not assert root cause, add observability and conservative mitigations first.
- Compile net48 C#7.3 SHVDN3 and rerun >=683 inherited/new relevant behavior tests + static scope checks. Extend tests for finance, menus, FIFO, save, staff states and arrival demand.
- Prepare a detailed GTA check sheet requiring actual owner verification on GH + Los Santos RED and then additional mod stacks, including owner-observed paths and return stage. Do not claim GTA/PSRP tests without owner report.
- Provide versioned developer guide, DOCX/PDF guide, changelog, hashes, rollback/compat notes and draft PR only. NO merge, release, tag, auto-install, paid car unsafe teleport, changes to v.5 PR or main without explicit authorization.

Codex's first report should show exactly which failure(s) are source-confirmed, which need in-game instrumentation, proposed bounded handling for PAID vs UNPAID car failure, and per-commit test plan before marking milestone test ready.
