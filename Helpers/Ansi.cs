// Helpers/Ansi.cs
using System;

namespace ssh_c.Helpers;

public static class Ansi
{
    private static bool _enabled =
        !Console.IsOutputRedirected &&
        Environment.GetEnvironmentVariable("NO_COLOR") is null;

    public static void SetEnabled(bool enabled) => _enabled = enabled;

    private const string Reset = "\u001b[0m";
    private const string Bold = "\u001b[1m";
    private const string Dim = "\u001b[2m";
    private const string FgCyan = "\u001b[36m";
    private const string FgBrightCyan = "\u001b[96m";
    private const string FgGreen = "\u001b[32m";
    private const string FgYellow = "\u001b[33m";
    private const string FgMagenta = "\u001b[35m";
    private const string FgGray = "\u001b[90m";

    private static string S(string s, string code) => _enabled ? $"{code}{s}{Reset}" : s;

    public static string Header(string s)       => S(s, Bold + FgBrightCyan);
    public static string Section(string s)      => S(s, Bold + FgCyan);
    public static string Command(string s)      => S(s, Bold + FgGreen);
    public static string Option(string s)       => S(s, FgYellow);
    public static string Placeholder(string s)  => S($"<{s}>", FgMagenta);
    public static string EnumSet(string s)      => S($"{{{s}}}", FgMagenta);
    public static string Subtle(string s)       => S(s, FgGray);
    public static string Em(string s)           => S(s, Bold);
}
