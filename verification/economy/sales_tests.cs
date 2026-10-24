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
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh-sales-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(DataDirectory);try{
        Test("one unit from each of four buckets and exact separate revenue",()=>{foreach(int kind in new[]{-1,0,1,2}){var h=Fresh();long price=h.EstimatedUnitAcquisitionCost(kind);Check(h.TrySellPart(kind),"sale");Check(h._cash==price&&h._revenue==price&&h._partsSalesRevenue==price&&h._partsSalesUnitsSold==1&&h._customerRevenue==0,"ledger");Check((kind<0?h._parts:h._specialStock[kind])==9,"stock");}});
        Test("duplicate confirmation in same game frame cannot sell twice",()=>{var h=Fresh();Check(h.TrySellPart(-1)&&!h.TrySellPart(-1)&&!h.TrySellPart(0)&&h._parts==9&&h._specialStock[0]==10&&h.saves==1,"same frame");});
        Test("fresh manual action later can sell exactly one more",()=>{var h=Fresh();Check(h.TrySellPart(-1),"first");Game.GameTime++;Check(h.TrySellPart(-1)&&h._parts==8&&h._partsSalesUnitsSold==2,"next action");});
        Test("closed business and no manager still permit explicit active sale",()=>{var h=Fresh();h._clockedIn=false;h._managerTier=-1;Check(h.TrySellPart(-1),"closed manual");});
        Test("paused game cannot mutate ledger or write",()=>{var h=Fresh();Game.IsPaused=true;try{Check(!h.TrySellPart(-1)&&h._parts==10&&h._cash==0&&h.saves==0,"paused");}finally{Game.IsPaused=false;}});
        Test("startup hold cannot sell",()=>{var h=Fresh();h._bootGameplayActivated=false;Check(!h.TrySellPart(-1)&&h.saves==0,"boot");});
        Test("invalid type never indexes inventory",()=>{var h=Fresh();Check(!h.TrySellPart(-2)&&!h.TrySellPart(3)&&h.saves==0,"type");});
        Test("zero stock or negative legacy count cannot create a sale",()=>{foreach(int stock in new[]{0,-5}){var h=Fresh();h._parts=stock;Check(!h.TrySellPart(-1)&&h._cash==0&&h._parts==stock&&h.saves==0,"phantom");}});
        Test("dedicated physical repair reservation cannot be sold",()=>{var h=Fresh();h._parts=0;h._specialStock[1]=2;h.Worker();var o=h.Customer(1,2);h.AssignCustomerMechanic(o);Check(!h.TrySellPart(1)&&h._specialStock[1]==2,"physical");});
        Test("mixed dedicated and universal fallback reservations protect both buckets",()=>{var h=Fresh();h._parts=1;h._specialStock[1]=1;h.Worker();h.AssignCustomerMechanic(h.Customer(1,2));Check(!h.TrySellPart(-1),"universal");Game.GameTime++;Check(!h.TrySellPart(1),"dedicated");});
        Test("background repair reserves its last universal unit",()=>{var h=Fresh();h._parts=1;h.Worker().HasJob=true;Check(!h.TrySellPart(-1),"background");});
        Test("remote repair reserves dedicated stock once",()=>{var h=Fresh();h._parts=0;h._specialStock[2]=1;var m=h.Worker();RepairQuote q;h.TryCreateRepairQuote(2,1,m,120,500,0,out q);m.HasJob=true;h._remoteJobs.Add(new RemoteJob{MechanicSlot=0,ServiceKind=2,PartsNeeded=1,Pricing=q});Check(!h.TrySellPart(2)&&h._specialStock[2]==1,"remote");});
        Test("excess stock beyond repair reservation remains sellable",()=>{var h=Fresh();h._parts=2;h.Worker().HasJob=true;Check(h.TrySellPart(-1)&&h._parts==1,"excess");});
        Test("unassigned requests do not block available stock",()=>{var h=Fresh();h._parts=1;h.Customer(1,2);Check(h.TrySellPart(-1),"unassigned");});
        Test("paid returning mechanic does not reserve a second part",()=>{var h=Fresh();h._parts=1;h.Worker().HasJob=true;var o=h.Customer(0);o.Paid=true;o.MechanicSlot=0;Check(h.TrySellPart(-1),"paid");});
        Test("sale preserves supplier invoice and procurement/payroll costs",()=>{var h=Fresh();h.TrySellPart(-1);Check(h._supplierInvoicesDue==567&&h._partsSpend==31&&h._payrollSpend==32&&h._customerCompleted==0&&h._completed==0,"double expense");});
        Test("failed durable save rolls back inventory and every sales amount",()=>{var h=Fresh();h._cash=35;h._revenue=19;h._partsSalesRevenue=7;h._partsSalesUnitsSold=2;h.saveSuccess=false;Check(!h.TrySellPart(0)&&h._specialStock[0]==10&&h._cash==35&&h._revenue==19&&h._partsSalesRevenue==7&&h._partsSalesUnitsSold==2,"rollback");Check(!h.notices.Any(n=>n.StartsWith("Sold 1")),"false success");});
        Test("failed commit writes no sales CSV",()=>{var h=Fresh();h.saveSuccess=false;h.TrySellPart(-1);Check(!File.Exists(Path.Combine(DataDirectory,"PartsSales.v1.csv")),"failed audit");});
        Test("successful sale writes one versioned six-column audit row",()=>{var h=Fresh();h.TrySellPart(2);h.TrySellPart(2);var lines=File.ReadAllLines(Path.Combine(DataDirectory,"PartsSales.v1.csv"));Check(lines.Length==2&&lines.All(l=>l.Split(',').Length==6),"audit");});
        Test("monetary overflow leaves stock and cash unchanged",()=>{foreach(int field in new[]{0,1,2,3}){var h=Fresh();if(field==0)h._cash=long.MaxValue;if(field==1)h._revenue=long.MaxValue;if(field==2)h._partsSalesRevenue=long.MaxValue;if(field==3)h._partsSalesUnitsSold=long.MaxValue;Check(!h.TrySellPart(-1)&&h._parts==10&&h.saves==0,"overflow");}});
        Test("explicit zero basis is zero-safe without phantom money",()=>{var h=Fresh();h._unitCostEstimates[0]=0;Check(h.TrySellPart(-1)&&h._cash==0&&h._partsSalesUnitsSold==1&&h._parts==9,"zero");});
        Test("preview is pure and reports unreserved quantity and fixed estimate",()=>{var h=Fresh();h.Worker().HasJob=true;string text=h.PartsSalePreview(-1);Check(text.StartsWith("9/10 FREE")&&text.Contains("$125")&&h.saves==0&&h._parts==10,"preview");});
        Test("supplier and employee skills never alter fixed retail estimate",()=>{var h=Fresh();h._managerTier=2;h._receptionistHired=true;h.Worker(10,2,6000);Check(h.TrySellPart(2)&&h._cash==157,"markup");});
        Test("atomic save creates a new synthetic data file",()=>{string path=Path.Combine(DataDirectory,"Save.ini");Check(TryCommitBusinessSave(path,p=>File.WriteAllText(p,"Cash=125"),e=>{throw e;}),"new");Check(File.ReadAllText(path)=="Cash=125"&&Directory.GetFiles(DataDirectory).Length==1,"new contents");});
        Test("atomic replacement preserves unknown settings in copied snapshot",()=>{string path=Path.Combine(DataDirectory,"Save.ini");File.WriteAllText(path,"Unknown=keep\nCash=0");Check(TryCommitBusinessSave(path,p=>File.AppendAllText(p,"\nPartsSalesRevenue=125"),e=>{throw e;}),"replace");Check(File.ReadAllText(path).Contains("Unknown=keep")&&File.ReadAllText(path).Contains("PartsSalesRevenue=125"),"unknown");});
        Test("writer failure leaves prior save bytes identical and removes own temp",()=>{string path=Path.Combine(DataDirectory,"Save.ini");File.WriteAllText(path,"protected original");int errors=0;Check(!TryCommitBusinessSave(path,p=>{File.WriteAllText(p,"partial");throw new IOException("fault injection");},e=>errors++),"failure");Check(File.ReadAllText(path)=="protected original"&&errors==1&&Directory.GetFiles(DataDirectory).Length==1,"prior damage");});
        Test("missing snapshot is rejected without inventing a save",()=>{string path=Path.Combine(DataDirectory,"Save.ini");Check(!TryCommitBusinessSave(path,p=>{},e=>{})&&!File.Exists(path),"missing writer");});
        Test("commit destination failure cleans only owned temp file",()=>{string path=Path.Combine(DataDirectory,"directory-target");Directory.CreateDirectory(path);try{Check(!TryCommitBusinessSave(path,p=>File.WriteAllText(p,"snapshot"),e=>{})&&Directory.Exists(path),"target");Check(!Directory.GetFiles(DataDirectory).Any(p=>Path.GetFileName(p).StartsWith(".gh-save-")),"temp");}finally{Directory.Delete(path);}});
        Test("reload of a committed sale ledger cannot repeat it without a new manual action",()=>{var h=Fresh();Check(h.TrySellPart(-1),"commit");var reload=Fresh();reload._parts=h.savedParts;reload._cash=h.savedCash;reload._revenue=h.savedRevenue;Check(reload._parts==9&&reload._cash==125&&reload._revenue==125&&reload.saves==0,"loaded mutation");});
        Console.WriteLine("sales "+passed+"/"+passed+" PASS; production transaction and atomic file helper, synthetic data only.");
    }finally{Directory.Delete(DataDirectory,true);}}
}
