using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace WoWU.GamepadAssistant;
internal sealed record PairRequest(string Id,string Name,string Address){public string Label=>$"{Name} ({Address})";}
internal sealed class SunshineHost
{
 private static readonly string Root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant","streaming");
 // Construct only after static paths have been initialized.
 private static readonly Lazy<SunshineHost> Host=new(()=>new SunshineHost());
 internal static SunshineHost Instance=>Host.Value;
 private SunshineHost(){}
 private const string Version="v2026.914.233613";
 private string Engine=>Path.Combine(Root,Version,"Sunshine","sunshine.exe");
 private string Config=>Path.Combine(Root,"sunshine.conf");
 private readonly string logSession=Guid.NewGuid().ToString("N");
 private int logNumber;
 private string Log {get;set;}=Path.Combine(Root,"sunshine.log");
 private Process? process;
 private ChildProcessJob? lifetime;
 private string password="";
 private string? topology;
 private HttpClient? client;
 internal string Status {get;private set;}="Streaming stopped.";
 internal bool Running=>process!=null && !process.HasExited;
 private static string P(string path)=>path.Replace('\\','/');
 internal async Task Prepare(Action<string>? report=null)
 {
  Stop();
  try {
   using var guard=new HostConflictCheck();
   var adapter=DisplaySetup.OwnedAdapter();var displays=Discovery.Displays();
   var target=displays.SingleOrDefault(d=>d.Name==adapter.Name) ?? throw new InvalidOperationException("The owned WoWU monitor is not enabled.");
   if(target.Bounds.Width!=1280 || target.Bounds.Height!=800 || target.Primary)throw new InvalidOperationException("WoWU capture requires the non-primary 1280 x 800 display.");
   topology=Discovery.Signature(displays);
   report?.Invoke("Preparing the integrated streaming engine");
   await DownloadEngine();
   Directory.CreateDirectory(Root);
   password=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
   // Discovery is loopback-only and exposes no application to clients.
   WriteConfig("{00000000-0000-0000-0000-000000000000}",true,false);
   await Credentials();
   report?.Invoke("Matching Sunshine to the WoWU display");
   StartProcess();
   string? id=null;
   for(int i=0;i<100 && id==null;i++) {
    await Task.Delay(200);EnsureAlive();
    id=InventoryId(ReadLog(),target.Name);
   }
   if(id==null)throw new InvalidOperationException("Sunshine did not identify the WoWU display. Streaming remains stopped. Diagnostic log: "+Log);
   StopProcess();
   report?.Invoke("Checking capture of the 1280 x 800 display");
   WriteConfig(id,true,false);StartProcess();
   int expectedX=target.Bounds.Left-displays.Min(d=>d.Bounds.Left),expectedY=target.Bounds.Top-displays.Min(d=>d.Bounds.Top);
   bool verified=false;
   for(int i=0;i<150;i++) {
    await Task.Delay(200);EnsureAlive();CheckTopology();
    var log=ReadLog();
    var captures=Regex.Matches(log,@"Capture size\s*:\s*(\d+)x(\d+)\s*\r?\nOffset\s*:\s*(-?\d+)x(-?\d+)");
    foreach(Match c in captures) {
     if(c.Groups[1].Value!="1280" || c.Groups[2].Value!="800" || int.Parse(c.Groups[3].Value)!=expectedX || int.Parse(c.Groups[4].Value)!=expectedY)
      throw new InvalidOperationException("Sunshine captured an unexpected display rectangle. Streaming was stopped.");
    }
    if(captures.Count>0 && log.Contains("Desktop resolution [1280x800]") && log.Contains("Found H.264 encoder:")){verified=true;break;}
   }
   if(!verified)throw new InvalidOperationException("Sunshine capture diagnostics could not verify the WoWU display. Streaming remains stopped.");
   CheckTopology();StopProcess();
   report?.Invoke("Starting Moonlight streaming");
   WriteConfig(id,false,true);StartProcess();
   await ConnectApi();
   CheckTopology();
   Status="Host available - WoWU capture rectangle checked. Set Moonlight to 1280 x 800; receiver stream not yet verified.";
  }catch(Exception ex){Stop();Status="Streaming needs attention: "+ex.Message;throw;}
 }
 private async Task DownloadEngine()
 {
  string archive=Path.Combine(Root,"sunshine.zip");Directory.CreateDirectory(Root);
  const string hash="233008E46F4C0E501A586CBFD6C4FD4A4C0D414A0B5FC7F13C070EB92EC3824B";
  if(!File.Exists(archive) || Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(archive)))!=hash) {
   using var http=new HttpClient{Timeout=TimeSpan.FromMinutes(5)};
   using var response=await http.GetAsync($"https://github.com/LizardByte/Sunshine/releases/download/{Version}/Sunshine-Windows-AMD64-lite.zip",HttpCompletionOption.ResponseHeadersRead);
   response.EnsureSuccessStatusCode();
   await using(var file=File.Create(archive+".download"))await response.Content.CopyToAsync(file);
   if(Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(archive+".download")))!=hash)throw new InvalidOperationException("Streaming engine download integrity check failed.");
   File.Move(archive+".download",archive,true);
  }
  // Restore pristine engine files from the verified upstream archive each startup.
  ZipFile.ExtractToDirectory(archive,Path.Combine(Root,Version),true);
 }
 private void WriteConfig(string id,bool local,bool advertise)
 {
  string logFolder=Path.Combine(Root,"logs",logSession);Directory.CreateDirectory(logFolder);
  Log=Path.Combine(logFolder,$"sunshine-{++logNumber:D3}.log");
  string apps=Path.Combine(Root,"apps.json");
  File.WriteAllText(apps,advertise?"{\"env\":{},\"apps\":[{\"name\":\"WoWU Gamepad\"}]}":"{\"env\":{},\"apps\":[]}");
  Directory.CreateDirectory(Path.Combine(Root,"credentials"));
  File.WriteAllText(Config,$"sunshine_name = WoWU Gamepad\nstream_audio = disabled\ninstall_steam_audio_drivers = disabled\nsystem_tray = disabled\nupnp = disabled\norigin_web_ui_allowed = pc\naddress_family = ipv4\nbind_address = {(local?"127.0.0.1":"0.0.0.0")}\noutput_name = {id}\ncapture = ddx\ndd_configuration_option = verify_only\ndd_resolution_option = disabled\ndd_refresh_rate_option = disabled\ndd_hdr_option = disabled\nfile_apps = {P(apps)}\ncredentials_file = {P(Path.Combine(Root,"web-credentials.json"))}\nfile_state = {P(Path.Combine(Root,"pairings.json"))}\npkey = {P(Path.Combine(Root,"credentials","cakey.pem"))}\ncert = {P(Path.Combine(Root,"credentials","cacert.pem"))}\nlog_path = {P(Log)}\nmin_log_level = info\n");
 }
 private ProcessStartInfo StartInfo()
 {
  var p=new ProcessStartInfo(Engine){WorkingDirectory=Path.GetDirectoryName(Engine)!,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
  p.ArgumentList.Add(Config);return p;
 }
 private async Task Credentials()
 {
  var info=StartInfo();info.ArgumentList.Add("--creds");info.ArgumentList.Add("wowu");info.ArgumentList.Add(password);
  using var p=Process.Start(info) ?? throw new InvalidOperationException("Cannot initialise streaming credentials.");
  using var commandLifetime=new ChildProcessJob(p);
  var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();
  await p.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));await Task.WhenAll(stdout,stderr);
  if(p.ExitCode!=0)throw new InvalidOperationException("Could not initialise isolated Sunshine credentials.");
 }
 private void StartProcess()
 {
  // Never delete/truncate a log that Sunshine, a child, or a log viewer may
  // still have open. Each host stage gets its own diagnostics file.
  string folder=Path.Combine(Root,"logs",logSession);Directory.CreateDirectory(folder);
  Log=Path.Combine(folder,$"sunshine-{++logNumber:D3}.log");
  var lines=File.ReadAllLines(Config).ToList();
  int logLine=lines.FindIndex(line=>line.TrimStart().StartsWith("log_path =",StringComparison.Ordinal));
  if(logLine<0)throw new InvalidOperationException("The managed Sunshine configuration is missing its log path.");
  lines[logLine]="log_path = "+P(Log);
  File.WriteAllLines(Config,lines);
  process=Process.Start(StartInfo()) ?? throw new InvalidOperationException("Could not start the managed Sunshine engine.");
  lifetime=new ChildProcessJob(process);
  process.OutputDataReceived+=(_,_)=>{};process.ErrorDataReceived+=(_,_)=>{};process.BeginOutputReadLine();process.BeginErrorReadLine();
 }
 private void EnsureAlive(){if(!Running)throw new InvalidOperationException("The managed Sunshine engine exited. See streaming/logs in the Assistant data folder.");}
 private string ReadLog(){try{using var f=new FileStream(Log,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);using var r=new StreamReader(f);return r.ReadToEnd();}catch(IOException){return "";}}
 private static string? InventoryId(string log,string display)
 {
  int start=log.IndexOf("Currently available display devices:",StringComparison.Ordinal);if(start<0)return null;
  start=log.IndexOf('[',start);if(start<0)return null;
  // Balanced JSON array; log prefix timestamps are outside this region.
  bool quoted=false,escape=false;int depth=0;
  for(int i=start;i<log.Length;i++) {
   char c=log[i];if(escape){escape=false;continue;}if(quoted && c=='\\'){escape=true;continue;}if(c=='"'){quoted=!quoted;continue;}if(quoted)continue;
   if(c=='[')depth++;if(c==']' && --depth==0) {
    using var doc=JsonDocument.Parse(log.Substring(start,i-start+1));
    var matches=doc.RootElement.EnumerateArray().Where(x=>x.GetProperty("display_name").GetString()==display).ToArray();
    if(matches.Length!=1)throw new InvalidOperationException("Sunshine's WoWU display match is missing or ambiguous.");
    string? id=matches[0].GetProperty("device_id").GetString();if(!Guid.TryParse(id,out _))throw new InvalidOperationException("Invalid Sunshine display identity.");return id;
   }
  }return null;
 }
 private async Task ConnectApi()
 {
  string certFile=Path.Combine(Root,"credentials","cacert.pem");
  for(int i=0;i<75;i++) {
   await Task.Delay(200);EnsureAlive();
   if(!File.Exists(certFile))continue;
   using var expected=X509Certificate2.CreateFromPem(File.ReadAllText(certFile));
   string fingerprint=expected.GetCertHashString(HashAlgorithmName.SHA256);
   var handler=new HttpClientHandler{UseProxy=false,ServerCertificateCustomValidationCallback=(request,cert,chain,errors)=>request.RequestUri?.Host=="127.0.0.1" && cert?.GetCertHashString(HashAlgorithmName.SHA256)==fingerprint};
   client?.Dispose();client=new HttpClient(handler){BaseAddress=new Uri("https://127.0.0.1:47990/"),Timeout=TimeSpan.FromSeconds(8)};
   client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.UTF8.GetBytes("wowu:"+password)));
   try{using var response=await client.GetAsync("api/config");if(response.IsSuccessStatusCode)return;}catch(HttpRequestException){}catch(TaskCanceledException){}
  }throw new InvalidOperationException("The integrated pairing API did not become available.");
 }
 internal async Task<List<PairRequest>> Pending()
 {
  EnsureAlive();CheckTopology();using var doc=await client!.GetFromJsonAsync<JsonDocument>("api/pin");
  return doc!.RootElement.GetProperty("pairings").EnumerateArray().Select(p=>new PairRequest(p.GetProperty("id").GetString()!,p.GetProperty("name").GetString()!,p.GetProperty("address").GetString()!)).ToList();
 }
 internal async Task Pair(PairRequest request,string pin)
 {
  EnsureAlive();CheckTopology();if(!Regex.IsMatch(pin,"^[0-9]{4}$"))throw new InvalidOperationException("Enter the four-digit Moonlight PIN.");
  using var token=await client!.GetFromJsonAsync<JsonDocument>("api/csrf-token");
  using var message=new HttpRequestMessage(HttpMethod.Post,"api/pin");message.Headers.Add("X-CSRF-Token",token!.RootElement.GetProperty("csrf_token").GetString());
  message.Content=JsonContent.Create(new{pairing_id=request.Id,pin,name=request.Name});using var response=await client!.SendAsync(message);response.EnsureSuccessStatusCode();
  using var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync());if(!result.RootElement.GetProperty("status").GetBoolean())throw new InvalidOperationException("Moonlight pairing was rejected.");
 }
 internal void CheckTopology()
 {
  if(topology!=null && Discovery.Signature(Discovery.Displays())!=topology){Stop();throw new InvalidOperationException("Display arrangement changed. Streaming stopped; restart it to resolve the capture target again.");}
 }
 private void StopProcess()
 {
  client?.Dispose();client=null;
  var stopping=process;process=null;
  // Closing the job starts termination; wait for exit before another stage starts.
  lifetime?.Dispose();lifetime=null;
  if(stopping==null)return;
  using(stopping) {
   if(!stopping.WaitForExit(5000)) {
    try{stopping.Kill(true);}catch(InvalidOperationException) when(stopping.HasExited){}
    if(!stopping.WaitForExit(5000))throw new InvalidOperationException("The previous streaming engine is still closing. Try Start streaming again shortly.");
   }
  }
 }
 internal void Stop(){StopProcess();topology=null;Status="Streaming stopped.";}
 private sealed class HostConflictCheck:IDisposable
 {
  public HostConflictCheck(){foreach(var p in Process.GetProcessesByName("sunshine")){p.Dispose();throw new InvalidOperationException("Another Sunshine instance is running. Close it before using integrated streaming.");}
   foreach(var p in Process.GetProcessesByName("sunshinesvc")){p.Dispose();throw new InvalidOperationException("An existing Sunshine service is running. Stop or uninstall it before using integrated streaming.");}
   var ports=IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();if(ports.Any(p=>p.Port is 47984 or 47989 or 47990 or 48010))throw new InvalidOperationException("Sunshine ports are already in use.");}
  public void Dispose(){}
 }
}
