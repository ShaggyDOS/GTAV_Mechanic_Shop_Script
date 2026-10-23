using System;
using System.Collections.Generic;
namespace GTA {enum Control {MoveLeftRight,MoveUpDown,LookLeftRight,LookUpDown,VehicleAccelerate,VehicleBrake,VehicleMoveLeftRight,VehicleMoveUpDown,VehicleHandbrake,FrontendPause,FrontendPauseAlternate}}
namespace GTA.Native {enum Hash {DISABLE_ALL_CONTROL_ACTIONS,ENABLE_CONTROL_ACTION}static class Function {internal static HashSet<string> enabled=new HashSet<string>();internal static void Call(Hash h,params object[] a){if(h==Hash.DISABLE_ALL_CONTROL_ACTIONS)enabled.Clear();else enabled.Add(a[0]+":"+a[1]);}}}
class Ped {internal bool driving;internal bool Exists()=>true;internal bool IsInVehicle()=>driving;}
static class Game {internal static P Player=new P();internal class P {internal Ped Character=new Ped();}}
class Harness {
/*<source:DisableControllerControlsWhileMenuOpen>*/
static void Main(){int n=0;foreach(bool driving in new[]{false,true}){Game.Player.Character.driving=driving;new Harness().DisableControllerControlsWhileMenuOpen();foreach(int group in new[]{0,2})foreach(var pause in new[]{GTA.Control.FrontendPause,GTA.Control.FrontendPauseAlternate}){if(!GTA.Native.Function.enabled.Contains(group+":"+(int)pause))throw new Exception("native pause blocked while focused");n++;}}Console.WriteLine($"focus {n}/{n} PASS; native pause controls enabled on foot/driving, GTA NOT RUN");}
}
