using Azure.Storage.Blobs.Models;

namespace Agile.Framework.BlobStorage.Interfaces;

public interface IBlobStorage
{
    Task<(Stream, BlobDownloadDetails)> DownloadAsync(string url);
    Task<string> GetShareableLinkByUrlAsync(string url);
    Task<string> UploadAsync(Stream stream, string contentType, string fileName, string indexName, string? folderName = null);
    Task<string> UploadThreadFileAsync(Stream stream, string contentType, string fileName, string threadId);
    Task DeleteThreadFileAsync(string fileName, string threadId);
    Task DeleteAsync(string fileName, string indexName, string? folderName = null);
    Task DeleteThreadFilesAsync(string threadId);
    Task DeleteIndexFilesAsync(string indexName);
}