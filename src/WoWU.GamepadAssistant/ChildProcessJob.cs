using System.Diagnostics;
using System.Runtime.InteropServices;
namespace WoWU.GamepadAssistant;
internal sealed class ChildProcessJob:IDisposable
{
 [StructLayout(LayoutKind.Sequential)] private struct Basic {public long ProcessTime,JobTime;public uint Flags;public nuint Min,Max;public uint Count;public nuint Affinity;public uint Priority,Scheduling;}
 [StructLayout(LayoutKind.Sequential)] private struct Io {public ulong ReadOps,WriteOps,OtherOps,ReadBytes,WriteBytes,OtherBytes;}
 [StructLayout(LayoutKind.Sequential)] private struct Limits {public Basic Basic;public Io Io;public nuint ProcessMemory,JobMemory,PeakProcess,PeakJob;}
 [DllImport("kernel32.dll",SetLastError=true)] private static extern nint CreateJobObject(nint attrs,string? name);
 [DllImport("kernel32.dll",SetLastError=true)] private static extern bool SetInformationJobObject(nint job,int kind,ref Limits limits,int size);
 [DllImport("kernel32.dll",SetLastError=true)] private static extern bool AssignProcessToJobObject(nint job,nint process);
 [DllImport("kernel32.dll")] private static extern bool CloseHandle(nint h);
 private nint job;
 internal ChildProcessJob(Process process)
 {
  job=CreateJobObject(0,null);Native.Check(job!=0,"Could not create streaming lifetime guard");
  try{var limits=new Limits{Basic=new Basic{Flags=0x2000}};Native.Check(SetInformationJobObject(job,9,ref limits,Marshal.SizeOf<Limits>()),"Could not configure streaming lifetime guard");Native.Check(AssignProcessToJobObject(job,process.Handle),"Could not attach streaming lifetime guard");}
  catch{Dispose();throw;}
 }
 public void Dispose(){if(job!=0){CloseHandle(job);job=0;}}
}
