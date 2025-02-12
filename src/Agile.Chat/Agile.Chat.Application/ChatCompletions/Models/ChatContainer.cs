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

    public List<AzureSearchDocument> Citations { get; set; } = new();
}