#!/usr/bin/env python3
"""Reverse only reviewed API/lifecycle edits and require whole-source R2 equality."""
import ast,hashlib,json,re,sys
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
fixture=repo/'verification/migration/qa-r2-reference.cs.txt'
base=fixture.read_text(encoding='utf-8-sig')
assert hashlib.sha256(fixture.read_bytes()).hexdigest()=='72f9e58d51a54584a1141a67740a2c4154647f65624c5107b39fdb8530315445'
v3_path=repo/'experimental/shvdn3/VisualStudio/scripts/GlassHeroesBusiness.3.cs'
s=v3_path.read_text(encoding='utf-8-sig');original=s
api=json.loads((repo/'verification/migration/api-map.json').read_text())

def reverse(new,old,count=1):
 global s
 assert s.count(new)==count, ('Approved edit count changed',new[:90],s.count(new),count)
 s=s.replace(new,old)
reverse('v0.3.3.1 SHVDN3 Phase 1 / EXPERIMENTAL (Issue #20)','v0.3.3.1 QA R2 / EXPERIMENTAL (#1 diagnostics + #2-#7)')
reverse('public class GlassHeroesBusinessV3Experimental : Script','public class GlassHeroesBusiness : Script')
reverse('public GlassHeroesBusinessV3Experimental()','public GlassHeroesBusiness()')
reverse('BOOT v0.3.3.1 SHVDN3 PHASE1 EXPERIMENTAL (QA R2 parity / Issue #20)','BOOT v0.3.3.1 QA R2 EXPERIMENTAL (#1 diagnostics + #2-#7)')
reverse('    // Issue #20: retain only visual model requests owned by this v3 instance.\n    private readonly HashSet<Model> _requestedVisualModels = new HashSet<Model>();\n','')
reverse('            RequireExclusiveBusinessInstallation(File.Exists);\n','')
reverse('        new GTA.UI.ContainerElement(new PointF(CompactX(x), CompactY(y)),','        new UIRectangle(new Point(CompactX(x), CompactY(y)),')
reverse('Math.Max(1, (int)Math.Round(h * MenuScale))), color, false).Draw();','Math.Max(1, (int)Math.Round(h * MenuScale))), color).Draw();')
reverse('        new GTA.UI.TextElement(caption, new PointF(CompactX(x), CompactY(y)), scale * MenuScale, color,\n            GTA.UI.Font.ChaletLondon, GTA.UI.Alignment.Left).Draw();','        new UIText(caption, new Point(CompactX(x), CompactY(y)), scale * MenuScale, color,\n            GTA.Font.ChaletLondon, false).Draw();')
reverse('GTA.UI.Screen.ShowSubtitle(', 'UI.ShowSubtitle(',4)
reverse('GTA.UI.Notification.Show("~b~Glass Heroes~s~: " + message, false);','UI.Notify("~b~Glass Heroes~s~: " + message);')
reverse('order.CustomerBlip.Delete();','order.CustomerBlip.Remove();',2)
reverse('.IsPositionFrozen = ','.FreezePosition = ',7)
lexer=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals')
ns={};exec(compile(ast.Module(body=[lexer],type_ignores=[]),'lexer','exec'),ns)
def member(text,name):
 clean,_=ns['strip_literals'](text);m=re.search(r'^    private\s+[^\n=;]+\b'+name+r'\(',clean,re.M);assert m,name
 end=clean.index('{',m.start())+1;depth=1
 while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
 return text[m.start():end]
for name in ['PadJustPressed','PadPressed']:
 body=member(s,name)
 assert hashlib.sha256(body.encode()).hexdigest()==api['approved_method_sha256'][name],name
 reverse(body,member(base,name))
reverse('        bool deskReady = RequestVisualModel(deskModel);\n        bool laptopReady = RequestVisualModel(laptopModel);\n        if (!deskReady || !laptopReady) return;', '        deskModel.Request(250); laptopModel.Request(250);\n        if (!deskModel.IsLoaded || !laptopModel.IsLoaded) return;')
reverse('        finally { ReleaseVisualModel(deskModel); ReleaseVisualModel(laptopModel); }','        finally { deskModel.MarkAsNoLongerNeeded(); laptopModel.MarkAsNoLongerNeeded(); }')
reverse('        if (!RequestVisualModel(model)) return null;\n        try\n        {\n            Prop item = World.CreateProp', '        model.Request(250);\n        try\n        {\n            if (!model.IsLoaded) return null;\n            Prop item = World.CreateProp')
reverse('        finally { ReleaseVisualModel(model); }','        finally { model.MarkAsNoLongerNeeded(); }')
start=s.index('    // Do not yield into nested callbacks');end=s.index('    private void UpdateEquipmentVisuals()',start)
block=s[start:end];assert hashlib.sha256(block.encode()).hexdigest()==api['approved_helper_block_sha256']
s=s[:start]+s[end:]
reverse('        ReleasePendingVisualModels();\n','')
assert s==base,'v3 changed source outside the explicit API/lifecycle migration whitelist'
assert (repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_bytes()==v3_path.read_bytes(),'Install mismatch'
for p in ['VisualStudio/scripts/GlassHeroesBusiness.2.cs','Install/scripts/GlassHeroesBusiness.2.cs']:
 if (repo/p).exists(): assert (repo/p).read_bytes()==fixture.read_bytes(),p+' modified'
for p,expected in api['protected_v2_files_sha256'].items():
 if (repo/p).exists():assert hashlib.sha256((repo/p).read_bytes()).hexdigest()==expected,p+' changed'
project=(repo/'experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj').read_text()
assert 'ScriptHookVDotNet3" Version="3.6.0"' in project and 'ScriptHookVDotNet2' not in project and '.2.cs' not in project
assert '<AssemblyName>GlassHeroesBusiness.V3.Experimental</AssemblyName>' in project
for forbidden in ['ForensicProbe','ForensicsEnabled','WholesalePackageBlip','.Request(250)','Script.Wait(']:assert forbidden not in original,forbidden
for op in ['GetValue','SetValue']:
 keys=lambda text:re.findall(r'\.'+op+r'\("([^"]+)",\s*"([^"]+)"',text)
 assert keys(base)==keys(original),op+' key/order/schema drift'
unchanged=['OnTick','ProcessEconomy','ProcessRemoteOperations','EnterRemoteOperations','LeaveRemoteOperations','Save','CommitSupplierDelivery','CancelSupplierOrderWithConfirmation','RecoverSupplierShipment','FindAssignedWholesaleVehicle']
for name in unchanged:assert member(base,name)==member(original,name),name+' drift'
result={'whole_source_matches_r2_after_explicit_api_reversal':True,'r2_sha256':hashlib.sha256(fixture.read_bytes()).hexdigest(),'v3_sha256':hashlib.sha256(v3_path.read_bytes()).hexdigest(),'unchanged_finance_clock_save_methods':unchanged,'save_and_config_schema_order_preserved':True,'v2_source_project_solution_unchanged':True,'install_source_equal':True,'unrelated_pr18_pr19_pr22_features_absent':True,'new_gta_testing':'NOT RUN','scope':'source parity, not GTA persistence or native API execution'}
if len(sys.argv)>1:Path(sys.argv[1]).write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2));print('PASS whole-source migration parity')
