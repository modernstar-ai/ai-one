using System.Text.Json;
using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.CosmosDb;

public static class DependencyInjection
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection services) =>
        services.AddSingleton<CosmosClient>(_ =>
        {
            var cosmosConfigs = Configs.CosmosDb;
            var cosmosClientOptions = new CosmosClientOptions();
            cosmosClientOptions.UseSystemTextJsonSerializerWithOptions = new()
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!string.IsNullOrEmpty(cosmosConfigs.Key))
            {
                return new CosmosClient(cosmosConfigs.Endpoint, new AzureKeyCredential(cosmosConfigs.Key), cosmosClientOptions);
            }

            return new CosmosClient(cosmosConfigs.Endpoint, new DefaultAzureCredential(), cosmosClientOptions);
        });
}