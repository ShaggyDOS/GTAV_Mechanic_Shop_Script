The RC2 Placement Settings menu makes mechanic routes difficult to discover, and customer queue/repair auditing exposed FIFO, spawn retry and bay-ownership gaps. This v0.3.3.5 candidate groups walking routes by role, renames the visible Mechanic Break Area without changing EmployeeBreakArea, restores two incorrect Back paths, and applies focused customer lifecycle safeguards.

Closes #31.

Separate commits cover role/menu corrections, final Back audit, staged FIFO/bounded creation retries, assignment/invalid-entity settlement checks, on-demand status and final validation/docs. Five shared physical work waypoints remain shared; each mechanic retains independent outbound/return masks. No placement engine or scheduler redesign.

A surviving older Car 3 cannot be overtaken by a newer replacement Car 2. Intake/assignment require a configured unreserved staffed logical bay; paid cars retain bay ownership until return. Creation failures retain pending requests with session-only GTA-time backoff. Both physical and teleport repair modes reject missing/destroyed entities before settlement. Existing atomic settlement, accepted quote recovery, transfer helpers, supplier/finance algorithms and active-game clock sampling are preserved.

Actual validation: SHVDN3 3.6.0 net48/C#7.3 Release, 0 errors/two inherited warnings (CS0169, CS0649). 683/683 behavior checks: 467 inherited, 178 navigation/persistence, 38 focused workflow. 21/21 static checks; 346 existing methods pinned unchanged with additional constructor/economy boundary assertions. Final full run was followed only by expansion/rerun of the navigation tests; no product changes after that full run. Tests use deterministic native/entity/movement adapters and temporary files, not GTA.

Source SHA256: 741c97730856adad11cd98d985e41d4eb325c9788af5ceebeee965a0eb64125f. Baseline Issue30 source 53205ad / 4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71; owner's installed RC2 hash remains unverified, with explicit permission to use this repository baseline. Save/config schema changes: NONE.

Paid pickup cars and unaccepted staged requests remain transient on reload; no completed-car persistence added. Conservative 4m bay separation can defer tight MLO layouts. Existing engine driving/worker recovery remains, with GTA validation pending. Enhanced 3.9.0.6/LS RED compatibility and extended stability NOT VERIFIED. No Issue28 crash attribution/fix, release, live installation or merge.

Review docs/features/v0.3.3.5-issue31/DEVELOPER_GUIDE.md, NAVIGATION_MATRIX.md (all 65 menus), GTA_TEST_CHECKLIST.md, source.diff, validation/preservation logs and versioned Word/PDF. Owner extended-menu/parallel-service QA is the next gate before packaging or larger features.
