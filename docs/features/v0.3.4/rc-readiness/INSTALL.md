# Glass Heroes v0.3.4 RC1 — installation

EXPERIMENTAL — GTA TESTING NOT YET COMPLETED. Owner approved integration and preapproved testing-package preparation. No public release, tag, merge or live installation has been performed.

1. Close GTA and all script hosts. Back up the exact installed Glass Heroes script AND the complete GTA/scripts/GlassHeroesBusiness directory outside all script loading paths. Include Save.ini, Settings.ini, CSVs, profiles and every companion file present; FinanceV1 is stored in Save.ini.
2. Keep the working v0.3.3.6 RC1 and matching complete backup, and the earlier v0.3.3.5 rollback files. Test with a disposable complete business-data copy. Never alternate candidate scripts against the same modified sole Save.ini.
3. Move other Glass Heroes .2.cs, .3.cs or compiled Business Manager DLL versions out of every script loading directory. Keep unrelated scripts untouched. Exactly ONE Glass Heroes business script must load.
4. Copy ONLY Install/scripts/GlassHeroesBusiness.3.cs from this archive into GTA/scripts/GlassHeroesBusiness.3.cs. Do not copy the whole ZIP or VisualStudio/Docs into GTA. Do not overwrite the business-data directory. No Save.ini, Settings.ini or runtime DLL is supplied.
5. Confirm the source SHA256 in SOURCE_SHA256.txt. On Windows: Get-FileHash 'YOUR_GTA_PATH/scripts/GlassHeroesBusiness.3.cs' -Algorithm SHA256.
6. Use GTA V Legacy 1.0.3889.0, SHVDN Enhanced 3.9.0.6 and the owner's LSRED configuration. The script compiles against SHVDN3 3.6.0; actual Enhanced/GTA compatibility is NOT verified by compilation. Preserve the normal startup gate and verify one instance/source hash in the log.
7. Follow Docs/GTA_TEST_CHECKLIST.md. Record source/runtime/mod versions and complete-backup identity. Do not claim stability from one session.

The exact reviewed source retains its v0.3.4 DEV startup label; RC1 is the external package identity. No source relabel or gameplay edit was made for packaging. Use its source hash to identify the candidate.
