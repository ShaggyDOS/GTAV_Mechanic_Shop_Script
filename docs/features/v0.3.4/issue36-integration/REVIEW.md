# v0.3.3.6 MAINTENANCE INTEGRATION INTO v0.3.4 — REVIEW COMPLETE

Status: implementation and offline verification complete; integration acceptance PENDING NEW OWNER YES/NO. No testing release is prepared. GTA and Windows gameplay execution NOT RUN.

## Authorization and boundaries

The owner explicitly approved maintenance/v0.3.3.6-issue36 final candidate 2f1ebc268fe93d3a1c84a810ad87a01cf9235774 for controlled integration. That supersedes the maintenance snapshot documents' historical PENDING handoff wording. It does not accept this receiving integration or authorize main merge, installation, tag, release, RC package or further phase work.

Receiving branch: feature/shvdn3-340-inventory-procurement.

Preintegration baseline: c77591b10437564814c873203e073cc4ead1901e.

Original protected RC1: c8617aed5cda492214bbbe1907042bcafbfa0dc1.

Combined product-source commit: 8eb6cf93d4eef5af25e13b564e0d872dc3799807.

Combined source SHA256: 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120.

The final branch review tip includes verification/documentation commits after the four source cherry-picks and is reported in the completion message. No history is rewritten. An external preintegration Git bundle preserves c77591b, also retained as an ancestor.

## Exact accepted commits integrated

| Fix | Original maintenance SHA | Receiving cherry-pick SHA |
|---|---|---|
| A footer spacing/wrapping/cache | 4ad2fc4c344bafff0c098ef17ce06ff2145affb8 | e2039c59fa480b9969e275b7f2605c0e70a7493e |
| B shared stops versus route selections | 15467753fc4a7d376e10d57e1d9ab649e5e6a7aa | 7e034de2e9dc1f8d7e44e93dbba1896ce92dbb4e |
| C ready wholesale locator / explicit GPS | 21d865c50493bbc233baed6135ffe18f241bbed1 | 18a4e53b1d619cd1546d10e4e76f662987eddb36 |
| D canonical receptionist/manager editors | e4860da7accc386ab4b0d36c9c85b0cc4c55f7b7 | 8eb6cf93d4eef5af25e13b564e0d872dc3799807 |

Each cherry-pick records its original SHA with -x. The final 2f1ebc2 documentation-only commit was reviewed as the approval identity; it was not a fifth source fix. Its handoff/changelog are copied as clearly marked historical reference documents here. No unfinished or unrelated maintenance commit was integrated.

## Combined behavior and files

Product source changed: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. No project, installed script or owner data replacement. The four accepted commits also carry their maintenance tests and historical evidence. Local support changes are limited to the combined validator, preservation audit, required adapter dependencies and this review's documents. CHANGED_FILES.json is the exact file inventory; integration-source.diff is the product delta from c77591b; combined-source.diff is the full product delta from RC1.

- Measured/cached two-line footer layout and spacing, with bounded input/cache and no repeated width measurement for unchanged descriptions.
- Mechanic menus explain shared entry corridor, shared work stops, workstation approaches and per-mechanic/direction use; previews show selected/saved/unset. Saved coordinates, masks and movement AI remain unchanged.
- One owned long-range map blip for READY, uncollected physical wholesale cargo; explicit GPS yields to changed player waypoint. The map locator is independent of distance-limited box streaming. Bounded retries, cleanup/recreation and no stock/invoice effects.
- Canonical receptionist/manager walking editors are shared by management and Placement Settings with contextual Back and original settings/points.
- v0.3.4 on-hand/reserved/free/incoming visibility, mixed purchase review, live delivery states, last receipt/cancellation, manager LOCAL/EXPRESS receipts and atomic cargo/stock/invoice rollback remain intact.
- Active Deliveries now has six rows: status; order/last receipt; recover; cancel; Set Warehouse GPS; Back. The cart/review workflow is unchanged. Wholesale remains player-operated.

Four product-source conflicts plus one harness conflict were resolved deliberately; see CONFLICT_RESOLUTIONS.md/.json. No conflict required changing v0.3.4 architecture or approved accounting.

## Actual validation

Complete combined validation: 954/954 behavior checks, including 212 navigation checks; 21/21 static and 13/13 inventory structural checks; preservation PASS. Build: genuine net48/C#7.3 SHVDN3 3.6.0 Release, zero errors and inherited CS0169/CS0649 warnings. No new build warnings.

Maintenance suites: footer36 15/15; routes36 16/16; locator36 33/33. All v0.3.4 suites remain: inventory340 30/30; procurement340 12/12; receipt340 32/32; manager340 13/13; integration340 9/9. Finance tests retain $5,000 remote sales with/without payments, liability accrual versus cash movement, journal/save rollback and reload checks. Other inherited suites and original 54 corrective + 9 logger + 22 migration checks remain passing.

Preservation confirms 396 unchanged v0.3.4 members; identical save/config key expressions/defaults, FinanceV1/repair/quote/reservation/stock methods, route geometry/masks and original v2 files. The only accepted OnTick difference is locator sync at the existing roughly one-second visual cadence. Active/unpaused time sampling and zero offline catch-up rules are unchanged. No new save schema or migration is introduced by this integration.

Evidence: validation/commands.json, validation/validation.json, build.txt, features.txt, navigation.txt, static results and two-parent preservation.json. Tests execute production extracts through deterministic adapters; the actual full source separately compiles under net48. They do not execute GTA native rendering/pathfinding or certify Enhanced runtime compatibility.

Reproduce:

```sh
source /workspace/.cloud-setup/env.sh
python verification/inventory340/validate.py --issue36-integration --output /tmp/gh340-issue36-validation
```

## Known limitations and controlled testing

GTA testing remains NOT RUN for both the maintenance fixes and combined candidate. Pixel measurements use deterministic test metrics; actual 720p/1080p HUD layout, physical blip visibility, GPS arbitration, controller feel, streaming, role paths and extended LSRED performance require owner testing after separate authorization. No crash resolution or stability certification.

All previous v0.3.4 limits remain: one pending mixed order, one terminal outcome, historical earned parcel slots retained, no new pricing/categories or physical damaged-car repair. No accepted baseline maintenance is automatically backported to protected RC1. Original RC1 and owner Save.ini/Settings.ini/journals were not modified.

Preserve the matching complete scripts/GlassHeroesBusiness directory and script when a later test is approved. Do not alternate builds against the same modified sole save. Restore the complete matching backup for rollback; do not mix Save.ini from one candidate with another's changed companion data.

## Mandatory stop

v0.3.3.6 MAINTENANCE INTEGRATION INTO v0.3.4 — REVIEW COMPLETE

ShaggyOS Dev Team: Do you approve the integration and authorize continuation of the next v0.3.4 phase? YES / NO

No further phase, integration acceptance or test-release preparation occurs without explicit YES.
