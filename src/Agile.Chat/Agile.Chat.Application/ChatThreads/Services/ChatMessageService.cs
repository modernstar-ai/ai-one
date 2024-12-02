using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.ChatThreads.Services;

public interface IChatMessageService  : ICosmosRepository<Message>
{
    public Task<List<Message>> GetAllAsync(string threadId);
}

[Export(typeof(IChatMessageService), ServiceLifetime.Singleton)]
public class ChatMessageService(CosmosClient cosmosClient) : 
    CosmosRepository<Message>(Constants.CosmosChatsContainerName, cosmosClient), IChatMessageService
{
    public async Task<List<Message>> GetAllAsync(string threadId)
    {
        var query = LinqQuery()
            .Where(c => c.ThreadId == threadId)
            .OrderBy(c => c.CreatedDate);
        
        var results = await CollectResultsAsync(query);
        return results;
    }
}