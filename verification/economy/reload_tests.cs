using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
struct Vector3 {internal float X,Y,Z;}
class Ped {internal bool IsDead;internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
class Vehicle {internal Vector3 Position;internal bool IsDead;internal bool Exists()=>true;}
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
    bool TeleportAttemptDue(CustomerOrder o)=>true;
    bool TeleportTargetAvailable(CustomerOrder o,WorkBay b,bool pickup)=>true;
    /*<source:TrySellPart>*/
    /*<source:TrySellParts>*/
    /*<source:PartsSaleTotal>*/
    /*<source:PartsSalePreview>*/
    /*<source:TryCommitBusinessSave>*/
    /*<source:RepairLedgerStamp>*/
    /*<source:ToggleBusinessShift>*/
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
    WorkBay ComputeAutomaticBay(int slot)=>new WorkBay{IsSet=true};
    List<Vector3> BuildMechanicPickupExitRoute(Vector3 p)=>new List<Vector3>();
    void BeginVehicleWalk(CustomerOrder o,Mechanic m,List<Vector3> route){}
    void AwardMechanicXp(Mechanic m,int xp){m.Xp+=xp;}
    void RecordCustomerInvoice(CustomerOrder o){invoiceWrites++;}void AppendRemoteInvoice(RemoteJob o,Mechanic m){invoiceWrites++;}
    void ReleaseCustomerForPickup(CustomerOrder o){o.Stage=CustomerStage.ReturningToVehicle;}
    bool CanReturnCustomerCar(CustomerOrder o)=>false;void StartReturnVehicleWalk(CustomerOrder o){}
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
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh-reload-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(DataDirectory);try{
        Test("old saves default to no pending records without changing ledger",()=>{var h=Fresh();h._cash=31;h.LoadRepairPromises(new ScriptSettings());Check(h._pendingRepairPromises.Count==0&&h._cash==31&&h.saves==0,"old default");});
        Test("physical acceptance persists its exact quote before work starts",()=>{var h=Fresh();h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Count==1&&loaded._pendingRepairPromises[0].Route==1&&loaded._cash==0,"acceptance");SameQuote(o.Pricing,loaded._pendingRepairPromises[0].Quote);});
        Test("all physical background and remote quote fields roundtrip",()=>{var h=Fresh();var a=h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);a.WorkSeconds=37;var b=h.Worker();var bq=h.Background(b,45);var c=h.Worker();h._managerTier=0;var r=h.Remote(c,2,65);Check(h.Save(),"save");var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Count==3,"count");foreach(var q in new[]{o.Pricing,bq,r.Pricing})SameQuote(q,loaded._pendingRepairPromises.Single(p=>p.Quote.OrderId==q.OrderId).Quote);Check(loaded._pendingRepairPromises.Single(p=>p.Route==1).RemainingSeconds==o.RepairSeconds-37&&loaded._pendingRepairPromises.Single(p=>p.Route==0).RemainingSeconds==75&&loaded._pendingRepairPromises.Single(p=>p.Route==2).RemainingSeconds==65,"remaining");});
        Test("loading a year-old snapshot gives no offline progress or payment",()=>{var h=Fresh();var m=h.Worker();h.Background(m,45);h.Save();File.SetLastWriteTimeUtc(h.StatePath,DateTime.UtcNow.AddYears(-1));var loaded=ReadForTest();Check(!loaded._clockedIn&&!loaded._bootGameplayActivated&&loaded._cash==0&&loaded._pendingRepairPromises[0].RemainingSeconds==75&&loaded._mechanics.All(x=>!x.HasJob),"offline");});
        Test("pending quotes reserve exact dedicated and fallback stock before entities exist",()=>{var h=Fresh();h._specialStock[1]=1;h.Worker();h.AssignCustomerMechanic(h.Customer(1,2));h.Save();var loaded=ReadForTest();var free=loaded.UnreservedRepairStock(null);Check(free[0]==9&&free[2]==0&&loaded._parts==10&&loaded._specialStock[1]==1,"saved reservation");});
        Test("manual sale cannot take a pending quote's last dedicated unit",()=>{var h=Fresh();h._specialStock[0]=1;h._parts=0;h.Worker();h.AssignCustomerMechanic(h.Customer(0));var loaded=ReadForTest();loaded._bootGameplayActivated=true;Game.GameTime++;Check(!loaded.TrySellPart(0)&&loaded._specialStock[0]==1&&loaded._pendingRepairPromises.Count==1,"pending sale");});
        Test("pending stock is not reserved twice after physical entity reconstruction",()=>{var h=Fresh();h.Worker();h.AssignCustomerMechanic(h.Customer(1,2));var loaded=ReadForTest();var o=loaded.RecoverPhysical();Check(loaded.UnreservedRepairStock(null)[2]==8&&loaded._pendingRepairPromises.Count==0&&o.MechanicSlot==-1,"double reserve");});
        Test("recovered physical price sources and billable time survive wage/config changes",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);m.WorkSeconds=37;h.Save();long price=o.Price;var loaded=ReadForTest();loaded.Activate();loaded._wagePerEmployee=90000;loaded._unitCostEstimates[2]=999999;var recovered=loaded.RecoverPhysical();loaded.AssignCustomerMechanic(recovered);Check(recovered.Price==price&&recovered.RepairSeconds==o.RepairSeconds&&loaded._mechanics[0].WorkSeconds==37,"repricing");SameQuote(o.Pricing,recovered.Pricing);});
        Test("physical resumed work must finish remaining active time and pays once",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.WorkSeconds=55;h.Save();var loaded=ReadForTest();loaded.Activate();var recovered=loaded.RecoverPhysical();loaded.AssignCustomerMechanic(recovered);loaded._mechanics[0].ArrivalPhase=PhaseWorking;loaded.ProcessOneCustomerRepair(recovered,64);Check(!recovered.Paid&&loaded._cash==0,"early payment");loaded.ProcessOneCustomerRepair(recovered,1);loaded.ProcessOneCustomerRepair(recovered,1);Check(recovered.Paid&&loaded._cash==recovered.Price&&loaded._specialStock[0]==9&&loaded._customerCompleted==1,"once");var again=ReadForTest();Check(again._pendingRepairPromises.Count==0&&again._cash==recovered.Price&&again._specialStock[0]==9,"paid reload");});
        Test("recovered background work keeps accepted duration after JobSeconds changes",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m,45);h.Save();var loaded=ReadForTest();loaded.Activate();loaded._jobSeconds=999;loaded._mechanics[0].ArrivalPhase=PhaseWorking;loaded.ProcessEconomy(74);Check(loaded._cash==0&&loaded._mechanics[0].HasJob,"early");loaded.ProcessEconomy(1);Check(loaded._cash==q.Total&&!loaded._mechanics[0].HasJob&&loaded._parts==9,"snapshot duration");});
        Test("remote quote resumes from saved remaining active time and pays once",()=>{var h=Fresh();var m=h.Worker();h._managerTier=0;var j=h.Remote(m,0,60);h.Save();var loaded=ReadForTest();loaded.Activate();loaded.ProcessEconomy(1);Check(loaded._remoteJobs.Count==1&&loaded._remoteJobs[0].RemainingGameSeconds==60&&loaded._cash==0,"restore");loaded.ProcessRemoteOperations(59);Check(loaded._cash==0,"early");loaded.ProcessRemoteOperations(1);loaded.ProcessRemoteOperations(1);Check(loaded._cash==j.Price&&loaded._remoteJobsCompleted==1&&loaded._remotePartsConsumed==1&&loaded._specialStock[2]==9,"remote twice");});
        Test("remote record remains parked without a manager instead of becoming free income",()=>{var h=Fresh();var m=h.Worker();h._managerTier=0;h.Remote(m,0);h.Save();var loaded=ReadForTest();loaded.Activate();loaded._managerTier=-1;loaded._customerServiceEnabled=true;loaded.ProcessEconomy(1);Check(loaded._pendingRepairPromises.Count==1&&loaded._remoteJobs.Count==0&&loaded._cash==0,"no manager");loaded._managerTier=0;loaded.ProcessEconomy(1);Check(loaded._pendingRepairPromises.Count==0&&loaded._remoteJobs.Count==1,"rehire");});
        Test("pause blocks restore and all direct economy/remote payout entry points",()=>{var h=Fresh();var m=h.Worker();h._managerTier=0;h.Remote(m,0,0);h.Save();var loaded=ReadForTest();loaded.Activate();Game.IsPaused=true;try{loaded.ProcessEconomy(100);loaded.ProcessRemoteOperations(100);Check(loaded._pendingRepairPromises.Count==1&&loaded._remoteJobs.Count==0&&loaded._cash==0,"paused");}finally{Game.IsPaused=false;}});
        Test("closed shift blocks recovered work and does not advance remaining seconds",()=>{var h=Fresh();var m=h.Worker();h.Background(m,55);h.Save();var loaded=ReadForTest();loaded._bootGameplayActivated=true;loaded.ProcessEconomy(999);Check(loaded._pendingRepairPromises[0].RemainingSeconds==65&&loaded._cash==0,"closed");});
        Test("graceful abort snapshot survives subsequent actor cleanup",()=>{var h=Fresh();var a=h.Worker();h.AssignCustomerMechanic(h.Customer(0));a.WorkSeconds=55;var b=h.Worker();h.Background(b,42);h._managerTier=0;var c=h.Worker();h.Remote(c,2,33);h.ParkRuntimeRepairPromises();h._activeCustomerOrders.Clear();h._remoteJobs.Clear();foreach(var m in h._mechanics){m.HasJob=false;m.Pricing=null;m.WorkSeconds=0;}h.Save();var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Count==3&&loaded._pendingRepairPromises.Single(p=>p.Route==1).RemainingSeconds==65&&loaded._pendingRepairPromises.Single(p=>p.Route==0).RemainingSeconds==78&&loaded._pendingRepairPromises.Single(p=>p.Route==2).RemainingSeconds==33,"abort loss");});
        Test("entity-loss cancellation parks accepted quote before worker progress is reset",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.WorkSeconds=55;h.RememberCancelledRepair(o);h._activeCustomerOrders.Clear();m.HasJob=false;m.WorkSeconds=0;h.Save();var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Single().RemainingSeconds==65,"cancel loss");});
        Test("manager handoff snapshots one remote identity without duplicate physical record",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);h._remoteJobs.Add(new RemoteJob{MechanicSlot=0,OrderId=o.Id,ServiceKind=o.ServiceKind,PartsNeeded=o.PartsNeeded,Price=o.Price,Pricing=o.Pricing,RemainingGameSeconds=55});h.RememberCancelledRepair(o);var records=h.CaptureRepairPromises();Check(records.Count==1&&records[0].Route==2&&records[0].RemainingSeconds==55&&h._pendingRepairPromises.Count==0,"handoff duplicate");});
        Test("paid returning vehicle does not persist another payable promise",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);o.Paid=true;m.HasJob=true;h.Save();Check(ReadForTest()._pendingRepairPromises.Count==0,"paid return");});
        Test("legacy promised amount below new floor roundtrips without surcharge",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m);q.LegacyPromise=true;q.Total=1;h.Save();var loaded=ReadForTest();loaded.Activate();loaded._mechanics[0].ArrivalPhase=PhaseWorking;loaded._unitCostEstimates[0]=99999;loaded.ProcessEconomy(120);Check(loaded._cash==1&&loaded._parts==9,"legacy price");});
        Test("failed physical settlement rolls back stock money paid flag XP counters and save bytes",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);string before=File.ReadAllText(h.StatePath);m.ArrivalPhase=PhaseWorking;h.saveSuccess=false;h.ProcessOneCustomerRepair(o,120);Check(!o.Paid&&m.HasJob&&m.Xp==0&&m.JobsCompleted==0&&h._cash==0&&h._revenue==0&&h._specialStock[0]==10&&h._completed==0&&h._customerCompleted==0&&File.ReadAllText(h.StatePath)==before&&h.invoiceWrites==0,"physical rollback");});
        Test("failed repair commit retries at most once per three game seconds and settles once",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h.saveSuccess=false;Game.GameTime=100;h.ProcessOneCustomerRepair(o,120);int saves=h.saves;for(int i=0;i<100;i++)h.ProcessOneCustomerRepair(o,1);Game.GameTime=3099;h.ProcessOneCustomerRepair(o,1);Check(h.saves==saves&&!o.Paid,"per-frame writes");h.saveSuccess=true;Game.GameTime=3100;h.ProcessOneCustomerRepair(o,1);h.ProcessOneCustomerRepair(o,1);Check(o.Paid&&h.saves==saves+1&&h._cash==o.Price&&h.invoiceWrites==1,"retry duplicate");});
        Test("commit cooldown handles signed GameTime wraparound",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m);h.Save();h.saveSuccess=false;Game.GameTime=int.MaxValue-999;Check(!h.TrySettleRepair(q,m,null,null),"failure");h.saveSuccess=true;Game.GameTime=unchecked(int.MaxValue-999+3000);Check(h.TrySettleRepair(q,m,null,null)&&h._cash==q.Total,"wrap retry");});
        Test("failed background settlement restores accepted job and exact quote",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m,119);h.Save();m.ArrivalPhase=PhaseWorking;h.saveSuccess=false;h.ProcessEconomy(1);Check(m.HasJob&&Object.ReferenceEquals(m.Pricing,q)&&h._parts==10&&h._cash==0&&h._completed==0,"background rollback");});
        Test("failed remote settlement restores queue position and all remote totals",()=>{var h=Fresh();var m=h.Worker();h._managerTier=0;var j=h.Remote(m,0,1);h.Save();h.saveSuccess=false;h.ProcessRemoteOperations(1);Check(h._remoteJobs.Count==1&&h._remoteJobs[0]==j&&m.HasJob&&h._remoteSales==0&&h._remoteJobsCompleted==0&&h._remotePartsConsumed==0&&h._specialStock[2]==10&&h._cash==0,"remote rollback");});
        Test("accepted price mismatch cannot consume or silently replace a quote",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);o.Price++;m.ArrivalPhase=PhaseWorking;h.ProcessOneCustomerRepair(o,120);Check(!o.Paid&&h._cash==0&&h._specialStock[0]==10,"mismatch");});
        Test("failed assignment commit does not accept premium price or reserve worker",()=>{var h=Fresh();var m=h.Worker(10,1);var o=h.Customer(1,2);long before=o.Price;h.saveSuccess=false;h.AssignCustomerMechanic(o);Check(o.MechanicSlot==-1&&o.Pricing==null&&o.Price==before&&!m.HasJob&&h._specialStock[1]==10&&!File.Exists(h.StatePath),"assignment rollback");});
        Test("old paid counters remain paid after repeated reloads with stale record sections",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m);h.Save();m.ArrivalPhase=PhaseWorking;h.ProcessEconomy(120);Check(File.ReadAllText(h.StatePath).Contains("OrderId="),"stale key retention");for(int i=0;i<3;i++){var loaded=ReadForTest();Check(loaded._pendingRepairPromises.Count==0&&loaded._cash==q.Total&&loaded._completed==1,"stale replay");}});
        Test("accepted records preserve old supplier invoice costs and unknown keys",()=>{var h=Fresh();var d=ScriptSettings.Load(h.StatePath);d.SetValue("Purchasing","DeliveryCost",12345L);d.SetValue("BusinessBills","SupplierInvoicesDue",567L);d.SetValue("UserCustom","Keep","untouched");d.Save();h.Worker();h.AssignCustomerMechanic(h.Customer(0));var after=ScriptSettings.Load(h.StatePath);Check(after.GetValue("Purchasing","DeliveryCost",0L)==12345&&after.GetValue("BusinessBills","SupplierInvoicesDue",0L)==567&&after.GetValue("UserCustom","Keep","")=="untouched","key damage");});
        Test("malformed version or over-capacity count fails closed without save changes",()=>{foreach(int count in new[]{-1,17}){var h=Fresh();var d=ScriptSettings.Load(h.StatePath);d.SetValue("RepairPromisesV1","Count",count);d.Save();string before=File.ReadAllText(h.StatePath);bool threw=false;try{h.LoadRepairPromises(d);}catch(InvalidDataException){threw=true;}Check(threw&&File.ReadAllText(h.StatePath)==before&&h.saves==0,"count");}var x=Fresh();var data=new ScriptSettings();data.SetValue("RepairPromisesV1","Count",1);data.SetValue("RepairPromisesV1","Version",2);bool bad=false;try{x.LoadRepairPromises(data);}catch(InvalidDataException){bad=true;}Check(bad,"version");});
        Test("invalid nonfinite duration negative stock allocation or money never loads",()=>{foreach(string key in new[]{"BillableSeconds","RemainingSeconds","Total","DedicatedUnits"}){var h=Fresh();var m=h.Worker();h.Background(m);h.Save();var d=ScriptSettings.Load(h.StatePath);d.SetValue("RepairPromiseV1.0",key,key=="BillableSeconds"?"NaN":"-1");string before=File.ReadAllText(h.StatePath);bool threw=false;try{ReadForTest(d);}catch(InvalidDataException e){threw=e.Message.Contains("Invalid or duplicate");}Check(threw&&File.ReadAllText(h.StatePath)==before,"invalid "+key);}});
        Test("duplicate accepted identities fail closed instead of replaying twice",()=>{var h=Fresh();var m=h.Worker();h.Background(m);h.Save();var d=ScriptSettings.Load(h.StatePath);foreach(var kv in d.values.Where(x=>x.Key.StartsWith("RepairPromiseV1.0|")).ToArray())d.values[kv.Key.Replace("RepairPromiseV1.0|","RepairPromiseV1.1|")]=kv.Value;d.SetValue("RepairPromisesV1","Count",2);bool threw=false;try{ReadForTest(d);}catch(InvalidDataException e){threw=e.Message.Contains("Invalid or duplicate");}Check(threw,"duplicate ID");});
        Test("pending-record capacity prevents new promises before bounded ledger overflows",()=>{var h=Fresh();var m=h.Worker();for(int i=1;i<=13;i++)h._pendingRepairPromises.Add(new SavedRepairPromise{Quote=new RepairQuote{OrderId=i,ServiceKind=-1,UniversalUnits=1,Total=500,BillableSeconds=120},MechanicSlot=0,Route=0,RemainingSeconds=120});RepairQuote q;Check(!h.TryCreateRepairQuote(-1,1,m,120,500,0,out q)&&h._cash==0,"capacity");});
        Test("missing original mechanic holds/reassigns safely without losing saved quote",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m,55);h.Save();var loaded=ReadForTest();loaded.Activate();loaded._mechanics.Clear();loaded.ProcessEconomy(1);Check(loaded._pendingRepairPromises.Count==1&&loaded._cash==0,"no staff");loaded.Worker().ArrivalPhase=PhaseWorking;loaded.ProcessEconomy(65);Check(loaded._cash==q.Total&&loaded._pendingRepairPromises.Count==0,"staff return");});
        Test("ready-but-unpaid record waits for positive unpaused gameplay after load",()=>{var h=Fresh();var m=h.Worker();var q=h.Background(m,120);h.Save();var loaded=ReadForTest();loaded.Activate();loaded._mechanics[0].ArrivalPhase=PhaseWorking;loaded.ProcessEconomy(0);Check(loaded._cash==0&&loaded._pendingRepairPromises.Count==1,"load/zero tick payment");loaded.ProcessEconomy(.1);Check(loaded._cash==q.Total&&loaded._completed==1,"active completion");});
        Test("all terminal settlements leave supplier and payroll expenses untouched",()=>{foreach(int route in new[]{0,1,2}){var h=Fresh();var m=h.Worker();if(route==0){var q=h.Background(m);h.Save();h.TrySettleRepair(q,m,null,null);}else if(route==1){var o=h.Customer(0);h.AssignCustomerMechanic(o);h.TrySettleRepair(o.Pricing,m,o,null);}else{h._managerTier=0;var j=h.Remote(m,0);h.Save();h.TrySettleRepair(j.Pricing,m,null,j);}Check(h._supplierInvoicesDue==567&&h._partsSpend==31&&h._payrollSpend==32,"double expense");}});
        Test("older-build financial edits cannot replay an unknown stale quote journal",()=>{var h=Fresh();var m=h.Worker();h.Background(m);h.Save();var d=ScriptSettings.Load(h.StatePath);d.SetValue("Business","Cash",500L);d.Save();string before=File.ReadAllText(h.StatePath);bool threw=false;try{ReadForTest();}catch(InvalidDataException e){threw=e.Message.Contains("does not match");}Check(threw&&File.ReadAllText(h.StatePath)==before,"stale rollback journal");});
        Test("older-build inventory changes with unchanged cash still fail closed",()=>{var h=Fresh();var m=h.Worker();h.Background(m);h.Save();var d=ScriptSettings.Load(h.StatePath);d.SetValue("Stock","Stock0",9);d.Save();bool threw=false;try{ReadForTest();}catch(InvalidDataException e){threw=e.Message.Contains("does not match");}Check(threw,"stale inventory");});
        Test("stock and money alone cannot forge a replay after completed count changed",()=>{var h=Fresh();var m=h.Worker();h.Background(m);h.Save();var d=ScriptSettings.Load(h.StatePath);d.SetValue("Business","Completed",1);d.Save();bool threw=false;try{ReadForTest();}catch(InvalidDataException e){threw=e.Message.Contains("does not match");}Check(threw,"completion stamp");});
        Test("an empty journal permits ordinary older-save finance changes",()=>{var h=Fresh();h.Save();var d=ScriptSettings.Load(h.StatePath);d.SetValue("Business","Cash",500L);d.Save();var loaded=ReadForTest();Check(loaded._cash==500&&loaded._pendingRepairPromises.Count==0,"empty compatibility");});
        Test("closing the business parks all accepted routes before clearing runtime workers",()=>{var h=Fresh();h._placed=true;var a=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);a.WorkSeconds=55;var b=h.Worker();h.Background(b,42);h._managerTier=0;var c=h.Worker();h.Remote(c,2,33);h.ToggleBusinessShift();var loaded=ReadForTest();Check(!h._clockedIn&&loaded._pendingRepairPromises.Count==3&&loaded._pendingRepairPromises.Single(p=>p.Route==1).RemainingSeconds==65&&loaded._pendingRepairPromises.Single(p=>p.Route==0).RemainingSeconds==78&&loaded._pendingRepairPromises.Single(p=>p.Route==2).RemainingSeconds==33&&h._cash==0,"close loss");});
        Test("reopening a closed saved shift preserves quotes and advances no work by itself",()=>{var h=Fresh();h._placed=true;var m=h.Worker();var q=h.Background(m,55);h.ToggleBusinessShift();var loaded=ReadForTest();loaded._placed=true;loaded._bootGameplayActivated=true;loaded.ToggleBusinessShift();Check(loaded._clockedIn&&loaded._cash==0&&loaded._pendingRepairPromises.Count==1&&loaded._pendingRepairPromises[0].RemainingSeconds==65&&loaded._pendingRepairPromises[0].Quote.Total==q.Total,"open catchup");});
        Console.WriteLine("reload "+passed+"/"+passed+" PASS; production promise serialization/restoration/atomic settlement; synthetic INI adapter and files only; GTA testing NOT RUN");
    }finally{Directory.Delete(DataDirectory,true);}}
}
