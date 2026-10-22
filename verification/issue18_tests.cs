using System;
using System.Collections.Generic;
class Harness {
    /*<source:ForensicProbe>*/
    /*<source:DiagnosticLog>*/
    static int passed;
    static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string name,Action action){action();passed++;Console.WriteLine("PASS #18 "+name);}
    static void Main(){
        Test("disabled probe schedules nothing across hours and lifecycle",()=>{
            int count=0;var p=new ForensicProbe(false,a=>{count++;a();return true;},s=>count++,"id");
            p.Lifecycle("start");for(int i=0;i<7200;i++)p.Observe(1,"idle",false,false,false,0,0,0);
            Check(count==0,"disabled I/O");});
        Test("stable idle emits initial state and 120-active-second heartbeats",()=>{
            var lines=new List<string>();var p=new ForensicProbe(true,a=>{a();return true;},lines.Add,"one");
            for(int i=0;i<1800;i++)p.Observe(1,"idle",false,false,false,7,0,0);
            Check(lines.Count==16 && lines.FindAll(s=>s.Contains("HEARTBEAT")).Count==15,"heartbeat rate");
            Check(lines[1].Contains("activeSeconds=120.0") && lines[1].Contains("instance=one"),"context");});
        Test("paused time does not advance heartbeat",()=>{
            var lines=new List<string>();var p=new ForensicProbe(true,a=>{a();return true;},lines.Add,"id");
            for(int i=0;i<10000;i++)p.Observe(0,"idle",false,false,false,7,0,0);
            Check(lines.Count==1,"paused heartbeat");});
        Test("invalid and giant samples cannot force catch-up",()=>{
            var lines=new List<string>();var p=new ForensicProbe(true,a=>{a();return true;},lines.Add,"id");
            foreach(double t in new[]{double.NaN,double.PositiveInfinity,-1d,100000d})p.Observe(t,"idle",false,false,false,7,0,0);
            Check(lines.Count==1,"invalid catch-up");});
        Test("transitions coalesce to newest managed state after ten active seconds",()=>{
            var lines=new List<string>();var p=new ForensicProbe(true,a=>{a();return true;},lines.Add,"id");
            for(int i=0;i<21;i++)p.Observe(1,"completed",i%2==0,false,false,i,2,3);
            Check(lines.Count==3&&lines[2].Contains("supplierStage=20")&&lines[2].Contains("completedPhase=completed"),"state throttle");});
        Test("blocked writer keeps one callback with bounded dropped accounting",()=>{
            var queue=new List<Action>();var lines=new List<string>();var p=new ForensicProbe(true,a=>{queue.Add(a);return true;},lines.Add,"id");
            for(int i=0;i<10000;i++)p.Lifecycle("transition");
            Check(queue.Count==1,"unbounded queue");queue[0]();p.Lifecycle("end");queue[1]();
            Check(lines.Count==2&&lines[1].Contains("dropped=9999"),"drop count");});
        Test("writer reentry is suppressed without recursion",()=>{
            int writes=0;ForensicProbe p=null;p=new ForensicProbe(true,a=>{a();return true;},s=>{writes++;p.Lifecycle("nested");},"id");
            p.Lifecycle("outer");p.Lifecycle("again");Check(writes==2,"recursive writer");});
        Test("writer failure releases capacity without escaping",()=>{
            int writes=0;var p=new ForensicProbe(true,a=>{a();return true;},s=>{writes++;throw new Exception();},"id");
            p.Lifecycle("one");p.Lifecycle("two");Check(writes==2,"capacity lost");});
        Test("queue refusal and exception recover capacity",()=>{
            int queues=0,writes=0;var p=new ForensicProbe(true,a=>{queues++;if(queues==1)return false;if(queues==2)throw new Exception();a();return true;},s=>writes++,"id");
            p.Lifecycle("one");p.Lifecycle("two");p.Lifecycle("three");Check(writes==1,"queue stuck");});
        Test("forensic writer retains thirty-second disk-failure backoff",()=>{
            var now=new DateTime(2026,10,22,0,0,0,DateTimeKind.Utc);int attempts=0;
            var logger=new DiagnosticLog(()=>now,s=>{attempts++;if(attempts==1)throw new Exception();});
            var p=new ForensicProbe(true,a=>{a();return true;},s=>logger.Write(s,null),"id");
            p.Lifecycle("start");for(int i=0;i<100;i++)p.Lifecycle("repeat");
            now=now.AddSeconds(29);p.Lifecycle("early");Check(attempts==1,"backoff lost");
            now=now.AddSeconds(1);p.Lifecycle("retry");Check(attempts==2,"backoff never recovers");});
        Test("reload instances carry distinct identity",()=>{
            var lines=new List<string>();new ForensicProbe(true,a=>{a();return true;},lines.Add,"first").Lifecycle("start");
            new ForensicProbe(true,a=>{a();return true;},lines.Add,"second").Lifecycle("start");
            Check(lines[0].Contains("instance=first")&&lines[1].Contains("instance=second"),"identity");});
        Console.WriteLine($"#18 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
