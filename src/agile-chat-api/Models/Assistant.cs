using agile_chat_api.Enums;
using System.Text.Json.Serialization;
using Newtonsoft.Json;


public class Assistant
{
    [JsonProperty("id")] public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = String.Empty;

    public AssistantType Type { get; set; } = AssistantType.Chat;

    public string Greeting { get; set; } = string.Empty;

    public string SystemMessage { get; set; } = String.Empty;

    public string? Group { get; set; }
    
    public string Index { get; set; }
    public List<string> Folder { get; set; } = new();

    public decimal Temperature { get; set; }
    public decimal TopP { get; set; }
    public int DocumentLimit { get; set; }
    public List<Tools> Tools { get; set; } = [];
    public AssistantStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
public class Tools
{
    [JsonPropertyName("toolId")]
    public string ToolId { get; set; } = string.Empty;

    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = string.Empty;
}