using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace GTA.UI {enum Font{ChaletLondon}static class TextElement{internal static int calls;internal static float GetStringWidth(string s,Font f,float scale){calls++;return Metrics(s)*scale;}internal static float Metrics(string s){s=Regex.Replace(s,"~[^~]+~","");float n=0;foreach(char c in s)n+=c=='W'?18:c=='i'||c=='.'?3:c==' '?5:10;return n;}}}
class Harness{
 const float MenuScale=.75f;
 static readonly Dictionary<string,string[]> _menuDescriptionCache=new Dictionary<string,string[]>();static string _lastDescriptionInput;static int _lastDescriptionWidth;static string[] _lastDescriptionLines;
 /*<source:FittingMenuWords>*/
 /*<source:LayoutMenuDescription>*/
 /*<source:WrapMenuDescription>*/
 static int passed;static void Check(bool b,string s){if(!b)throw new Exception(s);}static void Test(string n,Action a){a();passed++;Console.WriteLine("PASS footer36 "+n);}
 static float Measure(string s)=>GTA.UI.TextElement.Metrics(s);
 static void Fits(string[] lines,float w){Check(lines.Length==2&&lines.All(s=>Measure(s)<=w),"pixel overflow");}
 static void Main(){
 Test("empty and null descriptions have no fragment",()=>{Check(LayoutMenuDescription(null,100,Measure).All(s=>s=="")&&LayoutMenuDescription("  \t\r\n",100,Measure).All(s=>s==""),"empty");});
 Test("one-line description stays whole",()=>{var a=LayoutMenuDescription("Saved stop",200,Measure);Check(a[0]=="Saved stop"&&a[1]=="","single");});
 Test("variable-width letters fit where equal character counts cannot",()=>{foreach(string text in new[]{"WWWW WWWW WWWW WWWW","iiii iiii iiii iiii iiii iiii"})Fits(LayoutMenuDescription(text,160,Measure),160);});
 Test("two complete lines use natural balanced words",()=>{string text="one two three four five six seven eight";var a=LayoutMenuDescription(text,220,Measure);Check(string.Join(" ",a)==text&&!a.Any(x=>x.EndsWith("...")),"lost words");Check(Math.Abs(Measure(a[0])-Measure(a[1]))<90,"unbalanced");Fits(a,220);});
 Test("long footer truncates only at a word boundary",()=>{var a=LayoutMenuDescription("one two three four five six seven eight nine ten eleven twelve thirteen fourteen",160,Measure);Check(a[1].EndsWith("..."),"missing ellipsis");foreach(string word in (a[0]+" "+a[1].Replace("...","")).Split(' ',StringSplitOptions.RemoveEmptyEntries))Check(new[]{"one","two","three","four","five","six","seven","eight","nine","ten","eleven","twelve","thirteen","fourteen"}.Contains(word),"midword");Fits(a,160);});
 Test("unbreakable first token becomes safe ellipsis",()=>{var a=LayoutMenuDescription(new string('W',200),100,Measure);Check(a[0]=="..."&&a[1]=="","token fragment");});
 Test("unbreakable later token never leaks outside panel",()=>{Fits(LayoutMenuDescription("one two "+new string('W',200),100,Measure),100);});
 Test("newline whitespace and GTA newline tokens cannot make a third line",()=>{var a=LayoutMenuDescription("one\r\n two\tthree~n~four ~N~ five",300,Measure);Check(!string.Join(" ",a).Contains("~n~")&&!a.Any(x=>x.Contains('\n')||x.Contains('\r')||x.Contains('\t')),"extra line");Fits(a,300);});
 Test("GTA colour codes remain intact",()=>{var a=LayoutMenuDescription("~b~Shared~s~ work stops for this mechanic and direction",250,Measure);Check(string.Join(" ",a).Contains("~b~Shared~s~"),"format split");Fits(a,250);});
 Test("too-narrow or invalid width cannot overflow ellipsis",()=>{Fits(LayoutMenuDescription("hello world",5,Measure),5);Check(LayoutMenuDescription("text",0,Measure).All(x=>x==""),"zero");});
 Test("malformed huge input and token count are bounded",()=>{int calls=0;Func<string,float> measure=t=>{calls++;Check(t.Length<=1024,"unbounded native text");return Measure(t);};var a=LayoutMenuDescription(string.Join(" ",Enumerable.Repeat("word",10000)),200,measure);Fits(a,200);Check(calls<40,"unbounded search");});
 Test("unchanged selection does not repeat width queries",()=>{GTA.UI.TextElement.calls=0;var a=WrapMenuDescription("Shared work stops are selected separately by each mechanic.",394);int before=GTA.UI.TextElement.calls;for(int i=0;i<10000;i++)Check(Object.ReferenceEquals(a,WrapMenuDescription("Shared work stops are selected separately by each mechanic.",394)),"cache missed");Check(before>0&&GTA.UI.TextElement.calls==before,"per-frame width natives");});
 Test("returning to a cached selection reuses layout",()=>{WrapMenuDescription("old footer",394);WrapMenuDescription("other footer",394);int before=GTA.UI.TextElement.calls;WrapMenuDescription("old footer",394);Check(GTA.UI.TextElement.calls==before,"revisit");});
 Test("layout width invalidates while cache remains bounded",()=>{for(int i=0;i<400;i++)WrapMenuDescription("Description "+i,394);Check(_menuDescriptionCache.Count<=128,"cache growth");var narrow=WrapMenuDescription("Many shared work stops belong to all mechanics.",100);Check(narrow.All(s=>GTA.UI.TextElement.GetStringWidth(s,GTA.UI.Font.ChaletLondon,.30f*MenuScale)<=100*MenuScale),"width key");});
 Test("reference geometry scales proportionally at 720p and 1080p",()=>{var a=WrapMenuDescription("Choose saved shared work stops; car pickup uses the entry corridor.",394);foreach(float resolutionScale in new[]{1f,1.5f})foreach(string line in a)Check(GTA.UI.TextElement.GetStringWidth(line,GTA.UI.Font.ChaletLondon,.30f*MenuScale)*resolutionScale<=394*MenuScale*resolutionScale,"resolution");});
 Console.WriteLine($"footer36 {passed}/{passed} PASS; production bounded pixel layout with deterministic font metrics; GTA NOT RUN");
 }
}
