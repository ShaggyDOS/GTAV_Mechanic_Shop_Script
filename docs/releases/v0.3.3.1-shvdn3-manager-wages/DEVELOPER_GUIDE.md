# v0.3.3.1 SHVDN3 economy, pallets and manager wages — EXPERIMENTAL

Owner-requested manager wage payment change, October 22, 2026 (America/Chicago). Focused branch feature/shvdn3-manager-wage-payment, explicitly stacked on pallet layout 00b2d4e, manual parts sales #25 and repair coverage #27 from the SHVDN3 PR #21 foundation. This final head contains all four focused changes for review, without merging branches. Supplier delivery automation (#24/PR #26), map blip and forensic branches remain separate. Main, development and QA R2 remain unchanged.

## New manager action

Business Management → Expenses & Bills → Pay Manager Wages displays the current outstanding manager wage invoice and pays it separately from Pay All Bills. The new action is index 9, immediately before Back; prior action indices 0..8 are unchanged. It calls the same PayInvoice(ref _managerWagesDue, true, "Manager wages") already used by the existing Manager Profile page. There is one existing wage ledger, not a new charge or duplicate invoice.

Successful full payment subtracts the due amount from business cash, adds the same amount to paid payroll, clears the invoice and resets its age, then saves. The existing rule excludes manager wages from the manager's own payroll discount. Other outstanding bills, mechanic/receptionist wages, rent, supplier invoices, stock and pending deliveries are unchanged. Zero/negative invoices and insufficient funds cannot pay. Repeating the action, using the Manager Profile action afterward, or paying the manager before/after Pay All Bills cannot charge the same zeroed invoice twice.

The shared PayInvoice helper now checks active gameplay/unpaused state and expense overflow before mutation. It uses the atomic Save outcome introduced in the separate #25 branch; if saving fails, it restores due amount, cash, paid expenses, manager savings, invoice ages and receptionist overdue flag, without a success message. Existing receptionist/rent individual payments benefit from the same failure rollback; their successful accounting and staff payroll rebate remain unchanged. Pay All's original formulas are unchanged.

## Exact code and data boundary

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs changes executable code. GetMenuItems adds the action; GetMenuItemValue returns the existing amount due; GetSelectedDescription explains the individual payment; ExecuteBusinessExpensesSelection case 9 calls the existing helper. PayInvoice adds preconditions and failed-save rollback. Constructor changes its experimental build label only. Full changed-method list is in preservation.json; full source.diff is against 00b2d4e. There are no new wage keys, no changed wage accrual rates and no extra manager invoice.

```csharp
// New independent Expenses action, sharing the existing profile payment:
case 9: PayInvoice(ref _managerWagesDue, true, "Manager wages"); break;
// Before: Save(); followed by a PAID message even if the save failed.
// After: failed commit restores the captured invoice and money, then returns false.
if (!Save()) { /* restore full payment snapshot */ return false; }
```

Save.ini [Management] WagesDue, WageAgeSeconds and Savings retain their existing meaning/defaults. This change does not edit live Save.ini or Settings.ini. OnTick, ProcessEconomy, manager remote work, supplier timers, accepted price/cost helpers and pallet placement helpers are byte-for-byte unchanged relative to the pallet parent; active unpaused progression remains intact.

## Included review stack and limitations

Repair coverage (#27, first): fixed total = max(existing service/expertise total, estimated reserved acquisition cost plus allocated billable labor), shared across physical/background/remote paths. Default estimates $125/$113/$200/$157 reflect a conservative current-source ceiling, not historical lot costs. Optional [Pricing] UnitCostEstimate* keys are nonnegative, missing/negative falls back, zero is intentional. Prices/durations/source allocations are fixed on assignment, with no completion surcharge. Wages still accrue separately; idle/walking/stalls and later wage/config changes are outside billable estimates. Old CSV formats remain intact; RepairPricing.v1.csv is separate.

Manual sales (#25, second): Inventory & Suppliers → Parts Sales sells one unreserved stocked unit per explicit confirmation at that same fixed estimate, with no staff retail markup. It protects physical/background/remote reservations and excludes pending/cargo stock; retail revenue is separate from customer repair revenue. Additive [Business] PartsSalesRevenue and PartsSalesUnitsSold default to zero. A same-directory atomic snapshot retains the full old save payload/unknown keys and rolls back a sale if its commit fails. PartsSales.v1.csv is a separate audit. Windows ScriptSettings/NTFS execution remains untested; synthetic production-helper file fault tests passed on the cloud host.

Pallet layout (separate third): cached actual-model bounds, nine visible earned packages, preferred 4–4–1, centered partial layers and saved-heading rotation using the actual grounded prop. Alternate footprints may make taller stacks; actual GTA model/collision/MLO behavior and fallback realism remain to test. Legacy mask holes compact visible layout from the bottom without changing saved mask/count/capacity or fabricating old boxes. Delivery commit and supplier financial formulas remain unchanged. A missing lower visual hides unsupported higher owned props until bounded retry/re-entry.

Inherited limitation requiring explicit reviewer awareness: PR #21 does not persist runtime physical/remote customer objects or accepted quote history; reload still cancels these runtime orders. The cost branch retains in-memory legacy promised prices and never invents deferred payment after a crash, but it does not claim to restore a previously accepted customer entity/quote after reload. Automated reload-ledger tests verify modelled saved financial totals, not actual GTA order restoration. A durable accepted-order restoration design remains outside these focused changes and is a review consideration before claiming complete Issue #27 reload behavior.

The unknown 0xC00000FD CLR stack-overflow cause remains UNATTRIBUTED / UNPROVEN. These features and the automated tests are not evidence of a crash fix. Installed Enhanced 3.9.0.6 binding and engine behavior remain NOT VERIFIED.

## Reproduction and manual QA

From this branch, activate the cloud .NET environment when available, then run:

```bash
python verification/economy/verify_candidate.py --label manager-wages --profile wages --suites repair sales pallets wages
python verification/economy/package_candidate.py manager-wages --parent 00b2d4e
```

The product is actually compiled as net48/C#7.3 against pinned official ScriptHookVDotNet3 3.6.0; behavior harnesses run extracted production members under deterministic .NET stubs. The original v2 issue-6 template remains unchanged; the v3 template keeps its eight ownership/provenance/ledger/streaming invariants while adapting the old guessed-height assertion to actual bounds. Tests do not execute GTA natives or certify collisions, input hardware or save I/O in the Windows runtime.

Manual GTA QA (all NOT RUN): cold launch past the default 30-second gate and script reload; check manager invoice row with zero due, insufficient/exact/sufficient business cash and outstanding mechanic/receptionist/rent/supplier invoices. Pay manager alone, repeat keyboard/controller confirmations, use the Manager Profile action, then Pay All; repeat in reverse order and verify exactly one payroll charge and no self-rebate. Save/reload after payment and confirm due remains zero, cash/payroll persist and unrelated bills/ages remain intact. Pause/unpause, test a closed shop and stream out/back; no paused/offline wage accrual, remote earnings or supplier countdown catch-up is allowed. Test an isolated unwritable data target and confirm payment cancellation. Repeat the repair, one-unit sale and all nine pallet position cases in their focused guides, including old saves, mask holes, partial stacks, four cardinal headings, fallback model bounds and elevated MLO ground. Preserve pending wholesale cargo/DeliveryCost until actual pallet deposit. Use a controlled heavy-mod session after no-GH control, collecting crash evidence without attributing an unknown failure.

## Installation and rollback

This is one review-only source package containing a single .3.cs and a separate project linking it, matching guides/Word/PDF, source diffs and verification logs. Do not copy the whole archive into GTA. Only after later owner authorization, back up the full business data folder outside GTA (Save.ini, Settings.ini and logs), disable all duplicate/v2 Business Manager sources/DLLs, and copy only the intended experimental .3.cs to scripts. Keep installed SHVDN runtime files unchanged. No save, settings or runtime DLL is bundled; the agent has not touched the live game.

Rollback by disabling the final experimental .3.cs and restoring the chosen prior experimental source/project; never enable two Business Manager generations. No manager-wage schema rollback is needed. If reverting all four features, older builds ignore optional pricing/counter keys while existing stock/cash/revenue already reflect committed sales. Preserve saves; deliberately restoring a pre-test backup discards later transactions. No main/QA R2 merge or deployment is authorized. All branches are review candidates, with GitHub draft creation blocked by the API response described in REVIEW_HANDOFF.md.
