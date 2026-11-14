using System.IO;
using System.Text.Json;
namespace WoWU.GamepadAssistant;
internal sealed record StartupPreferences(string Identity)
{
 private static string FileName=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"WoWU.GamepadAssistant","preferences.json");
 internal static Display? Select(List<Display> displays)
 {
  return displays.FirstOrDefault(d=>d.Primary);
 }
 internal static void Save(Display display){Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);File.WriteAllText(FileName,JsonSerializer.Serialize(new StartupPreferences(display.Identity)));}
}
