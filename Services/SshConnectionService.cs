// SshConnectionService.cs
using System;
using System.Diagnostics;
using ssh_c.Models;

namespace ssh_c.Services;

public class SshConnectionService
{
    public void Connect(SshHostConfig config, bool verbose = false)
    {
        var args = $"-p {config.Port} {config.User}@{config.Host}";

        if (config.Auth.Type == "cert" && !string.IsNullOrWhiteSpace(config.Auth.IdentityFile))
        {
            args = $"-i {ExpandPath(config.Auth.IdentityFile)} {args}";
        }

        if (verbose)
        {
            Console.WriteLine($"ssh {args}");
        }

        var psi = new ProcessStartInfo
        {
            FileName = "/usr/bin/ssh",
            Arguments = args,
            UseShellExecute = false,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        Process.Start(psi);
    }

    private string ExpandPath(string path) =>
        path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
}
