using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GTA { enum Control { Test=201 } }
namespace GTA.Native {
 enum Hash { IS_CONTROL_JUST_PRESSED,IS_DISABLED_CONTROL_JUST_PRESSED,IS_CONTROL_PRESSED,IS_DISABLED_CONTROL_PRESSED }
 static class Function {
  internal static HashSet<string> active=new HashSet<string>();internal static List<string> calls=new List<string>();
  internal static T Call<T>(Hash h,params object[] a){string k=h+":"+a[0]+":"+a[1];calls.Add(k);return (T)(object)active.Contains(k);}
 }
}
struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;} public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.X+b.X,a.Y+b.Y,a.Z+b.Z);}
class Ped {internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive;}
static class Game {internal static bool IsPaused;internal static int GameTime;internal static PlayerState Player=new PlayerState();internal class PlayerState{internal Ped Character=new Ped();}}
class Model : IEquatable<Model> {
 internal static Dictionary<string,Model> models=new Dictionary<string,Model>();internal static bool defaultsLoaded;
 internal string name;internal Model(string n){name=n;if(!models.ContainsKey(n))models[n]=this;}
 Model State=>models[name];internal bool loaded;internal bool IsLoaded=>State.loaded||defaultsLoaded;
 internal int requests,releases;internal bool failRelease;
 internal void Request(){State.requests++;}internal void Request(int timeout){throw new Exception("blocking request");}
 internal void MarkAsNoLongerNeeded(){State.releases++;if(State.failRelease)throw new Exception("release failed");}
 public bool Equals(Model m)=>m!=null&&name==m.name;public override bool Equals(object m)=>Equals(m as Model);public override int GetHashCode()=>name.GetHashCode();
}
class Prop {internal bool alive=true,IsPositionFrozen;internal float Heading;internal Vector3 Position;internal bool Exists()=>alive;internal void Delete(){alive=false;}}
static class World {internal static int creates;internal static bool failCreate;internal static Prop CreateProp(Model m,Vector3 p,bool dynamic,bool ground){creates++;if(failCreate)throw new Exception("create");return new Prop{Position=p};}}
class Harness {
    long _financeActiveMs;
    bool _nativePauseYielded;
    bool YieldMenuToNativePause()=>Game.IsPaused;
    DateTime _supplierGameplayNow = DateTime.UtcNow;
    /*<source:PadJustPressed>*/
    /*<source:PadPressed>*/
    /*<source:RequireExclusiveBusinessInstallation>*/
    /*<source:RequestVisualModel>*/
    /*<source:ReleaseVisualModel>*/
    /*<source:ReleasePendingVisualModels>*/
    /*<source:CreateFixedEquipmentProp>*/
    /*<source:UpdateOfficeDeskVisuals>*/
    /*<source:DeleteOfficeDeskProps>*/
    /*<source:OnTick>*/
    /*<source:AdvanceCustomerDemand>*/
    bool _customerServiceEnabled=true;double _customerDemandElapsed;
    /*<source:TryInitializeGameplayModels>*/
    string _muleModelName="mule2",_supplierDriverModelName="driver",_pedModelName="mechanic",_receptionistModelName="receptionist",_managerPedModelName="manager",_customerPedModelName="customer",_customerVehicleModelName="asea";
    Model _muleModel,_supplierDriverModel,_mechanicModel,_receptionistModel,_managerPedModel,_customerPedModel,_customerVehicleModel;
    bool _gameplayModelsReady;int _gameplayModelAttempts;DateTime _nextGameplayModelAttemptUtc;

    class Point {internal bool IsSet=true;internal Vector3 Position;internal float Heading;}
    HashSet<Model> _requestedVisualModels=new HashSet<Model>();
    bool _officeDeskPurchased=true,_officeDeskPlaced=true;Point _officeDeskLocation=new Point();
    Prop _officeDeskProp,_officeLaptopProp;DateTime _nextOfficeSpawn=DateTime.MinValue;
    bool _bootGameplayActivated=true,_diagnosticHoldGameplayTick,_clockedIn=true,_remoteMode,_placed,_showMenu,_showPlacementMarkers,_autoLotPreview,_controllerEnabled;
    int _managerTier=-1,_lastGameTimeSample,_deliveryKind=-2,_supplierVisualStage;float _remoteDistanceMeters=100;
    Vector3 _location;DateTime _bootGateUntilUtc=DateTime.MinValue,_lastTick=DateTime.UtcNow;
    string _diagnosticTickStage;double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds,_visualRefreshClock;
    List<int> _remoteJobs=new List<int>(),_activeCustomerOrders=new List<int>();bool _workersPreviouslyInRange;
    class Checkpoint {internal void RecordTransition(bool a,bool b,bool c,bool d,int e,int f,int g){}}
    Checkpoint _diagnosticLog=new Checkpoint();
    int saves,notices,errors,economyCalls,remoteCalls,visuals;double economySeconds,remoteSeconds;
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    void Log(string c,Exception e){errors++;}void Notify(string s){notices++;}string MenuKeyLabel()=>"F2";
    void Save(){saves++;}void ProcessEconomy(double s){economyCalls++;economySeconds+=s;}
    void ProcessRemoteOperations(double s){remoteCalls++;remoteSeconds+=s;}
    void EnterRemoteOperations(){_remoteMode=true;}void LeaveRemoteOperations(){_remoteMode=false;}
    void DisableControllerControlsWhileMenuOpen(){}void PollController(){}void HandleSupplierPackageInteraction(){}
    void DrawWholesalePackageMarker(){}void HandleOfficeInteraction(){}void UpdateVisualMechanics(){visuals++;}
    void ProcessSupplierDelivery(DateTime n){}void DeferSupplierPlayerHandoff(DateTime n){}void DeleteSupplierVisuals(){}void UpdateCustomerVisuals(){}
    void UpdateEquipmentVisuals(){}void UpdateSupplierPalletVisual(){}void UpdateWholesalePalletVisual(){}
    void DisablePhoneWhileMenuOpen(){}void DrawMenu(){}void DrawPlacementMarkers(){}
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){Game.IsPaused=false;Game.GameTime=0;Game.Player.Character=new Ped();Model.models.Clear();Model.defaultsLoaded=false;World.creates=0;World.failCreate=false;GTA.Native.Function.active.Clear();GTA.Native.Function.calls.Clear();a();passed++;Console.WriteLine("PASS migration "+n);}
    static void Main(){
        Test("both groups and both enabled/disabled press edges retain R2 semantics",()=>{
            foreach(string h in new[]{"IS_CONTROL_JUST_PRESSED","IS_DISABLED_CONTROL_JUST_PRESSED"})foreach(int group in new[]{0,2}){
                GTA.Native.Function.active.Clear();GTA.Native.Function.active.Add(h+":"+group+":201");Check(PadJustPressed(GTA.Control.Test),h+group);
            }});
        Test("both groups and both enabled/disabled held states retain R2 semantics",()=>{
            foreach(string h in new[]{"IS_CONTROL_PRESSED","IS_DISABLED_CONTROL_PRESSED"})foreach(int group in new[]{0,2}){
                GTA.Native.Function.active.Clear();GTA.Native.Function.active.Add(h+":"+group+":201");Check(PadPressed(GTA.Control.Test),h+group);
            }});
        Test("inactive input reads at most four bounded native calls",()=>{Check(!PadJustPressed(GTA.Control.Test)&&GTA.Native.Function.calls.Count==4,"press bounded");GTA.Native.Function.calls.Clear();Check(!PadPressed(GTA.Control.Test)&&GTA.Native.Function.calls.Count==4,"held bounded");});
        Test("canonical v2 source, v2 DLL and old RC1 source block v3 before business load",()=>{
            foreach(string p in new[]{@"scripts\GlassHeroesBusiness.2.cs",@"scripts\GlassHeroesBusiness.dll",@"scripts\GlassHeroesBusiness_v0.3.3.0_RC1.2.cs"}){
                bool threw=false;try{RequireExclusiveBusinessInstallation(x=>x==p);}catch(InvalidOperationException){threw=true;}Check(threw,"duplicate "+p);
            }});
        Test("source plus compiled v3 duplication is rejected",()=>{bool threw=false;try{RequireExclusiveBusinessInstallation(x=>x==@"scripts\GlassHeroesBusiness.3.cs"||x==@"scripts\GlassHeroesBusiness.V3.Experimental.dll");}catch(InvalidOperationException){threw=true;}Check(threw,"v3 duplicate");});
        Test("one v3 source or one v3 assembly is accepted without file writes",()=>{RequireExclusiveBusinessInstallation(x=>x==@"scripts\GlassHeroesBusiness.3.cs");RequireExclusiveBusinessInstallation(x=>x==@"scripts\GlassHeroesBusiness.V3.Experimental.dll");});
        Test("unloaded prop returns without creation and retains owned model for retry",()=>{var h=new Harness();Check(h.CreateFixedEquipmentProp("box",new Vector3(),0)==null&&World.creates==0&&h._requestedVisualModels.Count==1&&Model.models["box"].requests==1&&Model.models["box"].releases==0,"unloaded");});
        Test("delayed model availability creates one frozen grounded prop and releases request",()=>{var h=new Harness();h.CreateFixedEquipmentProp("box",new Vector3(),0);Model.models["box"].loaded=true;var p=h.CreateFixedEquipmentProp("box",new Vector3(),33);Check(p!=null&&p.IsPositionFrozen&&p.Heading==33&&World.creates==1&&h._requestedVisualModels.Count==0&&Model.models["box"].releases==1,"retry");});
        Test("repeated failed requests retain bounded per-model ownership",()=>{var h=new Harness();for(int i=0;i<1000;i++)h.CreateFixedEquipmentProp("box",new Vector3(),0);Check(h._requestedVisualModels.Count==1&&Model.models.Count==1&&World.creates==0,"ownership growth");});
        Test("prop creation exception still releases model",()=>{Model.defaultsLoaded=true;World.failCreate=true;var h=new Harness();try{h.CreateFixedEquipmentProp("box",new Vector3(),0);}catch(Exception){}Check(h._requestedVisualModels.Count==0&&Model.models["box"].releases==1,"finally");});
        Test("abort releases independent requests and retains failed ownership for retry",()=>{var h=new Harness();h.CreateFixedEquipmentProp("a",new Vector3(),0);h.CreateFixedEquipmentProp("b",new Vector3(),0);Model.models["a"].failRelease=true;h.ReleasePendingVisualModels();Check(h._requestedVisualModels.Count==1&&Model.models["b"].releases==1&&h.errors==1,"cleanup ownership");Model.models["a"].failRelease=false;h.ReleasePendingVisualModels();Check(h._requestedVisualModels.Count==0&&Model.models["b"].releases==1,"retry or double release");});
        Test("office requests both models without waits and respects retry clock",()=>{var h=new Harness();h.UpdateOfficeDeskVisuals();for(int i=0;i<100;i++)h.UpdateOfficeDeskVisuals();Check(World.creates==0&&h._requestedVisualModels.Count==2&&Model.models.Values.All(m=>m.requests==1),"office flood");});
        Test("office retries create only one desk/laptop pair and releases both models",()=>{var h=new Harness();h.UpdateOfficeDeskVisuals();foreach(var m in Model.models.Values)m.loaded=true;h._nextOfficeSpawn=DateTime.MinValue;h.UpdateOfficeDeskVisuals();h.UpdateOfficeDeskVisuals();Check(World.creates==2&&h._requestedVisualModels.Count==0&&Model.models.Values.All(m=>m.releases==1),"office pair");});
        Test("distance-gated office makes no model request or entity",()=>{Game.Player.Character.Position=new Vector3(1000,0,0);var h=new Harness();h.UpdateOfficeDeskVisuals();Check(World.creates==0&&Model.models.Count==0,"distance gate");});
        Test("startup hold and delay make no progress or visual calls",()=>{var h=new Harness{_bootGameplayActivated=false,_diagnosticHoldGameplayTick=true};h.OnTick(null,EventArgs.Empty);Check(h.economyCalls==0&&h.remoteCalls==0&&h.visuals==0&&h.saves==0,"held startup");h._diagnosticHoldGameplayTick=false;h._bootGateUntilUtc=DateTime.UtcNow.AddMinutes(1);h.OnTick(null,EventArgs.Empty);Check(!h._bootGameplayActivated&&h.saves==0,"delay");});
        Test("startup requires valid unpaused player",()=>{var h=new Harness{_bootGameplayActivated=false};Game.Player.Character=null;h.OnTick(null,EventArgs.Empty);Check(!h._bootGameplayActivated,"invalid player");Game.Player.Character=new Ped();Game.IsPaused=true;h.OnTick(null,EventArgs.Empty);Check(!h._bootGameplayActivated,"paused activation");});
        Test("fresh activation resets clocks after days closed without offline accrual",()=>{var h=new Harness{_bootGameplayActivated=false,_lastTick=DateTime.UtcNow.AddDays(-20),_lastGameTimeSample=-100000,_deliveryKind=0,_deliveryRemainingGameSeconds=120};Game.GameTime=500000;h.OnTick(null,EventArgs.Empty);Check(h._bootGameplayActivated&&h._lastGameTimeSample==500000&&h._deliveryRemainingGameSeconds==120&&h.economyCalls==0&&h.remoteCalls==0&&h.saves==0,"offline gate");});
        Test("paused gameplay advances no wages income ETA or remote jobs",()=>{var h=new Harness{_lastTick=DateTime.UtcNow.AddHours(-2),_deliveryKind=0,_deliveryRemainingGameSeconds=120,_remoteMode=true};h._remoteJobs.Add(1);DateTime routeClock=h._supplierGameplayNow;Game.IsPaused=true;Game.GameTime=1000;h.OnTick(null,EventArgs.Empty);Check(h._supplierGameplayNow==routeClock,"paused supplier routes");Check(h.economyCalls==0&&h.remoteCalls==0&&h._deliveryRemainingGameSeconds==120&&h.saves==0,"pause advance");});
        Test("active game seconds advance ETA and remote jobs together",()=>{var h=new Harness{_deliveryKind=0,_deliveryRemainingGameSeconds=120,_remoteMode=true};h._remoteJobs.Add(1);DateTime routeClock=h._supplierGameplayNow;Game.GameTime=1000;h.OnTick(null,EventArgs.Empty);Check(h._supplierGameplayNow==routeClock.AddSeconds(1),"active supplier routes");Check(h._deliveryRemainingGameSeconds==119&&h.remoteSeconds==1&&h._supplierTimerSaveSeconds==1,"active game clock");});
        Test("large game-clock jump never catches up ETA or remote jobs",()=>{var h=new Harness{_deliveryKind=0,_deliveryRemainingGameSeconds=120,_remoteMode=true};h._remoteJobs.Add(1);Game.GameTime=1000000;h.OnTick(null,EventArgs.Empty);Check(h._deliveryRemainingGameSeconds==120&&h.remoteCalls==0,"jump catchup");});
        Test("long active sample retains R2 two-second cap",()=>{var h=new Harness{_deliveryKind=0,_deliveryRemainingGameSeconds=120,_remoteMode=true,_lastTick=DateTime.UtcNow.AddSeconds(-100)};h._remoteJobs.Add(1);Game.GameTime=10000;h.OnTick(null,EventArgs.Empty);Check(h._deliveryRemainingGameSeconds==118&&h.remoteSeconds==2&&h.economySeconds==2,"cap");});
        Test("ETA persists only after fifteen active seconds or readiness",()=>{var h=new Harness{_deliveryKind=0,_deliveryRemainingGameSeconds=120};for(int i=1;i<=14;i++){Game.GameTime=i*1000;h.OnTick(null,EventArgs.Empty);}Check(h.saves==0,"hot loop writes");Game.GameTime=15000;h.OnTick(null,EventArgs.Empty);Check(h.saves==1&&h._deliveryRemainingGameSeconds==105,"timer save");h._deliveryRemainingGameSeconds=0.5;Game.GameTime=16000;h.OnTick(null,EventArgs.Empty);Check(h._deliveryRemainingGameSeconds==0&&h.saves==2,"ready save");});
        Console.WriteLine($"Migration {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
