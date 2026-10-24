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
 int _deliveryKind=-2,_deliveryQuantity,_deliverySupplier=1,_selectedSupplier=1,_parts=10,_partCost=100,_ownedStoragePallets=1,_selection;
 long _deliveryCost,_deliveryDueUtcTicks,_managerSavings=17,_supplierInvoicesDue=12,_completedSupplierDeliveries,_cash=1000;
 double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds;
 bool _deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded,_palletVisualDirty;
 int[] _deliveryLines,_supplierCart=new int[4],_specialStock={5,6,7},_specialUnitCosts={90,160,125},_storedDeliveryCrates={0},_deliveredPackageMasks={0};
 MenuPage _menuPage=MenuPage.BuildSupplierOrder;
 const int CratesPerPallet=9;DateTime _nextSupplierVisualRetryUtc;
 bool capacity=true,saveFail,vehicle=true;int saves,cleanups;ScriptSettings disk;
 WorkBay point=new WorkBay();WorkBay StoragePalletPoint(int i)=>point;
 bool CanStoreShipment(int q)=>capacity&&q>0&&q<=100;
 bool HasWholesaleVehicle()=>vehicle;
 long InventoryPurchaseCost(int kind,int q)=>(long)(kind<0?_partCost:_specialUnitCosts[kind])*q;
 int SupplierDeliverySeconds(int s)=>s==0?120:s==1?60:20;
 string SupplierName(int s)=>"supplier";string DeliveryItemName(int k)=>"parts";void Notify(string s){}
 void SetSupplierPickupWaypoint(){}
 bool DeleteWholesalePickupBlip()=>true;
 void DeleteSupplierVisuals(){cleanups++;}
 bool Save(){saves++;if(saveFail)return false;disk=new ScriptSettings();WriteSupplierOrderLines(disk);WriteSupplierOutcome(disk);return true;}
 static int passed;static void Check(bool b,string n){if(!b)throw new Exception(n);}
 static void Test(string n,Action a){Game.IsPaused=false;a();passed++;Console.WriteLine("PASS cart "+n);}
 static Harness Pending(int[] lines=null){var h=new Harness();Check(h.TryPlaceSupplierOrder(lines??new[]{2,3,4,1}),"place fixture");return h;}
 Harness Reload(){var h=new Harness{_deliveryKind=_deliveryKind,_deliveryQuantity=_deliveryQuantity,_deliverySupplier=_deliverySupplier,_deliveryCost=_deliveryCost,_deliveryDueUtcTicks=_deliveryDueUtcTicks,_deliveryRemainingGameSeconds=_deliveryRemainingGameSeconds};h.LoadSupplierOrderLines(disk.Copy());h.LoadSupplierOutcome(disk.Copy());return h;}
 static void Reject(Action a){try{a();throw new Exception("accepted malformed cart");}catch(InvalidDataException){}}
 static void Main(){
 Test("four items one order ETA and consolidated price",()=>{var h=Pending();Check(h._deliveryQuantity==10&&h._deliveryCost==1235&&h._deliveryRemainingGameSeconds==60&&h.saves==1&&h._parts==10&&h._supplierInvoicesDue==12,"order totals");});
 Test("submission copies draft without aliasing",()=>{int[] lines={1,2,3,4};var h=Pending(lines);lines[0]=99;Check(h._deliveryLines[0]==1,"alias");});
 Test("one receipt applies all inventory once",()=>{var h=Pending();Check(h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks),"receipt");Check(h._parts==12&&h._specialStock.SequenceEqual(new[]{8,10,8})&&h._supplierInvoicesDue==1247&&h._completedSupplierDeliveries==1&&h._storedDeliveryCrates[0]==1&&h._deliveryLines==null,"distribution");Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._supplierInvoicesDue==1247,"double receipt");});
 Test("pending cart roundtrip no offline catchup",()=>{var h=Pending();var r=h.Reload();Check(r._deliveryLines.SequenceEqual(new[]{2,3,4,1})&&r._deliveryRemainingGameSeconds==60&&r._parts==10,"reload");Check(r.CommitSupplierDelivery(0,r._deliveryDueUtcTicks)&&r._parts==12,"reload receipt");});
 Test("completed snapshot clears additive payload",()=>{var h=Pending();h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks);var r=h.Reload();Check(r._deliveryKind==-2&&r._deliveryLines==null&&!r.CommitSupplierDelivery(0,r._deliveryDueUtcTicks),"completion reload");});
 Test("capacity blocks whole cart at ordering and receipt",()=>{var h=new Harness{capacity=false};Check(!h.TryPlaceSupplierOrder(new[]{1,1,1,1})&&h.saves==0,"order capacity");h=Pending();h.capacity=false;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._parts==10&&h._deliveryQuantity==10,"receipt capacity");});
 Test("failed receipt restores four buckets and original cart",()=>{var h=Pending();h.saveFail=true;long invoice=h._supplierInvoicesDue;Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._parts==10&&h._specialStock.SequenceEqual(new[]{5,6,7})&&h._supplierInvoicesDue==invoice&&h._deliveryLines.SequenceEqual(new[]{2,3,4,1})&&h._deliveredPackageMasks[0]==0,"rollback");h.saveFail=false;Check(h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._completedSupplierDeliveries==1,"retry");});
 Test("failed placement retains no pending order and no savings drift",()=>{var h=new Harness{saveFail=true};Check(!h.TryPlaceSupplierOrder(new[]{2,3,4,1})&&h._deliveryKind==-2&&h._deliveryLines==null&&h._managerSavings==17&&h.cleanups==0,"placement rollback");});
 Test("second order cannot replace pending",()=>{var h=Pending();Check(!h.TryPlaceSupplierOrder(new[]{9,0,0,0})&&h._deliveryQuantity==10&&h.saves==1,"replace");});
 Test("wholesale assignment remains required",()=>{var h=new Harness{_selectedSupplier=0,vehicle=false};Check(!h.TryPlaceSupplierOrder(new[]{1,1,1,1}),"wholesale");h.vehicle=true;Check(h.TryPlaceSupplierOrder(new[]{1,1,1,1})&&h._deliverySupplier==0&&h._deliveryRemainingGameSeconds==120,"wholesale order");});
 Test("legacy pending single item loads unchanged",()=>{var h=new Harness{_deliveryKind=1,_deliveryQuantity=5,_deliveryCost=800,_deliveryPrepaid=true};h.LoadSupplierOrderLines(new ScriptSettings());Check(h._deliveryLines==null&&h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._specialStock[1]==11&&h._supplierInvoicesDue==12,"legacy prepaid");});
 Test("legacy pending survives additive empty payload",()=>{var h=new Harness{_deliveryKind=-1,_deliveryQuantity=3,_deliveryCost=300};var d=new ScriptSettings();h.WriteSupplierOrderLines(d);h.LoadSupplierOrderLines(d);Check(h._deliveryLines==null&&h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks)&&h._parts==13&&h._supplierInvoicesDue==312,"legacy roundtrip");});
 Test("cancel clears all lines without stock or invoice",()=>{var h=Pending();Check(h.TryCancelSupplierOrder()&&h._deliveryLines==null&&h._parts==10&&h._supplierInvoicesDue==12&&!h.TryCancelSupplierOrder(),"cancel");var r=h.Reload();Check(r._deliveryLines==null&&r._deliveryKind==-2,"cancel reload");});
 Test("failed cancel restores cart and prepaid refund exactly",()=>{var h=Pending();h._deliveryPrepaid=true;h.saveFail=true;Check(!h.TryCancelSupplierOrder()&&h._cash==1000&&h._deliveryLines!=null,"cancel rollback");h.saveFail=false;Check(h.TryCancelSupplierOrder()&&h._cash==2235&&!h.TryCancelSupplierOrder()&&h._cash==2235,"refund");});
 Test("pause blocks placement and cancellation",()=>{var h=new Harness();Game.IsPaused=true;Check(!h.TryPlaceSupplierOrder(new[]{1,1,1,1}),"paused placement");Game.IsPaused=false;h=Pending();Game.IsPaused=true;Check(!h.TryCancelSupplierOrder()&&h._deliveryLines!=null,"paused cancellation");});
 foreach(int[] bad in new int[][]{null,new[]{1},new[]{-1,0,0,0},new[]{101,0,0,0},new[]{50,50,1,0},new[]{int.MaxValue,0,0,0},new[]{0,0,0,0}}) Test("reject invalid/empty cart "+(bad==null?"null":string.Join(",",bad)),()=>{var h=new Harness();Check(!h.TryPlaceSupplierOrder(bad)&&h._deliveryKind==-2&&h.saves==0,"invalid");});
 foreach(string key in new[]{"Version","Line0","Line1","OrderTicks","Cost","Supplier"}) Test("malformed/mismatching payload "+key,()=>{var h=Pending();var d=h.disk.Copy();d.SetValue("SupplierOrderV1",key,key=="Version"?2:key=="Line0"?-1:999);var r=new Harness{_deliveryKind=h._deliveryKind,_deliveryQuantity=h._deliveryQuantity,_deliverySupplier=h._deliverySupplier,_deliveryCost=h._deliveryCost,_deliveryDueUtcTicks=h._deliveryDueUtcTicks};Reject(()=>r.LoadSupplierOrderLines(d));Check(r.saves==0&&r._deliveryLines==null,"fail closed");});
 Test("legacy downgrade cleared order cannot replay stale lines",()=>{var h=Pending();h._deliveryKind=-2;Reject(()=>h.LoadSupplierOrderLines(h.disk));});
 Test("quantity edits clamp and never write stock or disk",()=>{var h=new Harness();h.AdjustSupplierCart(-1);Check(h._supplierCart[0]==0,"floor");for(int i=0;i<200;i++)h.AdjustSupplierCart(1);Check(h._supplierCart[0]==100&&h._parts==10&&h.saves==0,"ceiling");h._selection=4;h.AdjustSupplierCart(1);Check(h._supplierCart[0]==100,"supplier selection");});
 Test("roundtrip preserves unknown keys",()=>{var h=Pending();var d=h.disk.Copy();d.SetValue("OtherMod","Keep","yes");h.WriteSupplierOrderLines(d);Check(d.GetValue("OtherMod","Keep","")=="yes","unknown keys");});
 Test("stale physical identity rejects whole mixed receipt",()=>{var h=Pending();Check(!h.CommitSupplierDelivery(0,h._deliveryDueUtcTicks+1)&&h._parts==10&&h._deliveryLines!=null,"identity");});
 Console.WriteLine($"cart {passed}/{passed} PASS; production submission/serialization/receipt/cancel against deterministic INI adapter; GTA NOT RUN");
 }
}
