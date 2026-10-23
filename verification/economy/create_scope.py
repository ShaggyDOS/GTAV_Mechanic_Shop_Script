#!/usr/bin/env python3
"""Record explicit, reviewed method boundaries for a new focused candidate."""
import argparse,ast,hashlib,json,re
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('name');p.add_argument('--fixture',required=True);p.add_argument('--parent',required=True);p.add_argument('--changed',nargs='+',required=True);args=p.parse_args()
lex=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals');ns={};exec(compile(ast.Module(body=[lex],type_ignores=[]),'lexer','exec'),ns)
def members(s):
 clean,_=ns['strip_literals'](s);out={}
 for m in re.finditer(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?(\w+)\s*\(',clean,re.M):
  end=clean.index('{',m.start())+1;depth=1
  while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
  out[m[1]]=s[m.start():end]
 return out
base=Path(__file__).parent;parent=(base/args.fixture).read_text(encoding='utf-8-sig');current=(repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_text(encoding='utf-8-sig');before=members(parent);after=members(current)
changed=sorted(n for n in before.keys()&after.keys() if before[n]!=after[n]);assert changed==sorted(args.changed),(changed,sorted(args.changed));assert not before.keys()-after.keys()
profile={'parent':args.parent,'fixture':args.fixture,'parent_sha256':hashlib.sha256((base/args.fixture).read_bytes()).hexdigest(),'changed':changed,'added':sorted(after.keys()-before.keys()),'added_keys':{},'protected_v2':json.loads((base/'scopes/repair.json').read_text())['protected_v2']}
for op in ['GetValue','SetValue']:
 keys=lambda t:set(re.findall(r'\.'+op+r'\("([^"]+)",\s*"([^"]+)"',t));assert keys(parent)<=keys(current);profile['added_keys'][op]=sorted([list(x) for x in keys(current)-keys(parent)])
(base/'scopes'/f'{args.name}.json').write_text(json.dumps(profile,indent=2)+'\n');print(json.dumps(profile,indent=2))
