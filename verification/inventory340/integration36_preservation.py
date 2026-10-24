#!/usr/bin/env python3
"""Verify the reviewed union of accepted Issue36 and the pinned v0.3.4 source."""
import hashlib,json,re,subprocess,sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'maintenance36'))
from source_tools import ROOT,SOURCE,members,strip_literals
BASE='c8617aed5cda492214bbbe1907042bcafbfa0dc1'
PRE='c77591b10437564814c873203e073cc4ead1901e'
ACCEPTED='2f1ebc268fe93d3a1c84a810ad87a01cf9235774'
REL=str(SOURCE.relative_to(ROOT))
def read(rev):return subprocess.check_output(['git','show',rev+':'+REL],cwd=ROOT).decode('utf-8-sig')
base,pre,approved=map(read,[BASE,PRE,ACCEPTED]);current=SOURCE.read_text()
assert hashlib.sha256(pre.encode()).hexdigest()=='fbc8f7f963bbffa3e31c097bf34250fef5807d28dd5de3b7d75463146beb3251'
assert hashlib.sha256(approved.encode()).hexdigest()=='7ba7b6021eead8f1e972b92a382a477045df083ea76b0148b84124e8725ef2ca'
b,p,a,c=map(members,[base,pre,approved,current])
maintenance_changed={n for n in b.keys()&a.keys() if b[n]!=a[n]}
v4_changed={n for n in b.keys()&p.keys() if b[n]!=p[n]}
actual_changed={n for n in p.keys()&c.keys() if p[n]!=c[n]}
assert actual_changed==maintenance_changed,('Unexpected integration changes',actual_changed^maintenance_changed)
assert c.keys()-p.keys()==a.keys()-b.keys(),'Unexpected/lost added helper'
assert not p.keys()-c.keys(),'v0.3.4 member removed'
for n in p.keys()-maintenance_changed:assert c[n]==p[n],n+' v0.3.4 implementation changed'
for n in a.keys()-b.keys():assert c[n]==a[n],n+' accepted helper changed'
for n in maintenance_changed-v4_changed:assert c[n]==a[n],n+' does not match accepted maintenance'
# Merge overlapping menu arms, not whole source files. Only these three arms
# need a semantic union; every other arm follows its unchanged/approved owner.
menu_names={'GetMenuItems','GetMenuItemValue','GetMenuTitle','GetSelectedDescription','GetPageDescription','ExecuteSelection','NavigateBack'}
reviewed_arms={('GetMenuItems','ActiveDeliveries'),('GetMenuItemValue','ActiveDeliveries'),('GetSelectedDescription','ActiveDeliveries')}
def arms(text):
 clean,_=strip_literals(text);marks=list(re.finditer(r'^            (?:case MenuPage\.(\w+):|default:)',clean,re.M));assert marks
 result={m[1] or '<default>':text[m.start():marks[i+1].start() if i+1<len(marks) else len(text)].strip() for i,m in enumerate(marks)}
 return text[:marks[0].start()],result
for n in menu_names:
 prefixes,sets=zip(*(arms(m[n]) for m in [b,p,a,c]));bp,pp,ap,cp=prefixes
 assert cp==(ap if ap!=bp else pp),n+' prefix lost'
 bs,ps,ads,cs=sets
 assert set(cs)==set(ps)|set(ads),n+' menu arms removed'
 for page,value in cs.items():
  if (n,page) in reviewed_arms:continue
  baseline=bs.get(page);ours=ps.get(page);theirs=ads.get(page)
  if theirs!=baseline and ours!=baseline:assert ours==theirs==value,(n,page,'unexpected overlapping arm')
  else:assert value==(theirs if theirs!=baseline else ours),(n,page,'menu arm lost')
assert c['GetMenuItems'].count('"Order / Last Receipt"')==1
assert '"Set Warehouse GPS", "< Back to Inventory"' in c['GetMenuItems']
assert 'SupplierLiveState()' in c['GetMenuItemValue'] and 'WholesalePickupLocatorEligible() ? "READY"' in c['GetMenuItemValue']
assert 'last durable receipt/cancellation' in c['GetSelectedDescription'] and 'Route to READY warehouse cargo only' in c['GetSelectedDescription']
# These overlaps may add locator cleanup only, after existing durable guards.
for n in ['TrySupplierPackageInteraction','RecoverSupplierShipment','CancelSupplierOrderWithConfirmation']:
 restored=c[n].replace('            DeleteWholesalePickupBlip();\n','').replace('                if (atWarehouse) DeleteWholesalePickupBlip();\n','').replace('        DeleteWholesalePickupBlip();\n','')
 assert restored==p[n],n+' altered transaction/ownership behavior'
load=c['TrySupplierPackageInteraction'];assert load.index('if (!TryCommitWholesaleCargo(true))')<load.index('DeleteWholesalePickupBlip()')
status_block='''        if (WholesalePickupLocatorEligible())
            Notify("READY warehouse cargo: Active Deliveries > Set Warehouse GPS. Collection stays player-operated.");
        else if (_deliverySupplier == 0 && _deliveryRemainingGameSeconds <= 0 && !WholesalePickupLocationValid())
            Notify("Warehouse pickup location unavailable. Check Supplier Settings before collecting.");
'''
assert c['ShowSupplierDeliveryStatus'].replace(status_block,'')==p['ShowSupplierDeliveryStatus'],'Identified order/status lost'
# Save/read expressions, finance/receipt/repair/route data remain identical to v4.
for op in ['GetValue','SetValue']:
 expressions=lambda t:re.findall(r'\.'+op+r'\(([^\n;]+)',t)
 assert expressions(pre)==expressions(current),op+' schema/default expression changed'
assert c['OnTick'].replace('                SyncWholesalePickupBlip(_supplierGameplayNow);\n','')==p['OnTick'],'Active-time loop changed'
# Root declarations/defaults change only by accepted session-only locator/footer state.
old_root,new_root=pre,current
for value in p.values():old_root=old_root.replace(value,'',1)
for value in c.values():new_root=new_root.replace(value,'',1)
fields=['_menuDescriptionCache','_lastDescriptionInput','_lastDescriptionWidth','_lastDescriptionLines','_wholesalePickupBlip','_pickupLocatorOrderId','_pickupLocatorPosition','_pickupLocatorAttempts','_nextPickupLocatorAttempt','_nextPickupLocatorLog','_pickupGpsActive','_pickupLocatorConfigured','_pickupWaypointAtGps']
for field in fields:new_root=re.sub(r'^    private [^;\n]*\b'+field+r'\b[^;\n]*;\n','',new_root,flags=re.M)
tokens=lambda text:re.sub(r'\s+','',strip_literals(text)[0])
assert tokens(old_root)==tokens(new_root),'Unapproved defaults/fields change'
protected=json.loads((ROOT/'verification/economy/scopes/workflow336.json').read_text())['protected_v2']
for path,sha in protected.items():assert hashlib.sha256((ROOT/path).read_bytes()).hexdigest()==sha,path
report={'preintegration_commit':PRE,'accepted_maintenance_final_commit':ACCEPTED,'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'maintenance_changed_members':sorted(actual_changed),'accepted_added_members':sorted(a.keys()-b.keys()),'unchanged_v034_members':len(p.keys()-maintenance_changed),'reviewed_menu_arm_unions':[list(x) for x in sorted(reviewed_arms)],'v034_finance_inventory_receipt_and_quote_methods_unchanged':True,'saved_key_expressions_and_defaults_unchanged':True,'active_time_sampling_unchanged':True,'tick_delta':'one accepted locator sync at existing throttled visual cadence','protected_v2_files':protected,'gta_testing':'NOT RUN','integration_acceptance':'PENDING OWNER YES/NO'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2));print('PASS accepted maintenance + v0.3.4 semantic-union preservation')
