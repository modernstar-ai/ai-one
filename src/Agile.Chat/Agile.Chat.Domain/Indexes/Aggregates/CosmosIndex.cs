using Agile.Framework.Common.DomainAbstractions;

namespace Agile.Chat.Domain.Indexes.Aggregates;

public class CosmosIndex : AuditableAggregateRoot
{
    private CosmosIndex(){}
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? Group { get; private set; }

    public static CosmosIndex Create(string name, 
        string description, 
        string? group)
    {
        //Do validation logic and throw domain level exceptions if fails
        return new CosmosIndex
        {
            Name = name,
            Description = description,
            Group = group
        };
    }
    
    public void Update(string description, string? group)
    {
        //Do validation logic and throw domain level exceptions if fails
        Group = group;
        Description = description;
        LastModified = DateTime.UtcNow;
    }
}