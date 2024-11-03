using agile_chat_api.Enums;
using System.Text.Json.Serialization;

public enum ToolType
{
    Database,
    LogicApp,
    ExternalAPI
}

/// <summary>
/// Base class representing a tool with common properties
/// </summary>
public class Tool
{
    /// <summary>
    /// Unique identifier for the tool
    /// </summary>
    public Guid id { get; set; }

    /// <summary>
    /// Name of the tool
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of the tool
    /// </summary>
   

    public string type { get; set; }

    /// <summary>
    /// Current status of the tool
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ToolStatus Status { get; set; }

    /// <summary>
    /// Optional description of the tool
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// JSON template for database operations
    /// </summary>
    public string JsonTemplate { get; set; }

    /// <summary>
    /// Database connection string or DSN
    /// </summary>
    public string DatabaseDSN { get; set; }

    
    public string Method { get; set; }
    public string Api { get; set; }
    public string DatabaseQuery { get; set; }
    public string CreatedDate { get; set; }  
    public string LastUpdatedDate { get; set; }  
}
