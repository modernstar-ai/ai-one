using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb.Interfaces;
using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.CosmosDb;

public static class DependencyInjection
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services) =>
        services.AddSingleton<CosmosClient>(_ =>
        {
            var cosmosConfigs = Configs.CosmosDb;
            var client = new CosmosClient(cosmosConfigs.Endpoint, new AzureKeyCredential(cosmosConfigs.Key));
            return client;
        });
}