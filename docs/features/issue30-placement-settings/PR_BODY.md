Management exposes Expenses twice and location editing is spread across employee/customer/supplier menus. This menu-only change keeps Expenses under Finances & Accounting and adds a Placement Settings hub with Customer Locations, Employee Locations, Service Bays, Delivery & Stockroom, Walking Routes & Approaches, and Marker Display Settings. Employees/Customers placement shortcuts open that same hub.

Closes #30.

Financial deduplication is isolated in commit 8ed199e; hub navigation follows as a separate commit. Shared pages retain the calling parent and selected row; PointEditor, capture/save helpers, marker rendering, physical/teleport vehicle transfers, finance and all business clock/economy/supplier methods remain unchanged. Save.ini/Settings.ini schema changes: none. Service Bay vehicle/work coordinates stay separate.

Validation: actual SHVDN3 3.6.0 net48/C#7.3 Release build, 0 errors and two inherited warnings (CS0169, CS0649). 556/556 behavior tests: 467 inherited and 89 focused navigation/persistence checks. 21/21 static checks; 355 existing methods pinned unchanged. New tests cover category/editor Back paths, parent selection, aliases, keyboard/controller, native pause, marker toggle, no browsing payments/writes, and disk point/heading roundtrip. Native adapters do not constitute GTA testing.

Source SHA256: 4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71. Source matches RC1 before changes; deployed owner source identity and RC1 tag not independently assumed.

GTA testing NOT RUN. Enhanced 3.9.0.6 compatibility NOT VERIFIED. No release/installation/merge or Issue #28 crash claim. Review docs/features/issue30-placement-settings/DEVELOPER_GUIDE.md, full source.diff, validation logs and versioned Word/PDF for the structured menu listing, limitations and GTA acceptance checklist. Preserve complete matching backups for later QA.
