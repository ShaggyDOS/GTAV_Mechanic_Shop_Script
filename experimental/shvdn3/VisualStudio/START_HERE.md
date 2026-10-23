# Experimental v3 project — Issue #20 / draft PR #21

Open `GlassHeroesBusiness.V3.Experimental.sln`, restore the pinned NuGet package and build Release. Target is .NET Framework 4.8 / C#7.3, ScriptHookVDotNet3 3.6.0. The project links `../Install/scripts/GlassHeroesBusiness.3.cs`; that single file is both development and installation source. Output assembly is `GlassHeroesBusiness.V3.Experimental` and is not included in the source package.

The original v2 solution outside this directory is unchanged. Do not reference it from this project or enable both versions in GTA. Review the root experimental README and full developer report before any owner-approved test. GTA execution and Enhanced 3.9.0.6 runtime compatibility are NOT RUN / NOT VERIFIED.
