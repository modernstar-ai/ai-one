using System.Text.Json.Serialization;

namespace Agile.Framework.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResponseType
{
    Chat,
    Citations
}