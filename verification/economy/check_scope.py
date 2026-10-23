#!/usr/bin/env python3
"""Enforce a reviewed method-level change boundary and protect v2/game clocks."""
import argparse,ast,hashlib,json,re
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('profile');p.add_argument('--output',type=Path);args=p.parse_args()
profile=json.loads((Path(__file__).parent/'scopes'/f'{args.profile}.json').read_text())
parent=(Path(__file__).parent/profile['fixture']).read_text(encoding='utf-8-sig')
current=(repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_text(encoding='utf-8-sig')
assert hashlib.sha256((Path(__file__).parent/profile['fixture']).read_bytes()).hexdigest()==profile['parent_sha256']
lex=next(x for x in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(x,ast.FunctionDef) and x.name=='strip_literals');ns={};exec(compile(ast.Module(body=[lex],type_ignores=[]),'lexer','exec'),ns)
def methods(text):
 clean,_=ns['strip_literals'](text);out={}
 for m in re.finditer(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?(\w+)\s*\(',clean,re.M):
  opening=clean.index('{',m.start());end=opening+1;depth=1
  while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
  assert m[1] not in out,m[1];out[m[1]]=text[m.start():end]
 return out
before=methods(parent);after=methods(current)
changed=sorted(n for n in before.keys()&after.keys() if before[n]!=after[n]);added=sorted(after.keys()-before.keys());removed=sorted(before.keys()-after.keys())
assert changed==profile['changed'],('Out-of-scope method change',changed,profile['changed'])
assert added==profile['added'] and not removed,('Unexpected helpers/removal',added,removed)
for name in ['OnTick','ProcessSupplierDelivery','TrySupplierPackageInteraction','CommitSupplierDelivery','UpdateWholesalePalletVisual','RecoverSupplierShipment','CancelSupplierOrderWithConfirmation']:
 if name not in profile['changed']:assert before[name]==after[name],name+' changed'
if 'Save' in profile['changed'] and 'TryCommitBusinessSave' in profile['added']:
 old_payload=before['Save'][before['Save'].index('            data.SetValue'):before['Save'].index('            if (!data.Save())')]
 new_payload=after['Save'][after['Save'].index('            data.SetValue'):after['Save'].index('            if (!data.Save())')]
 for key in ['PartsSalesRevenue','PartsSalesUnitsSold']:
  new_payload=re.sub(r'^            data.SetValue\("Business", "'+key+r'", [^\n]+\n','',new_payload,flags=re.M)
 assert new_payload==old_payload,'Existing save value or point payload changed'
if args.profile=='final':
 restored=after['TrySupplierPackageInteraction'].replace('            RefreshStoragePalletAfterDeposit();','            _nextStorageVisualAttempt = DateTime.MinValue;\n            UpdateSupplierPalletVisual();')
 assert restored==before['TrySupplierPackageInteraction'],'Nonvisual supplier interaction changed'
 assert after['TrySupplierPackageInteraction'].index('CommitSupplierDelivery(slot)')<after['TrySupplierPackageInteraction'].index('RefreshStoragePalletAfterDeposit()')
if args.profile=='reload':
 old_save=before['Save'];new_save=after['Save'].replace('            WriteRepairPromises(data);\n','')
 assert old_save==new_save,'Existing save snapshot payload changed'
 assert after['GlassHeroesBusinessV3Experimental'].index('LoadRepairPromises(data)')<after['GlassHeroesBusinessV3Experimental'].index('Tick += OnTick')
 assert after['OnAborted'].replace('        ParkRuntimeRepairPromises(); // Preserve fixed accepted data before entity cleanup.\n','')==before['OnAborted']
 assert after['CancelCustomerOrder'].index('RememberCancelledRepair(order)')<after['CancelCustomerOrder'].index('mechanic.WorkSeconds = 0')
 assert after['AssignCustomerMechanic'].index('if (!Save())')<after['AssignCustomerMechanic'].index('BeginVehicleWalk(')
 for method,audit in [('ProcessOneCustomerRepair','RecordCustomerInvoice'),('ProcessRemoteOperations','AppendRemoteInvoice'),('ProcessEconomy','RecordRepairPricing')]:
  assert after[method].index('TrySettleRepair(')<after[method].index(audit+'(')
 assert after['TrySettleRepair'].index('if (!Save())')<after['TrySettleRepair'].index('quote.CommitRetryPending = false')
 fields=lambda name,op:set(re.findall(r'data\.'+op+r'\(section, "([^\"]+)"',after[name]))
 assert fields('LoadRepairPromises','GetValue')==fields('WriteRepairPromises','SetValue'), 'Promise serializer field mismatch'
for op in ['GetValue','SetValue']:
 keys=lambda t:set(re.findall(r'\.'+op+r'\("([^"]+)",\s*"([^"]+)"',t))
 old=keys(parent);new=keys(current);assert old<=new,op+' removed a key'
 assert sorted([list(x) for x in new-old])==profile['added_keys'][op],(op,new-old)
for pth,sha in profile['protected_v2'].items():assert hashlib.sha256((repo/pth).read_bytes()).hexdigest()==sha,pth+' modified'
for bad in ['ForensicProbe','ForensicsEnabled','WholesalePackageBlip','GlassHeroesV3Probe']:assert bad not in current,bad
assert 'private const string SaveFile = @"scripts\\GlassHeroesBusiness\\Save.ini";' in current
assert 'private const string SettingsFile = @"scripts\\GlassHeroesBusiness\\Settings.ini";' in current
if args.profile == 'supplier':
 restored = after['OnTick'].replace('            _supplierGameplayNow = _supplierGameplayNow.AddSeconds(activeGameSeconds);\n', '').replace('ProcessSupplierDelivery(_supplierGameplayNow)', 'ProcessSupplierDelivery(now)').replace('DeferSupplierPlayerHandoff(_supplierGameplayNow)', 'DeleteSupplierVisuals()')
 assert restored == before['OnTick'], 'Existing gameplay clock sampling or system ordering changed'
 assert before['Save'] == after['Save'], 'Existing atomic save snapshot changed'
 assert before['TryCommitBusinessSave'] == after['TryCommitBusinessSave'], 'Atomic persistence boundary changed'
else:
 assert before['OnTick']==after['OnTick'],'Active game-time sampling changed'
result={'profile':args.profile,'parent':profile['parent'],'parent_source_sha256':profile['parent_sha256'],'candidate_source_sha256':hashlib.sha256((repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_bytes()).hexdigest(),'changed_methods':changed,'new_helpers':added,'unchanged_methods':len(before)-len(changed),'clocks_and_startup_game_gate_unchanged':True,'original_v2_source_project_unchanged':True,'supplier_receipt_accounting_unchanged':args.profile != 'supplier','out_of_scope_forensics_and_blip_features_absent':True,'gta_testing':'NOT RUN'}
if args.output:args.output.write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2));print('PASS scope and preservation checks')
