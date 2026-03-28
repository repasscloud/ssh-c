// Services/SshConnectionService.cs
using System;
using System.Diagnostics;
using System.IO;
using ssh_c.Helpers;
using ssh_c.Models;

namespace ssh_c.Services;

public class SshConnectionService
{
    private static readonly string SshBinary = FindSshBinary();

    private static string FindSshBinary()
    {
        if (OperatingSystem.IsWindows())
        {
            var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var candidate = Path.Combine(system32, "OpenSSH", "ssh.exe");
            if (File.Exists(candidate)) return candidate;
            return "ssh";
        }

        // Unix — check common locations before falling back to PATH
        string[] candidates = ["/usr/bin/ssh", "/bin/ssh", "/usr/local/bin/ssh"];
        foreach (var c in candidates)
            if (File.Exists(c)) return c;

        return "ssh";
    }

    public int Connect(SshHostConfig config, bool verbose = false)
    {
        var args = BuildArgs(config);

        if (verbose)
            Console.WriteLine(Ansi.Subtle($"{SshBinary} {args}"));

        var psi = new ProcessStartInfo
        {
            FileName = SshBinary,
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
        return process.ExitCode;
    }

    private static string BuildArgs(SshHostConfig config)
    {
        var identityPart = config.Auth.Type == "cert" && !string.IsNullOrWhiteSpace(config.Auth.IdentityFile)
            ? $"-i {ExpandPath(config.Auth.IdentityFile!)} "
            : string.Empty;

        return $"{identityPart}-tt -p {config.Port} {config.User}@{config.Host}";
    }

    private static string ExpandPath(string path) =>
        path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
}
