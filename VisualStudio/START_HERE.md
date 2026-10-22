# Visual Studio source — v0.3.3.1 issue #1 DIAGNOSTIC

EXPERIMENTAL: idle crash cause remains unconfirmed. Open `GlassHeroesBusiness.sln`, targeting .NET Framework 4.8 / C# 7.3 and ScriptHookVDotNet2 2.11.6. The project compiles `scripts/GlassHeroesBusiness.2.cs`, which must match the `Install/scripts/` deployment source byte-for-byte. The original root RC1 source remains a historical reference.

A real cloud Release build passed with zero errors and two existing warnings. GTA execution and a Windows build are not verified. See `../docs/releases/v0.3.3.1-issue1/DEVELOPER_GUIDE.md` and validation artifacts for exact results, changed methods, full diff, rollback and manual regression steps. Install only the deployment .2.cs, preserving user saves/settings. Never install both source and its compiled business DLL.
