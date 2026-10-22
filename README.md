# GlassHeroes Business Script

A GTA V script / mod project focused on a mechanic shop business system, including manager palette and order flows.

This repository is organized as a scratch/project workspace for developing and tracking a GTA V gameplay script. It is meant to be used with a local GTA V modding workflow and can be expanded with additional scripts, docs, and assets as the project grows.

## Project Goal

Provide a structured foundation for a mechanic shop / business management-style GTA V script, including:

- business script logic
- manager interaction flow
- order / palette handling
- configuration and documentation for future expansion

## Repository Layout

```text
.
├── .gitignore
├── README.md
├── docs/
├── scripts/
├── assets/
├── config/
└── tools/
```

## Quick Start

1. Clone the repository.
2. Place your GTA V mod/script files in the appropriate folders.
3. Keep source files in `scripts/`, config data in `config/`, and documentation in `docs/`.
4. Test locally in your GTA V modding environment.

## Recommended Folder Structure

```text
scripts/
  mechanic_shop.lua
  config.lua
  manager_palette.lua

docs/
  README.md
  ROADMAP.md
  RELEASE_NOTES.md

assets/
  icons/
  images/

config/
  settings.ini
```

## Notes

- This project is currently set up as a working mod project repository.
- Local build artifacts, logs, and GTA install-specific files are intentionally ignored via `.gitignore`.
- To keep the repo clean, do not commit generated binaries or local test output unless you specifically want them tracked.

## Suggested Future Improvements

- add structured configuration files
- break logic into script modules
- add testing notes and verification steps
- document install and usage steps for mod users
- track changelog and roadmap entries in `docs/`

## License

This project does not include a formal license yet. If you intend to share or distribute the mod publicly, consider adding an appropriate license file such as MIT or GPL.

## Maintainer

ShaggyDOS
