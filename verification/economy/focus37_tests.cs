using System;using System.Collections.Generic;
class Harness {
 /*<source:MenuPage>*/
 /*<source:FocusedPlacementPoint>*/
 class WorkBay {internal bool IsSet;internal int X;}
 class Mechanic {internal int JobOutboundMask=31,JobReturnMask;}
 WorkBay[] _arrivalRoute=Points(5),_serviceRoute=Points(5),_bayApproaches=Points(3),_receptionRoute=Points(5),_managerRoute=Points(5),_customerWalkingRoute=Points(3),_supplierDriverWaypoints=Points(3);
 WorkBay _editingPoint,_customerDropoff=new WorkBay(),_customerDriveSpawn=new WorkBay(),_customerApproach=new WorkBay(),_customerQueue=new WorkBay(),_queueCar2=new WorkBay(),_queueCar3=new WorkBay(); WorkBay[] _bays=Points(3),_vehicleParkingBays=Points(3);int _serviceBaySlot;bool _showMenu=true;MenuPage _menuPage;int _selection,_routeConfigSlot;bool _routeConfigReturning;
 List<Mechanic> _mechanics=new List<Mechanic>{new Mechanic()};
 static WorkBay[] Points(int n){var p=new WorkBay[n];for(int i=0;i<n;i++)p[i]=new WorkBay{IsSet=true,X=i};return p;}
 string GetMenuItemValue(int index){switch(_menuPage){
 /*<cases:GetMenuItemValue:MechanicRoutePoints>*/
 default:return ">";}}
 string GetSelectedDescription(){switch(_menuPage){
 /*<cases:GetSelectedDescription:MechanicRoutePoints>*/
 default:return "";}}
 static int passed;static void Check(bool ok,string why){if(!ok)throw new Exception(why);}static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS "+n);}
 static void Main(){
 Test("each physical spawn-break coordinate resolves by identity",()=>{var h=new Harness{_menuPage=MenuPage.ArrivalWaypoints};for(int i=0;i<5;i++){h._selection=i;Check(ReferenceEquals(h.FocusedPlacementPoint(),h._arrivalRoute[i]),"spawn point");}});
 Test("each shared bay coordinate resolves from editor and per-mechanic selection",()=>{var h=new Harness();foreach(MenuPage page in new[]{MenuPage.ServiceWaypoints,MenuPage.MechanicRoutePoints})for(int i=0;i<5;i++){h._menuPage=page;h._selection=i;Check(ReferenceEquals(h.FocusedPlacementPoint(),h._serviceRoute[i]),"mask index vs coordinate");}});
 Test("point editor preserves the exact selected reference",()=>{var h=new Harness{_menuPage=MenuPage.PointEditor};h._editingPoint=h._bayApproaches[2];Check(ReferenceEquals(h.FocusedPlacementPoint(),h._bayApproaches[2]),"editor reference");});
 Test("role and customer driver lists select their own coordinates",()=>{var h=new Harness{_selection=1};foreach(MenuPage page in new[]{MenuPage.ReceptionWaypoints,MenuPage.ManagerWaypoints,MenuPage.CustomerWalkingRoutes,MenuPage.SupplierDriverWaypoints,MenuPage.BayApproaches}){h._menuPage=page;Check(h.FocusedPlacementPoint()!=null&&h.FocusedPlacementPoint().X==1,"role mapping");}});
 Test("Back rows and negative selections never highlight another point",()=>{var h=new Harness{_menuPage=MenuPage.ServiceWaypoints};foreach(int row in new[]{-1,5,6,99}){h._selection=row;Check(h.FocusedPlacementPoint()==null,"back highlight");}});
 Test("closing menu removes physical focus",()=>{var h=new Harness{_menuPage=MenuPage.PointEditor,_editingPoint=new WorkBay(),_showMenu=false};Check(h.FocusedPlacementPoint()==null,"stale focus");});
 Test("saved status and mechanic use are independent",()=>{var h=new Harness{_menuPage=MenuPage.MechanicRoutePoints};h._serviceRoute[0].IsSet=false;Check(h.GetMenuItemValue(0)=="USE"&&h.GetSelectedDescription().Contains("NOT SAVED"),"used but missing");h._mechanics[0].JobOutboundMask=0;h._serviceRoute[0].IsSet=true;Check(h.GetMenuItemValue(0)=="SKIP"&&h.GetSelectedDescription().Contains("SAVED"),"saved but skipped");});
 Test("preview and value lookup do not mutate geometry or either mask",()=>{var h=new Harness{_menuPage=MenuPage.MechanicRoutePoints};for(int i=0;i<1000;i++){h._selection=i%5;h.FocusedPlacementPoint();h.GetMenuItemValue(i%5);h.GetSelectedDescription();}Check(h._mechanics[0].JobOutboundMask==31&&h._mechanics[0].JobReturnMask==0&&h._serviceRoute[4].X==4,"preview writes");});
 Test("all/none commands are not physical coordinate references",()=>{var h=new Harness{_menuPage=MenuPage.MechanicRoutePoints};h._selection=5;Check(h.FocusedPlacementPoint()==null,"use all");h._selection=6;Check(h.FocusedPlacementPoint()==null,"skip all");});
 Console.WriteLine($"focus37 {passed}/{passed} PASS; actual physical-focus and independent use/saved menu arms; GTA NOT RUN");
 }
}
