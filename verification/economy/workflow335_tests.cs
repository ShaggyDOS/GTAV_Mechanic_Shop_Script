using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GTA.Native {
 enum Hash { IS_PED_IN_ANY_VEHICLE,TASK_ENTER_VEHICLE,TASK_VEHICLE_DRIVE_WANDER,TASK_LEAVE_VEHICLE,IS_SPHERE_VISIBLE,SET_PED_INTO_VEHICLE,TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,TASK_VEHICLE_PARK,IS_PED_IN_VEHICLE,IS_VEHICLE_SEAT_FREE,GET_VEHICLE_NUMBER_OF_PASSENGERS,IS_POSITION_OCCUPIED,CLEAR_PED_TASKS,TASK_START_SCENARIO_IN_PLACE,TASK_STAND_STILL }
 static class Function {internal static bool playerInside,driverFree=true,obstacle,throwQuery;internal static int passengers,calls,animations;internal static T Call<T>(Hash h,params object[] args){calls++;if(throwQuery)throw new IOException("query failure");object v=h==Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS?(object)passengers:h==Hash.IS_PED_IN_VEHICLE?playerInside:h==Hash.IS_VEHICLE_SEAT_FREE?driverFree:obstacle;return (T)v;}internal static void Call(Hash h,params object[] args){calls++;if(h==Hash.TASK_START_SCENARIO_IN_PLACE)animations++;}}
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
    // Issue37 review lifecycle adapters; existing input/economy assertions retained.
    void DropPlacementConfirmation(){}
    void CancelPlacementConfirmation(){}
    bool _placementRecoveryRequired;

 // Direction-free handoff native adapter; focused parking37 tests exercise actual helpers.
 void BeginDirectionFreeHandoff(CustomerOrder o,Ped driver,bool returning){o.Stage=returning?CustomerStage.MechanicParkingAtDropoff:CustomerStage.ParkingAtDropoff;o.SecondsInStage=0;}
 void UpdateDirectionFreeHandoff(CustomerOrder o,bool returning){}
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
 Ped AssignedWorker(CustomerOrder o)=>o.MechanicSlot>=0&&o.MechanicSlot<_mechanics.Count?_mechanics[o.MechanicSlot].Worker:null;
 WorkBay[] _bayApproaches={new WorkBay(),new WorkBay(),new WorkBay()};
 bool _customEmployeeWalkingPaths;bool _fineParkingEnabled=true;int _parkingTimeoutSeconds=30,_driveArrivalTimeout=90;float _parkingRadius=2;
 WorkBay _customerApproach=new WorkBay(),_customerQueue=new WorkBay();
 void BeginCustomerExit(CustomerOrder o){o.Stage=CustomerStage.ExitingVehicle;}void AdvanceCustomerWalkingNode(CustomerOrder o){}
 bool CustomerArrived(CustomerOrder o,Vector3 target)=>false;
 void WalkCustomer(CustomerOrder o,Vector3 target,CustomerStage stage){o.Stage=stage;}
 void HandleCustomerStall(CustomerOrder o,Vector3 target){}bool ProgressVehicleWalk(CustomerOrder o)=>false;
 bool MechanicInCustomerCar(CustomerOrder o)=>false;bool MechanicOutOfCar(CustomerOrder o)=>false;
 void BeginEnterVehicle(CustomerOrder o,bool returning){}void StartValetDeparture(CustomerOrder o,bool outbound){}
 void BeginVehicleDrive(CustomerOrder o,bool outbound){}void UpdateVehicleApproach(CustomerOrder o,bool inbound){}
 void UpdateVehicleParking(CustomerOrder o,bool inbound){}
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
 void ShowStatusNotice(string title,string a,string b,string c,string d){}

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
    bool MechanicOnVehicleTask(int slot)=>false;
    int CountSet(WorkBay[] route)=>0;WorkBay[] _arrivalRoute=new WorkBay[0];bool _pickupUsesArrivalExit;
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
    /*<source:HandoffProgress>*/
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
    List<Vector3> BuildMechanicPickupExitRoute(Vector3 p)=>new List<Vector3>();
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
    static void WorkflowTests(){
 BayTest("second intake while first repairing with separate staffed bay",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);Check(h.CanStartNextIntake(),"parallel intake");h.pickupBlocked=true;Check(!h.CanStartNextIntake(),"dropoff");});
 for(int staff=1;staff<=3;staff++){int n=staff;BayTest("configured staffed capacity "+n,()=>{var h=QueueFixture(n);for(int i=0;i<n;i++){Check(h.CanStartNextIntake(),"capacity "+i);var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.MechanicSlot==i,"independent slot");Advance(h,o);}Check(!h.CanStartNextIntake(),"bounded active capacity");});}
 BayTest("free worker without configured work/car bay cannot admit next car",()=>{var h=QueueFixture(2);var a=Assigned(h);Advance(h,a);h._bays[1].IsSet=false;Check(!h.CanStartNextIntake(),"no work bay");h._bays[1].IsSet=true;h._vehicleParkingBays[1].IsSet=false;Check(!h.CanStartNextIntake(),"no custom car bay");h._automaticLotMode=true;Check(h.CanStartNextIntake(),"auto bay");});
 BayTest("same saved physical target cannot admit another car",()=>{var h=QueueFixture(2);var a=Assigned(h);Advance(h,a);h._vehicleParkingBays[1].Position=a.ResolvedParkingBay.Position;Check(!h.CanStartNextIntake(),"reserved geometry");});
 BayTest("intake and paid pickup priorities hold dropoff",()=>{var h=QueueFixture();var a=Assigned(h);Check(!h.CanStartNextIntake(),"incoming TP holds intake");Advance(h,a);a.Paid=true;a.Stage=CustomerStage.TeleportWaitingPickup;Check(!h.CanStartNextIntake(),"paid pickup wins");});
 BayTest("staged FIFO keeps original entities and counts one reservation once",()=>{var h=QueueFixture();h._stagedCustomerOrder=new CustomerOrder{Id=10,Vehicle=new Vehicle(),Customer=new Ped(),Stage=CustomerStage.WaitingAtRoadside,PartsNeeded=1};h._stagedThirdCustomerOrder=new CustomerOrder{Id=11,Vehicle=new Vehicle(),Customer=new Ped(),Stage=CustomerStage.WaitingAtRoadside,PartsNeeded=1};var first=h._stagedCustomerOrder;var second=h._stagedThirdCustomerOrder;h.ActivateRoadsideWaitingCustomer();Check(h._activeCustomerOrders.Contains(first)&&Object.ReferenceEquals(h._stagedCustomerOrder,second)&&h._stagedThirdCustomerOrder==null&&h._waitingCustomerRequests==1,"FIFO transition");h.ActivateRoadsideWaitingCustomer();Check(h._activeCustomerOrders.Count==1&&h._waitingCustomerRequests==1,"not double dispatch");});
 BayTest("lost older slot cannot let a replacement jump ahead of surviving third",()=>{World.vehicles=0;var h=QueueFixture();h.Customer(0).Stage=CustomerStage.Repairing;h._stagedCustomerOrder=new CustomerOrder{Vehicle=new Vehicle{alive=false},Customer=new Ped()};var old=new CustomerOrder{Id=5,Vehicle=new Vehicle(),Customer=new Ped(),Stage=CustomerStage.WaitingAtRoadside,PartsNeeded=1};h._stagedThirdCustomerOrder=old;h.ValidateStagedCustomer(ref h._stagedCustomerOrder);h.SpawnRoadsideWaitingCustomer(DateTime.UtcNow,false);Check(h._stagedCustomerOrder==null&&World.vehicles==0&&Object.ReferenceEquals(h._stagedThirdCustomerOrder,old)&&h._waitingCustomerRequests==2,"replacement overtook old car");});
 BayTest("spawn failure cooldown prevents pending-request native churn",()=>{World.failVehicle=true;World.vehicles=0;var h=QueueFixture();for(int i=0;i<100;i++)h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==1&&h._waitingCustomerRequests==2,"retry bound");Game.GameTime+=29999;h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==1,"too soon");Game.GameTime++;World.failVehicle=false;h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==2&&h._activeCustomerOrders.Count==1,"recovered");});
 BayTest("staged creation failure retains request and shares bounded retry",()=>{World.failVehicle=true;World.vehicles=0;var h=QueueFixture();h.Customer(0).Stage=CustomerStage.Repairing;for(int i=0;i<100;i++)h.SpawnRoadsideWaitingCustomer(DateTime.UtcNow,false);Check(World.vehicles==1&&h._stagedCustomerOrder==null&&h._waitingCustomerRequests==2,"stage retry");World.failVehicle=false;Game.GameTime+=30000;h.SpawnRoadsideWaitingCustomer(DateTime.UtcNow,false);Check(h._stagedCustomerOrder!=null,"stage recovery");});
 BayTest("ped creation failure cleans owned car and preserves request",()=>{World.failPed=true;World.vehicles=0;var h=QueueFixture();h.SpawnCustomerOrder(DateTime.UtcNow);Check(h._activeCustomerOrders.Count==0&&h._waitingCustomerRequests==2,"no phantom order");h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==1,"ped retry bound");World.failPed=false;});
 BayTest("pause, boot and reload cannot trigger spawn or intake",()=>{World.vehicles=0;var h=QueueFixture();Game.IsPaused=true;h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==0&&!h.CanStartNextIntake(),"paused");Game.IsPaused=false;h._bootGameplayActivated=false;h.SpawnCustomerOrder(DateTime.UtcNow);Check(World.vehicles==0&&!h.CanStartNextIntake(),"boot");});
 BayTest("destroyed staged car retains request without dispatch",()=>{var h=QueueFixture();h._stagedCustomerOrder=new CustomerOrder{Customer=new Ped(),Vehicle=new Vehicle{IsDead=true}};h.ValidateStagedCustomer(ref h._stagedCustomerOrder);Check(h._stagedCustomerOrder==null&&h._waitingCustomerRequests==2,"destroyed stage");});
 BayTest("failed pending quote save retains fixed promise and no vehicle transfer",()=>{var h=QueueFixture();h.saveSuccess=false;var a=h.Customer(0);h.AssignCustomerMechanic(a);Check(a.MechanicSlot==-1&&!h._mechanics[0].HasJob&&a.Vehicle.moves==0,"quote failure");});
 BayTest("reserved parts block additional customer intake",()=>{var h=QueueFixture();h._parts=0;h._specialStock[0]=1;h._specialStock[1]=h._specialStock[2]=0;var a=Assigned(h);Advance(h,a);h._stagedCustomerOrder=new CustomerOrder{ServiceKind=0,PartsNeeded=1};Check(!h.CanStartNextIntake(),"part reservation");});
 BayTest("ON overlapping saved bays cannot double assign",()=>{var h=QueueFixture(2);h._vehicleTransfersEnabled=true;var a=h.Customer(0);h.AssignCustomerMechanic(a);h._vehicleParkingBays[1].Position=a.ResolvedParkingBay.Position;var b=h.Customer(0);h.AssignCustomerMechanic(b);Check(a.MechanicSlot==0&&b.MechanicSlot==-1&&!h._mechanics[1].HasJob&&h.drives==1,"ON duplicate bay");});
 BayTest("paid OFF bay remains reserved until vehicle returns",()=>{var h=QueueFixture(2);var a=Assigned(h);Advance(h,a);h._mechanics[0].ArrivalPhase=PhaseWorking;h._customerOrder=a;RunRepair(h,a,120);Check(a.Paid&&!h.CustomerBayAvailable(0,null),"paid bay released too soon");a.Stage=CustomerStage.ReturningToVehicle;Check(h.CustomerBayAvailable(0,null),"bay not released after return");});
 for(int mode=0;mode<2;mode++)for(int fault=0;fault<3;fault++){int m=mode,f=fault;BayTest("no ghost repair settlement mode/fault "+m+"/"+f,()=>{var h=QueueFixture();h._vehicleTransfersEnabled=m==1;var a=h.Customer(0);h.AssignCustomerMechanic(a);if(m==0)Advance(h,a);a.Stage=CustomerStage.Repairing;h._customerOrder=a;h._mechanics[0].ArrivalPhase=PhaseWorking;h._mechanics[0].WorkSeconds=a.RepairSeconds;if(f==0)a.Vehicle=null;if(f==1)a.Vehicle.alive=false;if(f==2)a.Vehicle.IsDead=true;RunRepair(h,a,1);Check(!a.Paid&&h._cash==0&&h.invoiceWrites==0&&!h._activeCustomerOrders.Contains(a)&&!h._mechanics[0].HasJob,"ghost charge");Check(h._pendingRepairPromises.Count==1,"accepted quote lost");});}
 BayTest("missing customer prevents completion credit",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);h._customerOrder=a;h._mechanics[0].ArrivalPhase=PhaseWorking;a.Customer.alive=false;RunRepair(h,a,120);Check(!a.Paid&&h._cash==0&&h._pendingRepairPromises.Count==1,"customer loss");});
 BayTest("unpaid cancel releases reservation and retains exact quote",()=>{var h=QueueFixture();var a=Assigned(h);var q=a.Pricing;h._customerOrder=a;h.CancelCustomerOrder("test cancellation",false);Check(h._activeCustomerOrders.Count==0&&!h._mechanics[0].HasJob&&h._cash==0&&h._specialStock[0]==10,"cancel ownership");SameQuote(q,h._pendingRepairPromises.Single().Quote);});
 BayTest("paid cancellation never restores unpaid quote or pays twice",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);h._customerOrder=a;h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,a,120);long cash=h._cash;h.CancelCustomerOrder("after payment",false);Check(a.Paid&&h._cash==cash&&h._pendingRepairPromises.Count==0&&h.invoiceWrites==1,"paid cancel");RunRepair(h,a,120);Check(h._cash==cash&&h.invoiceWrites==1,"repeat paid");});
 BayTest("failed settlement retries atomically with one invoice",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);h._customerOrder=a;h._mechanics[0].ArrivalPhase=PhaseWorking;h.saveSuccess=false;RunRepair(h,a,120);Check(!a.Paid&&h._cash==0&&h._specialStock[0]==10&&h.invoiceWrites==0,"failed settle");h.saveSuccess=true;RunRepair(h,a,6);RunRepair(h,a,6);Check(a.Paid&&h._cash==a.Price&&h._specialStock[0]==9&&h.invoiceWrites==1,"settle once");});
 BayTest("staged driving timeout cancels owned entities but keeps request",()=>{var h=QueueFixture();h._stagedCustomerOrder=new CustomerOrder{Customer=new Ped(),Vehicle=new Vehicle(),Stage=CustomerStage.DrivingToQueueSpace,QueueDestination=new WorkBay{IsSet=true,Position=new Vector3(200,0,0)}};for(int i=0;i<86;i++)h.UpdateStagedQueueTravel(ref h._stagedCustomerOrder);Check(h._stagedCustomerOrder==null&&h._waitingCustomerRequests==2,"staged timeout");});
 BayTest("reload during accepted service parks quote not stale transfer",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);h._mechanics[0].WorkSeconds=20;h.Save();var restored=ReadForTest();Check(restored._activeCustomerOrders.Count==0&&restored._pendingRepairPromises.Count==1&&!restored._bootGameplayActivated,"reload boundary");SameQuote(a.Pricing,restored._pendingRepairPromises[0].Quote);});
 BayTest("pause and closed shop cannot progress repairs",()=>{var h=QueueFixture();var a=Assigned(h);Advance(h,a);h._mechanics[0].ArrivalPhase=PhaseWorking;Game.IsPaused=true;h.ProcessEconomy(1000);Check(!a.Paid&&h._cash==0&&h._mechanics[0].WorkSeconds==0,"pause progress");Game.IsPaused=false;h._clockedIn=false;h.ProcessEconomy(1000);Check(!a.Paid&&h._cash==0,"closed progress");});
 foreach(var stage in new[]{CustomerStage.MechanicEnteringVehicle,CustomerStage.MechanicEnteringForReturn,CustomerStage.MechanicExitingAtBay,CustomerStage.MechanicExitingAtDropoff}){var phase=stage;BayTest("physical transition timeout cancels once without teleport "+phase,()=>{var h=QueueFixture();h._vehicleTransfersEnabled=true;var a=h.Customer(0);h.AssignCustomerMechanic(a);h._customerOrder=a;a.Stage=phase;a.SecondsInStage=23;h.UpdateOneCustomerOrder(a);Check(!h._activeCustomerOrders.Contains(a)&&a.Vehicle.moves==0&&h._cash==0&&h._pendingRepairPromises.Count==1,"bounded transition");});}
 BayTest("paid physical pickup wait cannot rerun payment or teleport",()=>{var h=QueueFixture();h._vehicleTransfersEnabled=true;var a=h.Customer(0);h.AssignCustomerMechanic(a);h._customerOrder=a;a.Stage=CustomerStage.Repairing;a.Vehicle.backing=a.ResolvedParkingBay.Position;h._mechanics[0].ArrivalPhase=PhaseWorking;h.pickupBlocked=true;RunRepair(h,a,120);Check(a.Paid&&a.Stage==CustomerStage.MechanicWaitingToReturn,"hold paid return");long cash=h._cash;for(int i=0;i<10;i++){h.UpdateOneCustomerOrder(a);RunRepair(h,a,10);}Check(h._cash==cash&&h.invoiceWrites==1&&a.Vehicle.moves==0,"repeat charge/move");});
 BayTest("on-demand status names assigned worker and bay without saving",()=>{var h=QueueFixture();var a=Assigned(h);h._customerOrder=a;int saves=h.saves,natives=GTA.Native.Function.calls;h.ShowCustomerOrderStatus();Check(h.notices.Any(x=>x.Contains("Assigned: Test | Bay 1"))&&h.saves==saves&&GTA.Native.Function.calls==natives,"status side effects");});
 BayTest("unassigned status explains mechanic and bay wait",()=>{var h=QueueFixture();h._customerOrder=h.Customer(0);h.ShowCustomerOrderStatus();Check(h.notices.Any(x=>x.Contains("CONFIGURED CLEAR BAY"))&&h.saves==0,"wait explanation");});
 World.failVehicle=World.failPed=false;
    }
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh335-"+Guid.NewGuid());Directory.CreateDirectory(DataDirectory);try{
WorkflowTests();Console.WriteLine($"workflow335 {passed}/{passed} PASS; production intake/staging/spawn with deterministic entities and temporary-file quote adapters; GTA NOT RUN");
}finally{Directory.Delete(DataDirectory,true);}}
}
