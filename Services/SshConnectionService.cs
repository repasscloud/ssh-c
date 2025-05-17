// SshConnectionService.cs
using System;
using System.Diagnostics;
using ssh_c.Models;

namespace ssh_c.Services;

public class SshConnectionService
{
    public void Connect(SshHostConfig config, bool verbose = false)
    {
        var args = $"-tt -p {config.Port} {config.User}@{config.Host}";

        if (config.Auth.Type == "cert" && !string.IsNullOrWhiteSpace(config.Auth.IdentityFile))
        {
            args = $"-i {ExpandPath(config.Auth.IdentityFile)} {args}";
        }

        if (verbose)
        {
            Console.WriteLine($"/usr/bin/ssh {args}");
        }

        var psi = new ProcessStartInfo
        {
            FileName = "/usr/bin/ssh",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        };

        psi.EnvironmentVariables["TERM"] = "xterm-256color";

        using var process = new Process { StartInfo = psi };
        process.Start();
        process.WaitForExit();
    }

    private string ExpandPath(string path) =>
        path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
}
