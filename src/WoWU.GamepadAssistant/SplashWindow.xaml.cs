using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
namespace WoWU.GamepadAssistant;
public partial class SplashWindow : Window
{
 private sealed record Stage(string Quote,double Start,double Ceiling);
 private static readonly Dictionary<string,Stage> Stages=new(StringComparer.Ordinal){
  ["Restoring any interrupted session"]=new("Gathering the party",4,14),
  ["Checking the virtual display driver"]=new("Reticulating Splines",18,33),
  ["Preparing your 1280 x 800 display"]=new("Convincing the monitors to cooperate",36,53),
  ["Selecting the WoWU display"]=new("Summoning a second screen",55,61),
  ["Preparing the integrated streaming engine"]=new("Doing what Nintendont",63,73),
  ["Matching Sunshine to the WoWU display"]=new("Looking for the any key",75,81),
  ["Checking capture of the 1280 x 800 display"]=new("Polishing the pixels",83,89),
  ["Starting Moonlight streaming"]=new("Opening a portal to the sofa",90,94),
  ["Finding your WoWU saved data"]=new("Checking the quest log",95,98),
  ["Ready for your next adventure"]=new("Applying +10 to couch comfort",100,100),
  ["Opening setup so we can sort this out"]=new("A wild setup issue appeared",96,98),
 };
 private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(30)};
 private Stage stage=Stages["Restoring any interrupted session"];
 private DateTime stageStarted=DateTime.UtcNow;
 private bool canClose,finishing;
 public SplashWindow()
 {
  InitializeComponent();Quote.Text=stage.Quote;
  timer.Tick+=(_,_)=>{
   double elapsed=(DateTime.UtcNow-stageStarted).TotalSeconds;
   double target=finishing?100:stage.Start+(stage.Ceiling-stage.Start)*(1-Math.Exp(-elapsed/10));
   if(target>Progress.Value)Progress.Value=Math.Min(target,Progress.Value+Math.Max(.015,(target-Progress.Value)*.14));
  };
  timer.Start();Closing+=(_,e)=>{if(!canClose)e.Cancel=true;};
 }
 internal async Task FadeInAsync()
 {
  BeginAnimation(OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(350)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseOut}});
  await Task.Delay(350);
 }
 internal void Report(string task,double progress=0)
 {
  if(!Stages.TryGetValue(task,out var next))return;
  if(stage!=next){stage=next;stageStarted=DateTime.UtcNow;Quote.Text=stage.Quote;}
  // The user-facing quote is fixed to the actual task, never randomly rotated.
  try {
   string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant");Directory.CreateDirectory(folder);
   File.AppendAllText(Path.Combine(folder,"startup-stages.log"),$"{DateTimeOffset.Now:O} | {stage.Quote} | {task}{Environment.NewLine}");
  }catch{}
 }
 internal async Task FinishAsync()
 {
  finishing=true;
  for(int i=0;i<32 && Progress.Value<99.8;i++)await Task.Delay(30);
  Progress.Value=100;await Task.Delay(180);
  BeginAnimation(OpacityProperty,new DoubleAnimation(1,0,TimeSpan.FromMilliseconds(300)){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseIn}});
  await Task.Delay(300);timer.Stop();canClose=true;Close();
 }
}
