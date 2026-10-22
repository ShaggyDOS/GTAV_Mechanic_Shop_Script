#!/usr/bin/env python3
"""Compile tests against extracted production members; GTA-facing APIs are stubbed."""
import ast
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile

repo = Path(__file__).resolve().parents[1]
source = (repo / 'VisualStudio/scripts/GlassHeroesBusiness.2.cs').read_text(encoding='utf-8-sig')
lexer_tree = ast.parse((repo / 'Verification/static_verify.py').read_text())
lexer = next(n for n in lexer_tree.body if isinstance(n, ast.FunctionDef) and n.name == 'strip_literals')
namespace = {}
exec(compile(ast.Module(body=[lexer], type_ignores=[]), 'lexer', 'exec'), namespace)
clean, _ = namespace['strip_literals'](source)

def extract(name):
    pattern = r'^    (?:private|public|internal|protected)\s+(?:[^\n=;]+\s+)?' + re.escape(name) + r'\s*(?:\(|\{|\n)'
    matches = list(re.finditer(pattern, clean, re.M))
    if len(matches) != 1:
        raise RuntimeError(f'Expected one production member {name}, got {len(matches)}')
    start = matches[0].start()
    opening = clean.index('{', start)
    depth = 1
    end = opening + 1
    while depth:
        depth += (clean[end] == '{') - (clean[end] == '}')
        end += 1
    return source[start:end]

if not shutil.which('dotnet'):
    raise SystemExit('Activate .NET 8 SDK (cloud: source /workspace/.cloud-setup/env.sh).')
issues = sys.argv[1:] or [p.stem.split('_')[0][5:] for p in sorted((repo/'verification').glob('issue*_tests.cs'))]
for issue in issues:
    template = (repo / f'verification/issue{issue}_tests.cs').read_text()
    generated = re.sub(r'/\*<source:(\w+)>\*/', lambda m: extract(m.group(1)), template)
    with tempfile.TemporaryDirectory(prefix=f'glassheroes-issue{issue}-') as tmp:
        path = Path(tmp)
        (path/'Tests.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>')
        (path/'Program.cs').write_text(generated)
        subprocess.run(['dotnet', 'run', '--project', str(path/'Tests.csproj'), '--configuration', 'Release'], cwd=path, check=True)
