using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
namespace WoWU.GamepadAssistant;
internal sealed class RecoveryRecord
{
    public int Schema {get;set;}=1;
    public int Pid {get;set;}
    public long Started {get;set;}
    public long Handle {get;set;}
    public long Style {get;set;}
    public long ExtendedStyle {get;set;}
    public Native.Placement Placement {get;set;}
    public Native.Rect OriginalRect {get;set;}
    public Native.Rect Target {get;set;}
    public string Topology {get;set;}="";
}
internal sealed class Session
{
    private static readonly string Folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant");
    private static readonly string RecoveryPath=Path.Combine(Folder,"recovery.json");
    private static readonly JsonSerializerOptions JsonOptions=new(){IncludeFields=true,WriteIndented=true};
    private RecoveryRecord? record;
    internal bool Active=>record!=null;
    internal bool RecoveryPending=>File.Exists(RecoveryPath);
    internal DateTime? RevertAt {get;private set;}
    internal bool SameWindow()
    {
        if(record==null)return false;
        Process p;
        try { p=Process.GetProcessById(record.Pid); }catch(ArgumentException){return false;}
        using(p) {
            if(p.HasExited || p.StartTime.ToUniversalTime().Ticks!=record.Started)return false;
            var hwnd=(nint)record.Handle;
            if(!Native.IsWindow(hwnd))return false;
            Native.GetWindowThreadProcessId(hwnd,out uint pid);
            return pid==record.Pid && p.MainWindowHandle==hwnd;
        }
    }
    internal void Recover()
    {
        if(!RecoveryPending)return;
        var recovered=JsonSerializer.Deserialize<RecoveryRecord>(File.ReadAllText(RecoveryPath),JsonOptions);
        if(recovered==null || recovered.Schema!=1 || recovered.Pid<=0 || recovered.Handle==0 || recovered.Placement.Length!=Marshal.SizeOf<Native.Placement>())
            throw new InvalidOperationException("The recovery file is not recognised. Activation is blocked; retain the file for recovery.");
        record=recovered;Restore();
    }
    internal async Task Start(Game game,Native.Rect target,string topology)
    {
        if(Active || RecoveryPending)throw new InvalidOperationException("Restore the previous session first.");
        using(var p=Process.GetProcessById(game.Pid)) {
            if(p.StartTime.ToUniversalTime().Ticks!=game.Started || p.MainWindowHandle!=game.Handle)
                throw new InvalidOperationException("The game window changed. Refresh and select it again.");
        }
        var placement=new Native.Placement{Length=Marshal.SizeOf<Native.Placement>()};
        Native.Check(Native.GetWindowPlacement(game.Handle,ref placement),"Could not read window placement");
        Native.Check(Native.GetWindowRect(game.Handle,out var original),"Could not read window bounds");
        var next=new RecoveryRecord{Pid=game.Pid,Started=game.Started,Handle=(long)game.Handle,
            Style=(long)Native.GetWindowLong(game.Handle,-16),ExtendedStyle=(long)Native.GetWindowLong(game.Handle,-20),
            Placement=placement,OriginalRect=original,Target=target,Topology=topology};
        Directory.CreateDirectory(Folder);
        var bytes=System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(next,JsonOptions));
        using(var file=new FileStream(RecoveryPath+".tmp",FileMode.Create,FileAccess.Write,FileShare.None)) {file.Write(bytes);file.Flush(true);}
        File.Move(RecoveryPath+".tmp",RecoveryPath,true);
        record=next;
        try {
            if(!SameWindow())throw new InvalidOperationException("The selected game closed before activation.");
            Native.ShowWindow(game.Handle,9); // SW_RESTORE, before changing styles.
            const long removeStyle=0x00C00000L|0x00040000L|0x20000000L|0x01000000L; // caption, sizing, minimized, maximized
            Native.Style(game.Handle,-16,(next.Style & ~removeStyle)|0x80000000L);
            Native.Style(game.Handle,-20,next.ExtendedStyle & ~(0x00000100L|0x00000200L|0x00020000L));
            Native.Check(Native.SetWindowPos(game.Handle,0,target.Left,target.Top,target.Width,target.Height,0x0020|0x0004|0x0010),"Could not span the game window");
            await Task.Delay(900);
            if(!SameWindow() || Discovery.Signature(Discovery.Displays())!=topology || !MatchesTarget())
                throw new InvalidOperationException("WoW did not retain the requested client area, or the displays changed. Use Windowed mode and retry.");
            RevertAt=DateTime.UtcNow.AddSeconds(30);
        }catch(Exception ex) {
            try{Restore();}catch(Exception restoreError){throw new InvalidOperationException(ex.Message+" Restoration also needs attention: "+restoreError.Message,ex);}
            throw;
        }
    }
    internal bool MatchesTarget()
    {
        if(record==null)return false;
        var h=(nint)record.Handle;
        if(!Native.GetClientRect(h,out var rect))return false;
        var origin=new Native.Point();if(!Native.ClientToScreen(h,ref origin))return false;
        var t=record.Target;
        return rect.Width==t.Width && rect.Height==t.Height && origin.X==t.Left && origin.Y==t.Top;
    }
    internal bool TopologyMatches()=>record!=null && record.Topology==Discovery.Signature(Discovery.Displays());
    internal void Keep() {if(Active)RevertAt=null;}
    internal void Restore()
    {
        if(record==null) {if(RecoveryPending)Recover();return;}
        if(SameWindow()) {
            var h=(nint)record.Handle;
            Native.Style(h,-16,record.Style);Native.Style(h,-20,record.ExtendedStyle);
            var r=record.OriginalRect;
            Native.Check(Native.SetWindowPos(h,0,r.Left,r.Top,r.Width,r.Height,0x0020|0x0004|0x0010),"Could not restore window bounds");
            var placement=record.Placement;
            Native.Check(Native.SetWindowPlacement(h,ref placement),"Could not restore window placement");
            if(Native.GetWindowLong(h,-16)!=(nint)record.Style || Native.GetWindowLong(h,-20)!=(nint)record.ExtendedStyle)
                throw new InvalidOperationException("Window styles were not restored; keep the Assistant open and try Restore again.");
        }
        // Never target a reused PID or a different HWND. A destroyed window needs no restoration.
        File.Delete(RecoveryPath);record=null;RevertAt=null;
    }
}
