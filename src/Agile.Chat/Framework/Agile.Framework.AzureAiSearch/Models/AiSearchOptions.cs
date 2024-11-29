using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace Agile.Framework.AzureAiSearch.Models;

public class AiSearchOptions(string userPrompt, ReadOnlyMemory<float> vector)
{
    public int DocumentLimit { get; init; } = 6;
    public double? Strictness { get; init; }
    public SearchOptions ParseSearchOptions()
    {
        VectorSearchOptions vectorSearchOptions = new VectorSearchOptions();
        vectorSearchOptions.FilterMode = VectorFilterMode.PreFilter;
        var vectorizedQuery = new VectorizedQuery(vector)
        {
            Fields = { SearchConstants.VectorFieldName },
        };
        if (Strictness is not null)
            vectorizedQuery.Threshold = new VectorSimilarityThreshold(Strictness.Value);
        
        vectorSearchOptions.Queries.Add(vectorizedQuery);

        var semanticSearch = new SemanticSearchOptions();
        semanticSearch.SemanticConfigurationName = SearchConstants.SemanticConfigurationName;
        semanticSearch.SemanticQuery = userPrompt;
        
        // Configure search options with semantic ranking
        var searchOptions = new SearchOptions();
        searchOptions.QueryType = SearchQueryType.Semantic;
        searchOptions.SemanticSearch = semanticSearch;
        searchOptions.VectorSearch = vectorSearchOptions;
        searchOptions.Size = DocumentLimit;
        
        return searchOptions;
    }
}