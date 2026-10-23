Implements #27 on the experimental SHVDN3 foundation (PR #21). Fixed quotes cover the larger of the original service/expertise total and estimated reserved parts plus billable mechanic labor across physical, background and remote work. Inventory reservations follow dedicated stock then universal fallback; completion cannot credit twice or charge procurement/payroll twice.

Actual net48 Release build against ScriptHookVDotNet3 3.6.0 passes (0 errors, 2 inherited warnings); 117/117 extracted-behavior tests and 21/21 static checks pass. Source-scope/preservation checks pass. Full diff, estimate defaults, persistence limits, QA/rollback, source ZIP receipt and Word/PDF report are under docs/releases/v0.3.3.1-shvdn3-repair-coverage.

Review points: estimates do not reconstruct historical acquisition lots; fixed labor excludes idle/walk/stall overhead; accepted runtime customer objects retain the foundation's cancellation-on-reload limitation. No hidden post-work surcharge or false restored-order guarantee. Existing save payload, OnTick, pending wholesale costs and original v2 code are unchanged.

DRAFT / REVIEW PENDING. GTA testing NOT RUN; Enhanced runtime compatibility NOT VERIFIED; stack-overflow cause UNATTRIBUTED. No manager supplier automation, box layout, forensic or map-blip changes. Do not merge into QA R2/main or deploy to the live GTA installation.
