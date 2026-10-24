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
 bool Save(){saves++;var candidate=PrepareFinanceCommit();if(saveFail)return false;var data=new ScriptSettings();WriteFinanceBook(data,candidate);WriteSupplierOutcome(data);if(realFile!=null){if(!TryCommitBusinessSave(realFile,p=>{var d=ScriptSettings.Load(p);foreach(var k in data.values)d.values[k.Key]=k.Value;d.Save();},e=>{}))return false;}disk=data;_financeBook=candidate;return true;}
 void Sales(long physical,long remote,long autonomous,long retail){_cash+=physical+remote+autonomous+retail;_revenue+=physical+remote+autonomous+retail;_customerRevenue+=physical+remote;_remoteSales+=remote;_partsSalesRevenue+=retail;_financeActiveMs+=1000;}
 Harness Reload(){var n=new Harness{_cash=_cash,_revenue=_revenue,_customerRevenue=_customerRevenue,_remoteSales=_remoteSales,_partsSalesRevenue=_partsSalesRevenue,_partsSpend=_partsSpend,_payrollSpend=_payrollSpend,_overheadSpend=_overheadSpend,_hireSpend=_hireSpend,_trainingSpend=_trainingSpend,_equipmentSpend=_equipmentSpend,_formerStaffWagesDue=_formerStaffWagesDue,_receptionWagesDue=_receptionWagesDue,_managerWagesDue=_managerWagesDue,_rentUtilitiesDue=_rentUtilitiesDue,_supplierInvoicesDue=_supplierInvoicesDue};foreach(var m in _mechanics)n._mechanics.Add(new Mechanic{UnpaidWages=m.UnpaidWages});n.LoadFinanceBook(disk.Copy());return n;}
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static void Test(string n,Action a){Game.IsPaused=false;Game.Player=new Game.PlayerState();a();passed++;Console.WriteLine("PASS integration340 "+n);}
 static void Reject(Action a){try{a();throw new Exception("bad data accepted");}catch(InvalidDataException){}}

 /*<source:SupplierOutcome>*/
 SupplierOutcome _lastSupplierOutcome;
 /*<source:CaptureSupplierOutcome>*/
 /*<source:WriteSupplierOutcome>*/
 /*<source:LoadSupplierOutcome>*/
 /*<source:SupplierLineTotal>*/
 /*<source:ApplySupplierInventory>*/
 /*<source:CommitSupplierDelivery>*/
 /*<source:TryCancelSupplierOrder>*/
 int _parts=10,_deliveryKind=-2,_deliveryQuantity,_deliverySupplier=2,_ownedStoragePallets=1;
 long _deliveryCost,_deliveryDueUtcTicks,_completedSupplierDeliveries;
 double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds;
 bool _deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded,_palletVisualDirty;
 int[] _specialStock={5,6,7},_deliveryLines,_storedDeliveryCrates={0},_deliveredPackageMasks={0};
 const int CratesPerPallet=9;
 class WorkBay{internal bool IsSet=true;}
 WorkBay point=new WorkBay();WorkBay StoragePalletPoint(int i)=>point;
 bool CanStoreShipment(int q)=>q>0&&q<=100;
 string DeliveryItemName(int kind)=>"parts";
 static Harness Pending(){var h=new Harness{_clockedIn=true,_deliveryKind=-1,_deliveryLines=new[]{2,3,4,1},_deliveryQuantity=10,_deliveryCost=1235,_deliveryDueUtcTicks=77};h.Save();return h;}
 static void Main(){

 Test("supplier receipt accrues liability with no cash or revenue",()=>{var h=Pending();Check(h.CommitSupplierDelivery(0,77),"receipt");Check(h._cash==1000&&h._revenue==0&&h._supplierInvoicesDue==1235&&h._financeBook.CashIn==0&&h._financeBook.CashOut==0,"accrual");Check(h._lastSupplierOutcome.Invoice==1235,"audit");});
 Test("5000 remote sales and receipt without outflows retain full cash increase",()=>{var h=Pending();h.Sales(0,5000,0,0);h.Save();Check(h.CommitSupplierDelivery(0,77)&&h._cash==6000&&h._financeBook.CashIn==5000&&h._financeBook.CashOut==0&&h._remoteSales==5000&&h._customerRevenue==5000&&h._revenue==5000,"remote counted once");});
 Test("same remote sales then actual supplier payment reconcile closing cash",()=>{var h=Pending();h.Sales(0,5000,0,0);h.Save();h.CommitSupplierDelivery(0,77);Check(h.TryPayBusinessBills(0)&&h._cash==4765&&h._supplierInvoicesDue==0&&h._partsSpend==1235&&h._financeBook.CashIn==5000&&h._financeBook.CashOut==1235&&h._cash==1000+h._financeBook.CashIn-h._financeBook.CashOut,"bridge");});
 Test("outcome and finance journal roundtrip together",()=>{var h=Pending();h.Sales(0,5000,0,0);h.CommitSupplierDelivery(0,77);var r=h.Reload();r.LoadSupplierOutcome(h.disk.Copy());Check(r._cash==6000&&r._supplierInvoicesDue==1235&&r._financeBook.CashIn==5000&&r._lastSupplierOutcome.OrderTicks==77&&r._lastSupplierOutcome.Invoice==1235,"reload");});
 Test("failed receipt never publishes candidate journal or terminal outcome",()=>{var h=Pending();var old=h._financeBook;int rows=old.Entries.Count;h.saveFail=true;Check(!h.CommitSupplierDelivery(0,77)&&Object.ReferenceEquals(old,h._financeBook)&&h._financeBook.Entries.Count==rows&&h._supplierInvoicesDue==0&&h._parts==10&&h._lastSupplierOutcome==null,"rollback");});
 Test("prepaid receipt does not generate another liability or payment",()=>{var h=Pending();h._deliveryPrepaid=true;Check(h.CommitSupplierDelivery(0,77)&&h._cash==1000&&h._supplierInvoicesDue==0&&h._financeBook.CashOut==0&&h._lastSupplierOutcome.Invoice==0,"prepaid");});
 Test("unpaid cancel creates neither income nor cash movement",()=>{var h=Pending();Check(h.TryCancelSupplierOrder()&&h._cash==1000&&h._revenue==0&&h._financeBook.CashIn==0&&h._financeBook.CashOut==0&&h._lastSupplierOutcome.State==2,"cancel");});
 Test("prepaid refund is cash inflow separate from business sales",()=>{var h=Pending();h._deliveryPrepaid=true;Check(h.TryCancelSupplierOrder()&&h._cash==2235&&h._revenue==0&&h._financeRefundIncome==1235&&h._financeBook.CashIn==1235&&!h.TryCancelSupplierOrder(),"refund");});
 Test("actual journal file includes matching outcome and preserves unrelated data",()=>{string dir=Path.Combine(Path.GetTempPath(),"gh-integration340-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);try{var h=Pending();h.realFile=Path.Combine(dir,"save.ini");File.WriteAllText(h.realFile,"Other|Keep=yes\n");h.Sales(0,5000,0,0);Check(h.CommitSupplierDelivery(0,77),"commit");var data=ScriptSettings.Load(h.realFile);Check(data.GetValue("Other","Keep","")=="yes"&&data.GetValue("SupplierOutcomeV1","OrderTicks",0L)==77,"file");h.LoadFinanceBook(data);Check(h._financeBook.CashIn==5000,"journal");}finally{Directory.Delete(dir,true);}});
 Console.WriteLine("integration340 "+passed+"/"+passed+" PASS; production finance observation, receipt and journal/outcome serializers, GTA NOT RUN.");
 }
}
