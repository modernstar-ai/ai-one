using Agile.Framework.AzureAiSearch.Models;

namespace Agile.Framework.AzureAiSearch.Interfaces;

public interface IAzureAiSearch
{
    Task<List<AzureSearchDocument>> SearchAsync(string indexName, AiSearchOptions aiSearchOptions);
    Task<bool> IndexerExistsAsync(string indexName);
    Task CreateIndexerAsync(string indexName);
    Task RunIndexerAsync(string indexName);
    Task DeleteIndexerAsync(string indexName);
}