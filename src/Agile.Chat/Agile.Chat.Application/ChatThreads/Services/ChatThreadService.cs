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
    Task<ChatThread> GetChatThreadById(string id);
    public Task<List<ChatThread>> GetAllAsync(string username);
}

[Export(typeof(IChatThreadService), ServiceLifetime.Scoped)]
public class ChatThreadService(CosmosClient cosmosClient, IAssistantService assistantService) :
    CosmosRepository<ChatThread>(Constants.CosmosChatsContainerName, cosmosClient), IChatThreadService
{
    public async Task<ChatThread> GetChatThreadById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        var chatThread = await GetItemByIdAsync(id, ChatType.Thread.ToString());

        if (chatThread is null)
            return chatThread;

        var assistant = await assistantService.GetAssistantById(chatThread.AssistantId);
        ApplyModelOptions(chatThread, assistant);
        return chatThread;
    }

    public async Task<List<ChatThread>> GetAllAsync(string username)
    {
        var query = LinqQuery()
            .Where(c => c.UserId == username && c.Type == ChatType.Thread)
            .OrderByDescending(c => c.LastModified);

        var results = await CollectResultsAsync(query);
        if (results.Count == 0)
            return results;

        var assistantId = results.First().AssistantId;
        var assistant = await assistantService.GetAssistantById(assistantId);
        foreach (var chatThread in results)
        {
            ApplyModelOptions(chatThread, assistant);
        }

        return results;
    }

    private void ApplyModelOptions(ChatThread? chatThread, Assistant? assistant)
    {
        if (assistant is null || chatThread is null)
            return;

        if (chatThread != null &&
           (chatThread.ModelOptions is null ||
           string.IsNullOrEmpty(chatThread.ModelOptions.ModelId)))
        {
            var modelOptions = assistant.ModelOptions.ParseChatThreadModelOptions();
            chatThread.UpdateModelOptions(modelOptions);
        }

        //reset model id if model selection is not allowed
        if (!Configs.AppSettings.AllowModelSelectionDefaultValue)
        {
            chatThread.ModelOptions.ModelId = Configs.AppSettings.DefaultTextModelId;
        }
    }
}