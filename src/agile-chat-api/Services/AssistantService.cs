using agile_chat_api.Enums;
using DotNetEnv;
using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

public interface IAssistantService
{
    Task<IEnumerable<Assistant>> GetAllAsync();
    Task<Assistant?> GetByIdAsync(Guid id);
    Task CreateAsync(Assistant assistant);
    Task UpdateAsync(Assistant assistant);
    Task DeleteAsync(Guid id);
}

public class AssistantService : IAssistantService
{
    private readonly Microsoft.Azure.Cosmos.Container _container;
    private readonly ILogger<AssistantService> _logger;

    public AssistantService(ILogger<AssistantService> logger)
    {
        _logger = logger;

        string cosmosDbUri = Env.GetString("AZURE_COSMOSDB_URI") ??
                             throw new InvalidOperationException("Cosmos DB URI is missing.");
        string cosmosDbKey = Env.GetString("AZURE_COSMOSDB_KEY") ??
                             throw new InvalidOperationException("Cosmos DB Key is missing.");
        string databaseName = Env.GetString("AZURE_COSMOSDB_DB_NAME") ??
                              throw new InvalidOperationException("Cosmos DB Database Name is missing.");
        string containerName = "assistants";

        var cosmosClient = new CosmosClient(cosmosDbUri, cosmosDbKey);
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Assistant>> GetAllAsync()
    {
        var results = new List<Assistant>();

        try
        {
            var query = _container.GetItemQueryIterator<Assistant>();

            while (query.HasMoreResults)
            {
                var response =await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all assistants.");
            throw;
        }

        return results;
    }

    public async Task< Assistant?> GetByIdAsync(Guid id)
    {
        try
        {
            var query = _container.GetItemLinqQueryable<Assistant>(true)
                .Where(t => t.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            return await Task.FromResult(query);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving the assistant with ID {id}.");
            throw;
        }
    }

    public async Task CreateAsync(Assistant assistant)
    {
        try
        {
            assistant.Id = Guid.NewGuid();
            assistant.CreatedAt = DateTime.UtcNow;
            assistant.UpdatedAt = DateTime.UtcNow;
            await _container.CreateItemAsync(assistant, new PartitionKey(assistant.CreatedBy));
        }
        catch (CosmosException ex)
        {
            HandleCosmosException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating an assistant.");
            throw;
        }
    }

    public async Task UpdateAsync(Assistant assistant)
    {
        var existingAssistant = await GetByIdAsync(assistant.Id);
        if (existingAssistant != null)
        {
            existingAssistant.Name = assistant.Name;
            existingAssistant.Description = assistant.Description;
            existingAssistant.Type = assistant.Type;
            existingAssistant.Greeting = assistant.Greeting;
            existingAssistant.SystemMessage = assistant.SystemMessage;
            existingAssistant.Group = assistant.Group;
            existingAssistant.Folder = assistant.Folder;
            existingAssistant.Temperature = assistant.Temperature;
            existingAssistant.DocumentLimit = assistant.DocumentLimit;
            existingAssistant.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _container.ReplaceItemAsync(existingAssistant, assistant.Id.ToString(),
                    new PartitionKey(existingAssistant.CreatedBy));
            }
            catch (CosmosException ex)
            {
                HandleCosmosException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the assistant with ID {assistant.Id}.");
                throw;
            }
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var existingAssistant =await GetByIdAsync(id);
        if (existingAssistant != null)
        {
            try
            {
                await _container.DeleteItemAsync<Assistant>(id.ToString(), new PartitionKey(existingAssistant.CreatedBy));
            }
            catch (CosmosException ex)
            {
                HandleCosmosException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the assistant with ID {id}.");
                throw;
            }
        }
    }

    private void HandleCosmosException(CosmosException ex)
    {
        _logger.LogError(ex, "A Cosmos DB error occurred.");
        throw new InvalidOperationException("An error occurred while accessing Cosmos DB.", ex);
    }
}