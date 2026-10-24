# RC2 Diagnostic Experimental — owner testing installation reference

For later owner-run testing only. No installation was performed by Codex. Diagnostic-only; GTA testing NOT RUN; no claim of a crash fix. Do not copy the complete archive into GTA.

1. Close GTA and every script host. Back up the exact installed script plus the COMPLETE scripts/GlassHeroesBusiness directory outside every script loading path. Include Save.ini, Settings.ini, CSVs, profiles and companion files. Preserve the verified RC1 source and its matching full backup separately.
2. Use a disposable complete data copy for the first RC2 test. Do not alternate source versions against the sole live business save or mix finance/receipt journals from different backup sets.
3. Ensure exactly one Glass Heroes Business Manager script is active. Keep older .2.cs/.3.cs variants and compiled Business Manager DLLs outside all script loading directories. Leave unrelated scripts untouched.
4. Copy only Install/scripts/GlassHeroesBusiness.3.cs from this package to the existing GTA/scripts/GlassHeroesBusiness.3.cs path. No business data/settings file or runtime DLL is included; do not replace the data directory.
5. Verify SHA256 with Get-FileHash on Windows. Expected RC2 source: 3475f93b67d5feb8d90ef30550b8a474642aeb95c77e76e8f0438ac21f30cdc9. Confirm matching SOURCE_SHA256.txt/PACKAGE_MANIFEST.json and source-identity log. RC1's unchanged DEV startup label is not a different candidate; use the hash and RC2 VEHICLE diagnostics to identify this build.
6. Retain the owner's current game/ScriptHookV/SHVDN/mod configuration. The RC1 documentation names GTA V Legacy 1.0.3889.0 and SHVDN Enhanced 3.9.0.6; actual RC2 runtime compatibility is NOT VERIFIED. Compilation uses the official SHVDN3 3.6.0 API and net48/C#7.3. Do not update runtime dependencies or mod settings as an implicit crash fix.
7. Preserve normal startup delay and follow GTA_TEST_CHECKLIST.md. Copy both RC2 diagnostic files and all host/error logs before reloading after a failure. No owner data is changed by package extraction or build; running GTA normally can save legitimate business transactions.
