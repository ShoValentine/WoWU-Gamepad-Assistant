using System.Threading;
using System.Windows;
namespace WoWU.GamepadAssistant;
public partial class App : Application
{
 private Mutex? singleInstance;
 private static string ErrorLog=>System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant","startup-errors.log");
 internal static void RecordError(Exception error)
 {
  try{System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ErrorLog)!);System.IO.File.AppendAllText(ErrorLog,DateTimeOffset.Now+Environment.NewLine+error+Environment.NewLine+Environment.NewLine);}catch{}
 }
 private static void ShowError(Exception error)
 {
  RecordError(error);
  MessageBox.Show("WoWU could not finish starting.\n\n"+error.GetBaseException().Message+"\n\nDetails saved to:\n"+ErrorLog,"WoWU Gamepad Assistant",MessageBoxButton.OK,MessageBoxImage.Error);
 }
 protected override async void OnStartup(StartupEventArgs e)
 {
  AppDomain.CurrentDomain.UnhandledException+=(_,args)=>{if(args.ExceptionObject is Exception error)RecordError(error);};
  DispatcherUnhandledException+=(_,args)=>{args.Handled=true;ShowError(args.Exception);Shutdown(1);};
  try{await StartAssistant(e);}
  catch(Exception error){ShowError(error);Shutdown(1);}
 }
 private async Task StartAssistant(StartupEventArgs e)
 {
  if(e.Args.Length==1 && e.Args[0]=="--name-display"){Shutdown(DriverInstall.Rename());return;}
  if(e.Args.Length==1 && e.Args[0]=="--install-display"){Shutdown(DriverInstall.Install());return;}
  if(e.Args.Length==1 && (e.Args[0]=="--enable-display" || e.Args[0]=="--disable-display")){Shutdown(DriverInstall.SetEnabled(e.Args[0]=="--enable-display"));return;}
  singleInstance=new Mutex(true,"Local\\WoWU.GamepadAssistant",out bool first);
  if(!first){MessageBox.Show("WoWU Gamepad Assistant is already running.");Shutdown();return;}
  base.OnStartup(e);
  var splash=new SplashWindow();splash.Show();await splash.FadeInAsync();
  Display? main=null,touch=null;string? issue=null;
  List<string> savedData=new();
  try {
   splash.Report("Restoring any interrupted session",8);
   new Session().Recover();
   main=StartupPreferences.Select(Discovery.Displays());
   if(main==null)throw new InvalidOperationException("No gameplay monitor found.");
   splash.Report("Checking the virtual display driver",25);
   await DisplaySetup.EnsureDriver();
   splash.Report("Preparing your 1280 x 800 display",55);
   DisplaySetup.Restore();
   touch=await DisplaySetup.Configure(main);
   splash.Report("Selecting the WoWU display",85);
   StartupPreferences.Save(main);
   try{await SunshineHost.Instance.Prepare(text=>splash.Report(text,90));}
   catch(Exception ex){RecordError(ex);issue="Display ready. Streaming needs attention: "+ex.Message;}
   splash.Report("Finding your WoWU saved data",95);
   try{savedData=await Task.Run(CompanionDiscovery.Scan);}
   catch(Exception ex){RecordError(ex);}
   splash.Report(issue==null?"Ready for your next adventure":"Opening setup so we can sort this out",100);
   await Task.Delay(350);
  }catch(Exception ex){RecordError(ex);issue="Startup needs attention: "+ex.Message;splash.Report("Opening setup so we can sort this out",95);}
  var window=new MainWindow(true);MainWindow=window;
  window.ApplyStartup(main,touch,issue,savedData);window.Show();
  ShutdownMode=ShutdownMode.OnMainWindowClose;await splash.FinishAsync();window.FadeIn();
 }
 protected override void OnExit(ExitEventArgs e){try{SunshineHost.Instance.Stop();}catch(Exception error){RecordError(error);}singleInstance?.Dispose();base.OnExit(e);}
}
