using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Models;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using Dtos;
using Config = agile_chat_api.Configurations.AppConfigs;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface IContainerIndexerService
    {
        Task<bool> SaveIndexToCosmosDbAsync(IndexesDto indexRequest);
    }
}

namespace Services
{
    public class IndexerService : IContainerIndexerService
    {
        private readonly CosmosClient _cosmosClient = new(Config.CosmosEndpoint, Config.CosmosKey);
        private readonly Container _cosmosContainer;

        public IndexerService()
        {
            _cosmosContainer = EnsureCosmosContainerExists().GetAwaiter().GetResult();
        }

        private async Task<Container> EnsureCosmosContainerExists()
        {
            try
            {
                var dbName = Config.CosmosDBName;
                var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
                var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
                {
                    Id = Config.IndexContainerName,
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

        public async Task<bool> SaveIndexToCosmosDbAsync(IndexesDto indexRequest)
        {
            try
            {
                if (indexRequest == null) return false;
                var dateTimeString = DateTimeOffset.Now.ToString("dd-MM-yyyy HH:mm:ss");
                var index = new Indexes
                {
                    id = Guid.NewGuid().ToString(), // Unique identity ID
                    Name = indexRequest.Name,
                    Description = indexRequest.Description,
                    SecurityGroup = indexRequest.Group,
                    CreatedAt = dateTimeString,
                    CreatedBy = indexRequest.CreatedBy
                };
                var response = await _cosmosContainer.CreateItemAsync(index, new PartitionKey(index.id));

                if (response == null)
                    return false;
                else
                {
                    Console.WriteLine("Index created.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring Cosmos DB: {ex.Message}");
                return false;
            }
        }
    }
}