namespace Agile.Framework.Common.DomainAbstractions;

public abstract class AggregateRoot
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
}