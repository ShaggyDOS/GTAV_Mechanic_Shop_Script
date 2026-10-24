Implements #37 on isolated feature/shvdn3-37-placement-parking from verified RC1 parent 75e3623f608a05d1a2b59d12c243dbcdfc061358. Owner pre-approved A–D implementation; final acceptance PENDING YES/NO. Keep draft.

Four focused commits change actual drop-off task behavior to position-only handoff, clarify mechanic physical points versus per-worker masks, add persistent filtered marker modes and cached previews, and implement closed/idle geometry resets/restores with two confirmations, complete verified backups and atomic spatial-only commits. Service-bay/queue facing, walking geometry/masks, financial/stock/receipt bodies, business save/settings schema and active time remain protected. Existing implicit relocation clearing is removed.

Actual net48/C#7.3 Release, SHVDN3 3.6.0: 0 errors/two inherited warnings. 1057/1057 behavior/navigation, 21/21 static, 13/13 structural and preservation PASS. GTA/Windows execution NOT RUN. No crash fix claimed. RC2 diagnostics remain isolated; do not merge these overlapping transfer/UI changes into the active diagnostic branch.

Review docs/features/issue37-placement-parking/DEVELOPER_GUIDE.md, full source.diff, actual logs, Word/PDF, manual GTA and rollback instructions plus exact-tip package handoff. No live deployment, protected-branch merge, public release or v0.3.5 work.
