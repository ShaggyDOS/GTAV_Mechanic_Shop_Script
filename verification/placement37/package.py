#!/usr/bin/env python3
"""Prepare Issue37 experimental review ZIP, verify every hash and build extraction."""
import argparse,hashlib,json,re,subprocess,tempfile,zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='75e3623f608a05d1a2b59d12c243dbcdfc061358';CODE='f6b90560304033ca538b585e61f850ee50863e10'
SRC='experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs';DOC='docs/features/issue37-placement-parking/'
p=argparse.ArgumentParser();p.add_argument('--output',type=Path,required=True);a=p.parse_args();out=a.output.resolve()
assert not out.is_relative_to(ROOT) and not subprocess.check_output(['git','status','--porcelain'],cwd=ROOT),'Use a clean committed checkout and external package directory'
branch=subprocess.check_output(['git','branch','--show-current'],cwd=ROOT,text=True).strip();assert branch=='feature/shvdn3-37-placement-parking'
tip=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip()
source=(ROOT/SRC).read_bytes();sha=hashlib.sha256(source).hexdigest();v=json.loads((ROOT/DOC/'final-validation/validation.json').read_text())
assert sha==v['source_sha256'] and v['behavior']['total']==1057 and v['build']['errors']==0
assert source==subprocess.check_output(['git','show',CODE+':'+SRC],cwd=ROOT)
commits=[]
for line in subprocess.check_output(['git','log','--reverse','--format=%H %s',BASE+'..'+tip],cwd=ROOT,text=True).splitlines():
    commit,title=line.split(' ',1);files=subprocess.check_output(['git','diff-tree','--no-commit-id','--name-status','-r',commit],cwd=ROOT,text=True).splitlines()
    commits.append({'sha':commit,'title':title,'changed_files':files})
changed=subprocess.check_output(['git','diff','--name-status',BASE,tip],cwd=ROOT,text=True).splitlines()
handoff={'identity':'Glass Heroes v0.3.4 Issue37 Placement & Parking Experimental','branch':branch,'final_commit':tip,'code_checkpoint':CODE,
         'base_commit':BASE,'verified_rc1_source_commit':'e312d8c9500f4a990eb993b82e8e76b55711d3de','rc1_source_sha256':'70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120',
         'source_sha256':sha,'commits':commits,'changed_files':changed,'release_build':'PASS — net48/C#7.3, SHVDN3 3.6.0, SDK8.0.426, 0 errors/2 inherited warnings',
         'behavior_navigation':'1057/1057 PASS','static':'21/21 PASS','inventory_structure':'13/13 PASS','preservation':'PASS',
         'features':['A direction-free customer handoff tasks','B physical walking coordinates versus mechanic use','C persisted marker modes/filters','D protected geometry reset/restore'],
         'save_compatibility':'No business Save.ini/Settings.ini schema/default migration. Point edits leave masks/AI geometry unchanged. Confirmed reset/restore changes only selected existing spatial keys/resume posts/optional anchor/equipment placement flags. Finances, inventory, personnel, ownership, receipts and fixed warehouse pickup preserved.',
         'new_file':'PlacementDisplay.v1.ini — UI-only preferences, created by explicit display changes',
         'owner_implementation_approval':'YES — explicit pre-approval in current chat','owner_final_acceptance':'PENDING YES/NO',
         'rc2_combined_or_modified':False,'rc2_checkpoint':'e7998af4db57b2a337f0e3e8ea3deb7a51478924',
         'integration_conflicts':['UpdateOneCustomerOrder','UpdateVehicleApproach','UpdateVehicleParking','shared menu/point/marker helpers; future integration requires separate authorization and complete revalidation'],
         'gta_testing':'NOT RUN','windows_execution':'NOT RUN','crash_cause':'UNATTRIBUTED','crash_fix_claimed':False,'live_gta_or_owner_data_modified':False,'merge_or_public_release':False}
text='# Issue37 Placement & Parking Experimental — exact handoff\n\nOwner implementation pre-approved. FINAL ACCEPTANCE PENDING YES/NO. GTA testing NOT RUN.\n\n'
text+=f'Branch: {branch}\n\nFinal commit: {tip}\n\nCode checkpoint: {CODE}\n\nBase: {BASE}\n\nSource SHA256: {sha}\n\n'
text+='Individual commits (ordered):\n\n'+'\n'.join('- '+c['sha']+' — '+c['title'] for c in commits)+'\n\n'
text+='A–D complete: position-only drop-off tasks with stable safe handoff; plain physical walking points versus per-mechanic masks; persistent filtered marker modes/caches; category/full reset and scope restore with two default-NO confirmations, complete verified backup and atomic spatial-only commit.\n\n'
text+='Actual Release 0 errors/two inherited warnings; 1057/1057 behavior/navigation, static21/21, inventory structure13/13 and preservation PASS. Evidence, source diff and Word/PDF are under Docs. Independent extraction/hash/build receipt is adjacent to the ZIP.\n\n'+handoff['save_compatibility']+'\n\n'
text+='RC2 remains isolated at '+handoff['rc2_checkpoint']+'. No merge into RC2, stable RC1 or main, live installation, public release or crash-fix claim. Use an isolated test profile and disposable complete data; do not overwrite active RC2 diagnostics. Future integration conflicts include the three transfer methods and shared menu/marker helpers and require separate authorization/revalidation.\n\n'
text+='Complete changed-file list relative to pinned RC1 parent:\n\n```text\n'+'\n'.join(changed)+'\n```\n\nPer-commit file lists are in FINAL_HANDOFF.json. This external receipt is written after the final documentation commit so its branch-tip SHA is exact.\n'
readme='''# Glass Heroes v0.3.4 Issue37 Placement & Parking Experimental

A–D implementation complete; FINAL OWNER ACCEPTANCE PENDING YES/NO. GTA testing NOT RUN. This feature variant is separate from active RC2 diagnostics and is not a crash fix or v0.3.5.

Read FINAL_HANDOFF.md for exact branch/final SHA/individual commits/changed files/results. Docs/ contains the versioned Word/PDF developer guide, changelog, actual final test evidence, complete source diff, installation/rollback and GTA test checklist.

Only Install/scripts/GlassHeroesBusiness.3.cs is installation source. VisualStudio/ links that exact source. No runtime DLL, Save.ini, Settings.ini or template preference INI is supplied. Do not copy the whole ZIP into GTA. Protect active RC2/stable RC1; test only in an isolated copy/profile with disposable complete business data after owner review. No live installation or merge was performed by Codex.

Rebuild: dotnet build VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj -c Release

Use .NET8 tooling and pinned SHVDN3 3.6.0/net48 references. Full regression/preservation reproduction requires the full Git branch/history at repository paths; archive test sources are references, not install scripts. See Docs/DEVELOPER_GUIDE.md. Docs/final-validation is authoritative; validation-attempt1 is labelled superseded.

Runtime reset/restore requires closed, unpaused, idle business; two distinct default-NO confirmations; verified complete data/source backup outside script loading paths; valid selected geometry/capacity; atomic commit. It never restores an old financial ledger in-game. Test destructive actions only on copied data. New PlacementDisplay.v1.ini is UI-only and created on explicit mode/filter changes; point saves never force markers ON.
'''
contents={}
for path in subprocess.check_output(['git','ls-files'],cwd=ROOT,text=True).splitlines():
    if path.startswith(DOC):contents['Docs/'+path[len(DOC):]]=(ROOT/path).read_bytes()
    elif path.startswith('verification/'):contents[path]=(ROOT/path).read_bytes()
for path in ['BASELINE_SHA256.txt','Verification/static_verify.py','AGENTS.md']:
    contents[path]=(ROOT/path).read_bytes()
for name in ['LICENSE','LICENSE.md','LICENSE.txt']:
    if (ROOT/name).is_file():contents[name]=(ROOT/name).read_bytes()
contents['Install/scripts/GlassHeroesBusiness.3.cs']=source
for name in ['GlassHeroesBusiness.V3.Experimental.csproj','GlassHeroesBusiness.V3.Experimental.sln']:
    contents['VisualStudio/'+name]=(ROOT/'experimental/shvdn3/VisualStudio'/name).read_bytes()
fixture='GlassHeroesBusiness_v0.3.3.0_RC1.2.cs';contents['verification/baselines/'+fixture+'.txt']=(ROOT/fixture).read_bytes()
contents['READ_ME_FIRST.md']=readme.encode();contents['FINAL_HANDOFF.md']=text.encode();contents['FINAL_HANDOFF.json']=(json.dumps(handoff,indent=2)+'\n').encode()
contents['SOURCE_SHA256.txt']=(sha+'  Install/scripts/GlassHeroesBusiness.3.cs\n').encode()
assert [p for p in contents if p.endswith('.3.cs')]==['Install/scripts/GlassHeroesBusiness.3.cs']
assert not any(Path(p).name.lower() in ['save.ini','settings.ini','placementdisplay.v1.ini'] or p.endswith(('.dll','.2.cs')) for p in contents)
manifest={p:hashlib.sha256(data).hexdigest() for p,data in sorted(contents.items())}
contents['PACKAGE_MANIFEST.json']=(json.dumps({'final_commit':tip,'source_sha256':sha,'sha256':manifest},indent=2)+'\n').encode()
out.mkdir(parents=True,exist_ok=True);archive=out/'Glass_Heroes_v0.3.4_Issue37_Placement_Parking_Experimental.zip'
assert not archive.exists(),'Preserve existing packages; choose a new output folder'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for name,data in sorted(contents.items()):z.writestr(name,data)
with tempfile.TemporaryDirectory(prefix='gh37-archive-') as temp:
    temp=Path(temp)
    with zipfile.ZipFile(archive) as z:assert z.testzip() is None;z.extractall(temp)
    for name,digest in manifest.items():assert hashlib.sha256((temp/name).read_bytes()).hexdigest()==digest,name
    with tempfile.TemporaryDirectory(prefix='gh37-archive-build-') as build:
        r=subprocess.run(['dotnet','build',str(temp/'VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj'),'-c','Release','--property:BaseIntermediateOutputPath='+build+'/obj/','--property:OutputPath='+build+'/bin/','--maxcpucount:2','--verbosity','minimal'],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
        (out/'EXTRACTED_RELEASE_BUILD.txt').write_text(r.stdout);assert r.returncode==0,r.stdout
receipt={'archive':archive.name,'archive_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'branch':branch,'final_commit':tip,'source_sha256':sha,
         'manifest_files_verified':len(manifest),'zip_integrity':'PASS','extracted_release_build':'PASS','errors':0,'warnings':sorted(set(re.findall(r'warning (CS\d+)',r.stdout))),
         'behavior_navigation':'1057/1057 PASS on byte-identical code','static':'21/21 PASS','inventory_structure':'13/13 PASS','gta_testing':'NOT RUN','owner_acceptance':'PENDING YES/NO','rc2_combined':False,'live_installation_modified':False}
(out/'PACKAGE_RECEIPT.json').write_text(json.dumps(receipt,indent=2)+'\n');(out/'FINAL_HANDOFF.md').write_text(text);(out/'FINAL_HANDOFF.json').write_text(json.dumps(handoff,indent=2)+'\n')
(out/'SHA256SUMS.txt').write_text(receipt['archive_sha256']+'  '+archive.name+'\n'+sha+'  Install/scripts/GlassHeroesBusiness.3.cs\n')
print(json.dumps(receipt,indent=2))
