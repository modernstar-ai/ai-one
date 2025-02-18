using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetadataType
{
    IsLiked,
    IsDisliked,
    Citations,
    SearchProcess
}