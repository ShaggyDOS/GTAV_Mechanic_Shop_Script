# v0.3.3.2 SHVDN3 Supplier Workflow — EXPERIMENTAL

Current candidate: feature/shvdn3-native-pause-compatibility, stacked linearly on feature/shvdn3-mixed-supplier-orders, feature/shvdn3-inventory-menu-workflow and feature/shvdn3-nearby-delivery-range. Baseline: manager-deliveries b777c64. No merges or live installation changes.

Implements shared shop physical/virtual distance boundary, six inventory/supplier workflows with measured columns, mixed-item cart with one persisted order/ETA/invoice and native Pause/Start/Esc compatibility. Includes all prior repair coverage, manual sales, wages, nine-box pallet geometry, accepted quote persistence and manager receipts. WHOLESALE remains player-only.

Read the [current developer guide](../../docs/releases/v0.3.3.2-shvdn3-supplier-workflow/DEVELOPER_GUIDE.md), [handoff](../../docs/releases/v0.3.3.2-shvdn3-supplier-workflow/REVIEW_HANDOFF.md), current validation/hashes and Word/PDF. Prior v0.3.3.1 guides are historical evidence; their old range/menu/single-item limitations are superseded by this candidate.

The one installation source is Install/scripts/GlassHeroesBusiness.3.cs, directly linked by the net48/C#7.3 project and official SHVDN3 3.6.0 reference. No runtime DLL/ASI, Save.ini or Settings.ini is bundled. Never enable duplicate/v2+v3 Business Managers. Back up matching source and complete data together. Do not downgrade a pending mixed order; finish/cancel or restore the complete matching backup. Never delete cart/repair journal keys to bypass a consistency error.

Owner reports previous manager-deliveries session smooth and crash-free, offsite/transition receipts working; this is not validation of this new candidate. GTA/Windows tests of v0.3.3.2 NOT RUN; Enhanced runtime binding, native pause display, UI readability and physical paths require owner QA. Crash cause remains UNATTRIBUTED / UNPROVEN.
