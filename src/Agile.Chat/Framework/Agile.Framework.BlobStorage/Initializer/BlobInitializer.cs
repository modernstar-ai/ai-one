using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.Common.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.BlobStorage.Initializer;

[Export(typeof(IAsyncInitializer), ServiceLifetime.Singleton)]
public class BlobInitializer(BlobServiceClient blobServiceClient) : IAsyncInitializer
{
    public async Task InitializeAsync()
    {
        var container = blobServiceClient.GetBlobContainerClient(Constants.BlobContainerName);
        await container.CreateIfNotExistsAsync();
    }
}