using System;
using System.Collections.Generic;
using System.Linq;
namespace GTA.Native {
 enum Hash {TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,TASK_LEAVE_VEHICLE,TASK_VEHICLE_PARK}
 static class Function {internal static List<Tuple<Hash,object[]>> calls=new List<Tuple<Hash,object[]>>();internal static void Call(Hash h,params object[] args){calls.Add(Tuple.Create(h,args));}}
}
struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
class Ped {internal int Handle=11;}
class Vehicle {internal int Handle=22;internal Vector3 Position;internal float Speed,Heading;}
class Harness {
 /*<source:CustomerStage>*/
 /*<source:HandoffProgress>*/
 /*<source:HandoffZoneAvailable>*/
 /*<source:IssueDirectionFreeHandoff>*/
 /*<source:BeginDirectionFreeHandoff>*/
 /*<source:UpdateDirectionFreeHandoff>*/
 /*<source:UpdateVehicleApproach>*/
 /*<source:UpdateVehicleParking>*/
 class WorkBay {internal bool IsSet=true;internal Vector3 Position;internal float Heading=180;}
 class CustomerOrder {internal int Id=1;internal Ped Customer=new Ped();internal Vehicle Vehicle=new Vehicle();internal CustomerStage Stage;internal double SecondsInStage,DriveNoProgressSeconds;internal int ParkingRetries,DriveRetryCount;internal Vector3 LastDrivePosition;internal bool AutomaticLot,ParkingTaskIssued,Paid;internal HandoffProgress Handoff;}
 WorkBay _customerDropoff=new WorkBay();List<CustomerOrder> _activeCustomerOrders=new List<CustomerOrder>();
 bool occupied,workerMissing;int cancelled,failures;int _parkingTimeoutSeconds=30,_vehicleTransferTimeout=90;long cash=123,stock=42,invoices=7;
 const int CarefulDrivingStyle=786603,RecoveryDrivingStyle=786607;
 static float DistanceSquared(Vector3 a,Vector3 b){float x=a.X-b.X,y=a.Y-b.Y,z=a.Z-b.Z;return x*x+y*y+z*z;}
 bool DropoffOccupied(Vehicle except)=>occupied;
 Ped AssignedWorker(CustomerOrder order)=>workerMissing?null:order.Customer;
 bool MechanicInCustomerCar(CustomerOrder o)=>!workerMissing;
 WorkBay VehicleDestination(CustomerOrder o,bool bay)=>bay?new WorkBay{Position=new Vector3(50,0,0),Heading=90}:_customerDropoff;
 void FailVehicleTransfer(CustomerOrder o,bool bay,string why){failures++;if(o.Paid)o.Stage=CustomerStage.PaidReturnRecovery;}
 void CancelCustomerOrder(string why,bool show){cancelled++;}
 void BeginCustomerExit(CustomerOrder o){GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE,o.Customer.Handle,o.Vehicle.Handle,0);o.Stage=CustomerStage.ExitingVehicle;o.SecondsInStage=0;}
 void Notify(string s){}void Log(string s,Exception e){}
 static int passed;
 static void Check(bool b,string why){if(!b)throw new Exception(why);}
 static void Test(string name,Action body){GTA.Native.Function.calls.Clear();body();passed++;Console.WriteLine("PASS "+name);}
 static void Main(){
 Test("intake and paid return accept opposite and cardinal headings",()=>{foreach(bool returning in new[]{false,true})foreach(float heading in new[]{0f,90f,180f,270f}){var h=new Harness();var o=new CustomerOrder{Paid=returning};o.Vehicle.Heading=heading;h.BeginDirectionFreeHandoff(o,o.Customer,returning);h.UpdateDirectionFreeHandoff(o,returning);Check(o.Stage==(returning?CustomerStage.MechanicParkingAtDropoff:CustomerStage.ParkingAtDropoff),"exited first sample");h.UpdateDirectionFreeHandoff(o,returning);Check(o.Stage==(returning?CustomerStage.MechanicExitingAtDropoff:CustomerStage.ExitingVehicle)&&o.Vehicle.Heading==heading,"heading gated/changed");}});
 Test("slow approach uses position-only native once",()=>{var h=new Harness();var o=new CustomerOrder();h.BeginDirectionFreeHandoff(o,o.Customer,false);var call=GTA.Native.Function.calls.Single();Check(call.Item1==GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE&&call.Item2.Length==8&&(float)call.Item2[5]==2.4f&&(float)call.Item2[7]==1.75f,"not position-only");});
 Test("fast vehicle cannot exit even inside zone",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Speed=3;h.BeginDirectionFreeHandoff(o,o.Customer,false);for(int i=0;i<10;i++)h.UpdateDirectionFreeHandoff(o,false);Check(!GTA.Native.Function.calls.Any(x=>x.Item1==GTA.Native.Hash.TASK_LEAVE_VEHICLE),"rolling exit");});
 Test("nonfinite and negative speeds cannot exit",()=>{foreach(float speed in new[]{float.NaN,float.PositiveInfinity,-1f}){var h=new Harness();var o=new CustomerOrder();o.Vehicle.Speed=speed;h.BeginDirectionFreeHandoff(o,o.Customer,false);for(int i=0;i<3;i++)h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ParkingAtDropoff,"bad speed");}});
 Test("outside acceptance radius never forces an exit",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=new Vector3(5,0,0);h.BeginDirectionFreeHandoff(o,o.Customer,false);h.UpdateDirectionFreeHandoff(o,false);h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ParkingAtDropoff,"outside exit");});
 Test("occupied zone delays task and exit until genuinely clear",()=>{var h=new Harness{occupied=true};var o=new CustomerOrder();h.BeginDirectionFreeHandoff(o,o.Customer,false);for(int i=0;i<5;i++)h.UpdateDirectionFreeHandoff(o,false);Check(GTA.Native.Function.calls.Count==0,"occupied task");h.occupied=false;h.UpdateDirectionFreeHandoff(o,false);h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ExitingVehicle,"not recovered");});
 Test("obstruction resets consecutive stop samples",()=>{var h=new Harness();var o=new CustomerOrder();h.BeginDirectionFreeHandoff(o,o.Customer,false);h.UpdateDirectionFreeHandoff(o,false);h.occupied=true;h.UpdateDirectionFreeHandoff(o,false);h.occupied=false;h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ParkingAtDropoff,"stale stop sample");h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ExitingVehicle,"clear stop");});
 Test("no progress gets exactly one direction-free retry",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=new Vector3(10,0,0);h.BeginDirectionFreeHandoff(o,o.Customer,false);for(int i=0;i<100;i++)h.UpdateDirectionFreeHandoff(o,false);Check(o.Handoff.Retries==1&&GTA.Native.Function.calls.Count==2&&GTA.Native.Function.calls.All(x=>x.Item1==GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE),"retry spam/heading task");});
 Test("progress resets no-progress counter",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=new Vector3(20,0,0);h.BeginDirectionFreeHandoff(o,o.Customer,false);for(int i=0;i<12;i++)h.UpdateDirectionFreeHandoff(o,false);o.Vehicle.Position=new Vector3(18,0,0);h.UpdateDirectionFreeHandoff(o,false);Check(o.Handoff.NoProgress==0&&o.Handoff.Retries==0,"progress ignored");});
 Test("intake timeout cancels without forcing exit",()=>{var h=new Harness();var o=new CustomerOrder{SecondsInStage=31};o.Vehicle.Position=new Vector3(10,0,0);h.UpdateDirectionFreeHandoff(o,false);Check(h.cancelled==1&&!GTA.Native.Function.calls.Any(x=>x.Item1==GTA.Native.Hash.TASK_LEAVE_VEHICLE),"timeout");});
 Test("paid return timeout retains existing recovery and ledgers",()=>{var h=new Harness();var o=new CustomerOrder{Paid=true,SecondsInStage=46};o.Vehicle.Position=new Vector3(10,0,0);h.UpdateDirectionFreeHandoff(o,true);Check(h.failures==1&&o.Paid&&o.Stage==CustomerStage.PaidReturnRecovery&&h.cash==123&&h.stock==42&&h.invoices==7,"paid lost or billed");});
 Test("missing worker follows failure without vehicle task",()=>{var h=new Harness{workerMissing=true};h.UpdateDirectionFreeHandoff(new CustomerOrder{Paid=true},true);Check(h.failures==1&&GTA.Native.Function.calls.Count==0,"missing worker");});
 Test("return approach dispatches no directional park task",()=>{var h=new Harness();var o=new CustomerOrder{Paid=true};h._activeCustomerOrders.Add(o);h.UpdateVehicleApproach(o,false);Check(o.Stage==CustomerStage.MechanicParkingAtDropoff&&GTA.Native.Function.calls.Single().Item1==GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE,"return park");});
 Test("service bay approach retains saved-facing park native",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=new Vector3(49,0,0);h.UpdateVehicleApproach(o,true);var call=GTA.Native.Function.calls.Single();Check(call.Item1==GTA.Native.Hash.TASK_VEHICLE_PARK&&(float)call.Item2[5]==90&&o.Stage==CustomerStage.MechanicParkingAtBay,"bay changed");});
 Test("service bay parking exit remains one task",()=>{var h=new Harness();var o=new CustomerOrder();o.Vehicle.Position=new Vector3(50,0,0);h.UpdateVehicleParking(o,true);Check(o.Stage==CustomerStage.MechanicExitingAtBay&&GTA.Native.Function.calls.Single().Item1==GTA.Native.Hash.TASK_LEAVE_VEHICLE,"bay exit");});
 Test("service bay retry retains saved heading and original bounded counter",()=>{var h=new Harness();var o=new CustomerOrder{SecondsInStage=60};h.UpdateVehicleParking(o,true);var call=GTA.Native.Function.calls.Single();Check(o.ParkingRetries==1&&call.Item1==GTA.Native.Hash.TASK_VEHICLE_PARK&&(float)call.Item2[5]==90,"bay retry");});
 Test("unset or invalid dropoff cannot start task",()=>{var h=new Harness();h._customerDropoff.IsSet=false;var o=new CustomerOrder();h.BeginDirectionFreeHandoff(o,o.Customer,false);Check(GTA.Native.Function.calls.Count==0,"unset zone");h._customerDropoff.IsSet=true;h._customerDropoff.Position=new Vector3(float.NaN,0,0);h.UpdateDirectionFreeHandoff(o,false);Check(GTA.Native.Function.calls.Count==0,"invalid zone");});
 Test("saved heading is not needed by direction-free zone",()=>{var h=new Harness();h._customerDropoff.Heading=float.NaN;var o=new CustomerOrder();h.BeginDirectionFreeHandoff(o,o.Customer,false);h.UpdateDirectionFreeHandoff(o,false);h.UpdateDirectionFreeHandoff(o,false);Check(o.Stage==CustomerStage.ExitingVehicle,"heading consulted");});
 Console.WriteLine($"parking37 {passed}/{passed} PASS; real direction-free helpers and bay branches with deterministic native adapters; GTA NOT RUN");
 }
}
