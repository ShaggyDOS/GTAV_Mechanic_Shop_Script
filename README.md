# Glass Heroes Business Manager | v0.3.3.0 RC1 (EXPERIMENTAL)
GTA V Legacy 1.0.3889.0 | ScriptHookVDotNet 2.11.6 | .NET Framework 4.8

**STATUS:** SOURCE ONLY. No in-game compiler or GTA V test available in this environment.
This is a release candidate that MUST pass a full cold-start test before replacing v0.3.2.9a.

## Baseline
Built from exact v0.3.2.9a diagnostic source, sha256 recorded in BASELINE_SHA256.txt.
v0.3.2.9a is the last user-tested baseline: 73 scripts/plugins with LS RED, manager hiring and wholesale delivery operational. Crash-free in this reported test only; repeated cold-start validation still outstanding.

## Install
1. BACK UP scripts/GlassHeroesBusiness.2.cs and scripts/GlassHeroesBusiness/{Settings.ini,Save.ini}.
2. Install ONLY Install/scripts/GlassHeroesBusiness.2.cs into your GTA V scripts folder.
3. Remove prior copies of GlassHeroesBusiness.2.cs and any compiled GlassHeroesBusiness.dll before loading. Do NOT run duplicate versions.
4. Leave existing Save.ini and Settings.ini unchanged. Do NOT copy Settings.ini.example over Settings.ini.
5. Retain [Diagnostics] HoldGameplayTick=false; StartupDelaySeconds=30.
6. Start Story Mode, keep unpaused >=45 seconds, press F2; do not test in GTA Online.
7. If cold-start crash recurs, replace only the .2.cs with your saved v0.3.2.9a file and send logs.

## Implemented changes
- Warehouse box sits atop a locally spawned, ground-placed pallet (visual-only).
- Delivered stockroom boxes reference actual grounded pallet entity position.
- Manager menu/profile mirrors receptionist, manager workstation and approach moved to Employee Placement Settings.
- Manager supports DIRECT navmesh or CUSTOM 0-5 waypoint routing with final approach and bounded retries.
- Point editor enables marker preview after saves, markers raised for visibility; toggle clarifies saved shop/distance prerequisites.
- Supplier recovery and two-click cancellation, no stock credited on recover/cancel; legacy prepaid refund.
- Supplier ETA now counts active, unpaused in-game time only; pending legacy ETA restarts as configured lead time.
- Removed hidden savings mutation from manager training-price display.

## Changed saved keys (additive)
[EmployeeSettings] CustomManagerPaths
[ManagerApproach] Set,X,Y,Z,Heading
[ManagerWaypoint1-5] Set,X,Y,Z,Heading
[Purchasing] DeliveryRemainingGameSeconds

See Docs/ for the complete Word/PDF developer guide, full before-after excerpts, tests and known limitations.
