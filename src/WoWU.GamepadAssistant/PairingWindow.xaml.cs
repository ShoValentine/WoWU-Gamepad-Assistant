using System.Windows;
using System.Windows.Input;
namespace WoWU.GamepadAssistant;
public partial class PairingWindow : Window
{
 private readonly PairRequest request;
 internal bool IsSubmitting {get;private set;}
 internal PairingWindow(PairRequest request)
 {
  this.request=request;InitializeComponent();DeviceName.Text=request.Name;
  Loaded+=(_,_)=>Pin.Focus();
 }
 private async Task Submit()
 {
  if(IsSubmitting)return;
  string pin=Pin.Text.Trim();
  if(pin.Length!=4 || pin.Any(c=>c<'0' || c>'9')){Error.Text="Enter the four digits shown in Moonlight.";Pin.Focus();return;}
  IsSubmitting=true;PairButton.IsEnabled=false;Pin.IsEnabled=false;Error.Text="Pairing...";
  try{await SunshineHost.Instance.Pair(request,pin);Close();}
  catch(Exception ex){if(IsVisible){Error.Text="Pairing failed: "+ex.Message;Pin.Clear();}App.RecordError(ex);}
  finally{IsSubmitting=false;PairButton.IsEnabled=true;Pin.IsEnabled=true;if(IsVisible)Pin.Focus();}
 }
 private async void OnPair(object sender,RoutedEventArgs e)=>await Submit();
 private async void OnPinKeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){e.Handled=true;await Submit();}}
 private void OnCancel(object sender,RoutedEventArgs e)=>Close();
 private void OnDrag(object sender,MouseButtonEventArgs e){if(e.ChangedButton==MouseButton.Left && e.ButtonState==MouseButtonState.Pressed)DragMove();}
}
