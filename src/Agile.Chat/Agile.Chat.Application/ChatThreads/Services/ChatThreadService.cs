using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Agile.Chat.Domain.Assistants.Aggregates;

namespace Agile.Chat.Application.ChatThreads.Services;

public interface IChatThreadService : ICosmosRepository<ChatThread>
{
    public Task<List<ChatThread>> GetAllAsync(string username);
}

[Export(typeof(IChatThreadService), ServiceLifetime.Scoped)]
public class ChatThreadService(CosmosClient cosmosClient, IAssistantService assistantService) :
    CosmosRepository<ChatThread>(Constants.CosmosChatsContainerName, cosmosClient), IChatThreadService
{
    public async Task<List<ChatThread>> GetAllAsync(string username)
    {
        var query = LinqQuery()
            .Where(c => c.UserId == username && c.Type == ChatType.Thread)
            .OrderByDescending(c => c.LastModified);

        var results = await CollectResultsAsync(query);
        return results;
    }
}