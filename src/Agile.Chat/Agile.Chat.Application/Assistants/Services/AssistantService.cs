using Agile.Chat.Application.ExtensionMethods;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Framework.Authentication.Enums;
using Agile.Framework.Authentication.Interfaces;
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

[Export(typeof(IAssistantService), ServiceLifetime.Scoped)]
public class AssistantService(CosmosClient cosmosClient, IRoleService roleService) : 
    CosmosRepository<Assistant>(Constants.CosmosAssistantsContainerName, cosmosClient), IAssistantService
{
    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name).AsQueryable();

        if (roleService.IsSystemAdmin())
            return await CollectResultsAsync(query);

        var roleClaims = roleService.GetRoleClaims();
        if (roleClaims.Count == 0 || (roleClaims.Count == 1 && roleClaims.First() == UserRole.EndUser.ToString()))
            query = query.Where(x => x.Status == AssistantStatus.Published);

        query = query.AccessControlQuery(roleService);

        return await CollectResultsAsync(query);
    }
}