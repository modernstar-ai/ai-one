using Agile.Framework.BlobStorage.Interfaces;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.BlobStorage;

[Export(typeof(IBlobStorage), ServiceLifetime.Singleton)]
public class BlobStorage(BlobServiceClient client, ILogger<BlobStorage> logger) : IBlobStorage
{
    private readonly BlobContainerClient _container = client.GetBlobContainerClient(Constants.BlobContainerName);

    public async Task<(Stream, BlobDownloadDetails)> DownloadAsync(string url)
    {
        BlobClient blob;
        if (!string.IsNullOrEmpty(Configs.BlobStorage.Key))
        {
            var credential = new StorageSharedKeyCredential(Configs.BlobStorage.AccountName, Configs.BlobStorage.Key);
            blob = new BlobClient(new Uri(url), credential);
        }
        else
        {
            var credential = new DefaultAzureCredential();
            blob = new BlobClient(new Uri(url), credential);
        }

        if (!await blob.ExistsAsync())
            throw new Exception("File doesn't exist");

        var resp = await blob.DownloadStreamingAsync();
        return (resp.Value.Content, resp.Value.Details);
    }

    public async Task<string> GetShareableLinkByUrlAsync(string url)
    {
        BlobClient blob;
        if (!string.IsNullOrEmpty(Configs.BlobStorage.Key))
        {
            var credential = new StorageSharedKeyCredential(Configs.BlobStorage.AccountName, Configs.BlobStorage.Key);
            blob = new BlobClient(new Uri(url), credential: credential);
        }
        else
        {
            var credential = new DefaultAzureCredential();
            blob = new BlobClient(new Uri(url), credential);
        }

        if (!await blob.ExistsAsync())
            throw new Exception("File doesn't exist");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = Constants.BlobContainerName,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
            ContentDisposition = "inline" // Set this to override on download,
        };
        sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

        var uri = blob.GenerateSasUri(sasBuilder);
        return uri.ToString();
    }

    public async Task<string> UploadAsync(Stream stream, string contentType, string fileName, string indexName, string? folderName = null)
    {
        var folderPath = string.IsNullOrWhiteSpace(folderName) ? fileName : $"{folderName}/{fileName}";
        var fullPath = $"{indexName}/{folderPath}";

        BlobClient blobClient = _container.GetBlobClient(fullPath);

        logger.LogInformation("Uploading {FileName} to Index {IndexName} with folder {FolderName}", fileName, indexName, folderName);
        var resp = await blobClient.UploadAsync(stream, new BlobHttpHeaders() { ContentType = contentType });
        logger.LogInformation("Finished upload with response {@Response}", resp.Value);
        return $"https://{blobClient.Uri.Host}{blobClient.Uri.LocalPath}";
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