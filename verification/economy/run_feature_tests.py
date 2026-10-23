#!/usr/bin/env python3
"""Compile verbatim production members against deterministic finance/world stubs."""
import ast,re,subprocess,tempfile,sys
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
s=(repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs').read_text(encoding='utf-8-sig')
lexer=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals');ns={};exec(compile(ast.Module(body=[lexer],type_ignores=[]),'lexer','exec'),ns);clean,_=ns['strip_literals'](s)
def member(name):
 matches=list(re.finditer(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?'+name+r'\s*(?:\(|\{)',clean,re.M));assert len(matches)==1,(name,len(matches))
 m=matches[0];end=clean.index('{',m.start())+1;depth=1
 while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
 return s[m.start():end]
for suite in sys.argv[1:] or ['repair']:
 template=(Path(__file__).parent/(suite+'_tests.cs')).read_text()
 text=re.sub(r'/\*<source:(\w+)>\*/',lambda m:member(m[1]),template)
 with tempfile.TemporaryDirectory(prefix='gh-'+suite+'-') as temp:
  p=Path(temp);(p/'Tests.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><NoWarn>CS0649;CS0169;CS0414</NoWarn></PropertyGroup></Project>');(p/'Program.cs').write_text(text)
  subprocess.run(['dotnet','run','--project',str(p/'Tests.csproj'),'-c','Release'],check=True)
