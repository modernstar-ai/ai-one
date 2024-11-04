using agile_chat_api.Enums;
using System.Text.Json.Serialization;

public enum ToolType
{
    Database,
    LogicApp,
    ExternalAPI
}

public class Tool
{
    public Guid id { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string type { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ToolStatus Status { get; set; } = ToolStatus.Active;
    
    public string? Description { get; set; }

    public string JsonTemplate { get; set; } = string.Empty;

    public string DatabaseDSN { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;
    public string Api { get; set; }= string.Empty;
    public string DatabaseQuery { get; set; }= string.Empty;
    public string CreatedDate { get; set; }  = string.Empty;
    public string LastUpdatedDate { get; set; }  = string.Empty;
}
