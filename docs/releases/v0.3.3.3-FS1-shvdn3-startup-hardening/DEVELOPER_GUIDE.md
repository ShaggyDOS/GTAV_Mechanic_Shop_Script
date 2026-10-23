# Glass Heroes v0.3.3.3-FS1 — Focused Financial Startup Hardening — EXPERIMENTAL

GTA testing NOT RUN. Neither the October 23 GTA5 access violation nor the earlier CLR stack overflow is attributed or proven fixed. See the included docs/investigations/2026-10-23-financial-startup/REPORT.md and sanitized dump-evidence.json for evidence, limits and controlled A/B instructions.

## Version, scope and implemented change

Branch investigation/shvdn3-financial-startup-audit, parent financial 0fefcf8798da54b53fae505f488d3b3d41fae985. Issue #1 crash investigation; no new financial-reporting, mixed-order, waypoint-editor or Mule feature integration. Preserve independent waypoint 7deff80 and all financial features. The combined local candidate is held for startup investigation, not offered as an integrated release.

Changed product file: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs. Existing changed methods: GlassHeroesBusinessV3Experimental constructor and OnTick (one early activation call only). New helpers: TryInitializeGameplayModels and LogStartupSourceIdentity. Full source.diff and preservation.json document the exact source delta.

The constructor previously constructed seven named GTA.Model objects during world loading. Offline IL from the official pinned 3.6.0 compile API resolves Model(string) -> Game.GenerateHash -> SHVDN.NativeMemory.GetHashKey; pinned upstream core uses a native function delegate. This inherited path exists in both reviewed candidates and is not proof of a financial regression. The deployed Enhanced implementation has not been independently inspected.

FS1 stores the same configured names as strings in the constructor. After the startup delay and validation of an unpaused player, the one-time initializer constructs seven models before enabling gameplay. It requests no models, spawns no entities, makes no scans and publishes all model fields together after successful construction. Managed failures retry at most three times, separated by five startup seconds. Exhaustion leaves business inactive; a reload can retry. No saved business data is reset. No unmanaged access violation is claimed catchable.

Startup logging adds one source-file SHA256 read (conventional scripts/GlassHeroesBusiness.3.cs path, at most 2 MiB) plus financial read begin/complete and model-hash begin/complete/failure checkpoints. Missing/unreadable/oversized source reports UNVERIFIED. Unique BOOT label is v0.3.3.3-FS1 SHVDN3 FINANCIAL STARTUP HARDENING EXPERIMENTAL. This hash identifies that conventional file, not the earlier unknown deployed source. Logging is at bounded transitions, never every Tick.

## Before and after

Before, in constructor:
```csharp
_muleModel = new Model(_muleModelName);
_supplierDriverModel = new Model(_configuration.GetValue("Suppliers", "DeliveryDriverModel", "s_m_m_trucker_01"));
// Five other named models were constructed before callback registration.
```
After, constructor retains configured names; early OnTick activation:
```csharp
if (readyPlayer == null || !readyPlayer.Exists() || Game.IsPaused) return;
if (!TryInitializeGameplayModels(DateTime.UtcNow)) return;
_lastGameTimeSample = Game.GameTime;
```
The initializer uses local model references and publishes them only after all seven succeed. Source.diff applies to 0fefcf8 and reproduces the packaged source. No journal/payment code was rewritten.

## Financial/save preservation and timing

FinanceV1 loader/writer and snapshots, transaction identities, active timestamps, cash bridge, safe replacement/rollback, remote revenue partition and all settlement code are byte-for-byte unchanged from the financial parent. Retained journal rows remain capped at 256. Legacy absence creates a managed opening snapshot without writing files. Malformed or stale journals fail closed before callbacks; no unsafe native recovery is added. Current data from another source must not be made loadable by deleting journal keys.

No new Save.ini or Settings.ini keys. Same model setting keys, defaults and values; initialization timing changes only. HoldGameplayTick=false and StartupDelaySeconds=30 remain. All economic/gameplay clock sampling after activation is unchanged. Activation resets clocks, so paused/loading/closed time adds no supplier ETA, wages, remote work or income. Five-second initialization retry uses startup wall time only and cannot accrue business progress. Do not mistake a diagnostic UTC timestamp for an income clock.

Repair cost coverage (#27), manual parts sales (#25), manager wages, mixed purchase orders, player-only wholesale, nine-box stacking, fixed accepted quotes, menu navigation and native pause compatibility remain as in the financial parent. Independent waypoint instrumentation and new Mule/receipt integration are held on another branch and are not included here. Existing limitations of purchase-basis profit, legacy chronology, wallet versus business-file hard-crash atomicity and old unchecked purchase-save callers remain documented in the financial guide.

## Exact evidence and limits

User confirms financial deployed source SHA256 84125a3465adc717b21984d3e95cdf4c1faed77426eda979a90cf7a3a4917fe5 for the loading crash. Uploaded dump SHA256 ea83f90005453d4694df99077d7fdea1efea4f7ddeb5d63ce98cb840690dfe74 confirms a read violation at GTA5.exe+0xF62E79 and table indexing with RDX=0xFFFFFFFF. It does not establish which component supplied the index. Raw stack pointers are not an unwound native/managed stack. No matching raw SHVDN/RPH/Errors logs, complete prelaunch save, prior F62E79 dump, matching symbols or full runtime binary analysis are available. The raw dump and private full module paths are excluded from Git and this package.

Financial constructor completion and absence of gameplay-gate entry are currently user-reported facts. One waypoint launch succeeded; identical backed-up save/mod/runtime conditions are NOT YET VERIFIED. Causality remains unresolved. October 23 00:41:09 CLR stack overflow is a separate incident with deployed identity UNVERIFIED. FS1 is focused shared-path hardening, not a claimed fix for either crash.

Automated tests use real production member extraction and deterministic GTA/INI/native adapters, including actual atomic filesystem replacement in temporary directories. They do not execute GTA or its native delegate. Original financial candidate passed a new 424-test audit before source modification; the recorded results below apply to FS1. Prior candidate logs remain versioned evidence, not certification of this candidate.

## Precise owner installation and controlled acceptance

1. With GTA CLOSED, back up exact source and the COMPLETE matching scripts/GlassHeroesBusiness data/log directory, including Save.ini, Settings.ini and recovery files, outside the scripts directory. Verify the backup. Use disposable data copies, preserving the sole original save. Do not alternate independent financial/waypoint builds on a modified save without restoring the complete matching backup.
2. Extract ZIP outside GTA, verify FILES_SHA256.json and read the report. If choosing to test, copy only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs to GTA/scripts/GlassHeroesBusiness.3.cs. Keep one Business Manager instance; move other sources/compiled copies outside scripts. Do not copy the whole ZIP, test DLLs, dependencies, saved data or settings into GTA. No runtime change is supplied or authorized by the package.
3. Record source SHA256, exact runtime/mod file versions/hashes, backup identity and launch conditions. Keep the same runtime/mod conditions between runs. Restore the same complete common-parent backup for each comparison of parent 422baa7, financial 0fefcf8, waypoint 7deff80 and FS1; use a separate matched finance-journal backup matrix. Verify the waypoint installed SHA, since its provided value is an expected release hash. Do not infer causality from one run; record repeated cold launches and a no-Glass-Heroes control where practical.
4. Cold launch FS1: expect source hash, FinanceV1 read begin/complete, constructor completed. Before the configured delay, no gameplay model hash begin or gameplay gate should appear. After the delay, while player is valid and unpaused, expect model hashes begin/complete, then gameplay gate passed. Inspect one active script and preserved business values. Invalid custom models retain existing later validation behavior; no new defaults silently replace them.
5. Test a copied Settings.ini with HoldGameplayTick=true in a disposable QA setup: no model hashes, saves or business progression. Restore the matching settings before other comparisons. Pause during loading/activation and verify model initialization waits for resume. Never inject failures into the sole live save; automated model-failure tests already cover retries/partial publication.
6. Verify income/ETA/wages and active timestamp remain frozen paused/offline. Resume/open business deliberately. Check remote $5,000/no outflows and with documented payments, bills accrued versus paid, parts sales, manager wages, accepted repair reload and one mixed receipt. Save/reload must preserve IDs/stock/liabilities without duplicate payment or offline catch-up.
7. Repeat script reload, shop distance/streaming, controller/keyboard/pause and 45–60 minute heavy-mod sessions on disposable matching copies. FS1 has not been tested with GTA or Enhanced 3.9.0.6 by Codex. A crash should preserve exact source, copied complete data, dump and matching SHVDN/RPH/Errors/Analyzer logs. No stability claim follows from a clean build or one successful session.
8. Roll back with GTA CLOSED by restoring the exact prior source AND complete matching data/settings backup together. Never delete journal keys or reuse a modified journal with an older independent build.

## Still awaiting evidence/testing

Actual GTA startup improvement or crash reproduction, installed Enhanced implementation of the hash path, controlled A/B comparison, prior F62E79 recurrence correlation, symbolic native/managed stack attribution and Windows runtime behavior. Combined financial/waypoint/Mule release remains on hold. No live installation, protected-branch change or merge was performed.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

433/433 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 25, wages: 22, reload: 41, supplier: 37, range: 40, menu: 13, cart: 32, pause: 9, focus: 8, finance: 33, startup: 18, hardening: 9). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game clock sampling and original v2 source/project are unchanged. Supplier route timing consumes that existing active-game sample. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: 0b14e8f9402007912b8316ecfc345a7ff9043ff866072d0429f00cc6ee76109b. The complete unified source.diff applies to parent 0fefcf8798da54b53fae505f488d3b3d41fae985 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
