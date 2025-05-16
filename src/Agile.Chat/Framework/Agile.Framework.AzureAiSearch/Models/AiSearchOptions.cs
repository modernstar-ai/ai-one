using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;

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
            Fields = {
                nameof(AzureSearchDocument.ChunkVector),
                nameof(AzureSearchDocument.NameVector)
            },
        };

        var strictness = Strictness.Value switch
        {
            0 => 0,
            1 => 0.2,
            2 => 0.4,
            3 => 0.6,
            4 => 0.8,
            5 => 0.9,
            _ => 0
        };

        if (Strictness is not null)
            vectorizedQuery.Threshold = new VectorSimilarityThreshold(strictness);

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