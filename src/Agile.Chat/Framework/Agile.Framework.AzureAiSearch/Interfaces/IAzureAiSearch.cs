using Agile.Framework.AzureAiSearch.Models;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.Interfaces;

public interface IAzureAiSearch
{
    Task<AzureSearchDocument?> GetChunkByIdAsync(string indexName, string chunkId);
    Task<List<AzureSearchDocument>> SearchAsync(string indexName, AiSearchOptions aiSearchOptions);
    Task IndexDocumentsAsync(List<AzureSearchDocument> documents, string indexName);
    Task CreateIndexIfNotExistsAsync(string indexName);
    Task<bool> IndexExistsAsync(string indexName);
    Task DeleteIndexAsync(string indexName);
    Task<SearchIndexStatistics> GetIndexStatisticsByNameAsync(string indexName);
    Task DeleteFileContentsByIdAsync(string fileId, string indexName);
}