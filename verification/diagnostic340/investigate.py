#!/usr/bin/env python3
"""Conservative RC1 lexical call audit. External/native/CLR stacks are unknown."""
import json,re,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'verification/maintenance36'))
from source_tools import members,strip_literals
BASE='e312d8c9500f4a990eb993b82e8e76b55711d3de'
s=subprocess.check_output(['git','show',BASE+':experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'],cwd=ROOT,text=True)
methods={n:b for n,b in members(s).items() if '(' in b.splitlines()[0]}
# Unqualified calls only. In particular, ScriptSettings data.Save() is not
# recursion into this script's Save(). This audit cannot resolve delegate targets.
graph={n:set(re.findall(r'(?<![\w.])(\w+)\s*\(',strip_literals(b[b.index('{')+1:])[0]))&set(methods) for n,b in methods.items()}
roots=['UpdateOneCustomerOrder','AssignCustomerMechanic','BeginVehicleWalk','BeginEnterVehicle','StartValetDeparture','BeginVehicleDrive','UpdateVehicleApproach','UpdateVehicleParking','FailVehicleTransfer','ProcessPaidReturnRecovery']
seen=set();todo=list(roots)
while todo:
    n=todo.pop()
    if n not in seen:seen.add(n);todo.extend(graph[n]-seen)
index={};low={};stack=[];active=set();cycles=[]
def visit(n):
    index[n]=low[n]=len(index);stack.append(n);active.add(n)
    for child in sorted(graph[n]&seen):
        if child not in index:visit(child);low[n]=min(low[n],low[child])
        elif child in active:low[n]=min(low[n],index[child])
    if low[n]==index[n]:
        group=[]
        while True:
            child=stack.pop();active.remove(child);group.append(child)
            if child==n:break
        if len(group)>1 or n in graph[n]:cycles.append(sorted(group))
for n in sorted(seen):
    if n not in index:visit(n)
result={'baseline':BASE,'observed_failure':{'date_owner_local':'October 24, 2026','exception_code':'0xC00000FD','reported_context':'Mechanic 1 transporting customer car into service bay','responsible_script':'UNKNOWN'},
        'crash_dump_or_owner_log_examined':False,'parsed_script_methods':len(methods),'reachable_script_methods':len(seen),'roots':roots,'lexical_call_cycles':cycles,
        'reachable_edges':{n:sorted(graph[n]) for n in sorted(seen)},
        'limitations':'Lexical audit only; not a C# semantic call graph. Qualified calls, SDK/CLR/native internals, callbacks, other scripts and runtime reentry are not resolved. No crash-cause exclusion or attribution follows.',
        'crash_cause':'UNATTRIBUTED','correction_implemented':False}
out=ROOT/'docs/features/v0.3.4-rc2-diagnostic/INVESTIGATION.json';out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({k:v for k,v in result.items() if k!='reachable_edges'},indent=2))
