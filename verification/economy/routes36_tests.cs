using System;
using System.Collections.Generic;
using System.Linq;
namespace GTA.UI{enum Font{ChaletLondon}static class TextElement{internal static int calls;internal static float GetStringWidth(string s,Font f,float scale){calls++;return s.Length*12*scale;}}}
class Harness{
 class Mechanic{internal string Name="Alex";internal int JobOutboundMask,JobReturnMask;}
 class WorkBay{internal bool IsSet;internal float X,Y,Z;}
 enum MenuPage{MechanicRouteDirection,MechanicRoutePoints}
 const float MenuScale=.75f;
 static readonly Dictionary<string,string> _menuFitCache=new Dictionary<string,string>();
 List<Mechanic> _mechanics=new List<Mechanic>{new Mechanic(),new Mechanic(),new Mechanic()};
 WorkBay[] _serviceRoute=Enumerable.Range(0,5).Select(i=>new WorkBay{X=i+10,Y=i+20,Z=i+30}).ToArray();
 WorkBay[] _bayApproaches=Enumerable.Range(0,3).Select(i=>new WorkBay{IsSet=true,X=i+50}).ToArray();
 bool _customMechanicJobRoutes=true,_routeConfigReturning;int _routeConfigSlot;MenuPage _menuPage=MenuPage.MechanicRouteDirection;
 /*<source:FitMenuText>*/
 /*<source:CountMask>*/
 /*<source:EffectiveWorkRouteMask>*/
 /*<source:MechanicRouteHeading>*/
 /*<source:MechanicRouteCounts>*/
 /*<source:MechanicRoutePreview>*/
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS routes36 "+n);}
 static void Main(){
 for(int slot=0;slot<3;slot++)foreach(bool returning in new[]{false,true}){int who=slot;bool reverse=returning;Test("all 32 selections x 32 physical masks for mechanic/direction "+who+"/"+reverse,()=>{var h=new Harness();for(int saved=0;saved<32;saved++)for(int selected=0;selected<32;selected++){for(int i=0;i<5;i++)h._serviceRoute[i].IsSet=(saved&(1<<i))!=0;h._mechanics[who].JobOutboundMask=h._mechanics[who].JobReturnMask=selected;int effective=selected&saved;string counts=h.MechanicRouteCounts(who,reverse);Check(counts==CountMask(selected)+" selected, "+CountMask(effective)+" saved, "+CountMask(selected&~saved)+" unset","counts");string p=h.MechanicRoutePreview(who,reverse);var expected=Enumerable.Range(1,5).Where(i=>(effective&(1<<(i-1)))!=0);if(reverse)expected=expected.Reverse();int previous=-1;foreach(int point in expected){int at=p.IndexOf("Work "+point,StringComparison.Ordinal);Check(at>previous,"ordered physical mapping");previous=at;}for(int i=1;i<=5;i++)Check(p.Contains("Work "+i)==((effective&(1<<(i-1)))!=0),"invented or skipped stop");}});}
 Test("outbound summary follows saved selections then bay approach and workstation",()=>{var h=new Harness();h._serviceRoute[0].IsSet=h._serviceRoute[2].IsSet=true;h._mechanics[1].JobOutboundMask=21;Check(h.MechanicRoutePreview(1,false)=="To workstation: Work 1 -> Work 3 -> Bay 2 Approach -> Workstation 2","outbound");});
 Test("return summary puts approach first and reverses selected shared stops",()=>{var h=new Harness();h._serviceRoute[0].IsSet=h._serviceRoute[2].IsSet=true;h._mechanics[1].JobReturnMask=21;Check(h.MechanicRoutePreview(1,true)=="From workstation: Bay 2 Approach -> Work 3 -> Work 1 -> Break Area","return");});
 Test("DIRECT mode reports selections inactive without changing them",()=>{var h=new Harness{_customMechanicJobRoutes=false};h._mechanics[0].JobOutboundMask=31;foreach(var p in h._serviceRoute)p.IsSet=true;string text=h.MechanicRoutePreview(0,false);Check(text.Contains("DIRECT")&&text.Contains("inactive")&&!text.Contains("Work 1")&&h._mechanics[0].JobOutboundMask==31&&h.MechanicRouteCounts(0,false)=="5 selected, 5 saved, 0 unset","direct");});
 Test("zero effective mask keeps target and configured approach only",()=>{var h=new Harness();h._mechanics[0].JobOutboundMask=8;Check(h.MechanicRouteCounts(0,false)=="1 selected, 0 saved, 1 unset"&&h.MechanicRoutePreview(0,false)=="To workstation: Bay 1 Approach -> Workstation 1","unset");});
 Test("missing approach never invents a physical point",()=>{var h=new Harness();h._bayApproaches[2].IsSet=false;Check(h.MechanicRoutePreview(2,true)=="From workstation: Break Area","approach");});
 Test("summary is read-only across all mechanic masks and saved locations",()=>{var h=new Harness();h._mechanics[1].JobOutboundMask=17;h._mechanics[1].JobReturnMask=6;var points=h._serviceRoute.ToArray();string initial=string.Join(";",points.Select(p=>$"{p.IsSet}/{p.X}/{p.Y}/{p.Z}"));for(int i=0;i<100;i++){h.MechanicRouteCounts(1,false);h.MechanicRouteCounts(1,true);h.MechanicRoutePreview(1,false);h.MechanicRoutePreview(1,true);}Check(h._mechanics[1].JobOutboundMask==17&&h._mechanics[1].JobReturnMask==6&&initial==string.Join(";",points.Select(p=>$"{p.IsSet}/{p.X}/{p.Y}/{p.Z}"))&&points.SequenceEqual(h._serviceRoute),"mutated data");});
 Test("invalid mechanic uses safe display without indexing",()=>{var h=new Harness();Check(h.MechanicRoutePreview(-1,false)=="No mechanic"&&h.MechanicRouteCounts(3,true)=="No mechanic","vacant");});
 Test("header identifies mechanic and both workstation directions",()=>{var h=new Harness{_routeConfigSlot=1,_menuPage=MenuPage.MechanicRoutePoints};h._mechanics[1].Name="Maya";Check(h.MechanicRouteHeading()=="TO WORKSTATION / Maya","to");h._routeConfigReturning=true;Check(h.MechanicRouteHeading()=="FROM WORKSTATION / Maya","from");});
 Test("long header name is fitted while direction remains readable",()=>{var h=new Harness{_menuPage=MenuPage.MechanicRoutePoints};h._mechanics[0].Name=new string('W',200);string title=h.MechanicRouteHeading();Check(title.StartsWith("TO WORKSTATION / ")&&title.EndsWith("...")&&GTA.UI.TextElement.GetStringWidth(title,GTA.UI.Font.ChaletLondon,.46f*MenuScale)<=382*MenuScale,"heading clipping");int calls=GTA.UI.TextElement.calls;for(int i=0;i<100;i++)h.MechanicRouteHeading();Check(calls==GTA.UI.TextElement.calls,"uncached header width");});
 Test("outbound and return masks remain independent",()=>{var h=new Harness();foreach(var p in h._serviceRoute)p.IsSet=true;h._mechanics[0].JobOutboundMask=1;h._mechanics[0].JobReturnMask=16;Check(h.MechanicRoutePreview(0,false).Contains("Work 1")&&!h.MechanicRoutePreview(0,false).Contains("Work 5")&&h.MechanicRoutePreview(0,true).Contains("Work 5")&&!h.MechanicRoutePreview(0,true).Contains("Work 1"),"mask coupling");});
 Console.WriteLine($"routes36 {passed}/{passed} PASS; production read-only route summaries, 6144 sparse-mask/direction/worker combinations; GTA NOT RUN");
 }
}
