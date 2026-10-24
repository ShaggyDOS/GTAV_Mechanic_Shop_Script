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
 // Diagnostics-only sink adapter; existing gameplay assertions stay unchanged.
 long BeginCustomerVehicleTrace(CustomerOrder order,int operation)=>-1;
 void EndCustomerVehicleTrace(CustomerOrder order,long sequence){}
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
    static void Test(string name,Action action){foreach(var p in Directory.GetFiles(DataDirectory))File.Delete(p);action();passed++;Console.WriteLine("PASS inventory340 "+name);}

    int _deliveryKind=-2,_deliveryQuantity,_ownedStoragePallets=1,_reorderThreshold=3;
    int[] _deliveryLines;
    const int MaxStoragePallets=4,CratesPerPallet=9,PartsPerPallet=25;
    int[] _storedDeliveryCrates=new int[4];
    WorkBay _supplierPartsPallet=new WorkBay{IsSet=true};
    WorkBay[] _extraStoragePallets={new WorkBay(),new WorkBay(),new WorkBay()};
    void ShowStatusNotice(string title,params string[] rows){notices.Add(title+"\n"+string.Join("\n",rows));}
    /*<source:PendingSupplierInventory>*/
    /*<source:OnHandInventory>*/
    /*<source:InventoryAvailabilityRow>*/
    /*<source:InventoryStorageRows>*/
    /*<source:InventoryReorderSuggestion>*/
    /*<source:ShowInventoryCategoryDetail>*/
    /*<source:ShowDetailedInventory>*/
    /*<source:ShowReorderAdvice>*/
    /*<source:StoragePalletPoint>*/
    /*<source:TotalStorageCapacity>*/
    static void Main(){DataDirectory=Path.Combine(Path.GetTempPath(),"gh-inventory340-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(DataDirectory);try{

 Test("no pending order ignores stale quantities and mixed lines",()=>{var h=Fresh();h._deliveryQuantity=9;h._deliveryLines=new[]{1,2,3,4};Check(h.PendingSupplierInventory().SequenceEqual(new int[4]),"stale");});
 Test("legacy single-item incoming in each existing bucket",()=>{foreach(int k in new[]{-1,0,1,2}){var h=Fresh();h._deliveryKind=k;h._deliveryQuantity=7;var x=h.PendingSupplierInventory();Check(x[k+1]==7&&x.Sum()==7,"legacy");}});
 Test("mixed incoming is copied and cannot mutate pending order",()=>{var h=Fresh();h._deliveryKind=-1;h._deliveryLines=new[]{2,3,4,1};var x=h.PendingSupplierInventory();Check(x.SequenceEqual(h._deliveryLines),"lines");x[0]=999;Check(h._deliveryLines[0]==2,"alias");});
 Test("malformed arrays do not invent legacy incoming",()=>{var h=Fresh();h._deliveryKind=-1;h._deliveryQuantity=9;h._deliveryLines=new[]{3};Check(h.PendingSupplierInventory().Sum()==0,"invalid array");});
 Test("negative legacy quantities remain untouched and display zero",()=>{var h=Fresh();h._deliveryKind=0;h._deliveryQuantity=-5;Check(h.PendingSupplierInventory().Sum()==0&&h._deliveryQuantity==-5,"negative");});
 Test("negative mixed display quantities never mutate order",()=>{var h=Fresh();h._deliveryKind=0;h._deliveryLines=new[]{0,-4,2,0};Check(h.PendingSupplierInventory()[1]==0&&h._deliveryLines[1]==-4,"clamp");});
 Test("invalid legacy kind is display-safe",()=>{var h=Fresh();h._deliveryKind=99;h._deliveryQuantity=7;Check(h.PendingSupplierInventory().Sum()==0,"kind");});
 Test("negative stored values are display-only clamped",()=>{var h=Fresh();h._parts=-4;h._specialStock=new[]{-1,3,-2};Check(h.OnHandInventory().SequenceEqual(new[]{0,0,3,0})&&h._parts==-4&&h._specialStock[0]==-1,"read only");});
 Test("unassigned unaccepted job reserves nothing",()=>{var h=Fresh();h.Customer(1,4);Check(h.UnreservedRepairStock(null).SequenceEqual(h.OnHandInventory()),"not accepted");});
 Test("physical dedicated and universal promises shown separately",()=>{var h=Fresh();h._activeCustomerOrders.Add(new CustomerOrder{MechanicSlot=0,Pricing=new RepairQuote{ServiceKind=1,DedicatedUnits=3,UniversalUnits=2}});var f=h.UnreservedRepairStock(null);Check(f.SequenceEqual(new[]{8,10,7,10}),"reservation");Check(h.InventoryAvailabilityRow(0,h.OnHandInventory(),f,new int[4])=="Universal: 10 / 2 / 8 / 0","row");});
 Test("paid physical job no longer reserves inventory",()=>{var h=Fresh();h._activeCustomerOrders.Add(new CustomerOrder{MechanicSlot=0,Paid=true,Pricing=new RepairQuote{UniversalUnits=5}});Check(h.UnreservedRepairStock(null)[0]==10,"paid");});
 Test("remote reservation is visible",()=>{var h=Fresh();h._remoteJobs.Add(new RemoteJob{Pricing=new RepairQuote{ServiceKind=2,DedicatedUnits=4}});Check(h.UnreservedRepairStock(null)[3]==6,"remote");});
 Test("autonomous worker reservation is visible",()=>{var h=Fresh();h.Worker().HasJob=true;Check(h.UnreservedRepairStock(null)[0]==9,"background");});
 Test("restored accepted promise is visible without a physical worker",()=>{var h=Fresh();h._pendingRepairPromises.Add(new SavedRepairPromise{Quote=new RepairQuote{ServiceKind=0,DedicatedUnits=2}});Check(h.UnreservedRepairStock(null)[1]==8,"promise");});
 Test("shared promise is counted only once across sources",()=>{var h=Fresh();var q=new RepairQuote{ServiceKind=0,DedicatedUnits=2,UniversalUnits=1};h._activeCustomerOrders.Add(new CustomerOrder{RestoredPromise=true,MechanicSlot=-1,Pricing=q});h._pendingRepairPromises.Add(new SavedRepairPromise{Quote=q});Check(h.UnreservedRepairStock(null).SequenceEqual(new[]{9,8,10,10}),"dedup");});
 Test("incoming never becomes usable or changes reservations",()=>{var h=Fresh();h._parts=0;h._specialStock=new int[3];h._deliveryKind=-1;h._deliveryLines=new[]{10,10,10,10};h.ShowDetailedInventory();Check(h.UnreservedRepairStock(null).Sum()==0&&h._parts==0,"unreceived");});
 Test("reorder advice subtracts free and incoming but not reserved",()=>{var h=Fresh();h._reorderThreshold=10;Check(h.InventoryReorderSuggestion(1,new[]{0,2,0,0},new[]{0,3,0,0})==5,"target");});
 Test("universal target retains existing minimum five",()=>{var h=Fresh();Check(h.InventoryReorderSuggestion(0,new int[4],new int[4])==5&&h.InventoryReorderSuggestion(1,new int[4],new int[4])==3,"targets");});
 Test("incoming covers target without negative recommendation",()=>{var h=Fresh();Check(h.InventoryReorderSuggestion(0,new[]{1,0,0,0},new[]{9,0,0,0})==0,"covered");});
 Test("fully reserved on-hand stock can trigger advice",()=>{var h=Fresh();h._activeCustomerOrders.Add(new CustomerOrder{MechanicSlot=0,Pricing=new RepairQuote{UniversalUnits=10}});Check(h.InventoryReorderSuggestion(0,h.UnreservedRepairStock(null),new int[4])==5,"reserved");});
 Test("only owned placed pallets contribute units and parcel slots",()=>{var h=Fresh();h._ownedStoragePallets=2;h._extraStoragePallets[1].IsSet=true;var rows=h.InventoryStorageRows(new int[4],new int[4]);Check(rows[0].Contains("0/25")&&rows[2].Contains("9/9"),"placed");});
 Test("earned parcel slots can be full with units available",()=>{var h=Fresh();h._storedDeliveryCrates[0]=9;var rows=h.InventoryStorageRows(new[]{1,0,0,0},new int[4]);Check(rows[0].Contains("free 24")&&rows[2].Contains("0/9")&&rows[3].Contains("receipt must wait")&&h._storedDeliveryCrates[0]==9,"slots");});
 Test("unit headroom and incoming forecast are independent",()=>{var h=Fresh();var rows=h.InventoryStorageRows(new[]{20,0,0,0},new[]{6,0,0,0});Check(rows[1].Contains("projected free 0")&&rows[3].Contains("exceeds"),"forecast");});
 Test("over-capacity legacy stock is preserved",()=>{var h=Fresh();h._parts=99;h._specialStock=new int[3];var rows=h.InventoryStorageRows(h.OnHandInventory(),new int[4]);Check(rows[0].Contains("99/25")&&rows[3].Contains("preserved")&&h._parts==99,"old stock");});
 Test("wide display sums cannot wrap negative",()=>{var h=Fresh();var rows=h.InventoryStorageRows(Enumerable.Repeat(int.MaxValue,4).ToArray(),new int[4]);Check(rows[0].Contains("8589934588/25"),"overflow");});
 Test("category detail validates selection and labels estimates",()=>{var h=Fresh();h.ShowInventoryCategoryDetail(-1);h.ShowInventoryCategoryDetail(4);Check(h.notices.Count==0,"invalid selection");h.ShowInventoryCategoryDetail(2);Check(h.notices.Single().Contains("Estimated cost $200/unit")&&h.notices[0].Contains("cannot be consumed or sold"),"cost");});
 Test("all four detail views show their own stock values",()=>{var h=Fresh();h._specialStock=new[]{2,3,4};for(int i=0;i<4;i++)h.ShowInventoryCategoryDetail(i);Check(h.notices.Count==4&&h.notices[1].Contains("On hand 2")&&h.notices[3].Contains("On hand 4"),"buckets");});
 Test("browsing repeatedly never saves or mutates cash stock invoice or timer",()=>{var h=Fresh();h._cash=123;h._deliveryKind=-1;h._deliveryQuantity=7;for(int j=0;j<5;j++){h.ShowDetailedInventory();h.ShowReorderAdvice();for(int i=0;i<4;i++)h.ShowInventoryCategoryDetail(i);}Check(h.saves==0&&h._cash==123&&h._parts==10&&h._specialStock.SequenceEqual(new[]{10,10,10})&&h._supplierInvoicesDue==567&&h._deliveryQuantity==7&&h._deliveryKind==-1,"browse mutations");});
 Test("paused snapshots are read-only",()=>{var h=Fresh();Game.IsPaused=true;try{h.ShowDetailedInventory();h.ShowReorderAdvice();Check(h.saves==0&&h._parts==10&&h._cash==0,"pause");}finally{Game.IsPaused=false;}});
 Test("low-stock advice is not a purchase or a second reservation",()=>{var h=Fresh();h._parts=0;h.ShowReorderAdvice();Check(h.notices[0].Contains("NOT A PURCHASE")&&h._deliveryKind==-2&&h.saves==0&&h.UnreservedRepairStock(null)[0]==0,"advice");});
 Console.WriteLine("inventory340 "+passed+"/"+passed+" PASS; production inventory and reservation projections, native rendering NOT RUN.");
 }finally{Directory.Delete(DataDirectory,true);}}
}
