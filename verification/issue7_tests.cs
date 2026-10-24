using System;
using System.Collections.Generic;
namespace GTA.Native {enum Hash {TASK_GO_STRAIGHT_TO_COORD,TASK_FOLLOW_NAV_MESH_TO_COORD,CLEAR_PED_TASKS,TASK_START_SCENARIO_IN_PLACE,TASK_STAND_STILL}static class Function{internal static List<Hash> calls=new List<Hash>();internal static void Call(Hash h,params object[] a){calls.Add(h);}}}
struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
class Ped{internal Vector3 Position;internal float Heading;internal int Handle;internal bool Exists()=>true;}
class Harness{
    class WorkBay{internal bool IsSet=true;internal Vector3 Position;internal float Heading;}
    class Mechanic{
        internal int JobOutboundMask=31,JobReturnMask=31,ArrivalPhase,PathNodeIndex,RouteTeleports,RouteRetries,WorkMode=-1,BreakAnimation;
        internal bool HasJob,ReturningFromCustomerCar;internal List<Vector3> PathNodes=new List<Vector3>();internal Ped Worker=new Ped();
        internal DateTime RouteSince,LastProgressAt;internal float LastDistanceSquared;internal double BreakAnimationSeconds;
    }
    const int PhaseToCheckpoint=1,PhaseToBreak=2,PhaseAtBreak=3,PhaseToBay=4,PhaseWorking=5,PhaseReturningToBreak=6,PhaseRouteBlocked=7;
    /*<source:BuildRoute>*/
    void WarnEmptyWorkRoute(int slot,bool returning){}
    /*<source:StartRouteLeg>*/
    /*<source:WalkCurrentNode>*/
    /*<source:IssueEmployeeWalkTask>*/
    /*<source:UpdateEmployeeActivity>*/
    bool _customEmployeeWalkingPaths=true,_customMechanicJobRoutes,_pickupUsesArrivalExit=true;
    bool _smoothMechanicWaypoints=true,_breakAnimationsEnabled,_safeTeleportOnStall;
    int _stallSeconds=9,blocked;float _walkSpeed=1;
    string[] _breakScenarios={"idle"};
    WorkBay _walkCheckpoint=new WorkBay{Position=new Vector3(99,0,0)},_breakArea=new WorkBay();
    WorkBay[] _arrivalRoute={new WorkBay{Position=new Vector3(1,0,0)},new WorkBay{IsSet=false}},
        _serviceRoute={new WorkBay{Position=new Vector3(10,0,0)},new WorkBay{Position=new Vector3(20,0,0)},new WorkBay{IsSet=false}},
        _bayApproaches={new WorkBay{Position=new Vector3(30,0,0)}},_bays={new WorkBay{Position=new Vector3(40,0,0)}};
    List<Mechanic> _mechanics=new List<Mechanic>{new Mechanic()};
    Vector3 GetBreakSpot(int slot)=>new Vector3(0,0,0);
    int CountSet(WorkBay[] p){int n=0;foreach(var x in p)if(x.IsSet)n++;return n;}
    bool MechanicOnVehicleTask(int slot)=>false;
    bool RecoverBlockedWorker(Mechanic e,int slot,Vector3 p,DateTime now)=>false;
    void PauseBlockedWorker(Mechanic e,int slot,Vector3 p){blocked++;e.ArrivalPhase=PhaseRouteBlocked;}
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){GTA.Native.Function.calls.Clear();a();passed++;Console.WriteLine("PASS #7 "+n);}
    static void Main(){
        Test("default job paths bypass existing MLO stops and approaches",()=>{var h=new Harness();var go=h.BuildRoute(0,PhaseToBay);var back=h.BuildRoute(0,PhaseReturningToBreak);Check(go.Count==1&&go[0].X==40&&back.Count==1&&back[0].X==0,"default detour");Check(h._serviceRoute[0].IsSet&&h._mechanics[0].JobOutboundMask==31,"old config removed");});
        Test("advanced opt-in restores sparse outbound and reverse masked routes",()=>{var h=new Harness{_customMechanicJobRoutes=true};h._mechanics[0].JobOutboundMask=1;h._mechanics[0].JobReturnMask=2;var go=h.BuildRoute(0,PhaseToBay);var back=h.BuildRoute(0,PhaseReturningToBreak);Check(go.Count==3&&go[0].X==10&&go[1].X==30&&go[2].X==40,"outbound mask");Check(back.Count==3&&back[0].X==30&&back[1].X==20&&back[2].X==0,"return mask");});
        Test("arrival remains custom without duplicating legacy checkpoint at unset gaps",()=>{var h=new Harness();var route=h.BuildRoute(0,PhaseToCheckpoint);Check(route.Count==1&&route[0].X==1,"dangling legacy fallback");h._arrivalRoute[0].IsSet=false;route=h.BuildRoute(0,PhaseToCheckpoint);Check(route.Count==1&&route[0].X==99,"legacy fallback absent");});
        Test("direct job leg uses navmesh even with custom arrival mode",()=>{var h=new Harness();var e=h._mechanics[0];h.StartRouteLeg(e,0,PhaseToBay);Check(GTA.Native.Function.calls.Contains(GTA.Native.Hash.TASK_FOLLOW_NAV_MESH_TO_COORD)&&!GTA.Native.Function.calls.Contains(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD),"direct path forced through wall");});
        Test("mechanic moves break to working and back with finite transitions",()=>{var h=new Harness();var e=h._mechanics[0];e.ArrivalPhase=PhaseAtBreak;e.HasJob=true;var now=DateTime.UtcNow;h.UpdateEmployeeActivity(e,0,now);Check(e.ArrivalPhase==PhaseToBay&&e.PathNodes.Count==1,"job route");e.Worker.Position=h._bays[0].Position;h.UpdateEmployeeActivity(e,0,now.AddSeconds(1));Check(e.ArrivalPhase==PhaseWorking&&e.HasJob,"work transition");e.HasJob=false;h.UpdateEmployeeActivity(e,0,now.AddSeconds(2));Check(e.ArrivalPhase==PhaseReturningToBreak,"return transition");e.Worker.Position=new Vector3();h.UpdateEmployeeActivity(e,0,now.AddSeconds(3));Check(e.ArrivalPhase==PhaseAtBreak,"break completion");});
        Test("blocked direct route stops after bounded retries without losing job",()=>{var h=new Harness();var e=h._mechanics[0];e.HasJob=true;h.StartRouteLeg(e,0,PhaseToBay);var now=DateTime.UtcNow;h.UpdateEmployeeActivity(e,0,now.AddSeconds(10));h.UpdateEmployeeActivity(e,0,now.AddSeconds(20));h.UpdateEmployeeActivity(e,0,now.AddSeconds(30));for(int i=0;i<100;i++)h.UpdateEmployeeActivity(e,0,now.AddSeconds(40+i));Check(h.blocked==1&&e.HasJob&&e.ArrivalPhase==PhaseRouteBlocked,"retry loop/job loss");});
        Test("advanced job opt-in is independent from arrival mode",()=>{var h=new Harness{_customEmployeeWalkingPaths=false,_customMechanicJobRoutes=true};Check(h.BuildRoute(0,PhaseToBay).Count==4,"independent advanced setting");});
        Console.WriteLine($"#7 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
