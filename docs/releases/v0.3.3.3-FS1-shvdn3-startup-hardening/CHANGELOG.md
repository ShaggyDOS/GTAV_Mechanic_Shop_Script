# v0.3.3.3-FS1 — EXPERIMENTAL / Issue #1 investigation

- Defer seven inherited named Model constructions from constructor to delayed, valid, unpaused activation.
- Bound managed initialization retries to three, at least five seconds apart; publish model fields together.
- Add bounded source SHA256 and finance-load/model-initialization checkpoints.
- Preserve FinanceV1, save/rollback, settings keys, active game clocks and all financial-parent features.
- Add 18 read-only/bounded finance-startup tests and 9 actual gate/model-failure tests.
- Analyze uploaded native dump without attributing cause or publishing the raw dump/private paths.
- Keep waypoint/Mule integration held separately. No proven crash fix; GTA testing NOT RUN.
