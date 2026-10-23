using System;
using System.Collections.Generic;
using System.Linq;
namespace GTA.Native { enum Hash { SET_ENTITY_COLLISION } static class Function { internal static void Call(Hash h,params object[] a){} } }
struct Vector3 { internal float X,Y,Z;internal static Vector3 Zero=>new Vector3();internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
struct Model {internal int Hash;internal static int queries;internal static Vector3 boxMin=new Vector3(-.2f,-.2f,0),boxMax=new Vector3(.2f,.2f,.35f);internal Model(string name){Hash=1;}internal Model(int hash){Hash=hash;}internal (Vector3,Vector3) Dimensions { get {queries++;return Hash==1?(boxMin,boxMax):(new Vector3(-.6f,-.5f,0),new Vector3(.6f,.5f,.2f));} } }
class Prop { internal bool alive=true;internal Vector3 Position;internal int Handle;internal float Heading;internal Model Model=new Model(2);internal bool Exists()=>alive;internal void Delete(){alive=false;} }
class Ped { internal Vector3 Position;internal bool Exists()=>true; }
class PlayerState { internal Ped Character=new Ped(); } static class Game { internal static PlayerState Player=new PlayerState(); }
class Harness {
    class WorkBay { internal bool IsSet=true;internal Vector3 Position;internal float Heading; }
    /*<source:CompatiblePackageVisualMask>*/
    /*<source:DeleteLegacyStockroomDisplayBoxes>*/
    /*<source:DeleteStoragePalletVisuals>*/
    /*<source:RefreshStoragePalletAfterDeposit>*/
    /*<source:HideUnsupportedPalletBoxes>*/
    /*<source:UpdateSupplierPalletVisual>*/
    /*<source:StoredBoxPosition>*/
    /*<source:PalletBoxLayout>*/
    /*<source:BuildPalletBoxLayout>*/
    /*<source:RotatePalletOffset>*/
    /*<source:VisiblePackageCount>*/
    /*<source:TryStoredBoxTransform>*/
    /*<source:StoredPalletLayout>*/
    readonly Dictionary<long,PalletBoxLayout> _palletBoxLayouts=new Dictionary<long,PalletBoxLayout>();
    bool RequestVisualModel(Model model){modelRequests++;return !failModels;}
    void ReleaseVisualModel(Model model){modelReleases++;}
    int modelRequests,modelReleases;
    /*<source:CommitSupplierDelivery>*/
    /*<source:ApplySupplierInventory>*/
    int[] _deliveryLines;
    const int MaxStoragePallets=4,CratesPerPallet=9;
    int _ownedStoragePallets=1,_storageVisualAttempts;
    int[] _storedDeliveryCrates={3,0,0,0},_deliveredPackageMasks=new int[4];
    Prop[] _storagePalletProps=new Prop[4],_deliveredPackageProps=new Prop[36];
    List<Prop> _storedCrateProps=new List<Prop>();
    bool _palletVisualDirty=true;DateTime _nextStorageVisualAttempt=DateTime.MinValue;
    WorkBay point=new WorkBay{Position=new Vector3(0,0,6)};
    Prop _supplierCrate=new Prop(),_wholesaleWarehousePallet=new Prop();
    int pallets,boxes,saves;bool failModels,failFirstBox;
    int _deliveryKind=0,_deliveryQuantity=5,_parts,_completedSupplierDeliveries;
    int[] _specialStock=new int[3];long _deliveryCost=450,_supplierInvoicesDue,_deliveryDueUtcTicks;
    bool _deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded;
    double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds;
    WorkBay StoragePalletPoint(int i)=>point;
    bool CanStoreShipment(int q)=>true;
    string DeliveryItemName(int k)=>"parts";
    bool Save(){saves++;return true;}void Notify(string m){}
    void DeleteOwnedProp(ref Prop p){if(p!=null)p.Delete();p=null;}
    Prop CreateFixedEquipmentProp(string n,Vector3 p,float h,bool ground=true){if(failModels||(!ground&&failFirstBox))return null;if(ground){pallets++;p.Z=4;}else boxes++;return new Prop{Position=p,Heading=h};}
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){Game.Player.Character=new Ped();a();passed++;Console.WriteLine("PASS pallets "+n);}
    static PalletBoxLayout Layout(Vector3? pmin=null,Vector3? pmax=null,Vector3? bmin=null,Vector3? bmax=null)=>BuildPalletBoxLayout(pmin??new Vector3(-.6f,-.5f,0),pmax??new Vector3(.6f,.5f,.2f),bmin??new Vector3(-.2f,-.2f,0),bmax??new Vector3(.2f,.2f,.35f));
    static bool Near(float a,float b)=>Math.Abs(a-b)<.0001;
    static void Geometry(PalletBoxLayout l,Vector3 pmn,Vector3 pmx,Vector3 bmn,Vector3 bmx){Check(l!=null,"layout");for(int n=1;n<=9;n++){Check(l.Offsets[n].Length==n,"count");for(int i=0;i<n;i++){
        var p=l.Offsets[n][i];bool turn=l.HeadingOffset==90;float xmin=p.X+(turn?-bmx.Y:bmn.X),xmax=p.X+(turn?-bmn.Y:bmx.X),ymin=p.Y+(turn?bmn.X:bmn.Y),ymax=p.Y+(turn?bmx.X:bmx.Y),zmin=p.Z+bmn.Z,zmax=p.Z+bmx.Z;
        Check(xmin>=pmn.X-.0001&&xmax<=pmx.X+.0001&&ymin>=pmn.Y-.0001&&ymax<=pmx.Y+.0001&&zmin>=pmx.Z-.0001,"outside or through pallet");
        for(int j=0;j<i;j++){var q=l.Offsets[n][j];float qxmin=q.X+(turn?-bmx.Y:bmn.X),qxmax=q.X+(turn?-bmn.Y:bmx.X),qymin=q.Y+(turn?bmn.X:bmn.Y),qymax=q.Y+(turn?bmx.X:bmx.Y),qzmin=q.Z+bmn.Z,qzmax=q.Z+bmx.Z;Check(!(xmin<qxmax-.0001&&xmax>qxmin+.0001&&ymin<qymax-.0001&&ymax>qymin+.0001&&zmin<qzmax-.0001&&zmax>qzmin+.0001),"overlap");}
        if(zmin>pmx.Z+.0001){bool supported=false;for(int j=0;j<n;j++){var q=l.Offsets[n][j];if(Near(q.Z+bmx.Z,zmin)){float qcx=q.X+(turn?-(bmn.Y+bmx.Y)/2:(bmn.X+bmx.X)/2),qcy=q.Y+(turn?(bmn.X+bmx.X)/2:(bmn.Y+bmx.Y)/2);if(qcx>=xmin-(bmx.X-bmn.X)&&qcx<=xmax+(bmx.X-bmn.X)&&qcy>=ymin-(bmx.Y-bmn.Y)&&qcy<=ymax+(bmx.Y-bmn.Y))supported=true;}}Check(supported,"floating");}
    }}}
    static void Main(){
        Test("all partial counts one through nine stay inside bounds without overlap",()=>{Geometry(Layout(),new Vector3(-.6f,-.5f,0),new Vector3(.6f,.5f,.2f),new Vector3(-.2f,-.2f,0),new Vector3(.2f,.2f,.35f));});
        Test("full nine-box layout is four-four-one",()=>{var a=Layout().Offsets[9];Check(a.Take(4).All(p=>Near(p.Z,.2f))&&a.Skip(4).Take(4).All(p=>Near(p.Z,.55f))&&Near(a[8].Z,.9f)&&Near(a[8].X,0)&&Near(a[8].Y,0),"4-4-1");});
        Test("one visible parcel is centered and two are symmetrically centered",()=>{var l=Layout();Check(Near(l.Offsets[1][0].X,0)&&Near(l.Offsets[1][0].Y,0)&&Near(l.Offsets[2][0].X,-l.Offsets[2][1].X),"center");});
        Test("offset model origins are correctly centered and bottom-supported",()=>{Vector3 pmn=new Vector3(.1f,-.8f,-.4f),pmx=new Vector3(1.3f,.2f,.1f),bmn=new Vector3(.1f,-.3f,-.2f),bmx=new Vector3(.5f,.1f,.15f);Geometry(Layout(pmn,pmx,bmn,bmx),pmn,pmx,bmn,bmx);var a=Layout(pmn,pmx,bmn,bmx).Offsets[1][0];Check(Near(a.X+.3f,.7f)&&Near(a.Y-.1f,-.3f)&&Near(a.Z-.2f,.1f),"origin");});
        Test("rotate boxes ninety degrees when necessary to fit four per layer",()=>{Vector3 pmn=new Vector3(-.5f,-.35f,0),pmx=new Vector3(.5f,.35f,.2f),bmn=new Vector3(-.15f,-.2f,0),bmx=new Vector3(.15f,.2f,.35f);var l=Layout(pmn,pmx,bmn,bmx);Check(l.HeadingOffset==90,"orientation");Geometry(l,pmn,pmx,bmn,bmx);});
        Test("narrow pallet safely falls back to three aligned boxes per layer",()=>{Vector3 pmn=new Vector3(-1.2f,-.25f,0),pmx=new Vector3(1.2f,.25f,.2f),bmn=new Vector3(-.35f,-.15f,0),bmx=new Vector3(.35f,.15f,.35f);var l=Layout(pmn,pmx,bmn,bmx);Geometry(l,pmn,pmx,bmn,bmx);Check(l.Offsets[9].Count(p=>Near(p.Z,.2f))==3,"fallback");});
        Test("unsafe oversized or degenerate model dimensions reject layout",()=>{Check(Layout(bmax:new Vector3(3,3,3))==null&&Layout(bmax:new Vector3(-.2f,.2f,.35f))==null&&Layout(pmax:new Vector3(-1,.5f,.2f))==null,"bounds");});
        Test("NaN and infinite bounds cannot create native coordinates",()=>{Check(Layout(bmax:new Vector3(float.NaN,1,1))==null&&Layout(pmin:new Vector3(float.NegativeInfinity,0,0))==null,"nonfinite");});
        Test("all four saved cardinal headings rotate local boxes exactly",()=>{var origin=new Vector3(10,20,30);var p=new Vector3(.2f,.3f,.4f);var east=RotatePalletOffset(origin,p,0);var south=RotatePalletOffset(origin,p,90);var west=RotatePalletOffset(origin,p,180);var north=RotatePalletOffset(origin,p,270);Check(Near(east.X,10.2f)&&Near(east.Y,20.3f)&&Near(south.X,9.7f)&&Near(south.Y,20.2f)&&Near(west.X,9.8f)&&Near(west.Y,19.7f)&&Near(north.X,10.3f)&&Near(north.Y,19.8f)&&Near(north.Z,30.4f),"cardinals");});
        Test("arbitrary heading retains distance and saved ground height",()=>{var p=RotatePalletOffset(new Vector3(0,0,8),new Vector3(3,4,.2f),37);Check(Math.Abs(p.X*p.X+p.Y*p.Y-25)<.001&&Near(p.Z,8.2f),"rotate");});
        Test("legacy holes compact proven parcels onto the bottom instead of floating",()=>{var h=new Harness();h._storedDeliveryCrates[0]=9;h._deliveredPackageMasks[0]=1<<8;h.UpdateSupplierPalletVisual();Check(h.boxes==1&&h._deliveredPackageProps[8]!=null&&Near(h._deliveredPackageProps[8].Position.Z,4.2f)&&Near(h._deliveredPackageProps[8].Position.X,0)&&h._deliveredPackageProps[0]==null,"provenance rank");});
        Test("all nine recorded parcels remain visible and no tenth is created",()=>{var h=new Harness();h._storedDeliveryCrates[0]=9;h._deliveredPackageMasks[0]=511;h.UpdateSupplierPalletVisual();Check(h.boxes==9&&h._deliveredPackageProps.Take(9).All(p=>p!=null)&&h._deliveredPackageProps.Skip(9).All(p=>p==null),"nine");});
        Test("rebuild repositions existing owned boxes as a partial layer fills",()=>{var h=new Harness();h._storedDeliveryCrates[0]=1;h._deliveredPackageMasks[0]=1;h.UpdateSupplierPalletVisual();var first=h._deliveredPackageProps[0];Check(Near(first.Position.X,0),"initial");h._storedDeliveryCrates[0]=2;h._deliveredPackageMasks[0]=3;h._palletVisualDirty=true;h._nextStorageVisualAttempt=DateTime.MinValue;h.UpdateSupplierPalletVisual();Check(h._deliveredPackageProps[0]==first&&first.Position.X<0&&h._deliveredPackageProps[1].Position.X>0&&h.boxes==2,"rebuild");});
        Test("completed visual layout performs no repeated model dimension queries",()=>{var h=new Harness();h._deliveredPackageMasks[0]=7;Model.queries=0;h.UpdateSupplierPalletVisual();int queries=Model.queries;for(int i=0;i<1000;i++)h.UpdateSupplierPalletVisual();Check(queries==2&&Model.queries==queries&&h.modelRequests==1&&h.modelReleases==1,"cache idle");});
        Test("dirty refresh reuses the same cached model layout",()=>{var h=new Harness();h._deliveredPackageMasks[0]=7;Model.queries=0;h.UpdateSupplierPalletVisual();h._palletVisualDirty=true;h._nextStorageVisualAttempt=DateTime.MinValue;h.UpdateSupplierPalletVisual();Check(Model.queries==2,"recache");});
        Test("model cache remains bounded even under changed model identities",()=>{var h=new Harness();h._storagePalletProps[0]=new Prop();for(int i=2;i<22;i++){h._storagePalletProps[0].Model=new Model(i);h.StoredPalletLayout(0);Check(h._palletBoxLayouts.Count<=8,"cache growth");}});
        Test("deposit transform preflight has no stock invoice or save side effects",()=>{var h=new Harness();h._storagePalletProps[0]=new Prop{Position=new Vector3(0,0,4)};Vector3 p;float heading;Check(h.TryStoredBoxTransform(0,3,true,out p,out heading)&&h._specialStock[0]==0&&h._supplierInvoicesDue==0&&h._storedDeliveryCrates[0]==3&&h.saves==0,"preflight");});
        Test("unready layout blocks transform without consuming stock or billing",()=>{var h=new Harness();Vector3 p;float heading;Check(!h.TryStoredBoxTransform(0,3,true,out p,out heading)&&h._deliveryKind==0&&h._deliveryQuantity==5&&h.saves==0,"not ready");});
        Test("existing delivery count mask and capacity ledger are unchanged by visual layout",()=>{var h=new Harness();h._storedDeliveryCrates[0]=5;h._deliveredPackageMasks[0]=24;h.UpdateSupplierPalletVisual();Check(h._storedDeliveryCrates[0]==5&&h._deliveredPackageMasks[0]==24&&h._supplierInvoicesDue==0&&h._specialStock[0]==0&&h.saves==0,"financial drift");});
        Test("stream out and re-entry reconstruct only provenance-backed boxes",()=>{var h=new Harness();h._deliveredPackageMasks[0]=7;h.UpdateSupplierPalletVisual();var old=h._deliveredPackageProps[0];Game.Player.Character.Position=new Vector3(1000,0,0);h.UpdateSupplierPalletVisual();Check(!old.alive&&h._deliveredPackageProps[0]==null,"cleanup");Game.Player.Character.Position=new Vector3();h.UpdateSupplierPalletVisual();Check(h.boxes==6&&h._storedDeliveryCrates[0]==3&&h._deliveredPackageMasks[0]==7,"reentry");});
        Test("visual refresh never changes original saved pallet position or heading",()=>{var h=new Harness();h.point.Heading=90;h._deliveredPackageMasks[0]=7;h.UpdateSupplierPalletVisual();Check(Near(h.point.Position.Z,6)&&Near(h.point.Heading,90)&&Near(h._deliveredPackageProps[0].Heading,90),"saved position");});
        Test("out of range box indexes are rejected including tenth parcel",()=>{var h=new Harness();Vector3 p;float heading;Check(!h.TryStoredBoxTransform(0,-1,true,out p,out heading)&&!h.TryStoredBoxTransform(0,9,true,out p,out heading),"index");});
        Test("failed lower box creation never draws unsupported upper parcels",()=>{var h=new Harness{failFirstBox=true};h._storedDeliveryCrates[0]=9;h._deliveredPackageMasks[0]=511;h._deliveredPackageProps[8]=new Prop();var upper=h._deliveredPackageProps[8];h.UpdateSupplierPalletVisual();Check(h.boxes==0&&!upper.alive&&h._deliveredPackageProps[8]==null&&h._palletVisualDirty&&h._deliveredPackageMasks[0]==511&&h._storedDeliveryCrates[0]==9,"unsupported upper");});
        Test("new deposit rebuilds centered partial layer after prior retries were exhausted",()=>{var h=new Harness();h._storedDeliveryCrates[0]=1;h._deliveredPackageMasks[0]=1;h.UpdateSupplierPalletVisual();var first=h._deliveredPackageProps[0];h._storageVisualAttempts=5;h._nextStorageVisualAttempt=DateTime.UtcNow.AddHours(1);h._storedDeliveryCrates[0]=2;h._deliveredPackageMasks[0]=3;h._palletVisualDirty=true;h.RefreshStoragePalletAfterDeposit();Check(h._storageVisualAttempts==1&&first.alive&&h._deliveredPackageProps[0]==first&&first.Position.X<0&&h._deliveredPackageProps[1].Position.X>0&&!h._palletVisualDirty&&h.saves==0,"exhausted deposit overlap");});
        Test("new deposit grants one bounded retry budget without per-frame model requests",()=>{var h=new Harness{failModels=true};h._storagePalletProps[0]=new Prop{Position=new Vector3(0,0,4)};h._storageVisualAttempts=5;h._deliveredPackageMasks[0]=7;h.RefreshStoragePalletAfterDeposit();int requests=h.modelRequests;for(int i=0;i<100;i++)h.UpdateSupplierPalletVisual();Check(h._storageVisualAttempts==1&&h.modelRequests==requests&&requests==1&&h._storedDeliveryCrates[0]==3&&h._deliveredPackageMasks[0]==7&&h._supplierInvoicesDue==0&&h.saves==0,"unbounded refresh or billing");});
        Console.WriteLine("pallets "+passed+"/"+passed+" PASS; production geometry and owned-visual helpers; GTA testing NOT RUN");
    }
}
