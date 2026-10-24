# Phase A — inventory visibility

Owner authorized Phase A, then explicitly authorized continuing through all phases without further phase gates. No merge, tag, release, packaging or installation authority is inferred.

Changes: on-demand per-item on-hand/reserved/free/incoming and estimated cost; compact overview rows; shared placed-pallet unit capacity versus historical parcel slots; forecast headroom and hold reason; reservation-aware reorder advice subtracting incoming without editing the cart. Existing universal minimum target five and other configured thresholds retained. Advice is not an order. Negative legacy values are display-clamped only; wide display sums avoid wrapping.

No new save keys, stock counters, price changes, per-Tick work, point edits or receipt changes in this commit. Existing eight rows and Back/controller routing retained. New production-method adapter tests cover all categories, physical/remote/autonomous/restored/deduplicated reservations, legacy/mixed pending quantities, negative values, unit/parcel limits, estimates and no side effects from browsing.

Actual results: 30/30 inventory behaviors; 13/13 existing inventory structural checks; genuine net48/C#7.3 SHVDN3 Release build, zero errors/two inherited warnings. Full inherited suites rerun at final integration. GTA display/navigation and runtime testing pending. Notification rendering under actual UI scale remains an owner QA item.

Test-fixture correction: the over-capacity fixture initially retained 30 default specialized units, so its expected total was wrong; the fixture now explicitly zeroes those units. No accounting code was changed to satisfy it.
