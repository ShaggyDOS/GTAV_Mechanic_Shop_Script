# Non-blocking QA addendum — October 23, 2026

Owner instruction: preserve current implementation and validation scope; add findings to review notes/backlog. No new branch or release solely for this update. FS1 b1b409b and its review ZIP remain unchanged artifacts; this addendum does not certify a new build. The pasted handoff's proposed next integration is recorded below, not used to override the accompanying non-blocking instruction.

## Evidence received and provenance

New evidence is the owner's message and uploaded Pasted text.txt, read in full. No new raw WP1 Errors.log, SHVDN, RPH, Crash Analyzer log or WP1 dump was attached with this update. Statements about those logs below are owner-reported, not independently parsed in this addendum. The financial startup dump previously parsed remains separate evidence.

### Gameplay stack overflow — verified WP1 source

- Candidate v0.3.3.2-WP1, branch investigation/shvdn3-waypoint-editor-diagnostics, commit 7deff80.
- Startup source SHA256 owner reports independently verified: 50bf63c16ec06fb9300dbc13246b3dda009411398ff812654a8d5163341702be.
- October 23, 2026, 01:56:07 AM as reported; event timezone remains unverified. Duration 17 minutes 32 seconds.
- Player standing idle, no active waypoint editing. Exception 0xC00000FD STACK_OVERFLOW at clr.dll+0x688D5.
- Owner reports successful initialization, gameplay activation and physical shop operations; preceding activity included customer/mechanic/valet navigation recoveries, parking failures, customer cancellations, and no immediately preceding waypoint-editor action.

This is the recurring CLR gameplay crash family. WP1 loaded successfully in this session but did not eliminate that failure. Idle activity broadens investigation beyond active editor actions; it does not prove editor guards are wrong, identify a faulting script, or establish a route-recovery message as the cause. Raw matching stacks/logs are needed for attribution. This verified identity applies to this WP1 session, not retroactively to the earlier 00:41:09 session whose deployed source remains UNVERIFIED.

### Native startup access violation — separate incident

Financial v0.3.3.3 / 0fefcf8, verified deployed source SHA256 84125a3465adc717b21984d3e95cdf4c1faed77426eda979a90cf7a3a4917fe5; October 23 at 01:35:39 AM as reported; 0xC0000005 at GTA5.exe+0xF62E79 before gameplay activation. Previously uploaded dump was parsed; the originating component remains UNATTRIBUTED. Prior recurrence of this native offset is owner-reported; earlier matching dumps/stacks have not been supplied for direct comparison.

FS1 defers an inherited model-construction path and adds diagnostics. It has not passed owner-side GTA startup testing and is not a proven fix for this access violation. Identical runtime/mod/complete-restored-save conditions across prior financial/WP1 runs remain NOT YET VERIFIED. WP1's later stack overflow does not establish the startup crash's cause or connect the two exceptions.

### Windows Read-only / EUP behavior — separate file-access investigation

Owner repeatedly observes EUP Menu functioning incorrectly when Read-only is enabled on the GTA installation folder. Clearing Read-only and applying recursively to folder/subfolders/files before launch restores EUP function for that session; the symptom returns after restarting GTA and the procedure must be repeated.

Preserve this functional observation. The folder checkbox alone does not identify individual changed attributes, but its UI semantics are not grounds to dismiss a reproducible functional difference. No actual file attributes, ACLs, write-failure trace, affected EUP dependency files or restoring process have been inspected in this workspace. No relationship to either crash is established.

Controlled follow-up, when appropriate and with read-only collection first:

1. Capture individual file Read-only attributes and NTFS ACLs for EUP configuration/dependency files and Glass Heroes Save.ini/Settings.ini/data-directory entries before recursive change, after change, after launch and after exit. Record exact paths, user identity/elevation, timestamps and backup identity. Compare child files as well as directories.
2. Capture EUP/dependency write errors and targeted filesystem events. Distinguish ACCESS DENIED, sharing violations, path errors and other failures. Identify a process restoring an attribute only from an observed event; do not guess antivirus, launcher, sync or another mod.
3. Compare the same backed-up configuration and runtime/mod setup while varying only the specific measured attribute/permission condition. Retain logs showing the EUP behavior, not just checkbox state.
4. Evaluate Glass Heroes separately. Its atomic save copies the prior snapshot to a same-directory temporary file and replaces the target. Read-only files, parent-directory create/delete/rename permissions, sharing locks or policy can affect different steps. A copied Read-only attribute might affect the temporary-file writer as well as replacement; this is a hypothesis requiring Windows observation, not a demonstrated cause here.
5. Check existing saving errors, financial checkpoint COMMITTED/UNSAVED feedback and preserved original snapshot after a failed write. Existing Linux temporary-file tests cover injected writer failures and rollback, not Windows NTFS attributes/ACL/antivirus behavior. Use disposable complete data copies for fault testing, never the sole save.

Do not strip attributes recursively, rewrite permissions, change elevation, add automatic retry loops or alter the GTA installation merely from this report. No live installation or save was accessed or modified. Read-only/EUP behavior is not blamed for the CLR stack overflow.

## Implementation continuity and subsequent integration backlog

Preserve financial candidate 0fefcf8, waypoint candidate 7deff80, FS1 implementation 65a25cf / packaged head b1b409b, and held local integration a5c70f2. Do not create duplicate integration work. These have different scopes and GTA validation status.

The existing integration milestone remains to review a5c70f2 and incorporate FS1 deliberately into an isolated candidate alongside FinanceV1, WP1 guards/logging and bounded Mule/receipt corrections. Resolve constructor/source-hash logging overlap without duplicating work or altering accounting/active-clock invariants. The posted QA update itself does not authorize an immediate additional release or change current implementation scope. Integration GTA safety is not established; FS1 startup acceptance is pending and must remain explicit in the eventual handoff.

Keep supplier lifecycle work separately tracked: repeated exit/walk/handoff defer/restart cycles, task ownership, real rear/pallet arrival, bounded owned-entity cleanup and retry, persistent pending order identity, exactly-once mixed receipt, and stale notification versus saved receipt distinction. The observed +5 stock / +$562 invoice delta is consistent with one receipt but does not establish historical order identity or exclude inconsistency without matching saved snapshots.

Future integrated validation retains genuine net48/C#7.3 compilation, inherited/combined regressions, static/source preservation, temporary-file interruption/reload, stale/malformed FinanceV1, paused/offline zero progress, exactly-once money/inventory, controller/native pause and bounded native work. No additional gameplay survival result follows from those tests. Source/package hashes, versioned guides and explicit GTA acceptance status are required for any later release.

Tracking remains separate until evidence connects it:

| Track | Current status |
| --- | --- |
| CLR gameplay stack overflow | WP1 source verified for new idle crash; cause UNATTRIBUTED |
| Financial native startup violation | Dump examined; cause UNATTRIBUTED; FS1 GTA startup pending |
| EUP/file-access behavior | Repeated owner observation; individual attributes/ACL/errors unmeasured |
| Mule supplier lifecycle/accounting | Held corrective integration exists; real GTA acceptance pending |

No protected branch change, merge, installation or sole-save alteration. No extra release or test rerun is warranted by this documentation-only addendum.
