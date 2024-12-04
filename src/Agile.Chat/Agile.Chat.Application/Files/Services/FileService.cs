using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.Authentication.Interfaces;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Agile.Framework.CosmosDb;
using Agile.Framework.CosmosDb.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Chat.Application.Files.Services;

public interface IFileService  : ICosmosRepository<CosmosFile>
{
    Task<List<CosmosFile>> GetAllAsync();
    Task<bool> ExistsAsync(string fileName, string indexName, string? folderName = null);
    Task DeleteByMetadataAsync(string fileName, string indexName, string? folderName = null);
    Task DeleteAllByIndexAsync(string indexName);
}

[Export(typeof(IFileService), ServiceLifetime.Scoped)]
public class FileService(CosmosClient cosmosClient, IIndexService indexService, IRoleService roleService) : 
    CosmosRepository<CosmosFile>(Constants.CosmosFilesContainerName, cosmosClient), IFileService
{
    public async Task<List<CosmosFile>> GetAllAsync()
    {
        var query = LinqQuery()
            .OrderBy(a => a.LastModified)
            .AsQueryable();
        
        if (!roleService.IsSystemAdmin())
        {
            var indexes = await indexService.GetAllAsync();
            var indexNames = indexes.Select(x => x.Name);
            query = query.Where(x => indexNames.Contains(x.IndexName));
        }
        
        var results = await CollectResultsAsync(query);
        return results;
    }

    public async Task<bool> ExistsAsync(string fileName, string indexName, string? folderName = null)
    {
        var query = LinqQuery()
            .Where(file => file.Name == fileName && file.IndexName == indexName && file.FolderName == folderName);
        
        var results = await CollectResultsAsync(query);
        return results.Count > 0;
    }

    public async Task DeleteByMetadataAsync(string fileName, string indexName, string? folderName = null)
    {
        var query = LinqQuery()
            .Where(file => file.Name == fileName && file.IndexName == indexName && file.FolderName == folderName);
        
        var results = await CollectResultsAsync(query);

        foreach (var result in results)
            await DeleteItemByIdAsync(result.Id);
    }

    public async Task DeleteAllByIndexAsync(string indexName)
    {
        var query = LinqQuery()
            .Where(file => file.IndexName == indexName);
        
        var results = await CollectResultsAsync(query);

        foreach (var result in results)
        {
            await DeleteItemByIdAsync(result.Id);
        }
    }
}