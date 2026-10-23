# SHVDN3 four-change review handoff — EXPERIMENTAL

The complete candidate is `feature/shvdn3-manager-wage-payment`. Four focused branches are stacked linearly; no merge was performed. Review each delta in order and avoid independently merging overlapping whole-source candidates. All review states are pending. GitHub returned `Post "https://api.github.com/graphql": Forbidden` for draft creation; no new PR number has been assigned by this work. Exact prepared bodies are in each release folder's PR_BODY.md.

| Order | Focus / head branch | Review base | Product commit / behavior tests |
|---|---|---|---|
| 1 | `feature/shvdn3-27-repair-cost-coverage` | `migration/qa-r2-shvdn3-prep` | `2532afe` / 117 |
| 2 | `feature/shvdn3-25-manual-parts-sales` | `feature/shvdn3-27-repair-cost-coverage` | `7a7afe7` / 146 |
| 3 | `feature/shvdn3-pallet-box-stacking` | `feature/shvdn3-25-manual-parts-sales` | `00b2d4e` / 169 |
| 4 | `feature/shvdn3-manager-wage-payment` | `feature/shvdn3-pallet-box-stacking` | final head / 191 |

Every stage has an actual net48 Release build, 21/21 static checks, a full focused source diff, method-boundary/data-preservation report, matching source ZIP receipt and versioned Word/PDF developer report. Final total is 85 inherited (54 corrective + 9 logger + 22 migration), 32 repair, 29 sales, 23 pallet and 22 wages = 191 behavior cases. The eight inherited SHVDN3 pallet cases retain their invariants with a bounds-support assertion replacing the obsolete guessed-height expectation; the v2 template remains unchanged. These deterministic extracted-behavior tests do not execute GTA.

## Review blockers and limits

- Draft creation is blocked by GitHub API Forbidden; branches and PR bodies are pushed/prepared, but no PR creation is claimed. The links below open comparisons using the correct bases; retain Draft status if creating them through the UI.
- Issue #27 reload requirement is not fully implemented: the PR #21 foundation never persisted accepted runtime customer objects/quotes and still cancels them on reload. This stack preserves in-memory legacy prices, current ledger save fields, pending wholesale costs and existing reload semantics, but accepted customer restoration needs a separate reviewed design before that part of #27 can be called complete. No deferred/offline payment is fabricated.
- Cost basis is a disclosed conservative current-source estimate with optional explicit overrides, not recovered historical lot costs. Fixed labor excludes walking/idle/stalls and later wage changes. Do not claim exact historical recovery or full-business profit.
- Nine-box layout is validated against stub bounds, not actual engine collision meshes or MLO support. Fallback footprints can produce taller stacks; their visual realism needs GTA inspection. Current earned mask/count/capacity semantics are unchanged.
- Atomic production file helper was fault-tested only on synthetic cloud-host files; Windows/SHVDN INI execution is NOT RUN.
- GTA gameplay/stress testing: NOT RUN. Installed Enhanced 3.9.0.6 binding: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No forensic/map-blip changes or manager supplier procurement were integrated.

## Comparison links

1. [Repair coverage #27](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/migration/qa-r2-shvdn3-prep...feature/shvdn3-27-repair-cost-coverage?expand=1)
2. [Manual sales #25](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-27-repair-cost-coverage...feature/shvdn3-25-manual-parts-sales?expand=1)
3. [Pallet placement](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-25-manual-parts-sales...feature/shvdn3-pallet-box-stacking?expand=1)
4. [Manager wage payment](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-pallet-box-stacking...feature/shvdn3-manager-wage-payment?expand=1)

## Protected state and package

Remote heads independently checked after feature work:

- main: `e13e977744f373a218d02215a193878bf95db252`
- development/v0.3.3.1: `86919e7648d55dc80295fa19b68560988fac41e0`
- qa/v0.3.3.1-final-review-r2: `7693ed2167405f475dd4d36392c5df1f257fe178`
- migration/qa-r2-shvdn3-prep (PR #21 foundation): `430a202e7b10e3f191acccb7a094723568bd14e9`

Original matching v2 installation/VS sources, project and solution retain their recorded hashes. No game installation, live business save or settings was read or edited. No merge or protected-branch push occurred.

Final review archive: `build/releases/GlassHeroesBusiness_v0.3.3.1_SHVDN3_manager-wages_EXPERIMENTAL.zip`, with matching manifest and docs package-receipt.json. One active .3.cs, linked project/solution, all four focused guides, Word/PDF reports, current tests/logs and hashes are included; no active v2 source, runtime DLL or Save.ini/Settings.ini is packaged. Do not install the whole ZIP. Review first; owner authorization is required before a controlled game test. Follow the documented data backup, one-generation-only installation and rollback steps.
