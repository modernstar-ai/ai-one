using System.Linq.Dynamic.Core;
using System.Net;
using Agile.Framework.Common.DomainAbstractions;
using Agile.Framework.Common.Dtos;
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

    public CosmosRepository(string containerId, CosmosClient cosmosClient)
    {
        CosmosClient = cosmosClient;
        Database = cosmosClient.GetDatabase(Configs.CosmosDb.DatabaseName);
        Container = Database.GetContainer(containerId);
    }

    //All the supported LINQ operations ref: https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/linq-to-sql#supported-linq-operators
    protected IOrderedQueryable<T> LinqQuery(string? partitionKey = null) => Container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: true, 
        requestOptions: partitionKey != null ? new QueryRequestOptions() {PartitionKey = new PartitionKey(partitionKey) } : null);

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

    protected async Task<PagedResultsDto<T>> CollectResultsAsync(IQueryable<T> query, QueryDto queryDto)
    {
        //Ordering by
        var isQuerying = !string.IsNullOrWhiteSpace(queryDto.OrderBy) && queryDto.OrderType != null;
        if (isQuerying)
        {
            query = query.OrderBy(queryDto.OrderBy + " " + queryDto.OrderType);
        }

        var countResponse = await query.Select(x => x.Id).CountAsync();
        //paging
        query = query.Skip((queryDto.Page ?? 0) * (queryDto.PageSize ?? 10)).Take(queryDto.PageSize ?? 10);
        var results = new List<T>();
        FeedIterator<T> feed;
        if (!isQuerying)
        {
            feed = query.ToFeedIterator();
        }
        else
        {
            var queryText = query.ToQueryDefinition().QueryText;

            var isString = typeof(T).GetProperty(queryDto.OrderBy!)?.PropertyType == typeof(string);
            if (isString)
            {
                var fieldName = $"lower{queryDto.OrderBy}";
                queryText = queryText.Replace($"\"{System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(queryDto.OrderBy!)}\"",
                    $"\"{fieldName}\"");
            }

            feed = Container.GetItemQueryIterator<T>(new QueryDefinition(queryText));
        }

        while (feed.HasMoreResults)
        {
            foreach (var item in await feed.ReadNextAsync())
                results.Add(item);
        }

        return new PagedResultsDto<T>
        {
            Page = queryDto.Page ?? 0,
            PageSize = queryDto.PageSize ?? 10,
            TotalCount = countResponse.Resource,
            Items = results
        };
    }

    public Task AddItemAsync(T item)
    {
        return Container.CreateItemAsync(item);
    }

    public Task DeleteItemByIdAsync(string id, string? partitionKey = null)
    {
        var key = partitionKey != null ? new PartitionKey(partitionKey) : new PartitionKey(id);
        return Container.DeleteItemAsync<T>(id, key);
    }

    public async Task<T?> GetItemByIdAsync(string id, string? partitionKey = null)
    {
        var key = partitionKey != null ? new PartitionKey(partitionKey) : new PartitionKey(id);
        try
        {
            ItemResponse<T> response = await Container.ReadItemAsync<T>(id, key);
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

    public async Task UpdateItemByIdAsync(string id, T item, string? partitionKey = null)
    {
        var key = partitionKey != null ? new PartitionKey(partitionKey) : new PartitionKey(id);
        await Container.UpsertItemAsync(item, key);
    }
}