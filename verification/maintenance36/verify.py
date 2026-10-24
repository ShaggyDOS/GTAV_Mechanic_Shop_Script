#!/usr/bin/env python3
"""Build and run RC1/maintenance suites without overwriting RC1 evidence."""
import argparse,hashlib,json,re,subprocess,sys,tempfile
from pathlib import Path
from datetime import datetime,timezone
repo=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--stage',required=True);p.add_argument('--extra',nargs='*',default=[]);args=p.parse_args()
out=repo/'docs/features/v0.3.3.6-issue36'/args.stage;out.mkdir(parents=True,exist_ok=True)
base=json.loads(subprocess.check_output(['git','show','c8617aed5cda492214bbbe1907042bcafbfa0dc1:docs/features/v0.3.3.6/validation.json'],cwd=repo,text=True))
source=repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
def run(cmd,name):
 result=subprocess.run(cmd,cwd=repo,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True);(out/name).write_text(result.stdout)
 if result.returncode: print(result.stdout);raise SystemExit(result.returncode)
 return result.stdout
with tempfile.TemporaryDirectory(prefix='gh36-build-') as temp:
 build=run(['dotnet','build','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','-c','Release','--property:BaseIntermediateOutputPath='+temp+'/obj/','--property:OutputPath='+temp+'/bin/','--maxcpucount:2','--verbosity','minimal'],'build.txt')
behavior=run([sys.executable,'verification/run_issue_tests.py','--source',str(source),'--v3-stubs','2','3','4','5','6','7'],'inherited-behavior.txt')
logger=run([sys.executable,'verification/run_diagnostics_tests.py','--source',str(source)],'logger-tests.txt')
migration=run([sys.executable,'verification/migration/run_tests.py'],'migration-tests.txt')
suites=list(base['feature_suites'])+args.extra
features=run([sys.executable,'verification/economy/run_feature_tests.py',*suites],'feature-tests.txt')
navigation=run([sys.executable,'verification/placement_settings/run_navigation.py'],'navigation-tests.txt')
run([sys.executable,'verification/run_static_checks.py',str(out),'--source',str(source),'--class-name','GlassHeroesBusinessV3Experimental'],'static-tests.txt')
if args.stage!='baseline':run([sys.executable,'verification/maintenance36/preservation.py','--output',str(out/'preservation.json')],'preservation.txt')
else:assert hashlib.sha256(source.read_bytes()).hexdigest()==base['source_sha256']
original=re.findall(r'#(\d+) (\d+)/(\d+) PASS;',behavior);assert len(original)==6 and sum(int(x[2]) for x in original)==54 and all(a==b for _,a,b in original)
assert '9/9 diagnostic behavior tests passed' in logger and 'Migration 22/22 PASS;' in migration
counts={}
for suite in suites:
 m=re.search(re.escape(suite)+r' (\d+)/(\d+) PASS;',features);assert m and m[1]==m[2] and int(m[1])>0,suite
 counts[suite]={'passed':int(m[1]),'total':int(m[2])}
nav=re.search(r'placement (\d+)/(\d+) PASS;',navigation);assert nav and nav[1]==nav[2]
static=json.loads((out/'StaticVerification.json').read_text());assert static['passed']==static['total']==21
sha=hashlib.sha256(source.read_bytes()).hexdigest();assert static['source_sha256']==sha
r={'stage':args.stage,'base_commit':'c8617aed5cda492214bbbe1907042bcafbfa0dc1','branch':subprocess.check_output(['git','branch','--show-current'],cwd=repo,text=True).strip(),'utc':datetime.now(timezone.utc).isoformat(),'source_sha256':sha,'sdk':subprocess.check_output(['dotnet','--version'],text=True).strip(),'target':'net48 / C# 7.3','reference':'ScriptHookVDotNet3 3.6.0','build':'PASS','errors':0,'warnings':sorted(set(re.findall(r'warning (CS\d+)',build))),'inherited':85,'feature_suites':counts,'navigation':{'passed':int(nav[1]),'total':int(nav[2])},'behavior_total':85+sum(x['total'] for x in counts.values())+int(nav[2]),'static':{'passed':21,'total':21},'gta_testing':'NOT RUN','windows_execution':'NOT RUN','owner_implementation_approval':'YES — current chat','owner_final_acceptance':'PENDING YES/NO','v034_integration':'NOT PERFORMED — separate instance owns integration','live_installation_or_user_saves_accessed':False}
(out/'validation.json').write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r,indent=2))
