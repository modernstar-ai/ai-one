using System.Net;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.CosmosDb.Initializer;

[Export(typeof(IAsyncInitializer), ServiceLifetime.Singleton)]
public class CosmosInitializer(CosmosClient client) : IAsyncInitializer
{
    private Database Db { get; set; }
    public async Task InitializeAsync()
    {
        await client.CreateDatabaseIfNotExistsAsync(Configs.CosmosDb.DatabaseName );
        Db = client.GetDatabase(Configs.CosmosDb.DatabaseName);
        
        await CreateOrUpdateContainerAsync(Constants.CosmosChatsContainerName, Constants.CosmosChatsPartitionKeyPath);
        await CreateOrUpdateContainerAsync(Constants.Cosmos.Files.ContainerName, Constants.Cosmos.Files.PartitionKeyPath, Constants.Cosmos.Files.SortableTextProperties);
        //await CreateOrUpdateContainerAsync(GetContainerProperties(Constants.CosmosToolsContainerName, "/");
        await CreateOrUpdateContainerAsync(Constants.CosmosAuditsContainerName, Constants.CosmosAuditsPartitionKeyPath);
        await CreateOrUpdateContainerAsync(Constants.CosmosIndexesContainerName, Constants.CosmosIndexesPartitionKeyPath);
        await CreateOrUpdateContainerAsync(Constants.CosmosAssistantsContainerName, Constants.CosmosAssistantsPartitionKeyPath);
    }

    private async Task CreateOrUpdateContainerAsync(string id, string partitionKey, string[]? sortableTextProperties = null)
    {
        var properties = GetContainerProperties(id,partitionKey, sortableTextProperties);
        var response = await Db.CreateContainerIfNotExistsAsync(properties);

        if (sortableTextProperties != null && response.StatusCode == HttpStatusCode.OK)
        {
            var container = await response.Container.ReadContainerAsync();
            var containerComputedPropertyNames = container.Resource.ComputedProperties.Select(x => x.Name).ToList();
            if (!containerComputedPropertyNames.SequenceEqual(sortableTextProperties.Select(name => $"lower{name}"), StringComparer.InvariantCultureIgnoreCase))
            {
                await container.Container.ReplaceContainerAsync(properties);
            }
        }
    }

    private ContainerProperties GetContainerProperties(string id, string partitionKey, string[]? sortableTextProperties = null)
    {
        var properties = new ContainerProperties(id, partitionKey);
        if (sortableTextProperties is null) return properties;
        
        //This is by default required for all containers in cosmos db
        properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath()
        {
            Path = $"/*"
        });
        
        foreach (var sortableTextProperty in sortableTextProperties)
        {
            //Add the computed property
            properties.ComputedProperties.Add(new ComputedProperty()
            {
                Name = $"lower{sortableTextProperty}", 
                Query = $"SELECT VALUE LOWER(c.{System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(sortableTextProperty)}) FROM c"
            });
            //Add the indexing policy
            properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath()
            {
                Path = $"/lower{sortableTextProperty}/?"
            });
        }
        
        return properties;
    }
}