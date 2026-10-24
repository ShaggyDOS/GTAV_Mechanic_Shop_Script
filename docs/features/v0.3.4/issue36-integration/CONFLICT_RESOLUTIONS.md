# Reviewed conflict resolutions

Only approved additive maintenance behavior was combined. No architecture change, order policy change, pricing change or older-source replacement was needed.

## Maintenance A / B

Footer layout and mechanic terminology/preview cherry-picked without Git conflicts. Auto-merges were subsequently audited against both pinned source parents, including menu arms and unchanged routes/masks.

## Maintenance C — four product-source conflict hunks

1. GetMenuItems / ActiveDeliveries: retain v0.3.4 Order / Last Receipt; append accepted Set Warehouse GPS at index 4, move Back to 5. Recovery/cancel remain indices 2/3. Supplier review stays a separate page.
2. GetMenuItemValue / ActiveDeliveries: preserve v0.3.4's live-state/ETA row; add maintenance GPS readiness at index 4.
3. GetSelectedDescription: retain v0.3.4 cart review/confirmation help, SupplierOrderReview help and last-outcome explanation; add accepted GPS footer before Back. No outdated immediate-submit wording restored.
4. TrySupplierPackageInteraction / wholesale loading: retain TryCommitWholesaleCargo(true) and captured identity. Delete the owned locator only after durable cargo checkpoint success, before the existing pose cleanup. Do not restore the older unchecked Save/flag sequence.

Machine-readable before/maintenance/resolved snippets: CONFLICT_RESOLUTIONS.json. All other locator cleanup hooks were auto-merged and verified to leave the v0.3.4 transaction body unchanged after removing locator-only lines.

## Maintenance D — one test-source conflict hunk

The navigation harness's extracted dispatch list was combined as a union: retain v0.3.4 BuildSupplierOrder/SupplierOrderReview coverage and add maintenance canonical receptionist/manager entry points. Neither assertion set was discarded. The production canonical-role change merged without a Git conflict.

## Test-support adaptation

Procurement/receipt adapters gained a no-op locator cleanup dependency for the production order-placement helper. Locator tests now extract the real v0.3.4 SupplierLiveState/SupplierReceiptBlocker and declare their deterministic dependencies. Existing original assertions/counts were preserved. The combined validator explicitly selects Issue36 suites and a two-parent preservation audit, instead of weakening the old preintegration-only audit.

## Preservation evidence

The audit compares RC1 c8617ae, v0.3.4 c77591b, approved maintenance 2f1ebc2 and the resulting source. Added maintenance helpers match the approved source exactly; non-overlap maintenance changes match exactly; 396 v0.3.4 members remain byte-identical. It checks each overlapping menu arm, mandatory receipt identity, durable-load-before-blip-cleanup order, all GetValue/SetValue expressions/defaults, root fields/defaults and original v2 hashes. Active-time sampling is identical after removing the one accepted throttled locator-sync call.
