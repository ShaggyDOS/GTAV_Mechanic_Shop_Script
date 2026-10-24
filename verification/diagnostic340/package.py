#!/usr/bin/env python3
"""Create a source-only RC2 diagnostic owner-review ZIP and compile extraction."""
import argparse,hashlib,json,re,subprocess,tempfile,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='75e3623f608a05d1a2b59d12c243dbcdfc061358'
CODE='3b2ef09a96dda9f6d0325f08d29bc986167abe68'
REL='experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
DOC='docs/features/v0.3.4-rc2-diagnostic/'
p=argparse.ArgumentParser();p.add_argument('--output',type=Path,required=True);args=p.parse_args();out=args.output.resolve()
assert not out.is_relative_to(ROOT)
assert not subprocess.check_output(['git','status','--porcelain'],cwd=ROOT),'Commit all review files first'
branch=subprocess.check_output(['git','branch','--show-current'],cwd=ROOT,text=True).strip()
assert branch=='diagnostic/v0.3.4-rc2-mechanic-transitions'
tip=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip()
source=(ROOT/REL).read_bytes();sha=hashlib.sha256(source).hexdigest()
validation=json.loads((ROOT/DOC/'final-validation/validation.json').read_text())
assert sha==validation['source_sha256'] and validation['behavior']['total']==985 and validation['build']['errors']==0
assert source==subprocess.check_output(['git','show',CODE+':'+REL],cwd=ROOT)
commits=[]
for line in subprocess.check_output(['git','log','--reverse','--format=%H %s',BASE+'..'+tip],cwd=ROOT,text=True).splitlines():
    commit,title=line.split(' ',1)
    files=subprocess.check_output(['git','diff-tree','--no-commit-id','--name-status','-r',commit],cwd=ROOT,text=True).splitlines()
    commits.append({'sha':commit,'title':title,'changed_files':files})
changed=subprocess.check_output(['git','diff','--name-status',BASE,tip],cwd=ROOT,text=True).splitlines()
handoff={'package_identity':'Glass Heroes Business Manager v0.3.4 RC2 Diagnostic Experimental','diagnostic_only':True,
         'branch':branch,'final_commit':tip,'instrumentation_commit':CODE,'parent_rc1_approval_commit':BASE,
         'verified_rc1_source_commit':'e312d8c9500f4a990eb993b82e8e76b55711d3de',
         'rc1_source_sha256':'70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120',
         'rc2_source_sha256':sha,'commits':commits,'changed_files':changed,'release_build':'PASS — 0 errors; existing CS0169/CS0649 warnings',
         'inherited_rc1_behavior':'954/954 PASS','rc2_behavior_navigation':'985/985 PASS','static':'21/21 PASS','inventory_structure':'13/13 PASS',
         'exact_rc1_restoration_and_fixture_preservation':'PASS','gta_testing':'NOT RUN','windows_execution':'NOT RUN','crash_cause':'UNATTRIBUTED',
         'crash_correction_implemented':False,'owner_review':'PENDING','live_gta_or_owner_data_modified':False,'merge_performed':False,
         'save_compatibility':'No save/settings keys/defaults/schema/migration or financial/stock/AI/route/active-clock change; new diagnostics write only their two bounded log files.',
         'archive_layout':'Install/scripts + VisualStudio map directly from the matching experimental/shvdn3 paths; Docs contains new RC2 documents/evidence. Full regression reproduction requires the Git checkout.'}
text='# Glass Heroes Business Manager v0.3.4 RC2 Diagnostic Experimental\n\nDIAGNOSTIC-ONLY. OWNER REVIEW PENDING. GTA testing NOT RUN. Crash cause UNATTRIBUTED.\n\n'
text+=f'Branch: {branch}\n\nFinal commit: {tip}\n\nInstrumentation commit: {CODE}\n\nVerified RC1 source: '+handoff['verified_rc1_source_commit']+'\n\nRC1 SHA256: '+handoff['rc1_source_sha256']+'\n\nRC2 SHA256: '+sha+'\n\n'
text+='Individual commits:\n\n'+'\n'.join('- '+c['sha']+' — '+c['title'] for c in commits)+'\n\n'
text+='Actual Release 0 errors / existing CS0169, CS0649 warnings. RC1 954/954; RC2 985/985 behavior/navigation; static 21/21; inventory structure 13/13; exact-restoration preservation PASS. Logs and Word/PDF are under Docs. PACKAGE_RECEIPT.json adjacent to the archive records all extracted hashes and clean compilation.\n\n'
text+=handoff['save_compatibility']+'\n\nNo crash fix, main/RC1 merge, live installation or v0.3.5 work. Use DIAGNOSTIC_INSTRUCTIONS, GTA_TEST_CHECKLIST and ROLLBACK from Docs for later owner-run testing. Preserve both trace segments and crash/dump/host evidence.\n\n'
text+='Complete changed-file list relative to the approval-only RC1 parent:\n\n```text\n'+'\n'.join(changed)+'\n```\n\nPer-commit file lists are in FINAL_HANDOFF.json. This receipt is generated after the final documentation commit and does not change the branch tip.\n'
readme='''# Glass Heroes Business Manager v0.3.4 RC2 Diagnostic Experimental

DIAGNOSTIC-ONLY — OWNER REVIEW PENDING. GTA testing NOT RUN. Crash cause UNATTRIBUTED. No crash fix is claimed.

Read FINAL_HANDOFF.md for exact branch/commits/checksums/results. Docs/ contains the Word/PDF developer guide, diagnostic capture/interpretation, changelog, GTA checklist, rollback, complete source diff and actual test evidence.

Install/scripts/GlassHeroesBusiness.3.cs is the ONLY installation source. VisualStudio/ has the matching linked project/solution. No runtime DLL or business Save.ini/Settings.ini is supplied. Never copy the entire archive into GTA. Codex has not deployed or merged this candidate. Follow Docs/INSTALL.md only for a later owner-run test, using complete matching backups and copied business data.

Clean source build: dotnet build VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release

Use .NET SDK 8 tooling, net48 references and the pinned official SHVDN3 3.6.0 NuGet package. Compilation does not verify the owner's Enhanced/GTA/mod stack. Tests included under verification/ are reference/reproduction sources, not installation scripts. Full inherited regression/preservation reproduction requires the exact Git branch/history at its repository layout; follow Docs/DEVELOPER_GUIDE.md. Historical baseline sources are inert .txt fixtures. No earlier runtime source is supplied.

New bounded logs after normal boot activation: scripts/GlassHeroesBusiness/CustomerVehicle.RC2.log and .log.previous. See Docs/DIAGNOSTIC_INSTRUCTIONS.md; missing/unmatched breadcrumbs cannot alone attribute a fatal crash. Preserve exact source/package hashes, pre/post full data, both trace segments, Errors.log, host logs and any dump.
'''
tracked=subprocess.check_output(['git','ls-files'],cwd=ROOT,text=True).splitlines()
contents={}
for path in tracked:
    if path.startswith(DOC):contents['Docs/'+path[len(DOC):]]=(ROOT/path).read_bytes()
    elif path.startswith('verification/'):contents[path]=(ROOT/path).read_bytes()
for path in ['BASELINE_SHA256.txt','Verification/static_verify.py','AGENTS.md']:
    contents[path]=(ROOT/path).read_bytes()
for name in ['LICENSE','LICENSE.md','LICENSE.txt']:
    if (ROOT/name).is_file():contents[name]=(ROOT/name).read_bytes()
contents['Install/scripts/GlassHeroesBusiness.3.cs']=source
for name in ['GlassHeroesBusiness.V3.Experimental.csproj','GlassHeroesBusiness.V3.Experimental.sln']:
    contents['VisualStudio/'+name]=(ROOT/'experimental/shvdn3/VisualStudio'/name).read_bytes()
fixture='GlassHeroesBusiness_v0.3.3.0_RC1.2.cs'
contents['verification/baselines/'+fixture+'.txt']=(ROOT/fixture).read_bytes()
contents['FINAL_HANDOFF.md']=text.encode();contents['FINAL_HANDOFF.json']=(json.dumps(handoff,indent=2)+'\n').encode()
contents['READ_ME_FIRST.md']=readme.encode();contents['SOURCE_SHA256.txt']=(sha+'  Install/scripts/GlassHeroesBusiness.3.cs\n').encode()
assert [n for n in contents if n.endswith('.3.cs')]==['Install/scripts/GlassHeroesBusiness.3.cs']
assert not any(Path(n).name.lower() in ['save.ini','settings.ini'] or n.endswith(('.dll','.2.cs')) for n in contents)
manifest={name:hashlib.sha256(data).hexdigest() for name,data in sorted(contents.items())}
contents['PACKAGE_MANIFEST.json']=(json.dumps({'final_commit':tip,'source_sha256':sha,'sha256':manifest},indent=2)+'\n').encode()
out.mkdir(parents=True,exist_ok=True)
archive=out/'Glass_Heroes_Business_Manager_v0.3.4_RC2_Diagnostic_Experimental.zip'
assert not archive.exists(),'Preserve existing archives; choose another output folder'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for name,data in sorted(contents.items()):z.writestr(name,data)
with tempfile.TemporaryDirectory(prefix='gh-rc2-extract-') as directory:
    directory=Path(directory)
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None;z.extractall(directory)
    for name,digest in manifest.items():assert hashlib.sha256((directory/name).read_bytes()).hexdigest()==digest,name
    with tempfile.TemporaryDirectory(prefix='gh-rc2-clean-build-') as build:
        result=subprocess.run(['dotnet','build',str(directory/'VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj'),'-c','Release',
            '--property:BaseIntermediateOutputPath='+build+'/obj/','--property:OutputPath='+build+'/bin/','--maxcpucount:2','--verbosity','minimal'],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
        (out/'EXTRACTED_RELEASE_BUILD.txt').write_text(result.stdout)
        assert result.returncode==0,result.stdout
receipt={'archive':archive.name,'archive_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'branch':branch,'final_commit':tip,'instrumentation_commit':CODE,
         'rc2_source_sha256':sha,'manifest_files_verified':len(manifest),'zip_integrity':'PASS','extracted_clean_release_build':'PASS','errors':0,
         'warnings':sorted(set(re.findall(r'warning (CS\d+)',result.stdout))), 'behavior_navigation':'985/985 PASS on the byte-identical product source',
         'static':'21/21 PASS','inventory_structure':'13/13 PASS','gta_testing':'NOT RUN','crash_cause':'UNATTRIBUTED','diagnostic_only':True,'owner_review':'PENDING'}
(out/'PACKAGE_RECEIPT.json').write_text(json.dumps(receipt,indent=2)+'\n')
(out/'FINAL_HANDOFF.md').write_text(text);(out/'FINAL_HANDOFF.json').write_text(json.dumps(handoff,indent=2)+'\n')
(out/'SHA256SUMS.txt').write_text(receipt['archive_sha256']+'  '+archive.name+'\n'+sha+'  Install/scripts/GlassHeroesBusiness.3.cs\n')
print(json.dumps(receipt,indent=2))
