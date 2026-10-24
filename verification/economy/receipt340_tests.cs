using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
class ScriptSettings {
 internal Dictionary<string,string> values=new Dictionary<string,string>();
 internal T GetValue<T>(string s,string k,T d)=>values.TryGetValue(s+"|"+k,out string v)?(T)Convert.ChangeType(v,typeof(T)):d;
 internal void SetValue<T>(string s,string k,T v){values[s+"|"+k]=Convert.ToString(v);}
 internal ScriptSettings Copy(){var d=new ScriptSettings();foreach(var p in values)d.values[p.Key]=p.Value;return d;}
}
static class Game {internal static bool IsPaused;}
class Harness {
 long _financeRebateIncome,_financeRefundIncome;
 /*<source:SupplierLineTotal>*/
 /*<source:SupplierCartCost>*/
 /*<source:TryPlaceSupplierOrder>*/
 /*<source:ApplySupplierInventory>*/
 /*<source:LoadSupplierOrderLines>*/
 /*<source:WriteSupplierOrderLines>*/
 /*<source:SupplierOutcome>*/
 SupplierOutcome _lastSupplierOutcome;long _financeActiveMs=12345;
 /*<source:CaptureSupplierOutcome>*/
 /*<source:LoadSupplierOutcome>*/
 /*<source:WriteSupplierOutcome>*/
 /*<source:CommitSupplierDelivery>*/
 /*<source:TryCancelSupplierOrder>*/
 /*<source:AdjustSupplierCart>*/
 /*<source:MenuPage>*/
 class WorkBay {internal bool IsSet=true;}
 int _deliveryKind=-2,_deliveryQuantity,_deliverySupplier=1,_selectedSupplier=1,_parts=10,_partCost=100,_ownedStoragePallets=4,_selection;
 long _deliveryCost,_deliveryDueUtcTicks,_managerSavings=17,_supplierInvoicesDue=12,_completedSupplierDeliveries,_cash=1000;
 double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds;
 bool _deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded,_palletVisualDirty;
 int[] _deliveryLines,_supplierCart=new int[4],_specialStock={5,6,7},_specialUnitCosts={90,160,125},_storedDeliveryCrates=new int[4],_deliveredPackageMasks=new int[4];
 MenuPage _menuPage=MenuPage.BuildSupplierOrder;
 const int CratesPerPallet=9,PartsPerPallet=25,MaxStoragePallets=4,ExtraPalletPrice=750;DateTime _nextSupplierVisualRetryUtc;
 bool capacity=true,saveFail,vehicle=true;int saves,cleanups;ScriptSettings disk;
 WorkBay point=new WorkBay();WorkBay[] extras={new WorkBay(),new WorkBay(),new WorkBay()};
 WorkBay _supplierPartsPallet=>point;WorkBay[] _extraStoragePallets=>extras;
 /*<source:StoragePalletPoint>*/
 /*<source:CanStoreShipment>*/
 /*<source:TotalStorageCapacity>*/
 bool HasWholesaleVehicle()=>vehicle;
 long InventoryPurchaseCost(int kind,int q)=>(long)(kind<0?_partCost:_specialUnitCosts[kind])*q;
 int SupplierDeliverySeconds(int s)=>s==0?120:s==1?60:20;
 string SupplierName(int s)=>"supplier";string DeliveryItemName(int k)=>"parts";void Notify(string s){}
 void SetSupplierPickupWaypoint(){}
 void DeleteWholesalePickupBlip(){}
 void DeleteSupplierVisuals(){cleanups++;}
 string savePath;int fileFault;long _equipmentSpend;
 /*<source:TryCommitBusinessSave>*/
 bool Save(){saves++;if(saveFail)return false;var next=disk==null?new ScriptSettings():disk.Copy();WriteSupplierOrderLines(next);WriteSupplierOutcome(next);
 next.SetValue("Business","Parts",_parts);for(int i=0;i<3;i++)next.SetValue("DetailedInventory","Stock"+i,_specialStock[i]);
 next.SetValue("Purchasing","Kind",_deliveryKind);next.SetValue("Purchasing","Quantity",_deliveryQuantity);
 next.SetValue("Bills","Due",_supplierInvoicesDue);next.SetValue("Business","Cash",_cash);
 if(savePath!=null&&!TryCommitBusinessSave(savePath,p=>{File.WriteAllLines(p,next.values.Select(kv=>kv.Key+"="+kv.Value));if(fileFault==1)throw new IOException("injected snapshot failure");},e=>{}))return false;
 disk=next;return true;}
 /*<source:TryBuyStoragePallet>*/
 /*<source:TryResetSupplierCargo>*/
 int _supplierVisualStage;bool _supplierPlayerCarrying;long _supplierVisualOrderTicks;
 /*<source:TryCommitWholesaleCargo>*/
 class ExitPed{internal int Handle;internal bool seated=true;internal bool IsInVehicle()=>seated;}
 class ExitVehicle{internal int Handle;}
 ExitPed _supplierDriver=new ExitPed();ExitVehicle _supplierTruck=new ExitVehicle();bool _supplierExitIssued,_supplierExitRetryIssued;int rearCalls,deferCalls;
 void BeginSupplierRearUnload(DateTime now){rearCalls++;_supplierVisualStage=8;}
 void DeferSupplierPlayerHandoff(DateTime now){deferCalls++;_supplierVisualStage=0;_supplierExitIssued=false;_supplierExitRetryIssued=false;}
 /*<source:UpdateSupplierDriverExit>*/
 static string folder;
 static int passed;static void Check(bool b,string n){if(!b)throw new Exception(n);}
 static void Test(string n,Action a){Game.IsPaused=false;GTA.Native.Function.Calls=0;a();passed++;Console.WriteLine("PASS receipt340 "+n);}
 static Harness Pending(int[] lines=null){var h=new Harness();Check(h.TryPlaceSupplierOrder(lines??new[]{2,3,4,1}),"place fixture");return h;}
 Harness Reload(){var h=new Harness{_deliveryKind=_deliveryKind,_deliveryQuantity=_deliveryQuantity,_deliverySupplier=_deliverySupplier,_deliveryCost=_deliveryCost,_deliveryDueUtcTicks=_deliveryDueUtcTicks,_deliveryRemainingGameSeconds=_deliveryRemainingGameSeconds};h.LoadSupplierOrderLines(disk.Copy());h.LoadSupplierOutcome(disk.Copy());return h;}
 static void Reject(Action a){try{a();throw new Exception("accepted malformed cart");}catch(InvalidDataException){}}
 static void Main(){folder=Path.Combine(Path.GetTempPath(),"gh-receipt340-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);try{

 Test("mixed receipt stores identity lines invoice and active timestamp",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;Check(h.CommitSupplierDelivery(0,id),"receipt");var o=h._lastSupplierOutcome;Check(o.OrderTicks==id&&o.Lines.SequenceEqual(new[]{2,3,4,1})&&o.Invoice==1235&&o.State==1&&o.ActiveMs==12345&&o.Pallet==0&&h._cash==1000,"outcome");});
 Test("last outcome survives completion and reload without recredit",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;h.CommitSupplierDelivery(0,id);var r=h.Reload();Check(r._lastSupplierOutcome.OrderTicks==id&&r._deliveryKind==-2&&!r.CommitSupplierDelivery(0,id)&&r.saves==0,"reload");});
 Test("new pending order does not erase last completed outcome",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;h.CommitSupplierDelivery(0,id);Check(h.TryPlaceSupplierOrder(new[]{1,0,0,0})&&h._lastSupplierOutcome.OrderTicks==id&&h._deliveryKind==-1,"last");});
 Test("old callback A cannot receive newly placed B",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;h.CommitSupplierDelivery(0,id);h.TryPlaceSupplierOrder(new[]{1,0,0,0});h._deliveryDueUtcTicks=id+1;int parts=h._parts;Check(!h.CommitSupplierDelivery(0,id)&&h._parts==parts&&h._lastSupplierOutcome.OrderTicks==id,"stale");});
 Test("zero legacy identity remains guarded rather than wildcard",()=>{var h=new Harness{_deliveryKind=-1,_deliveryQuantity=2,_deliveryDueUtcTicks=0};Check(!h.CommitSupplierDelivery(0,1)&&h.CommitSupplierDelivery(0,0)&&h._lastSupplierOutcome.OrderTicks==0,"legacy zero");});
 Test("failed receipt preserves previous terminal record",()=>{var h=Pending();h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks);var old=h._lastSupplierOutcome;h.TryPlaceSupplierOrder(new[]{1,0,0,0});h.saveFail=true;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&Object.ReferenceEquals(old,h._lastSupplierOutcome)&&h._deliveryQuantity==1,"rollback history");});
 Test("actual atomic writer failure preserves file stock liabilities and pending lines",()=>{var h=Pending();h.savePath=Path.Combine(folder,"Save.ini");Check(h.Save(),"initial");string before=File.ReadAllText(h.savePath);h.fileFault=1;long invoice=h._supplierInvoicesDue;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&File.ReadAllText(h.savePath)==before&&h._parts==10&&h._supplierInvoicesDue==invoice&&h._deliveryLines.SequenceEqual(new[]{2,3,4,1})&&h._lastSupplierOutcome==null,"failed save");});
 Test("after durable replacement an interruption cannot repeat receipt",()=>{var h=Pending();h.savePath=Path.Combine(folder,"Committed.ini");long id=h._deliveryDueUtcTicks;Check(h.CommitSupplierDelivery(0,id),"saved");string disk=File.ReadAllText(h.savePath);Check(disk.Contains("Purchasing|Kind=-2")&&disk.Contains("Business|Parts=12")&&disk.Contains("SupplierOutcomeV1|OrderTicks="+id)&&!h.CommitSupplierDelivery(0,id),"durable receipt");});
 Test("failed cancellation restores prior receipt history and prepaid cash",()=>{var h=Pending();h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks);var old=h._lastSupplierOutcome;h.TryPlaceSupplierOrder(new[]{1,0,0,0});h._deliveryPrepaid=true;long cash=h._cash;h.saveFail=true;Check(!h.TryCancelSupplierOrder()&&h._cash==cash&&Object.ReferenceEquals(old,h._lastSupplierOutcome)&&h._deliveryKind!=-2,"cancel rollback");});
 Test("unpaid cancellation records identity without refund or invoice",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;Check(h.TryCancelSupplierOrder()&&h._cash==1000&&h._supplierInvoicesDue==12&&h._lastSupplierOutcome.State==2&&h._lastSupplierOutcome.OrderTicks==id&&h._lastSupplierOutcome.Invoice==0&&!h.TryCancelSupplierOrder(),"cancel");});
 Test("prepaid cancellation refunds once and roundtrips its record",()=>{var h=Pending();h._deliveryPrepaid=true;long cost=h._deliveryCost;Check(h.TryCancelSupplierOrder()&&h._cash==1000+cost&&h._financeRefundIncome==cost&&!h.TryCancelSupplierOrder(),"refund");var r=h.Reload();Check(r._lastSupplierOutcome.Prepaid&&r._lastSupplierOutcome.Invoice==0&&r._lastSupplierOutcome.State==2,"record");});
 Test("prepaid receipt creates no second supplier liability",()=>{var h=Pending();h._deliveryPrepaid=true;Check(h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._supplierInvoicesDue==12&&h._lastSupplierOutcome.Invoice==0,"prepaid");});
 Test("missing history leaves legacy pending stock and order intact",()=>{var h=Pending();h.LoadSupplierOutcome(new ScriptSettings());Check(h._lastSupplierOutcome==null&&h._deliveryQuantity==10&&h._parts==10,"legacy history");});
 Test("malformed optional history does not discard valid pending order",()=>{var h=Pending();h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks);var d=h.disk.Copy();d.SetValue("SupplierOutcomeV1","Line0",-99);h.TryPlaceSupplierOrder(new[]{1,0,0,0});h.LoadSupplierOutcome(d);Check(h._lastSupplierOutcome==null&&h._deliveryQuantity==1&&h._deliveryKind==-1,"history invalid");});
 Test("receipt rejects paused or invalid mixed accounting",()=>{var h=Pending();Game.IsPaused=true;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks),"paused");Game.IsPaused=false;h._deliveryQuantity++;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._parts==10,"mismatch");});
 Test("invoice overflow leaves whole mixed order pending",()=>{var h=Pending();h._supplierInvoicesDue=long.MaxValue;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._deliveryLines!=null&&h._parts==10,"invoice overflow");});
 Test("refund overflow preserves prepaid order",()=>{var h=Pending();h._deliveryPrepaid=true;h._cash=long.MaxValue;Check(!h.TryCancelSupplierOrder()&&h._deliveryKind!=-2&&h._lastSupplierOutcome==null,"refund overflow");});
 Test("capacity arithmetic cannot wrap for extreme stored counters",()=>{var h=Pending();h._parts=int.MaxValue;h._specialStock=new[]{int.MaxValue,int.MaxValue,int.MaxValue};Check(!h.CanStoreShipment(10)&&!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks),"capacity overflow");});
 Test("full historical slots block despite free unit space",()=>{var h=Pending();for(int i=0;i<4;i++)h._storedDeliveryCrates[i]=9;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._parts==10,"parcel full");});
 Test("wholesale cannot commit before player unload checkpoint",()=>{var h=Pending();h._deliverySupplier=0;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks),"no warehouse automation");h._wholesaleCargoUnloaded=true;Check(h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks),"manual unload");});
 Test("failed wholesale loading restores player ownership and flags",()=>{var h=Pending();h._deliverySupplier=0;h._supplierVisualOrderTicks=h._deliveryDueUtcTicks;h._supplierPlayerCarrying=true;h._supplierVisualStage=6;h.saveFail=true;Check(!h.TryCommitWholesaleCargo(true)&&h._supplierPlayerCarrying&&h._supplierVisualStage==6&&!h._wholesaleCargoLoaded&&!h._wholesaleCargoUnloaded,"load rollback");});
 Test("successful load unload checkpoints never post inventory or invoices",()=>{var h=Pending();h._deliverySupplier=0;h._supplierVisualOrderTicks=h._deliveryDueUtcTicks;h._supplierPlayerCarrying=true;Check(h.TryCommitWholesaleCargo(true)&&h._wholesaleCargoLoaded&&!h._supplierPlayerCarrying,"loaded");Check(h.TryCommitWholesaleCargo(false)&&h._wholesaleCargoUnloaded&&!h._wholesaleCargoLoaded&&h._supplierPlayerCarrying&&h._parts==10&&h._supplierInvoicesDue==12,"unloaded");});
 Test("failed unload preserves vehicle cargo ownership",()=>{var h=Pending();h._deliverySupplier=0;h._supplierVisualOrderTicks=h._deliveryDueUtcTicks;h._wholesaleCargoLoaded=true;h._supplierVisualStage=9;h.saveFail=true;Check(!h.TryCommitWholesaleCargo(false)&&h._wholesaleCargoLoaded&&!h._wholesaleCargoUnloaded&&!h._supplierPlayerCarrying&&h._supplierVisualStage==9,"unload rollback");});
 Test("stale wholesale cargo identity cannot change ownership",()=>{var h=Pending();h._deliverySupplier=0;h._supplierVisualOrderTicks=h._deliveryDueUtcTicks+1;h._supplierPlayerCarrying=true;Check(!h.TryCommitWholesaleCargo(true),"stale cargo");});
 Test("failed pallet purchase restores cash spend and owned count",()=>{var h=new Harness{_ownedStoragePallets=1,saveFail=true};Check(!h.TryBuyStoragePallet()&&h._cash==1000&&h._equipmentSpend==0&&h._ownedStoragePallets==1,"pallet rollback");});
 Test("successful pallet purchase pays once within cap",()=>{var h=new Harness{_ownedStoragePallets=3};Check(h.TryBuyStoragePallet()&&h._cash==250&&h._equipmentSpend==750&&h._ownedStoragePallets==4&&!h.TryBuyStoragePallet(),"pallet purchase");});
 Test("blocked exit submits one normal and one fallback task then defers",()=>{var h=Pending();h._supplierVisualStage=2;for(int t=0;t<=41;t++)h.UpdateSupplierDriverExit(DateTime.UnixEpoch.AddSeconds(t),t);Check(GTA.Native.Function.Calls==2&&h.rearCalls==0&&h.deferCalls==1&&h._deliveryQuantity==10&&h._supplierInvoicesDue==12,"exit bounds");});
 Test("confirmed exit advances once caller leaves exit stage",()=>{var h=Pending();h.UpdateSupplierDriverExit(DateTime.UnixEpoch,5);Check(h.rearCalls==0,"still seated");h._supplierDriver.seated=false;h.UpdateSupplierDriverExit(DateTime.UnixEpoch,6);Check(h.rearCalls==1&&GTA.Native.Function.Calls==1,"exit confirmed");});
 Test("outcome writer preserves unrelated existing keys",()=>{var h=Pending();h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks);var d=h.disk.Copy();d.SetValue("OtherMod","Keep","yes");h.WriteSupplierOutcome(d);Check(d.GetValue("OtherMod","Keep","")=="yes","unknown key");});
 Test("negative receipt cost cannot mutate or produce malformed history",()=>{var h=Pending();h._deliveryCost=-1;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._lastSupplierOutcome==null&&h._parts==10,"negative cost");});
 Test("failed recovery retains cargo checkpoint and original pending identity",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;h._wholesaleCargoLoaded=true;h.saveFail=true;Check(!h.TryResetSupplierCargo()&&h._wholesaleCargoLoaded&&h._deliveryDueUtcTicks==id&&h._deliveryQuantity==10,"recover failed");});
 Test("successful recovery resets cargo only without new order or liability",()=>{var h=Pending();long id=h._deliveryDueUtcTicks;h._wholesaleCargoLoaded=true;Check(h.TryResetSupplierCargo()&&!h._wholesaleCargoLoaded&&h._deliveryDueUtcTicks==id&&h._parts==10&&h._supplierInvoicesDue==12,"recover");});
 Console.WriteLine("receipt340 "+passed+"/"+passed+" PASS; production guarded receipt, outcome, cargo checkpoints and atomic file helper, GTA NOT RUN.");
 }finally{Directory.Delete(folder,true);}}
}
namespace GTA.Native{enum Hash{TASK_LEAVE_VEHICLE}static class Function{internal static int Calls;internal static void Call(Hash h,params object[] args){Calls++;}}}
