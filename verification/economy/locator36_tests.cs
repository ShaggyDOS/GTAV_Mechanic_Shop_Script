using System;
using System.Collections.Generic;
using System.Linq;
struct Vector3{internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
enum BlipSprite{Package}enum BlipColor{Yellow}
class Blip{
 internal int Handle;internal Vector3 Position;internal bool alive=true,route,stickyDelete,failExists,failRoute,failSprite;internal int deleteFailures,deletes,existsCalls,routeWrites;
 internal bool Exists(){existsCalls++;if(failExists)throw new Exception("exists failed");return alive;}
 internal void Delete(){deletes++;if(deleteFailures>0){deleteFailures--;throw new Exception("delete failed");}if(!stickyDelete)alive=false;}
 internal bool ShowRoute{set{routeWrites++;if(failRoute)throw new Exception("route failed");route=value;}}
 BlipSprite sprite;internal BlipSprite Sprite{get=>sprite;set{if(failSprite)throw new Exception("configure failed");sprite=value;}}
 internal BlipColor Color;internal float Scale;internal bool IsShortRange;internal string Name;
}
static class World{
 internal static int creates,waypointReads,failures,nextHandle;internal static bool failSpriteNext,failWaypointRead;internal static Blip waypoint;internal static List<Blip> owned;
 internal static Blip CreateBlip(Vector3 p){creates++;if(failures>0){failures--;return null;}var b=new Blip{Handle=++nextHandle,Position=p,failSprite=failSpriteNext};failSpriteNext=false;owned.Add(b);return b;}
 internal static Blip WaypointBlip{get{waypointReads++;if(failWaypointRead)throw new Exception("waypoint failed");return waypoint;}}
 internal static void Reset(){creates=waypointReads=failures=0;nextHandle=0;failSpriteNext=failWaypointRead=false;waypoint=null;owned=new List<Blip>();}
}
static class Game{internal static bool IsPaused;}
class Harness{
 /*<source:MenuPage>*/
 /*<source:PickupWaypointSnapshot>*/
 /*<source:WholesalePickupLocationValid>*/
 /*<source:WholesalePickupLocatorEligible>*/
 /*<source:ReadPickupWaypoint>*/
 /*<source:PickupWaypointChanged>*/
 /*<source:NotePickupLocatorFailure>*/
 /*<source:DeleteWholesalePickupBlip>*/
 /*<source:SyncWholesalePickupBlip>*/
 /*<source:SetSupplierPickupWaypoint>*/
 /*<source:DistanceSquared>*/
 /*<source:FooterRow>*/
 class WorkBay{internal bool IsSet=true;internal Vector3 Position=new Vector3(10,20,6);}
 WorkBay _wholesaleWarehousePickup=new WorkBay();
 Blip _wholesalePickupBlip;long _pickupLocatorOrderId;Vector3 _pickupLocatorPosition;int _pickupLocatorAttempts;
 DateTime _nextPickupLocatorAttempt=DateTime.MinValue,_nextPickupLocatorLog=DateTime.MinValue,_supplierGameplayNow=new DateTime(2000,1,1);
 bool _pickupGpsActive,_pickupLocatorConfigured;PickupWaypointSnapshot _pickupWaypointAtGps;
 bool _bootGameplayActivated=true,_supplierPlayerCarrying,_wholesaleCargoLoaded,_wholesaleCargoUnloaded;
 int _deliveryKind=0,_deliverySupplier=0,_deliveryQuantity=5,_supplierVisualStage=7;double _deliveryRemainingGameSeconds;long _deliveryDueUtcTicks=12345;
 long _cash=1000,_supplierInvoicesDue=567;int _parts=10;
 List<string> notices=new List<string>(),logs=new List<string>();void Notify(string s){notices.Add(s);}void Log(string s,Exception e){logs.Add(s);}
 int _selection,status,contents,recover,cancel,back;MenuPage _menuPage=MenuPage.ActiveDeliveries;
 /*<source:ShowSupplierDeliveryStatus>*/
 void ShowSupplierOrderContents(){contents++;}void RecoverSupplierShipment(){recover++;}void CancelSupplierOrderWithConfirmation(){cancel++;}void NavigateBack(){back++;}
 bool _deliveryPrepaid;int _selectedSupplier;long _completedSupplierDeliveries,_deliveryCost;int[] _deliveryLines;
 string SupplierName(int x)=>"Supplier";int SupplierDeliverySeconds(int x)=>30;string DeliveryItemName(int x)=>"parts";
 string Status(WorkBay p)=>p.IsSet?"SET":"UNSET";string MuleStageName()=>"stage "+_supplierVisualStage;
 bool ManagerSupplierEligible()=>false;bool SupplierPlayerAway()=>false;int AvailableSupplierPallet()=>0;int StoredPartsTotal()=>10;int TotalStorageCapacity()=>100;
 void ShowStatusNotice(string title,params string[] rows){notices.Add(title);notices.AddRange(rows);}
 string[] GetMenuItems(){switch(_menuPage){
 /*<cases:GetMenuItems:ActiveDeliveries>*/
 default:return new[]{"Other"};}}
 string GetMenuItemValue(int index){switch(_menuPage){
 /*<cases:GetMenuItemValue:ActiveDeliveries>*/
 default:return "";}}
 string GetSelectedDescription(){switch(_menuPage){
 /*<cases:GetSelectedDescription:ActiveDeliveries>*/
 default:return "";}}
 void ExecuteSelection(){switch(_menuPage){
 /*<cases:ExecuteSelection:ActiveDeliveries>*/
 }}
 void Tick(int second){_supplierGameplayNow=new DateTime(2000,1,1).AddSeconds(second);SyncWholesalePickupBlip(_supplierGameplayNow);}
 string Ledger()=>_cash+"|"+_parts+"|"+_supplierInvoicesDue+"|"+_deliveryKind+"|"+_deliveryQuantity+"|"+_deliveryDueUtcTicks+"|"+_deliveryRemainingGameSeconds;
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}static void Test(string n,Action a){World.Reset();Game.IsPaused=false;a();passed++;Console.WriteLine("PASS locator36 "+n);}
 static void Main(){
 Test("READY cargo gets exactly one named long-range package blip at the existing point",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;Check(b!=null&&b.alive&&World.creates==1&&b.Name=="Glass Heroes — Wholesale Parts Pickup"&&b.Sprite==BlipSprite.Package&&b.Color==BlipColor.Yellow&&!b.IsShortRange&&b.Scale==.8f&&b.Position.X==10&&b.Position.Y==20&&b.Position.Z==6&&!b.route,"ready map");});
 Test("repeated READY sync reuses handle and never reads player waypoint without GPS",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;for(int i=1;i<=1000;i++)h.Tick(i);Check(World.creates==1&&h._wholesalePickupBlip==b&&World.waypointReads==0&&b.routeWrites==0,"per-frame churn");});
 Test("no map natives before boot activation or while paused",()=>{var h=new Harness{_bootGameplayActivated=false};h.Tick(0);h._bootGameplayActivated=true;Game.IsPaused=true;h.Tick(1);h.SetSupplierPickupWaypoint();Check(World.creates==0&&World.waypointReads==0,"premature native");});
 Test("transit ETA and local/express orders have no locator",()=>{foreach(int supplier in new[]{0,1,2}){var h=new Harness{_deliverySupplier=supplier,_deliveryRemainingGameSeconds=supplier==0?1:0};h.Tick(0);}Check(World.creates==0,"transit/supplier");});
 Test("invalid sentinel kind quantity stage and nonfinite ETA cannot create",()=>{var scenarios=new Action<Harness>[] {h=>h._deliveryKind=-2,h=>h._deliveryKind=3,h=>h._deliveryQuantity=0,h=>h._deliveryQuantity=-1,h=>h._supplierVisualStage=0,h=>h._supplierVisualStage=6,h=>h._supplierVisualStage=9,h=>h._deliveryRemainingGameSeconds=double.NaN,h=>h._deliveryRemainingGameSeconds=double.PositiveInfinity};foreach(var set in scenarios){var h=new Harness();set(h);h.Tick(0);}Check(World.creates==0,"invalid ready");});
 Test("carried loaded or unloaded cargo removes existing locator immediately on sync",()=>{foreach(int phase in new[]{0,1,2}){var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;if(phase==0)h._supplierPlayerCarrying=true;if(phase==1)h._wholesaleCargoLoaded=true;if(phase==2)h._wholesaleCargoUnloaded=true;h.Tick(1);Check(!b.alive&&h._wholesalePickupBlip==null&&!h._pickupGpsActive,"cargo cleanup");}});
 Test("world model streaming and player distance are not locator dependencies",()=>{var h=new Harness();h.Tick(0);Check(h._wholesalePickupBlip!=null,"locator requires actor/model/player");});
 Test("missing/nonfinite location never invents coordinates and setup notices are rate limited",()=>{var h=new Harness();h._wholesaleWarehousePickup.IsSet=false;for(int i=0;i<30;i++)h.Tick(i);Check(World.creates==0&&h.notices.Count==1,"missing");h.Tick(30);Check(h.notices.Count==2,"notice cooldown");foreach(float value in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity}){h._wholesaleWarehousePickup.IsSet=true;h._wholesaleWarehousePickup.Position=new Vector3(value,20,6);h.Tick(31);}Check(World.creates==0,"nonfinite");});
 Test("explicit GPS uses only owned route and preserves existing user waypoint",()=>{var user=new Blip{Handle=900,Position=new Vector3(90,80,7)};World.waypoint=user;var h=new Harness();h.Tick(0);Check(!h._pickupGpsActive&&!h._wholesalePickupBlip.route,"auto route");h.SetSupplierPickupWaypoint();Check(h._pickupGpsActive&&h._wholesalePickupBlip.route&&World.waypoint==user&&user.alive&&user.Position.X==90&&user.deletes==0&&user.routeWrites==0,"waypoint overwritten");});
 Test("changing an existing waypoint position yields only owned route",()=>{var user=new Blip{Handle=900,Position=new Vector3(90,80,7)};World.waypoint=user;var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();user.Position=new Vector3(100,80,7);h.Tick(1);Check(!h._pickupGpsActive&&!h._wholesalePickupBlip.route&&h._wholesalePickupBlip.alive&&user.alive&&user.deletes==0&&World.creates==1,"waypoint priority");});
 Test("new waypoint handle at same position also yields owned GPS",()=>{World.waypoint=new Blip{Handle=900,Position=new Vector3(90,80,7)};var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();World.waypoint=new Blip{Handle=901,Position=new Vector3(90,80,7)};h.Tick(1);Check(!h._pickupGpsActive&&World.waypoint.alive,"handle change");});
 Test("setting a waypoint from none or clearing an old one yields owned GPS",()=>{foreach(bool initial in new[]{false,true}){World.waypoint=initial?new Blip{Handle=900}:null;var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();World.waypoint=initial?null:new Blip{Handle=901};h.Tick(1);Check(!h._pickupGpsActive&&!h._wholesalePickupBlip.route,"presence change");}});
 Test("unchanged player waypoint retains explicitly chosen route",()=>{World.waypoint=new Blip{Handle=900,Position=new Vector3(90,80,7)};var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();for(int i=1;i<10;i++)h.Tick(i);Check(h._pickupGpsActive&&h._wholesalePickupBlip.route&&h._wholesalePickupBlip.routeWrites==1,"route reissued/yielded");});
 Test("explicit GPS can be selected again after yielding without altering the new waypoint",()=>{var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();World.waypoint=new Blip{Handle=901,Position=new Vector3(90,80,7)};h.Tick(1);h.SetSupplierPickupWaypoint();Check(h._pickupGpsActive&&World.waypoint.Handle==901&&World.waypoint.routeWrites==0&&World.creates==1,"reselect");});
 Test("waypoint read failure yields owned GPS safely without global writes",()=>{var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();World.failWaypointRead=true;h.Tick(1);Check(!h._pickupGpsActive&&!h._wholesalePickupBlip.route&&World.creates==1,"read failure");});
 Test("creation failure has three attempts then cooldown and later recovery",()=>{var h=new Harness();World.failures=100;for(int i=0;i<40;i++)h.Tick(i);Check(World.creates==3&&h.logs.Count==1,"unbounded retries/logs");h.Tick(40);Check(World.creates==4&&h.logs.Count==2,"cooldown");World.failures=0;h.Tick(41);Check(World.creates==4,"early retry");h.Tick(45);Check(h._wholesalePickupBlip!=null&&World.creates==5,"no recovery");});
 Test("partial configuration failure deletes owned candidate and respects retry interval",()=>{var h=new Harness();World.failSpriteNext=true;h.Tick(0);Check(World.owned.Count==1&&!World.owned[0].alive&&h._wholesalePickupBlip==null,"partial leak");h.Tick(1);Check(World.creates==1,"retry storm");h.Tick(5);Check(World.creates==2&&World.owned.Count(x=>x.alive)==1&&h._pickupLocatorConfigured,"configure recovery");});
 Test("failed deletion retains ownership and blocks duplicate creation",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;b.deleteFailures=4;h._wholesaleCargoLoaded=true;h.Tick(1);Check(h._wholesalePickupBlip==b&&b.alive&&World.creates==1,"lost handle");h._wholesaleCargoLoaded=false;h.Tick(2);Check(h._wholesalePickupBlip==b&&World.creates==1,"duplicate");h.Tick(5);Check(!b.alive&&World.creates==2&&World.owned.Count(x=>x.alive)==1,"delete recovery");});
 Test("silent delete failure cannot discard an owned handle",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;b.stickyDelete=true;h._deliveryKind=-2;h.Tick(1);Check(h._wholesalePickupBlip==b&&b.deletes==2&&b.alive,"silent delete");b.stickyDelete=false;h.Tick(2);Check(h._wholesalePickupBlip==null&&!b.alive,"retry cleanup");});
 Test("route-disable exception does not prevent deleting owned blip",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;b.failRoute=true;h._deliveryKind=-2;h.Tick(1);Check(h._wholesalePickupBlip==null&&!b.alive,"cleanup blocked by route");});
 Test("unrelated customer/map/waypoint objects are never deleted",()=>{var other=new Blip{Handle=801};var wp=new Blip{Handle=802};World.waypoint=wp;var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();h.DeleteWholesalePickupBlip();Check(other.alive&&wp.alive&&other.deletes==0&&wp.deletes==0&&wp.routeWrites==0,"unrelated cleanup");});
 Test("completion cancels owned locator and cannot change inventory/invoices",()=>{var h=new Harness();h.Tick(0);h.SetSupplierPickupWaypoint();h._deliveryKind=-2;string data=h.Ledger();h.Tick(1);Check(h._wholesalePickupBlip==null&&h.Ledger()==data,"completion bookkeeping");});
 Test("failed collection/cancellation represented by unchanged pending state retains locator",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;string data=h.Ledger();h.Tick(1);Check(h._wholesalePickupBlip==b&&World.creates==1&&data==h.Ledger(),"failed action removed locator");});
 Test("recovery resets visual only and recreates original identity once READY",()=>{var h=new Harness();h.Tick(0);var old=h._wholesalePickupBlip;h.DeleteWholesalePickupBlip();h._supplierVisualStage=0;h.Tick(1);h._supplierVisualStage=7;h.Tick(2);Check(!old.alive&&h._wholesalePickupBlip!=null&&h._pickupLocatorOrderId==12345&&World.creates==2&&h._supplierInvoicesDue==567&&h._parts==10,"recovery receipt");});
 Test("reload reconstructs only from valid data after normal boot gate",()=>{var old=new Harness();old.Tick(0);old.DeleteWholesalePickupBlip();var fresh=new Harness{_bootGameplayActivated=false};fresh.Tick(1);Check(World.creates==1,"load native");fresh._bootGameplayActivated=true;fresh.Tick(2);Check(World.creates==2&&World.owned.Count(x=>x.alive)==1&&!fresh._pickupGpsActive,"reload");});
 Test("new order identity or changed pickup point deletes old binding",()=>{var h=new Harness();h.Tick(0);var first=h._wholesalePickupBlip;h._deliveryDueUtcTicks=999;h.Tick(1);Check(!first.alive&&World.creates==2&&h._pickupLocatorOrderId==999,"identity");var second=h._wholesalePickupBlip;h._wholesaleWarehousePickup.Position=new Vector3(11,22,6);h.Tick(2);Check(!second.alive&&World.creates==3&&h._wholesalePickupBlip.Position.X==11,"location");});
 Test("external removal is recreated with bounded requests",()=>{var h=new Harness();h.Tick(0);h._wholesalePickupBlip.alive=false;h.Tick(1);Check(World.creates==1,"unbounded external recreate");h.Tick(5);Check(World.creates==2&&World.owned.Count(x=>x.alive)==1,"external recovery");});
 Test("GPS unavailable or missing location cannot create route",()=>{var h=new Harness{_deliveryRemainingGameSeconds=10};h.SetSupplierPickupWaypoint();h._deliveryRemainingGameSeconds=0;h._wholesaleWarehousePickup.IsSet=false;h.SetSupplierPickupWaypoint();Check(World.creates==0&&World.waypointReads==0&&h.notices.Any(x=>x.Contains("location unavailable")),"unavailable GPS");});
 Test("GPS setup failure cleans only the owned candidate",()=>{var h=new Harness();h.Tick(0);var b=h._wholesalePickupBlip;b.failRoute=true;World.waypoint=new Blip{Handle=987};h.SetSupplierPickupWaypoint();Check(!b.alive&&h._wholesalePickupBlip==null&&!h._pickupGpsActive&&World.waypoint.alive,"GPS failure leak");});
 Test("menu appends explicit GPS without moving original action indices",()=>{var h=new Harness();var rows=h.GetMenuItems();Check(rows.Length==6&&rows[2]=="Recover Original Order"&&rows[3]=="Cancel Pending Order"&&rows[4]=="Set Warehouse GPS"&&rows[5].StartsWith("< Back"),"menu order");h._selection=4;h.ExecuteSelection();Check(h._pickupGpsActive&&h.recover+h.cancel+h.back==0,"GPS dispatch");h._selection=5;h.ExecuteSelection();Check(h.back==1,"back");});
 Test("rendering GPS readiness/help is pure and does not create locator or save",()=>{var h=new Harness{_selection=4};string data=h.Ledger();for(int i=0;i<1000;i++){Check(h.GetMenuItemValue(4)=="READY"&&h.GetSelectedDescription().Length>0,"help");h.GetMenuItems();}Check(World.creates==0&&World.waypointReads==0&&data==h.Ledger(),"render side effect");});
 Test("all locator and GPS operations preserve financial and shipment fields",()=>{var h=new Harness();string data=h.Ledger();h.Tick(0);h.SetSupplierPickupWaypoint();for(int i=1;i<100;i++)h.Tick(i);h.DeleteWholesalePickupBlip();Check(data==h.Ledger()&&h._cash==1000&&h._parts==10&&h._supplierInvoicesDue==567,"navigation changed ledger");});
 Test("delivery status advertises GPS only for uncollected READY cargo",()=>{var h=new Harness();h.ShowSupplierDeliveryStatus();Check(h.notices.Any(x=>x.Contains("Set Warehouse GPS")),"missing ready help");h.notices.Clear();h._wholesaleCargoLoaded=true;h._supplierVisualStage=9;h.ShowSupplierDeliveryStatus();Check(!h.notices.Any(x=>x.Contains("Set Warehouse GPS")),"loaded falsely ready");});
 Console.WriteLine($"locator36 {passed}/{passed} PASS; production owned locator/GPS and actual menu arms with deterministic native adapters; GTA NOT RUN");
 }
}
