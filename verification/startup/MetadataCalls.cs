// Read-only PE/IL inspection. Never loads or executes GTA/SHVDN assemblies.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.Json;
class MetadataCalls {
 static MetadataReader r;
 static string TypeName(EntityHandle h) {
  if(h.Kind==HandleKind.TypeDefinition){var t=r.GetTypeDefinition((TypeDefinitionHandle)h);return r.GetString(t.Namespace)+"."+r.GetString(t.Name);}
  if(h.Kind==HandleKind.TypeReference){var t=r.GetTypeReference((TypeReferenceHandle)h);return r.GetString(t.Namespace)+"."+r.GetString(t.Name);}
  return h.Kind.ToString();
 }
 static string Name(EntityHandle h){
  if(h.Kind==HandleKind.MethodSpecification)return Name(r.GetMethodSpecification((MethodSpecificationHandle)h).Method);
  if(h.Kind==HandleKind.MethodDefinition){var m=r.GetMethodDefinition((MethodDefinitionHandle)h);return TypeName(m.GetDeclaringType())+"::"+r.GetString(m.Name);}
  if(h.Kind==HandleKind.MemberReference){var m=r.GetMemberReference((MemberReferenceHandle)h);return TypeName(m.Parent)+"::"+r.GetString(m.Name);}
  return h.Kind.ToString();
 }
 static void Main(string[] args){
  using var pe=new PEReader(File.OpenRead(args[0]));r=pe.GetMetadataReader();
  var codes=typeof(OpCodes).GetFields(BindingFlags.Static|BindingFlags.Public).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(x=>unchecked((ushort)x.Value));
  var output=new List<object>();
  foreach(var h in r.MethodDefinitions){var m=r.GetMethodDefinition(h);string name=Name(h);if(!args.Skip(1).Any(a=>name.Contains(a)))continue;
   var calls=new List<string>();if(m.RelativeVirtualAddress!=0){var il=pe.GetMethodBody(m.RelativeVirtualAddress).GetILBytes();int i=0;
    while(i<il.Length){ushort code=il[i++];if(code==254)code=(ushort)(0xfe00|il[i++]);var op=codes[code];int n;
     switch(op.OperandType){case OperandType.InlineNone:n=0;break;case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:n=1;break;case OperandType.InlineVar:n=2;break;case OperandType.InlineI8:case OperandType.InlineR:n=8;break;case OperandType.InlineSwitch:n=4+4*BitConverter.ToInt32(il,i);break;default:n=4;break;}
     if(op.OperandType==OperandType.InlineMethod)calls.Add(Name(MetadataTokens.EntityHandle(BitConverter.ToInt32(il,i))));i+=n;
    }
   }
   output.Add(new{method=name,token=MetadataTokens.GetToken(h).ToString("X8"),calls=calls.Distinct().OrderBy(x=>x).ToArray()});
  }
  Console.WriteLine(JsonSerializer.Serialize(output,new JsonSerializerOptions{WriteIndented=true}));
 }
}
