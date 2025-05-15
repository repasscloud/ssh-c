using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

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

            using var response = await client.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var latestTag = doc.RootElement.GetProperty("tag_name").GetString()?.Trim();

            if (string.IsNullOrWhiteSpace(latestTag))
            {
                Console.WriteLine("❌ Failed to read latest version from GitHub.");
            }
            else
            {
                var latestVersion = latestTag.TrimStart('v');

                if (currentVersion != latestVersion)
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
