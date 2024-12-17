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
    public async Task<IndexReport> GetIndexReportAsync(string indexName)
    {
        try
        {
            // Get index details
            var index = await indexClient.GetIndexAsync(indexName);
            var stats = await indexClient.GetIndexStatisticsAsync(indexName);

            // Get indexers and data sources
            var indexers = await indexerClient.GetIndexersAsync();
            var relevantIndexers = indexers.Value.Where(i => i.TargetIndexName == indexName).ToList();

            var report = new IndexReport
            {
                Name = indexName,
                DocumentCount = stats.Value.DocumentCount,
                StorageSize = FormatSize(stats.Value.StorageSize),
                VectorIndexSize = index.Value.VectorSearch != null ? FormatSize((long)(stats.Value.StorageSize * 0.2)) : "N/A",
                ReplicasCount = 1,
                LastRefreshTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = "Active",
                Indexers = new List<IndexerDetail>(),
                DataSources = new List<DataSourceDetail>()
            };

            // Get indexer details
            foreach (var indexer in relevantIndexers)
            {
                var indexerStatus = await indexerClient.GetIndexerStatusAsync(indexer.Name);

                report.Indexers.Add(new IndexerDetail
                {
                    Name = indexer.Name,
                    TargetIndex = indexer.TargetIndexName,
                    DataSource = indexer.DataSourceName,
                    Schedule = indexer.Schedule?.Interval.ToString() ?? "Manual",
                    LastRunTime = indexerStatus.Value.LastResult.EndTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                    NextRunTime = CalculateNextRunTime(indexerStatus.Value.LastResult.EndTime, indexer.Schedule?.Interval),
                    DocumentsProcessed = $"{indexerStatus.Value.LastResult?.ItemCount ?? 0} / {indexerStatus.Value.LastResult?.ItemCount ?? 0}",
                    Status = indexerStatus.Value.Status.ToString()
                });

                // Get data source details if not already added
                if (!report.DataSources.Any(ds => ds.Name == indexer.DataSourceName))
                {
                    var dataSource = await indexerClient.GetDataSourceConnectionAsync(indexer.DataSourceName);

                    report.DataSources.Add(new DataSourceDetail
                    {
                        Name = dataSource.Value.Name,
                        Type = dataSource.Value.Type.ToString(),
                        Container = dataSource.Value.Container?.Name ?? "N/A",
                        ConnectionStatus = indexerStatus.Value.Status.ToString()
                    });
                }
            }

            return report;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get index report for {{IndexName}}: {indexName} error: {ex.Message}");
        }
    }

    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }

    private string? CalculateNextRunTime(DateTimeOffset? lastRun, TimeSpan? interval)
    {
        if (lastRun == null || interval == null) return null;
        return lastRun.Value.Add(interval.Value).ToString("yyyy-MM-dd HH:mm:ss");
    }

    #endregion

}