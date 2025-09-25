// Models/ConfigRoot.cs
using System.Collections.Generic;

namespace ssh_c.Models;

public class ConfigRoot
{
    public List<SshHostConfig>? Hosts { get; set; }
}
