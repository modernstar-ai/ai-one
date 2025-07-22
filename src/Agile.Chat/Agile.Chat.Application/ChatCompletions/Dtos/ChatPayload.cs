namespace Agile.Chat.Application.ChatCompletions.Dtos;

/// <summary>
/// Generic payload for chat operations that can be routed to different commands
/// </summary>
public class ChatPayload
{
    /// <summary>
    /// The type of chat operation
    /// </summary>
    public ChatPayloadType Type { get; set; } = ChatPayloadType.Standard;

    /// <summary>
    /// The user's prompt/message
    /// </summary>
    public string UserPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the thread this chat belongs to
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// Optional files attached to the chat
    /// </summary>
    public IEnumerable<ChatFileDto>? Files { get; set; }

    /// <summary>
    /// Any additional properties as a dictionary for extensibility
    /// </summary>
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}

/// <summary>
/// Types of chat payloads that determine routing
/// </summary>
public enum ChatPayloadType
{
    /// <summary>
    /// Standard chat without special handling
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Chat that should trigger search functionality
    /// </summary>
    Search = 1,

    /// <summary>
    /// Chat with file attachments
    /// </summary>
    WithFiles = 2,

    /// <summary>
    /// Chat using agents
    /// </summary>
    Agent = 3,
}

/// <summary>
/// DTO for chat file attachments
/// </summary>
public class ChatFileDto
{
    /// <summary>
    /// The name of the file
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The content of the file
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The MIME type of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Optional URL where the file is stored
    /// </summary>
    public string? Url { get; set; }
}
