using System;
using System.Collections.Generic;
class Harness {
 /*<source:WaypointTrace>*/
 /*<source:MenuPage>*/
 /*<source:OpenPointEditor>*/
 /*<source:SaveEditorPoint>*/
 /*<source:ExecutePointEditorSelection>*/
 class WorkBay {internal bool IsSet;internal string Position="point";internal float Heading;}
 bool _pointEditorBusy,_pointEditorSaveBusy,_editingRoutePoint=true,_clockedIn,_showPlacementMarkers;
 long _pointEditorSequence;MenuPage _menuPage=MenuPage.EmployeePaths,_editorReturnPage;
 int _selection,_editorReturnSelection,_editingBayIndex=-1;string _editingPointLabel="test";
 WorkBay _editingPoint=new WorkBay(),_supplierPartsPallet=new WorkBay(),_receptionDesk=new WorkBay(),_receptionApproach=new WorkBay();
 WorkBay[] _extraStoragePallets=new WorkBay[0];object _customerOrder;
 List<string> events=new List<string>();int saves,captures,backs;Func<bool> save;Action configure;
 void TracePointEditor(string s){events.Add(s);}bool Save(){saves++;return save==null?true:save();}
 int EditingVehicleParkingSlot()=>-1;
 void ConfigureVehicleParkingBay(int i){captures++;}void ConfigureWorkBay(int i){captures++;}
 void ConfigureRoutePoint(WorkBay p,string l){captures++;if(configure!=null)configure();}
 void ConfigureArrivalPoint(WorkBay p,string l){captures++;}
 void DeleteStoragePalletVisuals(){}void DismissReceptionist(){}void DismissWorkerForSlot(int i){}void Notify(string s){}
 string FormatPosition(string p)=>p;void NavigateBack(){backs++;}
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS waypoint "+n);}
 static void Main(){
 Test("trace burst is bounded and recovery reports suppression",()=>{DateTime now=DateTime.UnixEpoch;var rows=new List<string>();var t=new WaypointTrace(()=>now,rows.Add);for(int i=0;i<10000;i++)t.Write("entry");Check(rows.Count==24,"unbounded");now=now.AddSeconds(11);t.Write("exit");Check(rows.Count==25&&rows[24].Contains("suppressed=9976"),"suppression");});
 Test("trace writer reentry cannot recurse",()=>{int writes=0;WaypointTrace t=null;t=new WaypointTrace(()=>DateTime.UnixEpoch,s=>{writes++;t.Write("nested");});t.Write("start");Check(writes==1,"recursive");});
 Test("trace storage error never escapes",()=>{var t=new WaypointTrace(()=>DateTime.UnixEpoch,s=>{throw new Exception();});for(int i=0;i<100;i++)t.Write("error");});
 Test("trace clock error never escapes or poisons guard",()=>{bool fail=true;int writes=0;var t=new WaypointTrace(()=>{if(fail)throw new Exception();return DateTime.UnixEpoch;},s=>writes++);t.Write("first");fail=false;t.Write("second");Check(writes==1,"guard");});
 Test("trace payload is bounded",()=>{string text="";new WaypointTrace(()=>DateTime.UnixEpoch,s=>text=s).Write(new string('x',10000));Check(text.Length<550,"size");});
 Test("entry preserves correct return page and row",()=>{var h=new Harness{_selection=3};var p=new WorkBay();h.OpenPointEditor(p,"arrival",-1,true);Check(h._editingPoint==p&&h._editorReturnPage==MenuPage.EmployeePaths&&h._editorReturnSelection==3&&h._pointEditorSequence==1,"entry");});
 Test("nested editor entry cannot overwrite parent with PointEditor",()=>{var h=new Harness{_selection=3};h.OpenPointEditor(new WorkBay(),"first",-1,true);h.OpenPointEditor(new WorkBay(),"nested",-1,true);Check(h._editorReturnPage==MenuPage.EmployeePaths&&h._editingPointLabel=="first"&&h._pointEditorSequence==1,"parent self-loop");});
 Test("action callback reentry is blocked once",()=>{var h=new Harness();h.configure=()=>h.ExecutePointEditorSelection();h.ExecutePointEditorSelection();Check(h.captures==1&&!h._pointEditorBusy&&h.events.Contains("action-reentry-blocked"),"action recursion");});
 Test("action error releases busy guard and permits retry",()=>{var h=new Harness();h.configure=()=>{throw new InvalidOperationException();};try{h.ExecutePointEditorSelection();}catch(InvalidOperationException){}Check(!h._pointEditorBusy&&h.events.Contains("error-recovery-InvalidOperationException"),"release");h.configure=null;h.ExecutePointEditorSelection();Check(h.captures==2,"retry");});
 Test("save callback reentry is blocked without second write",()=>{var h=new Harness();h.save=()=>{Check(!h.SaveEditorPoint(),"nested save allowed");return true;};Check(h.SaveEditorPoint()&&h.saves==1&&!h._pointEditorSaveBusy,"save guard");});
 Test("save failure reports failed commit",()=>{var h=new Harness{save=()=>false};Check(!h.SaveEditorPoint()&&h.events.Contains("save-failed")&&!h.events.Contains("save-committed"),"false commit");});
 Test("save exception restores guard",()=>{var h=new Harness{save=()=>{throw new Exception();}};try{h.SaveEditorPoint();}catch{}Check(!h._pointEditorSaveBusy&&h.events.Contains("save-error-Exception"),"save recovery");});
 Test("null point exits without recursion or save",()=>{var h=new Harness{_editingPoint=null};h.ExecutePointEditorSelection();Check(h.backs==1&&h.saves==0&&!h._pointEditorBusy,"null");});
 Console.WriteLine($"waypoint {passed}/{passed} PASS; bounded managed diagnostics, GTA NOT RUN");
 }
}
