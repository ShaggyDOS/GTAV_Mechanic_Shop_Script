#!/usr/bin/env python3
"""Run the existing static checks with only their machine-specific paths redirected."""
import ast
import sys
import hashlib
from pathlib import Path
import subprocess
import tempfile

repo = Path(__file__).resolve().parents[1]
output = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path(tempfile.gettempdir()) / 'glassheroes-static-results'
output.mkdir(exist_ok=True)
source = repo / 'VisualStudio/scripts/GlassHeroesBusiness.2.cs'
rc1 = repo / 'GlassHeroesBusiness_v0.3.3.0_RC1.2.cs'
if not rc1.exists(): rc1 = repo / 'verification/baselines/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs.txt'
with tempfile.TemporaryDirectory(prefix='glassheroes-static-') as tmp:
    baseline = Path(tmp) / 'source.2.cs'
    baseline.write_text(rc1.read_text(encoding='utf-8-sig'), encoding='utf-8')
    subprocess.run(['patch', '--batch', '--fuzz=0', '--reverse', str(baseline),
                    '--input', str(repo / 'SourceDiff_v0.3.2.9a_to_v0.3.3.0_RC1.patch')], check=True)
    baseline.write_bytes(b'\xef\xbb\xbf' + baseline.read_bytes())
    expected = (repo / 'BASELINE_SHA256.txt').read_text().splitlines()[0].split()[-1]
    if hashlib.sha256(baseline.read_bytes()).hexdigest() != expected:
        raise SystemExit('Reconstructed baseline does not match BASELINE_SHA256.txt')
    mapping = {
        '/mnt/data/gh_v033_work/GlassHeroesBusiness_v0.3.3.0_RC1.2.cs': str(source),
        '/mnt/data/gh_v033_work/source.2.cs': str(baseline),
        '/mnt/data/gh_v033_work/StaticVerification.json': str(output / 'StaticVerification.json'),
    }
    class Paths(ast.NodeTransformer):
        def visit_Constant(self, node):
            if isinstance(node.value, str) and node.value in mapping:
                return ast.copy_location(ast.Constant(value=mapping[node.value]), node)
            return node
    verifier = repo / 'Verification/static_verify.py'
    tree = ast.fix_missing_locations(Paths().visit(ast.parse(verifier.read_text(), filename=str(verifier))))
    exec(compile(tree, str(verifier), 'exec'), {'__name__': '__main__'})
