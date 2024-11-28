using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Initializer;

//[Export(typeof(IAsyncInitializer))]
public class CosmosInitializer(CosmosClient client) : IAsyncInitializer
{
    public async Task InitializeAsync()
    {
        await client.CreateDatabaseIfNotExistsAsync(Constants.CosmosDatabaseName);
        var db = client.GetDatabase(Constants.CosmosDatabaseName);
        //TODO: ADD PARTITION KEY PATHS
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosChatsContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosFilesContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosToolsContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosAuditsContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosIndexesContainerName, "/"));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosAssistantsContainerName, "/"));
    }
}