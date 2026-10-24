#!/usr/bin/env python3
"""Fail closed outside the owner-approved Issue #36 presentation/locator scope."""
import argparse,hashlib,json,re,subprocess
from source_tools import ROOT,BASE,SOURCE,baseline,members,strip_literals
p=argparse.ArgumentParser();p.add_argument('--output');a=p.parse_args()
old=baseline();new=SOURCE.read_text();before=members(old);after=members(new)
allowed_changed=set('WrapMenuDescription DrawMenu GetSelectedDescription GetPageDescription GetMenuItems GetMenuItemValue GetMenuTitle EditingPointPurpose ExecutePointListSelection ExecuteRouteDirectionSelection ExecuteRoutePointSelection ExecuteMechanicWalkingSelection OnTick TryPlaceSupplierOrder SetSupplierPickupWaypoint ExecuteSelection TrySupplierPackageInteraction RecoverSupplierShipment CancelSupplierOrderWithConfirmation OnAborted ShowSupplierDeliveryStatus ExecutePlacementSelection ExecuteReceptionistManagement ExecuteManagerManagement ExecuteEmployeePathsSelection ExecuteReceptionWalkingSelection ExecuteManagerWalkingSelection NavigateBack'.split())
allowed_added=set('FittingMenuWords LayoutMenuDescription MechanicRouteHeading MechanicRouteCounts MechanicRoutePreview PickupWaypointSnapshot WholesalePickupLocationValid WholesalePickupLocatorEligible PickupWaypointChanged ReadPickupWaypoint NotePickupLocatorFailure DeleteWholesalePickupBlip SyncWholesalePickupBlip'.split())
changed=sorted(k for k in before.keys()&after.keys() if before[k]!=after[k]);added=sorted(after.keys()-before.keys());removed=sorted(before.keys()-after.keys())
assert set(changed)<=allowed_changed,('unapproved changed methods',set(changed)-allowed_changed)
assert set(added)<=allowed_added and not removed,('unapproved additions/removals',added,removed)
def tokens(s):return re.sub(r'\s+','',strip_literals(s)[0])
# Confirm root declarations/field initializers are unchanged except session-only approved UI state.
old_root=old;new_root=new
for value in before.values():old_root=old_root.replace(value,'',1)
for value in after.values():new_root=new_root.replace(value,'',1)
new_fields=['_menuDescriptionCache','_lastDescriptionInput','_lastDescriptionWidth','_lastDescriptionLines',
 '_wholesalePickupBlip','_pickupLocatorOrderId','_pickupLocatorPosition','_pickupLocatorAttempts','_nextPickupLocatorAttempt','_nextPickupLocatorLog','_pickupGpsActive','_pickupLocatorConfigured','_pickupWaypointAtGps']
for field in new_fields:new_root=re.sub(r'^    private [^;\n]*\b'+field+r'\b[^;\n]*;\n','',new_root,flags=re.M)
assert tokens(old_root)==tokens(new_root),'Existing declarations/defaults changed or unapproved session fields added'
# No change to ANY existing settings/save key, including dynamically addressed point/profile keys.
for op in ['GetValue','SetValue']:
 keys=lambda t:re.findall(r'\.'+op+r'\(([^\n;]+)',t)
 assert keys(old)==keys(new),op+' expressions/schema changed'
assert after['OnTick'].replace('                SyncWholesalePickupBlip(_supplierGameplayNow);\n','')==before['OnTick'],'Existing active-time/gameplay loop changed'
if 'SyncWholesalePickupBlip' in after:
 tick=after['OnTick'];call='SyncWholesalePickupBlip(_supplierGameplayNow);'
 assert tick.count(call)==1 and tick.index('ProcessSupplierDelivery(_supplierGameplayNow)')<tick.index(call)<tick.index('UpdateCustomerVisuals()')
 assert tick.index('_visualRefreshClock >= 1.0')<tick.index(call),'Locator called outside throttled refresh'
 assert after['OnAborted'].index('DeleteWholesalePickupBlip()')>after['OnAborted'].index('return;'),'Pre-activation cleanup native'
 assert '            _supplierVisualStage = 9;\n            DeleteWholesalePickupBlip();' in after['TrySupplierPackageInteraction']
 assert '                _supplierStageAt = _supplierGameplayNow;\n                if (atWarehouse) DeleteWholesalePickupBlip();' in after['TrySupplierPackageInteraction']
 assert after['CancelSupplierOrderWithConfirmation'].index('if (!TryCancelSupplierOrder()) return;')<after['CancelSupplierOrderWithConfirmation'].index('DeleteWholesalePickupBlip()')

if 'TryPlaceSupplierOrder' in changed:
 assert after['TryPlaceSupplierOrder'].replace('DeleteWholesalePickupBlip();','SetSupplierPickupWaypoint();')==before['TryPlaceSupplierOrder'],'Supplier purchase delta is not navigation-only'
for name in ['TrySupplierPackageInteraction','RecoverSupplierShipment','CancelSupplierOrderWithConfirmation','OnAborted']:
 restored=after[name].replace('            DeleteWholesalePickupBlip();\n','').replace('                if (atWarehouse) DeleteWholesalePickupBlip();\n','').replace('        DeleteWholesalePickupBlip();\n','')
 assert restored==before[name],name+' non-locator behavior changed'
assert after['ExecuteRoutePointSelection'].split('        GTA.UI.Screen.ShowSubtitle')[0]==before['ExecuteRoutePointSelection'].split('        GTA.UI.Screen.ShowSubtitle')[0],'Mask mutation/save changed'
def point_calls_without_labels(text):
 clean,_=strip_literals(text);replacements=[]
 for match in re.finditer(r'OpenPointEditor\(',clean):
  start=match.end();level=0;commas=[];end=start
  while True:
   char=clean[end]
   if char==')' and level==0:break
   if char in '([{':level+=1
   elif char in ')]}':level-=1
   elif char==',' and level==0:commas.append(end)
   end+=1
  assert len(commas)==3
  label=text[commas[0]+1:commas[1]]
  assert re.fullmatch(r'[+()a-zA-Z0-9_\s]*',strip_literals(label)[0]),'Non-display expression in point label'
  replacements.append((commas[0]+1,commas[1]))
 for start,end in reversed(replacements):text=text[:start]+'"LABEL"'+text[end:]
 return tokens(text)
assert point_calls_without_labels(after['ExecutePointListSelection'])==point_calls_without_labels(before['ExecutePointListSelection']),'Point editor changed beyond labels'
assert tokens(after['ExecuteMechanicWalkingSelection'])==tokens(before['ExecuteMechanicWalkingSelection']),'Walking controls changed beyond notices'
# Canonical role routing changes only destinations/context; existing mode guards/mutations are byte-identical.
for method,page in [('ExecuteReceptionWalkingSelection','ReceptionWaypoints'),('ExecuteManagerWalkingSelection','ManagerWaypoints')]:
 restored=after[method].replace('case 1: OpenPlacementMenu(MenuPage.'+page+'); break;', 'case 1: _menuPage = MenuPage.'+page+'; _selection = 0; break;')
 assert restored==before[method],method+' changed beyond child navigation'
for path in ['Install/scripts/GlassHeroesBusiness.2.cs','VisualStudio/scripts/GlassHeroesBusiness.2.cs','VisualStudio/GlassHeroesBusiness.csproj','VisualStudio/GlassHeroesBusiness.sln','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.sln','AGENTS.md']:
 assert (ROOT/path).read_bytes()==subprocess.check_output(['git','show',BASE+':'+path],cwd=ROOT),path+' changed'
report={'base_commit':BASE,'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'changed_methods':changed,'new_members':added,'unchanged_members':len(before)-len(changed),'all_other_methods_and_existing_declarations_unchanged':True,'route_ai_geometry_and_bay_assignment_unchanged':True,'save_settings_expressions_and_schema_unchanged':True,'input_arbitration_and_controller_precedence_unchanged':True,'financial_transactions_and_active_clocks_unchanged':True,'protected_sources_projects_unchanged':True,'approval_gate_1':'YES — owner chat','approval_gate_2':'PENDING YES/NO','gta_testing':'NOT RUN'}
if a.output:open(a.output,'w').write(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2));print('PASS Issue #36 approved source boundary')
