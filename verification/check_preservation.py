#!/usr/bin/env python3
"""Verify preserved business/timer source and additive key changes against integration."""
import ast
import json
from pathlib import Path
import re
import subprocess
import sys
repo=Path(__file__).resolve().parents[1]
path='VisualStudio/scripts/GlassHeroesBusiness.2.cs'
current=(repo/path).read_text(encoding='utf-8-sig')
base=subprocess.check_output(['git','show','development/v0.3.3.1:'+path],cwd=repo).decode('utf-8-sig')
lexer=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals')
ns={};exec(compile(ast.Module(body=[lexer],type_ignores=[]),'lexer','exec'),ns)
def method(text,name):
    clean,_=ns['strip_literals'](text)
    match=re.search(r'^    private\s+(?:static\s+)?[^\n]+\b'+name+r'\(',clean,re.M)
    assert match, name
    opening=clean.index('{',match.start());depth=1;end=opening+1
    while depth:
        depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
    return text[match.start():end]
unchanged=['OnTick','OnAborted','ProcessEconomy','ProcessRemoteOperations','EnterRemoteOperations',
 'LeaveRemoteOperations','TransferPlayerMoney','CancelSupplierOrderWithConfirmation','RecoverSupplierShipment',
 'MatchesWholesaleVehicle','FindAssignedWholesaleVehicle']
for name in unchanged:assert method(base,name)==method(current,name),name+' changed unexpectedly'
newblock='''        if (palletSlot >= 0)
        {
            _deliveredPackageMasks[palletSlot] |= 1 << _storedDeliveryCrates[palletSlot];
            _storedDeliveryCrates[palletSlot]++;
            _palletVisualDirty = true;
        }'''
ledger=method(current,'CommitSupplierDelivery')
assert newblock in ledger
assert ledger.replace(newblock,'        if (palletSlot >= 0) _storedDeliveryCrates[palletSlot]++;')==method(base,'CommitSupplierDelivery'),'Financial commit changed beyond visual provenance'
keys=lambda s,op:set(re.findall(r'\.'+op+r'\("([^"]+)",\s*"([^"]+)"',s))
added={}
for op in ['GetValue','SetValue']:
    previous=keys(base,op);present=keys(current,op)
    assert previous<=present,op+' removed an old literal key'
    delta=present-previous
    assert delta=={('Storage','DeliveredPackageMask'),('EmployeeSettings','CustomJobRoutes')},(op,delta)
    added[op]=sorted(delta)
result={'unchanged_production_methods':unchanged,'ledger_change':'visual slot provenance only; all financial operations identical',
 'old_literal_save_config_keys_preserved':True,'added_literal_key_prefixes':added,
 'native_constructor_count':'covered by original static suite',
 'scope':'source preservation guard; does not execute GTA or prove file persistence','gta_testing':'NOT RUN'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2));print('PASS preservation guards')
