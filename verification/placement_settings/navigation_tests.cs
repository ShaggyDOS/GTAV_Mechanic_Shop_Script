using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
class ScriptSettings {
 internal Dictionary<string,string> values=new Dictionary<string,string>();string file;
 internal T GetValue<T>(string section,string key,T fallback){return values.TryGetValue(section+"|"+key,out string value)?(T)Convert.ChangeType(value,typeof(T),System.Globalization.CultureInfo.InvariantCulture):fallback;}
 internal void SetValue<T>(string section,string key,T value){values[section+"|"+key]=Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture);}
 internal static ScriptSettings Load(string path){var d=new ScriptSettings{file=path};string section="";if(File.Exists(path))foreach(string line in File.ReadAllLines(path)){if(line.StartsWith("[")&&line.EndsWith("]"))section=line.Substring(1,line.Length-2);else{int eq=line.IndexOf('=');if(eq>=0)d.values[section+"|"+line.Substring(0,eq)]=line.Substring(eq+1);}}return d;}
 internal bool Save(){var lines=new List<string>();foreach(var group in values.GroupBy(k=>k.Key.Split('|')[0])){lines.Add("["+group.Key+"]");foreach(var kv in group)lines.Add(kv.Key.Split('|')[1]+"="+kv.Value);}File.WriteAllLines(file,lines);return true;}
}
namespace GTA.UI {static class Screen {internal static void ShowSubtitle(string s,int ms){}}}
namespace GTA { enum Control { OpenModifier, OpenButton, Up, Down, Accept, Back, FrontendLeft, FrontendRight, FrontendPause, FrontendPauseAlternate } }
static class Game { internal static bool IsPaused; }
class Harness {
    enum Keys { None, Up, Down, Left, Right, Enter, Return=Enter, Escape, Back, F2, E }
    class KeyEventArgs { internal Keys KeyCode; internal bool Control; internal KeyEventArgs(Keys k) { KeyCode=k; } }
/*<source:MenuInputGate>*/
/*<source:MenuActionForKey>*/
/*<source:HideBusinessMenuForPause>*/
/*<source:YieldMenuToNativePause>*/
/*<source:OnKeyDown>*/
/*<source:OnKeyUp>*/
/*<source:PollController>*/
    bool _nativePauseYielded;
    bool _bootGameplayActivated=true, _controllerEnabled=true, _requireCtrlForMenu;







    Keys _menuKey=Keys.F2;
    readonly HashSet<Keys> _heldKeys=new HashSet<Keys>();
    readonly MenuInputGate _menuInput=new MenuInputGate();
    DateTime _lastControllerAction=DateTime.MinValue, _lastPadDetectedAt, _lastPadReadErrorAt=DateTime.MinValue;
    string _lastPadDetected="NONE";
    GTA.Control _controllerOpenModifier=GTA.Control.OpenModifier, _controllerOpenButton=GTA.Control.OpenButton;
    GTA.Control _controllerBack=GTA.Control.Back, _controllerUp=GTA.Control.Up, _controllerDown=GTA.Control.Down, _controllerAccept=GTA.Control.Accept;
    readonly HashSet<GTA.Control> edges=new HashSet<GTA.Control>();
    int selections;
    bool PadJustPressed(GTA.Control c) { return edges.Contains(c); }
    bool PadPressed(GTA.Control c) { return edges.Contains(c); }
    bool TrySupplierPackageInteraction() { return false; }
    bool IsPlayerAtOfficeDesk() { return false; }
    void OpenBusinessFromOffice() { }

    int[] _partsSaleCart=new int[4];
    /*<source:AdjustPartsSaleCart>*/
    void AdjustSupplierCart(int direction){}

    static void Log(string c, Exception e) { throw new Exception(c,e); }
    void Key(Keys k) { OnKeyDown(null,new KeyEventArgs(k)); }
    void Up(Keys k) { OnKeyUp(null,new KeyEventArgs(k)); }
    void Pad(GTA.Control c) { edges.Clear(); edges.Add(c); _lastControllerAction=DateTime.MinValue; PollController(); edges.Clear(); }

/*<source:MenuPage>*/
/*<source:ExecuteManagementSelection>*/
/*<source:ExecuteFinancesSelection>*/
/*<source:ExecuteBusinessExpensesSelection>*/
/*<source:NavigateBack>*/
/*<source:OpenPlacementMenu>*/
/*<source:ExecutePlacementSelection>*/
/*<source:ExecuteCustomerSelection>*/
/*<source:ExecuteEmployeeSettingsSelection>*/
bool _breakAnimationsEnabled,_customerServiceEnabled,_customerBlipsEnabled,_waitingForDropoff;int _waitingCustomerRequests;DateTime _nextCustomerAt;
class CustomerOrder{}List<CustomerOrder> _activeCustomerOrders=new List<CustomerOrder>();CustomerOrder _stagedCustomerOrder,_stagedThirdCustomerOrder;
void ShowCustomerOrderStatus(){}bool CustomerConfigurationReady()=>true;void CancelAllCustomerOrders(string reason){}void ClearStagedCustomers(){}void ScheduleCustomerRequest(){}void SyncCustomerBlip(CustomerOrder o){}

/*<source:OpenPointEditor>*/
/*<source:SavePoint>*/
/*<source:LoadPoint>*/
/*<source:ExecuteCustomerPlacementsSelection>*/
/*<source:ExecuteEmployeePlacementsSelection>*/
/*<source:ExecutePointListSelection>*/
/*<source:ExecuteCustomerWalkingRouteSelection>*/
/*<source:ExecuteSupplierDriverWaypoints>*/
/*<source:ExecuteSupplierPalletStorage>*/
/*<source:OpenServiceBayProfile>*/
/*<source:ExecuteServiceBayProfile>*/
/*<source:ExecuteMarkerSelection>*/
/*<source:ExecuteRouteRosterSelection>*/
/*<source:ExecuteRouteDirectionSelection>*/
/*<source:ExecuteRoutePointSelection>*/
/*<source:CountMask>*/
/*<source:ExecutePointEditorSelection>*/
/*<source:EditingVehicleParkingSlot>*/
readonly Dictionary<MenuPage,KeyValuePair<MenuPage,int>> _placementMenuReturns=new Dictionary<MenuPage,KeyValuePair<MenuPage,int>>();
class WorkBay{internal bool IsSet;internal Vector3 Position;internal float Heading;}
struct Vector3{internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
const int VisualBayCount=3,OptionalPathPointCount=5,MaxStoragePallets=4,ExtraPalletPrice=750;
WorkBay _editingPoint;string _editingPointLabel;int _editingBayIndex;bool _editingRoutePoint,_showPlacementMarkers,_placed=true,_clockedIn;
WorkBay _customerDropoff=new WorkBay(),_customerDriveSpawn=new WorkBay(),_customerApproach=new WorkBay(),_customerQueue=new WorkBay(),_queueCar2=new WorkBay(),_queueCar3=new WorkBay(),_breakArea=new WorkBay(),_receptionDesk=new WorkBay(),_receptionApproach=new WorkBay(),_managerWorkstation=new WorkBay(),_managerApproach=new WorkBay(),_employeeEntrance=new WorkBay(),_supplierTruckSpawn=new WorkBay(),_supplierTruckPark=new WorkBay(),_supplierHandoff=new WorkBay(),_walkCheckpoint=new WorkBay(),_supplierPartsPallet=new WorkBay();
static WorkBay[] Points(int n)=>Enumerable.Range(0,n).Select(i=>new WorkBay()).ToArray();
WorkBay[] _bays=Points(3),_vehicleParkingBays=Points(3),_arrivalRoute=Points(5),_serviceRoute=Points(5),_bayApproaches=Points(3),_receptionRoute=Points(5),_managerRoute=Points(5),_customerWalkingRoute=Points(3),_supplierDriverWaypoints=Points(3),_extraStoragePallets=Points(3);
int _ownedStoragePallets=4;long _cash=10000,_equipmentSpend;int[] _storedDeliveryCrates=new int[4];object _customerOrder;
bool _customEmployeeWalkingPaths;bool _automaticLotMode,_vehicleTransfersEnabled;bool _customVehicleParkingBays=true;List<Mechanic> _mechanics=new List<Mechanic>();class Mechanic{internal int JobOutboundMask=31,JobReturnMask=31;internal string Name="worker";internal bool HasJob;internal int WorkMode;internal double WorkSeconds,IdleSeconds,BreakAnimationSeconds;}
void Notify(string message){notices++;}void ShowStatusNotice(string title,params string[] lines){notices++;}
string Status(WorkBay p)=>p.IsSet?"SET":"NOT SET";string FormatPosition(Vector3 p)=>p.X+", "+p.Y+", "+p.Z;
int StoredPartsTotal()=>0;int TotalStorageCapacity()=>100;void DeleteStoragePalletVisuals(){}void DismissReceptionist(){}void DismissWorkerForSlot(int slot){}
WorkBay StoragePalletPoint(int slot)=>slot==0?_supplierPartsPallet:_extraStoragePallets[slot-1];
bool Save(){saves++;return true;}
// Existing capture/save functions are adapters here; production bodies are separately pinned unchanged.
Vector3 capture=new Vector3(11,22,33);float facing=123;
void Configure(WorkBay p){p.Position=capture;p.Heading=facing;p.IsSet=true;Save();}
void ConfigureVehicleParkingBay(int slot){Configure(_vehicleParkingBays[slot]);}void ConfigureWorkBay(int slot){Configure(_bays[slot]);}
void ConfigureRoutePoint(WorkBay p,string label){Configure(p);}void ConfigureArrivalPoint(WorkBay p,string label){Configure(p);}
void ExecuteSelection(){switch(_menuPage){
/*<cases:ExecuteSelection:BusinessManagement,PlacementSettings,DeliveryPlacements,PlacementWalkingRoutes,MechanicPlacementRoutes,ReceptionPlacementRoutes,ManagerPlacementRoutes,CustomerPlacementRoutes,SupplierPlacementRoutes,Customers,Employees,EmployeeStatus,OwnerTransferReview,PartsSales,PartsSaleReview,EmployeeSettings,EmployeePlacements,CustomerPlacements,CustomerWalkingRoutes,ServiceBayProfile,MechanicBays,ServiceVehicleBays,ArrivalWaypoints,ReceptionWaypoints,ManagerWaypoints,ServiceWaypoints,BayApproaches,PointEditor,PlacementMarkers,SupplierPalletStorage,SupplierDriverWaypoints,MechanicRouteRoster,MechanicRouteDirection,MechanicRoutePoints>*/
default:throw new Exception("unadapted dispatch "+_menuPage);}}

int _selection,_serviceBaySlot,_editingStaffSlot,_editorReturnSelection,_routeConfigSlot;
MenuPage _menuPage,_serviceBayParent,_editorReturnPage;
bool _showMenu,_managerPathsFromManagement,_mechanicPathsFromManagement,_receptionPathsFromManagement,_customCarBaysFromDriveOptions,_financesFromExpenses,_mechanicRosterFromManagement,_routeConfigReturning,_financeLifetime;
int _financeHistoryPage;void BuyInventory(int kind,int quantity){payments++;}
int saves,transfers,shop,payments,notices; bool deposit;long _receptionWagesDue,_rentUtilitiesDue,_managerWagesDue;
void TransferPlayerMoney(bool d){transfers++;deposit=d;}void SetShopLocationFromPlayer(){shop++;}
void ShowExpenseStatus(){notices++;}void PayInvoice(ref long d,bool wage,string label){payments++;}void TryPayBusinessBills(int mode){payments++;}
/*<source:GetMenuItems>*/
/*<source:GetMenuTitle>*/
string[] FinanceHistoryItems()=>new[]{"#1 test history","Older Transactions >","Newer Transactions >","< Back to Accounting"};
string GetMenuItemValue(int index){switch(_menuPage){
/*<cases:GetMenuItemValue:BusinessManagement,OwnerTransferReview,PartsSales,PartsSaleReview,EmployeeStatus,MechanicRoutePoints>*/
default:return ">";}}
/*<source:GetSelectedDescription>*/
/*<source:GetPageDescription>*/
/*<source:FooterRow>*/
/*<source:ExecutePartsSalesSelection>*/
/*<source:PartsSaleTotal>*/
/*<source:PartsSaleTotalText>*/
/*<source:ExecuteEmployeeSelection>*/
void ExecuteFinancePageForTest(){
/*<owner-navigation>*/
}
int _ownerTransferAmount=1000;bool _ownerTransferDeposit;int[] _partsSaleReview=new int[4];string[] _employeeStatusRows={"SNAPSHOT","SNAPSHOT","SNAPSHOT","SNAPSHOT","SNAPSHOT"};
string DeliveryItemName(int kind)=>new[]{"Universal","Filter","Brakes","Diagnostics"}[kind+1];
long EstimatedUnitAcquisitionCost(int kind)=>100;
int[] UnreservedRepairStock(object q)=>new[]{10,10,10,10};
bool TrySellParts(int[] q){payments++;return true;}
void RefreshEmployeeStatus(){}void ExecuteEmployeeStatusSelection(){if(_selection==6)NavigateBack();}
string PartsSalePreview(int kind)=>"10 FREE | $100";
string FinancePeriodLabel()=>"CURRENT SHIFT";
string EditingPointPurpose()=>"Edit existing point.";string MenuKeyLabel()=>"F2";
static int passed;static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);}static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS "+n);}
static void Main(){
Test("one finance path; expenses and equipment retained",()=>{var h=new Harness{_menuPage=MenuPage.BusinessManagement};Check(!h.GetMenuItems().Contains("Expenses & Bills >"),"duplicate");h._selection=3;h.ExecuteManagementSelection();Check(h._menuPage==MenuPage.Finances,"finance");h._selection=2;h.ExecuteFinancesSelection();Check(h._menuPage==MenuPage.BusinessExpenses,"expenses");h._selection=7;h.ExecuteBusinessExpensesSelection();Check(h._menuPage==MenuPage.EquipmentManagement,"equipment");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessExpenses&&h._selection==7,"equipment back");h.NavigateBack();Check(h._menuPage==MenuPage.Finances&&h._selection==2,"expenses back");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==3,"finance back");Check(h.transfers+h.shop+h.payments+h.saves==0,"browse side effects");});
for(int i=0;i<7;i++){int row=i;Test("management routing "+row,()=>{var h=new Harness{_menuPage=MenuPage.BusinessManagement,_selection=row};Check(h.GetSelectedDescription().Length>0,"description");Check(h.GetMenuItemValue(row).Length>0,"value");h.ExecuteManagementSelection();MenuPage[] targets={MenuPage.Employees,MenuPage.Customers,MenuPage.InventoryManagement,MenuPage.Finances,MenuPage.PlacementSettings};if(row<5){Check(h._menuPage==targets[row]&&h._selection==0,"route");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==row,"return row");}else if(row==5)Check(h.shop==1,"shop");else Check(h._menuPage==MenuPage.Business,"back");});}
MoreTests();
Console.WriteLine($"placement {passed}/{passed} PASS; production menu routing, GTA NOT RUN");
}
static Harness Hub(){var h=new Harness{_menuPage=MenuPage.BusinessManagement,_selection=4,_showMenu=true};h.ExecuteManagementSelection();return h;}
static void EditorRoundtrip(Harness h,WorkBay p,MenuPage parent,int row){Check(h._menuPage==MenuPage.PointEditor&&Object.ReferenceEquals(h._editingPoint,p),"point identity");Check(h.saves+h.payments+h.transfers==0,"browse write");h.NavigateBack();Check(h._menuPage==parent&&h._selection==row,"editor return");}
static MenuPage RouteRole(int route)=>route<3?MenuPage.MechanicPlacementRoutes:route==3?MenuPage.ReceptionPlacementRoutes:route==4?MenuPage.ManagerPlacementRoutes:route==5?MenuPage.CustomerPlacementRoutes:MenuPage.SupplierPlacementRoutes;
static int RouteRow(int route)=>route<3?route:0;
static void EnterRoute(Harness h,int route){h._selection=route<3?0:route-2;h.ExecuteSelection();h._selection=RouteRow(route);h.ExecuteSelection();}
static void MoreTests(){
MenuPage[] categories={MenuPage.CustomerPlacements,MenuPage.EmployeePlacements,MenuPage.MechanicBays,MenuPage.DeliveryPlacements,MenuPage.PlacementWalkingRoutes,MenuPage.PlacementMarkers};
for(int i=0;i<categories.Length;i++){int row=i;Test("hub category + explicit Back "+row,()=>{var h=Hub();Check(h.GetMenuItems().Length==7,"hub size");h._selection=row;h.ExecuteSelection();Check(h._menuPage==categories[row],"category");h._selection=h.GetMenuItems().Length-1;h.ExecuteSelection();Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==row,"explicit back");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==4&&h._placementMenuReturns.Count==0,"hub back/context");Check(h.saves+h.payments+h.transfers==0,"browse side effect");});}
for(int i=0;i<6;i++){int row=i;Test("customer point "+row,()=>{var h=Hub();h._selection=0;h.ExecuteSelection();var points=new[]{h._customerDropoff,h._customerDriveSpawn,h._customerApproach,h._customerQueue,h._queueCar2,h._queueCar3};h._selection=row;h.ExecuteSelection();EditorRoundtrip(h,points[row],MenuPage.CustomerPlacements,row);h.NavigateBack();Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==0,"category back");});}
for(int i=0;i<7;i++){int row=i;if(row==1)continue;Test("employee point "+row,()=>{var h=Hub();h._selection=1;h.ExecuteSelection();var points=new[]{h._breakArea,null,h._receptionDesk,h._receptionApproach,h._managerWorkstation,h._managerApproach,h._employeeEntrance};h._selection=row;h.ExecuteSelection();EditorRoundtrip(h,points[row],MenuPage.EmployeePlacements,row);h.NavigateBack();Check(h._selection==1&&h._menuPage==MenuPage.PlacementSettings,"employee back");});}
for(int bay=0;bay<3;bay++)for(int point=0;point<2;point++){int b=bay,p=point;Test("service bay separate point "+b+"/"+p,()=>{var h=Hub();h._selection=2;h.ExecuteSelection();h._selection=b;h.ExecuteSelection();Check(h._serviceBaySlot==b,"slot");h._selection=p;h.ExecuteSelection();EditorRoundtrip(h,p==0?h._vehicleParkingBays[b]:h._bays[b],MenuPage.ServiceBayProfile,p);Check(h._editingBayIndex==(p==1?b:-1),"work slot");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicBays&&h._selection==b,"profile back");h.NavigateBack();Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==2,"bay back");});}
for(int i=0;i<3;i++){int row=i;Test("supplier point "+row,()=>{var h=Hub();h._selection=3;h.ExecuteSelection();var points=new[]{h._supplierTruckSpawn,h._supplierTruckPark,h._supplierHandoff};h._selection=row;h.ExecuteSelection();EditorRoundtrip(h,points[row],MenuPage.DeliveryPlacements,row);});}
for(int i=0;i<4;i++){int slot=i;Test("pallet point + context "+slot,()=>{var h=Hub();h._selection=3;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();h._selection=slot+2;h.ExecuteSelection();EditorRoundtrip(h,h.StoragePalletPoint(slot),MenuPage.SupplierPalletStorage,slot+2);h.NavigateBack();Check(h._menuPage==MenuPage.DeliveryPlacements&&h._selection==3,"pallet back");Check(h._cash==10000,"no purchase");});}
MenuPage[] routePages={MenuPage.ArrivalWaypoints,MenuPage.ServiceWaypoints,MenuPage.BayApproaches,MenuPage.ReceptionWaypoints,MenuPage.ManagerWaypoints,MenuPage.CustomerWalkingRoutes,MenuPage.SupplierDriverWaypoints};
for(int i=0;i<routePages.Length;i++){int route=i;int count=i==0||i==1||i==3||i==4?5:3;for(int j=0;j<count;j++){int stop=j;Test("route stop + contextual Back "+route+"/"+stop,()=>{var h=Hub();h._selection=4;h.ExecuteSelection();EnterRoute(h,route);Check(h._menuPage==routePages[route],"route page");WorkBay[][] points={h._arrivalRoute,h._serviceRoute,h._bayApproaches,h._receptionRoute,h._managerRoute,h._customerWalkingRoute,h._supplierDriverWaypoints};h._selection=stop;h.ExecuteSelection();EditorRoundtrip(h,points[route][stop],routePages[route],stop);h.NavigateBack();Check(h._menuPage==RouteRole(route)&&h._selection==RouteRow(route),"route back");h.NavigateBack();Check(h._menuPage==MenuPage.PlacementWalkingRoutes,"role back");h.NavigateBack();Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==4,"route category back");});}}
Test("route assignment and legacy checkpoint",()=>{var h=Hub();h._selection=4;h.ExecuteSelection();h._selection=0;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();Check(h._menuPage==MenuPage.MechanicRouteRoster,"job selector");h.NavigateBack();Check(h._selection==3,"selector back");h._selection=4;h.ExecuteSelection();EditorRoundtrip(h,h._walkCheckpoint,MenuPage.MechanicPlacementRoutes,4);});
Test("equipment link returns to delivery hub",()=>{var h=Hub();h._selection=3;h.ExecuteSelection();h._selection=5;h.ExecuteSelection();Check(h._menuPage==MenuPage.EquipmentManagement,"equipment hub link");h.NavigateBack();Check(h._menuPage==MenuPage.DeliveryPlacements&&h._selection==5&&h.payments+h.saves==0,"equipment context");});
Test("supplier stops from delivery category",()=>{var h=Hub();h._selection=3;h.ExecuteSelection();h._selection=4;h.ExecuteSelection();h._selection=2;h.ExecuteSelection();EditorRoundtrip(h,h._supplierDriverWaypoints[2],MenuPage.SupplierDriverWaypoints,2);h.NavigateBack();Check(h._menuPage==MenuPage.DeliveryPlacements&&h._selection==4,"driver context");});
Test("markers toggle + legend never save or place",()=>{var h=Hub();h._selection=5;h.ExecuteSelection();h._selection=0;h.ExecuteSelection();Check(h._showPlacementMarkers,"on");h.ExecuteSelection();Check(!h._showPlacementMarkers,"off");h._selection=1;h.ExecuteSelection();Check(h.notices==3&&h.saves==0&&!h._customerDropoff.IsSet,"legend");});
Test("existing editor save + facing dispatch and return",()=>{var h=Hub();h._selection=0;h.ExecuteSelection();h.ExecuteSelection();h.ExecuteSelection();Check(h._customerDropoff.IsSet&&h._customerDropoff.Position.X==11&&h._customerDropoff.Heading==123&&h.saves==1,"save dispatch");h._selection=3;h.ExecuteSelection();Check(h.saves==1,"coordinate view");h._selection=4;h.ExecuteSelection();Check(h._menuPage==MenuPage.CustomerPlacements&&h._selection==0,"save back");});
Test("stale hub context cleared on fresh management selection",()=>{var h=Hub();h._selection=0;h.ExecuteSelection();h._menuPage=MenuPage.BusinessManagement;h._selection=1;h.ExecuteManagementSelection();h._menuPage=MenuPage.CustomerPlacements;h.NavigateBack();Check(h._menuPage==MenuPage.Customers&&h._selection==2&&h._placementMenuReturns.Count==0,"legacy return");});
Test("keyboard Enter repeat, Backspace and F2 hub navigation",()=>{var h=new Harness{_bootGameplayActivated=true};h.Key(Keys.F2);h.Up(Keys.F2);Check(h._showMenu&&h._menuPage==MenuPage.Business,"F2");h._menuPage=MenuPage.BusinessManagement;h._selection=4;h.Key(Keys.Enter);h.Key(Keys.Enter);Check(h._menuPage==MenuPage.PlacementSettings,"repeat entered child");h.Up(Keys.Enter);h.Key(Keys.Down);h.Up(Keys.Down);Check(h._selection==1,"down");h.Key(Keys.Enter);h.Up(Keys.Enter);Check(h._menuPage==MenuPage.EmployeePlacements,"keyboard child");h.Key(Keys.Back);h.Up(Keys.Back);Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==1,"keyboard back");});
Test("controller accept/down/back with real hub dispatcher",()=>{var h=Hub();h.Pad(GTA.Control.Down);Check(h._selection==1,"pad down");h.Pad(GTA.Control.Accept);Check(h._menuPage==MenuPage.EmployeePlacements,"pad accept");h.Pad(GTA.Control.Back);Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==1,"pad back");h.Pad(GTA.Control.Back);Check(h._menuPage==MenuPage.BusinessManagement&&h._selection==4,"pad management");});
for(int i=0;i<2;i++){int alias=i;Test("old placement entry redirects to central hub "+alias,()=>{var h=new Harness{_menuPage=alias==0?MenuPage.Customers:MenuPage.EmployeeSettings,_selection=alias==0?2:0};var parent=h._menuPage;int row=h._selection;Check(h.GetMenuItems()[row]=="Placement Settings >","alias caption");h.ExecuteSelection();Check(h._menuPage==MenuPage.PlacementSettings,"alias hub");h.NavigateBack();Check(h._menuPage==parent&&h._selection==row,"alias back");Check(h.saves+h.payments+h.transfers==0,"alias side effects");});}
Test("nested customer routes/bays and employee bay shortcut",()=>{var h=Hub();h._selection=0;h.ExecuteSelection();h._selection=7;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();Check(h._menuPage==MenuPage.CustomerPlacements&&h._selection==7,"nested route back");h._selection=6;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();Check(h._menuPage==MenuPage.CustomerPlacements&&h._selection==6,"nested vehicle bays back");h.NavigateBack();h._selection=1;h.ExecuteSelection();h._selection=1;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();Check(h._menuPage==MenuPage.EmployeePlacements&&h._selection==1,"nested work bays back");h.NavigateBack();Check(h._menuPage==MenuPage.PlacementSettings&&h._selection==1,"nested category back");});
Test("all route list Back buttons restore routes selection",()=>{for(int route=0;route<7;route++){var h=Hub();h._selection=4;h.ExecuteSelection();EnterRoute(h,route);h._selection=h.GetMenuItems().Length-1;h.ExecuteSelection();Check(h._menuPage==RouteRole(route)&&h._selection==RouteRow(route),"route list back "+route);}});
Test("unowned pallet cannot enter editor or spend",()=>{var h=Hub();h._ownedStoragePallets=1;h._selection=3;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();h._selection=5;h.ExecuteSelection();Check(h._menuPage==MenuPage.SupplierPalletStorage&&h._cash==10000&&h.saves==0,"unowned pallet");});
Test("service bay info and assignment are read only",()=>{var h=Hub();h._selection=2;h.ExecuteSelection();h._selection=1;h.ExecuteSelection();h._selection=2;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();Check(h.saves+h.transfers+h.payments==0&&!h._bays[1].IsSet&&!h._vehicleParkingBays[1].IsSet,"profile info");});
Test("keyboard/controller row wrapping at hub",()=>{var h=Hub();h.Key(Keys.Up);h.Up(Keys.Up);Check(h._selection==6,"key up wrap");h.Key(Keys.Down);h.Up(Keys.Down);Check(h._selection==0,"key down wrap");h=new Harness{_menuPage=MenuPage.BusinessManagement,_selection=4,_showMenu=true};h.ExecuteManagementSelection();h.Pad(GTA.Control.Up);Check(h._selection==6,"pad up wrap");h.Pad(GTA.Control.Down);Check(h._selection==0,"pad down wrap");});
Test("all location families preserve set flags xyz headings through disk reload",()=>{var sections=new List<string>{"EmployeeEntrance","WalkCheckpoint","EmployeeBreakArea","ReceptionDesk","ReceptionApproach","ManagerWorkstation","ManagerApproach","CustomerDriveSpawn","CustomerDropoff","CustomerApproach","CustomerQueue","CustomerQueueCar2","CustomerQueueCar3","SupplierTruckSpawn","SupplierTruckPark","SupplierDriverHandoff","SupplierPartsPallet","OfficeDesk","ToolSetPlacement","ScannerPlacement","SupplierWarehousePickup"};foreach(string family in new[]{"WorkBay","VehicleParkingBay","BayApproach","CustomerWalkingPoint","SupplierDriverWaypoint"})for(int i=1;i<=3;i++)sections.Add(family+i);foreach(string family in new[]{"ArrivalWaypoint","ServiceWaypoint","ReceptionWaypoint","ManagerWaypoint"})for(int i=1;i<=5;i++)sections.Add(family+i);for(int i=2;i<=4;i++)sections.Add("SupplierStoragePallet"+i);string dir=Path.Combine(Path.GetTempPath(),"gh-issue30-save-"+Guid.NewGuid());Directory.CreateDirectory(dir);try{string file=Path.Combine(dir,"Save.ini");var data=ScriptSettings.Load(file);data.SetValue("Business","Cash",123456);for(int i=0;i<sections.Count;i++){var point=new WorkBay{IsSet=i%2==0,Position=new Vector3(i+.25f,-i-.5f,2.75f+i),Heading=i*5.5f};SavePoint(data,sections[i],point);}Check(data.Save(),"disk save");var reload=ScriptSettings.Load(file);for(int i=0;i<sections.Count;i++){var p=new WorkBay();LoadPoint(reload,sections[i],p);Check(p.IsSet==(i%2==0)&&p.Position.X==i+.25f&&p.Position.Y==-i-.5f&&p.Position.Z==2.75f+i&&p.Heading==i*5.5f,"roundtrip "+sections[i]);}Check(reload.GetValue("Business","Cash",0)==123456,"unrelated cash sentinel");}finally{Directory.Delete(dir,true);}});
Test("missing point keeps existing unset defaults",()=>{string file=Path.Combine(Path.GetTempPath(),"missing-gh-"+Guid.NewGuid()+".ini");var p=new WorkBay{IsSet=true,Heading=50};LoadPoint(ScriptSettings.Load(file),"CustomerDropoff",p);Check(!p.IsSet&&p.Position.X==0&&p.Position.Y==0&&p.Position.Z==0&&p.Heading==0,"defaults");});
Test("display rename preserves EmployeeBreakArea key",()=>{var h=Hub();h._selection=1;h.ExecuteSelection();Check(h.GetMenuItems()[0]=="Mechanic Break Area >","break label");h.ExecuteSelection();Check(h._editingPointLabel=="Mechanic Break Area"&&Object.ReferenceEquals(h._editingPoint,h._breakArea),"editor target");Check(h.saves==0,"no coordinate reset");});
for(int role=0;role<5;role++){int slot=role;Test("role group Back and controller "+slot,()=>{var h=Hub();h._selection=4;h.ExecuteSelection();h._selection=slot;h.Pad(GTA.Control.Accept);MenuPage[] pages={MenuPage.MechanicPlacementRoutes,MenuPage.ReceptionPlacementRoutes,MenuPage.ManagerPlacementRoutes,MenuPage.CustomerPlacementRoutes,MenuPage.SupplierPlacementRoutes};Check(h._menuPage==pages[slot],"role entry");h._selection=h.GetMenuItems().Length-1;h.ExecuteSelection();Check(h._menuPage==MenuPage.PlacementWalkingRoutes&&h._selection==slot,"role Back");});}
for(int role=1;role<5;role++){int slot=role;Test("role approach roundtrip "+slot,()=>{var h=Hub();h._selection=4;h.ExecuteSelection();h._selection=slot;h.ExecuteSelection();var parent=h._menuPage;var points=new[]{h._receptionApproach,h._managerApproach,h._customerApproach,h._supplierHandoff};h._selection=1;h.ExecuteSelection();EditorRoundtrip(h,points[slot-1],parent,1);});}
Test("mechanic masks independent; shared physical route untouched",()=>{var h=Hub();h._mechanics.AddRange(new[]{new Mechanic(),new Mechanic(),new Mechanic()});var shared=h._serviceRoute;h._selection=4;h.ExecuteSelection();h._selection=0;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();h._selection=1;h.ExecuteSelection();h._selection=0;h.ExecuteSelection();h._selection=2;h.ExecuteSelection();Check(h._mechanics[1].JobOutboundMask==27&&h._mechanics[1].JobReturnMask==31&&h._mechanics[0].JobOutboundMask==31&&h._mechanics[2].JobOutboundMask==31,"masks");Check(Object.ReferenceEquals(shared,h._serviceRoute)&&shared.Length==5&&shared.All(x=>!x.IsSet),"shared 5 points");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicRouteDirection&&h._selection==0,"mask Back");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicRouteRoster&&h._selection==1,"staff Back");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicPlacementRoutes&&h._selection==3,"mechanic role Back");});
Test("expenses reports Back restores expenses row",()=>{var h=new Harness{_menuPage=MenuPage.BusinessExpenses,_selection=8};h.ExecuteBusinessExpensesSelection();Check(h._menuPage==MenuPage.Finances,"reports");h.NavigateBack();Check(h._menuPage==MenuPage.BusinessExpenses&&h._selection==8&&!h._financesFromExpenses,"reports return");});
foreach(MenuPage page in Enum.GetValues(typeof(MenuPage))){var menu=page;Test("all-menu shape, keyboard wrap and Back "+menu,()=>{var h=new Harness{_menuPage=menu,_showMenu=true,_editingPointLabel="TEST POINT"};var items=h.GetMenuItems();Check(items.Length>0&&items.All(x=>!string.IsNullOrWhiteSpace(x)),"menu items");Check(!string.IsNullOrWhiteSpace(h.GetMenuTitle()),"title");for(int row=0;row<items.Length;row++){h._selection=row;Check(!string.IsNullOrWhiteSpace(h.GetSelectedDescription()),"footer "+menu+"/"+row);}h._selection=0;h.Key(Keys.Up);h.Up(Keys.Up);Check(h._selection==items.Length-1,"wrap "+menu);h.Key(Keys.Down);h.Up(Keys.Down);Check(h._selection==0,"unwrap "+menu);string title=h.GetMenuTitle();h.NavigateBack();Console.WriteLine("MATRIX|"+menu+"|"+title+"|"+h._menuPage+"|"+h._selection+"|"+string.Join("; ",items));Check(h.saves+h.payments+h.transfers==0,"browse writes");});}
for(int staff=0;staff<3;staff++)for(int direction=0;direction<2;direction++)for(int mode=0;mode<2;mode++){int who=staff,way=direction,custom=mode;Test("each mechanic direction/mode route-mask path "+who+"/"+way+"/"+custom,()=>{var h=Hub();h._customEmployeeWalkingPaths=custom==1;h._mechanics.AddRange(new[]{new Mechanic(),new Mechanic(),new Mechanic()});var geometry=h._serviceRoute;h._selection=4;h.ExecuteSelection();h._selection=0;h.ExecuteSelection();h._selection=3;h.ExecuteSelection();h._selection=who;h.ExecuteSelection();h._selection=way;h.ExecuteSelection();h._selection=who;h.ExecuteSelection();for(int i=0;i<3;i++){Check(h._mechanics[i].JobOutboundMask==(i==who&&way==0?31^(1<<who):31),"outbound mask");Check(h._mechanics[i].JobReturnMask==(i==who&&way==1?31^(1<<who):31),"return mask");}Check(Object.ReferenceEquals(geometry,h._serviceRoute)&&geometry.Length==5&&geometry.All(x=>!x.IsSet),"shared geometry unchanged");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicRouteDirection&&h._selection==way,"direction restore");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicRouteRoster&&h._selection==who,"staff restore");h.NavigateBack();Check(h._menuPage==MenuPage.MechanicPlacementRoutes&&h._selection==3,"role restore");});}
Test("management has no cash or controller menu shortcuts",()=>{var h=new Harness{_menuPage=MenuPage.BusinessManagement};Check(h.GetMenuItems().All(x=>!x.Contains("Deposit")&&!x.Contains("Withdraw")&&!x.Contains("Controller")),"obsolete entry");});
Test("owner transfer review does not move cash before confirmation",()=>{var h=new Harness{_menuPage=MenuPage.OwnerTransfers};h._selection=0;h.ExecuteFinancePageForTest();Check(h._ownerTransferAmount==10000&&h.transfers==0,"amount selector");h._selection=1;h.ExecuteFinancePageForTest();Check(h._menuPage==MenuPage.OwnerTransferReview&&h.transfers==0,"premature transfer");h._selection=2;h.ExecuteSelection();Check(h._menuPage==MenuPage.OwnerTransfers&&h._selection==1&&h.transfers==0,"review back");});
Test("retail keyboard/controller quantity adjustments edit draft only",()=>{var h=new Harness{_menuPage=MenuPage.PartsSales,_selection=0,_showMenu=true};h.Key(Keys.Right);h.Up(Keys.Right);Check(h._partsSaleCart[0]==1&&h.payments==0,"key quantity");h=new Harness{_menuPage=MenuPage.PartsSales,_selection=1,_showMenu=true};h.Pad(GTA.Control.FrontendRight);Check(h._partsSaleCart[1]==1&&h.payments==0,"controller quantity");h._selection=4;h.ExecuteSelection();Check(h._menuPage==MenuPage.PartsSaleReview&&h.payments==0,"retail review");h.NavigateBack();Check(h._menuPage==MenuPage.PartsSales&&h._selection==4,"retail Back");});
Test("native pause hides placement overlay without writes",()=>{var h=Hub();h.Key(Keys.Escape);Check(!h._showMenu&&h.saves+h.payments==0,"Esc");h._showMenu=true;h.edges.Add(GTA.Control.FrontendPause);Check(h.YieldMenuToNativePause()&&!h._showMenu,"Start");});
}
}
