# v0.3.3.1 review handoff

All six source branches are pushed and independently based on development/v0.3.3.1. Draft PR creation was attempted for every branch and failed with GitHub GraphQL API Forbidden. No PR creation is claimed. Coordinator can open the prepared draft descriptions against the integration branch. No main or integration merge has occurred.

| Issue | Branch | Commit | Draft PR |
|---|---|---|---|
| #2 | [fix/2-menu-input-parity](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/2-menu-input-parity) | 0ed037afe927 | NOT CREATED: API Forbidden |
| #3 | [fix/3-warehouse-ready-cargo](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/3-warehouse-ready-cargo) | fcc448369031 | NOT CREATED: API Forbidden |
| #5 | [fix/5-direct-player-handoff](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/5-direct-player-handoff) | 17b434f34dd9 | NOT CREATED: API Forbidden |
| #6 | [fix/6-hide-legacy-stock-boxes](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/6-hide-legacy-stock-boxes) | 3d660a24bdc9 | NOT CREATED: API Forbidden |
| #4 | [fix/4-parking-heading-markers](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/4-parking-heading-markers) | c841beca5f95 | NOT CREATED: API Forbidden |
| #7 | [fix/7-direct-mechanic-job-routes](https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/tree/fix/7-direct-mechanic-job-routes) | cc6f3b5a78fa | NOT CREATED: API Forbidden |

Complete requested titles/bodies and captured creation errors are in PR_DRAFTS/. Existing PR #9 (diagnostics) and PR #8 (audit) are separate and unmodified.

Combined candidate: qa/v0.3.3.1-review-pending. REVIEW PENDING, EXPERIMENTAL, QA ONLY. Includes #2, #3, #5, #6, #4, #7; excludes #1 diagnostics. Real Release builds passed for every issue and the combined source; combined behavior tests 49/49 PASS, static checks 21/21 PASS, core preservation guards PASS. GTA testing NOT RUN; Windows build NOT RUN; stack-overflow cause UNPROVEN.

Review the two additive visual/route metadata changes before integration: DeliveredPackageMask1..4 (legacy untagged display reconstruction hidden, stock preserved) and CustomJobRoutes=false (direct default, all old points/masks retained). Do not overwrite any user Save.ini or Settings.ini.
