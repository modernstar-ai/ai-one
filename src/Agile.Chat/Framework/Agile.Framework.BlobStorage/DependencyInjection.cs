using Agile.Framework.Common.EnvironmentVariables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.BlobStorage;

public static class DependencyInjection
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services)
    {
        if (!string.IsNullOrEmpty(Configs.BlobStorage.Key))
        {
            return services.AddSingleton(_ => new BlobServiceClient(Configs.BlobStorage.ConnectionString()));
        }
        else
        {
            return services.AddSingleton(_ =>
            {
                return new BlobServiceClient(new Uri(Configs.BlobStorage.Endpoint),
                    new Azure.Identity.DefaultAzureCredential());
            });
        }
    }
}