using System;
using System.Drawing;
using System.Collections.Generic;
namespace GTA.Native {enum Hash{DRAW_LINE}static class Function{internal static List<object[]> lines=new List<object[]>();internal static void Call(Hash h,params object[] a){lines.Add(a);}}}
struct Vector3 {internal float X,Y,Z;internal Vector3(float x,float y,float z){X=x;Y=y;Z=z;}}
class Harness {
    class WorkBay{internal bool IsSet=true;internal Vector3 Position;internal float Heading;}
    /*<source:ParkingOutlineVertices>*/
    /*<source:MarkParkingSquare>*/
    static int passed;static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static void Near(double a,double b,string m){Check(Math.Abs(a-b)<0.0001,m+" "+a+" != "+b);}
    static double Dist(Vector3 a,Vector3 b)=>Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z));
    static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS #4 "+n);}
    static void Main(){
        float[] headings={0,90,180,270};float[] xs={0,-1,0,1},ys={1,0,-1,0};
        for(int k=0;k<4;k++){int i=k;Test("heading "+headings[i]+" follows actual GTA front axis",()=>{
            var p=ParkingOutlineVertices(new Vector3(10,20,6),headings[i]);
            Near((p[2].X+p[3].X)/2-10,2.55*xs[i],"front X");Near((p[2].Y+p[3].Y)/2-20,2.55*ys[i],"front Y");
            Near(p[4].X-10,2.15*xs[i],"arrow X");Near(p[4].Y-20,2.15*ys[i],"arrow Y");
            Near(Dist(p[0],p[1]),2.36,"width");Near(Dist(p[1],p[2]),5.1,"length");Near(p[4].Z,6.025,"floor lift");
        });}
        Test("diagonal geometry inverts vendor Vector3.ToHeading",()=>{
            foreach(float heading in new[]{45f,135f,225f,315f,-45f,405f}){
                var p=ParkingOutlineVertices(new Vector3(),heading);
                // Authoritative SHVDN v2 ToHeading: atan2(X,-Y)+PI, in degrees.
                double actual=(Math.Atan2(p[4].X,-p[4].Y)+Math.PI)*180/Math.PI;
                double expected=(heading%360+360)%360;
                Near((actual+360)%360,expected,"inverse heading");
            }
        });
        Test("draw emits exactly four edges and two front-arrow arms without mutating placement",()=>{
            GTA.Native.Function.lines.Clear();var point=new WorkBay{Position=new Vector3(1,2,3),Heading=90};MarkParkingSquare(point,Color.White);
            Check(GTA.Native.Function.lines.Count==6,"native draw count");Near(point.Heading,90,"saved heading changed");Near(point.Position.Z,3,"saved position changed");
            var l=GTA.Native.Function.lines;Near((float)l[4][0],(float)l[5][0],"arrow shared tip X");Near((float)l[4][1],(float)l[5][1],"arrow shared tip Y");
        });
        Test("unset or null placement produces no natives",()=>{GTA.Native.Function.lines.Clear();MarkParkingSquare(null,Color.White);MarkParkingSquare(new WorkBay{IsSet=false},Color.White);Check(GTA.Native.Function.lines.Count==0,"unset draw");});
        Console.WriteLine($"#4 {passed}/{passed} PASS; GTA testing NOT RUN");
    }
}
