using System;
using System.Drawing;
namespace GTA.Native { enum Hash { SET_ENTITY_COLLISION } static class Function { internal static void Call(Hash h,params object[] args){} } }
struct Vector3 { internal float X,Y,Z; internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;} public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
class Prop { internal Vector3 Position;internal bool alive=true,FreezePosition;internal int Handle;internal bool Exists()=>alive; }
class Ped { internal Vector3 Position; internal bool Exists()=>true; }
static class Game { internal static PlayerState Player=new PlayerState();internal class PlayerState { internal Ped Character=new Ped(); } }
enum MarkerType { Ground=1 }
static class World { internal static int markers; internal static void DrawMarker(MarkerType m,Vector3 p,Vector3 d,Vector3 r,Vector3 scale,Color c){markers++;} }
class Harness
{
    class WorkBay { internal Vector3 Position;internal float Heading;internal bool IsSet=true; }
    /*<source:UpdateWholesalePalletVisual>*/
    /*<source:WarehousePackagePosition>*/
    /*<source:CreateWarehousePallet>*/
    /*<source:TryBeginWarehouseVisualAttempt>*/
    /*<source:DrawWholesalePackageMarker>*/
    int _deliveryKind=0,_deliverySupplier=0,_supplierVisualStage=7,_warehouseVisualAttempts;
    double _deliveryRemainingGameSeconds;
    bool _wholesaleCargoLoaded,_wholesaleCargoUnloaded,_supplierPlayerCarrying;
    WorkBay _wholesaleWarehousePickup=new WorkBay{Position=new Vector3(0,0,6)};
    Prop _wholesaleWarehousePallet,_supplierCrate;
    DateTime _nextWarehouseVisualAttempt=DateTime.MinValue;
    int creates,deletes;bool failModels,failBoxes;
    Prop CreateFixedEquipmentProp(string name,Vector3 p,float h,bool ground=true) {
        creates++;if(failModels || (failBoxes && name=="prop_cs_cardbox_01"))return null;
        // Simulate GTA resolving saved Z=6 down to actual floor Z=4.
        return new Prop{Position=ground?new Vector3(p.X,p.Y,4):p};
    }
    void DeleteOwnedProp(ref Prop p){if(p!=null){p.alive=false;deletes++;}p=null;}
    static float DistanceSquared(Vector3 a,Vector3 b)=> (a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){Game.Player.Character=new Ped{Position=new Vector3(0,0,6)};World.markers=0;a();passed++;Console.WriteLine("PASS #3 "+n);}
    static void Main(){
        Test("ready shipment spawns without any vehicle API or nearby assigned vehicle",()=>{var h=new Harness();h.UpdateWholesalePalletVisual();Check(h.creates==2&&h._supplierCrate!=null,"pallet and box missing");});
        Test("box rests on actual grounded pallet rather than saved Z",()=>{var h=new Harness();h.UpdateWholesalePalletVisual();Check(Math.Abs(h._supplierCrate.Position.Z-4.28f)<0.001,"box support height");});
        Test("refresh does not duplicate live cargo",()=>{var h=new Harness();for(int i=0;i<100;i++)h.UpdateWholesalePalletVisual();Check(h.creates==2,"duplicate props");});
        Test("stream out deletes warehouse-owned visuals and re-entry recreates one pair",()=>{var h=new Harness();h.UpdateWholesalePalletVisual();Game.Player.Character.Position=new Vector3(1000,0,6);h.UpdateWholesalePalletVisual();Check(h.deletes==2,"cleanup");Game.Player.Character.Position=new Vector3(0,0,6);h.UpdateWholesalePalletVisual();Check(h.creates==4,"re-entry");});
        Test("carried and loaded cargo are never deleted by warehouse streaming",()=>{var h=new Harness();h.UpdateWholesalePalletVisual();var box=h._supplierCrate;h._supplierPlayerCarrying=true;h.UpdateWholesalePalletVisual();Check(box.alive,"carried box deleted");h._supplierPlayerCarrying=false;h._wholesaleCargoLoaded=true;h.UpdateWholesalePalletVisual();Check(box.alive,"loaded box deleted");});
        Test("failed models recover in place after bounded batch cooldown",()=>{
            var h=new Harness{failModels=true};
            for(int i=0;i<5;i++){h._nextWarehouseVisualAttempt=DateTime.MinValue;h.UpdateWholesalePalletVisual();}
            Check(h.creates==10,"initial batch requests");
            for(int i=0;i<100;i++)h.UpdateWholesalePalletVisual();
            Check(h.creates==10,"cooldown request flood");
            h.failModels=false;h._nextWarehouseVisualAttempt=DateTime.MinValue;
            h.UpdateWholesalePalletVisual();
            Check(h._supplierCrate!=null&&h._wholesaleWarehousePallet!=null,"stationary recovery still blocked after retry budget");
            Check(h._warehouseVisualAttempts==1,"batch did not renew");
        });
        Test("failed requests respect three-second cooldown",()=>{var h=new Harness{failModels=true};h.UpdateWholesalePalletVisual();for(int i=0;i<100;i++)h.UpdateWholesalePalletVisual();Check(h.creates==2,"request flood");});
        Test("pending ETA prevents spawning and ready marker is vehicle-independent",()=>{var h=new Harness{_deliveryRemainingGameSeconds=30};h.UpdateWholesalePalletVisual();Check(h.creates==0,"premature cargo");h.DrawWholesalePackageMarker();Check(World.markers==0,"premature marker");h._deliveryRemainingGameSeconds=0;h.DrawWholesalePackageMarker();Check(World.markers==1,"ready marker absent");});
        Test("non-wholesale shipment crate is not removed",()=>{var h=new Harness{_deliverySupplier=2,_supplierCrate=new Prop()};h.UpdateWholesalePalletVisual();Check(h._supplierCrate.alive&&h.creates==0,"express cargo modified");});
        Test("five-attempt batch admits no retry before thirty-second cooldown expires",()=>{
            var h=new Harness();var now=new DateTime(2026,10,22,0,0,0,DateTimeKind.Utc);
            for(int i=0;i<5;i++)Check(h.TryBeginWarehouseVisualAttempt(now.AddSeconds(3*i)),"batch attempt "+i);
            Check(!h.TryBeginWarehouseVisualAttempt(now.AddSeconds(41)),"retry before long cooldown");
            Check(h._warehouseVisualAttempts==5,"denied call changed budget");
            Check(h.TryBeginWarehouseVisualAttempt(now.AddSeconds(42))&&h._warehouseVisualAttempts==1,"automatic budget renewal");
            Check(!h.TryBeginWarehouseVisualAttempt(now.AddSeconds(44)),"new batch spacing");
        });
        Test("persistent failures remain rate bounded across repeated batches",()=>{
            var h=new Harness();var now=new DateTime(2026,10,22,0,0,0,DateTimeKind.Utc);int attempts=0;
            for(int second=0;second<180;second++)
                if(h.TryBeginWarehouseVisualAttempt(now.AddSeconds(second)))attempts++;
            Check(attempts==24,"unexpected retry rate "+attempts);
        });
        Test("box-only model failure recovers while retaining its existing grounded pallet",()=>{
            var h=new Harness{failBoxes=true};
            for(int i=0;i<5;i++){h._nextWarehouseVisualAttempt=DateTime.MinValue;h.UpdateWholesalePalletVisual();}
            var pallet=h._wholesaleWarehousePallet;
            Check(pallet!=null&&h._supplierCrate==null&&h.creates==6,"partial model setup");
            h.failBoxes=false;h._nextWarehouseVisualAttempt=DateTime.MinValue;h.UpdateWholesalePalletVisual();
            Check(h._wholesaleWarehousePallet==pallet&&pallet.alive&&h._supplierCrate!=null&&h.creates==7,"pallet duplicated or box failed recovery");
        });
        Test("ready marker remains available during failed-model cooldown",()=>{
            var h=new Harness{failModels=true};
            for(int i=0;i<5;i++){h._nextWarehouseVisualAttempt=DateTime.MinValue;h.UpdateWholesalePalletVisual();}
            h.DrawWholesalePackageMarker();Check(World.markers==1&&h.creates==10,"cooldown removed locator");
        });
        Test("successful live pair performs no new attempts after timer expiry",()=>{
            var h=new Harness();h.UpdateWholesalePalletVisual();
            for(int i=0;i<100;i++){h._nextWarehouseVisualAttempt=DateTime.MinValue;h.UpdateWholesalePalletVisual();}
            Check(h.creates==2&&h._warehouseVisualAttempts==1,"success caused ongoing requests");
        });
        Console.WriteLine($"#3 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
