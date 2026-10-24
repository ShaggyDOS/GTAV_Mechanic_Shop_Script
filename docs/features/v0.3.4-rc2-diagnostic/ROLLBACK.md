# RC2 rollback and data preservation

Close GTA and all script hosts. Preserve the complete RC2 test data directory and both RC2 diagnostic log segments, existing error/host logs and dumps outside script loading paths. Keep the original RC2 source/package hashes with this evidence.

Remove the RC2 script from active loading paths and restore the verified RC1 script plus its COMPLETE matching pre-test business-data/settings backup. Owner-specified RC1 source commit: e312d8c9500f4a990eb993b82e8e76b55711d3de; source SHA256: 70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120. The approval-only RC1 tip has the same source. Ensure exactly one generation/source/DLL is active before relaunch.

Do not reset, hand-edit or combine the sole live Save.ini with another version's finance/receipt/profile files. Restore all companion files together. RC2 adds no save/config schema or migration, but normal gameplay transactions can legitimately change data during testing. Keep both pre/post-test copies rather than discarding evidence. Diagnostic files may remain archived separately; no tool in this package restores or rewrites live files automatically.
