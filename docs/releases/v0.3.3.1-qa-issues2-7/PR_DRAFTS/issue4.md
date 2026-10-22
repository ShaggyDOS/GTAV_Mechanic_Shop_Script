Use GTA forward (-sin heading, cos heading), preserve saved placement/draw bounds, and add an explicit front arrow. Verify cardinal, diagonal and wrapped heading geometry.

Relates to #4. Targets development/v0.3.3.1. EXPERIMENTAL; review pending. No main/integration merge. Preserve all business saves/settings; see the documented additive visual/route metadata where applicable.

Validation: real net48 / C# 7.3 / ScriptHookVDotNet2 2.11.6 Release build PASS (two existing warnings), 21/21 static checks and 7/7 extracted production behavioral tests PASS. Includes complete matched install/Visual Studio sources, full applicable diff, actual transcripts, versioned MD/DOCX/PDF guide and experimental source ZIP.

GTA testing NOT RUN. Windows build NOT RUN. Stack-overflow cause UNPROVEN; existing issue #1 diagnostic PR #9 and audit PR #8 remain separate. Details: docs/releases/v0.3.3.1-issue4/DEVELOPER_GUIDE.md.
