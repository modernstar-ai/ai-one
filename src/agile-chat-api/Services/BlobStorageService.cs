using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;

namespace agile_chat_api.Services;

public interface IBlobStorageService
{
    Task<List<string>> GetHighLevelFolders(string containerName);
}

public class BlobStorageService : IBlobStorageService
{
    public static readonly string FOLDERS_CONTAINER_NAME =
        Environment.GetEnvironmentVariable("AZURE_SEARCH_FOLDERS_INDEX_NAME")!;

    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<List<string>> GetHighLevelFolders(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var delimiter = "/";

        var results = containerClient.GetBlobsByHierarchyAsync(delimiter: delimiter);
        var folders = new List<string>();

        await foreach (var result in results)
        {
            if(result.IsPrefix)
                folders.Add(result.Prefix.Replace("/", ""));
        }

        return folders;
    }
}