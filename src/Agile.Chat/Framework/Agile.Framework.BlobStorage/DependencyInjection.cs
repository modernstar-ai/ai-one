using Agile.Framework.Common.EnvironmentVariables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.BlobStorage;

public static class DependencyInjection
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services) =>
        services.AddSingleton<BlobServiceClient>(_ => new BlobServiceClient(Configs.BlobStorage.ConnectionString()));
}