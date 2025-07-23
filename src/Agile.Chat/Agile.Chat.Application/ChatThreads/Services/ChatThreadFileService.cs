using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.ChatThreads.Services;

public interface IChatThreadFileService  : ICosmosRepository<ChatThreadFile>
{
    public Task<List<ChatThreadFile>> GetAllAsync(string threadId);
}

[Export(typeof(IChatThreadFileService), ServiceLifetime.Singleton)]
public class ChatThreadFileService(CosmosClient cosmosClient) : 
    CosmosRepository<ChatThreadFile>(Constants.CosmosChatsContainerName, cosmosClient), IChatThreadFileService
{
    public async Task<List<ChatThreadFile>> GetAllAsync(string threadId)
    {
        var query = LinqQuery(ChatType.File.ToString())
            .Where(c => c.Type == ChatType.File && c.ThreadId == threadId)
            .OrderByDescending(c => c.LastModified);
        
        var results = await CollectResultsAsync(query);
        return results;
    }
}