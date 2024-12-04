using System.Text.Json.Serialization;

namespace Agile.Framework.Common.DomainAbstractions;

public abstract class AuditableAggregateRoot : AggregateRoot
{
    [JsonInclude]
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    [JsonInclude]
    public DateTime LastModified { get; protected set; } = DateTime.UtcNow;
}