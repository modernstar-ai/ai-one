using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

public class MessageOptions
{
    [JsonInclude]
    public bool IsLiked { get; internal set; } = false;
    [JsonInclude]
    public bool IsDisliked { get; internal set; } = false;
    [JsonInclude]
    public Dictionary<MetadataType, object> Metadata { get; internal set; } = new();
}