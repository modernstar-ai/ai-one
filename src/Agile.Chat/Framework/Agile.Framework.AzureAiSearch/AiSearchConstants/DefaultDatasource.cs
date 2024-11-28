using Agile.Framework.Common.EnvironmentVariables;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.AiSearchConstants;

public static class DefaultDatasource
{
    public static SearchIndexerDataSourceConnection Create(string indexName)
    {
        var datasourceName = SearchConstants.DatasourceName(indexName);
        
        var containerSettings = new SearchIndexerDataContainer(Constants.BlobContainerName);
        containerSettings.Query = indexName;

        var newDataSource = new SearchIndexerDataSourceConnection(datasourceName, 
            SearchIndexerDataSourceType.AzureBlob,
            Configs.BlobStorageConnectionString, 
            containerSettings)
        {
            DataDeletionDetectionPolicy = new NativeBlobSoftDeleteDeletionDetectionPolicy()
        };

        return newDataSource;
    }
}