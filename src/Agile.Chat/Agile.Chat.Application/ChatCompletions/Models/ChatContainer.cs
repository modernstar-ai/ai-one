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
    public string UserPrompt { get; set; }
    public ChatThread Thread { get; set; }
    
    public List<ChatThreadFile> ThreadFiles { get; set; } = new();
    public Assistant? Assistant { get; set; }
    public IAppKernel AppKernel { get; set; }
    public IAzureAiSearch AzureAiSearch { get; set; }
    public List<Message> Messages { get; set; } = new();
    public List<ChatContainerCitation> Citations { get; set; } = new();
}