using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Models;
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

        /// <summary>
        /// Deletes the file from BLOB asynchronous.
        /// </summary>
        /// <param name="blobUrl">The BLOB URL.</param>
        /// <returns></returns>
        Task DeleteFileFromBlobAsync(FileMetadata file);
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

        /// <summary>
        /// Uploads the file to BLOB asynchronous.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Error uploading file: {file.FileName}</exception>
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
                    return null!;
                }
                await _blobClient.UploadAsync(file.OpenReadStream(), true);
                return _blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error uploading file: {file.FileName}", ex);
            }
        }

        /// <summary>
        /// Deletes the file from BLOB asynchronous.
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="System.InvalidOperationException">Error deleting file from blob storage: {file.FileName}</exception>
        public async Task DeleteFileFromBlobAsync(FileMetadata file)
        {
            try
            {
                string fileName = Path.GetFileName(file.FileName);
                string blobPath = $"{file.Folder}/{fileName}";
                BlobClient _blobClient = _blobContainerClient.GetBlobClient(blobPath);
                await _blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"Blob '{file.FileName}' deleted successfully from container '{blobPath}'.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting file from blob storage: {file.FileName}", ex);
            }
        }
    }
}