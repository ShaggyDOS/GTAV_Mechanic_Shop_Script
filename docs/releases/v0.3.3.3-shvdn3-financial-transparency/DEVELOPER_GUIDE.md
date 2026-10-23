# Glass Heroes Business Manager v0.3.3.3 Financial Transparency — EXPERIMENTAL

October 23, 2026. Isolated feature/shvdn3-financial-transparency branch based on verified 422baa756ce16f6af370278105731de2e82abdd3. This preserves the complete pending supplier range/menu/mixed-order/native-pause candidate. It does not overwrite or merge those feature branches. Waypoint crash diagnostics are a different worktree/branch based on the same preserved candidate and are NOT included here. No live GTA installation, saves/settings, main or QA R2 were changed.

## Investigation: approximately $5,000 sales but only hundreds of new cash

Source findings: _remoteSales is a cumulative lifetime total loaded from Management/RemoteRevenue. The old returning-manager report printed that lifetime value without saying lifetime. _customerRevenue already includes remote jobs; adding remote again inflates a combined report. TrySettleRepair increments _cash and _revenue by the SAME fixed quote.Total in one successful save, then increments both the customer total and remote subset for remote work. Its existing inventory/payment rollback is unchanged. No code path was found there crediting gross revenue while intentionally withholding part of the cash. Unpaid wages/rent/supplier invoices accrue liabilities and do not reduce cash until actual payment.

These findings establish ambiguous period labeling and a reporting double-count risk, not the cause of the observed session discrepancy. There are no before/after user Save.ini snapshots or session cash transactions supplied for that financial observation. Legitimate wage/supplier/equipment/hiring/training payments, owner withdrawals, or comparing a lifetime remote total with a current cash delta can explain it; the exact session remains UNPROVEN. The new returning-manager/profile messages explicitly say LIFETIME gross sales, not current-shift cash.

Regression examples: opening $1,000 + $5,000 remote sales - $0 payments = closing $6,000. Opening $1,000 + $5,000 sales - $4,800 actual payments = closing $1,200 (only $200 cash growth). If those $4,800 bills were already owed at shift opening, paying them is cash outflow but not a second current-shift expense; shift accounting profit can still be $5,000. If incurred during this shift, expense accrual is $4,800 and accounting profit $200. Reports distinguish these cases rather than label cash as profit.

## Implemented menu and reports

Business Management > Finances & Accounting contains Financial Overview, Income & Sales, Expenses & Bills, Profit & Loss, Transaction History and Owner Cash Transfers, plus Reporting Period toggle. Existing management/bill/owner shortcuts remain for compatibility. Nested Back targets return to the accounting hub correctly. Existing bounded measured columns, keyboard/controller arbitration and native pause behavior are retained.

Default period is LIFETIME. Overview/income/P&L/owner pages label LIFETIME, CURRENT SHIFT, LAST SHIFT or NO SHIFT and permit toggling. A new shift opens only after an explicit business opening/save. Shift start/end financial snapshots are persisted. Closing freezes its ending snapshot; later owner transfers do not change that closed report. Reload deliberately leaves the shop closed and retains the last saved shift report; opening creates a new shift. A failed opening save leaves the shop closed, preventing an unrecorded new income clock. No invented historical shifts are created on upgrade.

Income & Sales partitions lifetime source counters exactly once:
- Physical customer repairs = legacy CustomerService/GrossRevenue minus Management/RemoteRevenue.
- Manager remote repairs = Management/RemoteRevenue.
- Autonomous mechanic jobs = Business/Revenue minus legacy customer total minus Business/PartsSalesRevenue.
- Parts retail = Business/PartsSalesRevenue.
Their sum equals existing gross revenue. Retail is not added again on top of the total, and remote is not added again on top of legacy customer revenue. Current-shift columns are differences of the same four sources, not resets of historical values. Legacy source inconsistencies are not silently rewritten.

Overview separates gross revenue, period closing cash, outstanding bills at close and cash reconciliation. Save / Audit State says COMMITTED or UNSAVED when live financial counters differ from the last successful journal checkpoint. Transaction History includes committed snapshots only. Owner deposits/withdrawals are capital movement, excluded from gross revenue and profit. Manager payroll rebates and legacy supplier refunds are separate known other credits, never sales.

## Cash movement, expense recognition and profit policy

Cash bridge: opening balance + cash inflows - cash outflows = closing balance. The breakdown shows sales receipts, actual expense payments, rebates/refunds, owner deposits/withdrawals and explicit unclassified adjustment. A residual is labeled Unclassified adjustment and recorded as such, never silently called sales or hidden by changing cash. Entries include signed amount, category, monotonic transaction identity, active-game timestamp, shift identity and resulting business cash.

Lifetime income and expense counters retain their complete available historical values. Historical cash transfers and cash chronology were never stored, so the LIFETIME cash bridge is explicitly SINCE AUDIT START, anchored at the preserved upgrade balance. Historical owner transfers/rebates/refunds before audit are unknown, not reconstructed as zero factual activity. New owner/credit counters record only activity since audit. Current-shift cash bridge uses its saved opening and closing balances.

Cash Expense Payments shows actual paid parts/payroll/rent/hiring/training/equipment counter changes. Expenses Incurred = paid expenses + change in outstanding liabilities. This means paying an already accrued invoice is NOT a second expense, and an unpaid new wage/supplier invoice is NOT a cash payment. Hiring/training/equipment are recognized immediately under the existing management-accounting policy. Accounting profit = nonoverlapping sales + known rebates/refunds - expenses incurred.

IMPORTANT BASIS: this preserves the existing purchase-expense management P&L: supplies are expensed on receipt/paid-cost history, not valued as an inventory asset and later COGS. This is not a formal inventory-valued/GAAP statement. The basis is visible in P&L notes/descriptions. Existing inventory, purchasing liabilities and cost counters are not revalued or rewritten. Lifetime other-credit history predating this version is incomplete and explicitly described.

## Atomic reporting ledger and payment audit

PrepareFinanceCommit clones the published FinanceBook, observes deltas against its last saved checkpoint and stages rows. WriteFinanceBook writes those rows, state snapshots and metadata inside the SAME existing atomic Save.ini snapshot as business cash, inventory, invoices, mixed-order lines and accepted quote journal. _financeBook is published only after TryCommitBusinessSave succeeds. A failed save consumes no transaction identity and publishes no cash history; retries observe the original checkpoint. Job settlement/retail receipt rollback remains authoritative and unchanged. A failure after a durable snapshot but before later visual work cannot book the same completed job or cart again.

Rows are SAVE-BATCH deltas, not a hook into external bank transactions or a trace of native instruction order. Each cash row has a signed amount and a reconciled running cash balance; noncash accrual rows preserve cash. Multiple categories in one save share active timestamp and commit atomically. Each ID is monotonic across reloads; the loader rejects invalid/missing tails, noncontiguous retained identities, backwards time, invalid categories, malformed snapshots or a broken cash chain. A stale financial checkpoint after older-source changes fails before callbacks/writes, preserving the original save rather than inventing a correction/replaying income.

History is bounded to the LATEST 256 rows, eight per page. Older cash balances roll into HistoryOpeningCash; since-audit CashIn/CashOut aggregates remain and reconcile to the original audit opening. This is an auditable retained window plus lifetime audit aggregates, NOT an unlimited historical archive. Pre-upgrade transaction chronology is unavailable. Export/archive tooling and full accounting inventory valuation are deferred.

TryPayBusinessBills now makes supplier, all-bill, all-mechanic and individual-mechanic settlement rollback-safe: cash, paid counters, each liability, staff wage age/overdue flags, manager savings and rebate metadata restore if saving fails. Existing PayInvoice also restores new rebate metadata on failure. Supplier receipt still books stock/liability exactly once; it does not pay the invoice. Supplier cancellation records a prepaid refund as separate other credit and rolls that metadata back with the existing order/cash rollback. No wage reductions or manager rebates on the manager's own wage invoice are introduced.

Owner transfer updates the supported protagonist wallet and business capital counters; a synchronous failed save restores business cash/counters and attempts wallet rollback. Cross-system limit: the GTA wallet and Save.ini cannot form one filesystem transaction. A hard process crash or another mod changing the wallet between those operations cannot be guaranteed atomic; preserve both game and business backups and review the wallet on a reported rollback error. No claim of cross-game-save exactly-once owner transfer is made. Existing hiring/training/equipment actions retain their legacy immediate state-and-save routines; if a legacy action's save fails, live staged values can remain UNSAVED. The ledger does not falsely publish them before a successful snapshot. Jobs/retail/cart and newly hardened bill settlement retain their tested rollback boundaries.

## Save schema and compatibility

Additive sections: [FinanceV1] Version=1, Count (0..256), NextId, ActiveMs, Shift, ShiftOpen, HistoryOpeningCash, CashIn/CashOut, Deposits/Withdrawals/Rebates/Refunds. [FinanceOpeningV1], [FinanceCheckpointV1], [FinanceShiftStartV1], [FinanceShiftEndV1] store V0..V17: cash, physical/remote/autonomous/retail sales, parts/payroll/rent/hiring/training/equipment paid, wage/rent/supplier liabilities, recorded owner deposit/withdrawal, rebate/refund totals. [FinanceEntryV1.0]..255 store Id, ActiveMs, Category, Amount, CashAfter and Shift. Count is authoritative; unused old INI keys remain preserved but are not replayed. Categories 0..14 represent cash sources/payments/owner/other/adjustment; 15..17 represent wage/rent/supplier accrual only.

Absent Version=0 initializes audit opening/checkpoint from current authoritative financial data with NO invented events, income or catch-up. Existing financial keys, Settings.ini, location/inventory/order/quote schemas and unknown keys remain unchanged. Finance snapshots are plain data, no entity handles. Unsupported or inconsistent finance schema fails closed before Tick registration. Do not delete journal keys to bypass a consistency error. Reverting to an older build that changes money while preserving unknown FinanceV1 keys will make returning to this version detect a stale checkpoint. Restore a matching full source/data backup or seek review.

## Active-game timing and exact changed scope

All new timestamps are accumulated active GTA Game.GameTime milliseconds, persisted without UTC/file-age catch-up. OnTick adds the existing clamped/unpaused activeGameSeconds sample to the audit clock. ProcessEconomy now consumes that SAME active-game sample instead of its former clamped UTC delta, closing a loading/freeze clock discrepancy for wages/autonomous/physical repair work. Remote operations and supplier ETA/route timing already use active game time and remain unchanged. Native pause still yields input/overlay and stops these clocks. Audit reads/writes do not start timers or create income merely by viewing reports.

Executable file: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs, directly linked by net48/C#7.3 v3 project, pinned official SHVDN3 3.6.0. Source.diff and preservation.json list exact changed methods: constructor finance load and unique financial-family boot label, Save finance staging/serialization/publication, OnTick clock plumbing, finance menu/title/value/description/dispatch/Back, reporting helpers/classes, PayInvoice/PayMechanicInvoice/PayAllMechanicWages/ExecuteBusinessExpensesSelection, payroll rebate metadata, supplier refund metadata, owner transfer, shift-open failure handling and lifetime manager report labels. TrySettleRepair, inventory receipt/distribution, supplier routing/range, cart persistence, sales settlement, quote persistence and waypoint editor are protected unchanged. No waypoint diagnostics are included.

```csharp
// Before: remote subset could be interpreted as extra customer sales/current-shift cash.
_customerRevenue += quote.Total; _remoteSales += quote.Total; // remote job, same receipt
// After: reporting partitions, without changing that existing exactly-once settlement.
physical = _customerRevenue - _remoteSales;
autonomous = _revenue - _customerRevenue - _partsSalesRevenue;
// Before: several bill paths cleared liabilities then ignored Save() result.
Save();
// After: stage ledger and bills in one snapshot; rollback on failure; publish only on success.
candidate = PrepareFinanceCommit();
WriteFinanceBook(data, candidate);
if (saved) _financeBook = candidate;
```

Actual compiler/test logs and hashes below describe this exact candidate. Standalone tests compile production reporting/journal/payment members against deterministic world/INI adapters and real temporary files; they do not certify Windows ScriptSettings, GTA wallet persistence or native runtime behavior.

## Installation, testing and rollback — owner only

1. Exit GTA/loaders. Preserve exact installed .3.cs and complete scripts/GlassHeroesBusiness data together (Save.ini, Settings.ini, repair/cart journals and logs). Retain the 422baa7 source/package with its matching pre-finance backup. Do not test on your sole financial save.
2. Extract review ZIP outside GTA; read CURRENT guide/DOCX/PDF, FILES_SHA256.json and package receipt. Copy ONLY experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs into GTA scripts/GlassHeroesBusiness.3.cs. Enable one Business Manager only, never duplicate .2.cs/.3.cs/DLLs. Keep runtime ASI/DLLs and unrelated mods unchanged. No save/settings/runtime files are supplied.
3. Cold launch; confirm one v3 instance and v0.3.3.3 Financial Transparency family log. Existing startup delay/hold behavior remains. Open the business deliberately; inspect preserved legacy cash, totals, inventory, bills and mixed order. Verify audit starts at current balance without new sales/history. Lifetime/current/last shift labels must be clear.
4. On backed-up test data, earn $5,000 remote sales with no payments: cash must rise $5,000. Repeat with intervening wage/supplier payments; reconcile each amount, cash after, owner deposit/withdrawal and resulting cash. Test physical repairs, autonomous jobs and retail separately, then together: four categories sum exactly to gross, remote never added twice.
5. Accrue wages/supplier receipt without paying: cash unchanged, liabilities/expenses increase. Pay opening bills: cash declines, current-shift incurred expense does not double. Verify manager wage payment, staff rebate and supplier prepaid refund as separate credit categories. Exercise all/individual bill paths, insufficient funds and ordinary save failure in disposable QA data only.
6. Submit/receive mixed LOCAL/EXPRESS nearby and offsite; WHOLESALE remains player-only. Stock, liability, quote payments and income/history must commit once; reload before/after settlement and repeat interaction to reject duplicates. Verify repair cost coverage, fixed accepted quotes, nine-box layouts, supplier range, parts sales and pause controls remain.
7. Start/end shift, make owner transfers after closing, reload and reopen. Closed shift does not drift; lifetime retains counters. Pause longer than wages/ETA/job timers, leave GTA closed and reload: zero paused/offline progress, no file-age timestamps. Exercise large loading/game-clock jumps and controller navigation.
8. Review history paging, 256-row retention and opening checkpoint, malformed/stale journal fail-closed behavior on disposable copies, and wallet error feedback. Test 720p/1080p/ultrawide for clipping. Run full heavy-mod session with normal travel/idle for 60–120 minutes; retain exact source hash and logs. GTA/Windows testing of this candidate NOT RUN; crash cause remains unproven.
9. Roll back only with GTA closed, restoring prior source AND complete matching business data as a unit. Do not let the independent waypoint diagnostic candidate mutate a financial save and then return without a matching backup. Do not downgrade pending mixed orders except by completing/cancelling first or restoring matching pre-order data. Never delete repair/cart/finance keys to force startup.

## Reproduction

```text
python verification/finance/verify.py --label financial-transparency --profile finance --suites repair sales pallets wages reload supplier range menu cart pause focus finance
python verification/finance/package.py financial-transparency --parent 422baa756ce16f6af370278105731de2e82abdd3
```

Use .NET SDK 8, Python, python-docx/reportlab/pypdf. Current review package includes source, linked project, current and historical guides, source diff, actual tests/logs and hashes. No live data/runtime libraries are bundled. Clean extraction is independently rebuilt. Draft review/owner QA only; no merge or live deployment.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

406/406 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 25, wages: 22, reload: 41, supplier: 37, range: 40, menu: 13, cart: 32, pause: 9, focus: 8, finance: 33). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game clock sampling and original v2 source/project are unchanged. Supplier route timing consumes that existing active-game sample. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 84125a3465adc717b21984d3e95cdf4c1faed77426eda979a90cf7a3a4917fe5. The complete unified source.diff applies to parent 422baa756ce16f6af370278105731de2e82abdd3 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
