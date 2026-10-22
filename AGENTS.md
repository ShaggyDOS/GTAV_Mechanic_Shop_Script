# AGENTS.md — Glass Heroes Mechanic Shop Script

These instructions apply to all folders unless a more specific `AGENTS.md` exists.

## Project context
- GTA V Legacy single-player on game build 1.0.3889.0.
- Current development base: **v0.3.3.0 RC1**, file `GlassHeroesBusiness_v0.3.3.0_RC1.2.cs`.
- Last user-tested rollback: **v0.3.2.9a diagnostic**. RC1 is **NOT** certified stable.
- Runtime API for this milestone: **ScriptHookVDotNet 2.11.6** via filename `.2.cs`. Do not migrate to v3 until the planned V4 milestone.
- User runs a **heavy mod stack** (e.g. Los Santos RED, sometimes RAGE Plugin Hook, 70+ scripts). Prefer performance safety and defensive lifecycle cleanup over visuals or feature breadth.
- Do not edit the user's game installation, gameconfig.xml, DLC files, or live `Save.ini`.

## Release and branch policy
- `main`: protected-by-convention user baseline. **Never push experimental code to main**.
- `development/v0.3.3.1`: milestone integration branch. Prefer small issue-specific branches and draft PRs against the integration branch.
- One GitHub issue per independently testable bug; reference issue numbers in PRs/commits.
- The repository contains legacy uppercase and lowercase parallel folders; do not casually delete/move files. Prefer lowercase `docs/`, `scripts/`, `verification/` for new files while leaving prior release artifacts intact.
- Do not report a build as verified unless a real compiler run succeeded; do not report GTA gameplay as verified unless tester evidence exists.

## Inviolable gameplay rules
1. Business, manager virtual work, supplier delivery timers, wages and any other progress advance **only in active, unpaused GTA gameplay**. Never derive catch-up progress from wall-clock time across closed GTA sessions.
2. Preserve existing `Save.ini`/`Settings.ini` values and additive migration. Existing jobs, inventory, money, and outstanding orders must not duplicate or vanish on reload.
3. Startup gating from v0.3.2.9a matters: no premature world natives; preserve boot delay / player validity checks. Do not enable per-frame disk writes.
4. Stream physical workers, cars, props, packages by proximity. No unnecessary continuous native calls, model loads, spawning, or pathfinding in idle loops.
5. Any spawned ped, prop, blip or vehicle needs bounded retries, ownership tracking, and safe cleanup on teardown/reload; never delete unrelated entities.
6. A wholesale order must have one consistent identity and payment/stock ledger, including failure recovery and cancellation.
7. Keyboard and controller inputs must execute one logical action per press; menus/labels must reflect stored state. Opening a menu must not mutate savings/inventory.

## Debugging v0.3.3.1
- P0: [stack overflow while idle](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/1). Oct 22 2026 at 00:34:33 local, 18m06s into session; code `0xC00000FD`, `clr.dll+0x19E9DD`. **No minidump saved**. Do not attribute to the last archive, PSRP, or a method without proof.
- P0: [keyboard double Enter/Back](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/issues/2). Review both `OnKeyDown` and `PollController` for duplicate actions.
- Other tracked release issues #3–#7 include warehouse cargo visibility, heading-aligned parking outlines, delivery driver handoff, grounded stockroom boxes, and simplifying mechanic paths.
- Record diagnostic checkpoints **at transitions only**, rate-limit repeated errors, and keep frame overhead tiny.

## Documentation contract — MANDATORY for every code change
Each change must include:
- Version, issue number, exact changed file/method, why the change is required, intended new behavior.
- Before/after code excerpt and full unified source diff.
- Save/config key changes, defaults and migration/rollback implications.
- Compilation checks and static tests **with actual results**; separate from *pending* in-game tests.
- Manual reproducible GTA regression test(s), including cold launch, reload, save/reload, shop distance/streaming and the heavy-mod environment.
- Updated human-readable changelog and **versioned Word (DOCX) and PDF developer guide** in the release package before asking the user to test a new build. Mark unverified builds experimental.
- Preserve a complete installable source package and Visual Studio project matching the install source.

## Do not
- Do not reset saves, silently remove features, implement offline profit, or migrate wholesale accounting without migration tests.
- Do not rewrite the entire mono-script opportunistically; small focused diffs make crashes easier to isolate.
- Do not merge to `main` or claim crash resolution without repeated in-game confirmation.
- Do not rely on missing screenshots or dump files as if they had been received.

## v0.3.3.1 priority
1. Crash investigation/instrumentation (#1), and keyboard input fixes (#2), as **separate changes**.
2. Warehouse visibility (#3), heading markers (#4), handoff routing (#5).
3. Box/pallet exact offsets when screenshot is provided (#6); mechanic route simplification (#7).
See `docs/V0_3_3_1_TEST_PLAN.md`.
