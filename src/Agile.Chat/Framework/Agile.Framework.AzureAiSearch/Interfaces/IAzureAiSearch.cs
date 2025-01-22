using Agile.Framework.AzureAiSearch.Models;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.Interfaces;

public interface IAzureAiSearch
{
    Task<AzureSearchDocument?> GetChunkByIdAsync(string indexName, string chunkId);
    Task<List<AzureSearchDocument>> SearchAsync(string indexName, AiSearchOptions aiSearchOptions);
    Task<bool> IndexerExistsAsync(string indexName);
    Task CreateIndexerAsync(string indexName);
    Task RunIndexerAsync(string indexName);
    Task DeleteIndexerAsync(string indexName);
    Task<SearchIndexStatistics> GetIndexStatisticsByNameAsync(string indexName);
    Task<IndexerDetail?> GetIndexersByIndexNameAsync(string indexName);
    Task<DataSourceDetail?> GetDataSourceByNameAsync(string indexName);
}