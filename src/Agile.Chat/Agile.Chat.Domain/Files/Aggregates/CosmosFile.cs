using System.Text.Json.Serialization;
using Agile.Chat.Domain.Files.ValueObjects;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Files.Aggregates;

public class CosmosFile : AuditableAggregateRoot
{
    [JsonConstructor]
    private CosmosFile(string name, FileStatus status, string url, string? contentType, long size, string indexName, string? folderName, List<string> tags)
    {
        Name = name;
        Status = status;
        Url = url;
        ContentType = contentType;
        Size = size;
        IndexName = indexName;
        FolderName = folderName;
        Tags = tags;
    }
    public string Name { get; private set; }
    public FileStatus Status { get; private set; }
    public string Url { get; private set; }
    public string? ContentType { get; private set; }
    public long Size { get; private set; }
    public string IndexName { get; private set; }
    public string? FolderName { get; private set; }
    public List<string> Tags { get; private set; }

    public static CosmosFile Create(string name, 
        string? contentType,
        long size,
        string indexName,
        string? folderName,
        List<string> tags)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new CosmosFile(name, FileStatus.Uploaded, string.Empty, contentType, size, indexName, folderName, tags);
    }
    
    public void Update(FileStatus status, string url, string? contentType, long size, List<string> tags)
    {
        Status = status;
        ContentType = contentType;
        Url = url;
        Size = size;
        Tags = tags;
        LastModified = DateTime.UtcNow;
    }
}