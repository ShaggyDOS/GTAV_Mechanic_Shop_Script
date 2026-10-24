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
 void DeleteSupplierVisuals(){cleanups++;}
 bool Save(){saves++;if(saveFail)return false;disk=new ScriptSettings();WriteSupplierOrderLines(disk);WriteSupplierOutcome(disk);return true;}
 static int passed;static void Check(bool b,string n){if(!b)throw new Exception(n);}
 static void Test(string n,Action a){Game.IsPaused=false;a();passed++;Console.WriteLine("PASS procurement340 "+n);}
 static Harness Pending(int[] lines=null){var h=new Harness();Check(h.TryPlaceSupplierOrder(lines??new[]{2,3,4,1}),"place fixture");return h;}
 Harness Reload(){var h=new Harness{_deliveryKind=_deliveryKind,_deliveryQuantity=_deliveryQuantity,_deliverySupplier=_deliverySupplier,_deliveryCost=_deliveryCost,_deliveryDueUtcTicks=_deliveryDueUtcTicks,_deliveryRemainingGameSeconds=_deliveryRemainingGameSeconds};h.LoadSupplierOrderLines(disk.Copy());h.LoadSupplierOutcome(disk.Copy());return h;}
 static void Reject(Action a){try{a();throw new Exception("accepted malformed cart");}catch(InvalidDataException){}}

 int[] _supplierReviewLines;int _supplierReviewSupplier;long _supplierReviewCost;
 List<string> notices=new List<string>();
 void ShowStatusNotice(string title,params string[] rows){notices.Add(title+string.Join("\n",rows));}
 void NavigateBack(){_menuPage=MenuPage.BuildSupplierOrder;_selection=6;}
 /*<source:PrepareSupplierOrderReview>*/
 /*<source:ShowSupplierPurchaseReview>*/
 /*<source:ExecuteSupplierOrderReview>*/
 static void Main(){

 Test("review copies all four lines without saving or receiving",()=>{var h=new Harness();h._supplierCart=new[]{2,3,4,1};h.PrepareSupplierOrderReview();Check(h._menuPage==MenuPage.SupplierOrderReview&&h._supplierReviewLines.SequenceEqual(h._supplierCart)&&h.saves==0&&h._deliveryKind==-2&&h._parts==10,"review");h._supplierCart[0]=99;Check(h._supplierReviewLines[0]==2,"snapshot");});
 Test("empty and oversized cart cannot enter review",()=>{foreach(var lines in new[]{new int[4],new[]{100,1,0,0}}){var h=new Harness{_supplierCart=lines};h.PrepareSupplierOrderReview();Check(h._menuPage==MenuPage.BuildSupplierOrder&&h.saves==0,"invalid");}});
 Test("pending order cannot be overwritten by review",()=>{var h=Pending();h._supplierCart[0]=1;h.PrepareSupplierOrderReview();Check(h._supplierReviewLines==null&&h.saves==1,"pending");});
 Test("Back retains draft and posts no invoice",()=>{var h=new Harness{_supplierCart=new[]{2,1,0,0}};h.PrepareSupplierOrderReview();h._selection=2;h.ExecuteSupplierOrderReview();Check(h._menuPage==MenuPage.BuildSupplierOrder&&h._selection==6&&h._supplierCart[0]==2&&h.saves==0&&h._supplierInvoicesDue==12,"back");});
 Test("explicit confirmation places single mixed order only",()=>{var h=new Harness{_supplierCart=new[]{2,3,4,1}};h.PrepareSupplierOrderReview();h._selection=1;h.ExecuteSupplierOrderReview();Check(h._deliveryQuantity==10&&h._deliveryLines.SequenceEqual(new[]{2,3,4,1})&&h.saves==1&&h._supplierCart.Sum()==0&&h._menuPage==MenuPage.ActiveDeliveries&&h._supplierInvoicesDue==12,"confirm");h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==1,"repeat");});
 Test("changed supplier requires fresh review",()=>{var h=new Harness();h._supplierCart[0]=1;h.PrepareSupplierOrderReview();h._selectedSupplier=2;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==0&&h._menuPage==MenuPage.BuildSupplierOrder,"supplier");});
 Test("changed current quote requires fresh review",()=>{var h=new Harness();h._supplierCart[0]=1;h.PrepareSupplierOrderReview();h._partCost++;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==0&&h._deliveryKind==-2,"price");});
 Test("capacity revalidated after review",()=>{var h=new Harness();h._supplierCart[0]=1;h.PrepareSupplierOrderReview();h.capacity=false;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==0&&h._supplierReviewLines[0]==1&&h._supplierCart[0]==1,"capacity");});
 Test("wholesale assigned vehicle required at final confirmation",()=>{var h=new Harness{_selectedSupplier=0};h._supplierCart[0]=1;h.PrepareSupplierOrderReview();h.vehicle=false;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==0&&h._deliveryKind==-2,"vehicle");});
 Test("failed save retains reviewed order and original draft",()=>{var h=new Harness();h._supplierCart[0]=2;h.PrepareSupplierOrderReview();h.saveFail=true;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==1&&h._deliveryKind==-2&&h._supplierReviewLines[0]==2&&h._supplierCart[0]==2,"rollback");});
 Test("paused confirmation cannot create order",()=>{var h=new Harness();h._supplierCart[0]=1;h.PrepareSupplierOrderReview();Game.IsPaused=true;h._selection=1;h.ExecuteSupplierOrderReview();Check(h.saves==0,"paused");});
 Test("details explicitly distinguish invoice from cash",()=>{var h=new Harness();h._supplierCart[1]=2;h.PrepareSupplierOrderReview();Check(h.notices.Single().Contains("not a cash payment")&&h._cash==1000,"description");});
 Console.WriteLine("procurement340 "+passed+"/"+passed+" PASS; production review/confirmation, native input/rendering NOT RUN.");
 }
}
