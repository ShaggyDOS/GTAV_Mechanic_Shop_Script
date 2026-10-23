#!/usr/bin/env python3
"""Compile and exercise the actual embedded logger without loading GTA natives."""
import os
import argparse
from pathlib import Path
import shutil
import subprocess
import tempfile

repo = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--source', type=Path, default=repo / 'VisualStudio/scripts/GlassHeroesBusiness.2.cs')
args = parser.parse_args()
source = args.source.read_text(encoding='utf-8-sig')
start = source.index('    // BEGIN ISSUE1 DIAGNOSTIC LOG')
end = source.index('    // END ISSUE1 DIAGNOSTIC LOG', start)
logger = source[start:end].replace('private sealed class DiagnosticLog', 'sealed class DiagnosticLog', 1)
# Only accessibility changes: all runtime logger code compiles verbatim in this harness.
dotnet = shutil.which('dotnet')
if not dotnet:
    raise SystemExit('Activate the .NET 8 SDK first (in cloud: source /workspace/.cloud-setup/env.sh).')
with tempfile.TemporaryDirectory(prefix='glassheroes-diagnostics-') as tmp:
    root = Path(tmp)
    (root / 'DiagnosticsTests.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework>
<EnableDefaultCompileItems>true</EnableDefaultCompileItems></PropertyGroup></Project>''')
    (root / 'Program.cs').write_text('using System;\nusing System.Collections.Generic;\n' + logger + '\n' +
                                   (repo / 'verification/diagnostics_tests.cs').read_text())
    subprocess.run([dotnet, 'run', '--project', str(root / 'DiagnosticsTests.csproj'),
                    '--configuration', 'Release'], check=True, cwd=root)
