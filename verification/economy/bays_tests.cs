using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GTA.Native {
 enum Hash { IS_PED_IN_VEHICLE,IS_VEHICLE_SEAT_FREE,GET_VEHICLE_NUMBER_OF_PASSENGERS,IS_POSITION_OCCUPIED,CLEAR_PED_TASKS,TASK_START_SCENARIO_IN_PLACE,TASK_STAND_STILL }
 static class Function {internal static bool playerInside,driverFree=true,obstacle,throwQuery;internal static int passengers,calls,animations;internal static T Call<T>(Hash h,params object[] args){calls++;if(throwQuery)throw new IOException("query failure");object v=h==Hash.GET_VEHICLE_NUMBER_OF_PASSENGERS?(object)passengers:h==Hash.IS_PED_IN_VEHICLE?playerInside:h==Hash.IS_VEHICLE_SEAT_FREE?driverFree:obstacle;return (T)v;}internal static void Call(Hash h,params object[] args){calls++;if(h==Hash.TASK_START_SCENARIO_IN_PLACE)animations++;}}
}

struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
class Ped {internal int Handle=9;internal float Heading;internal bool IsDead;internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
class Vehicle {internal float Speed;internal int Handle=4,moves;internal bool alive=true,IsDead,failHeading;internal Vector3 backing,Velocity;internal Vector3 Position{get=>backing;set{backing=value;moves++;}}internal float heading;internal float Heading{get=>heading;set{if(failHeading)throw new IOException("heading failure");heading=value;}}internal bool Exists()=>alive;}
class Blip {}
class ScriptSettings {
 internal Dictionary<string,string> values=new Dictionary<string,string>();string file;
 internal T GetValue<T>(string section,string key,T fallback){return values.TryGetValue(section+"|"+key,out string value)?(T)Convert.ChangeType(value,typeof(T),System.Globalization.CultureInfo.InvariantCulture):fallback;}
 internal void SetValue<T>(string section,string key,T value){values[section+"|"+key]=Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture);}
 internal static ScriptSettings Load(string path){var d=new ScriptSettings{file=path};string section="";if(File.Exists(path))foreach(string line in File.ReadAllLines(path)){if(line.StartsWith("[")&&line.EndsWith("]"))section=line.Substring(1,line.Length-2);else{int eq=line.IndexOf('=');if(eq>=0)d.values[section+"|"+line.Substring(0,eq)]=line.Substring(eq+1);}}return d;}
 internal bool Save(){var lines=new List<string>();foreach(var group in values.GroupBy(k=>k.Key.Split('|')[0])){lines.Add("["+group.Key+"]");foreach(var kv in group)lines.Add(kv.Key.Split('|')[1]+"="+kv.Value);}File.WriteAllLines(file,lines);return true;}
}

static class Game {internal static bool IsPaused;internal static int GameTime=100;internal static PlayerState Player=new PlayerState();internal class PlayerState {internal Ped Character=new Ped();}}
class Harness {
 /*<source:MenuPage>*/
 /*<source:OpenServiceBayProfile>*/
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
    /*<source:CustomerBayAvailable>*/
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
    void CancelCustomerOrder(string why,bool notify){if(_customerOrder!=null)_activeCustomerOrders.Remove(_customerOrder);}
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
    static void Main(){
 DataDirectory=Path.Combine(Path.GetTempPath(),"gh-bays-"+Guid.NewGuid());Directory.CreateDirectory(DataDirectory);try{
 BayTest("OFF assigns and teleports once with exact position heading",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);Check(o.VehicleTeleport&&!o.VehicleHandoff&&o.TeleportedToBay&&o.Stage==CustomerStage.Repairing&&o.Vehicle.Position.X==10&&o.Vehicle.Heading==72&&o.Vehicle.moves==1&&h.drives==0,"move");for(int i=0;i<100;i++)h.ProcessTeleportOrder(o);Check(o.Vehicle.moves==1,"one shot");});
 BayTest("ON dispatches existing driving and never teleport mode",()=>{var h=BayFixture();h._vehicleTransfersEnabled=true;var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.VehicleHandoff&&!o.VehicleTeleport&&o.Stage==CustomerStage.MechanicWalkingToPickup&&h.drives==1&&o.Vehicle.moves==0,"driving unchanged");});
 BayTest("repair requires mechanic arrival and starts actual animation once",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);var m=h._mechanics[0];RunRepair(h,o,120);Check(!o.Paid,"before arrival");m.ArrivalPhase=PhaseWorking;m.WorkMode=-1;h.UpdateEmployeeActivity(m,0,DateTime.UtcNow);h.UpdateEmployeeActivity(m,0,DateTime.UtcNow);Check(GTA.Native.Function.animations==1,"animation");RunRepair(h,o,120);Check(o.Paid&&o.Stage==CustomerStage.TeleportWaitingPickup,"complete");});
 BayTest("paid pickup moves once keeping current heading without second payment",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,o,120);long cash=h._cash;RunRepair(h,o,120);Advance(h,o);Check(o.TeleportedToPickup&&o.Vehicle.Position.X==0&&o.Vehicle.Heading==72&&o.Stage==CustomerStage.ReturningToVehicle&&o.Vehicle.moves==2&&h._cash==cash&&h._customerCompleted==1,"pickup");h.ProcessTeleportOrder(o);Check(o.Vehicle.moves==2,"repeat pickup");});
 BayTest("occupied bay preserves waiting order and cadence",()=>{var h=BayFixture();var o=h.Customer(0);GTA.Native.Function.obstacle=true;h.AssignCustomerMechanic(o);Check(o.MechanicSlot==-1&&!h._mechanics[0].HasJob&&h._activeCustomerOrders.Contains(o),"blocked");int calls=GTA.Native.Function.calls;for(int i=0;i<100;i++)h.AssignCustomerMechanic(o);Check(GTA.Native.Function.calls==calls,"rate");GTA.Native.Function.obstacle=false;Game.GameTime+=5000;h.AssignCustomerMechanic(o);Check(o.MechanicSlot==0,"cleared");});
 BayTest("pickup obstruction keeps paid order without repeat charge",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,o,120);h.pickupBlocked=true;Advance(h,o);long cash=h._cash;Check(o.Paid&&!o.TeleportedToPickup&&o.Vehicle.Position.X==10,"blocked pickup");h.pickupBlocked=false;Advance(h,o);Check(o.TeleportedToPickup&&h._cash==cash,"resume");});
 foreach(int flag in new[]{0,1,2})BayTest("occupied order car blocks movement "+flag,()=>{var h=BayFixture();var o=Assigned(h);if(flag==0)GTA.Native.Function.playerInside=true;if(flag==1)GTA.Native.Function.driverFree=false;if(flag==2)GTA.Native.Function.passengers=1;Advance(h,o);Check(!o.TeleportedToBay&&o.Vehicle.moves==0,"occupied");});
 BayTest("player close to target blocks placement",()=>{var h=BayFixture();var o=Assigned(h);Game.Player.Character.Position=new Vector3(10,0,0);Advance(h,o);Check(o.Vehicle.moves==0&&!o.TeleportedToBay,"player clearance");});
 foreach(int fault in new[]{0,1,2})BayTest("missing destroyed invalid vehicle safe "+fault,()=>{var h=BayFixture();var o=Assigned(h);if(fault==0)o.Vehicle=null;if(fault==1)o.Vehicle.alive=false;if(fault==2)o.Vehicle.IsDead=true;Advance(h,o);Check(!o.Paid&&!h._activeCustomerOrders.Contains(o)&&h._cash==0,"invalid entity");});
 BayTest("multiple assignments reserve overlapping saved targets",()=>{var h=BayFixture();h.Worker();h._vehicleParkingBays[1].IsSet=true;h._vehicleParkingBays[1].Position=new Vector3(10,0,0);var a=Assigned(h);var b=h.Customer(0);Game.GameTime+=5000;h.AssignCustomerMechanic(b);Check(b.MechanicSlot==-1&&!h._mechanics[1].HasJob,"duplicate bay");h._vehicleParkingBays[1].Position=new Vector3(30,0,0);Game.GameTime+=5000;h.AssignCustomerMechanic(b);Check(b.MechanicSlot==1,"alternate bay");});
 BayTest("first serviced order keeps pickup priority",()=>{var h=BayFixture();var a=Assigned(h);Advance(h,a);a.Paid=true;a.Stage=CustomerStage.TeleportWaitingPickup;var b=h.Customer(0);b.Paid=true;b.MechanicSlot=0;b.Stage=CustomerStage.TeleportWaitingPickup;Check(!h.CanReturnCustomerCar(b),"priority");Check(NeedsDropoffSoon(a)&&IntakeInProgress(new CustomerOrder{Stage=CustomerStage.TeleportWaitingBay}),"queue guards");});
 BayTest("failed accepted quote save never moves or reserves",()=>{var h=BayFixture();h.saveSuccess=false;var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.MechanicSlot==-1&&!o.VehicleTeleport&&o.Vehicle.moves==0&&!h._mechanics[0].HasJob,"save rollback");});
 BayTest("partial native failure defers completion and bounds reattempt",()=>{var h=BayFixture();var o=Assigned(h);o.Vehicle.failHeading=true;Advance(h,o);Check(!o.TeleportedToBay&&o.Stage==CustomerStage.TeleportWaitingBay,"partial");int moves=o.Vehicle.moves;h.ProcessTeleportOrder(o);Check(o.Vehicle.moves==moves,"partial retry bound");o.Vehicle.failHeading=false;Advance(h,o);Check(o.TeleportedToBay&&o.Vehicle.moves==1,"recover without duplicate position write");});
 BayTest("pause prevents native queries and movement",()=>{var h=BayFixture();var o=Assigned(h);Game.IsPaused=true;int calls=GTA.Native.Function.calls;Advance(h,o);Check(GTA.Native.Function.calls==calls&&o.Vehicle.moves==0,"pause");});
 BayTest("saved bay is frozen for accepted order",()=>{var h=BayFixture();var o=Assigned(h);h._vehicleParkingBays[0].Position=new Vector3(40,0,0);h._vehicleParkingBays[0].Heading=12;Advance(h,o);Check(o.Vehicle.Position.X==10&&o.Vehicle.Heading==72,"snapshot");});
 BayTest("reload preserves quote without stale teleport/entity command",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].WorkSeconds=30;h.Save();var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Single().Quote.Total==o.Price&&loaded._pendingRepairPromises.Single().RemainingSeconds==90&&loaded._activeCustomerOrders.Count==0,"reload");});
 BayTest("saved separate positions and unknown settings remain untouched",()=>{var h=BayFixture();var d=ScriptSettings.Load(h.StatePath);d.SetValue("VehicleParkingBay1","X",10f);d.SetValue("WorkBay1","X",13f);d.SetValue("UserCustom","Keep","yes");d.Save();Assigned(h);d=ScriptSettings.Load(h.StatePath);Check(d.GetValue("VehicleParkingBay1","X",0f)==10&&d.GetValue("WorkBay1","X",0f)==13&&d.GetValue("UserCustom","Keep","")=="yes","saved coordinates");});
 BayTest("unconfigured custom bay leaves customer pending",()=>{var h=BayFixture();h._vehicleParkingBays[0].IsSet=false;var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.MechanicSlot==-1&&h._activeCustomerOrders.Contains(o)&&h._cash==0,"missing config");});
 BayTest("malformed target cannot move vehicle",()=>{var h=BayFixture();h._vehicleParkingBays[0].Heading=float.NaN;var o=h.Customer(0);h.AssignCustomerMechanic(o);Check(o.MechanicSlot==-1&&o.Vehicle.moves==0,"invalid coordinate");});
 BayTest("clearance query failure preserves pending repair",()=>{var h=BayFixture();var o=Assigned(h);GTA.Native.Function.throwQuery=true;Advance(h,o);Check(o.Vehicle.moves==0&&o.Stage==CustomerStage.TeleportWaitingBay&&h._mechanics[0].HasJob,"query recovery");});
 BayTest("pickup navigation failure cannot repeat teleport",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,o,120);h.failRelease=true;Advance(h,o);int moves=o.Vehicle.moves;Check(o.TeleportedToPickup&&moves==2,"moved");h.failRelease=false;Advance(h,o);Check(o.Vehicle.moves==moves&&o.Stage==CustomerStage.ReturningToVehicle,"no replay");});
 BayTest("AUTO LOT uses original computed offset heading",()=>{var h=BayFixture();h._automaticLotMode=true;h._bays[0].Heading=90;h._autoLotOffsetMeters=4;var o=Assigned(h);Advance(h,o);Check(Math.Abs(o.Vehicle.Position.X-13)<.001&&Math.Abs(o.Vehicle.Position.Y+4)<.001&&o.Vehicle.Heading==90,"computed bay");});
 BayTest("player occupying paid car blocks pickup too",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;RunRepair(h,o,120);GTA.Native.Function.playerInside=true;Advance(h,o);Check(o.Paid&&!o.TeleportedToPickup&&o.Vehicle.moves==1&&o.Stage==CustomerStage.TeleportWaitingPickup,"paid wait");});
 foreach(var page in new[]{MenuPage.MechanicBays,MenuPage.ServiceVehicleBays})BayTest("profile keeps source menu and slot "+page,()=>{var h=BayFixture();h._menuPage=page;h._selection=2;h.OpenServiceBayProfile();Check(h._serviceBaySlot==2&&h._serviceBayParent==page&&h._menuPage==MenuPage.ServiceBayProfile&&h._selection==0,"profile entry");});
 BayTest("profile vehicle and mechanic editors keep separate existing points",()=>{var h=BayFixture();h._serviceBaySlot=0;h._selection=0;h.ExecuteServiceBayProfile();Check(object.ReferenceEquals(h.editedPoint,h._vehicleParkingBays[0])&&h.editorBay==-1,"car point");h._selection=1;h.ExecuteServiceBayProfile();Check(object.ReferenceEquals(h.editedPoint,h._bays[0])&&h.editorBay==0,"work point");});
 BayTest("view and existing roster assignment have no save or money side effects",()=>{var h=BayFixture();h._selection=2;h.ExecuteServiceBayProfile();h._selection=3;h.ExecuteServiceBayProfile();Check(h.saves==0&&h._cash==0&&h._mechanics.Count==1,"configuration side effects");});
 BayTest("profile Back and invalid slot exit safely",()=>{var h=BayFixture();h._selection=4;h.ExecuteServiceBayProfile();h._serviceBaySlot=3;h.ExecuteServiceBayProfile();Check(h.backs==2,"profile back");});
 BayTest("missing car at settlement boundary neither charges nor throws",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;o.Vehicle=null;RunRepair(h,o,120);Check(!o.Paid&&h._cash==0&&h.invoiceWrites==0,"ghost car settlement");});
 BayTest("OFF failed settlement restores money stock and retries exactly once",()=>{var h=BayFixture();var o=Assigned(h);Advance(h,o);h._mechanics[0].ArrivalPhase=PhaseWorking;h.saveSuccess=false;RunRepair(h,o,120);Check(!o.Paid&&h._cash==0&&h._specialStock[0]==10,"failed payment rollback");int saves=h.saves;h.saveSuccess=true;h.ProcessOneCustomerRepair(o,1);Check(h.saves==saves,"retry cadence");Game.GameTime+=5000;h.ProcessOneCustomerRepair(o,1);h.ProcessOneCustomerRepair(o,1);Check(o.Paid&&h._cash==o.Price&&h.invoiceWrites==1&&h.saves==saves+1,"single retry");});
 Console.WriteLine($"bays {passed}/{passed} PASS; production transfer/repair/animation and real temporary save adapters, GTA NOT RUN");
    }finally{Directory.Delete(DataDirectory,true);}
    }
}
