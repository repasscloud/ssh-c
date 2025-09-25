// Models/SshHostConfig.cs
namespace ssh_c.Models;
public class SshHostConfig
{
    public string Name { get; set; } = "";
    public string User { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 22;
    public AuthConfig Auth { get; set; } = new();

    public class AuthConfig
    {
        public string Type { get; set; } = "password"; // or "cert"
        public string? IdentityFile { get; set; }
    }
}

