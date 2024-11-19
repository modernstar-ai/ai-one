using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssistantStatus
{
    Draft,
    Published,
    Archived,
}