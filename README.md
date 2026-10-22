# GlassHeroes Business Script

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
