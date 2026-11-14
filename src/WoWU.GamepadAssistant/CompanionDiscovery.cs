using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
namespace WoWU.GamepadAssistant;
internal static class CompanionDiscovery
{
 private static string Cache=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant","game-folders.json");
 internal static string GameFolder(Game game)
 {
  using var process=Process.GetProcessById(game.Pid);
  if(process.StartTime.ToUniversalTime().Ticks!=game.Started)throw new InvalidOperationException("The game process changed.");
  string folder=Path.GetDirectoryName(process.MainModule!.FileName)!;
  Remember(folder);return folder;
 }
 private static void Remember(string folder)
 {
  var folders=Remembered();folders.Add(folder);
  Directory.CreateDirectory(Path.GetDirectoryName(Cache)!);
  File.WriteAllText(Cache,JsonSerializer.Serialize(folders.Distinct(StringComparer.OrdinalIgnoreCase)));
 }
 private static List<string> Remembered()
 {
  try{return File.Exists(Cache)?JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Cache)) ?? new():new();}catch(IOException){return new();}catch(JsonException){return new();}
 }
 internal static List<string> SavedFiles(string folder)
 {
  if(!File.Exists(Path.Combine(folder,"Interface","AddOns","WoWU","CompanionConfig.lua")))return new();
  string accounts=Path.Combine(folder,"WTF","Account");
  if(!Directory.Exists(accounts))return new();
  return Directory.EnumerateDirectories(accounts).Select(a=>Path.Combine(a,"SavedVariables","WoWU.lua")).Where(File.Exists).ToList();
 }
 internal static List<string> Scan()
 {
  var roots=Remembered();
  foreach(var game in Discovery.Games())try{roots.Add(GameFolder(game));}catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}
  foreach(var hive in new[]{Registry.LocalMachine,Registry.CurrentUser})foreach(var branch in new[]{@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"}) {
   using var uninstall=hive.OpenSubKey(branch);if(uninstall==null)continue;
   foreach(string name in uninstall.GetSubKeyNames()) {
    using var key=uninstall.OpenSubKey(name);
    if((key?.GetValue("DisplayName") as string)?.Contains("Warcraft",StringComparison.OrdinalIgnoreCase)==true && key?.GetValue("InstallLocation") is string location)roots.Add(location);
   }
  }
  // Inspect common install folders only, never scan entire drives.
  foreach(var drive in DriveInfo.GetDrives().Where(d=>d.DriveType==DriveType.Fixed && d.IsReady))
   foreach(string relative in new[]{"World of Warcraft",@"Games\World of Warcraft",@"Program Files (x86)\World of Warcraft",@"Program Files\World of Warcraft"})roots.Add(Path.Combine(drive.RootDirectory.FullName,relative));
  var found=new List<string>();
  foreach(string folder in roots.Distinct(StringComparer.OrdinalIgnoreCase))try {
   var candidates=new List<string>{folder};
   if(Directory.Exists(folder))candidates.AddRange(Directory.EnumerateDirectories(folder,"_*",SearchOption.TopDirectoryOnly));
   foreach(string candidate in candidates) {
    var files=SavedFiles(candidate);if(files.Count==0)continue;
    found.AddRange(files);Remember(candidate);
   }
  }catch(IOException){}catch(UnauthorizedAccessException){}
  return found.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
 }
}
