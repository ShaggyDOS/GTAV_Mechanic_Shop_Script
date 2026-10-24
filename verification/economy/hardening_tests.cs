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
 internal static int hashes,failAt;internal string name;internal Model(string n){hashes++;if(failAt==hashes)throw new InvalidOperationException("injected hash error");name=n;if(!models.ContainsKey(n))models[n]=this;}
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
    void SyncWholesalePickupBlip(DateTime now){} // The locator suite exercises its actual production body.
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
 Test("hold blocks model hashing and all progress",()=>{Model.hashes=0;var h=new Harness{_bootGameplayActivated=false,_diagnosticHoldGameplayTick=true};for(int i=0;i<100;i++)h.OnTick(null,EventArgs.Empty);Check(Model.hashes==0&&h.economyCalls==0&&h.saves==0&&h._gameplayModelAttempts==0,"hold");});
 Test("delay blocks model hashing",()=>{Model.hashes=0;var h=new Harness{_bootGameplayActivated=false,_bootGateUntilUtc=DateTime.UtcNow.AddMinutes(1)};h.OnTick(null,EventArgs.Empty);Check(Model.hashes==0&&!h._bootGameplayActivated,"delay");});
 foreach(int invalid in new[]{0,1,2})Test("invalid loading player blocks model hashing "+invalid,()=>{Model.hashes=0;var h=new Harness{_bootGameplayActivated=false};if(invalid==0)Game.Player.Character=null;if(invalid==1)Game.Player.Character.alive=false;if(invalid==2)Game.IsPaused=true;h.OnTick(null,EventArgs.Empty);Check(Model.hashes==0&&!h._bootGameplayActivated&&h._gameplayModelAttempts==0,"player gate");});
 Test("valid activation hashes seven models once and resets gameplay clock",()=>{Model.hashes=0;var h=new Harness{_bootGameplayActivated=false,_lastGameTimeSample=-100000,_deliveryKind=0,_deliveryRemainingGameSeconds=120};Game.GameTime=500000;h.OnTick(null,EventArgs.Empty);Check(Model.hashes==7&&h._gameplayModelsReady&&h._bootGameplayActivated&&h._lastGameTimeSample==500000&&h._deliveryRemainingGameSeconds==120&&h.saves==0&&h.economyCalls==0&&World.creates==0,"activation");h.OnTick(null,EventArgs.Empty);Check(Model.hashes==7,"repeat hashes");});
 Test("model failure does not publish partial model references",()=>{Model.hashes=0;Model.failAt=3;try{var h=new Harness{_bootGameplayActivated=false};h.OnTick(null,EventArgs.Empty);Check(!h._bootGameplayActivated&&!h._gameplayModelsReady&&h._muleModel==null&&h._mechanicModel==null&&h._gameplayModelAttempts==1&&h.saves==0,"partial");h.OnTick(null,EventArgs.Empty);Check(Model.hashes==3,"early retry");}finally{Model.failAt=0;}});
 Test("model retry is bounded to three attempts five seconds apart",()=>{Model.hashes=0;var h=new Harness();var n=DateTime.UtcNow;try{for(int i=0;i<3;i++){Model.failAt=Model.hashes+1;Check(!h.TryInitializeGameplayModels(n.AddSeconds(i*5)),"expected failure");Check(!h.TryInitializeGameplayModels(n.AddSeconds(i*5+1)),"retry wait");}int count=Model.hashes;Check(!h.TryInitializeGameplayModels(n.AddHours(1))&&Model.hashes==count&&h._gameplayModelAttempts==3,"attempt limit");}finally{Model.failAt=0;}});
 Test("retry recovery preserves configured model names",()=>{Model.hashes=0;var h=new Harness{_muleModelName="custom_mule",_supplierDriverModelName="custom_driver"};var n=DateTime.UtcNow;Model.failAt=1;Check(!h.TryInitializeGameplayModels(n),"first failure");Model.failAt=0;Check(h.TryInitializeGameplayModels(n.AddSeconds(5))&&h._muleModel.name=="custom_mule"&&h._supplierDriverModel.name=="custom_driver"&&h._gameplayModelAttempts==2,"recovery");int c=Model.hashes;Check(h.TryInitializeGameplayModels(n.AddHours(1))&&Model.hashes==c,"one-time");});
 Console.WriteLine($"hardening {passed}/{passed} PASS; actual gate/initializer with counted model constructors and injected failures; GTA NOT RUN");
 }
}
