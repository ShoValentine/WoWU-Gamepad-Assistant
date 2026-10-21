using System.Threading;
using System.Windows;
namespace WoWU.GamepadAssistant;
public partial class App : Application
{
    private Mutex? singleInstance;
    protected override void OnStartup(StartupEventArgs e)
    {
        singleInstance=new Mutex(true,"Local\\WoWU.GamepadAssistant",out bool first);
        if (!first) { MessageBox.Show("WoWU Gamepad Assistant is already running.");Shutdown();return; }
        base.OnStartup(e);
    }
    protected override void OnExit(ExitEventArgs e) { singleInstance?.Dispose();base.OnExit(e); }
}
