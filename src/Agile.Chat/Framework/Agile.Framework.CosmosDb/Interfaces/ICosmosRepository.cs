using Agile.Framework.Common.DomainAbstractions;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Interfaces;

public interface ICosmosRepository<T> where T : AggregateRoot
{
    //Task<IEnumerable<T>> GetItemsByQueryAsync(QueryDefinition queryDefinition);
    Task<T?> GetItemByIdAsync(string id, string? partitionKeyValue = null);
    Task AddItemAsync(T item, string? partitionKeyValue = null);
    Task UpdateItemByIdAsync(string id, T item, string? partitionKeyValue = null);
    Task DeleteItemByIdAsync(string id, string? partitionKeyValue = null);
}