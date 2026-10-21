using System.ComponentModel;
using System.Runtime.InteropServices;
namespace WoWU.GamepadAssistant;
internal static class Native
{
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; public readonly int Width=>Right-Left;public readonly int Height=>Bottom-Top; }
    [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] public struct Placement { public int Length,Flags,ShowCmd;public Point Min,Max;public Rect Normal; }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct MonitorInfo {public int Size;public Rect Monitor,Work;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Device;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct Device {public int Size;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Name;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Description;public uint Flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Id;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Key;}
    internal delegate bool MonitorCallback(nint monitor,nint dc,ref Rect rect,nint data);
    [DllImport("user32.dll")] internal static extern bool EnumDisplayMonitors(nint dc,nint clip,MonitorCallback callback,nint data);
    [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern bool EnumDisplayDevices(string? device,uint index,ref Device info,uint flags);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool GetWindowRect(nint hwnd,out Rect rect);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool GetClientRect(nint hwnd,out Rect rect);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool ClientToScreen(nint hwnd,ref Point point);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool GetWindowPlacement(nint hwnd,ref Placement placement);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool SetWindowPlacement(nint hwnd,ref Placement placement);
    [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW",SetLastError=true)] internal static extern nint GetWindowLong(nint hwnd,int index);
    [DllImport("user32.dll",EntryPoint="SetWindowLongPtrW",SetLastError=true)] private static extern nint SetWindowLong(nint hwnd,int index,nint value);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool SetWindowPos(nint hwnd,nint after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll")] internal static extern bool ShowWindow(nint hwnd,int cmd);
    [DllImport("user32.dll")] internal static extern bool IsWindow(nint hwnd);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(nint hwnd,out uint pid);
    internal static void Check(bool success,string action) {if(!success)throw new Win32Exception(Marshal.GetLastWin32Error(),action);}
    internal static void Style(nint hwnd,int index,long value)
    {
        Marshal.SetLastPInvokeError(0);
        var previous=SetWindowLong(hwnd,index,(nint)value);
        if(previous==0 && Marshal.GetLastPInvokeError()!=0)throw new Win32Exception(Marshal.GetLastPInvokeError(),"Could not change window style");
    }
}
