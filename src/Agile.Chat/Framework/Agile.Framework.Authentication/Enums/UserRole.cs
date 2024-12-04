using System.Text.Json.Serialization;

namespace Agile.Framework.Authentication.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    SystemAdmin,
    ContentManager,
    EndUser
}