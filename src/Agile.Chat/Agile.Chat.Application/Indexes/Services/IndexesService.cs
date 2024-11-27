using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Indexes.Services;

public interface IAssistantsService  : ICosmosRepository<Assistant>
{
    public Task<List<Assistant>> GetAllAsync();
}

[Export(typeof(IAssistantsService), ServiceLifetime.Singleton)]
public class IndexesService(CosmosClient cosmosClient) : 
    CosmosRepository<Assistant>(Constants.COSMOS_ASSISTANTS_CONTAINER_NAME, cosmosClient), IAssistantsService
{
    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name);
        var results = await CollectResultsAsync(query);
        return results;
    }
}