#!/usr/bin/env python3
"""Actual net48 build + original 85 behaviors + selected feature suites + scope."""
import argparse,hashlib,json,re,subprocess,sys,tempfile
from pathlib import Path
from datetime import datetime,timezone
repo=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--label',required=True);p.add_argument('--profile',required=True);p.add_argument('--suites',nargs='+',required=True);args=p.parse_args()
release=repo/'docs/releases/v0.3.3.2-shvdn3-supplier-workflow';release.mkdir(parents=True,exist_ok=True)
source=repo/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
def run(cmd,name):
 r=subprocess.run(cmd,cwd=repo,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True);(release/name).write_text(r.stdout)
 if r.returncode:print(r.stdout);raise SystemExit(r.returncode)
 return r.stdout
with tempfile.TemporaryDirectory(prefix='gh-economy-build-') as temp:
 build=run(['dotnet','build','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','-c','Release','--property:BaseIntermediateOutputPath='+temp+'/obj/','--property:OutputPath='+temp+'/bin/','--maxcpucount:2','--verbosity','minimal'],'build.txt')
behavior=run([sys.executable,'verification/run_issue_tests.py','--source',str(source),'--v3-stubs','2','3','4','5','6','7'],'inherited-behavior.txt')
logger=run([sys.executable,'verification/run_diagnostics_tests.py','--source',str(source)],'logger-tests.txt')
migration=run([sys.executable,'verification/migration/run_tests.py'],'migration-tests.txt')
feature=run([sys.executable,'verification/economy/run_feature_tests.py',*args.suites],'feature-tests.txt')
run([sys.executable,'verification/run_static_checks.py',str(release),'--source',str(source),'--class-name','GlassHeroesBusinessV3Experimental'],'static-tests.txt')
run([sys.executable,'verification/economy/check_scope.py',args.profile,'--output',str(release/'preservation.json')],'preservation.txt')
original=re.findall(r'#(\d+) (\d+)/(\d+) PASS;',behavior);assert len(original)==6 and sum(int(x[2]) for x in original)==54 and all(a==b for _,a,b in original)
assert '9/9 diagnostic behavior tests passed' in logger and 'Migration 22/22 PASS;' in migration
counts={}
for suite in args.suites:
 m=re.search(re.escape(suite)+r' (\d+)/(\d+) PASS;',feature);assert m and m[1]==m[2] and int(m[1])>0,suite
 counts[suite]={'passed':int(m[1]),'total':int(m[2])}
static=json.loads((release/'StaticVerification.json').read_text());assert static['passed']==static['total']==21
sha=hashlib.sha256(source.read_bytes()).hexdigest();assert static['source_sha256']==sha
result={'label':args.label,'profile':args.profile,'created_utc':datetime.now(timezone.utc).isoformat(),'reference':'ScriptHookVDotNet3 3.6.0','target':'net48 / C# 7.3','sdk':subprocess.check_output(['dotnet','--version'],text=True).strip(),'build':'PASS','build_errors':0,'warnings':sorted(set(re.findall(r'warning (CS\d+)',build))),'inherited_behavior':{'corrective':54,'logger':9,'migration':22,'total':85},'feature_suites':counts,'behavior_total':85+sum(c['total'] for c in counts.values()),'static':{'passed':21,'total':21},'source_sha256':sha,'clock_source_unchanged':True,'v2_and_protected_baseline_unchanged':True,'live_installation_or_saves_touched':False,'gta_testing':'NOT RUN','crash_cause':'UNATTRIBUTED','enhanced_runtime_compatibility':'NOT VERIFIED','review_status':'DRAFT / PENDING'}
(release/'validation.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
