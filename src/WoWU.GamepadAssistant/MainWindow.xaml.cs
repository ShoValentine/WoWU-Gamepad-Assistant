using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
namespace WoWU.GamepadAssistant;
public partial class MainWindow : Window
{
 private readonly Session session=new();
 private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromSeconds(1)};
 private readonly DispatcherTimer pairingTimer=new(){Interval=TimeSpan.FromSeconds(3)};
 private readonly Dictionary<string,PairingWindow> pairingWindows=new();
 private readonly HashSet<string> seenPairings=new();
 private bool busy,tickBusy,pairPollBusy,closing,closeReady,closeRequested;
 private Bridge? bridge;
 private Game? game;
 private Display? mainDisplay,touchDisplay;
 private int ticks,gameCount;
 private long? spanMismatchSince;
 private bool checkingUpdates;
 private DateTime nextUpdateCheck=DateTime.UtcNow.AddSeconds(5);
 private string? pendingInstaller;
 private string? issue,connectionIssue,pairPollIssue;
 public MainWindow(bool startupHandled=false)
 {
  InitializeComponent();
  try{if(!startupHandled){session.Recover();DisplaySetup.Restore();}RefreshLists();}
  catch(Exception ex){issue="Recovery needs attention: "+ex.Message;App.RecordError(ex);}
  timer.Tick+=Tick;pairingTimer.Tick+=PollPairings;timer.Start();pairingTimer.Start();UpdateStatus();
 }
 internal void ApplyStartup(Display? main,Display? touch,string? startupIssue,List<string>? savedData=null)
 {
  mainDisplay=main;touchDisplay=touch;issue=startupIssue ?? issue;
  UpdateGame();TryAutoConnect();UpdateStatus();
 }
 internal void FadeIn()=>BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(300)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseOut}});
 private void UpdateGame()
 {
  var games=Discovery.Games();gameCount=games.Count;
  game=games.FirstOrDefault(g=>g.Pid==game?.Pid && g.Started==game.Started) ?? (games.Count==1?games[0]:null);
  if(game==null)connectionIssue=null;
 }
 private void RefreshLists()
 {
  UpdateGame();var displays=Discovery.Displays();mainDisplay=displays.FirstOrDefault(d=>d.Primary);touchDisplay=null;
  try{string name=DisplaySetup.OwnedAdapter().Name;touchDisplay=displays.SingleOrDefault(d=>d.Name==name);}catch(InvalidOperationException){}
 }
 private void UpdateStatus()
 {
  string text;string color="#A9B78B";
  if(closing || closeRequested){text="Putting everything back...";color="#D8B766";}
  else if(issue!=null){text=issue;color="#EEA16C";}
  else if(!SunshineHost.Instance.Running){text=SunshineHost.Instance.Status.StartsWith("Streaming needs attention:")?SunshineHost.Instance.Status:"Streaming unavailable. Restart the Assistant to reconnect.";color="#EEA16C";}
  else if(pairPollIssue!=null){text="Pairing needs attention. Restart the Assistant to reconnect.";color="#EEA16C";}
  else if(pairingWindows.Count>0){text="Enter your Moonlight PIN in the pairing window.";color="#D8B766";}
  else if(busy){text="Connecting Gamepad Mode...";color="#D8B766";}
  else if(session.Active){text="Gamepad Mode connected.";color="#8BC68A";}
  else if(connectionIssue!=null){text=connectionIssue;color="#D8B766";}
  else if(game==null){text=gameCount>1?"Close extra WoW clients to connect.":"Ready. Waiting for World of Warcraft.";}
  else if(bridge!=null){text="Ready. Enable Gamepad Mode in WoW.";color="#8BC68A";}
  else{text="Ready. Waiting for WoWU.";}
  Status.Text=text;StatusLight.Fill=new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
 }
 private void TryAutoConnect()
 {
  if(session.Active || session.RecoveryPending)return;
  if(bridge!=null) {
   if(game!=null && bridge.Game.Pid==game.Pid && bridge.Game.Started==game.Started && bridge.Topology==Discovery.Signature(Discovery.Displays()))return;
   bridge.Stop();bridge=null;connectionIssue=null;
  }
  if(game==null || mainDisplay==null || touchDisplay==null)return;
  try {
   var files=CompanionDiscovery.SavedFiles(CompanionDiscovery.GameFolder(game));
   if(files.Count==0){connectionIssue="Waiting for WoWU saved data. Log in with the addon enabled, then reload once.";return;}
   bridge=new Bridge(game,mainDisplay,touchDisplay,files);connectionIssue=null;
  }catch(Exception ex){string message="Game connection: "+ex.Message;if(connectionIssue!=message)App.RecordError(ex);connectionIssue=message;}
 }
 private async Task PollBridge()
 {
  if(bridge==null)return;
  string? phase=bridge.Read();
  if(phase=="request") {
   busy=true;UpdateStatus();
   try {
    if(Discovery.Signature(Discovery.Displays())!=bridge.Topology)throw new InvalidOperationException("Displays changed. Restart the Assistant to reconnect.");
    bridge.RefreshWindow();await session.Start(bridge.Game,bridge.Target,bridge.Topology);bridge.MarkStarted();
    if(bridge.SingleReload)session.Keep();else session.ExtendPreview();
    issue=null;
   }catch(Exception ex){App.RecordError(ex);RestoreNow("Connection failed: "+ex.Message);}
   finally{busy=false;UpdateStatus();if(closeRequested)Close();}
  }else if(phase=="confirmed" && session.Active && bridge.Started){session.Keep();issue=null;}
  else if(phase=="off")RestoreNow();
  if(!closing)bridge?.EnsureHandoff();
 }
 private void RestoreNow(string? message=null)
 {
  spanMismatchSince=null;
  try{session.Restore();bridge?.Stop();bridge=null;issue=message;connectionIssue=null;}
  catch(Exception ex){issue="Restoration needs attention: "+ex.Message;App.RecordError(ex);}
  UpdateStatus();
 }
 private async void Tick(object? sender,EventArgs e)
 {
  if(busy || tickBusy || closing)return;
  tickBusy=true;
  try {
   if(SunshineHost.Instance.Running)SunshineHost.Instance.CheckTopology();
   await PollBridge();if(closing)return;
   if(session.Active) {
    if(!session.SameWindow(allowMissingMainWindow:true)){RestoreNow();return;}
    if(!session.TopologyMatches()){RestoreNow("Displays changed. Restart the Assistant to reconnect.");return;}
    if(session.RevertAt is DateTime until && until<=DateTime.UtcNow){RestoreNow("Connection confirmation timed out. Enable Gamepad Mode again.");return;}
    if(session.IsMinimized || session.MatchesTarget())spanMismatchSince=null;
    else if(session.RevertAt!=null){RestoreNow("The game window changed. Enable Gamepad Mode again.");return;}
    else {
     // WoW can briefly restore its normal window geometry while reloading.
     // Reapply the kept span using the original recovery record.
     long now=Environment.TickCount64;
     spanMismatchSince??=now;
     long elapsed=now-spanMismatchSince.Value;
     if(elapsed>=15000){RestoreNow("The game window could not recover its span. Enable Gamepad Mode again.");return;}
     if(elapsed>=1000)session.ReapplySpan();
    }
   }else if(++ticks%5==0 && !session.RecoveryPending){RefreshLists();TryAutoConnect();}
  }catch(Exception ex){issue="Session needs attention: "+ex.Message;App.RecordError(ex);}
  finally{tickBusy=false;UpdateStatus();}
  if(!checkingUpdates && pendingInstaller==null && !closing && !session.Active && !session.RecoveryPending
   && pairingWindows.Count==0 && DateTime.UtcNow>=nextUpdateCheck)
   await CheckForUpdate();
 }
 private async Task CheckForUpdate()
 {
  checkingUpdates=true;
  nextUpdateCheck=DateTime.UtcNow.AddHours(6);
  try {
   var update=await AssistantUpdates.Check();
   if(update==null || closing || busy || session.Active || session.RecoveryPending || pairingWindows.Count>0)return;
   if(MessageBox.Show(this,$"WoWU Gamepad Assistant {update.Version} is available on GitHub. Download it and open the installer?",
    "WoWU update available",MessageBoxButton.YesNo,MessageBoxImage.Information)!=MessageBoxResult.Yes)return;
   try {
    var path=await AssistantUpdates.Download(update);
    if(closing)return;
    pendingInstaller=path;
    if(!session.Active && !busy)Close();
   }catch(Exception ex) {
    App.RecordError(ex);
    if(!closing)MessageBox.Show(this,"The update could not be downloaded. "+ex.Message,"WoWU update",MessageBoxButton.OK,MessageBoxImage.Warning);
   }
  }catch(Exception ex){App.RecordError(ex);}
  finally{checkingUpdates=false;}
 }
 private async void PollPairings(object? sender,EventArgs e)
 {
  if(pairPollBusy || closing || !SunshineHost.Instance.Running)return;
  pairPollBusy=true;
  try {
   var requests=await SunshineHost.Instance.Pending();if(closing)return;
   var current=requests.Select(r=>r.Id).ToHashSet();
   seenPairings.RemoveWhere(id=>!current.Contains(id));
   foreach(var entry in pairingWindows.ToArray())if(!current.Contains(entry.Key) && !entry.Value.IsSubmitting)entry.Value.Close();
   foreach(var request in requests) {
    if(!seenPairings.Add(request.Id))continue;
    if(WindowState==WindowState.Minimized)WindowState=WindowState.Normal;
    var window=new PairingWindow(request){Owner=this};pairingWindows[request.Id]=window;
    window.Closed+=(_,_)=>{pairingWindows.Remove(request.Id);UpdateStatus();};
    window.Show();window.Activate();
   }
   pairPollIssue=null;
  }catch(Exception ex){if(!closing && pairPollIssue!=ex.Message){pairPollIssue=ex.Message;App.RecordError(ex);}}
  finally{pairPollBusy=false;if(!closing)UpdateStatus();}
 }
 private void OnMinimize(object sender,RoutedEventArgs e)=>WindowState=WindowState.Minimized;
 private void OnCloseClick(object sender,RoutedEventArgs e)=>Close();
 private async void OnClosing(object? sender,CancelEventArgs e)
 {
  if(closeReady)return;e.Cancel=true;
  if(closing)return;
  if(busy){closeRequested=true;UpdateStatus();return;}
  closing=true;timer.Stop();pairingTimer.Stop();UpdateStatus();
  foreach(var window in pairingWindows.Values.ToArray())window.Close();
  var errors=new List<string>();
  void Cleanup(Action action){try{action();}catch(Exception ex){errors.Add(ex.Message);App.RecordError(ex);}}
  Cleanup(()=>SunshineHost.Instance.Stop());Cleanup(()=>session.Restore());Cleanup(()=>bridge?.Stop());bridge=null;
  Cleanup(()=>DisplaySetup.Restore());
  try{await DisplaySetup.DisableDriver();}catch(Exception ex){errors.Add(ex.Message);App.RecordError(ex);}
  if(errors.Count>0)try {
   string folder=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant");
   System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"last-close-issues.txt"),string.Join(Environment.NewLine,errors));
  }catch{}
  if(pendingInstaller is string installer && errors.Count==0) {
   try{AssistantUpdates.Install(installer);}
   catch(Exception ex){App.RecordError(ex);MessageBox.Show(this,"Could not open the update installer. "+ex.Message,"WoWU update");}
  }
  closeReady=true;Close();
 }
}
