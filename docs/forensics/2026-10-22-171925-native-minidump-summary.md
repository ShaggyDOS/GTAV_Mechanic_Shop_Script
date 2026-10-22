# Private minidump forensic summary — Oct 22 2026 17:19:25 (native loading crash)

**Sanitized technical findings only. Original user-uploaded minidump remains private; DO NOT commit or publish it.**

## Provenance and scope

- Uploaded file: Oct 22 17:19:25 CrashAnalyzer minidump, ~5.4 MB. Parsed directly from Windows MINIDUMP structures and checked with native machine-instruction disassembly. The stream header's timestamp is **2026-10-22 22:19:26 UTC** (17:19:26 in US Central daylight time), so it matches the previously shared native startup crash report.
- GTA5.exe: Legacy 1.0.3889.0; process uptime in external crash report **57 seconds**, mid-load near official `mpstunt` archive. Last loaded archive alone does NOT prove corruption or causality.
- Dump includes 16 stream-directory entries; 244 loaded module records, 207 thread records; a crash exception stream, exception thread context and sampled process memory. It does NOT contain symbolic GTA engine function names, private mod source, or proof of the originating caller's semantic intent.
- **Exception code:** `0xC0000005` (READ access violation, not `0xC00000FD` CLR overflow).
- **Exception thread:** `12676` (0x3184), matching the crashing GTA process/main thread observed in other logs.
- **Instruction:** GTA5.exe relative virtual address `+0xF62E79` (`0x7FF6EF8B2E79` in this process). This exact **relative** address also appeared in the earlier Oct 22 16:40:30 Script Hook V report of an exception during execution of CargoOperations.asi. The latter was a *different session and native-plugin error*; common engine site is not attribution of the later CLR overflow.

## Exact evidence from CPU exception context and dump memory

At the faulting instruction, captured bytes are `48 8B 04 D0 C3`:

```asm
GTA5.exe+0xF62E79: mov rax, qword ptr [rax + rdx*8]
GTA5.exe+0xF62E7D: ret
```

Relevant preserved registers:
- `RAX=0x000001C99472E180`: a readable base pointer to what appears to be an array of 8-byte entries (process dump contains bytes there).
- `RDX=0x00000000FFFFFFFF`: 32-bit all-ones value, **zero extended** to 64 bits. Often represents an invalid/sentinel index but its upstream origin/type is not proven.
- Effective address = `0x000001C99472E180 + 0x00000000FFFFFFFF * 8 = 0x000001D19472E178`.
- The `MINIDUMP_EXCEPTION` read-fault address is **exactly** `0x000001D19472E178`. There is no valid captured mapping for that address in the sample. Instruction decode and formula thus match the exception, without assumptions about loaded mods or asset identifiers.
- This function directly indexes an array without an in-function bounds check. Whether caller/resource state is invalid, a sentinel is mishandled, a pointer corrupted, or a mod-specific hook/asset caused that value is **undetermined**. A debugger with build-matched engine symbols and caller context would be required to identify the source.
- Captured return/stack pointers resolve to GTA5.exe frames (first return `+0x627024`, next plausible `+0x63D453`, then `+0x638D4D`), consistent with the native engine-only crash report. No managed user business frame is identified on the exception stack. Lack of managed frames doesn't exonerate all plugins/hooks.

## Isolation state — important negative findings

The same-session ScriptHookV.log lists ASIs but **NO CargoOperations.asi** registration or launch. The minidump module table likewise has **no CargoOperations.asi**. The minidump has **no Glass Heroes Business module**; more importantly the same-session ScriptHookVDotNet log expressly states `AutoLoadScripts=false`, so managed scripts were not auto-started. A separate idle v3 probe may be present in the folder but not active in this crash. The CLR + SHVDN runtime binaries are loaded, though.

Other modules in process include SimpleHUD.asi, ScriptHookV, QuantV, RAGEPluginHook, Los Santos RED and LSROrgBridge. **Presence in module list is not causal evidence.** Both Los Santos RED and SimpleHUD separately reported initialization near 17:19:23 (2 sec before the crash), but their close timing alone doesn't establish responsibility. A functioning immediate subsequent boot reported by owner implies the failure is intermittent, not demonstrably a permanently bad `mpstunt` DLC pack.

The prior **no-GH / Cargo-ON** run crashed at 16:52:19 after a different CLR stack overflow (`0xC00000FD`), while this **GH-OFF / Cargo-OFF / SHVDN-auto-OFF** run failed during startup with **native** `0xC0000005`. These are not interchangeable crash signatures. Cargo ASI is not necessary for this particular native engine access-violation, and the active Glass Heroes Business code was not executed. Neither v2→v3 migration nor PR18 heartbeat is a demonstrated fix for this native crash.

## Next evidence / test guidance

1. Keep current stable follow-up launch going unchanged; record whether it can pass the earlier 30m failure intervals, exit normally, and whether the Glass Heroes Strawberry parking-lot ground is solid before and after LS RED.
2. Capture a separate failure only if one occurs. **One-change-per-run**, no simultaneous gameconfig / ASI / RPH / SHVDN / MLO edits; current uploaded base gameconfig retains the previously successful map-streaming values.
3. If native startup failures recur, compare cold boot with the SAME GH-off, Cargo-off, SHVDN-autoload-off configuration but ONE other component disabled, e.g. suppress LS RED autostart to check the map/collision problem; separately isolate SimpleHUD if failures persist. Neither is convicted. Preserve comparable timestamps and normal launcher path.
4. Symbolization of this exact internal GTA address cannot be completed solely from this small dump without matching GTA function symbols or external reverse-engineered signatures. If owner elects private debugger work, inspect caller-supplied array index and the object/resource that supplied `0xFFFFFFFF`; inspect any prior code path that should reject an invalid/sentinel index. Do not share or attach memory dumps to public GitHub.

**Review status:** Forensic evidence, not a fix. PR #18, PR #19 and PR #21 remain independent DRAFT; no user saves/settings or GH code should be modified in response to this dump alone.
