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
        var query = LinqQuery().OrderBy(a => a.Name);
        var results = await CollectResultsAsync(query);

        if (!roleService.IsSystemAdmin())
        {
            var groupClaims = roleService.GetGroupClaims();
            results = results.Where(assistant =>
            {
                var group = assistant.FilterOptions.Group?.ToLower() ?? string.Empty;

                // For EndUser: Include assistants that are published and belong to accessible groups
                if (roleService.IsUserInRole(UserRole.EndUser, group))
                {
                    if (!string.IsNullOrWhiteSpace(group))
                    {
                        return groupClaims.Contains(group) && assistant.Status == AssistantStatus.Published;
                    }
                }

                // For ContentManager: Include assistants in accessible groups, regardless of status
                if (roleService.IsUserInRole(UserRole.ContentManager, group))
                {
                    if (!string.IsNullOrWhiteSpace(group))
                    {
                        return groupClaims.Contains(group);
                    }
                }

                // For assistants with no group: Include only published ones (applies to all roles)
                if (string.IsNullOrWhiteSpace(group))
                {
                    return assistant.Status == AssistantStatus.Published;
                }

                return false;
            }).ToList();
        }

        return results;
    }
}