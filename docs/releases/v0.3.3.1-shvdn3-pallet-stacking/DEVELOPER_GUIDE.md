# v0.3.3.1 SHVDN3 parts pallet stacking — EXPERIMENTAL

Owner-requested focused pallet placement change, October 22, 2026 (America/Chicago). Branch feature/shvdn3-pallet-box-stacking is explicitly stacked on manual parts sales 7a7afe7 and repair coverage #27, originating in PR #21. This PR changes placement only; the separate followup for manager supplier procurement is not included. No QA R2/main changes or branch merges.

## Layout and performance

Before: guessed 0.34-meter offsets placed three boxes across with inconsistent row and vertical offsets, ignoring actual model bounds and model-origin offsets. After: BuildPalletBoxLayout obtains a bounds-based layout with up to nine boxes, preferring four on the first layer, four on the second and one centered on top. Partial layers of one/two/three/four are centered within the footprint. If that footprint cannot fit, bounded alternatives try three-in-line, two-in-line or a centered column, including a 90-degree box rotation where it makes the preferred footprint fit. The smallest fallback footprints can produce taller stacks; their visual realism and model support must be reviewed in GTA before use. An oversized, degenerate or nonfinite model that cannot safely fit even one box is rejected rather than spawned outside the pallet.

Model-local min/max dimensions include each model's off-center origin. Box bottoms rest at the pallet model top plus whole lower-box heights, with 1.5 cm pallet edge inset and 1.2 cm horizontal separation. No vertical gap is introduced. Local offsets rotate with the saved pallet heading; an optional 90-degree fitting turn is added to each box heading. World Z uses the actual grounded pallet prop, not an assumed saved ground plane. This is tested bounding-box geometry, not proof of collision-mesh or MLO behavior in GTA. Existing frozen/noncolliding display-prop semantics are retained to avoid physics fighting the visual stack.

StoredPalletLayout reads SHVDN3 Model.Dimensions (tuple Item1/Item2), only after the existing gameplay/proximity/dirty/model-load gates. Valid layouts for all counts 0..9 are cached by pallet/box model hashes with a strict eight-entry bound. No placement geometry or model bounds are recalculated for a completed stack on every Tick. Requests remain nonblocking and use existing owned-model cleanup. Original five bounded visual attempts and stream-out recovery are retained; no wholesale warehouse retry or map-blip code is changed. Rendering failures remain dirty for bounded retry/re-entry. If a lower parcel fails to spawn, HideUnsupportedPalletBoxes hides only the affected owned visuals at/above it so upper parcels are not left floating; their accounting/provenance is retained.

## Save, inventory and delivery invariants

CratesPerPallet remains nine; all four saved pallet coordinates/headings, BoxCount1..4 and DeliveredPackageMask1..4 keep their existing meaning. No keys or defaults were added. Existing legacy boxes remain hidden and still count toward the original capacity rules. Earned parcels are laid out by their visible rank rather than by gaps in legacy mask indices: e.g. mask bit 8 alone draws one centered ground-supported parcel, without fabricating the eight hidden legacy packages. The actual array ownership slot and saved bit stay unchanged.

TrySupplierPackageInteraction computes a safe next transform before the existing CommitSupplierDelivery call. If geometry is not ready, the player keeps the cargo and receives a retry notice; stock, delivery identity and invoice stay untouched. After a successful existing commit, the carried prop is detached/reused at that transform and existing owned boxes are repositioned on the dirty rebuild so a growing partial layer cannot overlap an old centered box. CommitSupplierDelivery, wholesale accounting, cargo flags, supplier prices, invoice logic, sales and wages remain unchanged. Manual parts sales do not remove these historically earned package visuals.

## Exact source changes and before/after

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs changes product code. New PalletBoxLayout stores cached offsets; BuildPalletBoxLayout, StoredPalletLayout, RotatePalletOffset, VisiblePackageCount, TryStoredBoxTransform and HideUnsupportedPalletBoxes implement geometry and failure handling. StoredBoxPosition delegates to the checked transform. UpdateSupplierPalletVisual places/repositions only owned provenance-backed parcels, bounded to nine per pallet. TrySupplierPackageInteraction adds the preflight and reuses its transform. Constructor changes only its experimental build label. See preservation.json and the full source.diff against 7a7afe7.

```csharp
// Before: model-independent guesses.
float localX = ((boxIndex % 3) - 1) * 0.34f;
// After: actual bounds, cached local layout, actual grounded prop origin.
position = RotatePalletOffset(palletGround, layout.Offsets[count][rank], pallet.Heading);
heading = pallet.Heading + layout.HeadingOffset;
```

The original v2 issue-6 test file is unchanged. Its SHVDN3 compatibility template keeps the same eight ownership/provenance/finite-retry/ledger/streaming cases, updating the old guessed-height assertion to model-bound support and visible-rank compaction. Additional geometry/cache/failure cases are separately counted. Test logs identify deterministic model stubs; no actual GTA model dimensions or physical collisions were measured here.

## Installation, QA and rollback

Review only. The source ZIP has one .3.cs and the linked net48 project. Do not install the whole archive. Before any later owner-authorized test, back up the full business data folder including Save.ini and Settings.ini, disable duplicate/v2 Business Manager sources or DLLs, and copy only the intended experimental .3.cs. Do not replace installed SHVDN DLLs. No save/settings file is bundled or edited by the agent.

Manual GTA tests (all NOT RUN): cold launch/reload and stream out past 80 m then back; verify no entity leak and only earned parcels reconstruct. Deposit boxes sequentially through all nine positions and inspect each partial layer, especially the transition 1→2, 3→4, 4→5 and 8→9. Test headings 0/90/180/270 and an arbitrary MLO heading on flat/elevated ground, both supported pallet model variants, and actual collision meshes for overlap, gaps, overhang or visually unreasonable fallback height. Reopen legacy saves with mask holes, confirm visible parcels are supported and no hidden legacy boxes reappear. Interrupt model streaming and confirm no unsupported upper visuals or premature invoice. Compare cash, four inventory buckets, saved coordinates, package count/masks, pending cargo and supplier invoice before/after deposit and save/reload. Confirm retail sales preserve historic delivered visuals. Test keyboard/controller input and pause/offline behavior, then a controlled heavy-mod session after no-GH control. Preserve unknown CLR crash evidence without claiming this placement change fixes it.

Rollback by disabling this source and restoring the manual-sales parent source/project, never enabling both. No schema rollback is needed; existing positions/masks/counts were not changed. Preserve financial saves; do not restore old money or inventory casually. Main/QA R2 and live GTA remain untouched.

## Recorded automated results

Actual net48 / C# 7.3 Release build against ScriptHookVDotNet3 3.6.0: PASS, 0 errors, inherited warnings CS0169 and CS0649. SDK 8.0.426.

169/169 extracted-behavior tests PASS (85 inherited; repair: 32, sales: 29, pallets: 23). 21/21 static checks PASS. Source preservation/scope checks PASS; active-game OnTick clock sampling and original v2 source/project are unchanged. Tests use deterministic world stubs; they do not run GTA.

Source SHA256: bd6bce07b309cea5f1117366b0cd5d92e66ddad8b39038ec7f17e91940375b19. The complete unified source.diff applies to parent 7a7afe7 and reproduces this source exactly.

GTA testing: NOT RUN. Windows execution: NOT RUN. Installed Enhanced 3.9.0.6 compatibility: NOT VERIFIED. Stack-overflow cause: UNATTRIBUTED / UNPROVEN. No installation, merge, protected-branch push, or live save/settings editing was performed.
