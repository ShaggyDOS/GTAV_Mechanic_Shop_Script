#!/usr/bin/env python3
"""Compile actual production menu members/case arms with side-effect adapters; no GTA."""
import ast,re,subprocess,tempfile,json
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
s=(repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_text()
lexer=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals');ns={};exec(compile(ast.Module(body=[lexer],type_ignores=[]),'lexer','exec'),ns);clean,_=ns['strip_literals'](s)
def member(name):
 m=re.search(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?'+name+r'\s*(?:\(|\{)',clean,re.M);assert m,name
 end=clean.index('{',m.start())+1;depth=1
 while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
 return s[m.start():end]
def cases(name,pages):
 text=member(name);cleaned,_=ns['strip_literals'](text);marks=list(re.finditer(r'^            (?:case MenuPage\.(\w+):|default:)',cleaned,re.M));out=''
 for i,m in enumerate(marks):
  if m[1] in pages:out+=text[m.start():marks[i+1].start() if i+1<len(marks) else text.rfind('        }')]
 return out
text=(Path(__file__).parent/'navigation_tests.cs').read_text()
text=re.sub(r'/\*<source:(\w+)>\*/',lambda m:member(m[1]),text)
text=re.sub(r'/\*<cases:(\w+):([\w,]+)>\*/',lambda m:cases(m[1],m[2].split(',')),text)
with tempfile.TemporaryDirectory(prefix='gh-issue30-nav-') as t:
 p=Path(t);(p/'Tests.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><NoWarn>CS0169;CS0649;CS0414</NoWarn></PropertyGroup></Project>');(p/'Program.cs').write_text(text)
 result=subprocess.run(['dotnet','run','--project',str(p/'Tests.csproj'),'-c','Release'],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
 print(result.stdout);raise SystemExit(result.returncode)
