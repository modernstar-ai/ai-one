using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Models;
using System.Collections.Concurrent;
using agile_chat_api.Configurations;
using agile_chat_api.Utils;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface IFileUploadService
    {
        /// <summary>
        /// Files the metadata exists asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        Task<bool> FileMetadataExistsAsync(string fileName, string indexName, string folder);

        Task DeleteAllFilesInIndexAsync(string indexName);

        /// <summary>
        /// Saves the file metadata to cosmos database asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task SaveFileMetadataToCosmosDbAsync(string fileName, string contentType, long contentLength, object blobUrl,
            string indexName, string folderName);

        /// <summary>
        /// Gets the file by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<FileMetadata?> GetFileByIdAsync(string id);

        /// <summary>
        /// Gets the bulk files asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FileMetadata?>> GetFileUploadsAsync();

        /// <summary>
        /// Deletes the file metadata from cosmos using file name asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        Task DeleteFileByNameFromCosmosAsync(string fileName, string indexName, string folder);

        /// <summary>
        /// Deletes the bulk file metadata from cosmos asynchronous.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns></returns>
        Task DeleteBulkFileMetadataFromCosmosAsync(IEnumerable<string> files);
    }
}

namespace Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _cosmosContainer;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(ILogger<FileUploadService> logger)
        {
            _logger = logger;
            _cosmosClient = new CosmosClient(AppConfigs.CosmosEndpoint, AppConfigs.CosmosKey);
            _cosmosContainer = EnsureCosmosContainerExists().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Ensures the cosmos container exists.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private async Task<Container> EnsureCosmosContainerExists()
        {
            try
            {
                var dbName = AppConfigs.CosmosDBName;
                var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(dbName);
                ContainerResponse containerResponse = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
                {
                    Id = Constants.FileUploadContainerName,
                    PartitionKeyPath = Constants.FileContainerPartitionKeyPath
                });

                if (containerResponse == null)
                {
                    _logger.LogError("ContainerResponse is null");
                    throw new InvalidOperationException("Failed to create or retrieve the container.");
                }

                return containerResponse.Container;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ensuring Cosmos container exists: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Files the metadata exists asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<bool> FileMetadataExistsAsync(string fileName, string indexName, string folder)
        {
            var queryable = _cosmosContainer.GetItemLinqQueryable<FileMetadata>();

            // Use LINQ to filter documents based on FileName and Folder
            var filteredQuery = queryable
                .Where(file => file.FileName == fileName && file.IndexName == indexName && file.Folder == folder)
                .ToFeedIterator();

            try
            {
                List<FileMetadata> results = [];
                while (filteredQuery.HasMoreResults)
                {
                    FeedResponse<FileMetadata> response = await filteredQuery.ReadNextAsync();
                    results.AddRange(response);
                }
                if (results.Count > 0)
                {
                    return true;
                }
                FileMetadata? data = results.FirstOrDefault();
                _logger.LogInformation("Document Count: {Data}", data);
                return false;

            }
            catch (Exception)
            {
                _logger.LogError("Error checking if file metadata exists for {FileName} in folder {Folder}", fileName, folder);
                return false;
            }
        }

        /// <summary>
        /// Saves the file metadata to cosmos database asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <param name="folderName">Name of the folder.</param>
        public async Task SaveFileMetadataToCosmosDbAsync(string fileName, string contentType, long contentLength, object blobUrl, string indexName, string folderName)
        {
            try
            {
                var dateTimeString = DateTimeOffset.Now.ToString("dd-MM-yyyy HH:mm:ss");
                var fileMetadata = new FileMetadata
                {
                    id = Guid.NewGuid().ToString(), // Unique identity ID
                    FileName = Path.GetFileName(fileName),
                    BlobUrl = blobUrl,
                    ContentType = contentType,
                    Size = contentLength,
                    Folder = folderName,
                    IndexName = indexName,
                    SubmittedOn = dateTimeString
                };
                await _cosmosContainer.CreateItemAsync(fileMetadata, new PartitionKey(fileMetadata.id));
            }
            catch (Exception)
            {
                _logger.LogError("Error saving file metadata for {FileName} in index {IndexName} and folder {FolderName}", fileName, indexName, folderName);
            }
        }

        /// <summary>
        /// Gets the file by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<FileMetadata?> GetFileByIdAsync(string id)
        {
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<FileMetadata>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError("File with ID {Id} not found.", id);
                return null;
            }
            catch (Exception)
            {
                _logger.LogError("Error retrieving file with ID {Id}", id);
            }
            return null;
        }

        /// <summary>
        /// Gets the file upload asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FileMetadata?>> GetFileUploadsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var results = new List<FileMetadata>();
            try
            {
                using var feedIterator = _cosmosContainer.GetItemQueryIterator<FileMetadata>(query);
                while (feedIterator.HasMoreResults)
                {
                    results.AddRange(await feedIterator.ReadNextAsync());
                }
            }
            catch (Exception e)
            {
                
                _logger.LogError("Error retrieving all file metadata. Message: {Message} StackTrace: {StackTrace}", e.Message, e.StackTrace);
                return []; // Ensure non-null return
            }
            return results;
        }

        /// <summary>
        /// Deletes the file with retry asynchronous.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="failedDeletions">The failed deletions.</param>
        private async Task DeleteFileWithRetryAsync(string fileId, ConcurrentBag<string> failedDeletions)
        {
            try
            {
                await _cosmosContainer.DeleteItemAsync<FileMetadata>(fileId, new PartitionKey(fileId));
                _logger.LogInformation("File with ID {Id} deleted successfully.", fileId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogInformation("Rate limit hit for ID {Id}. Retrying after delay...", fileId);
                await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(1));
                failedDeletions.Add(fileId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("File with ID {fileId} not found. Skipping deletion.", fileId);
            }
            catch (Exception)
            {
                _logger.LogInformation("Error deleting file with ID {Id}", fileId);
                failedDeletions.Add(fileId);
            }
        }

        /// <summary>
        /// Deletes the file metadata from cosmos using file name asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task DeleteFileByNameFromCosmosAsync(string fileName, string indexName, string folder)
        {
            var query = new QueryDefinition("SELECT c.id FROM c WHERE c.FolderName = @folder AND c.FileName = @fileName AND c.IndexName = @indexName")
                .WithParameter("@folder", folder)
                .WithParameter("@fileName", fileName)
                .WithParameter("@indexName", indexName);
            try
            {
                using var feedIterator = _cosmosContainer.GetItemQueryIterator<FileMetadata>(query);
                while (feedIterator.HasMoreResults)
                {
                    var items = await feedIterator.ReadNextAsync();
                    var deleteTasks = items.Select(item => _cosmosContainer.DeleteItemAsync<FileMetadata>(item.id.ToString(), new PartitionKey(item.id.ToString())));
                    await Task.WhenAll(deleteTasks);
                }

                _logger.LogInformation("File(s) with name {Name} in folder {Folder} deleted successfully.", fileName, folder);
            }
            catch (Exception)
            {
                _logger.LogError("Error deleting file by name {Name} in folder {Folder}", fileName, folder);
                throw;
            }
        }

        /// <summary>
        /// Deletes the bulk file metadata from cosmos asynchronous.
        /// </summary>
        /// <param name="files">The files.</param>
        public async Task DeleteBulkFileMetadataFromCosmosAsync(IEnumerable<string> files)
        {
            var failedDeletions = new ConcurrentBag<string>();
            var deleteTasks = files.Select(id => DeleteFileWithRetryAsync(id, failedDeletions));
            await Task.WhenAll(deleteTasks);
            if (!failedDeletions.IsEmpty)
            {
                _logger.LogError($"Retrying failed deletions for {failedDeletions.Count} files.", failedDeletions.Count);
                foreach (var fileId in failedDeletions)
                {
                    await DeleteFileWithRetryAsync(fileId, failedDeletions);
                }
            }
        }
        
        public async Task DeleteAllFilesInIndexAsync(string indexName)
        {
            string sqlQuery = $"SELECT * FROM c WHERE c.IndexName = @value";
            var queryDefinition = new QueryDefinition(sqlQuery).WithParameter("@value", indexName);
            var feedIterator = _cosmosContainer.GetItemQueryIterator<dynamic>(queryDefinition);

            // Iterate through the results and delete the items
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                foreach (var item in response)
                {
                    // Extract the id or other unique key of the item to delete it
                    string id = item.id.ToString();

                    try
                    {
                        // Delete the item by id and partition key
                        await _cosmosContainer.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
                        _logger.LogInformation("Deleted item with id {Id}", id);
                    }
                    catch (CosmosException ex)
                    {
                        _logger.LogError("Error deleting item with id {Id}: Message: {Message}, StackTrace: {StackTrace}", id, ex.Message, ex.StackTrace);
                    }
                }
            }
        }
    }
}