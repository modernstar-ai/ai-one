using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Attributes;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Search.Documents.Models;

namespace Agile.Framework.AzureAiSearch;

public interface IAzureAiSearch
{
    Task<AzureSearchDocument?> GetChunkByIdAsync(string indexName, string chunkId);
    Task IndexDocumentsAsync(List<AzureSearchDocument> documents, string indexName);
    Task CreateIndexIfNotExistsAsync(string indexName);
    Task SyncIndexAsync(string indexName);
    Task<bool> IndexExistsAsync(string indexName);
    Task DeleteIndexAsync(string indexName);
    Task<SearchIndexStatistics> GetIndexStatisticsByNameAsync(string indexName);
    Task DeleteFileContentsByIdAsync(string fileId, string indexName);
}

[Export(typeof(IAzureAiSearch), ServiceLifetime.Singleton)]
public class AzureAiSearch(SearchIndexClient indexClient, ILogger<AzureAiSearch> logger) : IAzureAiSearch
{
    public async Task<AzureSearchDocument?> GetChunkByIdAsync(string indexName, string chunkId)
    {
        var searchClient = indexClient.GetSearchClient(indexName);
        var searchResults = await searchClient.GetDocumentAsync<AzureSearchDocument>(chunkId);
        return !searchResults.HasValue ? null : searchResults.Value;
    }

    public async Task DeleteFileContentsByIdAsync(string fileId, string indexName)
    {
        var searchClient = indexClient.GetSearchClient(indexName);
        
        while (true)
        {
            int batchSize = 1000; // Adjust batch size based on your needs
            string filter = $"{nameof(AzureSearchDocument.FileId)} eq '{fileId}'";

            var options = new SearchOptions
            {
                Filter = filter,
                Select = { nameof(AzureSearchDocument.Id) },
                Size = batchSize
            };

            var searchResults = await searchClient.SearchAsync<AzureSearchDocument>(options);

            var documentIds = searchResults.Value.GetResults().Select(x => x.Document.Id).ToList();
            if (documentIds.Count == 0) break;

            // Create document objects with just the ID property
            var documents = documentIds.Select(id => new AzureSearchDocument { Id = id });
        
            // Create the batch with the document objects
            var batch = IndexDocumentsBatch.Delete(documents);
            await searchClient.IndexDocumentsAsync(batch);

            logger.LogInformation($"Deleted {documentIds.Count} documents where {nameof(AzureSearchDocument.FileId)} = '{fileId}'.");
        }
    }
    
    #region Indexes

    public async Task IndexDocumentsAsync(List<AzureSearchDocument> documents, string indexName)
    {
        if (documents.Count == 0) return;
        
        var searchClient = indexClient.GetSearchClient(indexName);

        int skip = 0;
        int take = 100;
        
        while (true)
        {
            var batch = documents.Skip(skip).Take(take).Select(document => new IndexDocumentsAction<AzureSearchDocument>(
                IndexActionType.Upload, document)
            ).ToList();
            if (batch.Count == 0) break;
            
            var batchRequest = IndexDocumentsBatch.Create([..batch]);
            await searchClient.IndexDocumentsAsync(batchRequest);
            skip += take;
        }
        
        logger.LogInformation("Added {documentsCount} to index {IndexName}.", documents.Count, indexName);
    }

    public async Task<bool> IndexExistsAsync(string indexName)
    {
        try
        {
            var index = await indexClient.GetIndexAsync(indexName);
            return index.HasValue;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task CreateIndexIfNotExistsAsync(string indexName)
    {
        try
        {
            await indexClient.GetIndexAsync(indexName);
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var newIndex = DefaultIndex.Create(indexName);
            var resp = await indexClient.CreateIndexAsync(newIndex);
            if(!resp.HasValue)
                throw new Exception($"Error creating new index name: {newIndex} response: {resp.GetRawResponse().ReasonPhrase}");
        }
    }

    public async Task SyncIndexAsync(string indexName)
    {
        var index = await indexClient.GetIndexAsync(indexName);
        if (!index.HasValue) return;

        var fields = DefaultIndex.Create(indexName).Fields;
        index.Value.Fields = fields;
        await indexClient.CreateOrUpdateIndexAsync(index.Value);
    }

    public async Task DeleteIndexAsync(string indexName)
    {
        await indexClient.DeleteIndexAsync(indexName);
    }
    #endregion

    #region IndexReport
    public async Task<SearchIndexStatistics> GetIndexStatisticsByNameAsync(string indexName)
    {
        try
        {
            // Get index details
            return await indexClient.GetIndexStatisticsAsync(indexName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get index statistics for {{IndexName}}: {indexName} error: {ex.Message}");
        }
    }
    #endregion

}