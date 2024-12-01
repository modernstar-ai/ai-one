using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Cosmos;
using System.Reflection;
using agile_chat_api.Authentication;
using agile_chat_api.Authentication.UTS;
using agile_chat_api.Enums;
using Microsoft.Azure.Cosmos.Linq;
using Config = agile_chat_api.Configurations.AppConfigs;
using agile_chat_api.Configurations;

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
    private readonly Container _container;
    private readonly ILogger<AssistantService> _logger;
    private readonly IRoleService _roleService;
    private readonly CosmosClient _cosmosClient;

    public AssistantService(ILogger<AssistantService> logger, IRoleService roleService)
    {
        _roleService = roleService;
        _logger = logger;
        _cosmosClient = new CosmosClient(Config.CosmosEndpoint, Config.CosmosKey);
        _container = _cosmosClient.GetContainer(Config.CosmosDBName, Constants.AssistantContainerName);
        _container = EnsureCosmosContainerExists().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Ensures the cosmos container exists.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    private async Task<Container> EnsureCosmosContainerExists()
    {
        try
        {
            var dbName = AppConfigs.CosmosDBName;
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
           
            ContainerResponse containerResponse = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = Constants.AssistantContainerName,
                PartitionKeyPath = Constants.AssistantContainerPartitionKeyPath
            });

            if (containerResponse == null)
            {
                _logger.LogError("ContainerResponse is null");
                throw new InvalidOperationException("Failed to create or retrieve the container.");
            }

            return containerResponse.Container;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error ensuring Cosmos container exists: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<Assistant>> GetAllAsync()
    {
        var results = new List<Assistant>();

        try
        {
            var query = _container.GetItemLinqQueryable<Assistant>().AsQueryable();

            var iterator = query.ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.Where(assistant =>
                {
                    if (string.IsNullOrWhiteSpace(assistant.Group))
                        return true;

                    return assistant.Status == AssistantStatus.Published ||
                           _roleService.IsUserInRole(UserRole.ContentManager, assistant.Group.ToLower());
                }));
            }

            if (!_roleService.IsSystemAdmin())
            {
                var groupClaims = _roleService.GetGroupClaims();
                results = results.Where(assistant =>
                {
                    // For EndUser: Only include assistants that are published and belong to groups the user has access to
                    if (_roleService.IsUserInRole(UserRole.EndUser, assistant.Group?.ToLower() ?? string.Empty))
                    {
                        return groupClaims.Contains(assistant.Group?.ToLower()) && assistant.Status == AssistantStatus.Published;
                    }

                    // For ContentManager: Include any assistants in the accessible group (ignoring status)
                    if (_roleService.IsUserInRole(UserRole.ContentManager, assistant.Group?.ToLower() ?? string.Empty))
                    {
                        return groupClaims.Contains(assistant.Group?.ToLower());
                    }

                    // For null/empty groups: Only include assistants that are published
                    if (string.IsNullOrWhiteSpace(assistant.Group))
                    {
                        return assistant.Status == AssistantStatus.Published;
                    }

                    // Default: Exclude any assistants not matching the above criteria
                    return false;
                }).ToList();
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

            if (query is not null && !string.IsNullOrWhiteSpace(query.Group) &&
                !_roleService.IsUserInGroup(query.Group))
                return null;

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
            existingAssistant.Status = assistant.Status;
            existingAssistant.Greeting = assistant.Greeting;
            existingAssistant.SystemMessage = assistant.SystemMessage;
            existingAssistant.Group = assistant.Group;
            existingAssistant.Index = assistant.Index;
            existingAssistant.Folder = assistant.Folder;
            existingAssistant.Temperature = assistant.Temperature;
            existingAssistant.TopP = assistant.TopP;
            existingAssistant.MaxResponseToken = assistant.MaxResponseToken;
            existingAssistant.Strictness = assistant.Strictness;
            existingAssistant.PastMessages = assistant.PastMessages;
            existingAssistant.DocumentLimit = assistant.DocumentLimit;
            existingAssistant.UpdatedAt = DateTime.UtcNow;
            existingAssistant.Tools = assistant.Tools;
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