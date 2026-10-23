using System;
using System.Collections.Generic;
namespace GTA.Native { enum Hash { SET_ENTITY_COLLISION } static class Function { internal static void Call(Hash h,params object[] a){} } }
struct Vector3 { internal float X,Y,Z;internal static Vector3 Zero=>new Vector3();internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.X+b.X,a.Y+b.Y,a.Z+b.Z); }
class Prop { internal bool alive=true;internal Vector3 Position;internal int Handle;internal float Heading;internal bool Exists()=>alive;internal void Delete(){alive=false;} }
class Ped { internal Vector3 Position;internal bool Exists()=>true; }
class PlayerState { internal Ped Character=new Ped(); } static class Game { internal static PlayerState Player=new PlayerState(); }
class Harness {
    class WorkBay { internal bool IsSet=true;internal Vector3 Position;internal float Heading; }
    /*<source:CompatiblePackageVisualMask>*/
    /*<source:DeleteLegacyStockroomDisplayBoxes>*/
    /*<source:DeleteStoragePalletVisuals>*/
    /*<source:HideUnsupportedPalletBoxes>*/
    /*<source:UpdateSupplierPalletVisual>*/
    /*<source:StoredBoxPosition>*/
    /*<source:PalletBoxLayout>*/
    /*<source:BuildPalletBoxLayout>*/
    /*<source:RotatePalletOffset>*/
    /*<source:VisiblePackageCount>*/
    /*<source:TryStoredBoxTransform>*/
    PalletBoxLayout StoredPalletLayout(int slot)=>BuildPalletBoxLayout(new Vector3(-.6f,-.5f,0),new Vector3(.6f,.5f,.2f),new Vector3(-.2f,-.2f,0),new Vector3(.2f,.2f,.35f));
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
    int pallets,boxes,saves;bool failModels;
    int _deliveryKind=0,_deliveryQuantity=5,_parts,_completedSupplierDeliveries;
    int[] _specialStock=new int[3];long _deliveryCost=450,_supplierInvoicesDue,_deliveryDueUtcTicks;
    bool _deliveryPrepaid,_wholesaleCargoLoaded,_wholesaleCargoUnloaded;
    double _deliveryRemainingGameSeconds,_supplierTimerSaveSeconds;
    WorkBay StoragePalletPoint(int i)=>point;
    bool CanStoreShipment(int q)=>true;
    string DeliveryItemName(int k)=>"parts";
    bool Save(){saves++;return true;}void Notify(string m){}
    void DeleteOwnedProp(ref Prop p){if(p!=null)p.Delete();p=null;}
    Prop CreateFixedEquipmentProp(string n,Vector3 p,float h,bool ground=true){if(failModels)return null;if(ground){pallets++;p.Z=4;}else boxes++;return new Prop{Position=p,Heading=h};}
    static float DistanceSquared(Vector3 a,Vector3 b)=>(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z);
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Test(string n,Action a){Game.Player.Character=new Ped();a();passed++;Console.WriteLine("PASS #6 "+n);}
    static void Main(){
        Test("legacy BoxCount does not recreate display boxes or erase stock",()=>{var h=new Harness();h.UpdateSupplierPalletVisual();Check(h.pallets==1&&h.boxes==0&&h._storedDeliveryCrates[0]==3,"legacy migration");});
        Test("only recorded delivered parcels reconstruct on actual grounded support",()=>{var h=new Harness();h._storedDeliveryCrates[0]=5;h._deliveredPackageMasks[0]=(1<<3)|(1<<4);h.UpdateSupplierPalletVisual();Check(h.boxes==2&&h._deliveredPackageProps[3]!=null&&h._deliveredPackageProps[0]==null,"provenance filter");Check(Math.Abs(h._deliveredPackageProps[3].Position.Z-4.2f)<0.001,"actual support; provenance holes compact to bottom layer");});
        Test("obsolete cleanup preserves live delivered cargo and warehouse ownership",()=>{var h=new Harness();var old=new Prop();var real=new Prop();h._storedCrateProps.Add(old);h._deliveredPackageProps[0]=real;h.DeleteLegacyStockroomDisplayBoxes();Check(!old.alive&&real.alive&&h._supplierCrate.alive&&h._wholesaleWarehousePallet.alive,"unrelated package removed");});
        Test("successful deposit records provenance and credits ledger once",()=>{var h=new Harness();Check(h.CommitSupplierDelivery(0),"commit");Check(h._specialStock[0]==5&&h._supplierInvoicesDue==450&&h._storedDeliveryCrates[0]==4&&h._deliveredPackageMasks[0]==8&&h.saves==1,"deposit state");Check(!h.CommitSupplierDelivery(0)&&h._specialStock[0]==5&&h._supplierInvoicesDue==450&&h.saves==1,"duplicate commit");});
        Test("dirty refresh preserves an existing real delivered parcel",()=>{var h=new Harness();h._deliveredPackageMasks[0]=1;var real=new Prop{Position=new Vector3(0,0,4.28f)};h._deliveredPackageProps[0]=real;h.UpdateSupplierPalletVisual();Check(real.alive&&h._deliveredPackageProps[0]==real&&h.boxes==0,"delivered parcel replaced");});
        Test("save-mask migration is additive bounded and defaults to hidden legacy",()=>{Check(CompatiblePackageVisualMask(9,0)==0,"old save default");Check(CompatiblePackageVisualMask(3,int.MaxValue)==7,"invalid mask clamp");Check(CompatiblePackageVisualMask(-1,-1)==0,"invalid count");Check(CompatiblePackageVisualMask(99,-1)==511,"slot bounds");});
        Test("stream-out and reload keep provenance and stock while respawning only real parcels",()=>{var h=new Harness();h._deliveredPackageMasks[0]=4;h.UpdateSupplierPalletVisual();Game.Player.Character.Position=new Vector3(1000,0,0);h.UpdateSupplierPalletVisual();Check(h._deliveredPackageProps[2]==null&&h._deliveredPackageMasks[0]==4&&h._storedDeliveryCrates[0]==3,"stream preservation");Game.Player.Character.Position=new Vector3(0,0,0);h.UpdateSupplierPalletVisual();Check(h.boxes==2&&h._deliveredPackageProps[2]!=null,"re-entry provenance");var reload=new Harness();reload._deliveredPackageMasks[0]=CompatiblePackageVisualMask(3,h._deliveredPackageMasks[0]);reload.UpdateSupplierPalletVisual();Check(reload.boxes==1,"reload");});
        Test("missing models have finite attempts and preserve masks/counters",()=>{var h=new Harness{failModels=true};for(int i=0;i<100;i++){h._nextStorageVisualAttempt=DateTime.MinValue;h.UpdateSupplierPalletVisual();}Check(h._storageVisualAttempts==5&&h._storedDeliveryCrates[0]==3,"unbounded attempts or stock loss");});
        Console.WriteLine($"#6 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
