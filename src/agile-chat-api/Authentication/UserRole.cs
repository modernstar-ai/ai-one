using System.Text.Json.Serialization;

namespace agile_chat_api.Authentication;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    SystemAdmin,
    ContentManager,
    EndUser
}