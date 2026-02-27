// Program.cs
using ssh_c.Services;
using ssh_c.Helpers;
using static System.Console;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ssh_c;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // allow user to disable color
            if (args.Contains("--no-color")) Ansi.SetEnabled(false);

            var wantsHelp = args.Length == 0 || args.Contains("--help") || args.Contains("-h");
            if (wantsHelp)
            {
                PrintHelp();
                return;
            }

            // Flags (keep -v alone as version; with alias it's verbose)
            var hasAlias = args.Length > 0 && !args[0].StartsWith("-");
            var verbose = args.Contains("--verbose") || (args.Contains("-v") && hasAlias);
            var wantsVersion = args.Contains("--version") || args.Contains("-V") || (args.Length == 1 && args[0] == "-v");
            var wantsUpdateCheck = args.Contains("--check-updates") || args.Contains("--update") || args.Contains("-u");

            if (wantsVersion)
            {
                var version = VersionReader.GetVersion();
                WriteLine($@"
{Ansi.Header("🛡️  ssh-c CLI")}
{Ansi.Subtle("Version:")}     v{version}
{Ansi.Subtle("Runtime:")}     Native AOT
{Ansi.Subtle("Platform:")}    {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}
{Ansi.Subtle("Copyright:")}   © {DateTime.UtcNow.Year} Repass Cloud
{Ansi.Subtle("License:")}     MIT
{Ansi.Subtle("Website:")}     https://github.com/repasscloud/ssh-c
");
                return;
            }

            if (wantsUpdateCheck)
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
                    WriteLine($"{Ansi.Option("--remove")} requires {Ansi.Placeholder("ALIAS")}.");
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
                    WriteLine($"{Ansi.Option("--export")} requires {Ansi.Placeholder("ALIAS")}.");
                    return;
                }
                ConfigLoader.ExportCommand(args[idx + 1]);
                return;
            }

            // Connect to alias
            var alias = args[0];
            var hosts = ConfigLoader.LoadConfig();
            var config = hosts.FirstOrDefault(h => h.Name.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (config == null)
            {
                WriteLine($"{Ansi.Subtle("Host alias")} {Ansi.Placeholder(alias)} {Ansi.Subtle("not found.")}");
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
        WriteLine($"{Ansi.Header("ssh-c — Lightweight SSH Connection Manager")}\n");

        WriteLine(Ansi.Section("USAGE"));
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Placeholder("ALIAS")} {Ansi.Subtle("[options]")}     Connect to a saved SSH alias");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--add")} " +
                  $"{Ansi.Option("--name")} {Ansi.Placeholder("ALIAS")} " +
                  $"{Ansi.Option("--host")} {Ansi.Placeholder("HOSTNAME_OR_IP")} " +
                  $"{Ansi.Option("--user")} {Ansi.Placeholder("USERNAME")} \\");
        WriteLine($"               {Ansi.Option("--auth-type")} {Ansi.EnumSet("password|cert")} " +
                  $"{Ansi.Subtle("[")}{Ansi.Option("--port")} {Ansi.Placeholder("SSH_PORT")}{Ansi.Subtle("]")} " +
                  $"{Ansi.Subtle("[")}{Ansi.Option("--identity-file")} {Ansi.Placeholder("PATH_TO_PRIVATE_KEY")}{Ansi.Subtle("]")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--list")}                List saved SSH aliases");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--remove")} {Ansi.Placeholder("ALIAS")}      Remove an alias");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--export")} {Ansi.Placeholder("ALIAS")}      Print the SSH command");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--check-updates")} {Ansi.Subtle("|")} {Ansi.Option("-u")}  Check for a newer release");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--help")} {Ansi.Subtle("|")} {Ansi.Option("-h")}           Show this help");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--version")} {Ansi.Subtle("|")} {Ansi.Option("-V")}        Show version");
        WriteLine();

        WriteLine(Ansi.Section("OPTIONS"));
        WriteLine($"  {Ansi.Option("-v")}, {Ansi.Option("--verbose")}                              Print the underlying ssh command before connecting");
        WriteLine($"      {Ansi.Option("--port")} {Ansi.Placeholder("SSH_PORT")}                      SSH port (default 22) when adding a host");
        WriteLine($"      {Ansi.Option("--identity-file")} {Ansi.Placeholder("PATH_TO_PRIVATE_KEY")}  Path to private key when {Ansi.Option("--auth-type=cert")}");
        WriteLine($"      {Ansi.Option("--no-color")}                             Disable ANSI colors (or set {Ansi.Option("NO_COLOR=1")})");
        WriteLine();

        WriteLine(Ansi.Section("PLACEHOLDERS"));
        WriteLine($"  {Ansi.Placeholder("ALIAS")}                A short name you choose for the host (e.g., prod, db, mybox)");
        WriteLine($"  {Ansi.Placeholder("HOSTNAME_OR_IP")}       Remote host, FQDN or IP (e.g., server.example.com, 203.0.113.10)");
        WriteLine($"  {Ansi.Placeholder("USERNAME")}             The SSH username (e.g., ubuntu, ec2-user, root)");
        WriteLine($"  {Ansi.Placeholder("SSH_PORT")}             SSH port number (default: 22)");
        WriteLine($"  {Ansi.Placeholder("PATH_TO_PRIVATE_KEY")}  Path to your private key file (e.g., ~/.ssh/id_ed25519)");
        WriteLine();

        WriteLine(Ansi.Section("EXAMPLES"));
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Placeholder("myserver")} {Ansi.Option("--verbose")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--add")} {Ansi.Option("--name")} {Ansi.Placeholder("prod")} {Ansi.Option("--host")} {Ansi.Placeholder("203.0.113.10")} " +
                  $"{Ansi.Option("--user")} {Ansi.Placeholder("ubuntu")} {Ansi.Option("--auth-type")} {Ansi.EnumSet("cert")} {Ansi.Option("--identity-file")} {Ansi.Placeholder("~/.ssh/prod")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--list")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--remove")} {Ansi.Placeholder("oldbox")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--export")} {Ansi.Placeholder("prod")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("-u")}");
        WriteLine();
    }
}
