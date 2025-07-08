
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MediatR;

namespace Agile.Framework.Common.DomainAbstractions;

public abstract class AggregateRoot
{
    [JsonInclude]
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    
    private readonly List<INotification> _events = new();

    [NotMapped]
    [JsonIgnore]
    public IReadOnlyList<INotification> Events => _events.AsReadOnly();

    public void AddEvent(INotification @event)
    {
        _events.Add(@event);
    }

    protected void RemoveEvent(INotification @event)
    {
        _events.Remove(@event);
    }
}