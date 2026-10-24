#!/usr/bin/env python3
"""Enforce the approved Issue37 delta; freeze economy, routes and RC2 isolation."""
import hashlib,json,re,subprocess,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'verification/maintenance36'))
from source_tools import SOURCE,members,strip_literals
BASE='75e3623f608a05d1a2b59d12c243dbcdfc061358'
old=subprocess.check_output(['git','show',BASE+':'+str(SOURCE.relative_to(ROOT))],cwd=ROOT,text=True);new=SOURCE.read_text();a,b=members(old),members(new)
allowed=set('ConfigureArrivalPoint ConfigureRoutePoint ConfigureVehicleParkingBay ConfigureWorkBay CustomerOrder DrawPlacementMarkers EditingPointPurpose ExecuteCustomerSelection ExecuteEmployeeSettingsSelection ExecuteMarkerSelection ExecutePlacementSelection ExecutePointEditorSelection ExecutePointListSelection ExecuteRouteDirectionSelection ExecuteRoutePointSelection ExecuteSelection ExecuteVehicleAutoLotSelection GetMenuItemValue GetMenuItems GetMenuTitle GetPageDescription GetSelectedDescription GlassHeroesBusinessV3Experimental HideBusinessMenuForPause MechanicRouteCounts MechanicRouteHeading MechanicRoutePreview MenuPage NavigateBack OnKeyDown OpenPlacementMenu PollController ProcessTeleportOrder SetShopLocationFromPlayer TeleportTargetAvailable ToggleBusinessShift UpdateOneCustomerOrder UpdateVehicleApproach UpdateVehicleParking'.split())
added=set('ApplyPlacementChange BeginDirectionFreeHandoff BuildPlacementChange CancelPlacementConfirmation CommitPlacementRequest CreatePlacementBackup DrawComputedBayPreviews DrawPlacementVisual DropPlacementConfirmation EncodePlacementPatch ExecutePlacementResetSelection FocusedPlacementPoint HandoffProgress HandoffZoneAvailable IssueDirectionFreeHandoff LatestPlacementBackup LoadPlacementDisplayPreferences MarkCustomerHandoffZone OpenMarkerDisplay PatchPlacementIni PlacementBackupFiles PlacementBackupRoot PlacementBool PlacementCategoryFor PlacementCategoryVisible PlacementChange PlacementChangeFitsStorage PlacementChangeWithinShop PlacementDisplayMode PlacementFloat PlacementHash PlacementInstalledSource PlacementOperationBlock PlacementOverrides PlacementPointFinite PlacementRequest PlacementRequestDescription PlacementScope PlacementValue PlacementVisual PlacementVisuals ReadPlacementIni RejectPlacementReparsePath SetPlacementDisplay StartPlacementRequest TryCommitPlacementBytes UpdateDirectionFreeHandoff ValidatePlacementRequest VerifyPlacementBackup'.split())
changed={n for n in a if a[n]!=b[n]};assert changed<=allowed,(changed-allowed)
assert set(b)-set(a)==added and not set(a)-set(b),'Unapproved additions/removals'
tokens=lambda t:re.sub(r'\s+','',strip_literals(t)[0])
for name in ['ConfigureArrivalPoint','ConfigureRoutePoint','ConfigureVehicleParkingBay','ConfigureWorkBay']:
    previous='\n'.join(line for line in a[name].splitlines() if '_showPlacementMarkers = true;' not in line)
    assert tokens(previous)==tokens(b[name]),name+' changed beyond visibility/notice text'
for name in ['MechanicRouteCounts','MechanicRouteHeading','MechanicRoutePreview','ExecutePointListSelection','ExecuteRouteDirectionSelection','ExecuteVehicleAutoLotSelection']:
    assert tokens(a[name])==tokens(b[name]),name+' changed beyond labels'
assert a['ExecuteRoutePointSelection'].split('        GTA.UI.Screen.ShowSubtitle')[0]==b['ExecuteRoutePointSelection'].split('        GTA.UI.Screen.ShowSubtitle')[0],'Saved mask mutation changed'
assert b['GlassHeroesBusinessV3Experimental'].replace('            LoadPlacementDisplayPreferences();\n','')==a['GlassHeroesBusinessV3Experimental'],'Business loading/defaults changed'
for name in ['OnKeyDown','PollController','HideBusinessMenuForPause','OpenPlacementMenu']:
    assert tokens(b[name].replace('DropPlacementConfirmation();',''))==tokens(a[name]),name+' input/caller behavior changed beyond cancelling review'
assert b['NavigateBack'].replace('        if (_menuPage == MenuPage.PlacementResetReview || _menuPage == MenuPage.PlacementResetConfirm) { CancelPlacementConfirmation(); return; }\n','')==a['NavigateBack']
assert b['ToggleBusinessShift'].replace('        if (!_clockedIn && _placementRecoveryRequired) { Notify("Reload after the interrupted placement cleanup before opening the shop."); return; }\n','')==a['ToggleBusinessShift']
prefix='            case CustomerStage.DrivingToDropoff:';tail='            case CustomerStage.ExitingVehicle:'
assert a['UpdateOneCustomerOrder'].split(prefix)[0]==b['UpdateOneCustomerOrder'].split(prefix)[0]
assert a['UpdateOneCustomerOrder'].split(tail)[1]==b['UpdateOneCustomerOrder'].split(tail)[1],'Unrelated customer states changed'
assert b['UpdateVehicleApproach'].replace('        if (!toBay && DistanceSquared(order.Vehicle.Position, dest.Position) < parkingDistanceSquared)\n        { BeginDirectionFreeHandoff(order, worker, true); return; }\n','')==a['UpdateVehicleApproach'],'Service bay/drive recovery changed'
assert b['UpdateVehicleParking'].replace('        if (!atBay) { UpdateDirectionFreeHandoff(order, true); return; }\n','')==a['UpdateVehicleParking'],'Service bay parking changed'
assert b['ProcessTeleportOrder'].replace('if (!pickup) order.Vehicle.Heading = target.Heading;','order.Vehicle.Heading = target.Heading;')==a['ProcessTeleportOrder'],'Teleport transfer/idempotence changed'
assert b['TeleportTargetAvailable'].replace('(!pickup && (float.IsNaN(target.Heading) || float.IsInfinity(target.Heading)))','float.IsNaN(target.Heading) || float.IsInfinity(target.Heading)')==a['TeleportTargetAvailable'],'Teleport clearance changed'
# Original customer mode/accounting/blip actions remain untouched; row7 opens display UI.
assert a['ExecuteCustomerSelection'].split('            case 7:')[0]==b['ExecuteCustomerSelection'].split('            case 7:')[0]
assert b['ExecuteEmployeeSettingsSelection'].replace('case 3: OpenMarkerDisplay(); break;','case 3: _menuPage = MenuPage.PlacementMarkers; _selection = 0; break;')==a['ExecuteEmployeeSettingsSelection']
def arms(body):
    clean,_=strip_literals(body);marks=list(re.finditer(r'^            case (\d+):',clean,re.M))
    return {m[1]:body[m.start():marks[i+1].start() if i+1<len(marks) else len(body)] for i,m in enumerate(marks)}
previous,current=arms(a['ExecutePointEditorSelection']),arms(b['ExecutePointEditorSelection'])
assert previous['0'].replace('                if (_editingPoint.IsSet)\n                    _showPlacementMarkers = true; // SET always enables saved-point preview\n','')==current['0']
for case in ['1','3','4']:assert previous[case]==current[case],'Individual point mutation changed'
# All other original bytes (including original field defaults/ledger/save schema)
# are immutable outside the reviewed methods and two clearly bounded new blocks.
restored=new
for name in changed:restored=restored.replace(b[name],a[name],1)
start=restored.index('    private sealed class PlacementValue');end=restored.index('    private void DrawPlacementMarkers()',start)
restored=restored[:start]+restored[end:]
start=restored.index('    private sealed class HandoffProgress');end=restored.index('    private void UpdateVehicleApproach(',start)
restored=restored[:start]+restored[end:]
restored=restored.replace('using System.Linq;\n','',1)
assert restored==old,'Original declarations or source outside approved scope changed'
assert 'CustomerVehicleTrace' not in new and 'RC2 VEHICLE' not in new,'RC2 crash instrumentation mixed into Issue37'
assert '_showPlacementMarkers = true' not in new,'Point save forces marker visibility'
assert 'GTA.Native' not in b['PlacementOperationBlock'] and 'World.' not in b['PlacementOperationBlock']
assert b['PlacementOverrides'].count('add("Shop"')==4 and b['PlacementOverrides'].count('add("Equipment"')==3
for forbidden in ['"Business"','"Storage"','"SupplierWarehousePickup"','"JobOutboundMask"','"JobReturnMask"']:
    assert forbidden not in b['PlacementOverrides'] and forbidden not in b['PlacementScope'],'Nonspatial reset scope'
for path in ['AGENTS.md','Install/scripts/GlassHeroesBusiness.2.cs','VisualStudio/scripts/GlassHeroesBusiness.2.cs','VisualStudio/GlassHeroesBusiness.csproj','VisualStudio/GlassHeroesBusiness.sln','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.sln']:
    assert (ROOT/path).read_bytes()==subprocess.check_output(['git','show',BASE+':'+path],cwd=ROOT),path
report={'base':BASE,'base_source_sha256':hashlib.sha256(old.encode()).hexdigest(),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'changed_members':sorted(changed),'added_members':sorted(added),'unchanged_original_members':len(a)-len(changed),'other_source_bytes_and_original_defaults_unchanged':True,'economy_finance_stock_supplier_receipts_and_active_time_bodies_unchanged':True,'ai_walking_geometry_and_saved_mask_mutation_unchanged':True,'service_bay_and_queue_orientation_preserved':True,'original_business_save_and_settings_schema_unchanged':True,'new_preferences':'PlacementDisplay.v1.ini only','reset_whitelist':'selected point spatial fields, affected resume posts, optional Shop anchor and Equipment placement flags only','rc2_instrumentation_included':False,'gta_testing':'NOT RUN','owner_implementation_approval':'YES — explicit pre-approval in chat','owner_final_acceptance':'PENDING YES/NO'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2));print('PASS approved Issue37 scope and protected invariants')
