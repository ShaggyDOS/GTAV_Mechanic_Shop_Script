# Issue #36 integration handoff — PENDING OWNER ACCEPTANCE

Exact branch: maintenance/v0.3.3.6-issue36

Base: c8617aed5cda492214bbbe1907042bcafbfa0dc1

Source/four-fix tip: e4860da7accc386ab4b0d36c9c85b0cc4c55f7b7

Final branch tip including documents: see FINAL_HANDOFF.md/.json in the review package. The external receipt is written after the final documentation commit so its SHA is exact.

4ad2fc4c344bafff0c098ef17ce06ff2145affb8 fix(v0.3.3.6): fit and cache readable menu footer lines (#36 A)
15467753fc4a7d376e10d57e1d9ab649e5e6a7aa fix(v0.3.3.6): distinguish shared mechanic stops from route selections (#36 B)
21d865c50493bbc233baed6135ffe18f241bbed1 fix(v0.3.3.6): own ready wholesale map locator and explicit GPS (#36 C)
e4860da7accc386ab4b0d36c9c85b0cc4c55f7b7 fix(v0.3.3.6): share canonical staff route editors (#36 D)

Product source: experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs

SHA256: 7ba7b6021eead8f1e972b92a382a477045df083ea76b0148b84124e8725ef2ca

Complete changed-file list, per-commit changed files, final documentation SHA and build results accompany the exact-tip final handoff. source.diff is the complete product delta; SOURCE_EXCERPTS.md and COMMIT_A/B/C/D.md explain each fix. Final evidence: D-canonical-roles/. 854/854 behavior/navigation, 21/21 static, preservation PASS, actual net48 Release 0 errors/two old warnings. GTA and Windows execution NOT RUN. No crash attribution.

Save compatibility: zero GetValue/SetValue expression or schema/key/default changes; no Save.ini/Settings.ini read or modified; no finance/clock/AI/coordinate/mask change. Session-only footer cache and owned map locator. Preserve exact source plus full matching data backups for any future test/rollback.

Gate 1 YES. Gate 2 PENDING owner YES/NO. Do not integrate or accept until gate 2 YES. The separate v0.3.4 instance owns the later port/review and must handle shared source/menu overlaps deliberately; do not replace its source wholesale. This instance performs no v0.3.4 merge or modification. Original RC1, main, QA R2 and live game untouched. Review-only ZIP is not a published release.
