// Program.cs
using ssh_c.Services;
using ssh_c.Helpers;
using static System.Console;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ssh_c;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                PrintHelp();
                return;
            }

            if (args.Contains("--version") || args.Contains("-v"))
            {
                var version = VersionReader.GetVersion();
                WriteLine($@"
🛡️  ssh-c CLI
Version:     v{version}
Copyright:   © {DateTime.UtcNow.Year} Repass Cloud
License:     MIT
Website:     https://github.com/repasscloud/ssh-c
");
                return;
            }

            if (args.Contains("--check-updates"))
            {
                await VersionReader.CheckForUpdates();
                return;
            }


            if (args.Contains("--add"))
            {
                ConfigLoader.AddNewHost(args);
                return;
            }

            if (args.Contains("--list"))
            {
                ConfigLoader.ListHosts();
                return;
            }

            if (args.Contains("--remove"))
            {
                var idx = Array.IndexOf(args, "--remove");
                if (idx + 1 >= args.Length)
                {
                    WriteLine("Missing host name for --remove");
                    return;
                }

                ConfigLoader.RemoveHost(args[idx + 1]);
                return;
            }

            if (args.Contains("--export"))
            {
                var idx = Array.IndexOf(args, "--export");
                if (idx + 1 >= args.Length)
                {
                    WriteLine("Missing host name for --export");
                    return;
                }

                ConfigLoader.ExportCommand(args[idx + 1]);
                return;
            }

            var alias = args[0];
            var verbose = args.Contains("--verbose");

            var hosts = ConfigLoader.LoadConfig();
            var config = hosts.FirstOrDefault(h => h.Name.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (config == null)
            {
                WriteLine($"Host alias '{alias}' not found.");
                return;
            }

            var ssh = new SshConnectionService();
            ssh.Connect(config, verbose);
        }
        catch (Exception ex)
        {
            WriteLine($"Error: {ex.Message}");
        }
    }

    private static void PrintHelp()
    {
        WriteLine("ssh-c: Lightweight SSH Connection Manager");
        WriteLine();
        WriteLine("Usage:");
        WriteLine("  ssh-c <alias> [--verbose]         Connect to a saved SSH alias");
        WriteLine("  ssh-c --add --name=NAME --host=HOST --user=USER --auth-type=TYPE [--port=PORT] [--identity-file=FILE]");
        WriteLine("                                   Add a new SSH alias");
        WriteLine("  ssh-c --list                      List saved SSH aliases");
        WriteLine("  ssh-c --remove NAME               Remove an alias");
        WriteLine("  ssh-c --export NAME               Print the SSH command");
        WriteLine("  ssh-c --help, -h                  Show this help message");
        WriteLine("  ssh-c --version, -v              Show version");
    }
}