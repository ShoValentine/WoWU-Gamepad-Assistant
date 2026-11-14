using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WoWU.GamepadAssistant;

internal static class AssistantUpdates
{
    private const string Repository = "ShoValentine/WoWU-Gamepad-Assistant";
    private const long MaxInstallerBytes = 256 * 1024 * 1024;
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly Regex InstallerName = new(
        @"^WoWU-Gamepad-Assistant-(\d+\.\d+\.\d+)(?:-beta\.\d+)?-win-x64\.msi$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private sealed record Release(
        [property: JsonPropertyName("draft")] bool Draft,
        [property: JsonPropertyName("tag_name")] string Tag,
        [property: JsonPropertyName("assets")] Asset[] Assets);

    private sealed record Asset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("size")] long Size,
        [property: JsonPropertyName("digest")] string? Digest,
        [property: JsonPropertyName("browser_download_url")] string DownloadUrl);

    internal sealed record Update(Version Version, string Name, Uri Url, long Size, string Sha256);

    internal static async Task<Update?> Check()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.github.com/repos/{Repository}/releases?per_page=30");
        request.Headers.UserAgent.ParseAdd("WoWU-Gamepad-Assistant");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var current = typeof(AssistantUpdates).Assembly.GetName().Version ?? new Version(0, 0, 0);
        return Select(json, new Version(current.Major, current.Minor, current.Build));
    }

    internal static Update? Select(string json, Version current)
    {
        Update? best = null;
        foreach (var release in JsonSerializer.Deserialize<Release[]>(json) ?? [])
        {
            if (release.Draft || string.IsNullOrEmpty(release.Tag)) continue;
            foreach (var asset in release.Assets ?? [])
            {
                var match = InstallerName.Match(asset.Name ?? "");
                if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version)
                    || version <= current || best?.Version >= version || asset.State != "uploaded"
                    || asset.Size <= 0 || asset.Size > MaxInstallerBytes
                    || asset.Digest is not string digest || !Regex.IsMatch(digest, @"^sha256:[a-fA-F0-9]{64}\z")
                    || !Uri.TryCreate(asset.DownloadUrl, UriKind.Absolute, out var url)) continue;
                var expected = $"https://github.com/{Repository}/releases/download/{Uri.EscapeDataString(release.Tag)}/{asset.Name}";
                if (!string.Equals(url.AbsoluteUri, expected, StringComparison.Ordinal)) continue;
                best = new Update(version, asset.Name!, url, asset.Size, digest[7..]);
            }
        }
        return best;
    }

    internal static async Task<string> Download(Update update)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        using var response = await Client.GetAsync(update.Url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        var finalUrl = response.RequestMessage?.RequestUri;
        if (finalUrl?.Scheme != Uri.UriSchemeHttps
            || finalUrl.Host is not ("github.com" or "release-assets.githubusercontent.com" or "objects.githubusercontent.com"))
            throw new InvalidDataException("Unexpected GitHub download location.");

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WoWU.GamepadAssistant", "updates");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, update.Name);
        var temporary = path + ".partial";
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var source = await response.Content.ReadAsStreamAsync(timeout.Token);
            long size = 0;
            await using (var target = File.Create(temporary))
            {
                var buffer = new byte[65536];
                int read;
                while ((read = await source.ReadAsync(buffer, timeout.Token)) > 0)
                {
                    size += read;
                    if (size > update.Size) throw new InvalidDataException("The installer is larger than expected.");
                    hash.AppendData(buffer, 0, read);
                    await target.WriteAsync(buffer.AsMemory(0, read), timeout.Token);
                }
            }
            if (size != update.Size || !CryptographicOperations.FixedTimeEquals(hash.GetHashAndReset(), Convert.FromHexString(update.Sha256)))
                throw new InvalidDataException("The installer did not match GitHub’s checksum.");
            File.Move(temporary, path, overwrite: true);
            return path;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static void Install(string path)
    {
        var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "msiexec.exe")) { UseShellExecute = true };
        start.ArgumentList.Add("/i");
        start.ArgumentList.Add(path);
        Process.Start(start);
    }
}
