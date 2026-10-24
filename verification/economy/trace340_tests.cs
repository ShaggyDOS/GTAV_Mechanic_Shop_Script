using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

struct Vector3 {
 internal float X,Y,Z;
 internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}
 public static Vector3 operator -(Vector3 a,Vector3 b)=>new Vector3(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
 public static Vector3 operator *(Vector3 a,float b)=>new Vector3(a.X*b,a.Y*b,a.Z*b);
}
class Ped {internal int Handle=11;}
class Vehicle {internal int Handle=22;internal bool alive=true;internal Vector3 Position,ForwardVector=new Vector3(0,1,0);internal float Speed;internal bool Exists()=>alive;}
namespace GTA.Native {
 enum Hash { CLEAR_PED_TASKS,TASK_ENTER_VEHICLE,SET_DRIVER_ABILITY,SET_DRIVER_AGGRESSIVENESS,TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,IS_POSITION_OCCUPIED,TASK_VEHICLE_TEMP_ACTION,TASK_VEHICLE_PARK,TASK_LEAVE_VEHICLE }
 static class Function {
  internal static List<string> calls=new List<string>();internal static Hash? fail;internal static Exception error=new InvalidOperationException("native adapter failure");
  internal static void Call(Hash h,params object[] args){calls.Add(h.ToString());if(fail==h)throw error;}
  internal static T Call<T>(Hash h,params object[] args){Call(h,args);return default(T);}
 }
}
class Harness {
 /*<source:CustomerStage>*/
 /*<source:CustomerVehicleTraceState>*/
 /*<source:CustomerVehicleTrace>*/
 /*<source:DiagnosticLog>*/
 /*<source:CaptureCustomerVehicleTrace>*/
 /*<source:BeginCustomerVehicleTrace>*/
 /*<source:EndCustomerVehicleTrace>*/
 /*<source:AppendCustomerVehicleDiagnosticLine>*/
 /*<source:BeginVehicleDrive>*/
 /*<source:BeginEnterVehicle>*/
 /*<source:StartValetDeparture>*/
 /*<source:UpdateVehicleApproach>*/
 /*<source:UpdateVehicleParking>*/
 class Mechanic {internal Ped Worker=new Ped();internal int ArrivalPhase=4;internal string Name="Mechanic 1";}
 class WorkBay {internal Vector3 Position=new Vector3(10,0,0);internal float Heading=90;}
 class CustomerOrder {
  internal int Id=1,MechanicSlot,VehicleWalkIndex,DriveRetryCount,ParkingRetries;
  internal CustomerStage Stage=CustomerStage.MechanicEnteringVehicle;internal Vehicle Vehicle=new Vehicle();
  internal bool Paid,VehicleHandoff=true,AutomaticLot,VehicleTeleport;internal List<Vector3> VehicleWalkPath=new List<Vector3>();
  internal double SecondsInStage,DriveNoProgressSeconds;internal Vector3 LastDrivePosition;
 }
 static string DataDirectory=Path.Combine(Path.GetTempPath(),"gh-rc2-io-"+Guid.NewGuid().ToString("N"));
 List<Mechanic> _mechanics=new List<Mechanic>{new Mechanic()};List<CustomerOrder> _activeCustomerOrders=new List<CustomerOrder>();
 bool _bootGameplayActivated=true,_vehicleTransfersEnabled=true,_valetReverseOut,blocked;
 float _vehicleTransferSpeed=6;int _vehicleTransferTimeout=80,_parkingTimeoutSeconds=30;
 const int CarefulDrivingStyle=1,RecoveryDrivingStyle=2;
 CustomerVehicleTrace _customerVehicleTrace;List<string> lines=new List<string>();DateTime now=new DateTime(2026,10,24,20,0,0,DateTimeKind.Utc);
 int failed,cancelled;WorkBay target=new WorkBay();
 Harness(){_customerVehicleTrace=new CustomerVehicleTrace(()=>now,line=>{lines.Add(line);return true;});}
 Ped AssignedWorker(CustomerOrder o)=>o.MechanicSlot>=0&&o.MechanicSlot<_mechanics.Count?_mechanics[o.MechanicSlot].Worker:null;
 bool MechanicInCustomerCar(CustomerOrder o)=>AssignedWorker(o)!=null;
 WorkBay VehicleDestination(CustomerOrder o,bool toBay)=>target;
 bool IsAutoLotParkingBlocked(CustomerOrder o)=>blocked;
 void FailVehicleTransfer(CustomerOrder o,bool bay,string reason){failed++;}
 void CancelCustomerOrder(string why,bool show){cancelled++;}
 void Notify(string s){} void Log(string s,Exception e){}
 static float DistanceSquared(Vector3 a,Vector3 b){float x=a.X-b.X,y=a.Y-b.Y,z=a.Z-b.Z;return x*x+y*y+z*z;}
 static int passed;
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Test(string name,Action body){GTA.Native.Function.calls.Clear();GTA.Native.Function.fail=null;body();passed++;Console.WriteLine("PASS "+name);}
 static CustomerVehicleTraceState State(int id=1)=>new CustomerVehicleTraceState{Order=id,Slot=0,Stage=CustomerStage.MechanicDrivingToBay,Vehicle=22,Worker=11,Mode=18};
 static void Pair(CustomerVehicleTrace trace,int op,CustomerVehicleTraceState s){long token=trace.Begin(op,s);trace.End(token,s);}
 static void Main(){
 Test("entry and exit correlate sequence, session, mechanic and stage",()=>{var h=new Harness();var s=State();Pair(h._customerVehicleTrace,5,s);Check(h.lines.Count==2&&h.lines[0].Contains("ENTER seq=1 op=BeginVehicleDrive depth=1")&&h.lines[1].Contains("EXIT seq=1")&&h.lines.All(x=>x.Contains("mechanic=1 stage=MechanicDrivingToBay vehicle=22 worker=11")),"breadcrumb");});
 Test("10000 stable outer updates cause no extra clock reads or writes",()=>{int clock=0,writes=0;var t=new CustomerVehicleTrace(()=>{clock++;return DateTime.UtcNow;},s=>{writes++;return true;});for(int i=0;i<10000;i++)Pair(t,0,State());Check(clock==1&&writes==2,"idle flood");});
 Test("stable ordinary nested calls do not become a heartbeat",()=>{var h=new Harness();var s=State();for(int i=0;i<10000;i++){long outer=h._customerVehicleTrace.Begin(0,s);Pair(h._customerVehicleTrace,6,s);h._customerVehicleTrace.End(outer,s);}Check(h.lines.Count==4,"nested heartbeat");});
 Test("three independent order identities retain separate snapshots",()=>{var h=new Harness();for(int i=0;i<100;i++)for(int id=1;id<=3;id++)Pair(h._customerVehicleTrace,0,State(id));Check(h.lines.Count==6,"orders coalesced or flooded");});
 Test("stage changes are recorded and exit can show next stage",()=>{var h=new Harness();var s=State();long token=h._customerVehicleTrace.Begin(0,s);s.Stage=CustomerStage.MechanicParkingAtBay;h._customerVehicleTrace.End(token,s);Pair(h._customerVehicleTrace,0,s);Check(h.lines.Count==4&&h.lines[1].Contains("stage=MechanicParkingAtBay"),"stage not observed");});
 Test("retry counters and route node changes are observable",()=>{var h=new Harness();var s=State();Pair(h._customerVehicleTrace,6,s);s.DriveRetries=1;Pair(h._customerVehicleTrace,6,s);s.WalkNode=2;Pair(h._customerVehicleTrace,6,s);Check(h.lines.Count==6&&h.lines[4].Contains("walk=2/0 driveRetries=1"),"retry state");});
 Test("changed-state flood is bounded to twelve pairs per ten seconds",()=>{var h=new Harness();for(int i=0;i<1000;i++)Pair(h._customerVehicleTrace,0,State(i));Check(h.lines.Count==24,"budget exceeded");});
 Test("time window recovers and reports suppressed changes",()=>{var h=new Harness();for(int i=0;i<100;i++)Pair(h._customerVehicleTrace,0,State(i));h.now=h.now.AddSeconds(10);Pair(h._customerVehicleTrace,0,State(200));Check(h.lines.Count==26&&h.lines[24].Contains("suppressed=88"),"no recovery");});
 Test("backward logging clock recovers without touching game time",()=>{var h=new Harness();Pair(h._customerVehicleTrace,0,State());h.now=h.now.AddDays(-1);Pair(h._customerVehicleTrace,0,State(2));Check(h.lines.Count==4,"clock rollback");});
 Test("false-returning storage does not commit snapshots or spin",()=>{int writes=0;DateTime now=DateTime.UtcNow;var t=new CustomerVehicleTrace(()=>now,s=>{writes++;return false;});for(int i=0;i<100;i++)Pair(t,0,State());Check(writes==12,"failure budget");now=now.AddSeconds(10);Pair(t,0,State());Check(writes==13,"failed state incorrectly cached");});
 Test("throwing writer cannot escape into gameplay",()=>{var t=new CustomerVehicleTrace(()=>DateTime.UtcNow,s=>throw new IOException("disk"));Pair(t,5,State());});
 Test("throwing logging clock cannot escape into gameplay",()=>{var t=new CustomerVehicleTrace(()=>throw new Exception("clock"),s=>true);Pair(t,5,State());});
 Test("writer reentry is rejected without recursive diagnostics",()=>{CustomerVehicleTrace t=null;int writes=0;t=new CustomerVehicleTrace(()=>DateTime.UtcNow,s=>{writes++;Check(t.Begin(0,State())==-1,"reentry");t.End(-1,State());return true;});Pair(t,0,State());Check(writes==2,"writer recursion");});
 Test("exit failure still balances later depth",()=>{int writes=0;var lines=new List<string>();var t=new CustomerVehicleTrace(()=>DateTime.UtcNow,s=>{writes++;if(writes==2)throw new IOException();lines.Add(s);return true;});Pair(t,0,State());Pair(t,0,State(2));Check(lines.Last().Contains("depth=1"),"depth leaked");});
 Test("recursive same-operation growth is visible but output stays bounded",()=>{var h=new Harness();var tokens=new List<long>();for(int i=0;i<50;i++)tokens.Add(h._customerVehicleTrace.Begin(0,State()));for(int i=49;i>=0;i--)h._customerVehicleTrace.End(tokens[i],State());Check(h.lines.Count==24&&h.lines.Any(x=>x.Contains("depth=12")),"depth evidence/bound");h.now=h.now.AddSeconds(10);Pair(h._customerVehicleTrace,0,State(3));Check(h.lines.Last().Contains("depth=1"),"unwind");});
 Test("invalid operation is ignored and scope remains balanced",()=>{var h=new Harness();Pair(h._customerVehicleTrace,-2,State());Pair(h._customerVehicleTrace,5,State());Check(h.lines.Count==2&&h.lines.Last().Contains("depth=1"),"invalid op");});
 Test("boot gate and null snapshot add no natives or disk writes",()=>{var h=new Harness{_bootGameplayActivated=false};long token=h.BeginCustomerVehicleTrace(null,0);h.EndCustomerVehicleTrace(null,token);Check(token==-1&&h.lines.Count==0&&GTA.Native.Function.calls.Count==0,"boot");});
 Test("capture reads handles and flags without entity validation or mutation",()=>{var h=new Harness{_valetReverseOut=true};var o=new CustomerOrder{Paid=true,AutomaticLot=true,VehicleTeleport=true,VehicleWalkIndex=3,DriveRetryCount=1};var s=h.CaptureCustomerVehicleTrace(o);Check(s.Mode==63&&s.Slot==0&&s.Worker==11&&s.WalkNode==3&&GTA.Native.Function.calls.Count==0&&o.Vehicle.Position.X==0,"snapshot");o.MechanicSlot=99;Check(h.CaptureCustomerVehicleTrace(o).Worker==0&&h.CaptureCustomerVehicleTrace(null).Order==-1,"invalid refs");});
 Test("outbound drive task order and stage unchanged",()=>{var h=new Harness();var o=new CustomerOrder();h.BeginVehicleDrive(o,true);Check(string.Join(",",GTA.Native.Function.calls)=="SET_DRIVER_ABILITY,SET_DRIVER_AGGRESSIVENESS,TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE"&&o.Stage==CustomerStage.MechanicDrivingToBay&&h.lines.Count==2,"drive changed");});
 Test("return drive preserves return stage",()=>{var h=new Harness();var o=new CustomerOrder{Paid=true};h.BeginVehicleDrive(o,false);Check(o.Stage==CustomerStage.MechanicDrivingToDropoff&&h.lines.Last().Contains("MechanicDrivingToDropoff"),"return");});
 Test("missing worker retains early-return failure behavior",()=>{var h=new Harness();var o=new CustomerOrder{MechanicSlot=-1};h.BeginVehicleDrive(o,true);Check(h.failed==1&&GTA.Native.Function.calls.Count==0&&h.lines.Count==2,"early return");});
 Test("occupied auto lot retains cancellation and skips drive task",()=>{var h=new Harness{blocked=true};h.BeginVehicleDrive(new CustomerOrder{AutomaticLot=true},true);Check(h.cancelled==1&&!GTA.Native.Function.calls.Contains("TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE")&&h.lines.Count==2,"blocked");});
 Test("native exception propagates unchanged with finally breadcrumb",()=>{var h=new Harness();var o=new CustomerOrder();GTA.Native.Function.fail=GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE;Exception caught=null;try{h.BeginVehicleDrive(o,true);}catch(Exception e){caught=e;}Check(ReferenceEquals(caught,GTA.Native.Function.error)&&h.lines.Count==2&&o.Stage==CustomerStage.MechanicEnteringVehicle,"exception swallowed or state changed");});
 Test("diagnostic storage failure does not prevent drive native",()=>{var h=new Harness();h._customerVehicleTrace=new CustomerVehicleTrace(()=>h.now,s=>throw new IOException());var o=new CustomerOrder();h.BeginVehicleDrive(o,true);Check(o.Stage==CustomerStage.MechanicDrivingToBay&&GTA.Native.Function.calls.Count==3,"logger changed AI");});
 Test("enter vehicle preserves task order and outbound stage",()=>{var h=new Harness();var o=new CustomerOrder();h.BeginEnterVehicle(o,false);Check(string.Join(",",GTA.Native.Function.calls)=="CLEAR_PED_TASKS,TASK_ENTER_VEHICLE"&&o.Stage==CustomerStage.MechanicEnteringVehicle&&h.lines.Count==2,"enter");});
 Test("reverse-out uses original single temp task and stage",()=>{var h=new Harness{_valetReverseOut=true};var o=new CustomerOrder();h.StartValetDeparture(o,true);Check(o.Stage==CustomerStage.MechanicReversingFromDropoff&&GTA.Native.Function.calls.Count(x=>x=="TASK_VEHICLE_TEMP_ACTION")==1&&!GTA.Native.Function.calls.Contains("TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE"),"reverse");});
 Test("approach issues existing park task and next stage",()=>{var h=new Harness();var o=new CustomerOrder();h._activeCustomerOrders.Add(o);h.UpdateVehicleApproach(o,true);Check(o.Stage==CustomerStage.MechanicParkingAtBay&&GTA.Native.Function.calls.SequenceEqual(new[]{"TASK_VEHICLE_PARK"}),"approach");});
 Test("parking exit preserves direction and exactly one leave task",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=h.target.Position;h.UpdateVehicleParking(o,false);Check(o.Stage==CustomerStage.MechanicExitingAtDropoff&&GTA.Native.Function.calls.SequenceEqual(new[]{"TASK_LEAVE_VEHICLE"}),"park exit");});
 Test("parking retry remains bounded by existing retry counter",()=>{var h=new Harness();h.target.Position=new Vector3(100,0,0);var o=new CustomerOrder{SecondsInStage=100};h.UpdateVehicleParking(o,true);o.SecondsInStage=100;h.UpdateVehicleParking(o,true);Check(o.ParkingRetries==1&&h.failed==1&&GTA.Native.Function.calls.Count(x=>x=="TASK_VEHICLE_PARK")==1,"parking retry");});
 Test("rotation touches only diagnostic files and bounds UTF8 size",()=>{try{Directory.CreateDirectory(DataDirectory);string save=Path.Combine(DataDirectory,"Save.ini"),settings=Path.Combine(DataDirectory,"Settings.ini"),path=Path.Combine(DataDirectory,"CustomerVehicle.RC2.log");File.WriteAllText(save,"money=123\n");File.WriteAllText(settings,"setting=unchanged\n");for(int i=0;i<180;i++)AppendCustomerVehicleDiagnosticLine(new string('\u00e9',1500));Check(new FileInfo(path).Length<=128*1024&&new FileInfo(path+".previous").Length<=128*1024&&Directory.GetFiles(DataDirectory).Length==4&&File.ReadAllText(save)=="money=123\n"&&File.ReadAllText(settings)=="setting=unchanged\n","file bounds/save");}finally{Directory.Delete(DataDirectory,true);}});
 Test("existing safe writer backs off failures for thirty seconds",()=>{DateTime now=DateTime.UtcNow;int attempts=0;var log=new DiagnosticLog(()=>now,s=>{attempts++;throw new IOException();});Check(!log.Write("vehicle",null),"failure result");for(int i=0;i<100;i++)log.Write("vehicle",null);Check(attempts==1,"failure spin");now=now.AddSeconds(30);log.Write("vehicle",null);Check(attempts==2,"no retry");});
 Console.WriteLine($"trace340 {passed}/{passed} PASS; actual extracted trace/scopes/native order and synthetic file rotation; GTA NOT RUN");
 }
}
