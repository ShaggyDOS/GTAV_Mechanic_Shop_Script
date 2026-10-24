# RC2 diagnostic capture and interpretation

Diagnostic-only; GTA testing NOT RUN; crash cause UNATTRIBUTED. Keep the exact RC2 source hash, complete pre-test business-data backup, game build, ScriptHookV/SHVDN runtime versions and mod list with each test. Use an isolated complete copy of business data. Do not alter working routes/AI to chase labels or force a reproduction.

## Files

After the existing startup delay and first eligible operation, the script attempts to create scripts/GlassHeroesBusiness/CustomerVehicle.RC2.log. The previous 128 KiB segment is CustomerVehicle.RC2.log.previous. Copy both promptly after a test or crash and before reloading; they rotate automatically. No new trace file before an eligible operation is normal. Storage failure can suppress the trace while gameplay continues.

Also preserve the existing Glass Heroes Errors.log/source-identity records, ScriptHookVDotNet and ScriptHookV logs, launcher/Windows crash events and any crash/minidump produced by the current host. Preserve pre/post-test complete business-data copies together with those logs. Do not send the only live Save.ini without keeping a backup.

## Record fields

The existing safe writer prepends UTC time. RC2 VEHICLE marks this diagnostic candidate; session changes on a new trace instance. ENTER and EXIT share seq within that session. ENTER names the operation. mechanic=1 means slot 0; mechanic=0 means unassigned. order is the existing order ID; vehicle/worker are existing managed entity handles, which can be reused by GTA and must be correlated with session/order/time. stage is the existing CustomerStage name.

mode is a bit mask: 1 paid, 2 physical vehicle handoff, 4 automatic lot, 8 vehicle teleport, 16 vehicle transfers enabled, 32 reverse-out enabled. walk is existing zero-based current node/count. driveRetries and parkingRetries are existing counters. phase is the mechanic's existing arrival phase (0 none, 1 checkpoint, 2 break-bound, 3 at break, 4 bay-bound, 5 working, 6 returning, 7 blocked). depth is diagnostic scope nesting, not a measured CLR stack depth. suppressed counts candidates dropped by the trace-entry rate limit since its last successful ENTER.

Expected transfer sequence can include assignment/walking, BeginEnterVehicle, StartValetDeparture, BeginVehicleDrive, UpdateVehicleApproach and UpdateVehicleParking. The existing stage names distinguish bay intake from customer pickup return. Original optional reverse, custom/automatic parking, retries and recovery decisions still apply.

## What a breadcrumb can establish

A matching EXIT means the scope's finally reached its logging attempt. It can occur on a normal return, early return or an ordinary propagated exception; it does not certify success. Consult the unchanged Errors.log for exceptions. An unmatched ENTER can indicate interruption, fatal failure, unavailable storage or missing/rotated output. It does not prove that operation caused a crash. There is no per-native trace and no complete event history.

Repeated/increasing depth may provide reentry context but ordinary nested helpers also produce depth greater than one. Stable snapshots are deliberately silent. Rate limiting, coalescing, writer backoff, file rotation and catastrophic process termination can omit relevant records. Absence of a breadcrumb cannot exclude this script or implicate another one. No logger is guaranteed to execute when the CLR has exhausted its stack.

For the next failure, record local time with timezone, elapsed session time, mechanic/order/stage if visible, exact player action and native/custom/automatic drive modes. Retain any host-produced dump. A dump with managed/native stack frames and module/runtime identity is needed to attribute 0xC00000FD; the last visible driving operation alone is insufficient. This package does not configure dump capture or modify Windows/GTA runtime settings automatically.
