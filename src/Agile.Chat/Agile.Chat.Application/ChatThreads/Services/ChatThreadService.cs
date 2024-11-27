using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.ChatThreads.Services;

public interface IChatThreadService  : ICosmosRepository<ChatThread>
{
    public Task<List<ChatThread>> GetAllAsync(string username);
}

[Export(typeof(IChatThreadService), ServiceLifetime.Singleton)]
public class ChatThreadService(CosmosClient cosmosClient) : 
    CosmosRepository<ChatThread>(Constants.COSMOS_CHATS_CONTAINER_NAME, cosmosClient, nameof(ChatThread.UserId)), IChatThreadService
{
    public async Task<List<ChatThread>> GetAllAsync(string username)
    {
        var query = LinqQuery()
            .Where(c => c.UserId == username)
            .OrderBy(c => c.LastModified);
        
        var results = await CollectResultsAsync(query);
        return results;
    }
}