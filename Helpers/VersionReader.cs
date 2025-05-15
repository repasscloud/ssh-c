// Helpers/VersionReader.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using ssh_c.Models;
using ssh_c.Helpers;
using System.Text.Json;

namespace ssh_c.Helpers;

public static class VersionReader
{
    private const string GitHubRepo = "danijeljw/ssh-c";

    public static string GetVersion()
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        return version ?? "?.?.?";
    }

    public static async Task CheckForUpdates()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ssh-c-cli");

            var url = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
            var httpResponse = await client.GetAsync(url);

            if (!httpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("❌ Failed to fetch release info.");
                return;
            }

            var stream = await httpResponse.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync(stream, AppJsonContext.Default.GitHubRelease);

            if (response == null || string.IsNullOrWhiteSpace(response.TagName))
            {
                Console.WriteLine("❌ Failed to parse release data.");
                return;
            }

            var current = GetVersion();
            var latest = response.TagName.TrimStart('v');

            if (current == latest)
            {
                Console.WriteLine($"✅ ssh-c is up to date (v{current})");
            }
            else
            {
                Console.WriteLine($"⬆️  Update available: v{latest} (you have v{current})");
                Console.WriteLine($"🔗 https://github.com/{GitHubRepo}/releases/latest");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Could not check for updates: {ex.Message}");
        }
    }
}
