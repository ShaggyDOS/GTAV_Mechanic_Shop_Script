#!/usr/bin/env python3
"""Create an exact-tip review ZIP outside Git; verify extraction and clean build."""
import argparse
import hashlib
import json
import re
import subprocess
import tempfile
import zipfile
from pathlib import Path

from source_tools import BASE, ROOT, SOURCE

p = argparse.ArgumentParser()
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
out = a.output.resolve()
assert not out.is_relative_to(ROOT), 'Write package outside the Git worktree'
assert not subprocess.check_output(['git', 'status', '--porcelain'], cwd=ROOT), 'Commit review files first'
tip = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
branch = subprocess.check_output(['git', 'branch', '--show-current'], cwd=ROOT, text=True).strip()
assert branch == 'maintenance/v0.3.3.6-issue36'
sha = hashlib.sha256(SOURCE.read_bytes()).hexdigest()
docroot = 'docs/features/v0.3.3.6-issue36/'
v = json.loads((ROOT/docroot/'D-canonical-roles/validation.json').read_text())
assert v['source_sha256'] == sha and v['behavior_total'] == 854
changed = subprocess.check_output(['git', 'diff', '--name-status', BASE, tip], cwd=ROOT, text=True).splitlines()
commits = []
for line in subprocess.check_output(['git', 'log', '--reverse', '--format=%H %s', BASE+'..'+tip], cwd=ROOT, text=True).splitlines():
    commit, title = line.split(' ', 1)
    paths = subprocess.check_output(['git', 'diff-tree', '--no-commit-id', '--name-status', '-r', commit], cwd=ROOT, text=True).splitlines()
    commits.append({'sha':commit, 'title':title, 'changed_files':paths})
handoff = {'branch':branch, 'final_commit':tip, 'base_commit':BASE, 'commits':commits, 'changed_files':changed,
           'source_sha256':sha, 'build':'PASS — net48/C#7.3, SHVDN3 3.6.0, SDK 8.0.426',
           'errors':0, 'warnings':['CS0169','CS0649'], 'behavior_navigation':'854/854 PASS',
           'static':'21/21 PASS', 'preservation':'PASS', 'gta_testing':'NOT RUN', 'windows_execution':'NOT RUN',
           'save_compatibility':'No key/default/schema changes; GetValue/SetValue expressions identical; no existing saves/settings accessed. Finances, inventory transactions, clocks, AI and saved route geometry unchanged.',
           'gate_1':'YES — owner chat', 'gate_2':'PENDING owner YES/NO',
           'integration_owner':'Separate v0.3.4 development instance, only after owner acceptance',
           'main_rc1_qa_r2_v034_or_live_gta_modified':False, 'published_release':False}
markdown = '# v0.3.3.6 Issue #36 — exact final handoff\n\nEXPERIMENTAL. Owner acceptance PENDING YES/NO. GTA testing NOT RUN.\n\n'
markdown += f'Branch: {branch}\n\nFinal commit: {tip}\n\nBase: {BASE}\n\nSource SHA256: {sha}\n\n'
markdown += 'Individual commits (ordered):\n\n'+'\n'.join('- '+c['sha']+' — '+c['title'] for c in commits)+'\n\n'
markdown += 'Actual clean Release: 0 errors; two inherited warnings CS0169/CS0649. 854/854 behavior/navigation; 21/21 static; preservation PASS. Net48/C#7.3, SHVDN3 3.6.0, SDK 8.0.426. Full evidence and Word/PDF in docs/features/v0.3.3.6-issue36. Archive extraction/hash/build receipt is adjacent to the ZIP.\n\n'
markdown += handoff['save_compatibility']+'\n\n'
markdown += 'Gate 1 YES; gate 2 PENDING owner YES/NO. No main/RC1/QA R2/v0.3.4 modification or merge. No live installation/data access. Separate v0.3.4 instance owns later integration after acceptance; review shared source overlaps instead of overwriting its source. Crash cause remains UNATTRIBUTED.\n\nComplete changed-file list relative to pinned RC1:\n\n```text\n'+'\n'.join(changed)+'\n```\n\nPer-commit file lists are in FINAL_HANDOFF.json. This external receipt was written after the final documentation commit to give an exact final tip.\n'
readme = '''# v0.3.3.6 Issue #36 review package — EXPERIMENTAL

Owner acceptance PENDING YES/NO. GTA testing NOT RUN. No deployment, published release or v0.3.4 integration. Read FINAL_HANDOFF.md for exact branch, final SHA, commits, changed files and results. Read the versioned Word/PDF developer guide and GTA checklist under docs/features/v0.3.3.6-issue36/.

Only experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs is installation source. The matching project links it directly. No compiled runtime DLL, Save.ini or Settings.ini is supplied. Tests under verification/ are not installation scripts. Never copy this entire archive into GTA. Installation/testing requires later owner authorization; preserve matching source and complete data/settings backups.

Clean product rebuild: dotnet build experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release

Use .NET SDK 8 tooling and NuGet connectivity for pinned SHVDN3 3.6.0/net48 references. Automated behavior tests run with deterministic .NET 8 adapters, not GTA. Full preservation verifier/report regeneration requires the Git branch and pinned base; archived raw evidence remains available here. Historical fixture source is .txt, not an active script. FINAL_HANDOFF.json lists all repository changes, including files not needed for installation.
'''
out.mkdir(parents=True, exist_ok=True)
archive = out/'GlassHeroesBusiness_v0.3.3.6_Issue36_Maintenance_REVIEW.zip'
assert not archive.exists(), 'Preserve existing packages; choose a new output directory'
tracked = subprocess.check_output(['git', 'ls-files'], cwd=ROOT, text=True).splitlines()
selected = [s for s in tracked if s.startswith(docroot) or s.startswith('verification/')]
selected += ['Verification/static_verify.py', 'BASELINE_SHA256.txt', 'AGENTS.md',
             'experimental/shvdn3/README.md', str(SOURCE.relative_to(ROOT)),
             'experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj',
             'experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.sln',
             'docs/features/v0.3.3.6/validation.json']
contents = {s:(ROOT/s).read_bytes() for s in selected}
fixture = 'GlassHeroesBusiness_v0.3.3.0_RC1.2.cs'
contents['verification/baselines/'+fixture+'.txt'] = (ROOT/fixture).read_bytes()
contents['FINAL_HANDOFF.md'] = markdown.encode()
contents['FINAL_HANDOFF.json'] = (json.dumps(handoff, indent=2)+'\n').encode()
contents['READ_ME_FIRST.md'] = readme.encode()
assert not any(Path(n).name.lower() in ['save.ini','settings.ini'] or n.endswith(('.dll','.2.cs')) for n in contents)
assert [n for n in contents if n.endswith('.3.cs')] == [str(SOURCE.relative_to(ROOT))]
manifest = {n:hashlib.sha256(data).hexdigest() for n, data in sorted(contents.items())}
contents['PACKAGE_MANIFEST.json'] = (json.dumps({'final_commit':tip, 'sha256':manifest}, indent=2)+'\n').encode()
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED) as z:
    for n, data in sorted(contents.items()):
        z.writestr(n, data)
with tempfile.TemporaryDirectory(prefix='gh36-archive-check-') as temp:
    temp = Path(temp)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None
        z.extractall(temp)
    for n, digest in manifest.items():
        assert hashlib.sha256((temp/n).read_bytes()).hexdigest() == digest, n
    with tempfile.TemporaryDirectory(prefix='gh36-archive-build-') as build:
        command = ['dotnet','build',str(temp/'experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj'),'-c','Release',
                   '--property:BaseIntermediateOutputPath='+build+'/obj/', '--property:OutputPath='+build+'/bin/', '--maxcpucount:2', '--verbosity','minimal']
        result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
        (out/'ARCHIVE_BUILD.txt').write_text(result.stdout)
        assert result.returncode == 0, result.stdout
receipt = {'archive':archive.name, 'archive_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),
           'final_commit':tip, 'source_sha256':sha, 'manifest_files_verified':len(manifest),
           'zip_integrity':'PASS', 'extracted_clean_release_build':'PASS', 'errors':0,
           'warnings':sorted(set(re.findall(r'warning (CS\d+)', result.stdout))),
           'behavior_navigation':'854/854 PASS on byte-identical source before docs commit', 'static':'21/21 PASS',
           'gta_testing':'NOT RUN', 'owner_acceptance':'PENDING YES/NO', 'live_installation_modified':False}
(out/'PACKAGE_RECEIPT.json').write_text(json.dumps(receipt, indent=2)+'\n')
(out/'FINAL_HANDOFF.md').write_text(markdown)
(out/'FINAL_HANDOFF.json').write_text(json.dumps(handoff, indent=2)+'\n')
(out/'SHA256SUMS.txt').write_text(receipt['archive_sha256']+'  '+archive.name+'\n')
print(json.dumps(receipt, indent=2))
