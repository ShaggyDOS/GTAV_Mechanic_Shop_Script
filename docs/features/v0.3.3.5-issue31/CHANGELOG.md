# v0.3.3.5 / Issue #31 — EXPERIMENTAL development review

Baseline branch at 1582ae8443704cc35926723f11dee8ef912d44be, source identical to Issue30 53205ad36aa398ebf6c9cb6626ea0d21ad135c0b (SHA256 4aba9e70bdc6c29a551cd4feb257c03d5bec7bf7218eedf7b32a1e3888a28e71). Installed RC2 identity unknown; no mismatch reported. No tag assumed.

Commit A: Rename visible Staff Break Area to Mechanic Break Area and reuse the same _breakArea / EmployeeBreakArea. Group Walking Routes by Mechanics, Receptionist, Manager, Customers and Supplier Driver. Mechanics exposes shared arrival/service stops, bay approaches, per-worker route-mask selection, checkpoint and existing mode/recovery controls. No save keys or waypoint semantics changed. Existing Back context reused for role nesting. Actual Release 0 errors/two inherited warnings; focused navigation results in commit-a-navigation.txt. GTA NOT RUN. No merge/release/live install.
