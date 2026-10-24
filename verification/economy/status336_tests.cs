using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GTA.Native {
 enum Hash { IS_PED_IN_ANY_VEHICLE,TASK_ENTER_VEHICLE,TASK_VEHICLE_DRIVE_WANDER,TASK_LEAVE_VEHICLE,IS_SPHERE_VISIBLE,SET_PED_INTO_VEHICLE,TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,TASK_VEHICLE_PARK,IS_PED_IN_VEHICLE,IS_VEHICLE_SEAT_FREE,GET_VEHICLE_NUMBER_OF_PASSENGERS,IS_POSITION_OCCUPIED,CLEAR_PED_TASKS,TASK_START_SCENARIO_IN_PLACE,TASK_STAND_STILL }
 static class Function {internal static bool workerInside,workerAny;internal static int parks,exits;internal static bool playerInside,driverFree=true,obstacle,throwQuery;internal static int passengers,calls,animations;internal static T Call<T>(Hash h,params object[] args){calls++;if(throwQuery)throw new IOException("query failure");object v=h==Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS?(object)passengers:h==Hash.IS_PED_IN_VEHICLE?((int)args[0]==77?workerInside:playerInside):h==Hash.IS_PED_IN_ANY_VEHICLE?workerInside||workerAny:h==Hash.IS_VEHICLE_SEAT_FREE?driverFree:obstacle;return (T)v;}internal static void Call(Hash h,params object[] args){calls++;if(h==Hash.TASK_VEHICLE_PARK)parks++;if(h==Hash.TASK_LEAVE_VEHICLE)exits++;if(h==Hash.TASK_START_SCENARIO_IN_PLACE)animations++;}}
}

struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
class Ped {internal int Handle=9;internal float Heading;internal bool IsDead,IsPersistent,BlockPermanentEvents,AlwaysKeepTask;internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
class Vehicle {internal float Speed;internal bool IsPersistent;internal int Handle=4,moves;internal bool alive=true,IsDead,failHeading;internal Vector3 backing,Velocity;internal Vector3 Position{get=>backing;set{backing=value;moves++;}}internal float heading;internal float Heading{get=>heading;set{if(failHeading)throw new IOException("heading failure");heading=value;}}internal bool Exists()=>alive;}
class Blip {}
class ScriptSettings {
 internal Dictionary<string,string> values=new Dictionary<string,string>();string file;
 internal T GetValue<T>(string section,string key,T fallback){return values.TryGetValue(section+"|"+key,out string value)?(T)Convert.ChangeType(value,typeof(T),System.Globalization.CultureInfo.InvariantCulture):fallback;}
 internal void SetValue<T>(string section,string key,T value){values[section+"|"+key]=Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture);}
 internal static ScriptSettings Load(string path){var d=new ScriptSettings{file=path};string section="";if(File.Exists(path))foreach(string line in File.ReadAllLines(path)){if(line.StartsWith("[")&&line.EndsWith("]"))section=line.Substring(1,line.Length-2);else{int eq=line.IndexOf('=');if(eq>=0)d.values[section+"|"+line.Substring(0,eq)]=line.Substring(eq+1);}}return d;}
 internal bool Save(){var lines=new List<string>();foreach(var group in values.GroupBy(k=>k.Key.Split('|')[0])){lines.Add("["+group.Key+"]");foreach(var kv in group)lines.Add(kv.Key.Split('|')[1]+"="+kv.Value);}File.WriteAllLines(file,lines);return true;}
}

static class Game {internal static bool IsPaused;internal static int GameTime=100;internal static PlayerState Player=new PlayerState();internal class PlayerState {internal Ped Character=new Ped();}}
class Model {internal bool IsLoaded=true,IsInCdImage=true,IsPed=true,IsVehicle=true;internal int requests;internal void Request(){requests++;}}
static class World {internal static bool failVehicle,failPed;internal static int vehicles,peds;internal static Vehicle CreateVehicle(Model m,Vector3 p,float h){vehicles++;return failVehicle?null:new Vehicle{Position=p,Heading=h};}internal static Ped CreatePed(Model m,Vector3 p,float h){peds++;return failPed?null:new Ped{Position=p,Heading=h};}}
class Harness {
 // Diagnostics-only sink adapter; existing gameplay assertions stay unchanged.
 long BeginCustomerVehicleTrace(CustomerOrder order,int operation)=>-1;
 void EndCustomerVehicleTrace(CustomerOrder order,long sequence){}
 /*<source:MenuPage>*/
 /*<source:OpenServiceBayProfile>*/
 /*<source:CustomerBayAvailable>*/
 /*<source:CanStartNextIntake>*/
 /*<source:ShowCustomerOrderStatus>*/
 int _customerQueueCapacity=3;int EffectiveQueueCapacity()=>3;int StagedCount()=>(_stagedCustomerOrder==null?0:1)+(_stagedThirdCustomerOrder==null?0:1);bool _waitingForDropoff;
 /*<source:UpdateOneCustomerOrder>*/
 /*<source:FailVehicleTransfer>*/
 /*<source:ProcessPaidReturnRecovery>*/
 /*<source:VehicleDestination>*/
 /*<source:RetryPaidCustomerReturns>*/
 /*<source:EffectiveWorkRouteMask>*/
 /*<source:WorkRouteStatus>*/
 /*<source:WarnEmptyWorkRoute>*/
 /*<source:BuildRoute>*/
 /*<source:BuildMechanicPickupExitRoute>*/
 readonly HashSet<int> _warnedEmptyWorkRoutes=new HashSet<int>();
 WorkBay[] _serviceRoute={new WorkBay(),new WorkBay(),new WorkBay(),new WorkBay(),new WorkBay()};WorkBay _walkCheckpoint=new WorkBay();
 Vector3 GetBreakSpot(int slot)=>new Vector3(-20,0,0);
 int _vehicleTransferTimeout=80;const int RecoveryDrivingStyle=786603;

 Ped AssignedWorker(CustomerOrder o)=>o.MechanicSlot>=0&&o.MechanicSlot<_mechanics.Count?_mechanics[o.MechanicSlot].Worker:null;
 WorkBay[] _bayApproaches={new WorkBay(),new WorkBay(),new WorkBay()};
 bool _customEmployeeWalkingPaths;bool _fineParkingEnabled=true;int _parkingTimeoutSeconds=30,_driveArrivalTimeout=90;float _parkingRadius=2;
 WorkBay _customerApproach=new WorkBay(),_customerQueue=new WorkBay();
 void BeginCustomerExit(CustomerOrder o){o.Stage=CustomerStage.ExitingVehicle;}void AdvanceCustomerWalkingNode(CustomerOrder o){}
 bool CustomerArrived(CustomerOrder o,Vector3 target)=>false;
 void WalkCustomer(CustomerOrder o,Vector3 target,CustomerStage stage){o.Stage=stage;}
 void HandleCustomerStall(CustomerOrder o,Vector3 target){}bool ProgressVehicleWalk(CustomerOrder o)=>false;
 /*<source:MechanicInCustomerCar>*/ bool MechanicOutOfCar(CustomerOrder o)=>false;
 void BeginEnterVehicle(CustomerOrder o,bool returning){}void StartValetDeparture(CustomerOrder o,bool outbound){}
 void BeginVehicleDrive(CustomerOrder o,bool outbound){}void UpdateVehicleApproach(CustomerOrder o,bool inbound){}
 /*<source:UpdateVehicleParking>*/
 List<Vector3> BuildMechanicReturnFromDropoffRoute(int slot)=>new List<Vector3>();

 /*<source:CustomerSpawnRetryReady>*/
 /*<source:DeferCustomerSpawn>*/
 /*<source:SpawnCustomerOrder>*/
 /*<source:SpawnRoadsideWaitingCustomer>*/
 /*<source:ActivateRoadsideWaitingCustomer>*/
 /*<source:ValidateStagedCustomer>*/
 /*<source:UpdateStagedQueueTravel>*/
 /*<source:GetQueueWaitingSpot>*/
 /*<source:AddActiveOrder>*/
 bool _customerSpawnRetryPending;int _customerSpawnFailedAt,_customerSpawnRetryDelayMs;
 CustomerOrder _stagedCustomerOrder,_stagedThirdCustomerOrder;
 bool _driveInEnabled=true,queueBlocked;int _waitingCustomerRequests=2;float _queueSpawnSafetyMeters=5,_driveSpeed=5;const int CarefulDrivingStyle=786603;
 Model _customerPedModel=new Model(),_customerVehicleModel=new Model();Random _customerRandom=new Random(123);
 WorkBay _customerDriveSpawn=new WorkBay{IsSet=true,Position=new Vector3(100,0,0)},_queueCar2=new WorkBay{IsSet=true,Position=new Vector3(120,0,0)},_queueCar3=new WorkBay{IsSet=true,Position=new Vector3(140,0,0)};
 bool QueueParkingOccupied(WorkBay spot)=>queueBlocked;
 void CancelCreatedCustomer(CustomerOrder o){if(o!=null){if(o.Vehicle!=null)o.Vehicle.alive=false;if(o.Customer!=null)o.Customer.alive=false;}}
 void SyncCustomerBlip(CustomerOrder o){}void StartCustomerWalkIn(CustomerOrder o){o.Stage=CustomerStage.ToCounter;}

 /*<source:ExecuteServiceBayProfile>*/
 int _serviceBaySlot,_selection,backs,editorBay;MenuPage _menuPage,_serviceBayParent;WorkBay editedPoint;
 void NavigateBack(){backs++;}
 void OpenPointEditor(WorkBay p,string label,int bay,bool route){editedPoint=p;editorBay=bay;}
 string Status(WorkBay p)=>p.IsSet?"SET":"UNSET";string FormatPosition(Vector3 p)=>p.X.ToString();
 void ShowStatusNotice(string title,params string[] lines){foreach(string line in lines)notices.Add(line);}

    /*<source:TeleportAttemptDue>*/
    /*<source:TeleportTargetAvailable>*/
    /*<source:ProcessTeleportOrder>*/
    /*<source:ReleaseRecoveredMechanic>*/
    /*<source:CanReturnCustomerCar>*/
    /*<source:NeedsDropoffSoon>*/
    /*<source:IntakeInProgress>*/
    /*<source:UpdateEmployeeActivity>*/
    const int PhaseToCheckpoint=1,PhaseToBreak=2,PhaseToBay=4,PhaseReturningToBreak=6;
    bool _breakAnimationsEnabled=true,_smoothMechanicWaypoints=true,_safeTeleportOnStall=true,_customMechanicJobRoutes;
    int _stallSeconds=9;string[] _breakScenarios={"idle"};
    /*<source:MechanicOnVehicleTask>*/
    int CountSet(WorkBay[] route)=>route.Count(p=>p.IsSet);WorkBay[] _arrivalRoute=new WorkBay[0];bool _pickupUsesArrivalExit;
    void StartRouteLeg(Mechanic m,int slot,int phase){m.ArrivalPhase=phase;}
    void WalkCurrentNode(Mechanic m,int slot,DateTime n){}void PauseBlockedWorker(Mechanic m,int slot,Vector3 p){}
    bool RecoverBlockedWorker(Mechanic m,int slot,Vector3 p,DateTime n)=>false;
    void IssueEmployeeWalkTask(Ped p,Vector3 v,float h,bool straight=false,bool direct=false){}
    bool DropoffOccupied(Vehicle except)=>pickupBlocked;
    bool pickupBlocked;
    WorkBay _customerDropoff=new WorkBay{IsSet=true,Position=new Vector3(0,0,0),Heading=180};

    /*<source:TrySellPart>*/
    /*<source:TrySellParts>*/
    /*<source:PartsSaleTotal>*/
    /*<source:PartsSalePreview>*/
    /*<source:TryCommitBusinessSave>*/
    /*<source:RepairLedgerStamp>*/
    /*<source:ToggleBusinessShift>*/
    double _customerDemandElapsed;
    /*<source:ValidRepairPromise>*/
    /*<source:LoadRepairPromises>*/
    /*<source:CaptureRepairPromises>*/
    /*<source:WriteRepairPromises>*/
    /*<source:ParkRuntimeRepairPromises>*/
    /*<source:RememberCancelledRepair>*/
    /*<source:ApplySavedCustomerPromise>*/
    /*<source:SavedRepairPromise>*/
    /*<source:RepairQuoteHasStock>*/
    /*<source:TrySettleRepair>*/
    /*<source:RestoreSavedNonphysicalRepairs>*/
    const int MaxSavedRepairPromises=16;
    readonly List<SavedRepairPromise> _pendingRepairPromises=new List<SavedRepairPromise>();
    /*<source:Mechanic>*/
    /*<source:CustomerStage>*/
    /*<source:CustomerOrder>*/
    /*<source:RemoteJob>*/
    /*<source:WorkBay>*/
    /*<source:RepairQuote>*/
    /*<source:InitialiseRepairPricing>*/
    /*<source:EstimatedUnitAcquisitionCost>*/
    /*<source:CalculateRepairQuote>*/
    /*<source:ReserveRepairUnits>*/
    /*<source:ReserveQuotedUnits>*/
    /*<source:UnreservedRepairStock>*/
    /*<source:TryCreateRepairQuote>*/
    /*<source:TryRetainLegacyRepairQuote>*/
    /*<source:CanCreditRepair>*/
    /*<source:TryConsumeQuotedSupplies>*/
    /*<source:FreeStockForService>*/
    /*<source:ConsumeRepairSupplies>*/
    /*<source:ServiceQuoteBaseline>*/
    /*<source:RepairQuoteBreakdown>*/
    /*<source:RecordRepairPricing>*/
    /*<source:MechanicWage>*/
    /*<source:ServiceName>*/
    /*<source:AssignCustomerMechanic>*/
    /*<source:ProcessOneCustomerRepair>*/
    /*<source:ProcessEconomy>*/
    /*<source:ProcessRemoteOperations>*/
    /*<source:ProcessCustomerRepair>*/
    private const int PhaseRouteBlocked=7,PhaseWorking=5,PhaseAtBreak=3,VisualBayCount=3;
    private static string DataDirectory;
    long _partsSalesRevenue,_partsSalesUnitsSold;int _lastPartsSaleGameTime=int.MinValue;bool _bootGameplayActivated=true;
    string DeliveryItemName(int kind)=>new[]{"Universal","Filters","Brakes","Diagnostics"}[kind+1];
    long _remoteJobsCompleted,_remotePartsConsumed;
    long _cash,_revenue,_customerRevenue,_remoteSales,_supplierInvoicesDue=567,_partsSpend=31,_payrollSpend=32;
    int _parts=10,_completed,_customerCompleted,_lastCustomerOrderId;
    int[] _specialStock={10,10,10},_specialUnitCosts={90,160,125};
    readonly long[] _unitCostEstimates=new long[4];ScriptSettings _configuration=new ScriptSettings();
    int _partCost=100,_jobRevenue=500,_wagePerEmployee=250,_jobSeconds=120,_betweenJobsSeconds=10,_wagePeriodSeconds=300;
    bool _toolSetPurchased,_diagnosticScannerPurchased,_automaticLotMode,_vehicleTransfersEnabled=true,_remoteMode,_clockedIn=true,_placed,_customerServiceEnabled,_receptionistHired;
    int _managerTier=-1,_spawnDistance=65,_receptionistWage,_receptionistWageBonus,_overheadPeriodSeconds=600,_rentAndUtilities,_payrollDueSeconds=300;
    long _receptionWagesDue,_managerWagesDue,_rentUtilitiesDue;double _receptionWageUnpaidSeconds,_managerWageAgeSeconds,_rentUnpaidSeconds,_payrollClock,_overheadClock;
    bool _receptionOverdueNotified;HashSet<int> _mechanicOverdueNotified=new HashSet<int>();Vector3 _location;
    double _remoteIdleGameSeconds;int _remoteJobGameSeconds=180;DateTime _nextCustomerAt;string _lastOrderOutcome;
    List<Mechanic> _mechanics=new List<Mechanic>();List<CustomerOrder> _activeCustomerOrders=new List<CustomerOrder>();List<RemoteJob> _remoteJobs=new List<RemoteJob>();
    WorkBay[] _bays={new WorkBay{IsSet=true},new WorkBay{IsSet=true},new WorkBay{IsSet=true}},_vehicleParkingBays={new WorkBay(),new WorkBay(),new WorkBay()};
    CustomerOrder _customerOrder;int saves,invoiceWrites;List<string> notices=new List<string>();
    long savedCash,savedRevenue;int savedParts;
    bool saveSuccess=true;int errors;string StatePath=>Path.Combine(DataDirectory,"Save.ini");
    bool Save(){saves++;bool ok=TryCommitBusinessSave(StatePath,p=>{var d=ScriptSettings.Load(p);d.SetValue("Business","Cash",_cash);d.SetValue("Business","Revenue",_revenue);d.SetValue("Business","Parts",_parts);d.SetValue("Business","Completed",_completed);d.SetValue("Business","CustomerCompleted",_customerCompleted);d.SetValue("Business","CustomerRevenue",_customerRevenue);d.SetValue("Business","RemoteRevenue",_remoteSales);d.SetValue("Business","RemoteCompleted",_remoteJobsCompleted);d.SetValue("Business","RemoteParts",_remotePartsConsumed);d.SetValue("Business","LastId",_lastCustomerOrderId);d.SetValue("Business","PartsSalesRevenue",_partsSalesRevenue);d.SetValue("Business","PartsSalesUnitsSold",_partsSalesUnitsSold);d.SetValue("Business","ManagerTier",_managerTier);d.SetValue("Business","Mechanics",_mechanics.Count);for(int i=0;i<3;i++)d.SetValue("Stock","Stock"+i,_specialStock[i]);for(int i=0;i<_mechanics.Count;i++){d.SetValue("Worker"+i,"XP",_mechanics[i].Xp);d.SetValue("Worker"+i,"Jobs",_mechanics[i].JobsCompleted);}WriteRepairPromises(d);d.Save();if(!saveSuccess)throw new IOException("injected failure after snapshot before replace");},e=>errors++);if(ok){savedCash=_cash;savedRevenue=_revenue;savedParts=_parts;}return ok;}

    void Notify(string text){notices.Add(text);}void Log(string text,Exception error){}
    int MechanicLevel(Mechanic m)=>Math.Min(10,1+m.Xp/200);
    bool SpecialistMatches(Mechanic m,int k)=>m.Specialty!=0&&m.Specialty==k;
    int ManagerLevel()=>_managerTier<0?0:_managerTier+1;
    int ManagerPeriodWage()=>_managerTier<0?0:200+_managerTier*125;
    long ManagerAdjustedExpense(long amount)=>amount;
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    /*<source:ComputeAutomaticBay>*/
 bool _autoLotRightSide=true;float _autoLotOffsetMeters=3;

    int drives;void BeginVehicleWalk(CustomerOrder o,Mechanic m,List<Vector3> route){drives++;}
    void AwardMechanicXp(Mechanic m,int xp){m.Xp+=xp;}
    void RecordCustomerInvoice(CustomerOrder o){invoiceWrites++;}void AppendRemoteInvoice(RemoteJob o,Mechanic m){invoiceWrites++;}
    bool failRelease;void ReleaseCustomerForPickup(CustomerOrder o){if(failRelease)throw new IOException("release failure");o.Stage=CustomerStage.ReturningToVehicle;}
void StartReturnVehicleWalk(CustomerOrder o){}
    /*<source:CancelCustomerOrder>*/
    void DismissWorker(Mechanic m){if(m.Worker!=null)m.Worker.alive=false;m.Worker=null;}
    void RestoreCurrentOrder(){_customerOrder=_activeCustomerOrders.FirstOrDefault();}
    static Harness Fresh(){var h=new Harness();h.InitialiseRepairPricing();return h;}
    Mechanic Worker(int level=1,int specialty=0,int bonus=0){var m=new Mechanic{Xp=(level-1)*200,Specialty=specialty,WageBonus=bonus,Name="Test",Worker=new Ped(),ArrivalPhase=PhaseAtBreak};_mechanics.Add(m);return m;}
    CustomerOrder Customer(int kind,int units=1){var o=new CustomerOrder{Id=++_lastCustomerOrderId,ServiceKind=kind,PartsNeeded=units,Price=ServiceQuoteBaseline(kind),RepairSeconds=_jobSeconds*(kind==1?1.25:1),Customer=new Ped(),Vehicle=new Vehicle(),Stage=CustomerStage.WaitingForMechanic};_activeCustomerOrders.Add(o);return o;}
    static int passed;static void Check(bool b,string why){if(!b)throw new Exception(why);}
    static void Test(string name,Action action){foreach(var p in Directory.GetFiles(DataDirectory))File.Delete(p);action();passed++;Console.WriteLine("PASS reload "+name);}
    bool _shiftReportReady,_warnedMissingEntrance,_warnedMissingBreakArea;
    long _shiftSalesBaseline,_shiftCostsBaseline;int _arrivalSpacingSeconds=1;double _visualRefreshClock;
    long AccruedBusinessCosts()=>0;
    void CleanupVisualMechanics(){foreach(var m in _mechanics)m.Worker=null;}
    void CancelAllCustomerOrders(string why){foreach(var o in _activeCustomerOrders)RememberCancelledRepair(o);_activeCustomerOrders.Clear();}
    static Harness ReadForTest(ScriptSettings supplied=null){var h=Fresh();var d=supplied??ScriptSettings.Load(h.StatePath);h._cash=d.GetValue("Business","Cash",0L);h._revenue=d.GetValue("Business","Revenue",0L);h._parts=d.GetValue("Business","Parts",10);h._completed=d.GetValue("Business","Completed",0);h._customerCompleted=d.GetValue("Business","CustomerCompleted",0);h._customerRevenue=d.GetValue("Business","CustomerRevenue",0L);h._remoteSales=d.GetValue("Business","RemoteRevenue",0L);h._remoteJobsCompleted=d.GetValue("Business","RemoteCompleted",0L);h._remotePartsConsumed=d.GetValue("Business","RemoteParts",0L);h._lastCustomerOrderId=d.GetValue("Business","LastId",0);h._partsSalesRevenue=d.GetValue("Business","PartsSalesRevenue",0L);h._partsSalesUnitsSold=d.GetValue("Business","PartsSalesUnitsSold",0L);h._managerTier=d.GetValue("Business","ManagerTier",-1);for(int i=0;i<3;i++)h._specialStock[i]=d.GetValue("Stock","Stock"+i,10);for(int i=0;i<d.GetValue("Business","Mechanics",0);i++){var m=h.Worker();m.Xp=d.GetValue("Worker"+i,"XP",0);m.JobsCompleted=d.GetValue("Worker"+i,"Jobs",0);}h.LoadRepairPromises(d);h._clockedIn=false;h._bootGameplayActivated=false;return h;}
    void Activate(){_clockedIn=true;_bootGameplayActivated=true;}
    RepairQuote Background(Mechanic m,double worked=0){RepairQuote q;Check(TryCreateRepairQuote(-1,1,m,120,500,0,out q),"background quote");q.OrderId=++_lastCustomerOrderId;m.Pricing=q;m.HasJob=true;m.WorkSeconds=worked;return q;}
    RemoteJob Remote(Mechanic m,int slot,double remaining=60){RepairQuote q;Check(TryCreateRepairQuote(2,1,m,120,500,0,out q),"remote quote");q.OrderId=++_lastCustomerOrderId;var j=new RemoteJob{MechanicSlot=slot,OrderId=q.OrderId,ServiceKind=2,PartsNeeded=1,Price=q.Total,Pricing=q,RemainingGameSeconds=remaining};m.HasJob=true;_remoteJobs.Add(j);return j;}
    CustomerOrder RecoverPhysical(){var p=_pendingRepairPromises.First(x=>x.Route==1);var o=new CustomerOrder{Customer=new Ped(),Vehicle=new Vehicle(),Stage=CustomerStage.WaitingForMechanic};ApplySavedCustomerPromise(o,p);_activeCustomerOrders.Add(o);_pendingRepairPromises.Remove(p);return o;}
    static void SameQuote(RepairQuote a,RepairQuote b){Check(a.OrderId==b.OrderId&&a.ServiceKind==b.ServiceKind&&a.DedicatedUnits==b.DedicatedUnits&&a.UniversalUnits==b.UniversalUnits&&a.Baseline==b.Baseline&&a.Premium==b.Premium&&a.PartsEstimate==b.PartsEstimate&&a.LaborEstimate==b.LaborEstimate&&a.Total==b.Total&&a.BillableSeconds==b.BillableSeconds&&a.LegacyPromise==b.LegacyPromise,"quote roundtrip");}
    static Harness BayFixture(){var h=Fresh();h._vehicleTransfersEnabled=false;var m=h.Worker();h._bays[0].Position=new Vector3(13,0,0);h._vehicleParkingBays[0].IsSet=true;h._vehicleParkingBays[0].Position=new Vector3(10,0,0);h._vehicleParkingBays[0].Heading=72;Game.Player.Character.Position=new Vector3(-100,0,0);return h;}
    static CustomerOrder Assigned(Harness h){var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.MechanicSlot==0&&o.Stage==CustomerStage.TeleportWaitingBay,"assignment");return o;}
    static void RunRepair(Harness h,CustomerOrder o,double seconds){Game.GameTime+=(int)(seconds*1000);h.ProcessOneCustomerRepair(o,seconds);}
    static void Advance(Harness h,CustomerOrder o){Game.GameTime+=5000;h._customerOrder=o;h.ProcessTeleportOrder(o);}
    static void ResetNative(){Game.IsPaused=false;Game.GameTime=100;GTA.Native.Function.playerInside=false;GTA.Native.Function.driverFree=true;GTA.Native.Function.obstacle=false;GTA.Native.Function.passengers=0;GTA.Native.Function.calls=0;GTA.Native.Function.throwQuery=false;GTA.Native.Function.animations=0;}
    static void BayTest(string name,Action action){ResetNative();Test(name,action);}
    static Harness QueueFixture(int staff=3){var h=BayFixture();for(int i=1;i<staff;i++)h.Worker();for(int i=0;i<staff;i++){h._bays[i].Position=new Vector3(10+20*i,0,0);h._vehicleParkingBays[i].IsSet=true;h._vehicleParkingBays[i].Position=new Vector3(10+20*i,0,0);}return h;}
    static Harness RecoveryFixture(){ResetNative();GTA.Native.Function.workerInside=false;GTA.Native.Function.workerAny=false;GTA.Native.Function.parks=GTA.Native.Function.exits=0;var h=QueueFixture();h._customMechanicJobRoutes=true;h._vehicleTransfersEnabled=true;h._mechanics[0].Worker.Handle=77;return h;}
    static CustomerOrder PaidCar(Harness h){var o=h.Customer(0);h.AssignCustomerMechanic(o);o.Vehicle.backing=o.ResolvedParkingBay.Position;o.Stage=CustomerStage.Repairing;h._customerOrder=o;h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,o,120);o.Stage=CustomerStage.MechanicParkingAtDropoff;Check(o.Paid&&h.invoiceWrites==1,"paid setup");return o;}
    static void RecoveryStep(Harness h,CustomerOrder o){Game.GameTime+=5000;h.ProcessPaidReturnRecovery(o);}
    static void RecoveryTests(){
 BayTest("mask eight selecting unset fourth point is explicit and unchanged",()=>{var h=RecoveryFixture();h._mechanics[0].JobOutboundMask=8;h._serviceRoute[0].IsSet=true;h._serviceRoute[0].Position=new Vector3(2,3,4);var path=h.BuildRoute(0,PhaseToBay);Check(h.EffectiveWorkRouteMask(8)==0&&path.Count==1&&h._mechanics[0].JobOutboundMask==8&&h._serviceRoute[0].IsSet,"mask altered");Check(h.WorkRouteStatus(0,false).Contains("unset=8")&&h.notices.Any(x=>x.Contains("WARNING")),"warning missing");int notes=h.notices.Count;for(int i=0;i<100;i++)h.BuildRoute(0,PhaseToBay);Check(h.notices.Count==notes,"warning spam");});
 BayTest("valid shared mask selects only saved points in direction order",()=>{var h=RecoveryFixture();h._mechanics[0].JobOutboundMask=h._mechanics[0].JobReturnMask=5;h._serviceRoute[0].IsSet=h._serviceRoute[2].IsSet=true;h._serviceRoute[0].Position=new Vector3(1,0,0);h._serviceRoute[2].Position=new Vector3(3,0,0);var a=h.BuildRoute(0,PhaseToBay);var b=h.BuildRoute(0,PhaseReturningToBreak);Check(a[0].X==1&&a[1].X==3&&b[0].X==3&&b[1].X==1&&h.notices.Count==0,"route order");});
 BayTest("pickup uses reversed arrival points independently from work mask",()=>{var h=RecoveryFixture();h._pickupUsesArrivalExit=true;h._arrivalRoute=new[]{new WorkBay{IsSet=true,Position=new Vector3(1,0,0)},new WorkBay{IsSet=true,Position=new Vector3(2,0,0)}};h._mechanics[0].JobOutboundMask=8;var p=h.BuildMechanicPickupExitRoute(new Vector3(99,0,0));Check(p.Count==3&&p[0].X==2&&p[1].X==1&&p[2].X==99,"pickup mask confusion");});
 BayTest("parking retries once then holds paid car instead of cancel",()=>{var h=RecoveryFixture();var o=PaidCar(h);o.SecondsInStage=50;GTA.Native.Function.workerInside=true;GTA.Native.Function.driverFree=false;h.UpdateVehicleParking(o,false);Check(o.ParkingRetries==1&&GTA.Native.Function.parks==1,"retry");o.SecondsInStage=50;h.UpdateVehicleParking(o,false);Check(o.Stage==CustomerStage.PaidReturnRecovery&&o.Paid&&h._activeCustomerOrders.Contains(o)&&h.invoiceWrites==1,"paid canceled");});
 BayTest("unpaid parking failure never pays and preserves accepted quote",()=>{var h=RecoveryFixture();var o=h.Customer(0);h.AssignCustomerMechanic(o);h._customerOrder=o;o.ParkingRetries=1;o.SecondsInStage=50;o.Vehicle.backing=new Vector3(99,0,0);h.UpdateVehicleParking(o,true);Check(!o.Paid&&h._cash==0&&h._pendingRepairPromises.Count==1&&!h._activeCustomerOrders.Contains(o),"unpaid premature payment");});
 BayTest("paid recovery asks driver exit before any car position write",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");GTA.Native.Function.workerInside=true;GTA.Native.Function.driverFree=false;RecoveryStep(h,o);Check(GTA.Native.Function.exits==1&&o.Vehicle.moves==0&&o.Stage==CustomerStage.PaidReturnRecovery,"occupied move");for(int i=0;i<20;i++)RecoveryStep(h,o);Check(GTA.Native.Function.exits==2&&o.Vehicle.moves==0,"unbounded exits");});
 BayTest("after confirmed empty safe recovery returns once and frees worker",()=>{var h=RecoveryFixture();var o=PaidCar(h);long cash=h._cash;h.FailVehicleTransfer(o,false,"test");RecoveryStep(h,o);Check(o.TeleportedToPickup&&o.Vehicle.moves==1&&o.RecoveryWorkerReleased&&!h._mechanics[0].HasJob&&o.Stage==CustomerStage.ReturningToVehicle,"recovery");Check(h._cash==cash&&h.invoiceWrites==1&&h._customerCompleted==1,"duplicate settlement");h.ProcessOneCustomerRepair(o,999);Check(h._cash==cash&&h.invoiceWrites==1,"repaid");});
 foreach(int blocker in new[]{0,1,2,3,4}){int flag=blocker;BayTest("unsafe paid recovery holds without moving "+flag,()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");if(flag==0)GTA.Native.Function.playerInside=true;if(flag==1)GTA.Native.Function.passengers=1;if(flag==2)GTA.Native.Function.obstacle=true;if(flag==3)h.pickupBlocked=true;if(flag==4)o.Vehicle.Speed=4;RecoveryStep(h,o);Check(o.Vehicle.moves==0&&o.Paid&&h._activeCustomerOrders.Contains(o),"unsafe move");});}
 BayTest("pause performs no recovery natives or move",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");int n=GTA.Native.Function.calls;Game.IsPaused=true;RecoveryStep(h,o);Check(GTA.Native.Function.calls==n&&o.Vehicle.moves==0,"pause");});
 BayTest("paid recovery prevents intake and preserves worker transport ownership",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");Check(NeedsDropoffSoon(o)&&!h.CanStartNextIntake()&&h.MechanicOnVehicleTask(0),"priority/worker");});
 BayTest("manual override resets bounded exits but never an unpaid order",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");o.RecoveryExitAttempts=2;h.RetryPaidCustomerReturns();Check(o.RecoveryExitAttempts==0&&o.Paid,"manual");var a=h.Customer(0);a.Stage=CustomerStage.MechanicWaitingToReturn;h.RetryPaidCustomerReturns();Check(!a.Paid&&!a.PaidReturnRecovery,"unpaid forced");});
 BayTest("manual override ignores completed departing customers",()=>{var h=RecoveryFixture();var o=PaidCar(h);o.Stage=CustomerStage.ReturningToVehicle;h.RetryPaidCustomerReturns();Check(!o.PaidReturnRecovery&&o.Stage==CustomerStage.ReturningToVehicle,"departure hijacked");});
 BayTest("staged parking one retry retains entity and FIFO until bounded failure",()=>{var h=RecoveryFixture();var o=new CustomerOrder{Customer=new Ped(),Vehicle=new Vehicle(),Stage=CustomerStage.ParkingAtQueueSpace,QueueDestination=new WorkBay{IsSet=true,Position=new Vector3(100,0,0)},SecondsInStage=40};h._stagedCustomerOrder=o;h.UpdateStagedQueueTravel(ref h._stagedCustomerOrder);Check(Object.ReferenceEquals(o,h._stagedCustomerOrder)&&o.QueueParkingRetries==1,"stage recreated");o.SecondsInStage=40;h.UpdateStagedQueueTravel(ref h._stagedCustomerOrder);Check(h._stagedCustomerOrder==null&&h._waitingCustomerRequests==2,"pending lost");});
 BayTest("recovery clearance later clears without another payment",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");h.pickupBlocked=true;RecoveryStep(h,o);long cash=h._cash;h.pickupBlocked=false;RecoveryStep(h,o);Check(o.TeleportedToPickup&&h._cash==cash&&h.invoiceWrites==1,"blocked resume");});
    }
    /*<source:ResolveControllerEnabled>*/
    /*<source:MechanicServiceStatus>*/
    /*<source:RefreshEmployeeStatus>*/
    /*<source:ExecuteEmployeeStatusSelection>*/
    /*<source:ShowEmployeeStatus>*/
    /*<source:SpecialtyName>*/
    readonly string[] _employeeStatusRows=new string[5];
    class Reception {internal Ped Worker;internal bool AtDesk;}
    Reception _receptionist=new Reception();Ped _managerWorker;bool _managerAtPost;string _managerName="manager";
    static void StatusTests(){
 BayTest("full staff summary cannot reuse stale receptionist desk flag",()=>{var h=RecoveryFixture();h._receptionistHired=true;h._receptionist.AtDesk=true;h._receptionist.Worker=null;h.ShowEmployeeStatus();Check(h.notices.Contains("Reception NOT STREAMED")&&!h.notices.Contains("Reception AT DESK")&&h.saves==0,"stale summary");});
 foreach(bool config in new[]{false,true})foreach(bool legacy in new[]{false,true}){bool current=config,old=legacy;BayTest("Settings.ini controller precedence "+current+"/"+old,()=>{var settings=new ScriptSettings();settings.SetValue("Gamepad","Enabled",current);var prior=new ScriptSettings();prior.SetValue("Gamepad","Enabled",old);Check(ResolveControllerEnabled(settings,prior,!current)==current,"legacy overrode settings");});}
 BayTest("missing setting retains legacy controller enable",()=>{var legacy=new ScriptSettings();legacy.SetValue("Gamepad","Enabled",true);Check(ResolveControllerEnabled(new ScriptSettings(),legacy,false),"silent disable");});
 BayTest("missing all settings retains supplied default",()=>{Check(ResolveControllerEnabled(new ScriptSettings(),new ScriptSettings(),true),"default changed");});
 BayTest("stale working phase with missing worker is not working",()=>{var h=RecoveryFixture();var m=h._mechanics[0];m.ArrivalPhase=PhaseWorking;m.HasJob=true;m.Worker=null;Check(h.MechanicServiceStatus(0).Contains("UNAVAILABLE"),"stale phase");});
 BayTest("transport stage is shown before repair",()=>{var h=RecoveryFixture();var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(h.MechanicServiceStatus(0).Contains("MechanicWalkingToPickup"),"incorrect work claim");});
 BayTest("worker phase alone cannot claim autonomous job without quote",()=>{var h=RecoveryFixture();var m=h._mechanics[0];m.HasJob=true;m.ArrivalPhase=PhaseWorking;Check(!h.MechanicServiceStatus(0).Contains("AUTONOMOUS"),"HasJob false work claim");});
 BayTest("car outside bay is explained even with worker at workstation",()=>{var h=RecoveryFixture();var o=h.Customer(0);h.AssignCustomerMechanic(o);o.Stage=CustomerStage.Repairing;h._mechanics[0].ArrivalPhase=PhaseWorking;o.Vehicle.backing=new Vector3(99,0,0);Check(h.MechanicServiceStatus(0).Contains("WAIT CAR"),"missing placement diagnosis");});
 BayTest("paid recovery is status not unpaid work",()=>{var h=RecoveryFixture();var o=PaidCar(h);h.FailVehicleTransfer(o,false,"test");Check(h.MechanicServiceStatus(0).Contains("PAID")&&h.MechanicServiceStatus(0).Contains("PaidReturnRecovery"),"paid state");});
 BayTest("employee snapshot refresh is explicit and browsing never saves",()=>{var h=RecoveryFixture();h.RefreshEmployeeStatus();string original=h._employeeStatusRows[0];h._mechanics[0].Worker=null;Check(h._employeeStatusRows[0]==original,"unexpected automatic scanning");h.RefreshEmployeeStatus();Check(h._employeeStatusRows[0].Contains("UNAVAILABLE")&&h.saves==0,"refresh");});
 BayTest("reception readiness requires live worker",()=>{var h=RecoveryFixture();h._receptionistHired=true;h._receptionist.AtDesk=true;h.RefreshEmployeeStatus();Check(h._employeeStatusRows[3]=="NOT STREAMED","phantom receptionist");});
 BayTest("remote manager and paused mechanics are explicit",()=>{var h=RecoveryFixture();h._managerTier=1;h._remoteMode=true;h.RefreshEmployeeStatus();Check(h._employeeStatusRows[4]=="REMOTE SIMULATION","manager mode");Game.IsPaused=true;Check(h.MechanicServiceStatus(0)=="PAUSED","paused");});
    }
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh335-"+Guid.NewGuid());Directory.CreateDirectory(DataDirectory);try{
StatusTests();Console.WriteLine($"status336 {passed}/{passed} PASS; production intake/staging/spawn with deterministic entities and temporary-file quote adapters; GTA NOT RUN");
}finally{Directory.Delete(DataDirectory,true);}}
}
