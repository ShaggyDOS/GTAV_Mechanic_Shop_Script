#!/usr/bin/env python3
"""Run the real SHVDN3 Release build and inherited/new offline suites; never GTA."""
import argparse,hashlib,json,re,subprocess,sys,tempfile
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime,timezone
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=argparse.ArgumentParser();p.add_argument('--output',type=Path,required=True);p.add_argument('--issue36-integration',action='store_true');p.add_argument('--integration-approved',action='store_true');p.add_argument('--diagnostic-rc2',action='store_true');args=p.parse_args();out=args.output.resolve();out.mkdir(parents=True,exist_ok=True)
source=ROOT/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
sha=hashlib.sha256(source.read_bytes()).hexdigest()
suites='repair sales pallets wages reload supplier range menu cart pause focus finance startup hardening bays workflow335 recovery336 owner336 retail336 status336 demand336 inventory340 procurement340 receipt340 manager340 integration340'.split()
if args.issue36_integration:suites += ['footer36','routes36','locator36']
preservation_script='integration36_preservation.py' if args.issue36_integration else 'preservation.py'
if args.diagnostic_rc2:
 assert args.issue36_integration
 suites += ['trace340']
 preservation_script='../diagnostic340/preservation.py'
with tempfile.TemporaryDirectory(prefix='gh340-release-') as temp:
 commands={
 'build':['dotnet','build','experimental/shvdn3/VisualStudio/GlassHeroesBusiness.V3.Experimental.csproj','-c','Release','--property:BaseIntermediateOutputPath='+temp+'/obj/','--property:OutputPath='+temp+'/bin/','--maxcpucount:2','--verbosity','minimal'],
 'inherited':[sys.executable,'verification/run_issue_tests.py','--source',str(source),'--v3-stubs','2','3','4','5','6','7'],
 'logger':[sys.executable,'verification/run_diagnostics_tests.py','--source',str(source)],
 'migration':[sys.executable,'verification/migration/run_tests.py'],
 'features':[sys.executable,'verification/economy/run_feature_tests.py',*suites],
 'navigation':[sys.executable,'verification/placement_settings/run_navigation.py'],
 'static':[sys.executable,'verification/run_static_checks.py',str(out),'--source',str(source),'--class-name','GlassHeroesBusinessV3Experimental'],
 'inventory-structure':[sys.executable,'verification/inventory340/verify.py'],
 'preservation':[sys.executable,'verification/inventory340/'+preservation_script,str(out/'preservation.json')],
 }
 def run(item):
  name,cmd=item
  try:
   r=subprocess.run(cmd,cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
   (out/(name+'.txt')).write_text(r.stdout);return name,{'command':cmd,'exit_code':r.returncode}
  except OSError as ex:
   (out/(name+'.txt')).write_text(str(ex));return name,{'command':cmd,'exit_code':-1}
 with ThreadPoolExecutor(max_workers=2) as pool: results=dict(pool.map(run,commands.items()))
(out/'commands.json').write_text(json.dumps(results,indent=2)+'\n')
failed=[name for name,v in results.items() if v['exit_code']!=0]
if failed:
 print('FAILED: '+', '.join(failed));sys.exit(1)
def read(n):return (out/(n+'.txt')).read_text()
corrective=re.findall(r'#(\d+) (\d+)/(\d+) PASS;',read('inherited'));assert len(corrective)==6 and sum(int(x[2]) for x in corrective)==54 and all(a==b for _,a,b in corrective)
assert '9/9 diagnostic behavior tests passed' in read('logger') and 'Migration 22/22 PASS;' in read('migration')
counts={}
for suite in suites:
 m=re.search(re.escape(suite)+r' (\d+)/(\d+) PASS;',read('features'));assert m and m[1]==m[2] and int(m[1])>0,suite;counts[suite]=int(m[1])
nav=re.search(r'placement (\d+)/(\d+) PASS;',read('navigation'));assert nav and nav[1]==nav[2]
static=json.loads((out/'StaticVerification.json').read_text());assert static['passed']==static['total']==21 and static['source_sha256']==sha
assert '13/13 v0.3.4 Phase A STRUCTURAL checks' in read('inventory-structure')
assert hashlib.sha256(source.read_bytes()).hexdigest()==sha
result={'created_utc':datetime.now(timezone.utc).isoformat(),'source_sha256':sha,'source_revision':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(),'baseline':'c8617aed5cda492214bbbe1907042bcafbfa0dc1','build':{'status':'PASS','target':'net48 / C# 7.3','reference':'ScriptHookVDotNet3 3.6.0','errors':0,'warnings':sorted(set(re.findall(r'warning (CS\d+)',read('build'))))},'behavior':{'corrective':54,'logger':9,'migration':22,'suites':counts,'navigation':int(nav[1]),'total':85+sum(counts.values())+int(nav[1])},'static':{'passed':21,'total':21},'inventory_structural':{'passed':13,'total':13},'preservation':'PASS','issue36_integration':args.issue36_integration,'integration_acceptance':('OWNER YES — integration accepted; GTA NOT RUN' if args.integration_approved else 'PENDING OWNER YES/NO') if args.issue36_integration else 'NOT APPLICABLE','gta_testing':'NOT RUN','enhanced_runtime_compatibility':'NOT VERIFIED','rc1_runtime_acceptance':'OWNER TESTING ONGOING','crash_cause':'UNATTRIBUTED','live_installation_or_owner_data_touched':False}
if args.diagnostic_rc2:
 result.update({'package_identity':'Glass Heroes Business Manager v0.3.4 RC2 Diagnostic Experimental','diagnostic_only':True,'rc1_source_commit':'e312d8c9500f4a990eb993b82e8e76b55711d3de','rc1_source_sha256':'70bc95bcf0d16cbf36b61870bff7e21193c05479b79a4d8beeb45a0b734c2120','owner_review':'PENDING; implementation scope pre-authorized','crash_fix_claimed':False})
(out/'validation.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))
