namespace Agile.Framework.AzureAiSearch.Interfaces;

public interface IAzureAiSearch
{
    Task<bool> IndexerExistsAsync(string indexName);
    Task CreateIndexerAsync(string indexName);
    Task RunIndexerAsync(string indexName);
    Task DeleteIndexerAsync(string indexName);
}