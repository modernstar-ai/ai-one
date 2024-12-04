namespace Agile.Framework.BlobStorage.Interfaces;

public interface IBlobStorage
{
    Task<string> UploadAsync(Stream stream, string fileName, string indexName, string? folderName = null);
    Task DeleteAsync(string fileName, string indexName, string? folderName = null);
    Task DeleteIndexFilesAsync(string indexName);
}