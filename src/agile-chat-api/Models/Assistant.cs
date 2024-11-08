using agile_chat_api.Enums;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

/// <summary>
/// Base class representing an assistant with common properties
/// </summary>
public class Assistant
{
    /// <summary>
    /// Unique identifier for the assistant
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the assistant
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the assistant
    /// </summary>
    public string Description { get; set; } = String.Empty;

    /// <summary>
    /// Type of the assistant
    /// </summary>
    public AssistantType Type { get; set; } = AssistantType.Chat;

    /// <summary>
    /// The welcome message displayed by the assistant
    /// </summary>
    public string Greeting { get; set; } = string.Empty;

    /// <summary>
    /// The System Message to control the assistants behavior
    /// </summary>
    public string SystemMessage { get; set; } = String.Empty;


    public string? Group { get; set; }
    public List<string>? Folder { get; set; }
    public decimal Temperature { get; set; }
    public decimal TopP { get; set; }
    public int DocumentLimit { get; set; }

    /// <summary>
    /// Current status of the assistant
    /// </summary>
    public AssistantStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    public List<Tools> Tools { get; set; } = [];
}
public class Tools
{
    [JsonPropertyName("toolId")]
    public string ToolId { get; set; } = string.Empty;

    [JsonPropertyName("toolName")]
    public string ToolName { get; set; } = string.Empty;
}