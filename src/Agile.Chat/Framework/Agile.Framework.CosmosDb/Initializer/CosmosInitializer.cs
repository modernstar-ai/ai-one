using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Initializer;

//[Export(typeof(IAsyncInitializer))]
public class CosmosInitializer(CosmosClient client) : IAsyncInitializer
{
    public async Task InitializeAsync()
    {
        await client.CreateDatabaseIfNotExistsAsync(Constants.COSMOS_DATABASE_NAME);
        var db = client.GetDatabase(Constants.COSMOS_DATABASE_NAME);
        //TODO: ADD PARTITION KEY PATHS
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_CHATS_CONTAINER_NAME, ""));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_FILES_CONTAINER_NAME, ""));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_TOOLS_CONTAINER_NAME, ""));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_AUDITS_CONTAINER_NAME, ""));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_INDEXES_CONTAINER_NAME, ""));
        await db.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.COSMOS_ASSISTANTS_CONTAINER_NAME, ""));
    }
}