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
 int _deliveryKind=-1,_deliveryQuantity=5,_deliverySupplier=1,_managerTier=0,_ownedStoragePallets=2,_parts=20,_completedSupplierDeliveries=3;
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
 /*<source:AvailableSupplierPallet>*/
 /*<source:CommitSupplierDelivery>*/
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
 static void Test(string n,Action a){Game.IsPaused=false;Game.Player.Character=new Ped{Position=new Vector3(200,0,0)};a();passed++;Console.WriteLine("PASS supplier "+n);}
 static DateTime N=DateTime.UnixEpoch.AddMinutes(1);
 static Harness Near(){Game.Player.Character.Position=new Vector3();return new Harness{_managerSupplierPallet=0,_supplierVisualStage=4};}
 static void Main(){
 foreach(int supplier in new[]{1,2}) Test("away receipt supplier "+supplier,()=>{var h=new Harness{_deliverySupplier=supplier};h.ProcessSupplierDelivery(N);Check(h._parts==25&&h._supplierInvoicesDue==650&&h._deliveryKind==-2&&h.spawns==0&&h._storedDeliveryCrates[0]==2,"receipt");h.ProcessSupplierDelivery(N.AddSeconds(1));Check(h._completedSupplierDeliveries==4&&h.saves==1,"duplicate");});
 Test("ETA pending",()=>{var h=new Harness{_deliveryRemainingGameSeconds=1};h.ProcessSupplierDelivery(N);Check(h.saves==0,"early");});
 foreach(int gate in new[]{0,1,2}) Test("manager eligibility gate "+gate,()=>{var h=new Harness();if(gate==0)h._managerTier=-1;if(gate==1)h._clockedIn=false;if(gate==2)h._placed=false;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h.spawns==0,"ineligible");});
 Test("pause prevents ready receipt and route work",()=>{var h=Near();Game.IsPaused=true;h.ProcessSupplierDelivery(N.AddDays(1));Check(h.saves==0&&h.departures==0,"pause");});
 foreach(bool prepaid in new[]{false,true}) Test("wholesale remains player only prepaid "+prepaid,()=>{var h=new Harness{_deliverySupplier=0,_deliveryPrepaid=prepaid};h.ProcessSupplierDelivery(N);Check(h._supplierVisualStage==7&&h.saves==0&&h.warehouseVisuals==1,"wholesale");});
 Test("wholesale missing pickup does not auto-credit",()=>{var h=new Harness{_deliverySupplier=0};h._wholesaleWarehousePickup.IsSet=false;h._physicalMuleDeliveries=false;h.ProcessSupplierDelivery(N);Check(h.saves==0,"wholesale fallback");});
 Test("aggregate capacity blocks then recovers",()=>{var h=new Harness{capacity=false};h.ProcessSupplierDelivery(N);Check(h._deliveryKind==-1&&h.saves==0,"capacity");h.capacity=true;h.ProcessSupplierDelivery(N);Check(h._deliveryKind==-2,"recover");});
 Test("full and missing pallets block",()=>{var h=new Harness();h._storedDeliveryCrates[0]=9;h.points[1].IsSet=false;h.ProcessSupplierDelivery(N);Check(h.saves==0,"slots");h.points[1].IsSet=true;h.ProcessSupplierDelivery(N);Check(h._storedDeliveryCrates[1]==1,"second pallet");});
 Test("player carrying is never stolen",()=>{var h=new Harness{_supplierVisualStage=6,_supplierPlayerCarrying=true};h.ProcessSupplierDelivery(N);Check(h.saves==0&&h.cleanups==0,"carry");});
 Test("near receipt requires actual pallet arrival",()=>{var h=Near();h._supplierDriver.Position=new Vector3(20,0,0);h.ProcessSupplierDelivery(N);Check(h.saves==0&&h.cleanups==1,"arrival");});
 Test("near receipt durable before visual cleanup",()=>{var h=Near();h.ProcessSupplierDelivery(N);Check(h.durable!=null&&h._parts==25&&h.departures==1&&h.refreshes==1,"near receipt");});
 foreach(int fault in new[]{0,1,2,3}) Test("near missing support/entity/layout "+fault,()=>{var h=Near();if(fault==0)h._storagePalletProps[0]=null;if(fault==1)h._supplierCrate=null;if(fault==2)h.layout=false;if(fault==3)h._supplierDriver.IsDead=true;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._deliveryKind==-1,"fault");});
 Test("save failure restores every accounting field then retries once",()=>{var h=new Harness{saveFails=true,_wholesaleCargoLoaded=true,_wholesaleCargoUnloaded=true};string before=h.State();Check(!h.CommitSupplierDelivery(0)&&before==h.State(),"rollback");h.saveFails=false;Check(h.CommitSupplierDelivery(0)&&!h.CommitSupplierDelivery(0)&&h.saves==2,"retry");});
 Test("prepaid obeys capacity and never bills twice",()=>{var h=new Harness{_deliveryPrepaid=true,capacity=false};Check(!h.CommitSupplierDelivery(0),"capacity prepaid");h.capacity=true;Check(h.CommitSupplierDelivery(0)&&h._supplierInvoicesDue==200,"prepaid invoice");});
 Test("stream out during route becomes single virtual receipt",()=>{var h=new Harness{_supplierVisualStage=3,_managerSupplierPallet=0};h.ProcessSupplierDelivery(N);Check(h.cleanups==1&&h.saves==1&&h.spawns==0,"stream out");});
 Test("manager removed during route retains order",()=>{var h=Near();h._managerTier=-1;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._deliveryKind==-1&&h.cleanups==1,"removed");});
 Test("blocked route defers without teleport or stock",()=>{var h=Near();h._supplierVisualStage=3;h._supplierWalkingNodes.Add(new Vector3(20,0,0));h.ProcessSupplierDelivery(N);Check(h.cleanups==1&&h._supplierDriver.Position.X==0&&h.saves==0,"blocked");});
 Test("failed save virtual retries are rate limited",()=>{var h=new Harness{saveFails=true};h.ProcessSupplierDelivery(N);h.ProcessSupplierDelivery(N.AddSeconds(1));Check(h.saves==1&&h._deliveryKind==-1,"retry budget");h.saveFails=false;h.ProcessSupplierDelivery(N.AddSeconds(10));Check(h.saves==2&&h._deliveryKind==-2,"retry saved");});
 Test("spawn attempts rate limited",()=>{var h=Near();h._supplierVisualStage=0;h._managerSupplierPallet=-1;h.ProcessSupplierDelivery(N);h.ProcessSupplierDelivery(N.AddSeconds(1));Check(h.spawns==1,"spawn budget");});
 Test("no manager direct handoff remains",()=>{var h=Near();h._managerTier=-1;h._managerSupplierPallet=-1;h._supplierDirectToPlayer=true;h.ProcessSupplierDelivery(N);Check(h.playerApproaches==1&&h.saves==0,"manual");});
 Test("near incomplete physical configuration retains order",()=>{var h=Near();h._physicalMuleDeliveries=false;h.ProcessSupplierDelivery(N);Check(h.saves==0,"near fallback");});
 Test("invalid pallet commit cannot mutate",()=>{var h=new Harness();string b=h.State();Check(!h.CommitSupplierDelivery(3)&&b==h.State(),"invalid slot");});
 Test("special stock receipt",()=>{var h=new Harness{_deliveryKind=2};Check(h.CommitSupplierDelivery(0)&&h._specialStock[2]==35&&h._parts==20,"special");});
 Test("atomic disk snapshot preserves original on writer failure",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(d);try{string p=Path.Combine(d,"save.ini");File.WriteAllText(p,"old");Check(!TryCommitBusinessSave(p,t=>{File.WriteAllText(t,"partial");throw new IOException("fault");},e=>{}),"failure");Check(File.ReadAllText(p)=="old"&&Directory.GetFiles(d).Length==1,"original snapshot");}finally{Directory.Delete(d,true);}});
 Test("atomic disk snapshot commits receipt and cleared order together",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());Directory.CreateDirectory(d);try{string p=Path.Combine(d,"save.ini");File.WriteAllText(p,"pending");Check(TryCommitBusinessSave(p,t=>File.WriteAllText(t,"stock=25;invoice=650;order=-2;unknown=preserved"),e=>{}),"commit");Check(File.ReadAllText(p).Contains("order=-2")&&Directory.GetFiles(d).Length==1,"reloaded snapshot");}finally{Directory.Delete(d,true);}});
 Test("failure before first snapshot leaves no invented receipt",()=>{string d=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString());try{string p=Path.Combine(d,"save.ini");Check(!TryCommitBusinessSave(p,t=>{throw new IOException("fault");},e=>{})&&!File.Exists(p),"new snapshot failure");}finally{Directory.Delete(d,true);}});
 Test("manager driver route ends at selected pallet and ignores player chase",()=>{var h=Near();h.BeginSupplierDriverWalk(N);Check(!h._supplierDirectToPlayer&&h._managerSupplierPallet==0&&h._supplierWalkingNodes.Count==2&&h._supplierWalkingNodes[1].X==0,"pallet route");});
 Test("manager route capacity loss retains order",()=>{var h=Near();h.capacity=false;h.BeginSupplierDriverWalk(N);Check(h.cleanups==1&&h._deliveryKind==-1&&h.saves==0,"route capacity");});
 Test("stale visual order identity cannot settle another order",()=>{var h=Near();h._supplierVisualOrderTicks=999;h.ProcessSupplierDelivery(N);Check(h.saves==0&&h._deliveryKind==-1,"stale receipt");});
 Test("virtual receipt grants a fresh finite visual budget",()=>{var h=new Harness();h.ProcessSupplierDelivery(N);Check(h._storageVisualAttempts==0&&h._nextStorageVisualAttempt==DateTime.MinValue,"visual retry");});
 Console.WriteLine($"supplier {passed}/{passed} PASS; production receipt/policy/route methods with deterministic world stubs; GTA testing NOT RUN");
 }
}
