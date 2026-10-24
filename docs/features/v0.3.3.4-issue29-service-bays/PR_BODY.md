Implements #29 for review; GTA acceptance remains pending.

With Mechanic Vehicle Transfers OFF, customer cars previously stayed at drop-off while repairs ran. This adds a session-only wait/one-shot path to the assigned saved CUSTOM bay (or existing AUTO LOT target), uses the existing mechanic animation/payment workflow, then places the paid car at pickup when existing ownership and clearance checks permit it. ON driving helpers remain unchanged.

Occupant/entity/destination checks and five-game-second retry eligibility preserve blocked orders without per-frame movement or scans. Accepted quotes save before movement; partial placement/navigation failures cannot repeat completed movement or payment. Existing car/work coordinates and all save/settings keys remain unchanged. Service Bay profiles group the two existing editors and explain the preserved mechanic roster-slot assignment.

Base: FS1 12fbbf7. Target integration/shvdn3-v0.3.3.4 is an unchanged fork of that baseline, keeping the Issue #28 investigation branch separate. Financial, mixed-cart, wages, pallets and saved quotes are retained. Independent waypoint/Mule candidate remains preserved but outside this issue.

Actual validation and source hash: docs/features/v0.3.3.4-issue29-service-bays/validation.json. Genuine net48/C#7.3 Release, inherited and focused production-member tests, static and preservation checks. GTA/Enhanced runtime validation NOT RUN. Versioned guide/DOCX/PDF, full diff, limitations and owner checklist are included. No release, install, protected-branch modification or merge. This does not claim to fix Issue #28.
