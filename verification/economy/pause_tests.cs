using System;
using System.Collections.Generic;
namespace GTA { enum Control { OpenModifier, OpenButton, Up, Down, Accept, Back, FrontendLeft, FrontendRight, FrontendPause, FrontendPauseAlternate } }
static class Game { internal static bool IsPaused; }
class Harness
{
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
    bool _pointEditorBusy;long _pointEditorSequence;
    void TracePointEditor(string s){}
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
        Test("Escape hides overlay and never navigates or transacts",()=>{var h=new Harness{_menuPage=MenuPage.BuildSupplierOrder};h.Key(Keys.Escape);Check(!h._showMenu&&h._menuPage==MenuPage.BuildSupplierOrder&&h.selections==0,"escape");});
        foreach(var control in new[]{GTA.Control.FrontendPause,GTA.Control.FrontendPauseAlternate}) Test("controller native pause yields overlay "+control,()=>{var h=new Harness();h.edges.Add(control);Check(h.YieldMenuToNativePause()&&!h._showMenu&&h.selections==0,"native pause");});
        Test("already paused hides overlay even with controller disabled",()=>{var h=new Harness{_controllerEnabled=false};Game.IsPaused=true;Check(h.YieldMenuToNativePause()&&!h._showMenu,"paused");Game.IsPaused=false;});
        Test("native pause wins over mapped accept and open combo",()=>{var h=new Harness();h.edges.Add(GTA.Control.FrontendPause);h.edges.Add(GTA.Control.Accept);h._nativePauseYielded=h.YieldMenuToNativePause();h.PollController();Check(!h._showMenu&&h.selections==0,"pause precedence");});
        Test("pause releases held keyboard arbitration for later reopening",()=>{var h=new Harness();h.Key(Keys.Enter);h.Key(Keys.Escape);Check(h._heldKeys.Count==0,"held state");h._showMenu=true;h.Key(Keys.Enter);Check(h.selections==2,"reopen key");});
        Test("Backspace remains submenu navigation",()=>{var h=new Harness{_menuPage=MenuPage.ActiveDeliveries};h.Key(Keys.Back);Check(h._showMenu&&h._menuPage==MenuPage.InventoryManagement&&h._selection==2,"back");});
        Test("paused F2 cannot reopen overlay",()=>{var h=new Harness();Game.IsPaused=true;h.Key(Keys.F2);Check(!h._showMenu,"paused toggle");Game.IsPaused=false;});
        Test("cart arrow keyboard/frontend echo is one semantic action",()=>{var gate=new MenuInputGate();Check(gate.TryKeyboard(6,1000)&&!gate.TryController(6,1001),"right echo");gate.ReleaseKeyboard(6,2000);Check(gate.TryKeyboard(5,3000)&&!gate.TryController(5,3001),"left echo");});
        Console.WriteLine($"pause {passed}/{passed} PASS; production input/pause yielding, GTA native menu NOT RUN");
    }
}
