using System.Diagnostics;
using System.Runtime.InteropServices;
namespace WoWU.GamepadAssistant;
internal sealed record Display(string Name,string Identity,string Description,Native.Rect Bounds,bool Primary)
{
    public string Label=>$"{Description} — {Name} — {Bounds.Width}×{Bounds.Height} at {Bounds.Left},{Bounds.Top}";
}
internal sealed record Game(int Pid,long Started,nint Handle,string Label);
internal static class Discovery
{
    internal static List<Display> Displays()
    {
        var result=new List<Display>();
        Native.MonitorCallback callback=(nint h,nint dc,ref Native.Rect rect,nint data)=>{
            var info=new Native.MonitorInfo{Size=Marshal.SizeOf<Native.MonitorInfo>(),Device=""};
            if(!Native.GetMonitorInfo(h,ref info))return true;
            var device=new Native.Device{Size=Marshal.SizeOf<Native.Device>(),Name="",Description="",Id="",Key=""};
            bool found=Native.EnumDisplayDevices(info.Device,0,ref device,1);
            result.Add(new Display(info.Device,found?device.Id:"",found?device.Description:"Display",info.Monitor,(info.Flags&1)!=0));return true;
        };
        Native.Check(Native.EnumDisplayMonitors(0,0,callback,0),"Could not enumerate displays");
        return result;
    }
    internal static List<Game> Games()
    {
        var result=new List<Game>();
        foreach(var p in Process.GetProcesses())
        {
            using(p)try {
                if(p.ProcessName is not ("Wow" or "WowT" or "WowClassic" or "WowClassicT"))continue;
                if(p.MainWindowHandle==0 || !Native.IsWindowVisible(p.MainWindowHandle))continue;
                result.Add(new Game(p.Id,p.StartTime.ToUniversalTime().Ticks,p.MainWindowHandle,$"{p.MainWindowTitle} ({p.ProcessName}, PID {p.Id})"));
            }catch(System.ComponentModel.Win32Exception){}catch(InvalidOperationException){}
        }
        return result;
    }
    internal static Native.Rect Plan(Display game,Display touch,List<Display> all)
    {
        if(game.Name==touch.Name)throw new InvalidOperationException("Choose two different displays.");
        if(string.IsNullOrWhiteSpace(touch.Identity))throw new InvalidOperationException("Windows did not provide an identity for the selected display.");
        var g=game.Bounds;var t=touch.Bounds;
        if(t.Width!=1280 || t.Height!=800)throw new InvalidOperationException("The WoWU display must already be 1280 × 800.");
        if(t.Top!=g.Bottom || t.Left<g.Left || t.Right>g.Right)throw new InvalidOperationException("The WoWU display must be directly below and entirely within the gameplay monitor's width. No displays were moved.");
        var span=new Native.Rect{Left=g.Left,Top=g.Top,Right=g.Right,Bottom=t.Bottom};
        foreach(var other in all.Where(d=>d.Name!=game.Name && d.Name!=touch.Name)) {
            var r=other.Bounds;
            if(r.Left<span.Right && r.Right>span.Left && r.Top<span.Bottom && r.Bottom>span.Top)
                throw new InvalidOperationException("The span would cover another monitor. No changes were made.");
        }
        return span;
    }
    internal static string Signature(List<Display> displays)=>string.Join("|",displays.OrderBy(d=>d.Name).Select(d=>$"{d.Name}:{d.Identity}:{d.Bounds.Left},{d.Bounds.Top},{d.Bounds.Right},{d.Bounds.Bottom}"));
}
