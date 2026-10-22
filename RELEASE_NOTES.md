# v0.3.3.1 issue #1 DIAGNOSTIC — EXPERIMENTAL

Issue #1: add bounded state checkpoints, tick-phase exception context, abort checkpoints, exception flood suppression and storage retry backoff. Crash cause remains unconfirmed; no GTA testing or crash resolution is claimed. No save/config schema, gameplay timing, inventory or money changes.

Real net48 / SHVDN2 2.11.6 Release build passed (0 errors; 2 existing warnings); 21/21 existing static checks and 9/9 production logger behavior tests passed. Detailed source diff, validation, migration/rollback and pending GTA scenarios: `docs/releases/v0.3.3.1-issue1/`. Versioned Word/PDF guide and complete install/Visual Studio source are supplied in `build/releases/GlassHeroesBusiness_v0.3.3.1_issue1_EXPERIMENTAL.zip`.

---

# Changelog: 0.3.2.9a -> 0.3.3.0 RC1

1. Visual cargo positioning: warehouse pallet, stockroom box base sourced from physical pallet.
2. New manager route states and five configurable points plus final approach.
3. Manager submenu reorganized into Management -> Profile/Hiring/Walking.
4. Manager workstation and approach moved to Employee Placement Settings.
5. Keyboard point SET feedback and marker visibility revised; editor ON state reflects saved marker toggles.
6. One-pending-order supplier recovery and double-confirm cancellation.
7. Supplier delivery ETA now in active gameplay seconds with additive save migration.
8. Removed side effect from manager training quote value display.
9. Preserved startup diagnostic hold and 30-second boot gate from stable 0.3.2.9a.

Static checks: 21 of 21 passed. No compiler or GTA execution performed.
