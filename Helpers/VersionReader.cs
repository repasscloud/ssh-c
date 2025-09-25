using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using ssh_c.Helpers; // for AppJsonContext
using ssh_c.Models;

namespace ssh_c.Helpers;

public static class VersionReader
{
    private const string GitHubApiUrl = "https://api.github.com/repos/repasscloud/ssh-c/releases/latest";

    public static string GetVersion()
    {
        var raw = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "?.?.?";

        return raw.Split('+')[0];
    }

    public static async Task CheckForUpdates()
    {
        var currentVersion = GetVersion();

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ssh-c", currentVersion));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            using var response = await client.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            // Typed or fallback to JSON doc
            GitHubRelease? release = null;
            try
            {
                release = await JsonSerializer.DeserializeAsync(stream, AppJsonContext.Default.GitHubRelease);
            }
            catch { /* fall back below if needed */ }

            var latestTag = release?.TagName;

            if (string.IsNullOrWhiteSpace(latestTag))
            {
                // fallback parse
                stream.Position = 0;
                using var doc = await JsonDocument.ParseAsync(stream);
                latestTag = doc.RootElement.GetProperty("tag_name").GetString();
            }

            if (string.IsNullOrWhiteSpace(latestTag))
            {
                Console.WriteLine("❌ Failed to read latest version from GitHub.");
            }
            else
            {
                var latestVersion = latestTag!.Trim().TrimStart('v');

                if (!currentVersion.Equals(latestVersion, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"⬆️  Update available: v{latestVersion} (you have v{currentVersion})");
                    Console.WriteLine("🔗 https://github.com/repasscloud/ssh-c/releases/latest");
                }
                else
                {
                    Console.WriteLine($"✅ ssh-c is up to date (v{currentVersion})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to fetch release info: {ex.Message}");
        }
    }
}
