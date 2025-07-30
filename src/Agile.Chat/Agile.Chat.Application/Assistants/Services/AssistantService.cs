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

public interface IAssistantService : ICosmosRepository<Assistant>
{
    public Task<Assistant> GetAssistantById(string id);
    public Task<List<Assistant>> GetAllAsync();
    
    public Task<List<Assistant>> GetAllAgentsAsync();
}

[Export(typeof(IAssistantService), ServiceLifetime.Scoped)]
public class AssistantService(CosmosClient cosmosClient, IAssistantModelConfigService assistantModelConfigService, IRoleService roleService) :
    CosmosRepository<Assistant>(Constants.CosmosAssistantsContainerName, cosmosClient), IAssistantService
{
    public async Task<Assistant> GetAssistantById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));

        var assistant = await GetItemByIdAsync(id);
        ApplyModelOptions(assistant);

        return assistant;
    }

    public async Task<List<Assistant>> GetAllAsync()
    {
        var query = LinqQuery().OrderBy(a => a.Name).AsQueryable();
        return await CollectAssistantResultsAsync(query);
    }

    public async Task<List<Assistant>> GetAllAgentsAsync()
    {
        var query = LinqQuery()
            .Where(a => a.Type == AssistantType.Agent)
            .OrderBy(a => a.Name)
            .AsQueryable();
        return await CollectAssistantResultsAsync(query);
    }

    private async Task<List<Assistant>> CollectAssistantResultsAsync(IQueryable<Assistant> query)
    {
        List<Assistant> results = new List<Assistant>();

        if (roleService.IsSystemAdmin())
        {
            results = await CollectResultsAsync(query);
        }
        else
        {
            var roleClaims = roleService.GetRoleClaims();
            if (roleClaims.Count == 0 || (roleClaims.Count == 1 && roleClaims.First() == UserRole.EndUser.ToString()))
                query = query.Where(x => x.Status == AssistantStatus.Published);

            query = query.AccessControlQuery(roleService);
            results = await CollectResultsAsync(query);
        }

        foreach (var assistant in results)
        {
            ApplyModelOptions(assistant);
        }

        return results;
    }

    private void ApplyModelOptions(Assistant? assistant)
    {
        if (assistant is null)
            return;

        //remove model options if model selection is not allowed
        if (!assistantModelConfigService.ModelSelectionFeatureEnabled)
        {
            var defaultModelOptions = assistantModelConfigService.GetDefaultModelOptions();
            assistant.UpdateModelOptions(defaultModelOptions);
        }
        else
        {
            //backward compatibility for old assistants
            if (assistant!.ModelOptions is null ||
                string.IsNullOrEmpty(assistant.ModelOptions.DefaultModelId))
            {
                var defaultModelOptions = assistantModelConfigService.GetDefaultModelOptions();
                assistant.UpdateModelOptions(defaultModelOptions);
            }
        }

        //if (assistant!.ModelOptions is null ||
        //   string.IsNullOrEmpty(assistant.ModelOptions.DefaultModelId) ||
        //   assistant.ModelOptions.Models.Count == 0)
        //{
        //    var defaultModelOptions = assistantModelConfigService.GetDefaultModelOptions();
        //    assistant.UpdateModelOptions(defaultModelOptions);
        //}
        //else
        //{
        //    if (!Configs.AppSettings.AllowModelSelection &&
        //        assistant.ModelOptions.AllowModelSelection)
        //    {
        //        //reset assistant model options
        //        assistant.ModelOptions.AllowModelSelection = false;
        //        var defaultModelOptions = assistantModelConfigService.GetDefaultModelOptions();
        //        assistant.UpdateModelOptions(defaultModelOptions);
        //    }
        //}
    }
}