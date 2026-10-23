#!/usr/bin/env python3
# Read-only AMD64 minidump evidence extraction; never executes uploaded code.
import struct,json,hashlib,datetime,argparse
from pathlib import Path
ap=argparse.ArgumentParser();ap.add_argument('dump',type=Path);args=ap.parse_args();p=args.dump;b=p.read_bytes()
def u(fmt,off):return struct.unpack_from('<'+fmt,b,off)
def string(r):n=u('I',r)[0];return b[r+4:r+4+n].decode('utf-16-le',errors='replace')
sig,ver,n,dr,checksum,ts,flags=u('4sIIIIIQ',0);streams={}
for i in range(n):t,size,r=u('III',dr+12*i);streams[t]=(size,r)
mods=[]
if 4 in streams:
 _,r=streams[4];count=u('I',r)[0]
 for i in range(count):
  off=r+4+108*i;base,size,cs,time,name=u('QIIII',off);v=u('13I',off+24);mods.append({'name':string(name),'base':base,'size':size,'timestamp':time,'version':'.'.join(map(str,[v[2]>>16,v[2]&65535,v[3]>>16,v[3]&65535]))})
def locate(addr):
 for m in mods:
  if m['base']<=addr<m['base']+m['size']:return m['name'].split('\\')[-1]+'+0x'+format(addr-m['base'],'X')
 return hex(addr)
out={'file':p.name,'sha256':hashlib.sha256(b).hexdigest(),'bytes':len(b),'dump_header_timestamp_utc':datetime.datetime.fromtimestamp(ts,datetime.timezone.utc).isoformat(),'flags':hex(flags),'streams':{str(t):size for t,(size,r) in streams.items()},'module_count':len(mods)}
_,er=streams[6];tid=u('I',er)[0];code,eflags,record,addr,num,align=u('IIQQII',er+8);params=u('15Q',er+40);csize,crva=u('II',er+160)
regs={name:hex(u('Q',crva+off)[0]) for name,off in {'RAX':120,'RCX':128,'RDX':136,'RBX':144,'RSP':152,'RBP':160,'RSI':168,'RDI':176,'R8':184,'R9':192,'R10':200,'R11':208,'R12':216,'R13':224,'R14':232,'R15':240,'RIP':248}.items()}
out['exception']={'thread_id':tid,'code':hex(code),'address':hex(addr),'module_offset':locate(addr),'parameters':[hex(x) for x in params[:num]],'context_size':csize,'registers':regs}
_,tr=streams[3];tc=u('I',tr)[0];out['thread_count']=tc
for i in range(tc):
 off=tr+4+48*i;thid=u('I',off)[0]
 if thid!=tid:continue
 start,size,rva=u('QII',off+24);rsp=int(regs['RSP'],16);candidates=[]
 for pos in range(max(0,rsp-start),size-7,8):
  value=u('Q',rva+pos)[0];label=locate(value)
  if '+' in label:candidates.append({'stack_offset':hex(start+pos-rsp),'value':hex(value),'module_offset':label})
 out['raw_stack_module_pointers_NOT_UNWOUND']=candidates[:25];out['exception_thread_stack']={'start':hex(start),'bytes':size,'rva':rva}
# Memory ranges available for instruction decoding.
ranges=[]
if 5 in streams:
 _,r=streams[5];count=u('I',r)[0]
 for i in range(count):a,sz,rv=u('QII',r+4+16*i);ranges.append((a,sz,rv))
if 9 in streams:
 _,r=streams[9];count,rv=u('QQ',r);pos=rv
 for i in range(count):a,sz=u('QQ',r+16+16*i);ranges.append((a,sz,pos));pos+=sz
out['captured_memory_ranges']=len(ranges)
out['exception']['effective_indexed_address']=hex((int(regs['RAX'],16)+int(regs['RDX'],16)*8)&((1<<64)-1))
out['interpretation_limits']='Raw module pointers are NOT an unwound stack. No symbols, managed stack attribution, or causal component identification.'
for a,sz,rv in ranges:
 if a<=addr<a+sz:
  offset=rv+addr-a;codebytes=b[offset:offset+5];out['fault_instruction_bytes']=codebytes.hex()
  try:
   from capstone import Cs,CS_ARCH_X86,CS_MODE_64
   out['fault_disassembly']=[hex(i.address)+': '+i.mnemonic+' '+i.op_str for i in Cs(CS_ARCH_X86,CS_MODE_64).disasm(codebytes,addr)]
  except ImportError:pass
out['selected_module_versions']=[{'name':m['name'].split('\\')[-1],'version':m['version']} for m in mods if m['name'].split('\\')[-1].lower() in {'gta5.exe','scripthookv.dll','scripthookvdotnet.asi','scripthookvdotnet2.dll','scripthookvdotnet3.dll','clr.dll','ragepluginhook.exe'}]
print(json.dumps(out,indent=2))
