## Manager-Assisted Supplier Deliveries (#24)

Implements Issue #24 / prior PR #26 scope on the completed SHVDN3 baseline aad2dff, without merging or modifying that branch, main or QA R2.

LOCAL/EXPRESS orders can be received virtually by an employed manager while the business is open and the player is away. Nearby delivery drivers follow the saved route and deposit automatically at an available grounded pallet. Existing direct player collection remains available. WHOLESALE, including legacy prepaid orders, stays player-only.

Supplier route deadlines and bounded retries now use the existing active, unpaused game-time sample. Receipt rechecks capacity and physical order identity; stock, invoice, package provenance and cleared order share the existing atomic save boundary, with memory rollback on save failure. Missing entities, blocked routes, storage loss and policy/streaming changes retain the original order.

Preserves repair coverage #27, manual sales #25, nine-box stacking, wages, accepted quote persistence, save/settings schema and original v2 code. No live GTA installation changes.

Validation: net48/C#7.3 Release against SHVDN3 3.6.0; 0 errors/two inherited warnings; 271/271 behavior cases (234 inherited + 37 supplier), 21/21 static checks and preservation guards; hashed review ZIP and clean extraction rebuild. See docs/releases/v0.3.3.1-shvdn3-manager-deliveries/ for exact logs, full source diff, current guide/DOCX/PDF and package receipt.

GTA/Windows testing NOT RUN. Native routing/collision and installed Enhanced runtime compatibility require owner QA. Crash cause remains unproven. Keep Draft; no merge/deployment authorization.

Closes #24
Related scope: #26. This is a new isolated implementation PR; the scope-only PR is not claimed updated.
