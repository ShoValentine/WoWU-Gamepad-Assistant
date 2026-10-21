using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace WoWU.GamepadAssistant;
public partial class MainWindow : Window
{
    private readonly Session session=new();
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromSeconds(1)};
    private bool busy;
    private int ticks;
    public MainWindow()
    {
        InitializeComponent();
        try {session.Recover();RefreshLists();}catch(Exception ex){Status.Text="Recovery needs attention: "+ex.Message;}
        timer.Tick+=Tick;timer.Start();UpdateControls();
    }
    private void RefreshLists()
    {
        var selectedGame=Games.SelectedItem as Game;
        var main=MainDisplay.SelectedItem as Display;var touch=TouchDisplay.SelectedItem as Display;
        var games=Discovery.Games();var displays=Discovery.Displays();
        Games.ItemsSource=games;Games.SelectedItem=games.FirstOrDefault(g=>g.Pid==selectedGame?.Pid && g.Started==selectedGame.Started) ?? (games.Count==1?games[0]:null);
        MainDisplay.ItemsSource=displays;MainDisplay.SelectedItem=displays.FirstOrDefault(d=>d.Identity==main?.Identity && d.Name==main.Name) ?? displays.FirstOrDefault(d=>d.Primary);
        TouchDisplay.ItemsSource=displays;
        TouchDisplay.SelectedItem=displays.FirstOrDefault(d=>!string.IsNullOrEmpty(touch?.Identity) && d.Identity==touch.Identity && d.Name==touch.Name);
        if(!session.Active && !session.RecoveryPending)Status.Text=games.Count==0?"WoWU Gamepad Assistant running. Please launch World of Warcraft.":"World of Warcraft detected. Enter the world, then choose Preview span.";
    }
    private void UpdateControls()
    {
        bool locked=busy || session.Active || session.RecoveryPending;
        ActivateButton.IsEnabled=!locked;Refresh.IsEnabled=!locked;Games.IsEnabled=!locked;MainDisplay.IsEnabled=!locked;TouchDisplay.IsEnabled=!locked;InWorld.IsEnabled=!locked;
        Keep.IsEnabled=!busy && session.Active && session.RevertAt!=null;Restore.IsEnabled=!busy && (session.Active || session.RecoveryPending);
    }
    private async void OnActivate(object sender,RoutedEventArgs e)
    {
        if(InWorld.IsChecked!=true){Status.Text="Enter the world in Forever and use Windowed mode, then tick the confirmation.";return;}
        if(Games.SelectedItem is not Game game || MainDisplay.SelectedItem is not Display main || TouchDisplay.SelectedItem is not Display touch){Status.Text="Select the game and both displays first.";return;}
        busy=true;UpdateControls();
        try {
            var displays=Discovery.Displays();
            var currentMain=displays.SingleOrDefault(d=>d.Name==main.Name && d.Identity==main.Identity);
            var currentTouch=displays.SingleOrDefault(d=>d.Name==touch.Name && d.Identity==touch.Identity);
            if(currentMain==null || currentTouch==null)throw new InvalidOperationException("The selected displays changed. Refresh before continuing.");
            var plan=Discovery.Plan(currentMain,currentTouch,displays);
            Status.Text="Preparing the window spanâ€¦";
            await session.Start(game,plan,Discovery.Signature(displays));
            Instructions.Text=$"Keep the span, then run in game:\n/wowuviewport preview {currentMain.Bounds.Width} {currentMain.Bounds.Height} {currentTouch.Bounds.Left-currentMain.Bounds.Left}\nUse /wowuviewport keep to retain the addon viewport too.\nBefore Restore: /wowuviewport off";
            Status.Text="Window span ready. Automatically restoring in 30 seconds unless kept.";
        }catch(Exception ex){Status.Text=ex.Message;}
        finally{busy=false;UpdateControls();}
    }
    private void OnKeep(object sender,RoutedEventArgs e){session.Keep();Status.Text="Window span active. Streaming is not configured by this prototype.";UpdateControls();}
    private void OnRestore(object sender,RoutedEventArgs e)=>RestoreNow("Original window restored.");
    private void RestoreNow(string message)
    {
        try{session.Restore();Status.Text=message;Instructions.Text="If the addon preview remains active, run /wowuviewport off.";}
        catch(Exception ex){Status.Text="Restoration needs attention: "+ex.Message;}
        UpdateControls();
    }
    private void OnRefresh(object sender,RoutedEventArgs e){try{RefreshLists();}catch(Exception ex){Status.Text=ex.Message;}}
    private void Tick(object? sender,EventArgs e)
    {
        if(busy)return;
        try {
            if(session.Active) {
                if(!session.SameWindow()){RestoreNow("World of Warcraft closed. Waiting for the next session.");return;}
                if(!session.TopologyMatches()){RestoreNow("Displays changed. The window was restored; refresh before trying again.");return;}
                if(!session.MatchesTarget()){RestoreNow("The game changed its window geometry. Restored instead of repeatedly resizing it.");return;}
                if(session.RevertAt is DateTime until) {
                    int remaining=(int)Math.Ceiling((until-DateTime.UtcNow).TotalSeconds);
                    if(remaining<=0)RestoreNow("Preview ended. Original window restored.");
                    else Status.Text=$"Window span preview â€” restoring in {remaining} seconds. Choose Keep span to retain it.";
                }
            } else if(++ticks%5==0 && !session.RecoveryPending) {
                // Refresh process choices only. Do not reset the user's display choices while waiting.
                var old=Games.SelectedItem as Game;var games=Discovery.Games();
                Games.ItemsSource=games;Games.SelectedItem=games.FirstOrDefault(g=>g.Pid==old?.Pid && g.Started==old.Started) ?? (games.Count==1?games[0]:null);
            }
        }catch(Exception ex){Status.Text="Session needs attention: "+ex.Message;}
        UpdateControls();
    }
    private void OnClosing(object? sender,CancelEventArgs e)
    {
        if(busy){e.Cancel=true;Status.Text="Wait for the current window operation to finish.";return;}
        try{session.Restore();timer.Stop();}catch(Exception ex){e.Cancel=true;Status.Text="Could not restore before closing: "+ex.Message;}
    }
}
