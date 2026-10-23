# CODEX HANDOFF — QA R2 SHVDN2 → SHVDN3 migration preparation (Issue #20)

## Authorization, branch, priority

**This is PREPARATION for an experimental, source-level API migration. It does not establish a crash root cause and authorizes NO change to the user's active GTA installation.**

- Source-of-truth QA R2: commit `7693ed2167405f475dd4d36392c5df1f257fe178`, branch `qa/v0.3.3.1-final-review-r2`, draft PR #17.
- Work only on `migration/qa-r2-shvdn3-prep`; submit a **DRAFT PR against QA R2**, NOT main/development. Do not rebase or modify PRs #17 (R2), #18 (P0 crash diagnostics) or #19 (P1 wholesale blip).
- P0 incident #1 / draft PR #18 takes precedence. The owner is now performing a **no-Glass-Heroes controlled GTA session** to determine whether the full mod stack runs for at least one hour; historical sessions before Glass Heroes reportedly ran 2+ hours. Do not assume that removing Glass Heroes restores stability before results arrive.
- Existing `GlassHeroesV3Probe.3.cs` is only a separate probe. It is not a v3 Business Manager. Do not reuse its class name or confuse its logs with those of `GlassHeroesBusiness.2.cs`.

## Actual source audit, verified from GitHub (not guesses)

QA R2 source is approximately 8,050 C# lines / 432,515 characters in `Install/scripts/GlassHeroesBusiness.2.cs`, with byte-matched counterpart `VisualStudio/scripts/GlassHeroesBusiness.2.cs`. `VisualStudio/GlassHeroesBusiness.csproj` specifies:
- `TargetFramework=net48`, `LangVersion=7.3`, assembly `GlassHeroesBusiness`.
- `Compile Include="scripts\\GlassHeroesBusiness.2.cs"`.
- `PackageReference Include="ScriptHookVDotNet2" Version="2.11.6"`.
- Windows Forms and System.Drawing references.

The live setup has GTA V Legacy `1.0.3889.0`, SHVDN Enhanced runtime `3.9.0.6`, `ScriptHookVDotNet2.dll` and `ScriptHookVDotNet3.dll` loaded alongside RAGE Plugin Hook, Los Santos RED, PSRP and many third-party scripts. The logged v2 warning means deprecated API, **not** a proven stack-overflow culprit.

Source audit counts (rough regex occurrence counts, NOT exact call counts):
- 107 lines with `GTA.Native.Function.Call` or `GTA.Native.Hash`.
- 19 lines with `World.Create*`, `World.Draw*`, `World.Get*` or equivalent.
- 80 lines with `Game.Player`, `Game.GameTime`, or `Game.IsPaused`.
- 42 lines mentioning `GTA.Control` or game input-control APIs.
- 17 lines mentioning `Model.Request` or `MarkAsNoLongerNeeded`.
- Source directly uses `System.Windows.Forms`, `System.Drawing`, `GTA`, and `GTA.Math`.
- Key methods requiring explicit migration QA include `OnTick`, `OnKeyUp/OnKeyDown`, `DrawMenu`, `DrawWholesalePackageMarker`, `UpdateWholesalePalletVisual`, `ProcessSupplierDelivery`, `TrySupplierPackageInteraction`, `CreateFixedEquipmentProp`, `SyncCustomerBlip`, `UpdateVisualMechanics`, business-save load/write and script abort cleanup.

**Do not claim these calls are incompatible by default.** For each finding record the exact v2 symbol, v3 replacement (if needed), compiler error/API-doc evidence, behavior change risk and test. Maintain a machine-readable migration checklist where feasible.

## Phase 1 — API contract and build assessment

1. Find the genuine v3 reference aligned with the user's installed Enhanced version and/or officially supported stable SHVDN3 compile reference; record its version and provenance. Do **not** replace `ScriptHookVDotNet.asi`, `ScriptHookVDotNet2.dll`, or `ScriptHookVDotNet3.dll` in the user's GTA root. No upgrade is authorized.
2. Build the existing v2 solution first to confirm the exact clean baseline (the previous R2 release test recorded net48 Release 0 errors / 2 existing warnings, 63 behavior cases, 21 static, 9 diagnostic cases).
3. Make a **separate experimental v3 project/source** targeting `ScriptHookVDotNet3` (for an installable raw source only after it *actually compiles* as v3, use `GlassHeroesBusiness.3.cs`). Keep the v2 file and v2 project unchanged for rollback. Avoid library assembly naming collisions with v2/other scripts; distinguish build artifacts and namespaces if appropriate.
4. Compile and resolve v3 changes in small isolated commits with concrete build transcripts, not global text substitutions. Check `Script.Tick`, key events, `Aborted`, `World`, `Game`, `GTA.UI` interface, entity lifecycle, tasks, vehicle-door/native APIs, drawing, blips, model loading, `ScriptSettings`, `Vector3`, hashes, and startup/reload semantics.
5. Preserve any reason for a direct native `Function.Call`; do not assume v3 introduces an equivalent. Review synchronous waits, nested callbacks, entity access and cleanup for additional stack-reentry risk without asserting guilt. No unbounded allocations/scans or blocking waits during active play.

## Phase 2 — behavior & save parity (no financial drift)

- Same data root `scripts\\GlassHeroesBusiness`, same `Save.ini` and `Settings.ini` semantics, same placement coordinates, inventory, persistent packages, business money/bills, manager discounts, payroll, customer orders and pending wholesale pickups.
- **No progress while GTA is closed or paused**, including remote jobs, supplier ETA, employee pay and business income. Active/unpaused game seconds remain authoritative; do not replace with `DateTime.UtcNow` elapsed across sessions. Wall clock may throttle nonbusiness UI/logs.
- Keep `HoldGameplayTick=false` and `StartupDelaySeconds=30` semantics for the startup gate. Separate safe startup and full feature activation.
- Preserve v0.3.3.1 QA R2 issue #2–#7 behavior: one menu keypress = one action; reliable Back; warehouse ETA→READY, registered vehicle load/unload, physical pallet deposit, invoice **only on stock**; heading arrow placement; direct player handoff; legit delivered packages retained vs legacy visual cleanup; mechanic routing.
- Do not absorb unrelated package-blip changes from draft PR #19 until integration is explicitly approved. A completed v3 port must preserve the *existing* R2 behavior, not silently add or remove features.

## Phase 3 — test matrix / integration gates

**Build/static (Codex):** net48 C#7.3 v3 compile; full transitive dependency check; exact reference versions; 63 original behavior + 21 static + 9 diagnostic cases (or document any justified test changes); value/INI round trips, time pause/close, order stage and finances; no duplicated entity or blip ownership; no v2 class references left in v3 project; install+project source parity, hash and clean ZIP.

**Real GTA — NOT RUN until owner explicitly asks:** 
- Cold start GTA/SHVDN/RAGE Plugin Hook full stack (multiple clean starts).
- Cold start without RAGE/LSPDFR control only if agreed.
- 60–120 minutes full load idle + normal travel; compare no-GH and v2 baselines.
- F2/keyboard/controller, menu navigation, multiple wholesale orders, GPS/pickup/loading/stock/invoice, user settings and save reloading, manager+mechanic behavior, stream-in/out, leave/cancel order and restart.
- Aborted/reload without stale props/entities/blips, zero duplicate Glass Heroes scripts loaded. One variable changed per experiment.
- Fatal stack overflow may bypass managed exception handlers. Healthy v3 run is evidence, not proof of root-cause correction.

**Deliverables:** separate experimental code and .csproj, API migration map, compile/test logs, source diff, rollback guide, SHA256, exact one-script source installation zip, MD/DOCX/PDF developer report where feasible. Mark tests performed vs tests NOT RUN accurately. No copying user's saved business data to public GitHub.

## References (verification date: October 22, 2026)

- Official upstream SHVDN project: https://github.com/scripthookvdotnet/scripthookvdotnet
- Official v2-to-v3 Script Migration Guide: https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Script-Migration-Guide — **mostly a warning about cross-version assembly name collisions; not a method-by-method recipe.**
- Official Getting Started (explicit API suffix and v3 reference): https://github.com/scripthookvdotnet/scripthookvdotnet/wiki/Getting-Started
- Official FAQ: https://scripthookvdotnet.github.io/faq/
- Existing live diagnosis: Issue #1, draft PR #18. Separate package-blip task: Issue #3, draft PR #19.

## Safe rollback & coordination

The owner must finish the no-GH control before any installation experiment. Preserve QA R2 .2.cs and the original save/config in their existing folders; build/test v3 without adding both business versions at once. A future owner-approved test will use **only one** of `GlassHeroesBusiness.2.cs` or `GlassHeroesBusiness.3.cs` (or separately approved one business DLL), never two simultaneously. Explicit user sign-off required before merging to production or development.
