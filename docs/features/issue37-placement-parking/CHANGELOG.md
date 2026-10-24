# v0.3.4 Issue #37 Placement & Parking Experimental

October 24, 2026. Implementation pre-approved; final acceptance PENDING owner YES/NO. GTA testing NOT RUN.

- A: position-only slow customer drop-off approach for incoming/returned vehicles, stable low-speed/clearance acceptance and bounded retry/timeout. Preserve service/queue bay orientation and paid-return accounting. Neutral drop-off zone; saved heading retained for compatibility/initial spawn.
- B: plain Staff Spawn -> Break and Break <-> Service Bay labels, final pedestrian approaches, per-mechanic use separate from saved coordinates, exact selected-point focus. No saved mask/route geometry change.
- C: persistent cosmetic Hide All/Focused/Custom/All with five categories, custom recall and filtered cached auto previews. Point saves no longer force visibility; separate UI preference file.
- D: guarded per-category/full geometry reset and scope-based restore, opt-in full shop anchor, two default-NO confirmations, complete verified backup, atomic spatial patch, current-ledger preservation and cleanup/reload guard. Remove implicit destructive clearing when relocating a configured shop.
- Actual Release: 0 errors/two inherited warnings. 1057/1057 behavior/navigation, 21/21 static, 13/13 structural and preservation PASS. Word/PDF, source diff, manual GTA/rollback and exact final handoff included.
- No main/RC1/RC2 modification, merge, public release, live installation or crash-fix claim. Not v0.3.5.
