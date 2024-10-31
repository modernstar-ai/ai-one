using Microsoft.Azure.Cosmos;
using Models;
using Config = agile_chat_api.Configurations.AppConfigs;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface ICosmosService
    {
        /// <summary>
        /// Files the metadata exists asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <returns></returns>
        Task<bool> FileMetadataExistsAsync(string fileName, string blobUrl);

        /// <summary>
        /// Saves the file metadata to cosmos database asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <returns></returns>
        Task SaveFileMetadataToCosmosDbAsync(IFormFile file, object blobUrl);

        /// <summary>
        /// Gets the file uploads asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FileMetadata>> GetFileUploadsAsync();
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
        public async Task<bool> FileMetadataExistsAsync(string fileName, string blobUrl)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.FileName = @fileName OR c.BlobUrl = @blobUrl")
                    .WithParameter("@fileName", fileName)
                    .WithParameter("@blobUrl", blobUrl);

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
        public async Task SaveFileMetadataToCosmosDbAsync(IFormFile file, object blobUrl)
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
    }
}