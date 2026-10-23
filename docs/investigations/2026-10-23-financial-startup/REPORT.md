# October 23 financial startup crash — evidence and focused hardening

Status: UNATTRIBUTED native access violation; financial-build regression, save interaction and unrelated intermittent startup crash remain unresolved. User confirms identical runtime/mods and complete restored data conditions are NOT YET VERIFIED. One successful waypoint launch is not a controlled causal comparison.

## Evidence actually examined

Uploaded Crash_2026-10-23_01-35-40.dmp: 7,171,477 bytes; SHA256 ea83f90005453d4694df99077d7fdea1efea4f7ddeb5d63ce98cb840690dfe74. Parsed exception, AMD64 context, module and memory streams directly, then cross-checked exception/module/thread counts with minidump 0.0.24. Its optional PEB parsing failed on missing captured memory; that does not invalidate the independently parsed exception stream. Capstone 5.0.9 decoded captured bytes. This is not WinDbg symbolic unwinding or a managed stack analysis.

Exception 0xC0000005; thread 23412 / 0x5B74; read operation; fault RIP 0x7FF69E482E79, GTA5.exe+0xF62E79. Invalid address 0x20653505708. Captured bytes 48 8B 04 D0 C3 decode as mov rax, qword ptr [rax + rdx*8]; ret. RAX=0x1FE53505710 and RDX=0xFFFFFFFF. Their indexed address equals the reported invalid read exactly. Preceding captured bytes show mov edx, ecx; ECX contains the same 32-bit all-ones value. This supports an out-of-range-looking table index at the fault. The table's semantic purpose, upstream caller and origin of the index are not established.

Dump module table reports GTA5.exe file version 1.0.3889.0, CLR 4.8.9310.0 and SHVDN3 PE file version 3.9.0.0. A PE file version is not proof of the full Enhanced distribution build identity; retain the user's reported Enhanced 3.9.0.6 pending matching runtime files/logs. Multiple mapped module entries are not proof of duplicate active script instances. Header timestamp is 2026-10-23 06:35:41 UTC; user event time 01:35:39 AM is recorded as supplied, with event timezone unverified. Do not silently treat the two as UTC-identical.

Raw stack values resolve to GTA module offsets, including the first value GTA5.exe+0x627024. These are candidate pointers, NOT an unwound stack or proof that no managed/native mod initiated the operation. Matching module images, symbols and Windows debugger analysis are still needed. The dump and full module paths are not committed or included in the package; sanitized evidence JSON contains only selected basenames/version fields and bounded raw pointer observations.

The financial deployed source identity is user-confirmed: 0fefcf8 / SHA256 84125a3465adc717b21984d3e95cdf4c1faed77426eda979a90cf7a3a4917fe5. Constructor completion at 01:35:28 and gate-not-reached are user-reported log facts; raw matching SHVDN/Errors/RPH logs were not uploaded with this dump. Exact source identity for the earlier 00:41:09 CLR stack overflow remains UNVERIFIED. These are separate incidents with different exception codes and modules. No earlier F62E79 dump or stack was available locally for recurrence comparison.

## Financial versus supplier-parent audit

Relative to 422baa7, financial constructor changes only its BOOT label and one LoadFinanceBook(data) call before callback registration. Finance initialization reads saved values, allocates managed snapshots/rows, checks invariants and closes the loaded shift. CaptureFinanceSnapshot and MechanicWagesDue touch managed business fields only. The official 3.6.0 compiled API ScriptSettings.GetValue uses string/dictionary/conversion operations; no GTA native calls are present in the inspected initializer call graph. Row processing is bounded to 256 retained entries. Legacy absence starts an audit opening without writing files or inventing transactions.

Malformed/stale FinanceV1 data raises managed exceptions before callback registration; constructor catch logs/rethrows. It does not run native recovery, overwrite a save or continue gameplay. A journal failure is therefore a concrete compatibility issue to test, not an explanation by itself for this native read violation. If the reported constructor completion belongs to this process, its journal load already returned successfully before the later crash.

Before activation, OnTick returns during HoldGameplayTick/startup delay, then requires a valid unpaused player. Financial OnKeyDown/OnKeyUp/OnAborted and pre-activation Tick gate are unchanged from supplier parent. The new financial active-clock increment, revenue/payment logic and reporting UI are after the activation gate. Callback registration did not add a financial background thread or recurring timer. This does not prove that script size/JIT/loading timing, runtime integration or another mod cannot affect startup indirectly.

## Concrete inherited early-load path and FS1 change

The broader constructor audit found seven Model(string) constructions before the delay. They are inherited by both financial and waypoint builds. Offline IL in the pinned 3.6.0 API shows Model(string) -> Game.GenerateHash -> SHVDN.NativeMemory.GetHashKey. Pinned upstream core source invokes a native GetHashKeyFunc delegate after pinning the string. Thus source-only scanning for GTA.Native.Function.Call missed a native path. The installed Enhanced implementation has not been independently inspected, and this is not a proven connection to the dump's fault.

Focused candidate v0.3.3.3-FS1, branch investigation/shvdn3-financial-startup-audit, starts directly at reviewed financial 0fefcf8. Constructor now retains the same configured model names as strings. TryInitializeGameplayModels runs only after the delay and a valid unpaused player, before activation resets the gameplay clocks. It hashes seven models without requesting assets or spawning entities, publishes model references only after all succeed, and has at most three managed-failure attempts at least five startup seconds apart. It stops inactive after exhaustion, preserving saves. That throttle is not business progression; no loading time becomes income/ETA/wages. Unmanaged access violations are not claimed catchable.

FS1 also adds bounded source SHA256 and financial-read/model-hash begin/end checkpoints. FinanceV1 loader/writer, safe atomic save, transaction IDs, accepted quotes, all economic settlement paths, pending supplier payload and saved settings remain unchanged. No waypoint or Mule integration is added to this focused candidate. Held integration a5c70f2 remains in its separate local branch, with 465/465 automated behaviors and 21/21 static checks, but no integration package/PR or GTA installation request.

This is shared startup hardening and instrumentation, NOT a demonstrated financial-regression fix or stack-overflow fix. FS1 changes execution timing and requires fresh controlled GTA acceptance. Do not claim resolution from compilation or one successful launch.

## Actual verification and gaps

The original hash-confirmed financial source was rebuilt Release and passed 424/424 automated behaviors (406 inherited financial candidate behaviors plus 18 new startup journal tests), 21/21 static checks and financial scope checks before the FS1 source edit. New tests count game access and file writes during production loader execution, covering absent/valid/stale/malformed journals, maximum retained rows and read-only file reload.

FS1 exact results are in its versioned release validation.json, with a genuine net48/C#7.3 Release build against official 3.6.0, inherited tests plus actual gate/initializer tests with model construction counts, injected failure/retry and partial-publication checks. These deterministic stubs are not GTA execution, Enhanced compatibility proof or a native-crash reproduction. Matching-save interaction, Windows sharing/disk behavior and controlled A/B launches remain NOT RUN.

## Controlled acceptance and missing evidence

Close GTA, preserve exact installed source and full data/log backup outside scripts. Use disposable complete copies; never alternate financial/waypoint/FS1 on one modified Save.ini. For a comparison, restore an identical common-parent data backup before each run, preserve identical runtime/mod configuration, record launch conditions, source SHA and full logs, and change only one candidate. An existing modified FinanceV1 save needs a separate matching-backup matrix; do not delete journal keys to force it to load. Repeat each condition rather than comparing single launches. Add a no-Glass-Heroes control if feasible without changing the rest of the stack. Do not intentionally reproduce on the sole save.

FS1 should log source hash -> FinanceV1 read begin/complete -> constructor completed -> (after delay/unpaused valid player) model hashes begin/complete -> gameplay gate passed. No model hashes, saves or economy work should occur while held/delayed/paused. Reload preserves active timestamps, jobs, inventory, liabilities and pending ETAs without offline catch-up.

For further attribution: matching SHVDN, RPH, Errors.log and Crash Analyzer output; complete copied prelaunch Save.ini/Settings.ini and source; actual runtime file hashes/versions; prior F62E79 dumps/stacks; Windows WinDbg !analyze -v, .ecxr, register dump, native kv and module versions using matching GTA/mod binaries. Managed SOS output may help if the relevant thread/runtime state is captured; this small dump has uncaptured memory and cannot guarantee full managed recovery.

No live installation or saved data was accessed. No protected branch was modified or merged. No raw dump has been uploaded to GitHub.

## Subsequent non-blocking owner QA

See [WP1/EUP QA addendum](QA_ADDENDUM_WP1_EUP.md). A later WP1 session with verified source identity crashed while idle after 17m32s with the recurring CLR stack-overflow signature. EUP behavior after recursive Read-only changes is retained as a separate observed file-access symptom. This supersedes any reading of the earlier successful waypoint launch as continuing stability evidence; original session facts and immutable FS1 release artifacts remain preserved. No causal connection or new gameplay validation is claimed.
