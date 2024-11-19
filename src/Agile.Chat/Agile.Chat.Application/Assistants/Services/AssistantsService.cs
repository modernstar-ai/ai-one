using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Assistants.Services;

public interface IAssistantsService  : ICosmosRepository<Assistant>
{
    public Task<List<Assistant>> GetAllAsync();
    public Task<Assistant?> GetByIdAsync(Guid id);
}

[Export(typeof(IAssistantsService), ServiceLifetime.Scoped)]
public class AssistantsService(CosmosClient cosmosClient) : 
    CosmosRepository<Assistant>(Constants.COSMOS_ASSISTANTS_CONTAINER_NAME, cosmosClient), IAssistantsService
{
    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name);
        var results = await CollectResultsAsync(query);
        return results;
    }
    
    public async Task<Assistant?> GetByIdAsync(Guid id)
    {
        var assistant = await GetItemAsync(id.ToString());
        return assistant;
    }
}