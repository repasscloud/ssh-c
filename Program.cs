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
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Contains("--no-color")) Ansi.SetEnabled(false);

            var wantsHelp = args.Length == 0 || args.Contains("--help") || args.Contains("-h");
            if (wantsHelp)
            {
                PrintHelp();
                return 0;
            }

            // -v alone = version; -v alongside an alias = verbose
            var hasAlias      = args.Length > 0 && !args[0].StartsWith("-");
            var verbose       = args.Contains("--verbose") || (args.Contains("-v") && hasAlias);
            var wantsVersion  = args.Contains("--version") || args.Contains("-V") || (args.Length == 1 && args[0] == "-v");
            var wantsUpdate   = args.Contains("--check-updates") || args.Contains("--update") || args.Contains("-u");

            if (wantsVersion)
            {
                var version = VersionReader.GetVersion();
                WriteLine($@"
{Ansi.Header("ssh-c — Lightweight SSH Connection Manager")}
{Ansi.Subtle("Version:")}     v{version}
{Ansi.Subtle("Runtime:")}     Native AOT
{Ansi.Subtle("Platform:")}    {System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier}
{Ansi.Subtle("Copyright:")}   © {DateTime.UtcNow.Year} RePass Cloud Pty Ltd
{Ansi.Subtle("License:")}     MIT
{Ansi.Subtle("Website:")}     https://github.com/repasscloud/ssh-c
");
                return 0;
            }

            if (wantsUpdate)
            {
                await VersionReader.CheckForUpdates();
                return 0;
            }

            if (args.Contains("--add"))
            {
                ConfigLoader.AddNewHost(args);
                return 0;
            }

            if (args.Contains("--list"))
            {
                ConfigLoader.ListHosts();
                return 0;
            }

            if (args.Contains("--remove"))
            {
                var idx = Array.IndexOf(args, "--remove");
                if (idx + 1 >= args.Length)
                {
                    Error.WriteLine($"{Ansi.Option("--remove")} requires {Ansi.Placeholder("ALIAS")}.");
                    return 1;
                }
                var alias = args[idx + 1];
                if (!ConfigLoader.RemoveHost(alias))
                {
                    Error.WriteLine($"Host alias '{alias}' not found.");
                    return 1;
                }
                WriteLine($"{Ansi.Subtle("Removed alias")} {Ansi.Em(alias)}.");
                return 0;
            }

            if (args.Contains("--export"))
            {
                var idx = Array.IndexOf(args, "--export");
                if (idx + 1 >= args.Length)
                {
                    Error.WriteLine($"{Ansi.Option("--export")} requires {Ansi.Placeholder("ALIAS")}.");
                    return 1;
                }
                ConfigLoader.ExportCommand(args[idx + 1]);
                return 0;
            }

            // Connect to alias
            var target = args[0];
            var hosts  = ConfigLoader.LoadConfig();
            var config = hosts.FirstOrDefault(h => h.Name.Equals(target, StringComparison.OrdinalIgnoreCase));

            if (config == null)
            {
                Error.WriteLine($"Host alias '{target}' not found. Run {Ansi.Option("--list")} to see saved aliases.");
                return 1;
            }

            var ssh = new SshConnectionService();
            return ssh.Connect(config, verbose);
        }
        catch (ArgumentException ex)
        {
            Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void PrintHelp()
    {
        WriteLine($"{Ansi.Header("ssh-c — Lightweight SSH Connection Manager")}\n");

        WriteLine(Ansi.Section("USAGE"));
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Placeholder("ALIAS")} {Ansi.Subtle("[--verbose]")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--add")} " +
                  $"{Ansi.Option("--name")}={Ansi.Placeholder("ALIAS")} " +
                  $"{Ansi.Option("--host")}={Ansi.Placeholder("HOSTNAME_OR_IP")} " +
                  $"{Ansi.Option("--user")}={Ansi.Placeholder("USERNAME")} " +
                  $"{Ansi.Option("--auth-type")}={Ansi.EnumSet("cert|password")}");
        WriteLine($"              " +
                  $"{Ansi.Subtle("[")}{Ansi.Option("--port")}={Ansi.Placeholder("PORT")}{Ansi.Subtle("]")} " +
                  $"{Ansi.Subtle("[")}{Ansi.Option("--identity-file")}={Ansi.Placeholder("PATH_TO_KEY")}{Ansi.Subtle("]")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--list")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--remove")} {Ansi.Placeholder("ALIAS")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--export")} {Ansi.Placeholder("ALIAS")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--check-updates")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--version")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--help")}");
        WriteLine();

        WriteLine(Ansi.Section("OPTIONS"));
        WriteLine($"  {Ansi.Option("-v")}, {Ansi.Option("--verbose")}         Print the ssh command before connecting");
        WriteLine($"      {Ansi.Option("--no-color")}         Disable ANSI colors (or set {Ansi.Option("NO_COLOR=1")})");
        WriteLine($"  {Ansi.Option("-V")}, {Ansi.Option("--version")}         Show version information");
        WriteLine($"  {Ansi.Option("-u")}, {Ansi.Option("--check-updates")}   Check for a newer release");
        WriteLine($"  {Ansi.Option("-h")}, {Ansi.Option("--help")}            Show this help");
        WriteLine();

        WriteLine(Ansi.Section("ADD FLAGS"));
        WriteLine($"  {Ansi.Option("--name")}={Ansi.Placeholder("ALIAS")}              Short name for the host (e.g. {Ansi.Em("prod")}, {Ansi.Em("db")}, {Ansi.Em("bastion")})");
        WriteLine($"  {Ansi.Option("--host")}={Ansi.Placeholder("HOSTNAME_OR_IP")}     FQDN or IP address (e.g. {Ansi.Em("server.example.com")}, {Ansi.Em("203.0.113.10")})");
        WriteLine($"  {Ansi.Option("--user")}={Ansi.Placeholder("USERNAME")}           SSH login user (e.g. {Ansi.Em("ubuntu")}, {Ansi.Em("ec2-user")}, {Ansi.Em("root")})");
        WriteLine($"  {Ansi.Option("--auth-type")}={Ansi.EnumSet("cert|password")}    Authentication method");
        WriteLine($"  {Ansi.Option("--port")}={Ansi.Placeholder("PORT")}              SSH port — default {Ansi.Em("22")}");
        WriteLine($"  {Ansi.Option("--identity-file")}={Ansi.Placeholder("PATH")}    Path to private key (required when {Ansi.Option("--auth-type")}={Ansi.Em("cert")})");
        WriteLine();

        WriteLine(Ansi.Section("EXAMPLES"));
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Placeholder("prod")} {Ansi.Option("--verbose")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--add")} {Ansi.Option("--name")}={Ansi.Placeholder("prod")} {Ansi.Option("--host")}={Ansi.Placeholder("203.0.113.10")} {Ansi.Option("--user")}={Ansi.Placeholder("ubuntu")} {Ansi.Option("--auth-type")}={Ansi.Em("cert")} {Ansi.Option("--identity-file")}={Ansi.Placeholder("~/.ssh/prod_ed25519")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--add")} {Ansi.Option("--name")}={Ansi.Placeholder("db")} {Ansi.Option("--host")}={Ansi.Placeholder("10.0.0.5")} {Ansi.Option("--user")}={Ansi.Placeholder("admin")} {Ansi.Option("--auth-type")}={Ansi.Em("password")} {Ansi.Option("--port")}={Ansi.Placeholder("2222")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--list")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--remove")} {Ansi.Placeholder("oldbox")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("--export")} {Ansi.Placeholder("prod")}");
        WriteLine($"  {Ansi.Command("ssh-c")} {Ansi.Option("-u")}");
        WriteLine();
    }
}
