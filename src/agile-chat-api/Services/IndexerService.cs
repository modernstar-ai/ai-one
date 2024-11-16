using agile_chat_api.Configurations;
using Microsoft.Azure.Cosmos;
using Models;
using Dtos;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface IContainerIndexerService
    { 
        bool IndexExistsAsync(string indexName);
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

        public IndexerService()
        {
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
                Console.WriteLine("ContainerResponse is null.");
                throw new InvalidOperationException("Failed to create or retrieve the container.");
                return containerResponse.Container;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring Cosmos container exists: {ex.Message}");
                throw;
            }
        }

        public bool IndexExistsAsync(string indexName)
        {
            return _cosmosContainer.GetItemLinqQueryable<Indexes>().Where(x => x.Name == indexName).FirstOrDefault() != null;
        }

        public async Task<IEnumerable<Indexes>> GetContainerIndexesAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var results = new List<Indexes>();
            try
            {
                using var feedIterator = _cosmosContainer.GetItemQueryIterator<Indexes>(query);
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }
                return results;
            }
            catch (CosmosException cosmosEx)
            {
                Console.WriteLine($"Cosmos DB error: {cosmosEx.Message}");
                return Enumerable.Empty<Indexes>(); // Return an empty list to ensure non-null
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return Enumerable.Empty<Indexes>(); // Return an empty list to ensure non-null
            }
        }

        public async Task<Indexes?> SaveIndexToCosmosDbAsync(IndexesDto indexRequest)
        {
            try
            {
                if (indexRequest == null)
                {
                    Console.WriteLine("Index request cannot be null.");
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
                    Console.WriteLine("Index created successfully.");
                    return indexMetadata;
                }
                Console.WriteLine($"Failed to create item with status code: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving index to Cosmos DB: {ex.Message}");
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
                    Console.WriteLine($"Index with ID {indexId} deleted successfully.");
                    return index;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        Console.WriteLine($"Rate limit hit for ID {indexId}. Max retry attempts reached. Deletion failed.");
                        throw;
                    }
                    Console.WriteLine($"Rate limit hit for ID {indexId}. Retrying after delay (attempt {attempt}/{maxRetries})...");
                    await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(1));
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Index with ID {indexId} not found. Skipping deletion.");
                    return null;
                }
                catch (Exception)
                {
                    attempt++;
                    if (attempt >= maxRetries)
                    {
                        Console.WriteLine($"Error deleting Index with ID {indexId}. Max retry attempts reached. Deletion failed.");
                        throw;
                    }
                    Console.WriteLine($"Error deleting Index with ID {indexId}. Retrying (attempt {attempt}/{maxRetries})...");
                }
            }

            return null;
        }
    }
}