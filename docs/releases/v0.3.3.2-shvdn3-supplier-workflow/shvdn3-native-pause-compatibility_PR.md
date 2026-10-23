## Native GTA pause compatibility

Yield custom overlay/input to Esc and controller Pause/Start; retain Backspace/controller Back and active/unpaused clocks. Includes complete stack verification/package.

Related #24 / #25 QA. Based on `feature/shvdn3-mixed-supplier-orders`; isolated linear commit, no merge or live GTA modifications. Preserve completed repair/sales/wage/quote/pallet features and player-only Wholesale.

Validation: genuine net48/C#7.3 Release against SHVDN3 3.6.0, 0 errors/two inherited warnings. See `docs/releases/v0.3.3.2-shvdn3-supplier-workflow/04-pause.md` and corresponding focused build/test logs. Final stack validation.json, 21 static checks, method/hash preservation and clean extracted review rebuild are available on the final branch.

GTA/Windows tests of this new candidate NOT RUN. Range/navigation/native-pause display requires owner QA; prior smooth gameplay report applies to the baseline. Keep Draft; no merge authorization.
