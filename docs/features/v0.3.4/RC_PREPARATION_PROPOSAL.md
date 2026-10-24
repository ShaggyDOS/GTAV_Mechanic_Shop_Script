# Proposed later RC contents — no package prepared

PR readiness does not authorize an RC package, tag, release or installation. After review and separate owner approval, propose an experimental source-only install layout:

- Install/scripts/GlassHeroesBusiness.3.cs (exact approved reviewed source; one copy only).
- VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj referencing ../Install/scripts/GlassHeroesBusiness.3.cs, matching current project.
- Docs: install/rollback instructions, complete business-directory backup procedure, changelog, developer guide, versioned Word/PDF report and detailed GTA checklist.
- MANIFEST.json and SHA256 checksums for exact source/project/document contents; retain source/commit identity and actual validation logs.

No duplicated .2.cs/.3.cs script versions, compiled Business Manager DLL, SHVDN reference DLLs, unrelated builds or owner Save.ini/Settings.ini. Do not bundle runtime dependencies unnecessarily. Do not edit gameplay just to package.

Before presenting a package: verify approved commit/hash and no newer unreviewed changes; rebuild Release, run required regression checks, verify archive contents/hashes, extract cleanly and rebuild from the packaged project. Keep working RC1 and a matching complete business-data backup available. Mark EXPERIMENTAL — GTA TESTING NOT COMPLETED. Public release/tag/install/merge require separate authorization even if packaging is approved.
