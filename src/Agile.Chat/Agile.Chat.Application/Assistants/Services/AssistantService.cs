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

[Export(typeof(IAssistantService), ServiceLifetime.Singleton)]
public class AssistantService(CosmosClient cosmosClient, IRoleService roleService) : 
    CosmosRepository<Assistant>(Constants.CosmosAssistantsContainerName, cosmosClient), IAssistantService
{
    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name).AsQueryable();

        if (roleService.IsSystemAdmin())
            return await CollectResultsAsync(query);

        var roleClaims = roleService.GetRoleClaims();
        query = query.Where(x =>
            (x.FilterOptions.Group == null || x.FilterOptions.Group == string.Empty)
                ? x.Status == AssistantStatus.Published
                : roleClaims.Contains(UserRole.ContentManager.ToString() + "." + x.FilterOptions.Group.ToLower()) ||
                  (roleClaims.Contains(UserRole.EndUser.ToString() + "." + x.FilterOptions.Group.ToLower()) && x.Status == AssistantStatus.Published)
        );

        return await CollectResultsAsync(query);
    }
}