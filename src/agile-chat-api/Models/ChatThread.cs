using Microsoft.AspNetCore.Http.HttpResults;

public class ChatThread
{
    public string id { get; set; } = Guid.NewGuid().ToString();  // CosmosDB uses string ids
    public string name { get; set; } = string.Empty;
    public string userName { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
    public string type { get; set; } = "CHAT_THREAD";
    public DateTime createdAt { get; set; } = DateTime.UtcNow;
    public DateTime lastMessageAt { get; set; } = DateTime.UtcNow;
    public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    public bool bookmarked { get; set; } = false;
    public bool isDeleted { get; set; } = false;
    public string assistantMessage { get; set; } = string.Empty;
    public string assistantTitle { get; set; } = string.Empty;
    public string assistantId { get; set; } = string.Empty;
    public List<string> extension { get; set; } = new List<string>();

    public decimal Temperature { get; set; }
    public decimal? TopP { get; set; }
    public int? MaxResponseToken { get; set; }
    public int? Strictness { get; set; }
    public int DocumentLimit { get; set; }

    public static string PartitionKey = "userId";
}


public class ExtensionUpdate
{
    public string ChatThreadId { get; set; } = string.Empty;
    public string ExtensionId { get; set; } = string.Empty;
}