using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.Attributes;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Search.Documents.Models;

namespace Agile.Framework.AzureAiSearch;

[Export(typeof(IAzureAiSearch), ServiceLifetime.Singleton)]
public class AzureAiSearch(SearchIndexerClient indexerClient, SearchIndexClient indexClient, ILogger<AzureAiSearch> logger) : IAzureAiSearch
{
    public async Task<List<AzureSearchDocument>> SearchAsync(string indexName, AiSearchOptions aiSearchOptions)
    {
        var searchClient = indexClient.GetSearchClient(indexName);

        var searchResults = await searchClient.SearchAsync<AzureSearchDocument>(aiSearchOptions.ParseSearchOptions());

        if (!searchResults.HasValue) return [];

        var results = searchResults.Value.GetResultsAsync();
        return results
            .ToBlockingEnumerable()
            .Select(x => x.Document)
            .ToList();
    }

    public async Task<AzureSearchDocument?> GetChunkByIdAsync(string indexName, string chunkId)
    {
        var searchClient = indexClient.GetSearchClient(indexName);
        var searchResults = await searchClient.GetDocumentAsync<AzureSearchDocument>(chunkId);
        return !searchResults.HasValue ? null : searchResults.Value;
    }

    public async Task DeleteFileContentsByIdAsync(string fileId, string indexName)
    {
        var searchClient = indexClient.GetSearchClient(indexName);
        var idProperty = typeof(AzureSearchDocument).GetProperty(nameof(AzureSearchDocument.Id))!
            .GetCustomAttribute<JsonPropertyNameAttribute>()!
            .Name;
        var property = typeof(AzureSearchDocument).GetProperty(nameof(AzureSearchDocument.FileId))!
            .GetCustomAttribute<JsonPropertyNameAttribute>()!
            .Name;

        var documentIds = new List<string>();
        while (documentIds.Count > 0)
        {
            int batchSize = 1000; // Adjust batch size based on your needs
            string filter = $"{property} eq '{fileId}'";

            var options = new SearchOptions
            {
                Filter = filter,
                Select = { idProperty },
                Size = batchSize
            };

            var searchResults = await searchClient.SearchAsync<AzureSearchDocument>(options);

            documentIds = searchResults.Value.GetResults().Select(x => x.Document.Id).ToList();
            if (documentIds.Count > 0)
            {
                var batch = documentIds.Select(id => new IndexDocumentsAction<AzureSearchDocument>(
                    IndexActionType.Delete, new AzureSearchDocument { Id = id })
                ).ToList();

                var batchRequest = IndexDocumentsBatch.Delete(batch);
                await searchClient.IndexDocumentsAsync(batchRequest);

                logger.LogInformation($"Deleted {documentIds.Count} documents where {property} = '{fileId}'.");
            }
        }

    }
    
    #region Indexes

    public async Task IndexDocumentsAsync(List<AzureSearchDocument> documents, string indexName)
    {
        var searchClient = indexClient.GetSearchClient(indexName);

        var batch = documents.Select(document => new IndexDocumentsAction<AzureSearchDocument>(
            IndexActionType.Upload, document)
        ).ToList();

        var batchRequest = IndexDocumentsBatch.Create([..batch]);
        await searchClient.IndexDocumentsAsync(batchRequest);

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