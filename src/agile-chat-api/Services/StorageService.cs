using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
        Task<string> UploadFileToBlobAsync(IFormFile file, string folderName);
    }
}

namespace Services
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public StorageService()
        {
            BlobServiceClient blobServiceClient = new(Config.BlobStorageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.BlobContainerName);
            _blobContainerClient.CreateIfNotExists();
        }

        public async Task<string> UploadFileToBlobAsync(IFormFile file, string folderName)
        {
            try
            {
                string fileName = Path.GetFileName(file.FileName);
                string blobPath = $"{folderName}/{fileName}";
                BlobClient _blobClient = _blobContainerClient.GetBlobClient(blobPath);

                // Check if both folder and file exist
                bool fileExists = await _blobClient.ExistsAsync();
                bool folderExists = false;
                await foreach (BlobItem _blobItem in _blobContainerClient.GetBlobsAsync(prefix: folderName))
                {
                    folderExists = true;
                    break;
                }
                // If both folder and file exist, do nothing
                if (folderExists && fileExists)
                {
                    return null;
                }
                await _blobClient.UploadAsync(file.OpenReadStream(), true);
                return _blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error uploading file: {file.FileName}", ex);
            }
        }
    }
}