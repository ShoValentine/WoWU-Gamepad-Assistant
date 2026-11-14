using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
namespace WoWU.GamepadAssistant;
internal static class DriverInstall
{
 internal static readonly string Folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"WoWU.GamepadAssistant");
 internal static string Ownership=>Path.Combine(Folder,"display-device.txt");
 private static readonly Guid DisplayClass=new("4d36e968-e325-11ce-bfc1-08002be10318");
 [StructLayout(LayoutKind.Sequential)] private struct Info {public int Size;public Guid Class;public uint DevInst;public nint Reserved;}
 [DllImport("setupapi.dll",SetLastError=true)] private static extern nint SetupDiCreateDeviceInfoList(ref Guid c,nint h);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool SetupDiCreateDeviceInfo(nint h,string name,ref Guid c,string desc,nint parent,uint flags,ref Info info);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool SetupDiSetDeviceRegistryProperty(nint h,ref Info i,uint prop,byte[] data,uint size);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool SetupDiGetDeviceRegistryProperty(nint h,ref Info i,uint prop,out uint type,byte[] data,uint size,out uint required);
 [DllImport("setupapi.dll",SetLastError=true)] private static extern bool SetupDiCallClassInstaller(uint function,nint h,ref Info i);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool SetupDiGetDeviceInstanceId(nint h,ref Info i,StringBuilder id,int size,out int needed);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern nint SetupDiGetClassDevs(ref Guid c,string? enumerator,nint h,uint flags);
 [DllImport("setupapi.dll",SetLastError=true)] private static extern bool SetupDiEnumDeviceInfo(nint h,uint index,ref Info i);
 [DllImport("setupapi.dll")] private static extern bool SetupDiDestroyDeviceInfoList(nint h);
 [DllImport("newdev.dll",CharSet=CharSet.Unicode,SetLastError=true)] private static extern bool UpdateDriverForPlugAndPlayDevices(nint h,string hardware,string inf,uint flags,out bool reboot);

 [DllImport("cfgmgr32.dll",CharSet=CharSet.Unicode)] private static extern uint CM_Locate_DevNodeW(out uint dev,string id,uint flags);
 [DllImport("cfgmgr32.dll")] private static extern uint CM_Get_DevNode_Status(out uint status,out uint problem,uint dev,uint flags);
 [DllImport("cfgmgr32.dll")] private static extern uint CM_Enable_DevNode(uint dev,uint flags);
 [DllImport("cfgmgr32.dll")] private static extern uint CM_Disable_DevNode(uint dev,uint flags);
 internal static string OwnedId()
 {
  string id=File.ReadAllText(Ownership).Trim();
  if(!id.StartsWith(@"ROOT\WOWUVIRTUALDISPLAY\",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The recorded identity is not a WoWU-created device.");
  return id;
 }
 internal static bool IsMissing()
 {
  if(!File.Exists(Ownership))return true;
  uint result=CM_Locate_DevNodeW(out _,OwnedId(),0);
  if(result==0)return false;
  if(result==0xD)return true; // CR_NO_SUCH_DEVNODE
  throw new InvalidOperationException($"Windows could not locate the WoWU driver (code {result}).");
 }
 internal static bool HasFriendlyName()
 {
  if(!File.Exists(Ownership))return false;
  using var key=Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\"+OwnedId());
  return key?.GetValue("FriendlyName") as string=="WoWU Display";
 }
 private static void NameDevice()
 {
  Guid cls=DisplayClass;var set=SetupDiGetClassDevs(ref cls,null,0,2);
  Native.Check(set!=new nint(-1),"Could not find the WoWU driver to name it");
  try {
   for(uint n=0;;n++) {
    var info=new Info{Size=Marshal.SizeOf<Info>()};if(!SetupDiEnumDeviceInfo(set,n,ref info))break;
    var id=new StringBuilder(512);Native.Check(SetupDiGetDeviceInstanceId(set,ref info,id,id.Capacity,out _),"Could not read device identity");
    if(!string.Equals(id.ToString(),OwnedId(),StringComparison.OrdinalIgnoreCase))continue;
    byte[] name=Encoding.Unicode.GetBytes("WoWU Display\0");
    Native.Check(SetupDiSetDeviceRegistryProperty(set,ref info,12,name,(uint)name.Length),"Could not name WoWU Display");return;
   }
   throw new InvalidOperationException("The WoWU driver is not currently present.");
  }finally{SetupDiDestroyDeviceInfoList(set);}
 }
 internal static int Rename()
 {
  try{NameDevice();return 0;}
  catch(Exception ex){Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"install-result.txt"),ex.Message);return 1;}
 }
 internal static bool IsDisabled()
 {
  if(!File.Exists(Ownership) || IsMissing())return false;
  if(CM_Locate_DevNodeW(out uint dev,File.ReadAllText(Ownership).Trim(),0)!=0)throw new InvalidOperationException("The recorded WoWU driver is missing. Its ownership record has been retained for recovery.");
  if(CM_Get_DevNode_Status(out _,out uint problem,dev,0)!=0)throw new InvalidOperationException("Could not read WoWU driver status.");
  if(problem!=0 && problem!=22)throw new InvalidOperationException($"WoWU driver needs attention (Windows problem {problem}).");
  return problem==22;
 }
 internal static int SetEnabled(bool enable)
 {
  try {
   if(!File.Exists(Ownership))return 0;
   string id=File.ReadAllText(Ownership).Trim();
   if(!id.StartsWith("ROOT\\WOWUVIRTUALDISPLAY\\",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("The recorded device identity is not a WoWU-created device.");
   if(!enable && IsMissing())return 0;
   if(CM_Locate_DevNodeW(out uint dev,id,0)!=0)throw new InvalidOperationException("The WoWU driver could not be found.");
   uint result=enable?CM_Enable_DevNode(dev,0):CM_Disable_DevNode(dev,0);
   if(result!=0)throw new InvalidOperationException($"Windows could not {(enable?"enable":"disable")} the WoWU driver (code {result}).");
   File.WriteAllText(Path.Combine(Folder,"install-result.txt"),enable?"Driver enabled.":"Driver disabled.");return 0;
  }catch(Exception ex){Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"install-result.txt"),ex.Message);return 1;}
 }
 internal static int Install()
 {
  Directory.CreateDirectory(Folder);
  try {
   if(File.Exists(Ownership)) {
    if(!IsMissing())throw new InvalidOperationException("WoWU Display is already installed. Use Set up 1280 x 800.");
    File.Move(Ownership,Ownership+".missing-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
   }
   Guid cls=DisplayClass;var existing=SetupDiGetClassDevs(ref cls,null,0,2);
   if(existing==new nint(-1))throw new InvalidOperationException("Could not enumerate existing adapters.");
   try {for(uint n=0;;n++) {
    var item=new Info{Size=Marshal.SizeOf<Info>()};if(!SetupDiEnumDeviceInfo(existing,n,ref item))break;
    var bytes=new byte[4096];
    if(SetupDiGetDeviceRegistryProperty(existing,ref item,1,out _,bytes,(uint)bytes.Length,out _) && Encoding.Unicode.GetString(bytes).Contains("MttVDD",StringComparison.OrdinalIgnoreCase))
     throw new InvalidOperationException("An existing Virtual Display Driver is present. It will not be overwritten or claimed by WoWU.");
   }}finally{SetupDiDestroyDeviceInfoList(existing);}
   string archive=Path.Combine(AppContext.BaseDirectory,"driver.zip");
   var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(archive)));
   if(hash!="E24210692B442B39AF763536330CE78B423F19342B7A7792C26DE3944E418B3A")throw new InvalidOperationException("Driver package integrity check failed.");
   string package=Path.Combine(Folder,"driver-25.7.23");Directory.CreateDirectory(package);
   ZipFile.ExtractToDirectory(archive,package,true);
   string inf=Path.Combine(package,"VirtualDisplayDriver","MttVDD.inf");
   // Driver's documented configuration location. Preserve any previous settings.
   string configFolder=Path.Combine(Path.GetPathRoot(Environment.SystemDirectory)!,"VirtualDisplayDriver");
   Directory.CreateDirectory(configFolder);string config=Path.Combine(configFolder,"vdd_settings.xml");
   string backup=Path.Combine(Folder,"vdd_settings.before.xml");
   if(File.Exists(config) && !File.Exists(backup))File.Copy(config,backup);
   string? old=File.Exists(config)?File.ReadAllText(config):null;
   File.WriteAllText(config,"<?xml version=\"1.0\"?><vdd_settings><monitors><count>1</count></monitors><gpu><friendlyname>default</friendlyname></gpu><resolutions><resolution><width>1280</width><height>800</height><refresh_rate>60</refresh_rate></resolution></resolutions><options><CustomEdid>false</CustomEdid><HardwareCursor>true</HardwareCursor><SDR10bit>false</SDR10bit><HDRPlus>false</HDRPlus></options></vdd_settings>");
   var set=SetupDiCreateDeviceInfoList(ref cls,0);bool registered=false;
   var info=new Info{Size=Marshal.SizeOf<Info>()};
   try {
    Native.Check(set!=new nint(-1),"Could not create device list");
    Native.Check(SetupDiCreateDeviceInfo(set,"WoWUVirtualDisplay",ref cls,"WoWU Display",0,1,ref info),"Could not create virtual display");
    byte[] ids=Encoding.Unicode.GetBytes("Root\\MttVDD\0\0");
    Native.Check(SetupDiSetDeviceRegistryProperty(set,ref info,1,ids,(uint)ids.Length),"Could not assign driver ID");
    Native.Check(SetupDiCallClassInstaller(0x19,set,ref info),"Could not register device");registered=true;
    var id=new StringBuilder(512);Native.Check(SetupDiGetDeviceInstanceId(set,ref info,id,id.Capacity,out _),"Could not record device identity");
    File.WriteAllText(Ownership,id.ToString());
    Native.Check(UpdateDriverForPlugAndPlayDevices(0,"Root\\MttVDD",inf,0,out bool reboot),"Windows could not install the signed driver");
    NameDevice();
    File.WriteAllText(Path.Combine(Folder,"install-result.txt"),reboot?"Restart Windows to finish installing the driver.":"Driver installed.");
    return reboot?3010:0;
   }catch {
    bool removed=!registered || SetupDiCallClassInstaller(5,set,ref info);
    if(removed && File.Exists(Ownership))File.Delete(Ownership);
    if(removed) {if(old!=null)File.WriteAllText(config,old);else File.Delete(config);}
    throw;
   }finally{if(set!=new nint(-1))SetupDiDestroyDeviceInfoList(set);}
  }catch(Exception ex){File.WriteAllText(Path.Combine(Folder,"install-result.txt"),ex.Message);return 1;}
 }
}
