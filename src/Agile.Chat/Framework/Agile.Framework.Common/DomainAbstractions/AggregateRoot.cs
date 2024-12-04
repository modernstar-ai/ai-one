
using System.Text.Json.Serialization;

namespace Agile.Framework.Common.DomainAbstractions;

public abstract class AggregateRoot
{
    [JsonInclude]
    public string Id { get; private set; } = Guid.NewGuid().ToString();
}