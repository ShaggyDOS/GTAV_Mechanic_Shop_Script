# Oct 22 2026 18:21:30 CDT — private CLR minidump review (sanitized)

This note contains technical metadata from a user-supplied Windows minidump, **not the dump itself**. Do not upload the private dump, raw stack bytes, identifying file paths or process memory to this public repository.

## Directly confirmed in uploaded dump

- Windows MINIDUMP (~6.0 MiB); header timestamp **2026-10-22 23:21:32 UTC** = 18:21:32 CDT, consistent with the related CrashAnalyzer event.
- **Exception 0xC00000FD**, a stack overflow; **not** an engine startup access violation (0xC0000005).
- Exception thread ID **24812**; fault instruction resolves to **clr.dll+0x5F8D10** (one of the previously observed CLR offsets, different from the later 18:40:06 `clr.dll+0x68AFE` report).
- The exception stream includes a thread context and a sampled ~12.6 KiB stack slice. This is a minidump, not a full process image.
- The exception thread's sampled stack contains CLR exception/unwind frames and numerous GTA5.exe addresses. **No verifiable repeated managed user-script frame or named recursive method was recovered from this sampled stack.** Scanning stack words for apparent return addresses is not a reliable managed stack walk.
- Module records include GTA5.exe, CLR, SHVDN.asi, ScriptHookVDotNet2.dll, ScriptHookVDotNet3.dll, and several managed game-mod assemblies; **mere module presence is not attribution of this exception**.
- The native module table is not a reliable way to prove a script dynamically compiled by SHVDN from a .cs source was or was not active. Matching-session SHVDN startup logs would be needed. This upload alone **does not determine the role of the Glass Heroes probe**.

## Limits and follow-up

This dump **does not identify the culpable mod or script**. A matching 64-bit Windows WinDbg session with symbols, SOS for .NET Framework 4.8, !analyze -v, .ecxr, !threads, !clrstack and !dumpstack may help attribute the overflow. Limited minidump stack content may prevent recovery even with SOS. Compare against the separate 18:40:06 crash; their CLR instruction offsets differ and must not be assumed to be identical execution paths.

Owner explicitly approved continuing isolated Glass Heroes development independent of this ongoing crash work. Keep GH issue #20 / PR #21 migration and PR #19/#22 wholesale work unmerged and uninstalled until code passes tests and owner authorizes GTA testing. No business save/INI modifications as part of crash diagnosis.