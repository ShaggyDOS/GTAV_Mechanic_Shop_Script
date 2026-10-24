#!/usr/bin/env python3
"""Prove that removing diagnostic-only scopes restores the exact RC1 bytes."""
import hashlib,json,re,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'verification/maintenance36'))
from source_tools import members,SOURCE
BASE='e312d8c9500f4a990eb993b82e8e76b55711d3de'
EXPECTED='70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120'
original=subprocess.check_output(['git','show',BASE+':'+str(SOURCE.relative_to(ROOT))],cwd=ROOT)
assert hashlib.sha256(original).hexdigest()==EXPECTED
old=original.decode();current=SOURCE.read_text();before,after=members(old),members(current)
names=['UpdateOneCustomerOrder','AssignCustomerMechanic','BeginVehicleWalk','BeginEnterVehicle','StartValetDeparture','BeginVehicleDrive','UpdateVehicleApproach','UpdateVehicleParking','FailVehicleTransfer','ProcessPaidReturnRecovery']
added={'CustomerVehicleTraceState','CustomerVehicleTrace','CaptureCustomerVehicleTrace','BeginCustomerVehicleTrace','EndCustomerVehicleTrace','AppendCustomerVehicleDiagnosticLine'}
assert set(after)-set(before)==added and not set(before)-set(after)
assert {n for n in before if before[n]!=after[n]}==set(names)
restored=current
for operation,name in enumerate(names):
    head,body=after[name].split('\n    {\n',1)
    prefix='        long trace = BeginCustomerVehicleTrace(order, '+str(operation)+');\n        try\n        {\n'
    suffix='\n        }\n        finally { EndCustomerVehicleTrace(order, trace); }\n    }'
    assert body.startswith(prefix) and body.endswith(suffix),name
    body=body[len(prefix):-len(suffix)]
    assert all(not line or line.startswith('    ') for line in body.splitlines())
    unwrapped=head+'\n    {\n'+'\n'.join(line[4:] if line else '' for line in body.splitlines())+'\n    }'
    assert unwrapped==before[name],name+' gameplay body changed'
    restored=restored.replace(after[name],unwrapped,1)
start=restored.index('    // RC2 DIAGNOSTIC ONLY:')
end=restored.index('    private static void Notify(string message)',start)
block=restored[start:end]
for forbidden in ['GTA.Native','Game.','World.','.Position','Save(','GetValue(','SetValue(','Task.','Thread.','Environment.StackTrace']:
    assert forbidden not in block,forbidden+' in new diagnostics'
assert not re.search(r'(?<!File)\.Exists\(',block),'Entity existence query added'
assert block.count('Path.Combine(DataDirectory, "CustomerVehicle.RC2.log")')==1
restored=restored[:start]+restored[end:]
assert restored.encode()==original,'Non-diagnostic source drift'
protected=['AGENTS.md','Install/scripts/GlassHeroesBusiness.2.cs','VisualStudio/scripts/GlassHeroesBusiness.2.cs','VisualStudio/GlassHeroesBusiness.csproj','VisualStudio/GlassHeroesBusiness.sln','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.sln']
for path in protected:
    assert (ROOT/path).read_bytes()==subprocess.check_output(['git','show',BASE+':'+path],cwd=ROOT),path
for fixture in 'reload status336 recovery336 inventory340 workflow335 bays retail336 repair sales'.split():
    path='verification/economy/'+fixture+'_tests.cs'
    previous=subprocess.check_output(['git','show',BASE+':'+path],cwd=ROOT,text=True)
    adapter='\n // Diagnostics-only sink adapter; existing gameplay assertions stay unchanged.\n long BeginCustomerVehicleTrace(CustomerOrder order,int operation)=>-1;\n void EndCustomerVehicleTrace(CustomerOrder order,long sequence){}'
    assert (ROOT/path).read_text()==previous.replace('class Harness {','class Harness {'+adapter,1),fixture+' inherited assertions changed'
report={'rc1_source_commit':BASE,'rc1_source_sha256':EXPECTED,'rc2_source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
        'diagnostic_wrapped_methods':names,'diagnostic_added_members':sorted(added),'original_bodies_and_all_other_source_bytes_unchanged':True,
        'unchanged_original_members':len(before)-len(names),'save_settings_schema_defaults_finances_inventory_ai_route_geometry_and_clocks_unchanged':True,
        'new_native_calls_world_scans_or_gameplay_tasks':0,'protected_sources_projects_unchanged':True,'inherited_fixture_assertions_byte_preserved':True,'gta_testing':'NOT RUN','crash_cause':'UNATTRIBUTED'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2));print('PASS RC2 diagnostic-only exact RC1 restoration')
