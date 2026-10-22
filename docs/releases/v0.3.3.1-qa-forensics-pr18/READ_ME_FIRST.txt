v0.3.3.1 PR18 FORENSICS — EXPERIMENTAL / REVIEW FIRST
Draft PR #18; target unchanged QA R2. GTA testing: NOT RUN. Crash cause: UNPROVEN.
No P1 map-blip code. No automatic deployment. Preserve live Save.ini/Settings.ini.
Read docs/releases/v0.3.3.1-qa-forensics-pr18/DEVELOPER_GUIDE.md (also DOCX/PDF).
Optional QA enablement: ForensicsEnabled = true in EXISTING [Diagnostics], then reload.
Default is false. Normal startup defaults remain false/30. Do not replace live settings.
After approval, GTA stopped: back up externally and copy ONLY Install/scripts/GlassHeroesBusiness.2.cs.
VisualStudio/scripts is the matching development copy: never install both.
Rollback: GTA stopped; restore accepted R2 source, disable the optional forensic key.
New Forensics.log is bounded, best effort and may not survive a fatal overflow; keep it private.
Review source.diff, validation.json, audit, test/build transcripts and ZIP manifest.
Do not merge branches or main. No business data has been changed by this package.
