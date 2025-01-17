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

    public async Task<string?> GetChunkByIdAsync(string indexName, string chunkId)
    {
        var searchClient = indexClient.GetSearchClient(indexName);
        var searchResults = await searchClient.GetDocumentAsync<AzureSearchDocument>(chunkId);
        return !searchResults.HasValue ? null : searchResults.Value.Chunk;
    }
    
    #region Indexers
    public async Task DeleteIndexerAsync(string indexName)
    {
        var indexerName = SearchConstants.IndexerName(indexName);
        var datasourceName = SearchConstants.DatasourceName(indexName);
        var skillsetName = SearchConstants.SkillsetName(indexName);
        
        var respSkillset = await indexerClient.DeleteSkillsetAsync(skillsetName);
        var respDatasource = await indexerClient.DeleteDataSourceConnectionAsync(datasourceName);
        var respIndexer = await indexerClient.DeleteIndexerAsync(indexerName);
        var respIndex = await indexClient.DeleteIndexAsync(indexName);
        
        if (respSkillset.IsError || respDatasource.IsError || respIndexer.IsError || respIndex.IsError)
            logger.LogCritical($"Ran into error deleting. skillset resp: {respSkillset.ReasonPhrase} datasource resp: {respDatasource.ReasonPhrase} Indexer resp: {respIndexer.ReasonPhrase} index resp: {respIndex.ReasonPhrase}");
    }

    public async Task<bool> IndexerExistsAsync(string indexName)
    {
        try
        {
            var indexResp = await indexClient.GetIndexAsync(indexName);
            return indexResp.Value is not null;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return false;
        }
    }
    
    public async Task CreateIndexerAsync(string indexName)
    {
        var dataSource = await GetOrCreateDefaultDataSourceAsync(indexName);
        var index = await GetOrCreateDefaultIndexAsync(indexName);
        var skillset = await GetOrCreateDefaultSkillsetAsync(indexName);
        
        var indexer = DefaultIndexer.Create(index.Name, dataSource.Name, skillset.Name);
        var indexerResp = await indexerClient.CreateIndexerAsync(indexer);

        if(!indexerResp.HasValue)
            throw new Exception($"Failed to create indexer name: {indexer.Name} index name: {index.Name} response: {indexerResp.GetRawResponse().ReasonPhrase}");
    }

    public async Task RunIndexerAsync(string indexName)
    {
        var indexerName = SearchConstants.IndexerName(indexName);
        await indexerClient.RunIndexerAsync(indexerName);
    }
    #endregion
    
    #region Indexes
    private async Task<SearchIndex> GetOrCreateDefaultIndexAsync(string indexName)
    {
        SearchIndex? index = null;
        try
        {
            var indexResp = await indexClient.GetIndexAsync(indexName);
            index = indexResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var newIndex = DefaultIndex.Create(indexName);
            var resp = await indexClient.CreateIndexAsync(newIndex);
            index = resp.HasValue ? resp.Value : throw new Exception($"Error creating new index name: {newIndex} response: {resp.GetRawResponse().ReasonPhrase}");
        }

        return index;
    }
    #endregion
    
    #region DataSources
    private async Task<SearchIndexerDataSourceConnection> GetOrCreateDefaultDataSourceAsync(string indexName)
    {
        var datasourceName = SearchConstants.DatasourceName(indexName);
        SearchIndexerDataSourceConnection dataSource = null!;
        SearchIndexer? searchIndexer = null;
        try
        {
            var datasourceResp = await indexerClient.GetDataSourceConnectionAsync(datasourceName);
            dataSource = datasourceResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var newDatasource = DefaultDatasource.Create(indexName);
            var dataSourceCreateResp = await indexerClient.CreateDataSourceConnectionAsync(newDatasource);
            return dataSourceCreateResp.HasValue ? dataSourceCreateResp.Value : 
                throw new Exception($"Problem creating data source connection name: {datasourceName} response: {dataSourceCreateResp.GetRawResponse().ReasonPhrase}");
        }

        return dataSource;
    }
    #endregion
    
    #region Skillsets
    private async Task<SearchIndexerSkillset> GetOrCreateDefaultSkillsetAsync(string indexName)
    {
        SearchIndexerSkillset? skillset = null;
        try
        {
            var skillsetName = SearchConstants.SkillsetName(indexName);
            var skillsetResp = await indexerClient.GetSkillsetAsync(skillsetName);
            skillset = skillsetResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var newSkillset = DefaultSkillset.Create(indexName);
            var skillsetCreateResp = await indexerClient.CreateSkillsetAsync(newSkillset);
            skillset = skillsetCreateResp.HasValue ? skillsetCreateResp.Value : 
                throw new Exception($"Ran into error creating skillset name: {newSkillset.Name} Response: {skillsetCreateResp.GetRawResponse().ReasonPhrase}");
        }

        return skillset;
    }
    #endregion

    #region IndexReport
    public async Task<SearchIndexStatistics> GetIndexStatisticsByNameAsync(string indexName)
    {
        try
        {
            // Get index details
            var stats = await indexClient.GetIndexStatisticsAsync(indexName);
             
            return stats;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get index statistics for {{IndexName}}: {indexName} error: {ex.Message}");
        }
    }

    public async Task<IndexerDetail?> GetIndexersByIndexNameAsync(string indexName)
    {
        try
        {
            var indexer = await indexerClient.GetIndexerAsync(SearchConstants.IndexerName(indexName));

            if (indexer.HasValue)
            {
                var indexerStatus = await indexerClient.GetIndexerStatusAsync(indexer.Value.Name);

                return new IndexerDetail
                {
                    Name = indexer.Value.Name,
                    TargetIndex = indexer.Value.TargetIndexName,
                    DataSource = indexer.Value.DataSourceName,
                    Schedule = indexer.Value.Schedule?.Interval.ToString() ?? "Manual",
                    LastRunTime = indexerStatus.Value.LastResult.EndTime?.DateTime,
                    DocumentsProcessed = indexerStatus.Value.LastResult?.ItemCount,
                    Status = indexerStatus.Value.LastResult?.Status.ToString()
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get index details for {{IndexName}}: {indexName} error: {ex.Message}");
        }
    }


    public async Task<DataSourceDetail?> GetDataSourceByNameAsync(string indexName)
    {
        try
        {
            var datasource = await indexerClient.GetDataSourceConnectionAsync(SearchConstants.DatasourceName(indexName));

            if (datasource.HasValue)
            {
                return new DataSourceDetail
                {
                    Name = datasource.Value.Name,
                    Type = datasource.Value.Type.ToString(),
                    Container = datasource.Value.Container?.Name ?? "N/A",
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get index data source for {{IndexName}}: {indexName} error: {ex.Message}");
        }
    }

    

    #endregion

}