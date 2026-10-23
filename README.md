# SHVDN3 Phase 1 experimental migration — Issue #20 / draft PR #21

A separate net48/C#7.3 project now compiles against pinned ScriptHookVDotNet3 3.6.0. Original QA R2 v2 code/project remain untouched. [Review report](docs/releases/v0.3.3.1-shvdn3-phase1/DEVELOPER_GUIDE.md) · [API map](docs/releases/v0.3.3.1-shvdn3-phase1/API_MAP.md) · [v3 project/source](experimental/shvdn3/README.md). GTA testing NOT RUN; Enhanced runtime compatibility NOT VERIFIED; crash cause UNATTRIBUTED. No merge or live installation change.

# GlassHeroes Business Script

Current final-review candidate: **v0.3.3.1 QA R2 EXPERIMENTAL**, combining PR #9 diagnostics, PR #16 corrections and bounded warehouse retry renewal. See [R2 developer guide](docs/releases/v0.3.3.1-qa-r2-final-review/DEVELOPER_GUIDE.md) and [final review handoff](docs/releases/v0.3.3.1-qa-r2-final-review/FINAL_REVIEW_HANDOFF.md). All automated checks pass; GTA testing NOT RUN and crash cause UNPROVEN. Use the single R2 source ZIP for review; earlier QA links below are historical and do not include this follow-up.

Review-pending v0.3.3.1 QA candidate (#2–#7): see [developer guide](docs/releases/v0.3.3.1-qa-issues2-7/DEVELOPER_GUIDE.md) and [branch/PR handoff](docs/releases/v0.3.3.1-qa-issues2-7/REVIEW_HANDOFF.md). Build and automated checks pass; GTA testing NOT RUN; crash cause UNPROVEN. Install one `Install/scripts/GlassHeroesBusiness.2.cs` only, preserving all saves/settings. Historical root RC1 remains unchanged.

A GTA V mod project focused on a business / mechanic shop script system.

## Purpose

This repository is organized as a clean working project for the GlassHeroes Business script, including:

- business logic
- manager / palette interactions
- order flow and script structure
- documentation and setup notes
- local verification and installation references

## Repository Layout

```text
.
├── .gitignore
├── README.md
├── LICENSE
├── docs/
│   ├── START_HERE.md
│   ├── ROADMAP.md
│   └── RELEASE_NOTES.md
├── config/
│   └── Settings.ini.example
├── scripts/
│   └── README.md
├── assets/
│   └── README.md
├── install/
│   └── README.md
├── verification/
│   └── README.md
├── visualstudio/
│   └── README.md
└── build/
    └── README.md
```

## Recommended Use

- Put all source script files in `scripts/`
- Put settings/example config in `config/`
- Put design and notes in `docs/`
- Keep packaged or install-ready files in `install/`
- Keep verification/test notes in `verification/`
- Keep generated build output out of Git via `.gitignore`

## Notes

This repo is intended to be a clean GitHub-ready version of the local GTA V mod project. It is not a game-install folder and should not contain generated binary output, local build artifacts, or test logs.

## Next Steps

1. Add or move the actual source files into `scripts/`
2. Add config values to `config/Settings.ini.example`
3. Update `docs/RELEASE_NOTES.md` with your current version notes
4. Expand `docs/ROADMAP.md` as the project grows
5. Test in your local GTA V modding environment before releasing

## Maintainer

ShaggyDOS
