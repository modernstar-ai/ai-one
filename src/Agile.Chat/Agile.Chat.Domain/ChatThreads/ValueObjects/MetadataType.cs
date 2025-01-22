using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.ChatThreads.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetadataType
{
    Citations,
    DocumentsRetrieved,
    SearchProcess
}