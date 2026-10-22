using System;
using System.Collections.Generic;
struct Vector3 { internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;} }
struct Model { internal int Hash; }
class Vehicle { internal bool alive=true;internal Model Model=new Model{Hash=42};internal string plate="ASSIGNED";internal Vector3 Position;internal bool Exists()=>alive; }
class Ped { internal bool alive=true;internal Vector3 Position;internal Vehicle CurrentVehicle;internal bool Exists()=>alive;internal bool IsInVehicle()=>CurrentVehicle!=null; }
class Prop { internal bool alive=true;internal Vector3 Position;internal bool Exists()=>alive; }
static class Game { internal static PlayerState Player=new PlayerState();internal class PlayerState { internal Ped Character=new Ped(); } }
enum BlipSprite { Package }
enum BlipColor { Orange,Yellow }
class Blip {
    internal bool alive=true;internal int writes,removes,removeAttempts;internal bool failName,failRemove;
    internal Vector3 pos;internal Vector3 Position {get=>pos;set{pos=value;writes++;}}
    internal BlipSprite Sprite;internal BlipColor Color;internal float Scale;internal bool IsShortRange=true;
    string name;internal string Name {get=>name;set{if(failName){failName=false;throw new Exception("name");}name=value;}}
    internal bool Exists()=>alive;internal void Remove(){removeAttempts++;if(failRemove)throw new Exception("remove");alive=false;removes++;}
}
static class World {
    internal static List<Blip> blips=new List<Blip>();internal static bool failCreate,failName;
    internal static int calls;internal static int waypoint=123;
    internal static Blip CreateBlip(Vector3 p){calls++;if(failCreate)return null;var b=new Blip{pos=p,failName=failName};blips.Add(b);return b;}
}
class Harness {
    class WorkBay {internal bool IsSet=true;internal Vector3 Position=new Vector3(-164,-2676,6);}
    /*<source:UpdateWholesalePackageBlip>*/
    /*<source:WholesalePackageBlipPosition>*/
    /*<source:DeleteWholesalePackageBlip>*/
    /*<source:MatchesWholesaleVehicle>*/
    Blip _wholesalePackageBlip;bool _wholesalePackageBlipConfigured,_wholesalePackageHasPosition;
    Vector3 _wholesalePackageLastPosition;long _wholesalePackageOrderIdentity;
    Vehicle _wholesaleBlipVehicle,_cachedWholesaleVehicle;
    DateTime _nextWholesaleBlipVehicleCheck=DateTime.MinValue,_nextWholesaleBlipRetry=DateTime.MinValue,_nextWholesaleBlipCleanupRetry=DateTime.MinValue;
    int _deliveryKind=0,_deliverySupplier=0,_deliveryQuantity=5;long _deliveryDueUtcTicks=1;
    double _deliveryRemainingGameSeconds;bool _supplierPlayerCarrying,_wholesaleCargoLoaded,_wholesaleCargoUnloaded;
    int _wholesaleVehicleModel=42;string _wholesaleVehiclePlate="ASSIGNED";
    WorkBay _wholesaleWarehousePickup=new WorkBay();Prop _supplierCrate;
    int errors,plateReads;
    bool HasWholesaleVehicle()=>_wholesaleVehicleModel!=0&&!String.IsNullOrWhiteSpace(_wholesaleVehiclePlate);
    string WholesalePlate(Vehicle v){plateReads++;return v.plate;}
    void Log(string c,Exception e){errors++;}
    static DateTime start=new DateTime(2026,10,22,0,0,0,DateTimeKind.Utc);
    void Update(int second=0)=>UpdateWholesalePackageBlip(start.AddSeconds(second));
    static int passed;
    static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void At(Blip b,Vector3 p){Check(b!=null&&b.alive&&b.Position.X==p.X&&b.Position.Y==p.Y&&b.Position.Z==p.Z,"position");}
    static Vector3 P(float x)=>new Vector3(x,12,6);
    static void Test(string n,Action a){Game.Player.Character=new Ped{Position=P(10000)};World.blips.Clear();World.failCreate=World.failName=false;World.calls=0;World.waypoint=123;a();passed++;Console.WriteLine("PASS #19 "+n);}
    static void Main(){
        Test("no pending order creates nothing",()=>{var h=new Harness{_deliveryKind=-2};h.Update();Check(World.calls==0,"ghost");});
        Test("pending ETA has one named long-range package icon at warehouse",()=>{var h=new Harness{_deliveryRemainingGameSeconds=300};h.Update();At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);Check(h._wholesalePackageBlip.Name=="WHOLESALE package"&&!h._wholesalePackageBlip.IsShortRange&&h._wholesalePackageBlip.Color==BlipColor.Orange&&h._wholesalePackageBlip.Sprite==BlipSprite.Package,"identity");});
        Test("ready warehouse icon exists far beyond 80m with no props",()=>{var h=new Harness();h.Update();At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);Check(h._supplierCrate==null,"model dependency");});
        Test("ETA expiration reuses icon without requiring model creation",()=>{var h=new Harness{_deliveryRemainingGameSeconds=1};h.Update();var b=h._wholesalePackageBlip;h._deliveryRemainingGameSeconds=0;h.Update(1);Check(h._wholesalePackageBlip==b&&World.calls==1,"ETA duplicate");});
        Test("pickup and carrying follow player without removing icon",()=>{var h=new Harness();h.Update();var b=h._wholesalePackageBlip;h._supplierPlayerCarrying=true;h.Update(1);At(b,Game.Player.Character.Position);Game.Player.Character.Position=P(20000);h.Update(2);At(b,Game.Player.Character.Position);Check(World.calls==1,"pickup duplicate");});
        Test("loaded cargo follows actual registered vehicle while driving",()=>{var v=new Vehicle{Position=P(200)};var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=v};h.Update();At(h._wholesalePackageBlip,v.Position);v.Position=P(500);h.Update(1);At(h._wholesalePackageBlip,v.Position);Check(h.plateReads==1,"unthrottled validation");});
        Test("retains validated vehicle after nearby visual cache clears",()=>{var v=new Vehicle{Position=P(200)};var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=v};h.Update();h._cachedWholesaleVehicle=null;v.Position=P(2000);h.Update(4);At(h._wholesalePackageBlip,v.Position);});
        Test("unload follows carrier through pallet walk and preserves icon",()=>{var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=new Vehicle{Position=P(2)}};h.Update();var b=h._wholesalePackageBlip;h._wholesaleCargoLoaded=false;h._wholesaleCargoUnloaded=h._supplierPlayerCarrying=true;Game.Player.Character.Position=P(55);h.Update(1);At(b,Game.Player.Character.Position);Check(World.calls==1,"unload duplicate");});
        Test("actual completed order removes owned icon once",()=>{var h=new Harness();h.Update();var b=h._wholesalePackageBlip;h._deliveryKind=-2;h._deliveryQuantity=0;h.Update(1);h.Update(2);Check(!b.alive&&b.removes==1&&h._wholesalePackageBlip==null,"deposit cleanup");});
        Test("unfinished full pallet or failed deposit retains icon",()=>{var h=new Harness{_supplierPlayerCarrying=true,_wholesaleCargoUnloaded=true};h.Update();for(int i=1;i<10;i++)h.Update(i);Check(h._wholesalePackageBlip.alive&&World.calls==1,"premature stock");});
        Test("abort cleanup then reload reconstructs exactly one active icon",()=>{var h=new Harness();h.Update();h.DeleteWholesalePackageBlip();Check(!World.blips[0].alive,"abort orphan");var reload=new Harness();reload.Update();reload.Update(1);Check(World.calls==2&&World.blips.FindAll(b=>b.alive).Count==1,"reload duplicates");});
        Test("abort before activation performs no blip creation",()=>{new Harness().DeleteWholesalePackageBlip();Check(World.calls==0,"early native creation");});
        Test("second order gets fresh warehouse location and no stale vehicle",()=>{var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=new Vehicle{Position=P(5)}};h.Update();h._deliveryKind=-2;h.Update(1);h._deliveryKind=1;h._wholesaleCargoLoaded=false;h._deliveryDueUtcTicks=2;h._deliveryRemainingGameSeconds=20;h.Update(2);At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);Check(World.blips.FindAll(b=>b.alive).Count==1,"second order duplicate");});
        Test("cancel removes owned marker without customer or waypoint changes",()=>{var customer=new Blip{Color=BlipColor.Yellow};var h=new Harness();h.Update();h._deliveryKind=-2;h.Update(1);Check(customer.alive&&customer.removes==0&&World.waypoint==123,"foreign marker");});
        Test("recovery resets actionable location to warehouse using same icon",()=>{var h=new Harness{_supplierPlayerCarrying=true};h.Update();var b=h._wholesalePackageBlip;h._supplierPlayerCarrying=false;h._wholesaleCargoLoaded=h._wholesaleCargoUnloaded=false;h.Update(1);At(b,h._wholesaleWarehousePickup.Position);Check(World.calls==1,"recovery duplicate");});
        Test("missing loaded vehicle keeps last known location",()=>{var v=new Vehicle{Position=P(700)};var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=v};h.Update();v.alive=false;h._cachedWholesaleVehicle=null;Game.Player.Character=null;h.Update(4);At(h._wholesalePackageBlip,P(700));});
        Test("missing carrier keeps last known location",()=>{var h=new Harness{_supplierPlayerCarrying=true};h.Update();var last=Game.Player.Character.Position;Game.Player.Character=null;h.Update(1);At(h._wholesalePackageBlip,last);});
        Test("loaded reload resolves registered player vehicle with no scan",()=>{var v=new Vehicle{Position=P(66)};Game.Player.Character.CurrentVehicle=v;var h=new Harness{_wholesaleCargoLoaded=true};h.Update();At(h._wholesalePackageBlip,v.Position);});
        Test("wrong plate and wrong model never become tracked vehicle",()=>{foreach(var v in new[]{new Vehicle{plate="WRONG",Position=P(1)},new Vehicle{Model=new Model{Hash=7},Position=P(2)}}){var h=new Harness{_wholesaleCargoLoaded=true,_cachedWholesaleVehicle=v};h.Update();At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);}});
        Test("missing vehicle uses owned attached cargo then last known location",()=>{var h=new Harness{_wholesaleCargoLoaded=true,_supplierCrate=new Prop{Position=P(9)}};h.Update();At(h._wholesalePackageBlip,P(9));h._supplierCrate.alive=false;h.Update(1);At(h._wholesalePackageBlip,P(9));});
        Test("old save needs no new settings or persisted blip fields",()=>{var h=new Harness{_deliveryDueUtcTicks=0,_wholesaleCargoLoaded=true};h.Update();At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);Check(World.calls==1,"old schema");});
        Test("repeated refresh creates no duplicate marker",()=>{var h=new Harness();for(int i=0;i<1000;i++)h.Update(i);Check(World.calls==1&&World.blips.Count==1,"duplicate");});
        Test("externally lost owned handle reconstructs one active icon",()=>{var h=new Harness();h.Update();h._wholesalePackageBlip.alive=false;h.Update(1);h.Update(2);Check(World.calls==2&&World.blips.FindAll(b=>b.alive).Count==1,"lost handle");});
        Test("blip creation failure backs off without order changes",()=>{World.failCreate=true;var h=new Harness();for(int i=0;i<10;i++)h.Update(i);Check(World.calls==1&&h._deliveryKind==0&&h._deliveryQuantity==5&&h.errors==1,"create flood/business");World.failCreate=false;h.Update(10);Check(World.calls==2&&h._wholesalePackageBlip.alive,"create recovery");});
        Test("setter failure retains owned handle and completes configuration",()=>{World.failName=true;var h=new Harness();h.Update();Check(World.calls==1&&!h._wholesalePackageBlipConfigured,"failed setter");World.failName=false;h.Update(10);Check(World.calls==1&&h._wholesalePackageBlipConfigured&&h._wholesalePackageBlip.Name=="WHOLESALE package","leak");});
        Test("failed cleanup retains ownership until retry succeeds",()=>{var h=new Harness();h.Update();var b=h._wholesalePackageBlip;b.failRemove=true;h._deliveryKind=-2;h.Update(1);Check(h._wholesalePackageBlip==b&&b.alive,"orphan reference");b.failRemove=false;h.Update(20);Check(!b.alive&&h._wholesalePackageBlip==null,"cleanup recovery");});
        Test("setter failure then cancel removes owned blip immediately despite visual backoff",()=>{
            World.failName=true;var h=new Harness();h.Update();var blip=h._wholesalePackageBlip;
            Check(blip!=null&&blip.alive&&!h._wholesalePackageBlipConfigured,"setup failure");
            h._deliveryKind=-2;h.Update(1);
            Check(h._wholesalePackageBlip==null&&!blip.alive&&blip.removes==1,"cleanup delayed by visual retry");
        });
        Test("create failure then cancel clears transient retry and next order can create",()=>{
            World.failCreate=true;var h=new Harness();h.Update();
            Check(h._wholesalePackageBlip==null&&h.errors==1,"create should fail once");
            h._deliveryKind=-2;h.Update(1);
            Check(h._nextWholesaleBlipRetry==DateTime.MinValue&&!h._wholesalePackageHasPosition,"stale failed order");
            h._deliveryKind=0;h._deliveryDueUtcTicks=2;World.failCreate=false;h.Update(2);
            At(h._wholesalePackageBlip,h._wholesaleWarehousePickup.Position);
        });
        Test("new order resets prior setter backoff and preserves single handle",()=>{
            World.failName=true;var h=new Harness();h.Update();var b=h._wholesalePackageBlip;
            h._deliveryDueUtcTicks=2;h._deliveryRemainingGameSeconds=10;
            h.Update(1);
            Check(h._wholesalePackageBlip==b&&h._wholesalePackageBlipConfigured&&World.calls==1,"old order retry carried over");
        });
        Test("failed cleanup retries at most every ten seconds without leaking blips",()=>{
            var h=new Harness();h.Update();var b=h._wholesalePackageBlip;b.failRemove=true;
            h._deliveryKind=-2;h.Update(1);
            for(int t=2;t<11;t++)h.Update(t);
            Check(b.removeAttempts==1&&h._wholesalePackageBlip==b,"cleanup failure spam");
            h.Update(11);h.Update(12);
            Check(b.removeAttempts==2,"retry interval");
            b.failRemove=false;h.Update(21);
            Check(b.removeAttempts==3&&!b.alive&&h._wholesalePackageBlip==null,"cleanup recovery");
        });
        Test("invalid order and non-wholesale supplier remove only own icon",()=>{foreach(int invalid in new[]{-2,3}){var h=new Harness();h.Update();h._deliveryKind=invalid;h.Update(1);Check(h._wholesalePackageBlip==null,"invalid retained");}var local=new Harness();local.Update();local._deliverySupplier=1;local.Update(1);Check(local._wholesalePackageBlip==null,"local retained");});
        Console.WriteLine($"#19 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
