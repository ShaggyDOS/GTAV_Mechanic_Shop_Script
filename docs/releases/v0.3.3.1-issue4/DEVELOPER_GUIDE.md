# v0.3.3.1 issue #4 — parking heading and front arrow — EXPERIMENTAL

Issue #4; fix/4-parking-heading-markers to development/v0.3.3.1. Review PENDING; GTA testing NOT RUN; stack-overflow cause UNPROVEN. Diagnostic PR #9 is separate and unchanged.

## Changelog, changed methods, before/after and coordinate evidence

Matching VisualStudio/scripts/GlassHeroesBusiness.2.cs and Install/scripts/GlassHeroesBusiness.2.cs: new ParkingOutlineVertices computes the car rectangle and forward arrow; MarkParkingSquare emits four outline edges plus two arrow arms. DrawPlacementMarkers, saved heading, toggles and draw distance are unchanged. One seven-vector array replaces the prior vector and two float arrays; two extra line draws make the front unambiguous.

Before:
```csharp
float x = length[i] * sin + width[i] * cos;
float y = length[i] * cos - width[i] * sin;
```
After:
```csharp
position.X - length * sin + width * cos
position.Y + length * cos + width * sin
```

GTA heading 0 faces +Y/north; 90 faces -X/west; 180 faces -Y/south; 270 faces +X/east. The installed SHVDN2 XML documents RelativeFront=(0,1,0), North=(0,1,0), West=(-1,0,0). Vendor v2 source defines ToHeading as (atan2(X,-Y)+PI)*180/PI:
https://github.com/scripthookvdotnet/scripthookvdotnet/blob/main/source/scripting_v2/GTA.Math/Vector3.cs
The corrected forward axis (-sin,cos) is its inverse. Tests use cardinal expectations and the vendor inverse at diagonal/wrapped headings. The prior mirrored sign error is visible at diagonal headings; a symmetric rectangle alone cannot identify its front. Full exact source.diff is included.

## Save/config, install and rollback

No Save.ini/Settings.ini key, default, coordinate or stored heading is changed. No economy, inventory, job, order, payment, timer or migration changes. No additional world entity is created; this is draw geometry only. Native calls remain behind the existing preview controls and distance bounds.

Install only Install/scripts/GlassHeroesBusiness.2.cs after backing up the prior source outside the active scripts folder. Preserve existing Save.ini/Settings.ini; packaged example settings are reference only. Never activate both source copies, historical RC1 or a compiled business DLL with it. Roll back after exiting GTA by restoring the previous source; no save reset or migration. Last user-tested rollback is v0.3.2.9a diagnostic.

## Automated scope and pending GTA regression

Tests execute actual ParkingOutlineVertices and MarkParkingSquare against a captured DRAW_LINE stub. Cardinal/diagonal geometry, length/width, floor lift, forward arrow, exact draw count and unchanged saved placement are checked. Native rendering quality and game cameras are NOT validated. Actual SHVDN2 compilation is separate.

Repeat activated-SDK checks: python3 verification/run_issue_tests.py 4; python3 verification/run_static_checks.py /tmp/gh-static. Full compilation/results are recorded beside this guide.

GTA NOT RUN: three cold launches and boot gate; save a bay facing north/west/south/east and diagonally, compare arrow with player and parked vehicle forward axis; verify 5.1m long by 2.36m wide outline; toggle previews and move beyond draw distance; save/reload and recheck identical heading; exercise auto-lot and customer bay previews; repeat with LS RED/heavy-mod load, shop streaming, >30m idle and pending-order saves. No immediate user test requested; no stability/crash fix claim.

## Recorded automated results

Real Release build: PASS. SDK 8.0.426; net48 / C# 7.3 / SHVDN2 2.11.6. Two existing production warnings: CS0649 and CS0169.

Existing static checks: 21/21 PASS. Issue #4: 7/7 behavioral tests PASS.

Install and Visual Studio sources have equal SHA256: 3069376cee1416d2af02e24e9f78c1bd66ce08a0f7c7dfa1f6aee8bed8cd8a2a.

Full source.diff applies to the recorded integration baseline and reproduces both sources byte-for-byte. Build, static and behavior transcripts plus validation.json are included. The static runner compiled=false field describes its static-only run; actual compilation is separately verified. Test harnesses use extracted production members with GTA APIs stubbed, not real game sessions. GTA testing: NOT RUN. Windows build: NOT RUN. Stack-overflow cause: UNPROVEN. Review: PENDING.
