using Azure.Core;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Models;

public class Indexes
{
    public required string id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? SecurityGroup { get; set; }

    public string CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
}