using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
namespace WoWU.GamepadAssistant;
internal static class DisplaySetup
{
 private sealed record Saved(string DeviceId,byte[] Mode);
 private static string Journal=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant","display-position.json");
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern bool EnumDisplaySettingsEx(string device,int mode,[In,Out]byte[] data,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int ChangeDisplaySettingsEx(string device,[In]byte[] data,nint window,uint flags,nint param);
 private static byte[] Mode(string name,int index=-1)
 {
  byte[] data=new byte[220];BitConverter.GetBytes((short)220).CopyTo(data,68);
  if(!EnumDisplaySettingsEx(name,index,data,0))throw new InvalidOperationException("Windows has no mode information for the virtual display.");return data;
 }
 private static void Put(byte[] mode,int offset,int value)=>BitConverter.GetBytes(value).CopyTo(mode,offset);
 internal static Native.Device OwnedAdapter()
 {
  if(!File.Exists(DriverInstall.Ownership))throw new InvalidOperationException("Install the WoWU virtual display first.");
  string id=File.ReadAllText(DriverInstall.Ownership).Trim();
  if(!id.StartsWith(@"ROOT\WOWUVIRTUALDISPLAY\",StringComparison.OrdinalIgnoreCase))
   throw new InvalidOperationException("The saved display identity is not a WoWU-created device.");
  // EnumDisplayDevices reports the hardware ID (Root\MttVDD), not the
  // unique PnP instance ID saved when we created the device. Resolve the
  // instance's current VideoID instead of guessing from a name or size.
  using var device=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\"+id);
  using var parameters=device?.OpenSubKey("Device Parameters");
  string? video=parameters?.GetValue("VideoID") as string;
  if(!Guid.TryParse(video,out var videoId))
   throw new InvalidOperationException("Windows has not assigned a display identity to the WoWU driver yet. Restart Windows if driver setup requested it, then reopen the Assistant.");
  string prefix=@"\Registry\Machine\System\CurrentControlSet\Control\Video\"+videoId.ToString("B")+@"\";
  var matches=new List<Native.Device>();
  for(uint n=0;;n++) {
   var d=new Native.Device{Size=Marshal.SizeOf<Native.Device>(),Name="",Description="",Id="",Key=""};
   if(!Native.EnumDisplayDevices(null,n,ref d,0))break;
   if(d.Key.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))matches.Add(d);
  }
  // An owned driver may expose multiple outputs in other configurations.
  // Never pick an arbitrary output or silently fall back to another screen.
  var active=matches.Where(d=>(d.Flags&1)!=0).ToList();
  Native.Device selected;
  if(active.Count==1)selected=active[0];
  else if(active.Count==0 && matches.Count==1)selected=matches[0];
  else if(matches.Count==0)throw new InvalidOperationException("The recorded WoWU adapter is not available. Restart Windows if installation requested it; no other display was selected.");
  else throw new InvalidOperationException("The WoWU driver exposes multiple display outputs. Streaming is stopped until a single WoWU output can be identified.");
  // Recovery journals retain the device instance identity across display renumbering.
  selected.Id=id;
  return selected;
 }

 internal static Task Install()=>RunHelper("--install-display");
 internal static async Task EnsureDriver()
 {
  // Device enumeration can lag briefly during enable/disable or driver updates.
  if(File.Exists(DriverInstall.Ownership)) {
   for(int i=0;i<8 && DriverInstall.IsMissing();i++)await Task.Delay(500);
  }
  if(!File.Exists(DriverInstall.Ownership) || DriverInstall.IsMissing()) {
   // A recreated device must not inherit an old device's recovery geometry.
   if(File.Exists(Journal))File.Move(Journal,Journal+".previous-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
   await Install();
  }
  else if(DriverInstall.IsDisabled())await RunHelper("--enable-display");
  if(!DriverInstall.HasFriendlyName())await RunHelper("--name-display");
  for(int i=0;i<30;i++) {
   try{OwnedAdapter();return;}catch(InvalidOperationException){if(i==29)throw;}
   await Task.Delay(500);
  }
 }
 internal static Task DisableDriver()=>File.Exists(DriverInstall.Ownership) && !DriverInstall.IsMissing() && !DriverInstall.IsDisabled()?RunHelper("--disable-display"):Task.CompletedTask;
 private static async Task RunHelper(string argument)
 {
  var start=new ProcessStartInfo(Environment.ProcessPath!,argument){UseShellExecute=true,Verb="runas",WindowStyle=ProcessWindowStyle.Hidden};
  using var p=Process.Start(start) ?? throw new InvalidOperationException("Could not start display setup.");
  await p.WaitForExitAsync();
  if(p.ExitCode!=0 && p.ExitCode!=3010)throw new InvalidOperationException(File.Exists(Path.Combine(DriverInstall.Folder,"install-result.txt"))?File.ReadAllText(Path.Combine(DriverInstall.Folder,"install-result.txt")):"Display setup was cancelled or failed.");
 }
 internal static async Task<Display> Configure(Display main)
 {
  await EnsureDriver();
  var d=OwnedAdapter();var before=Discovery.Displays();
  var currentMain=before.SingleOrDefault(x=>x.Name==main.Name && x.Identity==main.Identity) ?? throw new InvalidOperationException("Gameplay monitor changed. Refresh and select it again.");
  if(d.Name==main.Name || (d.Flags&4)!=0)throw new InvalidOperationException("The virtual display cannot be the gameplay or primary monitor.");
  if(main.Bounds.Width<1280)throw new InvalidOperationException("The gameplay monitor must be at least 1280 pixels wide.");
  int x=main.Bounds.Left+(main.Bounds.Width-1280)/2,y=main.Bounds.Bottom;
  var span=new Native.Rect{Left=main.Bounds.Left,Top=main.Bounds.Top,Right=main.Bounds.Right,Bottom=y+800};
  foreach(var other in before.Where(o=>o.Name!=d.Name && o.Name!=main.Name)) {
   var r=other.Bounds;if(r.Left<span.Right && r.Right>span.Left && r.Top<span.Bottom && r.Bottom>span.Top)throw new InvalidOperationException("Another monitor occupies the space below the gameplay monitor. Its position was not changed.");
  }
  var original=Mode(d.Name,(d.Flags&1)!=0?-1:-2);var desired=(byte[])original.Clone();
  Put(desired,72,0x20|0x40000|0x80000|0x100000|0x400000|0x8000000); // position, bpp, width, height, frequency, orientation
  Put(desired,76,x);Put(desired,80,y);Put(desired,84,0);Put(desired,168,32);Put(desired,172,1280);Put(desired,176,800);Put(desired,184,60);
  int result=ChangeDisplaySettingsEx(d.Name,desired,0,2,0);
  if(result!=0)throw new InvalidOperationException($"Windows rejected 1280 x 800 at 60 Hz (code {result}).");
  if((d.Flags&1)==0){Put(original,172,0);Put(original,176,0);Put(original,72,0x20|0x80000|0x100000);}
  Directory.CreateDirectory(Path.GetDirectoryName(Journal)!);
  if(!File.Exists(Journal)) {File.WriteAllText(Journal+".tmp",JsonSerializer.Serialize(new Saved(d.Id,original)));File.Move(Journal+".tmp",Journal);}
  try {
   result=ChangeDisplaySettingsEx(d.Name,desired,0,0,0);
   if(result!=0)throw new InvalidOperationException($"Could not position virtual display (code {result}).");
   await Task.Delay(1500);
   var after=Discovery.Displays();var target=after.SingleOrDefault(o=>o.Name==d.Name);
   if(target==null || target.Bounds.Left!=x || target.Bounds.Top!=y || target.Bounds.Width!=1280 || target.Bounds.Height!=800)throw new InvalidOperationException("Windows did not retain the requested virtual display geometry.");
   foreach(var prior in before.Where(o=>o.Name!=d.Name)) {
    var now=after.SingleOrDefault(o=>o.Identity==prior.Identity && o.Name==prior.Name);
    if(now==null || !now.Bounds.Equals(prior.Bounds) || now.Primary!=prior.Primary)throw new InvalidOperationException("Another monitor changed during setup; virtual display settings will be restored.");
   }
   return target;
  }catch{Restore();throw;}
 }
 internal static void Restore()
 {
  if(!File.Exists(Journal))return;
  var saved=JsonSerializer.Deserialize<Saved>(File.ReadAllText(Journal)) ?? throw new InvalidOperationException("Display recovery data is invalid.");
  if(DriverInstall.IsMissing())return; // Retain recovery journal for the missing device.
  if(DriverInstall.IsDisabled())return; // No active display geometry to restore.
  var d=OwnedAdapter();if(!string.Equals(d.Id,saved.DeviceId,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Display identity changed; recovery was not applied to a different device.");
  if(saved.Mode.Length!=220)throw new InvalidOperationException("Display recovery mode is invalid.");
  int result=ChangeDisplaySettingsEx(d.Name,saved.Mode,0,0,0);
  if(result!=0)throw new InvalidOperationException($"Display restoration failed (code {result}).");File.Delete(Journal);
 }
}
