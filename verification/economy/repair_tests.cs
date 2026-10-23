using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
struct Vector3 {internal float X,Y,Z;}
class Ped {internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
class Vehicle {internal Vector3 Position;}
class Blip {}
class ScriptSettings {
 internal Dictionary<string,long> values=new Dictionary<string,long>();
 internal T GetValue<T>(string section,string key,T fallback){return values.TryGetValue(section+"."+key,out long value)?(T)Convert.ChangeType(value,typeof(T)):fallback;}
}
static class Game {internal static PlayerState Player=new PlayerState();internal class PlayerState {internal Ped Character=new Ped();}}
class Harness {
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
    long _cash,_revenue,_customerRevenue,_remoteSales,_supplierInvoicesDue=567,_partsSpend=31,_payrollSpend=32;
    int _parts=10,_completed,_customerCompleted,_remoteJobsCompleted,_remotePartsConsumed,_lastCustomerOrderId;
    int[] _specialStock={10,10,10},_specialUnitCosts={90,160,125};
    readonly long[] _unitCostEstimates=new long[4];ScriptSettings _configuration=new ScriptSettings();
    int _partCost=100,_jobRevenue=500,_wagePerEmployee=250,_jobSeconds=120,_betweenJobsSeconds=10,_wagePeriodSeconds=300;
    bool _toolSetPurchased,_diagnosticScannerPurchased,_automaticLotMode,_vehicleTransfersEnabled,_remoteMode,_clockedIn=true,_placed,_customerServiceEnabled,_receptionistHired;
    int _managerTier=-1,_spawnDistance=65,_receptionistWage,_receptionistWageBonus,_overheadPeriodSeconds=600,_rentAndUtilities,_payrollDueSeconds=300;
    long _receptionWagesDue,_managerWagesDue,_rentUtilitiesDue;double _receptionWageUnpaidSeconds,_managerWageAgeSeconds,_rentUnpaidSeconds,_payrollClock,_overheadClock;
    bool _receptionOverdueNotified;HashSet<int> _mechanicOverdueNotified=new HashSet<int>();Vector3 _location;
    double _remoteIdleGameSeconds;int _remoteJobGameSeconds=180;DateTime _nextCustomerAt;string _lastOrderOutcome;
    List<Mechanic> _mechanics=new List<Mechanic>();List<CustomerOrder> _activeCustomerOrders=new List<CustomerOrder>();List<RemoteJob> _remoteJobs=new List<RemoteJob>();
    WorkBay[] _bays={new WorkBay{IsSet=true},new WorkBay{IsSet=true},new WorkBay{IsSet=true}},_vehicleParkingBays={new WorkBay(),new WorkBay(),new WorkBay()};
    CustomerOrder _customerOrder;int saves,invoiceWrites;List<string> notices=new List<string>();
    long savedCash,savedRevenue;int savedParts;
    void Save(){saves++;savedCash=_cash;savedRevenue=_revenue;savedParts=_parts;}
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
    static void Test(string name,Action action){foreach(var p in Directory.GetFiles(DataDirectory))File.Delete(p);action();passed++;Console.WriteLine("PASS repair "+name);}
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh-repair-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(DataDirectory);try{
        Test("conservative source fallback uses express ceiling for all four buckets",()=>{var h=Fresh();Check(h._unitCostEstimates.SequenceEqual(new long[]{125,113,200,157}),"fallback");});
        Test("explicit estimates are fixed and zero-safe; negative selects fallback",()=>{var h=new Harness();h._configuration.values["Pricing.UnitCostEstimateUniversal"]=0;h._configuration.values["Pricing.UnitCostEstimateFilters"]=77;h.InitialiseRepairPricing();Check(h.EstimatedUnitAcquisitionCost(-1)==0&&h.EstimatedUnitAcquisitionCost(0)==77&&h.EstimatedUnitAcquisitionCost(1)==200,"configured");});
        Test("existing total is a floor and is never stacked twice",()=>{var q=CalculateRepairQuote(500,75,0,1,0,113,125,250,300,120);Check(q.Total==575&&q.PartsEstimate==113&&q.LaborEstimate==100,"baseline stacked");});
        Test("high wage raises total to parts plus rounded labor",()=>{var q=CalculateRepairQuote(500,0,-1,0,1,0,125,6000,300,120);Check(q.Total==2525&&q.LaborEstimate==2400,"coverage");});
        Test("fractional labor rounds up without negative money",()=>{var q=CalculateRepairQuote(-1,-2,-1,0,1,0,0,1,300,1);Check(q.Total==1&&q.LaborEstimate==1&&q.Baseline==0&&q.Premium==0,"rounding");});
        Test("quote rejects NaN infinite time invalid buckets and negative counts",()=>{foreach(double d in new[]{double.NaN,double.PositiveInfinity,-1d}){bool threw=false;try{CalculateRepairQuote(1,0,-1,0,1,0,1,1,30,d);}catch(ArgumentOutOfRangeException){threw=true;}Check(threw,"bad duration");}});
        Test("monetary overflow fails instead of wrapping quote",()=>{bool threw=false;try{CalculateRepairQuote(long.MaxValue,1,-1,0,1,0,1,1,30,1);}catch(OverflowException){threw=true;}Check(threw,"overflow");});
        Test("brakes use one dedicated and one universal when appropriate",()=>{var h=Fresh();h._specialStock[1]=1;var m=h.Worker();RepairQuote q;Check(h.TryCreateRepairQuote(1,2,m,150,800,0,out q)&&q.DedicatedUnits==1&&q.UniversalUnits==1&&q.PartsEstimate==325,"allocation");});
        Test("all four stock classes calculate their actual chosen source",()=>{foreach(int k in new[]{-1,0,1,2}){var h=Fresh();var m=h.Worker();RepairQuote q;Check(h.TryCreateRepairQuote(k,1,m,120,500,0,out q),"quote");Check(q.PartsEstimate==h.EstimatedUnitAcquisitionCost(k),"unit basis");}});
        Test("negative and zero inventory cannot create a repair quote",()=>{var h=Fresh();h._parts=-1;h._specialStock[0]=0;var m=h.Worker();RepairQuote q;Check(!h.TryCreateRepairQuote(0,1,m,120,500,0,out q),"phantom stock");});
        Test("assigned physical reservation protects exact dedicated and fallback buckets",()=>{var h=Fresh();h._parts=2;h._specialStock[1]=1;h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);int[] a=h.UnreservedRepairStock(null);Check(a[0]==1&&a[2]==0,"bucket reservation");});
        Test("unassigned waiting customer does not reserve inventory",()=>{var h=Fresh();h.Customer(1,2);Check(h.UnreservedRepairStock(null)[0]==10&&h.UnreservedRepairStock(null)[2]==10,"unassigned reservation");});
        Test("background job reserves universal only even while away",()=>{var h=Fresh();var m=h.Worker();m.HasJob=true;Check(h.UnreservedRepairStock(null)[0]==9,"background missing");});
        Test("remote reserved quote and its mechanic are not counted twice",()=>{var h=Fresh();var m=h.Worker();RepairQuote q;h.TryCreateRepairQuote(0,1,m,120,500,0,out q);m.HasJob=true;h._remoteJobs.Add(new RemoteJob{MechanicSlot=0,PartsNeeded=1,ServiceKind=0,Pricing=q});Check(h.UnreservedRepairStock(null)[1]==9&&h.UnreservedRepairStock(null)[0]==10,"remote double reservation");});
        Test("handoff snapshot shared by physical and remote reserves once",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);h._remoteJobs.Add(new RemoteJob{MechanicSlot=0,Pricing=o.Pricing});Check(h.UnreservedRepairStock(null)[2]==8,"handoff duplicate");});
        Test("paid vehicle return reserves no additional inventory",()=>{var h=Fresh();h.Worker().HasJob=true;var o=h.Customer(0);o.MechanicSlot=0;o.Paid=true;Check(h.UnreservedRepairStock(null)[0]==10&&h.UnreservedRepairStock(null)[1]==10,"paid reservation");});
        Test("multiple simultaneous customer quotes cannot promise the same stock",()=>{var h=Fresh();h._parts=0;h._specialStock[0]=1;h.Worker();h.Worker();var first=h.Customer(0);var second=h.Customer(0);h.AssignCustomerMechanic(first);h.AssignCustomerMechanic(second);Check(first.MechanicSlot==0&&second.MechanicSlot==-1,"double promise");});
        Test("specialist wage and wage override enter cost floor without payroll charge",()=>{var h=Fresh();var m=h.Worker(10,1,6000);var o=h.Customer(1,2);h.AssignCustomerMechanic(o);Check(h.MechanicWage(m)==6300&&o.Price>=o.Pricing.PartsEstimate+o.Pricing.LaborEstimate&&h._payrollSpend==32&&h._cash==0,"wage coverage");});
        Test("equipment speed discounts affect billable time and labor once",()=>{var h=Fresh();h._toolSetPurchased=h._diagnosticScannerPurchased=true;var m=h.Worker(10,2,0);var o=h.Customer(2);h.AssignCustomerMechanic(o);double expected=120*Math.Max(.55,1-9*.035-.15)*.93*.9;Check(Math.Abs(o.RepairSeconds-expected)<.00001&&o.Pricing.Premium==255,"speed/premium");});
        Test("duplicate mechanic assignment does not add expertise twice",()=>{var h=Fresh();h.Worker(10,1);var o=h.Customer(1,2);h.AssignCustomerMechanic(o);long amount=o.Price;h.AssignCustomerMechanic(o);Check(o.Price==amount,"duplicate premium");});
        Test("insufficient or blocked repair never consumes or pays",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseRouteBlocked;h._customerOrder=o;h.ProcessOneCustomerRepair(o,120);Check(h._cash==0&&h._specialStock[0]==10&&h._supplierInvoicesDue==567,"failed job");});
        Test("customer pays and consumes exactly once with no extra expense",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h.ProcessOneCustomerRepair(o,10000);long paid=h._cash;h.ProcessOneCustomerRepair(o,10000);Check(paid==o.Price&&h._cash==paid&&h._specialStock[1]==8&&h._customerCompleted==1&&h.invoiceWrites==1&&h._supplierInvoicesDue==567&&h._partsSpend==31&&h._payrollSpend==32,"payment/expense");});
        Test("customer completion waits when its reserved bucket is missing",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h._specialStock[0]=0;h.ProcessOneCustomerRepair(o,120);Check(!o.Paid&&h._cash==0&&h._parts==10,"silent substitution");});
        Test("payment guard rejects long overflow before consumption",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h._cash=long.MaxValue;h.ProcessOneCustomerRepair(o,120);Check(!o.Paid&&h._specialStock[0]==10,"overflow consumed");});
        Test("background route uses cost floor and credits once",()=>{var h=Fresh();h._wagePerEmployee=6000;var m=h.Worker();h.ProcessEconomy(10);Check(m.HasJob&&m.Pricing.Total==2525,"background quote");m.ArrivalPhase=PhaseWorking;h.ProcessEconomy(120);Check(!m.HasJob&&h._parts==9&&h._cash==2525&&h._completed==1&&h._supplierInvoicesDue==567,"background payout");});
        Test("remote job quotes use actual shortened duration and pay once",()=>{var h=Fresh();h._managerTier=0;h._remoteMode=h._customerServiceEnabled=h._receptionistHired=true;h._wagePerEmployee=6000;var m=h.Worker();h.ProcessRemoteOperations(200);Check(h._remoteJobs.Count==1,"remote schedule");var j=h._remoteJobs[0];Check(j.Price==j.Pricing.Total&&j.Price>=j.Pricing.PartsEstimate+j.Pricing.LaborEstimate,"remote floor");h._remoteMode=false;h.ProcessRemoteOperations(j.RemainingGameSeconds);long paid=h._cash;h.ProcessRemoteOperations(1);Check(paid==j.Price&&h._cash==paid&&h._remoteJobs.Count==0&&h._remoteJobsCompleted==1&&h._supplierInvoicesDue==567,"remote payout");});
        Test("zero active time advances no remote work or payment",()=>{var h=Fresh();h._managerTier=0;h._remoteMode=true;h._remoteJobs.Add(new RemoteJob{MechanicSlot=0,RemainingGameSeconds=20});h.Worker().HasJob=true;h.ProcessRemoteOperations(0);Check(h._remoteJobs[0].RemainingGameSeconds==20&&h._cash==0,"paused progress");});
        Test("legacy promised quote is retained rather than silently increased",()=>{var h=Fresh();h._managerTier=0;h._wagePerEmployee=6000;h.Worker().HasJob=true;var job=new RemoteJob{MechanicSlot=0,OrderId=1,ServiceKind=0,PartsNeeded=1,Price=500,RemainingGameSeconds=0};h._remoteJobs.Add(job);h.ProcessRemoteOperations(1);Check(h._cash==500&&job.Pricing.LegacyPromise&&job.Pricing.Total==500,"legacy repriced");});
        Test("new audit schema leaves old invoice writer format independent",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h.ProcessOneCustomerRepair(o,120);var lines=File.ReadAllLines(Path.Combine(DataDirectory,"RepairPricing.v1.csv"));Check(lines.Length==2&&lines[0].Split(',').Length==13&&lines[1].Contains(",CUSTOMER,"),"pricing audit");});
        Test("quote estimate is immutable after procurement selection or configuration changes",()=>{var h=Fresh();h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);long price=o.Price;h._unitCostEstimates[1]=99999;Check(o.Price==price&&o.Pricing.PartsEstimate==113,"promised quote changed");});
        Test("saved paid totals restore without re-crediting canceled runtime jobs",()=>{var h=Fresh();var m=h.Worker();var o=h.Customer(0);h.AssignCustomerMechanic(o);m.ArrivalPhase=PhaseWorking;h.ProcessOneCustomerRepair(o,120);var restored=Fresh();restored._cash=h.savedCash;restored._revenue=h.savedRevenue;restored._parts=h.savedParts;restored.ProcessCustomerRepair(120);Check(restored._cash==h._cash&&restored._revenue==h._revenue,"reload double pay");});
        Test("no clock catch-up is introduced in quote computation",()=>{var h=Fresh();var m=h.Worker();RepairQuote a,b;h.TryCreateRepairQuote(-1,1,m,120,500,0,out a);h.TryCreateRepairQuote(-1,1,m,120,500,0,out b);Check(a.Total==b.Total&&h._cash==0&&h.saves==0,"quote side effects");});
        Console.WriteLine($"repair {passed}/{passed} PASS; GTA testing NOT RUN");
    }finally{Directory.Delete(DataDirectory,true);}}
}
