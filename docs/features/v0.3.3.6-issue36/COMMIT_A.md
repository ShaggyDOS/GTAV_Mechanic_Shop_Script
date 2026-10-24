# A — v0.3.3.6 Issue #36 footer correction

WrapMenuDescription now measures against the actual 394-design-pixel footer content width at the existing ChaletLondon scale, caches at most 128 layouts and reuses the last unchanged description without width queries. Pure bounded LayoutMenuDescription/FittingMenuWords normalize whitespace/GTA newline tokens, preserve words, balance two fitting lines and use a word-boundary final ellipsis. Oversized/unbreakable text is bounded safely. SHVDN Draw/width measurements share reference coordinates, so no per-frame physical resolution query is needed.

DrawMenu retains its 420-wide layout, 119-high footer, colours, row/header dimensions, font and bottom hints/counter. Description baselines use 23-pixel leading and center a single line in the help band. The generic Back help is shortened. No Save/Settings keys/defaults, financial/movement/input behavior or route geometry changed.

Before: fixed 53-character split, hard word cuts, +10/+40 description offsets. After: bounded measured words, two fitting lines, +16/+39 baselines (or one centered line). Full source diff will accompany the complete maintenance report.

Actual targeted tests: footer36 15/15 PASS using extracted production helpers and deterministic variable-width metrics. Actual net48 Release build: PASS, 0 errors / inherited CS0169 and CS0649. Full A-stage/baseline results are in their separate folders. GTA visual testing NOT RUN; owner still must inspect actual 720p/1080p font/overlay appearance. No installation or acceptance.
