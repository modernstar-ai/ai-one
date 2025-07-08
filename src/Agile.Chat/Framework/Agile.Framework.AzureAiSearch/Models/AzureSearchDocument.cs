namespace Agile.Framework.AzureAiSearch.Models;

public class AzureSearchDocument
{
    public static AzureSearchDocument Create(string fileId, string chunk, string fileName, string url, List<string> tags, ReadOnlyMemory<float> chunkVector, ReadOnlyMemory<float> nameVector)
    {
        return new AzureSearchDocument()
        {
            Id = Guid.NewGuid() + "_" + fileId,
            FileId = fileId,
            Chunk = chunk,
            Name = fileName,
            Url = url,
            ChunkVector = chunkVector,
            NameVector = nameVector,
            Tags = tags ?? new()
        };
    }
    public string Id { get; set; }
    public string FileId { get; set; }
    public string Chunk { get; set; }
    public ReadOnlyMemory<float> ChunkVector { get; set; }
    public string Name { get; set; }
    public ReadOnlyMemory<float> NameVector { get; set; }
    public string Url { get; set; }
    public List<string> Tags { get; set; } = new();
}
