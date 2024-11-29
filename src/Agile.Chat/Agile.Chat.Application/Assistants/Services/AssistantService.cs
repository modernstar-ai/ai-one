using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Assistants.Services;

public interface IAssistantService  : ICosmosRepository<Assistant>
{
    public Task<List<Assistant>> GetAllAsync();
}

[Export(typeof(IAssistantService), ServiceLifetime.Singleton)]
public class AssistantService(CosmosClient cosmosClient) : 
    CosmosRepository<Assistant>(Constants.CosmosAssistantsContainerName, cosmosClient), IAssistantService
{
    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name);
        var results = await CollectResultsAsync(query);
        return results;
    }
}