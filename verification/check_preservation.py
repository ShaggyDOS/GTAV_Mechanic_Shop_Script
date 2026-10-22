#!/usr/bin/env python3
"""Verify preserved business/timer source and additive key changes against integration."""
import ast
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
repo=Path(__file__).resolve().parents[1]
path='VisualStudio/scripts/GlassHeroesBusiness.2.cs'
current=(repo/path).read_text(encoding='utf-8-sig')
baseline=repo/'GlassHeroesBusiness_v0.3.3.0_RC1.2.cs'
if not baseline.exists(): baseline=repo/'verification/baselines/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs.txt'
assert hashlib.sha256(baseline.read_bytes()).hexdigest()=='030a44cfe4afc199c7865cd721361056be54398cf735eb1dbfc53cc87dee6949'
base=baseline.read_text(encoding='utf-8-sig')
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
unchanged=['ProcessEconomy','ProcessRemoteOperations','EnterRemoteOperations',
 'LeaveRemoteOperations','TransferPlayerMoney','CancelSupplierOrderWithConfirmation','RecoverSupplierShipment',
 'MatchesWholesaleVehicle','FindAssignedWholesaleVehicle']
for name in unchanged:assert method(base,name)==method(current,name),name+' changed unexpectedly'
reference=json.loads((repo/'verification/pr9_diagnostics_reference.json').read_text())
a=current.index('    // BEGIN ISSUE1 DIAGNOSTIC LOG'); b=current.index('    // END ISSUE1 DIAGNOSTIC LOG',a)
assert hashlib.sha256(current[a:b].encode()).hexdigest()==reference['diagnostic_region_sha256'], 'Logger differs from PR9'
for name,expected in reference['methods_sha256'].items():
    assert hashlib.sha256(method(current,name).encode()).hexdigest()==expected, name+' differs from PR9'
tick=method(current,'OnTick')
tick=re.sub(r'^[ \t]*_diagnosticTickStage = "[^"\n]*";\n','',tick,flags=re.M)
checkpoint="""                _diagnosticLog.RecordTransition(_clockedIn, _remoteMode,
                    _workersPreviouslyInRange, _showMenu, _supplierVisualStage,
                    _activeCustomerOrders.Count, _remoteJobs.Count);
"""
assert checkpoint in tick
tick=tick.replace(checkpoint,'').replace('Log("Tick / " + _diagnosticTickStage, ex);','Log("Tick", ex);')
assert tick==method(base,'OnTick'), 'Tick changed beyond PR9 diagnostics'
abort=method(current,'OnAborted').replace('        Log("LIFECYCLE abort requested; last tick phase=" + _diagnosticTickStage, null);\n','').replace('        Log("LIFECYCLE abort cleanup completed", null);\n','')
assert abort==method(base,'OnAborted'), 'Abort changed beyond PR9 diagnostics'

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
result={'unchanged_production_methods':unchanged,'diagnostic_only_methods_normalized_to_base':['OnTick','OnAborted'],'diagnostics_match_pr9':True,'pr9_commit':reference['commit'],'ledger_change':'visual slot provenance only; all financial operations identical',
 'old_literal_save_config_keys_preserved':True,'added_literal_key_prefixes':added,
 'native_constructor_count':'covered by original static suite',
 'scope':'source preservation guard; does not execute GTA or prove file persistence','gta_testing':'NOT RUN'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2));print('PASS preservation guards')
