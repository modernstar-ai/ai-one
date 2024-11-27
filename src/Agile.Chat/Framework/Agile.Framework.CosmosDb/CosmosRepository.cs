using System.Net;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.DomainAbstractions;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Agile.Framework.CosmosDb;

public abstract class CosmosRepository<T> : ICosmosRepository<T> where T : AggregateRoot
{
    protected CosmosClient CosmosClient { get; }
    private Database Database { get; }
    private Container Container { get; }
    private readonly string? _partitionKey;

    public CosmosRepository(string containerId, CosmosClient cosmosClient, string? partitionKey = null)
    {
        CosmosClient = cosmosClient;
        Database = cosmosClient.GetDatabase(Constants.COSMOS_DATABASE_NAME);
        Container = Database.GetContainer(containerId);
        _partitionKey = partitionKey;
    }
    
    //All the supported LINQ operations ref: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/linq-to-sql#supported-linq-operators
    protected IOrderedQueryable<T> LinqQuery() => Container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: true);

    protected async Task<List<T>> CollectResultsAsync(IQueryable<T> query)
    {
        var results = new List<T>();
        var feed = query.ToFeedIterator();

        while (feed.HasMoreResults)
        {
            foreach (var item in await feed.ReadNextAsync())
                results.Add(item);
        }

        return results;
    }

    public Task AddItemAsync(T item)
    {
        var partitionKey = _partitionKey != null ? new PartitionKey(_partitionKey) : new PartitionKey(item.Id);
        return Container.CreateItemAsync(item, partitionKey);
    }

    public Task DeleteItemByIdAsync(string id)
    {
        var partitionKey = _partitionKey != null ? new PartitionKey(_partitionKey) : new PartitionKey(id);
        return Container.DeleteItemAsync<T>(id, partitionKey);
    }

    public async Task<T?> GetItemByIdAsync(string id)
    {
        var partitionKey = _partitionKey != null ? new PartitionKey(_partitionKey) : new PartitionKey(id);
        try
        {
            ItemResponse<T> response = await Container.ReadItemAsync<T>(id, partitionKey);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    // public async Task<IEnumerable<T>> GetItemsByQueryAsync(QueryDefinition queryDefinition)
    // {
    //     var query = Container.GetItemQueryIterator<T>(queryDefinition);
    //     List<T> results = new List<T>();
    //     while (query.HasMoreResults)
    //     {
    //         var response = await query.ReadNextAsync();
    //         results.AddRange(response.ToList());
    //     }
    //
    //     return results;
    // }

    public async Task UpdateItemByIdAsync(string id, T item)
    {
        var partitionKey = _partitionKey != null ? new PartitionKey(_partitionKey) : new PartitionKey(id);
        await Container.UpsertItemAsync(item, partitionKey);
    }
}