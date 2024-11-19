namespace Agile.Framework.Common.DomainAbstractions;

public abstract class AuditableAggregateRoot : AggregateRoot
{
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public DateTime LastModified { get; protected set; } = DateTime.UtcNow;
}