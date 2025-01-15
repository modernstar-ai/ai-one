using System.Reflection;
using System.Text.Json.Serialization;
using Agile.Framework.AzureAiSearch.AiSearchConstants;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace Agile.Framework.AzureAiSearch.Models;

public class AiSearchOptions(string userPrompt, ReadOnlyMemory<float> vector)
{
    public int DocumentLimit { get; init; } = 6;
    public double? Strictness { get; init; }
    public List<string> FolderFilters { get; init; } = new();
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
        searchOptions.Filter = BuildODataFolderFilters();
        
        return searchOptions;
    }

    private string BuildODataFolderFilters()
    {
        if (FolderFilters.Count == 0)
            return string.Empty;
        
        var cleanFilters = FolderFilters
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(filter =>
        {
            filter = filter.Trim();
            
            if (filter.StartsWith("/"))
                filter = filter.TrimStart('/');
            if (!filter.EndsWith('/'))
                filter += '/';

            return filter;
        });

        var property = typeof(AzureSearchDocument).GetProperty(nameof(AzureSearchDocument.Url))!
            .GetCustomAttribute<JsonPropertyNameAttribute>()!
            .Name;
        return string.Join(" or ", cleanFilters.Select(folder => $"search.ismatch('\"/{Constants.BlobContainerName}/{folder}\"~', '{property}')"));
    }
}