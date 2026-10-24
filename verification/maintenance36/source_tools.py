"""Parse verbatim top-level production members with the repository C# literal lexer."""
import ast,re,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
BASE='c8617aed5cda492214bbbe1907042bcafbfa0dc1'
SOURCE=ROOT/'experimental/shvdn3/Install/scripts/GlassHeroesBusiness.3.cs'
fn=next(n for n in ast.parse((ROOT/'Verification/static_verify.py').read_text()).body if isinstance(n,ast.FunctionDef) and n.name=='strip_literals');ns={};exec(compile(ast.Module(body=[fn],type_ignores=[]),'lexer','exec'),ns);strip_literals=ns['strip_literals']
def members(s):
 clean,_=strip_literals(s);out={}
 for m in re.finditer(r'^    (?:private|public)\s+(?:[^\n=;{}]+\s)?(\w+)\s*(?:\(|\{)',clean,re.M):
  end=clean.index('{',m.start())+1;depth=1
  while depth:depth+=(clean[end]=='{')-(clean[end]=='}');end+=1
  assert m[1] not in out,m[1];out[m[1]]=s[m.start():end]
 return out
def baseline():return subprocess.check_output(['git','show',BASE+':'+str(SOURCE.relative_to(ROOT))],cwd=ROOT,text=True)
