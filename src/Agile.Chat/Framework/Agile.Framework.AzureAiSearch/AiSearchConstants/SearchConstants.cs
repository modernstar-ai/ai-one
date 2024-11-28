using Agile.Framework.Common.EnvironmentVariables;

namespace Agile.Framework.AzureAiSearch.AiSearchConstants;

public static class SearchConstants
{
    public static string IndexerName(string indexName) => indexName + "-indexer";
    public static string SkillsetName(string indexName) => Constants.BlobContainerName + $"-{indexName}-skillset";
    public static string DatasourceName(string indexName) => Constants.BlobContainerName + $"-{indexName}-datasource";
}