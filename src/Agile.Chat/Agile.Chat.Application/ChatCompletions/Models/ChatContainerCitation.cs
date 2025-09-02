using System.Text.Json.Serialization;

namespace Agile.Chat.Application.ChatCompletions.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CitationType
{
    FileUpload,
    AzureSearch,
    WebSearch
}
public class ChatContainerCitation
{
    public CitationType CitationType { get; set; }
    public int ReferenceNumber { get; set; }
    public string Content { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public ChatContainerCitation(CitationType citationType, int referenceNumber, string content, string name, string url)
    {
        CitationType = citationType;
        ReferenceNumber = referenceNumber;
        Content = content;
        Name = name;   
        Url = url;
    }
    
    public new string ToString()
    {
        return $"""
                Reference Number:
                {ReferenceNumber}
                Title:
                {Name}
                Chunk (wrapped in stars *****):
                **********************************
                {ModelHelpers.RemoveExtraWhitespaces(Content)}
                **********************************
                """;
    }
}