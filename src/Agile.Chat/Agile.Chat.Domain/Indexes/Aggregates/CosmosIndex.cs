using System.Text.Json.Serialization;
using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Indexes.Aggregates;

public class CosmosIndex : AuditableAggregateRoot
{
    [JsonConstructor]
    private CosmosIndex(string name, string description, int chunkSize, int chunkOverlap, string? group)
    {
        Name = name;
        Description = description;
        Group = group;
        ChunkSize = chunkSize;
        ChunkOverlap = chunkOverlap;
    }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int ChunkSize { get; private set; }
    public int ChunkOverlap { get; private set; }
    public string? Group { get; private set; }

    public static CosmosIndex Create(string name, 
        string description, 
        int chunkSize,
        int chunkOverlap,
        string? group)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new CosmosIndex(name, description, chunkSize, chunkOverlap, group);
    }
    
    public void Update(string description, string? group)
    {
        //Do validation logic and throw domain level exceptions if fails
        Group = group;
        Description = description;
        LastModified = DateTime.UtcNow;
    }
}