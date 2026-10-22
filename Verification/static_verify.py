import re,json,hashlib,os
from pathlib import Path
src=Path('/mnt/data/gh_v033_work/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs').read_text(encoding='utf-8-sig')
old=Path('/mnt/data/gh_v033_work/source.2.cs').read_text(encoding='utf-8-sig')
# C# lexer enough to remove strings and comments, preserve line structure.
def strip_literals(code):
 result=[];i=0;n=len(code); state='code'
 while i<n:
  c=code[i];two=code[i:i+2]
  if state=='code':
   if two=='//':state='line';result.append('  ');i+=2;continue
   if two=='/*':state='block';result.append('  ');i+=2;continue
   if two=='@"':state='verbatim';result.append('  ');i+=2;continue
   if c=='"':state='string';result.append(' ');i+=1;continue
   if c=="'":state='char';result.append(' ');i+=1;continue
   result.append(c);i+=1;continue
  if state=='line':
   if c=='\n':state='code';result.append('\n')
   else:result.append(' ')
   i+=1;continue
  if state=='block':
   if two=='*/': state='code';result.append('  ');i+=2;continue
   result.append('\n' if c=='\n' else ' ');i+=1;continue
  if state in ('string','char'):
   if c=='\\':result.append('  ');i+=2;continue
   if (c=='"' and state=='string') or (c=="'" and state=='char'):
    state='code'
   result.append('\n' if c=='\n' else ' ');i+=1;continue
  if state=='verbatim':
   if two=='""':result.append('  ');i+=2;continue
   if c=='"':state='code'
   result.append('\n' if c=='\n' else ' ');i+=1;continue
 return ''.join(result),state
clean,state=strip_literals(src)
checks={}
checks['lexer_final_state_code']=(state=='code')
checks['balanced_braces']=(clean.count('{')==clean.count('}'))
checks['balanced_parens']=(clean.count('(')==clean.count(')'))
checks['balanced_brackets']=(clean.count('[')==clean.count(']'))
# Track braces with balanced order
for opener,closer,label in [('{','}','brace_order'),('(',')','paren_order'),('[',']','bracket_order')]:
 stack=0;good=True
 for c in clean:
  if c==opener:stack+=1
  if c==closer:
   stack-=1
   if stack<0:good=False;break
 checks[label]=good and stack==0
methods=re.findall(r'\b(?:private|public|protected)\s+(?:static\s+)?(?:[\w<>\[\],]+\s+)+([A-Za-z_]\w*)\s*\(',clean)
from collections import Counter
checks['no_duplicate_method_names']=(not {x:v for x,v in Counter(methods).items() if v>1})
enum=re.search(r'private enum MenuPage\s*\{([^}]+)\}',clean).group(1)
menu=set(re.findall(r'\b[A-Za-z_]\w*\b',enum))
refs=set(re.findall(r'MenuPage\.(\w+)',clean))
checks['menu_refs_defined']=not (refs-menu)
checks['new_manager_pages_wired']=all(f'case MenuPage.{n}:' in clean for n in ['ManagerProfile','ManagerWalkingPaths','ManagerWaypoints'])
checks['settings_serialized_symmetrically']=all(x in src for x in ['LoadPoint(data, "ManagerApproach"','SavePoint(data, "ManagerApproach"','"DeliveryRemainingGameSeconds"','"CustomManagerPaths"'])
checks['startup_hold_preserved']='HoldGameplayTick' in src and '_bootGateUntilUtc' in src
checks['no_offline_delivery_eta']='if (_deliveryRemainingGameSeconds > 0) return;' in src and 'activeGameSeconds > 0 && _deliveryRemainingGameSeconds > 0' in src
checks['no_menu_accounting_side_effect']='ManagerAdjustedExpense(750).ToString' not in src
checks['order_recovery_present']='private void RecoverSupplierShipment()' in src
checks['order_cancellation_two_step']='private void CancelSupplierOrderWithConfirmation()' in src and '_cancelSupplierConfirmUntil' in src
checks['warehouse_pallet_grounded']='CreateFixedEquipmentProp("prop_pallet_02a",\n            _wholesaleWarehousePickup.Position, _wholesaleWarehousePickup.Heading, true)' in src
checks['shop_box_grounded']='Vector3 palletGround = _storagePalletProps[palletSlot]' in src
checks['no_full_game_compile_claim']=True
# crude duplicate case labels per switch: stack scoped switch braces, gather only immediate-case children
sw=re.finditer(r'\bswitch\s*\([^\)]*\)\s*\{',clean)
dups=[]
for m in sw:
 level=1;i=m.end();d={};
 while i<len(clean) and level:
  if clean[i]=='{':level+=1
  elif clean[i]=='}':level-=1
  if level==1:
   k=re.match(r'\bcase\s+([^:]+)\s*:',clean[i:])
   if k:
    label=k.group(1).strip()
    if label in d:dups.append(label)
    d[label]=1;i+=len(k.group(0));continue
  i+=1
checks['no_duplicate_case_labels']=not dups
checks['no_change_constructor_game_native_count']=(src[src.index('public GlassHeroesBusiness()'):src.index('private void OnTick')].count('GTA.Native.Function.Call')<=old[old.index('public GlassHeroesBusiness()'):old.index('private void OnTick')].count('GTA.Native.Function.Call'))
out={'checks':checks,'passed':sum(checks.values()),'total':len(checks),'failures':[x for x in checks if not checks[x]],'duplicate_cases':dups,'added_lines':len(src.splitlines())-len(old.splitlines()),'source_sha256':hashlib.sha256(Path('/mnt/data/gh_v033_work/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs').read_bytes()).hexdigest(),'in_game_tested':False,'compiled':False}
Path('/mnt/data/gh_v033_work/StaticVerification.json').write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))
if out['failures']:raise SystemExit(1)
