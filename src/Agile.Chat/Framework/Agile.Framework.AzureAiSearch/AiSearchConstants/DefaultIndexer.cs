using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.AiSearchConstants;

public static class DefaultIndexer
{
    public static SearchIndexer Create(string indexName, string datasourceName, string skillsetName)
    {
        var indexerName = SearchConstants.IndexerName(indexName);
        
        var searchIndexer = new SearchIndexer(indexerName, datasourceName, indexName);
        searchIndexer.SkillsetName = skillsetName;
        
        searchIndexer.Parameters = new IndexingParameters();
        searchIndexer.Parameters.Configuration.Add("imageAction", "generateNormalizedImages");
        searchIndexer.Parameters.Configuration.Add("dataToExtract", "contentAndMetadata");
        searchIndexer.Parameters.Configuration.Add("parsingMode", "default");
        searchIndexer.Parameters.Configuration.Add("allowSkillsetToReadFileData", true);
        
        searchIndexer.FieldMappings.Add(new FieldMapping("metadata_storage_name")
        {
            SourceFieldName = "metadata_storage_name",
            TargetFieldName = "title",
            MappingFunction = null
        });

        return searchIndexer;
    }
}