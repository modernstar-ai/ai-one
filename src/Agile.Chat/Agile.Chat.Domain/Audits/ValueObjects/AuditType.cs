using System.Text.Json.Serialization;

namespace Agile.Chat.Domain.Audits.ValueObjects;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditType
{
    Thread,
    Message,
}