#!/usr/bin/env python3
"""Read-only source/compiled-call boundary checks for the hash-confirmed financial build."""
import ast,hashlib,json,re,subprocess
from pathlib import Path
repo=Path(__file__).resolve().parents[2];src='experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs';candidate=(repo/src).read_text(encoding='utf-8-sig');s=(repo/'verification/economy/startup-parent.cs.txt').read_text(encoding='utf-8-sig')
base=(repo/'verification/economy/finance-parent.cs.txt').read_text(encoding='utf-8-sig')
lex=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals');ns={};exec(compile(ast.Module(body=[lex],type_ignores=[]),'lexer','exec'),ns)
def member(t,name):
 clean,_=ns['strip_literals'](t);m=re.search(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?'+name+r'\s*\(',clean,re.M);assert m,name;end=clean.index('{',m.start())+1;depth=1
 while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
 return t[m.start():end]
old=member(base,'GlassHeroesBusinessV3Experimental');new=member(s,'GlassHeroesBusinessV3Experimental');restored=new.replace('            LoadFinanceBook(data);\n','').replace('BOOT v0.3.3.3 SHVDN3 FINANCIAL TRANSPARENCY EXPERIMENTAL / constructor entered','BOOT v0.3.3.1 SHVDN3 ECONOMY + PALLETS + WAGES + SAVED QUOTES EXPERIMENTAL (#27, #25) / constructor entered');assert old==restored
checks=['constructor delta is BOOT label plus one LoadFinanceBook call']
for name in ['OnKeyDown','OnKeyUp','OnAborted']:
 assert member(base,name)==member(s,name),name;checks.append(name+' unchanged from supplier parent')
end='            _diagnosticTickStage = "game clock";';assert member(base,'OnTick').split(end)[0]==member(s,'OnTick').split(end)[0];checks.append('pre-activation OnTick gate unchanged')
assert new.index('LoadFinanceBook(data)')<new.index('Tick += OnTick')<new.index('constructor completed')
for name in ['LoadFinanceBook','CaptureFinanceSnapshot','ReadFinanceSnapshot','MechanicWagesDue']:
 text=member(s,name)
 for bad in ['Game.','World.','GTA.Native','Notify(','Save(','Request(','Function.Call','Task.']:
  assert bad not in text,(name,bad)
checks.append('financial initialization helpers contain no game/world/native/save/notification/model-request calls')
assert 'count > FinanceBook.Retained' in member(s,'LoadFinanceBook') and 'public const int Retained = 256;' in s
checks.append('journal row count explicitly bounded at 256 before row loop')
sha=hashlib.sha256((repo/'verification/economy/startup-parent.cs.txt').read_bytes()).hexdigest();assert sha=='84125a3465adc717b21984d3e95cdf4c1faed77426eda979a90cf7a3a4917fe5'
assert 'new Model(' not in member(candidate,'GlassHeroesBusinessV3Experimental')
assert member(candidate,'OnTick').index('TryInitializeGameplayModels') > member(candidate,'OnTick').index('readyPlayer == null')
assert member(candidate,'OnTick').replace('                if (!TryInitializeGameplayModels(DateTime.UtcNow)) return;\n','') == member(s,'OnTick')
checks.append('FS1 defers seven named models; all downstream Tick/economy ordering unchanged')
out={'source_sha256':sha,'base':'422baa756ce16f6af370278105731de2e82abdd3','financial_candidate':'0fefcf8798da54b53fae505f488d3b3d41fae985','checks':checks,'source_changed':True,'candidate_source_sha256':hashlib.sha256((repo/src).read_bytes()).hexdigest(),'gta_execution':'NOT RUN','native_crash_cause':'UNATTRIBUTED','limits':'Source boundary checks and separate PE call audit do not prove runtime safety of the entire mod stack.'}
print(json.dumps(out,indent=2))
