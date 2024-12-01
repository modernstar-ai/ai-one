using Agile.Framework.Common.DomainAbstractions;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Interfaces;

public interface ICosmosRepository<T> where T : AggregateRoot
{
    //Task<IEnumerable<T>> GetItemsByQueryAsync(QueryDefinition queryDefinition);
    Task<T?> GetItemByIdAsync(string id, string? partitionKey = null);
    Task AddItemAsync(T item);
    Task UpdateItemByIdAsync(string id, T item, string? partitionKey = null);
    Task DeleteItemByIdAsync(string id, string? partitionKey = null);
}