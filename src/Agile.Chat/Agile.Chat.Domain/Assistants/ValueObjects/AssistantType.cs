using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssistantType
{
    Chat,
    Search
}