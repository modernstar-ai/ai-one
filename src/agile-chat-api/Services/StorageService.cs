using Azure.Storage.Blobs;
using Config = agile_chat_api.Configurations.AppConfigs;
using Constants = agile_chat_api.Configurations.Constants;

namespace Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads the file to BLOB asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        Task<string> UploadFileToBlobAsync(IFormFile file);
    }
}

namespace Services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public StorageService()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.BlobStorageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.BlobContainerName);
            _blobContainerClient.CreateIfNotExists();
        }

        public async Task<string> UploadFileToBlobAsync(IFormFile file)
        {
            try
            {
                string fileName = Path.GetFileName(file.FileName);
                BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
                await blobClient.UploadAsync(file.OpenReadStream(), true);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error uploading file: {file.FileName}", ex);
            }
        }
    }
}