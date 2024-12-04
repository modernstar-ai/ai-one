using System.Text.Json.Serialization;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Files.Aggregates;

public class CosmosFile : AuditableAggregateRoot
{
    [JsonConstructor]
    private CosmosFile(string name, string url, string? contentType, long size, string indexName, string? folderName)
    {
        Name = name;
        Url = url;
        ContentType = contentType;
        Size = size;
        IndexName = indexName;
        FolderName = folderName;
    }
    public string Name { get; private set; }
    public string Url { get; private set; }
    public string? ContentType { get; private set; }
    public long Size { get; private set; }
    public string IndexName { get; private set; }
    public string? FolderName { get; private set; }

    public static CosmosFile Create(string name, 
        string url, 
        string? contentType,
        long size,
        string indexName,
        string? folderName)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new CosmosFile(name, url, contentType, size, indexName, folderName);
    }
}