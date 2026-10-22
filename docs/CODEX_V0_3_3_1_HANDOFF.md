# CODEX HANDOFF — v0.3.3.1 Corrective Patches

**Prepared:** 2026-10-22
**Owner:** ShaggyOS Glass Heroes project
**Working integration branch:** `development/v0.3.3.1` — do **not** modify `main`.
**Status:** Ready for Codex coding tasks. No GTA tests conducted by this handoff.

## Important preflight

1. Read root `AGENTS.md`, `docs/V0_3_3_1_TEST_PLAN.md` and actual source.
2. Inspect the existing experimental crash-diagnostics draft [PR #9](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/pull/9) from `fix/1-idle-crash-diagnostics` (issue #1). The branch already includes a reported successful Release compile, 21 static checks, nine isolated logger tests and Word/PDF guides. **It has not been in-game tested; no crash fix is confirmed**. Do not independently duplicate its logger changes.
3. [PR #8](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/pull/8) is a **documentation-only** audit of the same incident. Avoid accidental duplicate documentation or conflicted merges.
4. The user is away/resting; proceed through code/static tests and packaging where possible without waiting for interactive Q&A. Explicitly report GTA tests as **NOT RUN**; do not claim the build is stable.
5. Keep exact install filename `GlassHeroesBusiness.2.cs`; target GTA Legacy build 1.0.3889.0 and SHVDN2 2.11.6, C# 7.3, .NET Framework 4.8.
6. Never overwrite user `Save.ini` or `Settings.ini`, delete user jobs/inventory, create off-session profits/timers, or introduce persistent manager staff respawn loops.

## Next queue (focus per issue, separate branches and PRs)

### First — issue #2, P0 keyboard/controller parity
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/2

- Inspect `OnKeyDown()`, `PollController()`, `ExecuteSelection()`, `NavigateBack()`.
- De-duplicate physical keyboard Enter and mapped GTA frontend Accept; de-duplicate keyboard Back / gamepad Back; prevent accidental second action after submenu state changes.
- Preserve controller-only behavior exactly as users tested. Restore correct parent-page selection on Back, and correct point-editor SAVE/SET labels. A held Enter must not buy twice or satisfy two confirmation stages.
- Add focused behavioral harness covering keyboard, controller, mixed input, hold/keyup, Back and nested editor.

### Second — issue #3, P1 warehouse pallet, shipment and local marker
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/3

- Preserve assignment/ownership/payment ledger. Once pickup is *ready*, spawn the warehouse package and pallet when streamed into range even if its assigned pickup vehicle is >24 m away; require the correct vehicle for the actual load.
- Keep reliable ready-state marker/instructions independent from vehicle proximity.
- Fit the pallet to the floor and prop box to its support; bounded model request, cleanup, stream/reload, no duplicates.
- Do not confuse with legacy floating **shop stock display boxes**, removed under #6.

### Third — issue #5, P1 direct player handoff
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/5

- When configured to hand package to player, bypass supplier-driver waypoints, approach player directly with bounded repath/timeout and stable cargo handoff.
- Regular drop-off **must continue** using configured waypoint route.
- Preserve order status, retry/recovery and invoice/stock once-only guarantees.

### Fourth — issue #6, P1 remove legacy floating stockroom decorative display boxes
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/6

- User explicitly chose to hide/remove the old floating **stockroom display boxes** instead of tuning their heights.
- **Preserve newly delivered / express package props**: these were placed correctly during GTA testing.
- Preserve the distinct **warehouse pickup** package/pallet required by #3.
- Remove only Glass Heroes-owned obsolete visuals, not MLO geometry, job inventory, stock records or packages.
- No screenshot required; ensure streaming/reload doesn't re-create obsolete visuals.

### Fifth — issue #4, P1 car-spot marker heading and front indicator
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/4

- In `MarkParkingSquare`, use verified GTA heading convention and vehicle forward axis.
- Show unambiguous front arrow/crossbar and correct rectangle long axis at heading 0, 90, 180, 270 degrees; preserve heading saved from player and preview performance/draw distance.

### Sixth — issue #7, P2 simplify mechanic break-to-bay routes
https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/7

- Eliminate default unnecessary Break→Bay waypoint detours; preserve optional custom routes if needed for complex MLO layouts.
- Preserve old route keys as compatible save data. No deletion of existing user configuration.
- Ensure mechanics remain able to complete repair work and do not fall into path/retry loops.

## Change integration and quality gate
- Use **one feature branch per issue** based on `development/v0.3.3.1`, with a **draft PR** to that integration branch. If GitHub PR creation fails, push code and report the branch URL; the coordinator can open the PR.
- Do not edit or merge `fix/1-idle-crash-diagnostics` while working on unrelated issues. Avoid merging or cherry-picking to `main`.
- If source fixes span the same monolithic C# methods, sequence them deliberately and rebase/resolve overlaps; avoid a single unreviewable monolithic PR. Integrate only after code review.
- For every code change: versioned source, Visual Studio copy, install source, unified diff, full changelog, before/after explanations, actual compile/static/unit test logs, and Word/PDF developer documentation. Follow `AGENTS.md`.
- Perform tests using available SDK; don't present synthetic 30-min idle simulation as a GTA session.
- No actual game access is implied. Separate **automated PASS** from **GTA NOT RUN**.
- Before offering the user an experimental combined ZIP, confirm SHA256 equality of install and Visual Studio source and verify only one active Glass Heroes script is needed.
- Build a combined **v0.3.3.1 EXPerimental / QA-only** package after issue PR reviews; clearly list which issues are included and any missing or unverified functions.
- The first user game test should prioritize three cold starts, >30m idle at shop with LS RED, then keyboard Enter/Back, warehouse cargo, standard/direct handoff, marker heading, box cleanup, route simplification and save/reload.

## User-facing status when finished
Report links to branches and draft PRs, build status, test outputs, changed behavior, unresolved blockers, a versioned install ZIP, and the DOCX/PDF guide. Clearly say **the stack-overflow cause is unproven** until independently reproduced/diagnosed, and that the package is **experimental**, not approved for production. Do not demand a new user test until the user returns.
