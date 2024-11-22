using agile_chat_api.Authentication;
using agile_chat_api.Configurations;
using Microsoft.Azure.Cosmos;
using Models;
using Dtos;
using Microsoft.Azure.Cosmos.Linq;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface IContainerIndexerService
    { 
        bool IndexExistsAsync(string indexName);
        Task<Indexes?> GetContainerIndexByIdAsync(string id);
        Task UpdateAsync(Indexes index, string newDescription, string newGroup);
        Task<Indexes?> GetContainerIndexByNameAsync(string indexName);
        Task<IEnumerable<Indexes>> GetContainerIndexesAsync();
        Task<Indexes?> SaveIndexToCosmosDbAsync(IndexesDto indexRequest);
        Task<Indexes?> DeleteIndexWithRetryAsync(string indexId);
    }
}

namespace Services
{
    public class IndexerService : IContainerIndexerService
    {
        private readonly CosmosClient _cosmosClient = new(AppConfigs.CosmosEndpoint, AppConfigs.CosmosKey);
        private readonly Container _cosmosContainer;
        private readonly ILogger<IndexerService> _logger;
        private readonly IRoleService _roleService;

        public IndexerService(ILogger<IndexerService> logger, IRoleService roleService)
        {
            _roleService = roleService;
            _logger = logger;
            _cosmosContainer = EnsureCosmosContainerExists().GetAwaiter().GetResult();
        }

        private async Task<Container> EnsureCosmosContainerExists()
        {
            try
            {
                var dbName = AppConfigs.CosmosDBName;
                var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
                var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
                {
                    Id = Constants.IndexContainerName,
                    PartitionKeyPath = Constants.IndexContainerPartitionKeyPath
                });

                if (containerResponse != null) return containerResponse.Container;
                _logger.LogCritical("ContainerResponse is null.");
                throw new InvalidOperationException("Failed to create or retrieve the container.");
                return containerResponse.Container;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ensuring Cosmos container exists: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        public bool IndexExistsAsync(string indexName)
        {
            return _cosmosContainer.GetItemLinqQueryable<Indexes>().Where(x => x.Name == indexName).FirstOrDefault() != null;
        }
        
        public async Task UpdateAsync(Indexes index, string newDescription, string newGroup)
        {
            if (!_roleService.IsSystemAdmin())
            {
                var groupClaims = _roleService.GetGroupClaims();
                if (!string.IsNullOrWhiteSpace(index.Group) && !groupClaims.Contains(index.Group.ToLower()))
                    throw new Exception("Not authorized");
            }
            
            index.Description = newDescription;
            index.Group = newGroup;
            try
            {
                await _cosmosContainer.ReplaceItemAsync(index, index.id,
                    new PartitionKey(index.id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the assistant with ID {index.id}.");
                throw;
            }
        }
        
        public async Task<Indexes?> GetContainerIndexByIdAsync(string id)
        {
            var index = _cosmosContainer.GetItemLinqQueryable<Indexes>()
                .Where(x => x.id == id)
                .FirstOrDefault();
            if (!_roleService.IsSystemAdmin())
            {
                var groupClaims = _roleService.GetGroupClaims();
                if(!string.IsNullOrWhiteSpace(index?.Group) && !groupClaims.Contains(index.Group.ToLower()))
                    return null;
            }

            return index;
        }

        public async Task<Indexes?> GetContainerIndexByNameAsync(string indexName)
        {
            var index = _cosmosContainer.GetItemLinqQueryable<Indexes>()
                .Where(x => x.Name == indexName)
                .FirstOrDefault();
            if (!_roleService.IsSystemAdmin())
            {
                var groupClaims = _roleService.GetGroupClaims();
                if(!string.IsNullOrWhiteSpace(index?.Group) && !groupClaims.Contains(index.Group.ToLower()))
                    return null;
            }

            return index;
        }
        
        public async Task<IEnumerable<Indexes>> GetContainerIndexesAsync()
        {
            var results = new List<Indexes>();
            try
            {
                var query = _cosmosContainer.GetItemLinqQueryable<Indexes>().AsQueryable();
                if (!_roleService.IsSystemAdmin())
                {
                    var groupClaims = _roleService.GetGroupClaims();
                    query = query.Where(x => x.Group == null || x.Group == "" || groupClaims.Contains(x.Group.ToLower()));
                }

                var feedIterator = query.ToFeedIterator();
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }
                return results;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError("Cosmos DB Error: {Message}, StackTrace: {StackTrace}", cosmosEx.Message, cosmosEx.StackTrace);
                return Enumerable.Empty<Indexes>(); // Return an empty list to ensure non-null
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected Error: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return Enumerable.Empty<Indexes>(); // Return an empty list to ensure non-null
            }
        }

        public async Task<Indexes?> SaveIndexToCosmosDbAsync(IndexesDto indexRequest)
        {
            try
            {
                if (indexRequest == null)
                {
                    _logger.LogInformation("Index request cannot be null.");
                    return null;
                }

                var dateTimeString = DateTimeOffset.Now.ToString("dd-MM-yyyy HH:mm:ss");
                var indexMetadata = new Indexes()
                {
                    id = Guid.NewGuid().ToString(), // Unique identity ID
                    Name = indexRequest.Name,
                    Description = indexRequest.Description,
                    Group = indexRequest.Group,
                    CreatedAt = dateTimeString,
                    CreatedBy = indexRequest.CreatedBy
                };
                var response = await _cosmosContainer.CreateItemAsync(indexMetadata, new PartitionKey(indexMetadata.id));
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    _logger.LogInformation("Index created successfully.");
                    return indexMetadata;
                }
                _logger.LogError("Failed to create item with status code: {StatusCode}, Message: {Message}", response.StatusCode, response.Diagnostics.ToString());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while saving index to Cosmos DB: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return null;
            }
        }

        public async Task<Indexes?> DeleteIndexWithRetryAsync(string indexId)
        {
            const int maxRetries = 3;
            var attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    var index = _cosmosContainer.GetItemLinqQueryable<Indexes>().Where(x => x.id == indexId).FirstOrDefault();
                    var resp = await _cosmosContainer.DeleteItemAsync<Indexes>(indexId, new PartitionKey(indexId));
                    _logger.LogInformation("Index with ID {Id} deleted successfully.", indexId);
                    return index;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        _logger.LogError("Rate limit hit for ID: {IndexId}. Max retry attempts reached. Deletion failed. Message: {Message} StackTrace: {StackTrace}", 
                            indexId, ex.Message, ex.StackTrace);
                        throw;
                    }
                    
                    _logger.LogError("Rate limit hit for ID: {IndexId}. Retrying after delay (atempt {Attempt}/{MaxTries}).... Message: {Message} StackTrace: {StackTrace}", 
                        indexId, attempt, maxRetries, ex.Message, ex.StackTrace);
                    await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(1));
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Index with ID {Id} not found. Skipping deletion", indexId);
                    return null;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        _logger.LogError("Error deleting Index with ID {Id}. Max Retry attempts reached. Deletion failed. Message: {Message}, StackTrace: {StackTrace}", 
                            indexId, ex.Message, ex.StackTrace);
                        throw;
                    }
                    _logger.LogError("Error deleting Index with ID {Id}. Retrying (attempt {Attempt}/{MaxRetries})...", indexId, attempt, maxRetries);
                }
            }

            return null;
        }
    }
}