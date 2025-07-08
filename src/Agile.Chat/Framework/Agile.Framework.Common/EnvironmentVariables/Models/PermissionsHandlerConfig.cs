using System.Text.Json.Serialization;

namespace Agile.Framework.Common.EnvironmentVariables.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PermissionsType
{
    Default,
    AzureAd
}

public class AzureAdPermissionsConfig
{
    public HashSet<string> SystemAdminGroups { get; set; } = new();
    public HashSet<string> ContentManagerGroups { get; set; } = new();
}