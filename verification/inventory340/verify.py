#!/usr/bin/env python3
"""v0.3.4 Phase A — conservative source/structure checks, NOT native GTA QA.

Run: python verification/inventory340/verify.py
This does not compile C# or prove gameplay correctness. Run the inherited
SHVDN3 build, 762 behavior checks, 21 static checks and real GTA QA separately.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs"
source = SOURCE.read_text(encoding="utf-8")


def method(name):
    anchor = "private "  # no exact return-type requirement
    lines = source.splitlines()
    start = next((i for i, line in enumerate(lines)
                  if anchor in line and (name + "(") in line), None)
    if start is None:
        raise AssertionError("Missing method: " + name)
    pos = source.find(lines[start], 0)
    # Search the matching line and first brace in source rather than relying on
    # a fixed line count; no expression-bodied helper is used in this feature.
    prev = "\n".join(lines[:start])
    pos = len(prev) + (1 if start else 0)
    opening = source.index("{", pos)
    depth = 0
    for index in range(opening, len(source)):
        if source[index] == "{":
            depth += 1
        elif source[index] == "}":
            depth -= 1
            if depth == 0:
                return source[opening + 1:index]
    raise AssertionError("Unbalanced method body for " + name)


pending = method("PendingSupplierInventory")
on_hand = method("OnHandInventory")
line = method("InventoryAvailabilityRow")
per_item = method("ShowInventoryCategoryDetail")
summary = method("ShowDetailedInventory")

checks = {
    "no-pending-order reports zero incoming": "if (_deliveryKind == -2) return pending;" in pending,
    "mixed pending supplier lines accepted": "_deliveryLines.Length == 4" in pending and "_deliveryLines[i]" in pending,
    "legacy pending supplier quantity supported": "pending[_deliveryKind + 1] = _deliveryQuantity" in pending,
    "on-hand excludes nonstocked incoming": "_parts" in on_hand and "_specialStock[2]" in on_hand and "_delivery" not in on_hand,
    "reserved = on-hand minus free": "onHand[index] - free[index]" in line,
    "category snapshot uses actual existing reservation logic": "UnreservedRepairStock(null)" in per_item,
    "summary uses actual existing reservation logic": "UnreservedRepairStock(null)" in summary,
    "incoming counted only for display": "PendingSupplierInventory()" in per_item and "PendingSupplierInventory()" in summary,
    "existing selected supplier order logic stays separate": "TryPlaceSupplierOrder" not in pending + on_hand + line + per_item + summary,
    "existing inventory view remains on demand": "ShowInventoryCategoryDetail(_selection)" in source and "else if (_selection == 4) ShowDetailedInventory();" in source,
    "inventory edit does not write saves": all("Save(" not in body for body in [pending, on_hand, line, per_item, summary]),
    "inventory edit avoids per-frame natives": all("GTA.Native." not in body and "World.GetNearby" not in body
                                               for body in [pending, on_hand, line, per_item, summary]),
    "incoming is explicitly nonusable in UI": "cannot be consumed or sold" in per_item,
}
for name, ok in checks.items():
    print(("[PASS] " if ok else "[FAIL] ") + name)
if not all(checks.values()):
    raise SystemExit(1)
print(f"PASS: {len(checks)}/{len(checks)} v0.3.4 Phase A STRUCTURAL checks; compile and GTA tests NOT RUN.")
