using System;
using System.Collections.Generic;
struct Vector3 { internal float X,Y,Z; internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;} }
class Ped { internal Vector3 Position;internal bool IsDead;internal bool Exists()=>true; }
class Prop { internal bool alive=true;internal bool Exists()=>alive; }
class PlayerState { internal Ped Character=new Ped{Position=new Vector3(10,20,0)}; }
static class Game { internal static PlayerState Player=new PlayerState(); }
class Harness {
    class WorkBay { internal bool IsSet=true;internal Vector3 Position; }
    /*<source:BeginSupplierDriverWalk>*/
    /*<source:UpdateSupplierPlayerApproach>*/
    /*<source:DeferSupplierPlayerHandoff>*/
    List<Vector3> _supplierWalkingNodes=new List<Vector3>();
    WorkBay _supplierPartsPallet=new WorkBay(),_supplierHandoff=new WorkBay{Position=new Vector3(99,99,0)};
    WorkBay[] _supplierDriverWaypoints={new WorkBay{Position=new Vector3(1,1,0)},new WorkBay{IsSet=false},new WorkBay{Position=new Vector3(3,3,0)}};
    bool _supplierDirectToPlayer,_supplierBoxAttachedToDriver=true;
    DateTime _supplierPlayerApproachDeadline,_nextSupplierPlayerRepath,_nextSupplierVisualRetryUtc,_supplierStageAt;
    int _supplierPlayerRepaths,_supplierRouteIndex,_supplierVisualStage;
    Ped _supplierDriver=new Ped();Prop _supplierCrate=new Prop();
    int tasks,deferred,quantity=10,stock=20;long invoice=0;
    Vector3 target;
    void TaskSupplierWalkingNode(){tasks++;target=_supplierWalkingNodes[_supplierRouteIndex];}
    void DeleteSupplierVisuals(){deferred++;_supplierVisualStage=0;_supplierDirectToPlayer=false;}
    void Log(string c,Exception e){}
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){Game.Player.Character=new Ped{Position=new Vector3(10,20,0)};a();passed++;Console.WriteLine("PASS #5 "+n);}
    static DateTime Now=>new DateTime(2026,10,22,0,0,0,DateTimeKind.Utc);
    static void Main(){
        Test("player handoff bypasses saved waypoints and fixed handoff",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);Check(h._supplierWalkingNodes.Count==1&&h.target.X==10&&h.target.Y==20,"direct target");});
        Test("regular drop-off retains configured stops in order",()=>{var h=new Harness();h._supplierPartsPallet.IsSet=false;h.BeginSupplierDriverWalk(Now);Check(!h._supplierDirectToPlayer&&h._supplierWalkingNodes.Count==3,"regular nodes");Check(h._supplierWalkingNodes[0].X==1&&h._supplierWalkingNodes[1].X==3&&h._supplierWalkingNodes[2].X==99,"route order");});
        Test("moving player repaths no more frequently than three seconds",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);Game.Player.Character.Position=new Vector3(30,0,0);h.UpdateSupplierPlayerApproach(Now.AddSeconds(1));Check(h.tasks==1,"early native task");h.UpdateSupplierPlayerApproach(Now.AddSeconds(3));Check(h.tasks==2&&h.target.X==30,"moving target");});
        Test("driver arrival waits with same crate and can follow player moving away",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);h._supplierDriver.Position=Game.Player.Character.Position;var box=h._supplierCrate;h.UpdateSupplierPlayerApproach(Now.AddSeconds(4));Check(h._supplierVisualStage==4&&h._supplierCrate==box&&h.quantity==10&&h.stock==20&&h.invoice==0,"premature stock or replaced crate");Game.Player.Character.Position=new Vector3(30,20,0);h.UpdateSupplierPlayerApproach(Now.AddSeconds(7));Check(h._supplierVisualStage==3&&h.target.X==30,"departed player not followed");});
        Test("90-second timeout preserves pending ledger and backs off visuals",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);h.UpdateSupplierPlayerApproach(Now.AddSeconds(91));Check(h.deferred==1&&h.quantity==10&&h.stock==20&&h.invoice==0,"timeout discarded/credited order");Check(h._nextSupplierVisualRetryUtc==Now.AddSeconds(121),"retry backoff");});
        Test("repath count is bounded",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);for(int i=1;i<=16;i++)h.UpdateSupplierPlayerApproach(Now.AddSeconds(i*3));Check(h.tasks==16&&h.deferred==1,"unbounded chase");});
        Test("invalid player or lost cargo defers without credit",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);h._supplierCrate.alive=false;h.UpdateSupplierPlayerApproach(Now.AddSeconds(4));Check(h.deferred==1&&h.quantity==10&&h.invoice==0,"lost cargo recovery");});
        Test("waiting nearby after arrival never consumes retry budget or auto-commits",()=>{var h=new Harness();h.BeginSupplierDriverWalk(Now);h._supplierDriver.Position=Game.Player.Character.Position;for(int i=1;i<200;i++)h.UpdateSupplierPlayerApproach(Now.AddSeconds(i));Check(h.deferred==0&&h.tasks==1&&h.quantity==10&&h._supplierVisualStage==4,"handoff wait");});
        Console.WriteLine($"#5 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
