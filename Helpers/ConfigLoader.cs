// Helpers/ConfigLoader.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ssh_c.Models;

namespace ssh_c.Helpers;

public static class ConfigLoader
{
    private static readonly string Home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    // Correct config location
    private static readonly string ConfigDir  = Path.Combine(Home, ".ssh-c");
    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    // Migration support from the old accidental path ".shh-c"
    private static readonly string OldConfigDir  = Path.Combine(Home, ".shh-c");
    private static readonly string OldConfigPath = Path.Combine(OldConfigDir, "config.json");

    private static void EnsureMigrated()
    {
        try
        {
            if (!File.Exists(ConfigPath) && File.Exists(OldConfigPath))
            {
                Directory.CreateDirectory(ConfigDir);
                File.Move(OldConfigPath, ConfigPath, overwrite: true);
                try
                {
                    if (Directory.Exists(OldConfigDir) && !Directory.EnumerateFileSystemEntries(OldConfigDir).Any())
                        Directory.Delete(OldConfigDir);
                }
                catch { /* non-fatal */ }
                Console.WriteLine(Ansi.Subtle("Migrated ssh-c config from ~/.shh-c to ~/.ssh-c"));
            }
        }
        catch { /* non-fatal */ }
    }

    public static List<SshHostConfig> LoadConfig()
    {
        EnsureMigrated();

        if (!File.Exists(ConfigPath))
            return [];

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var root = JsonSerializer.Deserialize(json, AppJsonContext.Default.ConfigRoot);
            return root?.Hosts ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveConfig(List<SshHostConfig> allHosts)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(new ConfigRoot { Hosts = allHosts }, AppJsonContext.Default.ConfigRoot);
        File.WriteAllText(ConfigPath, json);
    }

    public static void SaveHostConfig(SshHostConfig newHost)
    {
        var allHosts = LoadConfig();

        if (allHosts.Any(h => h.Name.Equals(newHost.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Host alias '{newHost.Name}' already exists.");

        allHosts.Add(newHost);
        SaveConfig(allHosts);
        Console.WriteLine($"{Ansi.Command("✓")} Added alias {Ansi.Em(newHost.Name)} → {newHost.User}@{newHost.Host}:{newHost.Port}");
    }

    /// <summary>Returns true if the alias was found and removed; false if it did not exist.</summary>
    public static bool RemoveHost(string name)
    {
        var allHosts = LoadConfig();
        var updated = allHosts.Where(h => !h.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

        if (updated.Count == allHosts.Count)
            return false;

        SaveConfig(updated);
        return true;
    }

    public static void ListHosts()
    {
        var allHosts = LoadConfig();
        if (allHosts.Count == 0)
        {
            Console.WriteLine($"{Ansi.Subtle("No hosts saved. Use")} {Ansi.Option("--add")} {Ansi.Subtle("to create one.")}");
            return;
        }

        var nameWidth = Math.Max(5, allHosts.Max(h => h.Name.Length));
        foreach (var host in allHosts.OrderBy(h => h.Name, StringComparer.OrdinalIgnoreCase))
        {
            var left  = Ansi.Command(host.Name.PadRight(nameWidth));
            var arrow = Ansi.Subtle(" → ");
            var right = $"{Ansi.Placeholder($"{host.User}@{host.Host}")}:{Ansi.Placeholder(host.Port.ToString())}";
            var auth  = Ansi.Subtle($" ({host.Auth.Type})");
            Console.WriteLine($"{left}{arrow}{right}{auth}");
        }
    }

    public static void ExportCommand(string name)
    {
        var host = LoadConfig().FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (host == null)
        {
            Console.Error.WriteLine($"Host alias '{name}' not found.");
            return;
        }

        var identityPart = host.Auth.Type == "cert" && !string.IsNullOrWhiteSpace(host.Auth.IdentityFile)
            ? $"-i {ExpandPath(host.Auth.IdentityFile!)} "
            : string.Empty;

        Console.WriteLine(Ansi.Command($"ssh {identityPart}-p {host.Port} {host.User}@{host.Host}"));
    }

    public static void AddNewHost(string[] args)
    {
        var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--")) continue;

            if (args[i].Contains('='))
            {
                var parts = args[i].Split('=', 2);
                dict[parts[0]] = parts[1];
            }
            else if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
            {
                dict[args[i]] = args[i + 1];
                i++;
            }
            else
            {
                dict[args[i]] = null;
            }
        }

        string Require(string key) =>
            dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val)
                ? val.Trim()
                : throw new ArgumentException($"Missing required parameter: {key}");

        var name     = Require("--name");
        var host     = Require("--host");
        var user     = Require("--user");
        var authType = Require("--auth-type").ToLowerInvariant();

        if (authType != "cert" && authType != "password")
            throw new ArgumentException($"--auth-type must be 'cert' or 'password', got '{authType}'.");

        var port = dict.TryGetValue("--port", out var portStr) && int.TryParse(portStr, out var portVal) && portVal > 0
            ? portVal
            : 22;

        var identityFile = dict.TryGetValue("--identity-file", out var idFile) && !string.IsNullOrWhiteSpace(idFile)
            ? idFile!.Trim()
            : null;

        if (authType == "cert" && string.IsNullOrWhiteSpace(identityFile))
            throw new ArgumentException("--identity-file is required when --auth-type=cert.");

        var newHost = new SshHostConfig
        {
            Name = name,
            Host = host,
            User = user,
            Port = port,
            Auth = new SshHostConfig.AuthConfig
            {
                Type         = authType,
                IdentityFile = identityFile
            }
        };

        SaveHostConfig(newHost);
    }

    private static string ExpandPath(string path) =>
        path.Replace("~", Home);
}
