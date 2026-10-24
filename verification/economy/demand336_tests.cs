using System;
namespace GTA.Native { enum Hash { GET_CLOCK_HOURS } static class Function {internal static int hour=12,calls;internal static T Call<T>(Hash h,params object[] a){calls++;return (T)(object)hour;}}}
static class Game {internal static bool IsPaused;}
class Harness {
/*<source:CustomerDemandInterval>*/
/*<source:AdvanceCustomerDemand>*/
/*<source:QueueDueCustomer>*/
/*<source:ScheduleCustomerRequest>*/
bool _bootGameplayActivated=true,_clockedIn=true,_customerServiceEnabled=true,_remoteMode;
double _customerDemandElapsed;int _customerDemandIntervalSeconds=150,_customerIntervalSeconds=150,_waitingCustomerRequests,capacity=3;
int EffectiveQueueCapacity()=>capacity;void Notify(string s){}
static int passed;static void Check(bool ok,string m){if(!ok)throw new Exception(m);}static void Test(string n,Action a){Game.IsPaused=false;GTA.Native.Function.hour=12;GTA.Native.Function.calls=0;a();passed++;Console.WriteLine("PASS demand "+n);}
void Tick(int seconds=1){for(int i=0;i<seconds;i++){AdvanceCustomerDemand(1);QueueDueCustomer(DateTime.UtcNow);}}
static void Main(){
Test("02:00 demand is quieter than day/commute",()=>{Check(CustomerDemandInterval(2,150)==600&&CustomerDemandInterval(12,150)==105&&CustomerDemandInterval(7,150)==128&&CustomerDemandInterval(19,150)==135,"profile");});
Test("all hours and config bounds stay positive and bounded",()=>{for(int hour=-24;hour<48;hour++)foreach(int baseline in new[]{-1,30,150,900,99999})Check(CustomerDemandInterval(hour,baseline)>=20&&CustomerDemandInterval(hour,baseline)<=3600,"interval bounds");});
Test("daytime produces more new requests for equal active gameplay",()=>{var day=new Harness();var night=new Harness();int d=0,n=0;for(int i=0;i<600;i++){GTA.Native.Function.hour=12;day.Tick();d+=day._waitingCustomerRequests;day._waitingCustomerRequests=0;GTA.Native.Function.hour=2;night.Tick();n+=night._waitingCustomerRequests;night._waitingCustomerRequests=0;}Check(d==5&&n==1,"frequency");});
Test("pause adds no progress and performs no GTA clock read",()=>{var h=new Harness();Game.IsPaused=true;h.Tick(600);Check(h._waitingCustomerRequests==0&&h._customerDemandElapsed==0&&GTA.Native.Function.calls==0,"paused");});
Test("closed/remote/startup demand does not accrue",()=>{foreach(int mode in new[]{0,1,2}){var h=new Harness();if(mode==0)h._clockedIn=false;if(mode==1)h._remoteMode=true;if(mode==2)h._bootGameplayActivated=false;h.Tick(600);Check(h._waitingCustomerRequests==0&&h._customerDemandElapsed==0,"inactive demand");}});
Test("wall-clock year jump cannot create catch-up request",()=>{var h=new Harness();h.QueueDueCustomer(DateTime.UtcNow.AddYears(10));Check(h._waitingCustomerRequests==0&&h._customerDemandElapsed==0,"offline date");});
Test("fresh restarted instance has no previous interval credit",()=>{var prior=new Harness();prior.Tick(100);var loaded=new Harness();loaded.QueueDueCustomer(DateTime.UtcNow.AddYears(1));Check(loaded._waitingCustomerRequests==0&&loaded._customerDemandElapsed==0,"restart");});
Test("queue remains bounded and existing requests remain",()=>{var h=new Harness{_waitingCustomerRequests=3};h.Tick(1200);Check(h._waitingCustomerRequests==3,"overflow/deletion");});
Test("GTA time change alone does not add demand without active progress",()=>{var h=new Harness();for(int i=0;i<100;i++){GTA.Native.Function.hour=i%24;h.QueueDueCustomer(DateTime.UtcNow);}Check(h._waitingCustomerRequests==0,"clock jump");});
Test("manual night request bypasses demand delay but obeys capacity",()=>{var h=new Harness{capacity=1};GTA.Native.Function.hour=2;h.ScheduleCustomerRequest();h.ScheduleCustomerRequest();Check(h._waitingCustomerRequests==1&&h._customerDemandElapsed==0,"manual");});
Test("paused manual scheduling is rejected",()=>{var h=new Harness();Game.IsPaused=true;h.ScheduleCustomerRequest();Check(h._waitingCustomerRequests==0,"paused manual");});
Test("large or invalid sample cannot synthesize offline progress",()=>{var h=new Harness();h.AdvanceCustomerDemand(999999);h.AdvanceCustomerDemand(double.NaN);h.AdvanceCustomerDemand(double.PositiveInfinity);h.AdvanceCustomerDemand(-5);Check(h._customerDemandElapsed==2&&h._waitingCustomerRequests==0,"sample clamp");});
Test("clock read is one per active visual update",()=>{var h=new Harness();h.Tick(100);Check(GTA.Native.Function.calls==100,"native bound");});
Console.WriteLine($"demand336 {passed}/{passed} PASS; production GTA-hour weighting and active-time scheduling, GTA NOT RUN");
}
}
