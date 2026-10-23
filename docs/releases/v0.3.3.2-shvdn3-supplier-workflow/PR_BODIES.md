# Delivery range gap

Head: `feature/shvdn3-nearby-delivery-range`
Base: `feature/shvdn3-24-manager-supplier-deliveries`

https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-24-manager-supplier-deliveries...feature/shvdn3-nearby-delivery-range?expand=1

## Delivery range gap

Use one shop-distance boundary for physical/virtual receipt; retain collision/model/route and transaction guards.

Related #24 / #25 QA. Based on `feature/shvdn3-24-manager-supplier-deliveries`; isolated linear commit, no merge or live GTA modifications. Preserve completed repair/sales/wage/quote/pallet features and player-only Wholesale.

Validation: genuine net48/C#7.3 Release against SHVDN3 3.6.0, 0 errors/two inherited warnings. See `docs/releases/v0.3.3.2-shvdn3-supplier-workflow/01-range.md` and corresponding focused build/test logs. Final stack validation.json, 21 static checks, method/hash preservation and clean extracted review rebuild are available on the final branch.

GTA/Windows tests of this new candidate NOT RUN. Range/navigation/native-pause display requires owner QA; prior smooth gameplay report applies to the baseline. Keep Draft; no merge authorization.


---

# Inventory workflow and menu fitting

Head: `feature/shvdn3-inventory-menu-workflow`
Base: `feature/shvdn3-nearby-delivery-range`

https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-nearby-delivery-range...feature/shvdn3-inventory-menu-workflow?expand=1

## Inventory workflow and menu fitting

Separate six inventory/supplier destinations, preserve parent navigation, fit bounded columns and clarify one-unit sales.

Related #24 / #25 QA. Based on `feature/shvdn3-nearby-delivery-range`; isolated linear commit, no merge or live GTA modifications. Preserve completed repair/sales/wage/quote/pallet features and player-only Wholesale.

Validation: genuine net48/C#7.3 Release against SHVDN3 3.6.0, 0 errors/two inherited warnings. See `docs/releases/v0.3.3.2-shvdn3-supplier-workflow/02-menu.md` and corresponding focused build/test logs. Final stack validation.json, 21 static checks, method/hash preservation and clean extracted review rebuild are available on the final branch.

GTA/Windows tests of this new candidate NOT RUN. Range/navigation/native-pause display requires owner QA; prior smooth gameplay report applies to the baseline. Keep Draft; no merge authorization.


---

# Mixed supplier purchase orders

Head: `feature/shvdn3-mixed-supplier-orders`
Base: `feature/shvdn3-inventory-menu-workflow`

https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-inventory-menu-workflow...feature/shvdn3-mixed-supplier-orders?expand=1

## Mixed supplier purchase orders

Add adjustable four-item cart, one supplier/ETA/invoice, additive identity-bound persistence, legacy compatibility and atomic submit/receipt/cancellation.

Related #24 / #25 QA. Based on `feature/shvdn3-inventory-menu-workflow`; isolated linear commit, no merge or live GTA modifications. Preserve completed repair/sales/wage/quote/pallet features and player-only Wholesale.

Validation: genuine net48/C#7.3 Release against SHVDN3 3.6.0, 0 errors/two inherited warnings. See `docs/releases/v0.3.3.2-shvdn3-supplier-workflow/03-cart.md` and corresponding focused build/test logs. Final stack validation.json, 21 static checks, method/hash preservation and clean extracted review rebuild are available on the final branch.

GTA/Windows tests of this new candidate NOT RUN. Range/navigation/native-pause display requires owner QA; prior smooth gameplay report applies to the baseline. Keep Draft; no merge authorization.


---

# Native GTA pause compatibility

Head: `feature/shvdn3-native-pause-compatibility`
Base: `feature/shvdn3-mixed-supplier-orders`

https://github.com/ShaggyDOS/GTAV_Mechanic_Shop_Script/compare/feature/shvdn3-mixed-supplier-orders...feature/shvdn3-native-pause-compatibility?expand=1

## Native GTA pause compatibility

Yield custom overlay/input to Esc and controller Pause/Start; retain Backspace/controller Back and active/unpaused clocks. Also labels over-limit drafts explicitly and includes complete stack verification/package.

Related #24 / #25 QA. Based on `feature/shvdn3-mixed-supplier-orders`; isolated linear commit, no merge or live GTA modifications. Preserve completed repair/sales/wage/quote/pallet features and player-only Wholesale.

Validation: genuine net48/C#7.3 Release against SHVDN3 3.6.0, 0 errors/two inherited warnings. See `docs/releases/v0.3.3.2-shvdn3-supplier-workflow/04-pause.md` and corresponding focused build/test logs. Final stack validation.json, 21 static checks, method/hash preservation and clean extracted review rebuild are available on the final branch.

GTA/Windows tests of this new candidate NOT RUN. Range/navigation/native-pause display requires owner QA; prior smooth gameplay report applies to the baseline. Keep Draft; no merge authorization.
