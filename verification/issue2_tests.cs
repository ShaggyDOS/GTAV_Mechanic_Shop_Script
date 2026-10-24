using System;
using System.Collections.Generic;
namespace GTA { enum Control { OpenModifier, OpenButton, Up, Down, Accept, Back, FrontendLeft, FrontendRight, FrontendPause, FrontendPauseAlternate } }
static class Game { internal static bool IsPaused; }
class Harness
{
    readonly Dictionary<MenuPage,KeyValuePair<MenuPage,int>> _placementMenuReturns=new Dictionary<MenuPage,KeyValuePair<MenuPage,int>>();
    bool _ownerTransferDeposit;MenuPage _serviceBayParent;int _serviceBaySlot;
    enum Keys { None, Up, Down, Left, Right, Enter, Return=Enter, Escape, Back, F2, E }
    class KeyEventArgs { internal Keys KeyCode; internal bool Control; internal KeyEventArgs(Keys k) { KeyCode=k; } }
    class WorkBay { }
    /*<source:MenuPage>*/
    /*<source:MenuInputGate>*/
    /*<source:MenuActionForKey>*/
    /*<source:PointEditorActionLabel>*/
    /*<source:HideBusinessMenuForPause>*/
    /*<source:YieldMenuToNativePause>*/
    /*<source:OnKeyDown>*/
    /*<source:OnKeyUp>*/
    /*<source:PollController>*/
    /*<source:NavigateBack>*/
    /*<source:OpenPointEditor>*/
    bool _nativePauseYielded;
    bool _bootGameplayActivated=true, _showMenu=true, _controllerEnabled=true, _requireCtrlForMenu;
    bool _managerPathsFromManagement, _mechanicPathsFromManagement, _receptionPathsFromManagement;
    bool _customCarBaysFromDriveOptions, _financesFromExpenses, _mechanicRosterFromManagement, _editingRoutePoint;
    int _selection, _editingStaffSlot, _editorReturnSelection, _routeConfigSlot, _editingBayIndex;
    bool _routeConfigReturning;
    MenuPage _menuPage=MenuPage.Business, _editorReturnPage;
    WorkBay _editingPoint;
    string _editingPointLabel;
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
    string[] GetMenuItems() { return new string[10]; }
    int[] _partsSaleCart=new int[4];
    /*<source:AdjustPartsSaleCart>*/
    void AdjustSupplierCart(int direction){}
    void ExecuteSelection() { selections++; _menuPage=MenuPage.PointEditor; }
    static void Log(string c, Exception e) { throw new Exception(c,e); }
    void Key(Keys k) { OnKeyDown(null,new KeyEventArgs(k)); }
    void Up(Keys k) { OnKeyUp(null,new KeyEventArgs(k)); }
    void Pad(GTA.Control c) { edges.Clear(); edges.Add(c); _lastControllerAction=DateTime.MinValue; PollController(); edges.Clear(); }
    static int passed;
    static void Check(bool b,string m) { if(!b)throw new Exception(m); }
    static void Test(string n,Action a) { a(); passed++; Console.WriteLine("PASS #2 "+n); }
    static void Main()
    {
        Test("keyboard Enter plus held/repeated frontend Accept performs one action",()=> {
            var h=new Harness(); h.Key(Keys.Enter);
            for(int i=0;i<1000;i++){h.Key(Keys.Enter);h.Pad(GTA.Control.Accept);}
            Check(h.selections==1,"duplicate confirmation/submenu action");
            h.Up(Keys.Enter); h.Key(Keys.Enter); Check(h.selections==2,"fresh keyboard press rejected");
        });
        Test("controller-first mapped keyboard echo does not execute twice",()=> {
            var h=new Harness(); h.Pad(GTA.Control.Accept); h.Key(Keys.Enter);
            Check(h.selections==1,"controller-first duplicate");
        });
        Test("held Enter cannot satisfy the next confirmation after ten seconds",()=> {
            var gate=new MenuInputGate(); long t=10000000;
            Check(gate.TryKeyboard(2,t),"first Enter");
            Check(!gate.TryController(2,t+10*TimeSpan.TicksPerSecond),"long hold allowed second action");
            gate.ReleaseKeyboard(2,t+11*TimeSpan.TicksPerSecond);
            Check(!gate.TryController(2,t+11*TimeSpan.TicksPerSecond+1),"release echo");
            Check(gate.TryKeyboard(2,t+11*TimeSpan.TicksPerSecond+2),"fresh keydown");
        });
        Test("controller-only consecutive physical presses remain accepted",()=> {
            var gate=new MenuInputGate(); Check(gate.TryController(2,1000),"first pad");
            Check(gate.TryController(2,1000+180*TimeSpan.TicksPerMillisecond),"second pad");
            var h=new Harness(); h.Pad(GTA.Control.Accept);h.Pad(GTA.Control.Accept);
            Check(h.selections==2,"pad callbacks");
        });
        Test("keyboard Back and frontend Back return only one page",()=> {
            var h=new Harness{_menuPage=MenuPage.ArrivalWaypoints};h.Key(Keys.Back);h.Pad(GTA.Control.Back);
            Check(h._menuPage==MenuPage.MechanicWalkingPaths && h._selection==2,"Back skipped parent or lost row");
        });
        Test("nested editor returns exact saved page and cursor",()=> {
            var h=new Harness{_menuPage=MenuPage.ManagerWaypoints,_selection=3};
            h.OpenPointEditor(new WorkBay(),"manager point",-1,true);
            h.Key(Keys.Back);h.Pad(GTA.Control.Back);
            Check(h._menuPage==MenuPage.ManagerWaypoints && h._selection==3,"editor parent/cursor");
        });
        Test("all corrected parent rows match source menu positions",()=> {
            var h=new Harness{_menuPage=MenuPage.ServiceWaypoints};h.NavigateBack();Check(h._selection==3,"service row");
            h._menuPage=MenuPage.BayApproaches;h.NavigateBack();Check(h._selection==4,"approach row");
            h._menuPage=MenuPage.ServiceVehicleBays;h.NavigateBack();Check(h._menuPage==MenuPage.CustomerPlacements&&h._selection==6,"customer bays row");
            h._customCarBaysFromDriveOptions=true;h._menuPage=MenuPage.ServiceVehicleBays;h.NavigateBack();Check(h._selection==4,"driving bays row");
        });
        Test("Back aliases hold until both physical keys are released",()=> {
            var g=new MenuInputGate();Check(g.TryKeyboard(3,1000),"first back");Check(!g.TryKeyboard(3,1001),"second alias");
            g.ReleaseKeyboard(3,1002);Check(!g.TryController(3,10000000),"other alias still held");
            g.ReleaseKeyboard(3,10000001);Check(g.TryController(3,20000000),"all aliases released");
        });
        Test("point editor labels distinguish unset saved and marker state",()=> {
            Check(PointEditorActionLabel(0,false,false)=="SET / SAVE","unset action");
            Check(PointEditorActionLabel(0,true,false)=="UPDATE / SAVE","saved action");
            Check(PointEditorActionLabel(1,false,false)=="NOT SET","clear unset");
            Check(PointEditorActionLabel(2,true,true)=="ON","marker persisted state");
        });
        Test("boot gate prevents both input effects and held-key registration",()=> {
            var h=new Harness{_bootGameplayActivated=false};h.Key(Keys.Enter);
            Check(h.selections==0&&h._heldKeys.Count==0,"startup input bypass");
        });
        Console.WriteLine($"#2 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
