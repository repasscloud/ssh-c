// Helpers/AppJsonContext.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ssh_c.Models;

namespace ssh_c.Helpers;

[JsonSerializable(typeof(ConfigRoot))]
[JsonSerializable(typeof(SshHostConfig))]
[JsonSerializable(typeof(List<SshHostConfig>))]
[JsonSerializable(typeof(GitHubRelease))]
public partial class AppJsonContext : JsonSerializerContext
{
}
