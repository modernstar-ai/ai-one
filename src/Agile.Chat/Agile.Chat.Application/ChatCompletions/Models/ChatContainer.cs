using System.Text.RegularExpressions;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.Models;

namespace Agile.Chat.Application.ChatCompletions.Models;

public class ChatContainer
{
    public ChatThread Thread { get; set; }
    public Assistant? Assistant { get; set; }
    public IAppKernel AppKernel { get; set; }
    public IAzureAiSearch AzureAiSearch { get; set; }
    public List<Message> Messages { get; set; } = new();
    public List<object> Citations { get; set; } = new();
}

public class ChatContainerCitation
{
    public string Content { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}

public class ChatContainerCitationExt : ChatContainerCitation
{
    public int ReferenceNumber { get; set; }
    public string Content { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    
    public ChatContainerCitationExt(int referenceNumber, string content, string name, string url)
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
                {RemoveExtraWhitespaces(Content)}

                """;
    }

    string RemoveExtraWhitespaces(string input)
    {
        // First, remove redundant spaces (multiple spaces replaced by one)
        input = Regex.Replace(input, @" +", " ");

        // Then, reduce consecutive tabs to a single tab
        input = Regex.Replace(input, @"\t+", "\t");

        // Reduce consecutive newlines to a single newline
        input = Regex.Replace(input, @"\n+", "\a");

        input = Regex.Replace(input, @" \a+", "\a");
        input = Regex.Replace(input, @"\a+", "\n");
        // Return the modified result
        return input;
    }
}