using Microsoft.Azure.Cosmos;
using Models;
using System.Collections.Concurrent;
using Config = agile_chat_api.Configurations.AppConfigs;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface ICosmosService
    {
        /// <summary>
        /// Files the metadata exists asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        Task<bool> FileMetadataExistsAsync(IFormFile file, string blobUrl, string folder);

        /// <summary>
        /// Saves the file metadata to cosmos database asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <returns></returns>
        Task SaveFileMetadataToCosmosDbAsync(IFormFile file, object blobUrl, string folderName);

        /// <summary>
        /// Gets the specific file asynchronous.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        Task<FileMetadata> GetFileByIdAsync(string fileId);

        /// <summary>
        /// Gets the file uploads asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FileMetadata>> GetFileUploadsAsync();

        /// <summary>
        /// Deletes the file metadata from cosmos database asynchronous.
        /// </summary>
        /// <param name="fileIds">The file ids.</param>
        /// <returns></returns>
        Task DeleteFileMetadataFromCosmosDbAsync(IEnumerable<string> fileIds);
    }
}

namespace Services
{
    public class CosmosService : ICosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _cosmosContainer;
        public CosmosService()
        {
            _cosmosClient = new CosmosClient(Config.CosmosEndpoint, Config.CosmosKey);
            _cosmosContainer = EnsureCosmosContainerExists().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Ensures the cosmos container exists.
        /// </summary>
        /// <returns></returns>
        private async Task<Container> EnsureCosmosContainerExists()
        {
            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(Config.CosmosDBName);
            ContainerResponse containerResponse = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
            {
                Id = Config.FileContainerName,
                PartitionKeyPath = Constants.FileContainerPartitionKeyPath
            });
            return containerResponse.Container;
        }

        /// <summary>
        /// Files the metadata exists asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <returns></returns>
        public async Task<bool> FileMetadataExistsAsync(IFormFile file, string blobUrl, string folder)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.FolderName = @folderName AND c.FileName = @fileName")
                    .WithParameter("@folderName", folder)
                    .WithParameter("@fileName", file.FileName);

                using FeedIterator<FileMetadata> feedIterator = _cosmosContainer.GetItemQueryIterator<FileMetadata>(query);

                if (feedIterator.HasMoreResults)
                {
                    var existingItems = await feedIterator.ReadNextAsync();
                    if (existingItems.Count > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                throw;
            }
            return false;
        }

        /// <summary>
        /// Saves the file metadata to cosmos database asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        public async Task SaveFileMetadataToCosmosDbAsync(IFormFile file, object blobUrl, string folderName)
        {
            try
            {
                var dateTimeString = DateTimeOffset.Now.ToString("dd-MM-yyyy HH:mm:ss");
                var fileMetadata = new FileMetadata
                {
                    id = Guid.NewGuid().ToString(), // Unique identity ID
                    FileId = Guid.NewGuid(),
                    FileName = Path.GetFileName(file.FileName),
                    BlobUrl = blobUrl,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    Folder = folderName,
                    SubmittedOn = dateTimeString
                };
                await _cosmosContainer.CreateItemAsync(fileMetadata, new PartitionKey(fileMetadata.id));
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Gets the file by identifier upload asynchronous.
        /// </summary>
        /// <param name="fileIds">The file ids.</param>
        /// <returns></returns>
        public async Task<FileMetadata> GetFileByIdAsync(string fileIds)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @Id")
                    .WithParameter("@Id", fileIds);
                using FeedIterator<FileMetadata> feedIterator = _cosmosContainer.GetItemQueryIterator<FileMetadata>(query);
                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<FileMetadata> response = await feedIterator.ReadNextAsync();
                    foreach (var fileMetadata in response)
                    {
                        return fileMetadata; // Return the first match found
                    }
                }
                return null!; // Return null if no results found
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Gets the file uploads asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FileMetadata>> GetFileUploadsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var results = new List<FileMetadata>();

            using (FeedIterator<FileMetadata> feedIterator = _cosmosContainer.GetItemQueryIterator<FileMetadata>(query))
            {
                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<FileMetadata> response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            return results;
        }

        /// <summary>
        /// Deletes the file metadata from cosmos database asynchronous.
        /// </summary>
        /// <param name="fileIds"></param>
        /// <exception cref="System.ArgumentException">No file IDs provided for deletion.</exception>
        public async Task DeleteFileMetadataFromCosmosDbAsync(IEnumerable<string> fileIds)
        {
            if (fileIds?.Any() != true)
            {
                throw new ArgumentException("No file IDs provided for deletion.");
            }
            var tasks = new List<Task>();
            var failedDeletions = new ConcurrentBag<string>();
            foreach (var fileId in fileIds)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await _cosmosContainer.DeleteItemAsync<FileMetadata>(fileId, new PartitionKey(fileId));
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // Log that the item was not found but continue with the deletion of other items
                        Console.WriteLine($"File with ID {fileId} not found. Skipping deletion.");
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // Handle rate limiting by retrying after a delay
                        Console.WriteLine($"Rate limited while deleting file ID {fileId}. Retrying after delay.");
                        if (ex.RetryAfter.HasValue)
                        {
                            await Task.Delay(ex.RetryAfter.Value);
                        }
                        failedDeletions.Add(fileId);  // Track files that failed due to rate limiting
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting file with ID {fileId}: {ex.Message}");
                        failedDeletions.Add(fileId);  // Track files that failed due to other errors
                    }
                }));
            }
            // Run all delete tasks in parallel
            await Task.WhenAll(tasks);

            // Retry failed deletions (due to rate limiting or other transient errors)
            if (!failedDeletions.IsEmpty)
            {
                Console.WriteLine("Retrying failed deletions...");
                foreach (var fileId in failedDeletions)
                {
                    try
                    {
                        await _cosmosContainer.DeleteItemAsync<FileMetadata>(fileId, new PartitionKey(fileId));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Final attempt to delete file with ID {fileId} failed: {ex.Message}");
                    }
                }
            }
        }
    }
}