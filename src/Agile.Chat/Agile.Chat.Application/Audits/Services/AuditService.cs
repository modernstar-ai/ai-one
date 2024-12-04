using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.Audits.ValueObjects;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.DomainAbstractions;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Audits.Services;

public interface IAuditService<T> : ICosmosRepository<Audit<T>> where T : AuditableAggregateRoot
{
    Task UpdateItemByPayloadIdAsync(T item);
}

[Export(typeof(IAuditService<>), ServiceLifetime.Singleton)]
public class AuditService<T>(CosmosClient cosmosClient) : 
    CosmosRepository<Audit<T>>(Constants.CosmosAuditsContainerName, cosmosClient), IAuditService<T> where T : AuditableAggregateRoot
{
    public async Task UpdateItemByPayloadIdAsync(T item)
    {
        var auditItem = LinqQuery()
            .Where(x => x.Payload.Id == item.Id)
            .FirstOrDefault();

        if (auditItem == null) return;
        
        auditItem.Update(item);
        await UpdateItemByIdAsync(auditItem.Id, auditItem, auditItem.Type.ToString());
    }
}