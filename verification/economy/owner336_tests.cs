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
foreach(int amount in new[]{1000,10000,100000}){
 int value=amount;
 Test("selected owner amount deposit then withdrawal "+value,()=>{var h=new Harness{_cash=500000,_ownerTransferAmount=value};h._financeBook=FinanceBook.Create(h.CaptureFinanceSnapshot());Game.Player.Money=500000;h.TransferPlayerMoney(true);Check(h._cash==500000+value&&Game.Player.Money==500000-value&&h._financeOwnerDeposits==value&&h._revenue==0,"deposit");h.TransferPlayerMoney(false);Check(h._cash==500000&&Game.Player.Money==500000&&h._financeOwnerWithdrawals==value&&h._revenue==0&&FinanceSnapshot.Profit(h.CaptureFinanceSnapshot())==0,"withdraw");});
 Test("selected owner amount failure rollback "+value,()=>{var h=new Harness{_cash=500000,_ownerTransferAmount=value,saveFail=true};h._financeBook=FinanceBook.Create(h.CaptureFinanceSnapshot());Game.Player.Money=500000;h.TransferPlayerMoney(true);Check(h._cash==500000&&Game.Player.Money==500000&&h._financeOwnerDeposits==0&&h._financeBook.Entries.Count==0,"rollback");});
 Test("wallet limit validation at selected amount "+value,()=>{var h=new Harness{_cash=500000,_ownerTransferAmount=value};Game.Player.Money=int.MaxValue-value+1;h.TransferPlayerMoney(false);Check(h._cash==500000&&h.saves==0,"wallet overflow");});
}
Test("freemode player cannot transfer",()=>{var h=new Harness{_ownerTransferAmount=100000};Game.Player.Character.Model.Hash=123;h.TransferPlayerMoney(true);Check(h.saves==0&&h._cash==1000,"ped restriction");});
Test("owner totals overflow cannot commit",()=>{var h=new Harness{_financeOwnerDeposits=long.MaxValue-1};h.TransferPlayerMoney(true);Check(h.saves==0&&Game.Player.Money==10000,"counter overflow");});
Test("invalid amount cannot transfer",()=>{var h=new Harness{_ownerTransferAmount=-1000};h.TransferPlayerMoney(true);Check(h.saves==0&&h._cash==1000,"amount injection");});
Test("paused amount transfer has no effects",()=>{var h=new Harness{_ownerTransferAmount=10000};Game.IsPaused=true;h.TransferPlayerMoney(true);Check(h.saves==0&&Game.Player.Money==10000,"pause");});
Console.WriteLine($"owner336 {passed}/{passed} PASS; production selected-amount owner transfers and FinanceV1, GTA NOT RUN");
}
}
