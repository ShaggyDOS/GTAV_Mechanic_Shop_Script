using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class ScriptSettings {
 internal static int readCalls,writeCalls;
 internal Dictionary<string,string> values=new Dictionary<string,string>();string file;
 internal T GetValue<T>(string s,string k,T d){readCalls++;return values.TryGetValue(s+"|"+k,out string v)?(T)Convert.ChangeType(v,typeof(T),System.Globalization.CultureInfo.InvariantCulture):d;}
 internal void SetValue<T>(string s,string k,T v){values[s+"|"+k]=Convert.ToString(v,System.Globalization.CultureInfo.InvariantCulture);}
 internal ScriptSettings Copy(){var n=new ScriptSettings();foreach(var kv in values)n.values[kv.Key]=kv.Value;return n;}
 internal static ScriptSettings Load(string p){var d=new ScriptSettings{file=p};if(File.Exists(p))foreach(var l in File.ReadAllLines(p)){int eq=l.IndexOf('=');if(eq>0)d.values[l.Substring(0,eq)]=l.Substring(eq+1);}return d;}
 internal bool Save(){writeCalls++;File.WriteAllLines(file,values.Select(k=>k.Key+"="+k.Value));return true;}
}
class Model {internal int Hash=unchecked((int)0x0D7114C9);}class Ped {internal Model Model=new Model();}
static class Game {internal static int reads;static bool paused;static PlayerState player=new PlayerState();internal static bool IsPaused {get{reads++;return paused;}set{paused=value;}}internal static PlayerState Player {get{reads++;return player;}set{player=value;}}internal class PlayerState{internal Ped Character=new Ped();internal int Money=10000;}}

class Harness {
 /*<source:FinanceSnapshot>*/
 /*<source:FinanceEntry>*/
 /*<source:FinanceBook>*/
 /*<source:CaptureFinanceSnapshot>*/
 /*<source:WriteFinanceSnapshot>*/
 /*<source:ReadFinanceSnapshot>*/
 /*<source:WriteFinanceBook>*/
 /*<source:LoadFinanceBook>*/
 /*<source:PrepareFinanceCommit>*/
 /*<source:TryCommitBusinessSave>*/
 /*<source:TryPayBusinessBills>*/
 /*<source:PayInvoice>*/
 /*<source:GiveManagerPayrollRebate>*/
 /*<source:MechanicWagesDue>*/
 /*<source:TransferPlayerMoney>*/
 class Mechanic {internal long UnpaidWages;internal double WageUnpaidSeconds;}
 List<Mechanic> _mechanics=new List<Mechanic>();HashSet<int> _mechanicOverdueNotified=new HashSet<int>();
 FinanceBook _financeBook;long _financeActiveMs,_financeOwnerDeposits,_financeOwnerWithdrawals,_financeRebateIncome,_financeRefundIncome;
 static string[] FinanceCategories=new string[18];
 long _cash=1000,_revenue,_customerRevenue,_remoteSales,_partsSalesRevenue,_partsSpend,_payrollSpend,_overheadSpend,_hireSpend,_trainingSpend,_equipmentSpend;
 long _formerStaffWagesDue,_receptionWagesDue,_managerWagesDue,_rentUtilitiesDue,_supplierInvoicesDue,_managerSavings;
 double _managerWageAgeSeconds,_receptionWageUnpaidSeconds,_rentUnpaidSeconds;
 bool _clockedIn,_bootGameplayActivated=true,_receptionOverdueNotified,saveFail;int _managerTier=-1,saves;
 ScriptSettings disk;string realFile;List<string> notices=new List<string>();
 void Notify(string s){notices.Add(s);}void Log(string s,Exception e){}int ManagerDiscountPercent()=>10;
 Harness(){_financeBook=FinanceBook.Create(CaptureFinanceSnapshot());}
 bool Save(){saves++;var candidate=PrepareFinanceCommit();if(saveFail)return false;var data=new ScriptSettings();WriteFinanceBook(data,candidate);if(realFile!=null){if(!TryCommitBusinessSave(realFile,p=>{var d=ScriptSettings.Load(p);foreach(var k in data.values)d.values[k.Key]=k.Value;d.Save();},e=>{}))return false;}disk=data;_financeBook=candidate;return true;}
 void Sales(long physical,long remote,long autonomous,long retail){_cash+=physical+remote+autonomous+retail;_revenue+=physical+remote+autonomous+retail;_customerRevenue+=physical+remote;_remoteSales+=remote;_partsSalesRevenue+=retail;_financeActiveMs+=1000;}
 Harness Reload(){var n=new Harness{_cash=_cash,_revenue=_revenue,_customerRevenue=_customerRevenue,_remoteSales=_remoteSales,_partsSalesRevenue=_partsSalesRevenue,_partsSpend=_partsSpend,_payrollSpend=_payrollSpend,_overheadSpend=_overheadSpend,_hireSpend=_hireSpend,_trainingSpend=_trainingSpend,_equipmentSpend=_equipmentSpend,_formerStaffWagesDue=_formerStaffWagesDue,_receptionWagesDue=_receptionWagesDue,_managerWagesDue=_managerWagesDue,_rentUtilitiesDue=_rentUtilitiesDue,_supplierInvoicesDue=_supplierInvoicesDue};foreach(var m in _mechanics)n._mechanics.Add(new Mechanic{UnpaidWages=m.UnpaidWages});n.LoadFinanceBook(disk.Copy());return n;}
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static void Test(string n,Action a){Game.IsPaused=false;Game.Player=new Game.PlayerState();a();passed++;Console.WriteLine("PASS finance "+n);}
 static void Reject(Action a){try{a();throw new Exception("bad data accepted");}catch(InvalidDataException){}}
 static void Main(){
 Test("legacy initialization uses no game access writes or fabricated transactions",()=>{Game.reads=0;int writes=ScriptSettings.writeCalls;var h=new Harness{_cash=253,_revenue=5000,_customerRevenue=5000,_remoteSales=5000};h.LoadFinanceBook(new ScriptSettings());Check(Game.reads==0&&ScriptSettings.writeCalls==writes&&h._financeBook.Entries.Count==0&&h._financeBook.Opening.V[0]==253&&h._cash==253,"legacy init");});
 Test("valid journal initialization preserves timestamp and closed shift without game access",()=>{var h=new Harness{_clockedIn=true};h.Save();h.Sales(0,5000,0,0);h.Save();Game.reads=0;int writes=ScriptSettings.writeCalls;var r=h.Reload();Check(Game.reads==0&&ScriptSettings.writeCalls==writes&&r._cash==6000&&r._financeActiveMs==1000&&!r._financeBook.ShiftOpen&&r._financeBook.Entries.Count==1,"valid init");});
 foreach(int count in new[]{-1,257,int.MaxValue})Test("invalid journal count bounded managed rejection "+count,()=>{var h=new Harness();h.Save();h.disk.SetValue("FinanceV1","Count",count);Game.reads=0;ScriptSettings.readCalls=0;int writes=ScriptSettings.writeCalls;Reject(()=>h.Reload());Check(Game.reads==0&&ScriptSettings.writeCalls==writes&&ScriptSettings.readCalls<100,"count limit");});
 foreach(string key in new[]{"Version","NextId","ActiveMs","Shift","CashIn","CashOut","Deposits","Withdrawals","Rebates","Refunds"})Test("invalid startup field is managed failure "+key,()=>{var h=new Harness();h.Save();h.disk.SetValue("FinanceV1",key,key=="Version"?2:-1);Game.reads=0;int writes=ScriptSettings.writeCalls;Reject(()=>h.Reload());Check(Game.reads==0&&ScriptSettings.writeCalls==writes,"no unsafe recovery");});
 Test("stale cross-build journal fails closed without modifying data",()=>{var h=new Harness();h.Sales(0,5000,0,0);h.Save();string before=string.Join(";",h.disk.values);h._cash++;Game.reads=0;Reject(()=>h.Reload());Check(Game.reads==0&&string.Join(";",h.disk.values)==before,"stale save");});
 Test("maximum retained rows load with bounded reads and no game access",()=>{var h=new Harness();for(int i=0;i<300;i++){h.Sales(0,1,0,0);h.Save();}Game.reads=0;ScriptSettings.readCalls=0;var r=h.Reload();Check(Game.reads==0&&r._financeBook.Entries.Count==256&&ScriptSettings.readCalls<2000&&r._cash==1300,"retention bound");});
 Test("real journal file initialization is read only",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(d);try{string p=Path.Combine(d,"save.ini");var h=new Harness{realFile=p};h.Sales(0,5000,0,0);h.Save();byte[] before=File.ReadAllBytes(p);int writes=ScriptSettings.writeCalls;Game.reads=0;h.LoadFinanceBook(ScriptSettings.Load(p));Check(Game.reads==0&&ScriptSettings.writeCalls==writes&&File.ReadAllBytes(p).SequenceEqual(before)&&Directory.GetFiles(d).Length==1,"read only");}finally{Directory.Delete(d,true);}});
 Console.WriteLine($"startup {passed}/{passed} PASS; production finance initialization with counted forbidden game access and deterministic INI adapter; GTA NOT RUN");
 }
}
