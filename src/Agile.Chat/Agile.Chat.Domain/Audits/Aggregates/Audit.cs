using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Agile.Chat.Domain.Audits.ValueObjects;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.DomainAbstractions;
using Agile.Framework.Common.EnvironmentVariables;

namespace Agile.Chat.Domain.Audits.Aggregates;

public class Audit<T> : AuditableAggregateRoot where T: AuditableAggregateRoot
{
    [JsonConstructor]
    private Audit(AuditType type, T payload)
    {
        Type = type;
        Payload = payload;
    }
    
    public AuditType Type { get; private set; }
    public T Payload { get; private set; }

    public static Audit<T> Create(T payload)
    {
        var auditType = payload switch
        {
            ChatThread => AuditType.Thread,
            Message => AuditType.Message,
            _ => throw new ArgumentException("Invalid Audit Type")
        };

        var filtered = FilterPII(payload, Configs.Audit.IncludePII);
        return new Audit<T>(auditType, filtered);
    }
    
    public void Update(T payload)
    {
        var filtered = FilterPII(payload, Configs.Audit.IncludePII);
        Payload = filtered;
    }
    
    private static T FilterPII(T obj, bool includePII)
    {
        if (includePII) return obj;

        var result = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // Check if the property is marked with the PIIAttribute
            bool isPII = property.GetCustomAttributes(typeof(PIIAttribute), false).Any();
            if (!isPII)
            {
                // Copy non-PII properties to the new object
                property.SetValue(result, property.GetValue(obj));
            }
        }
        return result;
    }
}