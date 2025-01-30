using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.Files.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileStatus
{
    Uploaded,
    Indexing,
    Indexed
}