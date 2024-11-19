using Agile.Framework.Common.DomainAbstractions;
using Microsoft.Azure.Cosmos;

namespace Agile.Framework.CosmosDb.Interfaces;

public interface ICosmosRepository<T> where T : AggregateRoot
{
    //All the supported Cosmos LINQ operations
    //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/linq-to-sql#supported-linq-operators
    IOrderedQueryable<T> LinqQuery();
    Task<List<T>> CollectResultsAsync(IQueryable<T> query);
    
    //Task<IEnumerable<T>> GetItemsByQueryAsync(QueryDefinition queryDefinition);
    Task<T?> GetItemAsync(string id, string? partitionKeyValue = null);
    Task AddItemAsync(T item, string? partitionKeyValue = null);
    Task UpdateItemAsync(string id, T item, string? partitionKeyValue = null);
    Task DeleteItemAsync(string id, string? partitionKeyValue = null);
}