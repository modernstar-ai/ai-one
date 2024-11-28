using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.Common.Attributes;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.AzureAiSearch;

[Export(typeof(IAzureAiSearch), ServiceLifetime.Singleton)]
public class AzureAiSearch(SearchIndexerClient indexerClient, SearchIndexClient indexClient, ILogger<AzureAiSearch> logger) : IAzureAiSearch
{
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
}