#!/usr/bin/env python3
"""Compile verbatim migrated members against deterministic native/model stubs."""
import ast,re,subprocess,tempfile
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
source=(repo/'experimental/shvdn3/VisualStudio/scripts/GlassHeroesBusiness.3.cs').read_text(encoding='utf-8-sig')
lexer=next(n for n in ast.parse((repo/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals')
ns={};exec(compile(ast.Module(body=[lexer],type_ignores=[]),'lexer','exec'),ns);clean,_=ns['strip_literals'](source)
def extract(name):
 m=re.search(r'^    (?:private|public)\s+[^\n=;]+\b'+name+r'\(',clean,re.M)
 assert m,name
 start=clean.index('{',m.start());end=start+1;depth=1
 while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
 return source[m.start():end]
program=re.sub(r'/\*<source:(\w+)>\*/',lambda m:extract(m[1]),(Path(__file__).parent/'runtime_tests.cs').read_text())
with tempfile.TemporaryDirectory(prefix='gh-v3-parity-') as temp:
 p=Path(temp);(p/'Tests.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>')
 (p/'Program.cs').write_text(program)
 subprocess.run(['dotnet','run','--project',str(p/'Tests.csproj'),'--configuration','Release'],check=True)
