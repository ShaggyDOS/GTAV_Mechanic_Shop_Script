using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class ScriptSettings {
 internal Dictionary<string,string> values=new Dictionary<string,string>();string file;
 internal T GetValue<T>(string s,string k,T d)=>values.TryGetValue(s+"|"+k,out string v)?(T)Convert.ChangeType(v,typeof(T),System.Globalization.CultureInfo.InvariantCulture):d;
 internal void SetValue<T>(string s,string k,T v){values[s+"|"+k]=Convert.ToString(v,System.Globalization.CultureInfo.InvariantCulture);}
 internal ScriptSettings Copy(){var n=new ScriptSettings();foreach(var kv in values)n.values[kv.Key]=kv.Value;return n;}
 internal static ScriptSettings Load(string p){var d=new ScriptSettings{file=p};if(File.Exists(p))foreach(var l in File.ReadAllLines(p)){int eq=l.IndexOf('=');if(eq>0)d.values[l.Substring(0,eq)]=l.Substring(eq+1);}return d;}
 internal bool Save(){File.WriteAllLines(file,values.Select(k=>k.Key+"="+k.Value));return true;}
}
class Model {internal int Hash=unchecked((int)0x0D7114C9);}class Ped {internal Model Model=new Model();}
static class Game {internal static bool IsPaused;internal static PlayerState Player=new PlayerState();internal class PlayerState {internal Ped Character=new Ped();internal int Money=10000;}}
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
 int _ownerTransferAmount=1000;
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
 Test("5000 remote sales without outflows increases cash exactly 5000",()=>{var h=new Harness();h.Sales(0,5000,0,0);h.Save();Check(h._cash==6000&&h._financeBook.CashIn==5000&&h._financeBook.CashOut==0,"cash mismatch");Check(h._financeBook.Entries.Count==1&&h._financeBook.Entries[0].Category==1,"remote once");});
 Test("5000 sales with 4800 bill payments yields 200 cash increase",()=>{var h=new Harness{_supplierInvoicesDue=4800};h._financeBook=FinanceBook.Create(h.CaptureFinanceSnapshot());h._clockedIn=true;h.Save();var opening=h._financeBook.ShiftStart;h.Sales(0,5000,0,0);h.Save();Check(h.TryPayBusinessBills(0)&&h._cash==1200,"net cash");var d=FinanceSnapshot.Difference(h.CaptureFinanceSnapshot(),opening);Check(FinanceSnapshot.Sales(d)==5000&&FinanceSnapshot.Paid(d)==4800&&FinanceSnapshot.Paid(d)+FinanceSnapshot.Due(d)==0&&FinanceSnapshot.Profit(d)==5000,"opening bill double expense");});
 Test("four revenue categories sum gross without overlapping remote/customer",()=>{var h=new Harness();h.Sales(1000,2000,1500,500);h.Save();var v=h.CaptureFinanceSnapshot();Check(v.V[1]==1000&&v.V[2]==2000&&v.V[3]==1500&&v.V[4]==500&&FinanceSnapshot.Sales(v)==5000&&h._financeBook.Entries.Count==4,"partition");});
 Test("unpaid wages accrue profit expense but never cash payment",()=>{var h=new Harness();h._managerWagesDue=500;h.Save();Check(h._cash==1000&&h._financeBook.CashOut==0&&h._financeBook.Entries.Single().Category==15&&h._financeBook.Entries.Single().CashAfter==1000,"accrual cash");Check(FinanceSnapshot.Profit(h.CaptureFinanceSnapshot())==-500,"profit");});
 Test("supplier receipt creates one noncash accrual then payment changes only cash",()=>{var h=new Harness{_supplierInvoicesDue=700};h.Save();Check(h._financeBook.Entries.Single().Category==17&&h._cash==1000,"invoice cash");Check(h.TryPayBusinessBills(0)&&h._cash==300,"payment");Check(h._financeBook.Entries.Count(e=>e.Category==17)==1&&h._financeBook.Entries.Count(e=>e.Category==4)==1,"double expense");});
 Test("failed supplier settlement restores cash invoice and journal",()=>{var h=new Harness{_supplierInvoicesDue=500,saveFail=true};Check(!h.TryPayBusinessBills(0)&&h._cash==1000&&h._supplierInvoicesDue==500&&h._partsSpend==0&&h._financeBook.NextId==1,"rollback");h.saveFail=false;Check(h.TryPayBusinessBills(0)&&!h.TryPayBusinessBills(0)&&h._cash==500,"retry");});
 Test("all bills failed save restores individual staff liabilities and rebates",()=>{var h=new Harness{_managerTier=1,_managerWagesDue=100,_receptionWagesDue=50,_rentUtilitiesDue=40,_supplierInvoicesDue=30,saveFail=true};h._mechanics.Add(new Mechanic{UnpaidWages=200,WageUnpaidSeconds=300});h._mechanicOverdueNotified.Add(0);Check(!h.TryPayBusinessBills(1)&&h._cash==1000&&h._mechanics[0].UnpaidWages==200&&h._mechanics[0].WageUnpaidSeconds==300&&h._managerWagesDue==100&&h._financeRebateIncome==0&&h._managerSavings==0&&h._mechanicOverdueNotified.Contains(0),"all rollback");});
 Test("staff rebate is separate cash credit not gross sales",()=>{var h=new Harness{_managerTier=1};h._mechanics.Add(new Mechanic{UnpaidWages=500});Check(h.TryPayBusinessBills(3,0)&&h._cash==550&&h._payrollSpend==500&&h._financeRebateIncome==50&&h._revenue==0,"rebate");Check(h._financeBook.Entries.Any(e=>e.Category==5&&e.Amount==-500)&&h._financeBook.Entries.Any(e=>e.Category==12&&e.Amount==50),"rebate rows");});
 Test("owner deposit and withdrawal are capital not income or expense",()=>{var h=new Harness();h.TransferPlayerMoney(true);Check(h._cash==2000&&Game.Player.Money==9000&&h._revenue==0&&h._financeOwnerDeposits==1000,"deposit");h.TransferPlayerMoney(false);Check(h._cash==1000&&Game.Player.Money==10000&&h._financeOwnerWithdrawals==1000&&FinanceSnapshot.Profit(h.CaptureFinanceSnapshot())==0,"withdraw");});
 Test("failed owner transfer restores wallet business and ledger",()=>{var h=new Harness{saveFail=true};h.TransferPlayerMoney(true);Check(h._cash==1000&&Game.Player.Money==10000&&h._financeOwnerDeposits==0&&h._financeBook.Entries.Count==0,"wallet rollback");});
 Test("unexplained cash becomes adjustment not invented revenue",()=>{var h=new Harness();h._cash+=400;h.Save();Check(h._financeBook.Entries.Single().Category==14&&h._revenue==0&&h._financeBook.CashIn==400,"adjustment");});
 Test("snapshot reload preserves IDs cash totals and no offline clock age",()=>{var h=new Harness();h.Sales(0,5000,0,0);h.Save();var r=h.Reload();Check(r._financeActiveMs==1000&&r._financeBook.NextId==2&&r._financeBook.CashIn==5000&&r._financeBook.Entries[0].CashAfter==6000,"reload");r.Save();Check(r._financeBook.NextId==2&&r._financeBook.Entries.Count==1,"duplicate after load");});
 Test("failed staged journal cannot consume identities or mutate published book",()=>{var h=new Harness();h.Sales(0,5000,0,0);h.saveFail=true;Check(!h.Save()&&h._financeBook.NextId==1&&h._financeBook.Entries.Count==0,"staged published");h.saveFail=false;h.Save();Check(h._financeBook.NextId==2&&h._financeBook.Entries.Single().Amount==5000,"retry duplicate");});
 Test("current shift includes active work and closes without later owner-transfer drift",()=>{var h=new Harness{_clockedIn=true};h.Save();h.Sales(1000,2000,1000,1000);h.Save();h._clockedIn=false;h.Save();long close=h._financeBook.ShiftEnd.V[0];h.TransferPlayerMoney(true);Check(h._financeBook.ShiftEnd.V[0]==close&&h._financeBook.ShiftNumber==1,"closed shift drift");h._clockedIn=true;h.Save();Check(h._financeBook.ShiftNumber==2&&FinanceSnapshot.Sales(FinanceSnapshot.Difference(h.CaptureFinanceSnapshot(),h._financeBook.ShiftStart))==0,"new shift");});
 Test("reload closes saved shift without creating new income",()=>{var h=new Harness{_clockedIn=true};h.Save();h.Sales(0,5000,0,0);h.Save();var r=h.Reload();Check(!r._financeBook.ShiftOpen&&r._financeBook.ShiftEnd.V[0]==6000&&r._financeBook.ShiftNumber==1,"reload shift");r._clockedIn=true;r.Save();Check(r._financeBook.ShiftNumber==2,"reopen");});
 Test("legacy migration preserves all counters and opens audit at existing cash",()=>{var h=new Harness{_cash=253,_revenue=5000,_customerRevenue=5000,_remoteSales=5000,_supplierInvoicesDue=1200};h.LoadFinanceBook(new ScriptSettings());Check(h._cash==253&&h._revenue==5000&&h._financeBook.Opening.V[0]==253&&h._financeBook.Entries.Count==0&&h._supplierInvoicesDue==1200,"legacy drift");h.Save();Check(h._financeBook.Entries.Count==0,"fabricated history");});
 Test("history retention rolls opening balance forward without losing lifetime bridge",()=>{var h=new Harness();for(int i=0;i<300;i++){h.Sales(0,10,0,0);h.Save();}Check(h._financeBook.Entries.Count==256&&h._financeBook.HistoryOpeningCash==1440&&h._financeBook.Opening.V[0]+h._financeBook.CashIn-h._financeBook.CashOut==4000,"retention");var r=h.Reload();Check(r._financeBook.Entries[0].Id==45&&r._financeBook.NextId==301,"retained identity");});
 foreach(string key in new[]{"Version","Count","NextId","CashIn","HistoryOpeningCash"})Test("malformed header rejected "+key,()=>{var h=new Harness();h.Sales(0,50,0,0);h.Save();h.disk.SetValue("FinanceV1",key,key=="Version"?2:-1);Reject(()=>h.Reload());});
 foreach(string key in new[]{"Id","ActiveMs","Category","Amount","CashAfter","Shift"})Test("malformed transaction rejected "+key,()=>{var h=new Harness();h.Sales(0,50,0,0);h.Save();h.disk.SetValue("FinanceEntryV1.0",key,key=="Amount"?0:-1);Reject(()=>h.Reload());});
 Test("stale source financial changes reject journal replay",()=>{var h=new Harness();h.Sales(0,50,0,0);h.Save();h._cash++;Reject(()=>h.Reload());});
 Test("book rejects backward active-game clock",()=>{var h=new Harness();h._financeActiveMs=1000;h.Save();h._financeActiveMs=999;Reject(()=>h.PrepareFinanceCommit());});
 Test("paused money actions never settle bills or owner transfers",()=>{var h=new Harness{_supplierInvoicesDue=500};Game.IsPaused=true;Check(!h.TryPayBusinessBills(0),"paused bill");h.TransferPlayerMoney(true);Check(h._cash==1000&&h._financeOwnerDeposits==0&&h.saves==0,"paused owner");});
 Test("atomic file failure retains prior journal and business snapshot",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(d);try{string p=Path.Combine(d,"save.ini");File.WriteAllText(p,"old-business-and-journal");Check(!TryCommitBusinessSave(p,t=>{File.WriteAllText(t,"partial");throw new IOException();},e=>{})&&File.ReadAllText(p)=="old-business-and-journal"&&Directory.GetFiles(d).Length==1,"atomic failure");}finally{Directory.Delete(d,true);}});
 Test("actual file journal serialization roundtrip preserves unknown keys",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(d);try{string p=Path.Combine(d,"save.ini");File.WriteAllText(p,"Other|Keep=yes\n");var h=new Harness{realFile=p};h.Sales(0,5000,0,0);h.Save();var data=ScriptSettings.Load(p);Check(data.GetValue("Other","Keep","")=="yes","unknown");h.LoadFinanceBook(data);Check(h._financeBook.Entries.Single().Amount==5000,"file roundtrip");}finally{Directory.Delete(d,true);}});
 Console.WriteLine($"finance {passed}/{passed} PASS; production reporting/journal/payment members; deterministic INI adapter and real temporary files; GTA NOT RUN");
 }
}
