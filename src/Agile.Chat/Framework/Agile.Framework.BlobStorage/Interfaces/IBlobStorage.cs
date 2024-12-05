using Azure.Storage.Blobs.Models;

namespace Agile.Framework.BlobStorage.Interfaces;

public interface IBlobStorage
{
    Task<(Stream, BlobDownloadDetails)> DownloadAsync(string url);
    Task<string> GetShareableLinkByUrlAsync(string url);
    Task<string> UploadAsync(Stream stream, string fileName, string indexName, string? folderName = null);
    Task DeleteAsync(string fileName, string indexName, string? folderName = null);
    Task DeleteIndexFilesAsync(string indexName);
}