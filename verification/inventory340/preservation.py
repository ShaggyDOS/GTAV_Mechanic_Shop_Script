#!/usr/bin/env python3
"""Check v0.3.4's explicit source boundary against the pinned RC1 Git object."""
import ast, hashlib, json, re, subprocess, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
BASE = 'c8617aed5cda492214bbbe1907042bcafbfa0dc1'
REL = 'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
old_bytes = subprocess.check_output(['git', 'show', BASE + ':' + REL], cwd=ROOT)
assert hashlib.sha256(old_bytes).hexdigest() == '7fff30978e2dc4c3235656f6ca6865506d5cea53387b7eb6d83654d71c8a9d6b'
old = old_bytes.decode('utf-8-sig'); new_bytes = (ROOT / REL).read_bytes(); new = new_bytes.decode('utf-8-sig')
lexer = next(n for n in ast.parse((ROOT/'Verification/static_verify.py').read_text()).body if isinstance(n, ast.FunctionDef) and n.name == 'strip_literals')
ns = {}; exec(compile(ast.Module(body=[lexer], type_ignores=[]), 'lexer', 'exec'), ns)
def members(text):
    clean, _ = ns['strip_literals'](text); out = {}
    for m in re.finditer(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?(\w+)\s*\(', clean, re.M):
        end = clean.index('{', m.start()) + 1; depth = 1
        while depth:
            depth += (clean[end] == '{') - (clean[end] == '}'); end += 1
        assert m[1] not in out, m[1]
        out[m[1]] = text[m.start():end]
    return out
before = members(old); after = members(new)
allowed = set('''BeginSupplierRearUnload CanStoreShipment CancelSupplierOrderWithConfirmation CommitSupplierDelivery DeleteSupplierVisuals ExecuteSelection ExecuteSupplierCart ExecuteSupplierPalletStorage GetMenuItemValue GetMenuItems GetMenuTitle GetPageDescription GetSelectedDescription GiveBoxToDriver GlassHeroesBusinessV3Experimental MuleStageName NavigateBack ProcessSupplierDelivery RecoverSupplierShipment Save ShowDetailedInventory ShowReorderAdvice ShowSupplierDeliveryStatus ShowSupplierOrderContents TryCancelSupplierOrder TryManagerSupplierDeposit TrySupplierPackageInteraction'''.split())
expected_new = set('''CaptureSupplierOutcome ExecuteSupplierOrderReview InventoryAvailabilityRow InventoryReorderSuggestion InventoryStorageRows LoadSupplierOutcome OnHandInventory PendingSupplierInventory PrepareSupplierOrderReview ShowInventoryCategoryDetail ShowLastSupplierOutcome ShowSupplierPurchaseReview SupplierLiveState SupplierReceiptBlocker TryBuyStoragePallet TryCommitWholesaleCargo TryResetSupplierCargo UpdateSupplierDriverExit WriteSupplierOutcome'''.split())
changed = {n for n in before.keys() & after.keys() if before[n] != after[n]}
assert changed == allowed, ('Unexpected method boundary', sorted(changed ^ allowed))
assert after.keys() - before.keys() == expected_new
assert not before.keys() - after.keys()
# All remaining methods must match, including clocks, quotes, retail, wages,
# finance, customer/mechanic service, placement and existing purchase formulas.
unchanged = sorted(before.keys() - changed)
assert all(before[n] == after[n] for n in unchanged)
assert after['Save'].replace('            WriteSupplierOutcome(data);\n', '') == before['Save'], 'Existing save payload changed'
restored_ctor = after['GlassHeroesBusinessV3Experimental'].replace('            LoadSupplierOutcome(data);\n', '').replace('v0.3.4 SHVDN3 INVENTORY + PROCUREMENT DEV', 'v0.3.3.6 SHVDN3 SERVICE RECOVERY + BUSINESS POLISH')
assert restored_ctor == before['GlassHeroesBusinessV3Experimental'], 'Constructor exceeds pure metadata read + label'
for name in ['FinanceSnapshot', 'FinanceEntry', 'FinanceBook', 'RepairQuote', 'SavedRepairPromise']:
    def block(t):
        c, _ = ns['strip_literals'](t); m = re.search(r'\bclass '+name+r'\s*\{', c); assert m, name
        start=m.start(); end=c.index('{',start)+1; depth=1
        while depth: depth += (c[end]=='{')-(c[end]=='}'); end+=1
        return t[start:end]
    assert block(old) == block(new), name+' changed'
protected = json.loads((ROOT/'verification/economy/scopes/workflow336.json').read_text())['protected_v2']
for path, sha in protected.items(): assert hashlib.sha256((ROOT/path).read_bytes()).hexdigest() == sha, path
# Every final receipt call supplies identity; zero is not an identity bypass.
for line in new.splitlines():
    if 'CommitSupplierDelivery(' in line:
        assert ',' in line, line
assert 'expectedOrderTicks != 0L' not in after['CommitSupplierDelivery']
# Projection helpers are selected on demand; no new saves/natives/background work.
for name in ['PendingSupplierInventory','OnHandInventory','InventoryStorageRows','InventoryReorderSuggestion','ShowInventoryCategoryDetail','ShowDetailedInventory','ShowReorderAdvice']:
    assert 'Save(' not in after[name] and 'GTA.Native' not in after[name], name
report = {'baseline_commit': BASE, 'baseline_source_sha256': hashlib.sha256(old_bytes).hexdigest(), 'source_sha256': hashlib.sha256(new_bytes).hexdigest(), 'changed_methods': sorted(changed), 'new_methods': sorted(expected_new), 'unchanged_method_count': len(unchanged), 'protected_v2_files': protected, 'clock_and_income_methods_unchanged': True, 'existing_save_payload_unchanged_except_additive_outcome': True, 'finance_and_repair_types_unchanged': True, 'gta_testing': 'NOT RUN'}
if len(sys.argv)>1: Path(sys.argv[1]).write_text(json.dumps(report, indent=2)+'\n')
print(json.dumps(report, indent=2)); print('PASS inventory340 method, save, clock, finance, placement and v2 preservation')
