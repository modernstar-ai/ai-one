using Agile.Chat.Domain.Indexes.Aggregates;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Indexes.Services;

public interface IIndexService  : ICosmosRepository<CosmosIndex>
{
    public bool Exists(string indexName);
    public Task<List<CosmosIndex>> GetAllAsync();
}

[Export(typeof(IIndexService), ServiceLifetime.Scoped)]
public class IndexService(CosmosClient cosmosClient, IRoleService roleService) : 
    CosmosRepository<CosmosIndex>(Constants.CosmosIndexesContainerName, cosmosClient), IIndexService
{
    public async Task<List<CosmosIndex>> GetAllAsync()
    {
        var query = LinqQuery().AsQueryable();
        if (!roleService.IsSystemAdmin())
        {
            var roleClaims = roleService.GetRoleClaims();
            query = query.Where(x => roleClaims.Contains(UserRole.ContentManager.ToString() + "." + (x.Group != null ? x.Group.ToLower() : "")));
        }
        
        var results = await CollectResultsAsync(query);
        return results;
    }

    public bool Exists(string indexName)
    {
        return LinqQuery().Where(x => x.Name == indexName).FirstOrDefault() != null;
    }
}