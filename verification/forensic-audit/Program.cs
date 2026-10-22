using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
class Program {
 static void Main(string[] args) {
  var root=CSharpSyntaxTree.ParseText(File.ReadAllText(args[0])).GetRoot();
  var methods=root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToDictionary(x=>x.Identifier.Text);
  var calls=methods.ToDictionary(x=>x.Key,x=>x.Value.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(c=>c.Expression is IdentifierNameSyntax || c.Expression is MemberAccessExpressionSyntax m && (m.Expression is ThisExpressionSyntax || m.Expression.ToString()=="GlassHeroesBusiness")).Select(c=>c.Expression is IdentifierNameSyntax n ? n.Identifier.Text : ((MemberAccessExpressionSyntax)c.Expression).Name.Identifier.Text).Where(methods.ContainsKey).Distinct().ToArray());
  Console.WriteLine($"Parsed {methods.Count} methods; local unqualified/this/class call cycles (syntax only, no native/runtime/delegate inference):");
  var found=new HashSet<string>();
  foreach(var start in calls.Keys) {
   var path=new List<string>(); var visited=new HashSet<string>();
   void Search(string node) {
    if(path.Contains(node)) {var cycle=path.Skip(path.IndexOf(node)).ToArray(); var key=string.Join(",",cycle.OrderBy(x=>x)); if(found.Add(key))Console.WriteLine(string.Join(" -> ",cycle.Concat(new[]{node}))); return;}
    if(!visited.Add(node))return;
    path.Add(node); foreach(var n in calls[node])Search(n);path.RemoveAt(path.Count-1);
   }
   Search(start);
  }
  Console.WriteLine($"Unique cycle sets: {found.Count}");
  Console.WriteLine("Manually reviewed same-name external calls: UI.Notify and data.Save are not local recursion.");
  foreach(var ctor in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
    Console.WriteLine("Constructor " + ctor.Identifier + " calls: " + string.Join(", ",ctor.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(x=>x.Expression.ToString()).Distinct().DefaultIfEmpty("<none>")));
  foreach(var node in root.DescendantNodes().OfType<InvocationExpressionSyntax>().Where(x=>x.Expression.ToString().Contains("Request") || x.Expression.ToString().Contains("World.Create") || x.Expression.ToString().Contains("World.GetNearby") || x.Expression.ToString().Contains("OnTick") || x.Expression.ToString().Contains("OnAborted")))
    Console.WriteLine("Review line " + (node.GetLocation().GetLineSpan().StartLinePosition.Line+1) + ": " + node.ToString().Replace("\n", " "));
  Console.WriteLine("Limits: no semantic/native/other-script/dynamic event/delegate analysis; cycles absent is not proof of no recursion.");
 }
}
