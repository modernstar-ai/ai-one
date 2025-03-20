using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.AiSearchConstants;

public static class DefaultIndex
{
    public static SearchIndex Create(string indexName)
    {
        var newIndex = new SearchIndex(indexName);
            newIndex.Fields = new List<SearchField>()
            {
                new(nameof(AzureSearchDocument.Id), SearchFieldDataType.String)
                {
                    IsKey = true,
                    IsSearchable = true,
                    IsFilterable = true,
                    IsSortable = true,
                    IsFacetable = true,
                    AnalyzerName = "keyword"
                },
                new(nameof(AzureSearchDocument.FileId), SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = true,
                    IsSortable = true,
                    IsFacetable = true
                },
                new(nameof(AzureSearchDocument.Chunk), SearchFieldDataType.String)
                {
                    IsSearchable = true,
                },
                new(nameof(AzureSearchDocument.Name), SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = true
                },
                new(nameof(AzureSearchDocument.Tags), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsSearchable = true,
                    IsFilterable = true,
                    IsSortable = false,
                    IsFacetable = true
                },
                new(nameof(AzureSearchDocument.ChunkVector), SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    IsHidden = true,
                    IsStored = true,
                    VectorSearchDimensions = 1536,
                    VectorSearchProfileName = "azureOpenAi-text-profile"
                },
                new(nameof(AzureSearchDocument.NameVector), SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    IsHidden = true,
                    IsStored = true,
                    VectorSearchDimensions = 1536,
                    VectorSearchProfileName = "azureOpenAi-text-profile"
                },
                new(nameof(AzureSearchDocument.Url), SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = true
                },
            };
            
            newIndex.SemanticSearch = new SemanticSearch();
            newIndex.SemanticSearch.Configurations.Add(new SemanticConfiguration("semantic-configuration", new SemanticPrioritizedFields()
            {
                TitleField = new SemanticField(nameof(AzureSearchDocument.Name)),
                ContentFields = { new SemanticField(nameof(AzureSearchDocument.Chunk)) }
            }));
            newIndex.SemanticSearch.DefaultConfigurationName = "semantic-configuration";
            
            newIndex.VectorSearch = new VectorSearch();
            newIndex.VectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration("algorithm")
            {
                Parameters = new HnswParameters()
                {
                    Metric = VectorSearchAlgorithmMetric.Cosine,
                    M = 4,
                    EfConstruction = 400,
                    EfSearch = 500
                }
            });
            newIndex.VectorSearch.Vectorizers.Add(new AzureOpenAIVectorizer("azureOpenAi-text-vectorizer")
            {
                Parameters = new AzureOpenAIVectorizerParameters()
                {
                    ResourceUri = new Uri(Configs.AzureOpenAi.Endpoint),
                    DeploymentName = Configs.AzureOpenAi.EmbeddingsDeploymentName,
                    ApiKey = Configs.AzureOpenAi.ApiKey,
                    ModelName = Configs.AzureOpenAi.EmbeddingsModelName
                }
            });
            newIndex.VectorSearch.Profiles.Add(new VectorSearchProfile("azureOpenAi-text-profile", "algorithm")
            {
                VectorizerName = "azureOpenAi-text-vectorizer"
            });

            return newIndex;
    }
}