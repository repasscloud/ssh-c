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
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".shh-c");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    public static List<SshHostConfig> LoadConfig()
    {
        if (!File.Exists(ConfigPath))
            return new List<SshHostConfig>();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var root = JsonSerializer.Deserialize(json, AppJsonContext.Default.ConfigRoot);
            return root?.Hosts ?? new List<SshHostConfig>();
        }
        catch
        {
            return new List<SshHostConfig>();
        }
    }

    public static void SaveConfig(List<SshHostConfig> allHosts)
    {
        var json = JsonSerializer.Serialize(new ConfigRoot { Hosts = allHosts }, AppJsonContext.Default.ConfigRoot);
        Directory.CreateDirectory(ConfigDir);
        File.WriteAllText(ConfigPath, json);
    }

    public static void SaveHostConfig(SshHostConfig newHost)
    {
        var allHosts = LoadConfig();

        if (allHosts.Any(h => h.Name.Equals(newHost.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Host with name '{newHost.Name}' already exists.");

        allHosts.Add(newHost);
        SaveConfig(allHosts);
    }

    public static void RemoveHost(string name)
    {
        var allHosts = LoadConfig();
        var updated = allHosts.Where(h => !h.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
        SaveConfig(updated);
    }

    public static void ListHosts()
    {
        var allHosts = LoadConfig();
        foreach (var host in allHosts)
        {
            Console.WriteLine($"{host.Name} → {host.User}@{host.Host}:{host.Port} ({host.Auth.Type})");
        }
    }

    public static void ExportCommand(string name)
    {
        var host = LoadConfig().FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (host == null)
        {
            Console.WriteLine($"Host alias '{name}' not found.");
            return;
        }

        var cmd = $"ssh";

        if (host.Auth.Type == "cert" && !string.IsNullOrWhiteSpace(host.Auth.IdentityFile))
        {
            cmd += $" -i {host.Auth.IdentityFile}";
        }

        cmd += $" -p {host.Port} {host.User}@{host.Host}";
        Console.WriteLine(cmd);
    }

    public static void AddNewHost(string[] args)
    {
        var dict = new Dictionary<string, string?>();

        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--")) continue;

            if (args[i].Contains('='))
            {
                var parts = args[i].Split('=', 2);
                if (parts.Length == 2)
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
            dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val) ? val : throw new ArgumentException($"Missing or empty required parameter: {key}");

        var name = Require("--name");
        var host = Require("--host");
        var user = Require("--user");
        var authType = Require("--auth-type");

        var port = dict.TryGetValue("--port", out var portStr) && int.TryParse(portStr, out var portVal)
            ? portVal
            : 22;

        var identityFile = dict.TryGetValue("--identity-file", out var idFile) ? idFile : null;

        var newHost = new SshHostConfig
        {
            Name = name,
            Host = host,
            User = user,
            Port = port,
            Auth = new SshHostConfig.AuthConfig
            {
                Type = authType,
                IdentityFile = identityFile
            }
        };

        SaveHostConfig(newHost);
        Console.WriteLine($"✅ Added config for '{name}'");
    }
}
