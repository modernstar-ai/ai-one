using Agile.Framework.BlobStorage.Interfaces;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.BlobStorage;

[Export(typeof(IBlobStorage), ServiceLifetime.Singleton)]
public class BlobStorage(BlobServiceClient client, ILogger<BlobStorage> logger) : IBlobStorage
{
    private readonly BlobContainerClient _container = client.GetBlobContainerClient(Constants.BlobContainerName);

    public async Task<string> UploadAsync(Stream stream, string fileName, string indexName, string? folderName = null)
    {
        var folderPath = string.IsNullOrWhiteSpace(folderName) ? fileName : $"{folderName}/{fileName}";
        var fullPath = $"{indexName}/{folderPath}";
        
        BlobClient blobClient = _container.GetBlobClient(fullPath);
        if(await blobClient.ExistsAsync()) throw new Exception("Blob already exists");
        
        logger.LogInformation("Uploading {FileName} to Index {IndexName} with folder {FolderName}", fileName, indexName, folderName);
        var resp = await blobClient.UploadAsync(stream);
        logger.LogInformation("Finished upload with response {@Response}", resp.Value);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileName, string indexName, string? folderName = null)
    {
        var folderPath = string.IsNullOrWhiteSpace(folderName) ? fileName : $"{folderName}/{fileName}";
        var fullPath = $"{indexName}/{folderPath}";
        
        logger.LogInformation("Deleting file: {FileName} with Index: {IndexName} with folder: {FolderName}", fileName, indexName, folderName);
        var resp = await _container.DeleteBlobIfExistsAsync(fullPath, DeleteSnapshotsOption.IncludeSnapshots);
        logger.LogInformation("Finished deleting file with response: {Response}", resp.Value);
    }
    
    public async Task DeleteIndexFilesAsync(string indexName)
    {
        await foreach (var blobItem in _container.GetBlobsAsync(prefix: indexName))
        {
            var blobClient = _container.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            logger.LogInformation("Deleted file: {FileName} with Index: {IndexName}", blobItem.Name, indexName);
        }
    }
}