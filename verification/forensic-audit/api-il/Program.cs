using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
var reader=new PEReader(File.OpenRead(args[0]));var md=reader.GetMetadataReader();
string Name(EntityHandle h) {
 if(h.Kind==HandleKind.TypeDefinition){var t=md.GetTypeDefinition((TypeDefinitionHandle)h);return md.GetString(t.Namespace)+"."+md.GetString(t.Name);}
 if(h.Kind==HandleKind.TypeReference){var t=md.GetTypeReference((TypeReferenceHandle)h);return md.GetString(t.Namespace)+"."+md.GetString(t.Name);}
 if(h.Kind==HandleKind.MethodDefinition){var m=md.GetMethodDefinition((MethodDefinitionHandle)h);return Name(m.GetDeclaringType())+"::"+md.GetString(m.Name);}
 if(h.Kind==HandleKind.MemberReference){var m=md.GetMemberReference((MemberReferenceHandle)h);return Name(m.Parent)+"::"+md.GetString(m.Name);}
 return h.Kind.ToString();
}
var ops=typeof(OpCodes).GetFields().Where(f=>f.FieldType==typeof(OpCode)).Select(f=>(OpCode)f.GetValue(null)).ToDictionary(o=>unchecked((ushort)o.Value));
foreach(var th in md.TypeDefinitions){var type=md.GetTypeDefinition(th);var tn=Name(th);
 foreach(var mh in type.GetMethods()){var method=md.GetMethodDefinition(mh);var mn=md.GetString(method.Name);
 if(!((tn=="GTA.Model"&&mn==".ctor") || (tn=="GTA.Game"&&mn=="GenerateHash") || args.Skip(1).Contains(tn+"::"+mn)))continue;
 Console.WriteLine(Name(mh)+" signature="+Convert.ToHexString(md.GetBlobBytes(method.Signature)));
 if(method.RelativeVirtualAddress==0){Console.WriteLine("  no IL body");continue;}
 var il=reader.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();int at=0;
 while(at<il.Length){int pos=at;ushort key=il[at++];if(key==0xfe)key=(ushort)(0xfe00|il[at++]);var op=ops[key];int size=op.OperandType switch {
 OperandType.InlineNone=>0,OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar=>1,
 OperandType.InlineVar=>2,OperandType.InlineI8 or OperandType.InlineR=>8,OperandType.InlineSwitch=>4+4*BitConverter.ToInt32(il,at),_=>4};
 if(op.OperandType==OperandType.InlineMethod)Console.WriteLine("  IL_"+pos.ToString("x4")+" "+op.Name+" "+Name(MetadataTokens.EntityHandle(BitConverter.ToInt32(il,at))));
 at+=size;
 }
 }}
