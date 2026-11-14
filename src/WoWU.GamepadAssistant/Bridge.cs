using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
namespace WoWU.GamepadAssistant;
internal sealed class Bridge
{
 internal Game Game {get;private set;}
 internal Native.Rect Target {get;}
 internal string Topology {get;}
 private readonly string token=Guid.NewGuid().ToString("N");
 private readonly List<string> savedFiles;
 private readonly Dictionary<string,string> previous=new(StringComparer.OrdinalIgnoreCase);
 private string? activeFile;
 private readonly string configFile;
 private readonly int width,height,left;
 private string? handoff;
 private long refreshHandoffAt;
 private readonly long armedAt=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
 private long activeId;
 internal bool Started {get;private set;}
 internal bool SingleReload {get;private set;}
 internal Bridge(Game game,Display main,Display touch,IEnumerable<string> saves)
 {
  Game=game;var all=Discovery.Displays();Target=Discovery.Plan(main,touch,all);Topology=Discovery.Signature(all);
  using var process=Process.GetProcessById(game.Pid);
  if(process.StartTime.ToUniversalTime().Ticks!=game.Started)throw new InvalidOperationException("The game process changed.");
  string root=Path.GetDirectoryName(process.MainModule!.FileName)!;
  savedFiles=saves.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
  string accountRoot=Path.Combine(root,"WTF","Account")+Path.DirectorySeparatorChar;
  if(savedFiles.Count==0 || savedFiles.Any(saved=>!saved.StartsWith(accountRoot,StringComparison.OrdinalIgnoreCase) || !string.Equals(Path.GetFileName(saved),"WoWU.lua",StringComparison.OrdinalIgnoreCase) || !File.Exists(saved)))
   throw new InvalidOperationException("WoWU saved data has not been found for this game yet.");
  configFile=Path.Combine(root,"Interface","AddOns","WoWU","CompanionConfig.lua");
  if(!File.Exists(configFile))throw new InvalidOperationException("Install WoWU poc9 or later first.");
  if(!File.Exists(configFile+".assistant-backup"))File.Copy(configFile,configFile+".assistant-backup");
  width=main.Bounds.Width;height=main.Bounds.Height;left=touch.Bounds.Left-main.Bounds.Left;
  EnsureHandoff();
  // Ignore any request that was already on disk when the user armed us.
  foreach(string file in savedFiles) {
   var existing=Regex.Match(ReadSaved(file),"(?m)^WoWUBridgeSignal.*$");
   if(existing.Success)previous[file]=existing.Value.TrimEnd();
  }
 }
 internal void EnsureHandoff()
 {
  long now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
  if(handoff==null || now>=refreshHandoffAt) {
   handoff=$"-- Generated Assistant handoff; no SavedVariables are modified.\nWoWUCompanionConfig={{schema=1,token=\"{token}\",expires={now+7200},width={width},height={height},left={left}}}\n";
   refreshHandoffAt=now+3600;
  }
  if(File.Exists(configFile) && ReadSaved(configFile)==handoff)return;
  File.WriteAllText(configFile+".tmp",handoff);
  File.Move(configFile+".tmp",configFile,true);
 }
 private static string ReadSaved(string file)
 {
  using var stream=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
  using var reader=new StreamReader(stream);return reader.ReadToEnd();
 }
 internal string? Read()
 {
  foreach(string file in savedFiles) {
   if(activeFile!=null && !string.Equals(activeFile,file,StringComparison.OrdinalIgnoreCase))continue;
   try{string? phase=ReadFile(file);if(phase!=null)return phase;}catch(IOException){}
  }
  return null;
 }
 private string? ReadFile(string file)
 {
  if(!File.Exists(file))return null;
  string text=ReadSaved(file);
  var match=Regex.Match(text,"(?m)^WoWUBridgeSignal\\s*=\\s*\"([a-f0-9]+)\\|(\\d+)\\|(request|activate|confirmed|off)\\|(\\d+)\"");
  if(!match.Success || (previous.TryGetValue(file,out string? last) && match.Value==last))return null;
  bool bootstrap=match.Groups[1].Value==new string('0',32) && (match.Groups[3].Value=="request" || match.Groups[3].Value=="activate" || match.Groups[3].Value=="confirmed");
  if(match.Groups[1].Value!=token && !bootstrap)return null;
  if(!long.TryParse(match.Groups[4].Value,out long stamp) || stamp<armedAt || Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds()-stamp)>90)return null;
  if(!long.TryParse(match.Groups[2].Value,out long id) || id<=0)return null;
  string phase=match.Groups[3].Value;
  if(Started && (phase=="request" || phase=="activate")){
   // A retry from the same save uses the existing span. Its new ID must
   // still be accepted when the player switches Gamepad Mode off.
   previous[file]=match.Value;
   if(phase=="activate" && id>activeId)activeId=id;
   return null;
  }
  // The second reload can finish before the next poll: a fresh confirmation
  // carries the original enable intent. Start once, then consume it as confirmation.
  if(phase=="confirmed" && !Started){activeFile=file;activeId=id;return "request";}
  previous[file]=match.Value;
  if(phase=="request" || phase=="activate") {if(Started)return null;SingleReload=phase=="activate";activeFile=file;activeId=id;return "request";}
  return id==activeId?phase:null;
 }
 internal void RefreshWindow()
 {
  using var p=Process.GetProcessById(Game.Pid);
  if(p.StartTime.ToUniversalTime().Ticks!=Game.Started)throw new InvalidOperationException("The game process changed during display-mode switching.");
  nint handle=p.MainWindowHandle;
  if(handle==0 || !Native.IsWindow(handle) || !Native.IsWindowVisible(handle))throw new InvalidOperationException("The game window is not ready after display-mode switching. Try Gamepad Mode again.");
  Native.GetWindowThreadProcessId(handle,out uint pid);
  if(pid!=Game.Pid)throw new InvalidOperationException("The game window identity changed.");
  Game=Game with{Handle=handle};
 }
 internal void MarkStarted()=>Started=true;
 internal void Stop()
 {
  Started=false;
  // Invalidate the handoff for the next reload without touching game-owned saves.
  File.WriteAllText(configFile,"-- Assistant connection ended. Prepare again to reconnect.\nWoWUCompanionConfig=nil\n");
 }
}
