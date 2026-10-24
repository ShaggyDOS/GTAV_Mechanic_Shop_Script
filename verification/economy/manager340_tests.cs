using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
namespace GTA.Native {
 enum Hash { TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,TASK_VEHICLE_PARK,TASK_LEAVE_VEHICLE }
 static class Function {internal static int calls;internal static void Call(Hash h, params object[] a){calls++;}}
}
struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}public static Vector3 operator-(Vector3 a,Vector3 b)=>new Vector3(a.X-b.X,a.Y-b.Y,a.Z-b.Z);public static Vector3 operator*(Vector3 a,float b)=>new Vector3(a.X*b,a.Y*b,a.Z*b);}
class Ped {internal bool alive=true,IsDead;internal Vector3 Position;internal int Handle;internal bool Exists()=>alive;internal bool IsInVehicle()=>false;}
class Prop {internal bool alive=true;internal bool Exists()=>alive;}
class Vehicle {internal bool alive=true;internal Vector3 Position,ForwardVector;internal int Handle;internal bool Exists()=>alive;}
static class Game {internal static bool IsPaused;internal static PlayerState Player=new PlayerState();internal class PlayerState {internal Ped Character=new Ped();}}
class Harness {
 class WorkBay {internal bool IsSet=true;internal Vector3 Position;internal float Heading;}
 const int CratesPerPallet=9,CarefulDrivingStyle=0;
 int _deliveryKind=-1,_deliveryQuantity=5,_deliverySupplier=1,_managerTier=0,_ownedStoragePallets=2,_parts=20;long _completedSupplierDeliveries=3;
 int[] _specialStock={10,20,30},_storedDeliveryCrates={1,0},_deliveredPackageMasks={1,0};
 int _storageVisualAttempts=5;DateTime _nextStorageVisualAttempt=DateTime.MaxValue;long _supplierVisualOrderTicks=123;
 long _deliveryDueUtcTicks=123,_deliveryCost=450,_supplierInvoicesDue=200;
 double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds=7;
 bool _clockedIn=true,_placed=true,_deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded,_palletVisualDirty;
 float _remoteDistanceMeters=100;Vector3 _location;
 int _supplierVisualStage,_managerSupplierPallet=-1,_supplierRouteIndex;
 bool _supplierPlayerCarrying,_supplierDepartIssued,_supplierExitIssued,_supplierDirectToPlayer,_supplierBoxAttachedToDriver=true;
 DateTime _supplierStageAt=DateTime.UnixEpoch,_nextSupplierVisualRetryUtc=DateTime.MinValue;
 WorkBay _wholesaleWarehousePickup=new WorkBay(),_supplierTruckSpawn=new WorkBay(),_supplierTruckPark=new WorkBay(),_supplierPartsPallet=new WorkBay();
 WorkBay[] points={new WorkBay(),new WorkBay()};
 Ped _supplierDriver=new Ped();Vehicle _supplierTruck=new Vehicle();Prop _supplierCrate=new Prop(),_wholesaleWarehousePallet;
 Prop[] _storagePalletProps={new Prop(),new Prop()};List<Vector3> _supplierWalkingNodes=new List<Vector3>();
 bool _physicalMuleDeliveries=true,_routeRecoveryEnabled=true,_safeTeleportOnStall=true;
 bool capacity=true,route=true,layout=true,saveFails;int saves,spawns,departures,refreshes,cleanups,playerApproaches,warehouseVisuals;string durable;
 /*<source:TryCommitBusinessSave>*/
 /*<source:ManagerSupplierEligible>*/
 /*<source:SupplierPlayerAway>*/
 /*<source:SupplierPhysicalRange>*/
 /*<source:AvailableSupplierPallet>*/
 /*<source:SupplierOutcome>*/
 SupplierOutcome _lastSupplierOutcome;long _financeActiveMs=12345;
 /*<source:CaptureSupplierOutcome>*/
 /*<source:SupplierLineTotal>*/
 bool _supplierExitRetryIssued;
 /*<source:UpdateSupplierDriverExit>*/
 /*<source:CommitSupplierDelivery>*/
    /*<source:ApplySupplierInventory>*/
    int[] _deliveryLines;
 /*<source:TryManagerSupplierDeposit>*/
 /*<source:ProcessSupplierDelivery>*/
 WorkBay StoragePalletPoint(int i)=>points[i];bool CanStoreShipment(int q)=>capacity;
 static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
 bool Save(){saves++;if(saveFails)return false;durable=State();return true;}
 string State()=>string.Join("|",_parts,string.Join(",",_specialStock),_supplierInvoicesDue,_completedSupplierDeliveries,_deliveryKind,_deliveryQuantity,_deliveryCost,_deliveryDueUtcTicks,_deliveryRemainingGameSeconds,_supplierTimerSaveSeconds,_deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded,string.Join(",",_storedDeliveryCrates),string.Join(",",_deliveredPackageMasks),_palletVisualDirty);
 void Notify(string s){}string DeliveryItemName(int k)=>"parts";
 void DeleteSupplierVisuals(){cleanups++;_supplierVisualStage=0;_managerSupplierPallet=-1;_supplierPlayerCarrying=false;}
 void DeferSupplierPlayerHandoff(DateTime n){DeleteSupplierVisuals();_nextSupplierVisualRetryUtc=n.AddSeconds(30);}
 void DeleteOwnedProp(ref Prop p){p=null;}
 void MaintainSupplierCarryPose(DateTime n){}void RefreshWholesaleCargoVisual(){}void UpdateWholesalePalletVisual(){warehouseVisuals++;}
 bool MuleRouteConfigured()=>route;
 bool StartMuleDelivery(DateTime n){spawns++;return false;}
 void UpdateSupplierPlayerApproach(DateTime n){playerApproaches++;}
 void BeginSupplierRearUnload(DateTime n){_supplierVisualStage=8;}
 void GiveBoxToDriver(DateTime n){BeginSupplierDriverWalk(n);}
 /*<source:BeginSupplierDriverWalk>*/
 WorkBay[] _supplierDriverWaypoints={new WorkBay{Position=new Vector3(1,2,0)}};
 WorkBay _supplierHandoff=new WorkBay();
 DateTime _supplierPlayerApproachDeadline,_nextSupplierPlayerRepath;int _supplierPlayerRepaths;
 void TaskSupplierWalkingNode(){}void Log(string s,Exception e){}
 void BeginSupplierTruckDeparture(DateTime n){departures++;_supplierVisualStage=5;}
 void UpdateSupplierPalletVisual(){}void RefreshStoragePalletAfterDeposit(){refreshes++;}
 bool TryStoredBoxTransform(int s,int b,bool actual,out Vector3 p,out float h){p=new Vector3();h=0;return layout;}
 static int passed;static void Check(bool b,string n){if(!b)throw new Exception(n);}
 static void Test(string n,Action a){Game.IsPaused=false;Game.Player.Character=new Ped{Position=new Vector3(200,0,0)};a();passed++;Console.WriteLine("PASS manager340 "+n);}
 static DateTime N=DateTime.UnixEpoch.AddMinutes(1);
 static Harness Near(){Game.Player.Character.Position=new Vector3();return new Harness{_managerSupplierPallet=0,_supplierVisualStage=4};}
 /*<source:SupplierReceiptBlocker>*/
 /*<source:MuleStageName>*/
 /*<source:SupplierLiveState>*/
 static void Main(){

 foreach(int supplier in new[]{1,2}) Test("offsite mixed order one liability and outcome supplier "+supplier,()=>{var h=new Harness{_deliverySupplier=supplier,_deliveryLines=new[]{2,3,1,2},_deliveryQuantity=8,_deliveryCost=870};h.ProcessSupplierDelivery(N);Check(h._parts==22&&h._specialStock.SequenceEqual(new[]{13,21,32})&&h._supplierInvoicesDue==1070&&h._lastSupplierOutcome.Lines.SequenceEqual(new[]{2,3,1,2})&&h._lastSupplierOutcome.OrderTicks==123&&h._lastSupplierOutcome.Invoice==870,"mixed");h.ProcessSupplierDelivery(N.AddSeconds(30));Check(h.saves==1&&h._supplierInvoicesDue==1070,"duplicate");});
 Test("nearby physical to away virtual preserves order identity",()=>{var h=Near();h._supplierVisualStage=2;Game.Player.Character.Position=new Vector3(101,0,0);h.ProcessSupplierDelivery(N);Check(h._lastSupplierOutcome.OrderTicks==123&&h._deliveryKind==-2&&h.cleanups==1&&h.saves==1,"transition");});
 Test("stale nearby manager cannot touch accounting",()=>{var h=Near();h._supplierVisualOrderTicks=124;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._deliveryKind!=-2&&h._parts==20,"stale nearby");});
 Test("manager removed while walking retains pending order",()=>{var h=Near();h._managerTier=-1;h._supplierVisualStage=3;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h.cleanups==1&&h._deliveryQuantity==5,"removed");});
 Test("closed shop never gets offsite manager receipt",()=>{var h=new Harness{_clockedIn=false};h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._deliveryKind!=-2&&h._lastSupplierOutcome==null,"closed");});
 Test("nearby manager route configuration hold is explicit",()=>{var h=Near();h.route=false;Check(h.SupplierReceiptBlocker().Contains("configure physical Mule"),"route hold");});
 Test("capacity hold explains both independent limits",()=>{var h=Near();h.capacity=false;Check(h.SupplierReceiptBlocker().Contains("unit capacity")&&h.SupplierReceiptBlocker().Contains("parcel slots"),"capacity reason");});
 Test("player carrying beats automatic receipt even offsite",()=>{var h=new Harness{_supplierVisualStage=6,_supplierPlayerCarrying=true};h.ProcessSupplierDelivery(N);Check(h.saves==0&&h.SupplierReceiptBlocker().Contains("Player owns"),"carrying");});
 Test("manager live stage distinguishes pallet drop from player handoff",()=>{var h=Near();Check(h.MuleStageName()=="MANAGER PALLET DROP"&&h.SupplierLiveState()=="PALLET DROP","manager state");h._managerSupplierPallet=-1;Check(h.SupplierLiveState()=="PLAYER HANDOFF","player state");});
 Test("wholesale remains manual with hired manager and saved cargo",()=>{var h=new Harness{_deliverySupplier=0,_wholesaleCargoLoaded=true};h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._supplierVisualStage==9&&h.SupplierReceiptBlocker().Contains("Player must return"),"wholesale");});
 Test("pause prevents new mixed outcome and cleanup",()=>{var h=Near();Game.IsPaused=true;h.ProcessSupplierDelivery(N.AddDays(10));Check(h.saves==0&&h._lastSupplierOutcome==null&&h.cleanups==0,"pause");});
 Test("offsite failed save retains identity and no terminal outcome",()=>{var h=new Harness{saveFails=true};h.ProcessSupplierDelivery(N);Check(h._deliveryDueUtcTicks==123&&h._deliveryQuantity==5&&h._lastSupplierOutcome==null&&h._supplierInvoicesDue==200,"failed save");});
 Console.WriteLine("manager340 "+passed+"/"+passed+" PASS; production nearby/offsite shared transaction and eligibility, GTA NOT RUN.");
 }
}
