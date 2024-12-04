using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Initializer;

[Export(typeof(IAsyncInitializer))]
public class CosmosInitializer(CosmosClient client) : IAsyncInitializer
{
    public async Task InitializeAsync()
    {
        await client.CreateDatabaseIfNotExistsAsync(Constants.CosmosDatabaseName);
        var db = client.GetDatabase(Constants.CosmosDatabaseName);
        //TODO: ADD PARTITION KEY PATHS
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosChatsContainerName, Constants.CosmosChatsPartitionKeyPath));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosFilesContainerName, Constants.CosmosFilesPartitionKeyPath));
        //await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosToolsContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosAuditsContainerName, Constants.CosmosAuditsPartitionKeyPath));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosIndexesContainerName, Constants.CosmosIndexesPartitionKeyPath));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosAssistantsContainerName, Constants.CosmosAssistantsPartitionKeyPath));
    }
}