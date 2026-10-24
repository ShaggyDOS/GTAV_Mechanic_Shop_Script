using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
struct Vector3 {internal float X,Y,Z;}
class Ped {internal bool IsDead;internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
class Vehicle {internal Vector3 Position;internal bool IsDead;internal bool Exists()=>true;}
class Blip {}
class ScriptSettings {
 internal Dictionary<string,long> values=new Dictionary<string,long>();
 internal T GetValue<T>(string section,string key,T fallback){return values.TryGetValue(section+"."+key,out long value)?(T)Convert.ChangeType(value,typeof(T)):fallback;}
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
    bool saveSuccess=true;bool Save(){saves++;if(!saveSuccess)return false;savedCash=_cash;savedRevenue=_revenue;savedParts=_parts;return true;}
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
    static void Test(string name,Action action){foreach(var p in Directory.GetFiles(DataDirectory))File.Delete(p);action();passed++;Console.WriteLine("PASS sales "+name);}
    /*<source:MenuPage>*/
    /*<source:ExecutePartsSalesSelection>*/
    /*<source:AdjustPartsSaleCart>*/
    int _selection;MenuPage _menuPage=MenuPage.PartsSales;readonly int[] _partsSaleCart=new int[4];int[] _partsSaleReview=new int[4];
    void ShowStatusNotice(string title,params string[] lines){}void NavigateBack(){_menuPage=MenuPage.PartsSales;_selection=4;}
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh-retail336-"+Guid.NewGuid());Directory.CreateDirectory(DataDirectory);try{
Test("mixed quantities commit all lines once at unchanged estimates",()=>{var h=Fresh();Check(h.TrySellParts(new[]{2,3,1,4}),"batch sale");Check(h._cash==1417&&h._revenue==1417&&h._partsSalesRevenue==1417&&h._partsSalesUnitsSold==10&&h._parts==8&&h._specialStock.SequenceEqual(new[]{7,9,6})&&h.saves==1,"amount/stock");Check(!h.TrySellParts(new[]{2,3,1,4})&&h.saves==1,"same frame repeat");var lines=File.ReadAllLines(Path.Combine(DataDirectory,"PartsSales.v1.csv"));Check(lines.Length==5&&lines.All(x=>x.Split(',').Length==6),"compatible line audit");});
Test("mixed sale failed save rolls back every line and total",()=>{var h=Fresh();h.saveSuccess=false;Check(!h.TrySellParts(new[]{2,3,1,4})&&h._cash==0&&h._revenue==0&&h._partsSalesRevenue==0&&h._partsSalesUnitsSold==0&&h._parts==10&&h._specialStock.All(x=>x==10),"rollback");Check(!File.Exists(Path.Combine(DataDirectory,"PartsSales.v1.csv")),"uncommitted csv");});
Test("one insufficient bucket rejects whole batch",()=>{var h=Fresh();h._specialStock[1]=0;Check(!h.TrySellParts(new[]{2,1,1,1})&&h.saves==0&&h._parts==10&&h._cash==0,"partial batch");});
Test("accepted quote reserved stock remains unavailable at confirmation",()=>{var h=Fresh();h._parts=1;h.Worker().HasJob=true;Check(!h.TrySellParts(new[]{1,2,0,0})&&h.saves==0&&h._specialStock[0]==10,"reserved stock");});
Test("draft and preview do not sell; confirmation uses snapshot",()=>{var h=Fresh();h._selection=0;h.ExecutePartsSalesSelection();h.AdjustPartsSaleCart(1);h._selection=4;h.ExecutePartsSalesSelection();Check(h._menuPage==MenuPage.PartsSaleReview&&h._partsSaleReview[0]==2&&h.saves==0&&h._parts==10,"review mutation");h._partsSaleCart[0]=7;h._selection=1;h.ExecutePartsSalesSelection();Check(h._parts==8&&h._cash==250&&h._partsSaleCart.All(x=>x==0)&&h.saves==1,"confirm snapshot");});
Test("clear draft and back have no business writes",()=>{var h=Fresh();h._partsSaleCart[0]=5;h._selection=5;h.ExecutePartsSalesSelection();h._selection=6;h.ExecutePartsSalesSelection();Check(h._partsSaleCart.All(x=>x==0)&&h.saves==0&&h._cash==0&&h._parts==10,"clear/back");});
Test("quantity bounds do not allocate or change stock",()=>{var h=Fresh();h._selection=0;for(int i=0;i<200;i++)h.AdjustPartsSaleCart(1);Check(h._partsSaleCart[0]==100,"upper");for(int i=0;i<200;i++)h.AdjustPartsSaleCart(-1);Check(h._partsSaleCart[0]==0&&h.saves==0,"lower");});
foreach(var qty in new[]{new[]{0,0,0,0},new[]{-1,0,0,0},new[]{101,0,0,0},new[]{1,0}}){var invalid=qty;Test("invalid/empty retail quantities rejected "+string.Join(",",invalid),()=>{var h=Fresh();Check(!h.TrySellParts(invalid)&&h.saves==0&&h._parts==10,"invalid");});}
Test("checked batch estimate overflow leaves stock intact",()=>{var h=Fresh();h._unitCostEstimates[0]=long.MaxValue;Check(!h.TrySellParts(new[]{3,0,0,0})&&h.saves==0&&h._parts==10&&h._cash==0,"overflow");});
Test("paused confirmation cannot sell",()=>{var h=Fresh();Game.IsPaused=true;Check(!h.TrySellParts(new[]{2,0,0,0})&&h.saves==0,"paused");Game.IsPaused=false;});
Console.WriteLine($"retail336 {passed}/{passed} PASS; production batch retail, review/confirm and rollback, GTA NOT RUN");
}finally{Directory.Delete(DataDirectory,true);}}
}
