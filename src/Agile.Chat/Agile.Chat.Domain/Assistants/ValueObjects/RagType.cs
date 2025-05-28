using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.Assistants.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RagType
{
    AzureSearchChatDataSource,
    Plugin
}