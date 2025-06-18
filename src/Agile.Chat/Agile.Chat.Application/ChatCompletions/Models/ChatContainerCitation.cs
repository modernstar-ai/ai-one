namespace Agile.Chat.Application.ChatCompletions.Models;

public class ChatContainerCitation
{
    public int ReferenceNumber { get; set; }
    public string Content { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public ChatContainerCitation(int referenceNumber, string content, string name, string url)
    {
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
                Chunk:
                {ModelHelpers.RemoveExtraWhitespaces(Content)}

                """;
    }
}