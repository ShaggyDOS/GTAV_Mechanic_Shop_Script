// Offline PE metadata only. Does not load/execute GTA, SHVDN or user scripts.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
class MetadataAudit {
 static Dictionary<string,string> Constants(MetadataReader r,string typeName){
  foreach(var h in r.TypeDefinitions){var t=r.GetTypeDefinition(h);if(r.GetString(t.Namespace)+"."+r.GetString(t.Name)!=typeName)continue;
   var values=new Dictionary<string,string>();foreach(var fh in t.GetFields()){var f=r.GetFieldDefinition(fh);var c=f.GetDefaultValue();if(c.IsNil)continue;values[r.GetString(f.Name)]=Convert.ToHexString(r.GetBlobBytes(r.GetConstant(c).Value));}return values;}
  throw new Exception("Missing enum "+typeName);
 }
 static object Identity(MetadataReader r){var a=r.GetAssemblyDefinition();return new {name=r.GetString(a.Name),version=a.Version.ToString(),references=r.AssemblyReferences.Select(h=>{var x=r.GetAssemblyReference(h);return new{name=r.GetString(x.Name),version=x.Version.ToString()};}).ToArray()};}
 static void Main(string[] args){
  using var p2=new PEReader(File.OpenRead(args[0]));using var p3=new PEReader(File.OpenRead(args[1]));using var product=new PEReader(File.OpenRead(args[2]));
  var v2=p2.GetMetadataReader();var v3=p3.GetMetadataReader();var built=product.GetMetadataReader();var source=File.ReadAllText(args[3]);
  var c2=Constants(v2,"GTA.Control");var c3=Constants(v3,"GTA.Control");var controlChanges=c2.Where(x=>!c3.TryGetValue(x.Key,out var val)||val!=x.Value).Select(x=>x.Key).ToArray();
  if(controlChanges.Length!=0)throw new Exception("v2 Control setting compatibility changed: "+string.Join(",",controlChanges));
  var h2=Constants(v2,"GTA.Native.Hash");var h3=Constants(v3,"GTA.Native.Hash");var names=Regex.Matches(source,@"GTA\.Native\.Hash\.(\w+)").Select(m=>m.Groups[1].Value).Distinct().OrderBy(x=>x).ToArray();
  foreach(var n in names)if(!h2.TryGetValue(n,out var x)||!h3.TryGetValue(n,out var y)||x!=y)throw new Exception("Native hash changed: "+n);
  if(Constants(v2,"GTA.Font")["ChaletLondon"]!=Constants(v3,"GTA.UI.Font")["ChaletLondon"])throw new Exception("font index changed");
  foreach(var h in built.AssemblyReferences){if(built.GetString(built.GetAssemblyReference(h).Name)=="ScriptHookVDotNet2")throw new Exception("Product references v2");}
  var output=new{v2_api=Identity(v2),v3_api=Identity(v3),built_product=Identity(built),v3_api_sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))).ToLowerInvariant(),v2_control_names_checked=c2.Count,control_value_changes=controlChanges,native_hashes_checked=names.Length,native_hashes=names,font_index_unchanged=true,scope="Offline metadata; engine/native execution and Enhanced runtime compatibility NOT RUN"};
  Console.WriteLine(JsonSerializer.Serialize(output,new JsonSerializerOptions{WriteIndented=true}));
 }
}
