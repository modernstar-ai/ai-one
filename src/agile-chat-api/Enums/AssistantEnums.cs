using System.Text.Json.Serialization;

namespace agile_chat_api.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum AssistantType
    {
        Chat,
        Search
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssistantStatus
    {
        Draft,
        Published,
        Archived,
        Deleted
    }

}
