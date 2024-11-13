using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace agile_chat_api.Services;

public interface IAzureAiSearchService
{
    Task<bool> RunIndexer(string indexName);
    Task<List<string>> GetAllIndexes();
    Task<string?> CreateIndex(string indexName);
}

public class AzureAiSearchService : IAzureAiSearchService
{
    public static readonly string FOLDERS_INDEX_NAME =
        Environment.GetEnvironmentVariable("AZURE_STORAGE_FOLDERS_CONTAINER_NAME")! + "-indexer";

    private readonly Uri _uri;
    private readonly AzureKeyCredential _credentials;
    private readonly ILogger<AzureAiSearchService> _logger;

    public AzureAiSearchService(ILogger<AzureAiSearchService> logger)
    {
        _logger = logger;
        _uri = new(Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")!);
        _credentials = new(Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY")!);
    }

    public async Task<List<string>> GetAllIndexes()
    {
        var indexList = new List<string>();
        
        var indexClient = new SearchIndexClient(_uri, _credentials);
        var indexes = indexClient.GetIndexNamesAsync();

        await foreach (var indexName in indexes)
        {
            indexList.Add(indexName);
        }

        return indexList;
    }
    
    public async Task<string?> CreateIndex(string indexName)
    {
        var indexClient = new SearchIndexClient(_uri, _credentials);
        var index = await indexClient.GetIndexAsync(indexName);

        if (index.HasValue)
            return "Index already exists";

        var newIndex = new SearchIndex(indexName);
        newIndex.Fields = CreateDefaultFields();
        CreateDefaultSemanticSearchConfigs(newIndex);
        CreateDefaultVectorConfigs(newIndex);
        
        var resp = await indexClient.CreateIndexAsync(newIndex);

        return resp.HasValue ? null : "Error creating index";
    }

    private List<SearchField> CreateDefaultFields()
    {
        var fields = new List<SearchField>()
        {
            new("chunk_id", SearchFieldDataType.String)
            {
                IsKey = true,
                IsSearchable = true,
                IsFilterable = true,
                IsSortable = true,
                IsFacetable = true,
                AnalyzerName = "keyword"
            },
            new("parent_id", SearchFieldDataType.String)
            {
                IsSearchable = true,
                IsFilterable = true,
                IsSortable = true,
                IsFacetable = true
            },
            new("chunk", SearchFieldDataType.String)
            {
                IsSearchable = true,
            },
            new("title", SearchFieldDataType.String)
            {
                IsSearchable = true,
                IsFilterable = true
            },
            new("text_vector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                IsSearchable = true,
                IsHidden = true,
                IsStored = true,
                VectorSearchDimensions = 1536,
                VectorSearchProfileName = "folders-azureOpenAi-text-profile"
            },
            new("metadata_storage_name", SearchFieldDataType.String),
            new("metadata_storage_path", SearchFieldDataType.String)
            {
                IsSearchable = true,
                IsFilterable = true
            },
        };
        
        
        return fields;
    }

    private void CreateDefaultSemanticSearchConfigs(SearchIndex index)
    {
        index.SemanticSearch = new SemanticSearch();
        index.SemanticSearch.Configurations.Add(new SemanticConfiguration("folders-semantic-configuration", new SemanticPrioritizedFields()
        {
            TitleField = new SemanticField("title"),
            ContentFields = { new SemanticField("chunk") }
        }));
        index.SemanticSearch.DefaultConfigurationName = "folders-semantic-configuration";
    }
    
    private void CreateDefaultVectorConfigs(SearchIndex index)
    {
        index.VectorSearch = new VectorSearch();
        index.VectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration("folders-algorithm")
        {
            Parameters = new HnswParameters()
            {
                Metric = VectorSearchAlgorithmMetric.Cosine,
                M = 4,
                EfConstruction = 400,
                EfSearch = 500
            }
        });
        index.VectorSearch.Vectorizers.Add(new AzureOpenAIVectorizer("folders-azureOpenAi-text-vectorizer")
        {
            Parameters = new AzureOpenAIVectorizerParameters()
            {
                ResourceUri = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!),
                DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME")!,
                ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!,
                ModelName = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_DEPLOYMENT_NAME")!,
            }
        });
        index.VectorSearch.Profiles.Add(new VectorSearchProfile("folders-azureOpenAi-text-profile", "folders-algorithm")
        {
            VectorizerName = "folders-azureOpenAi-text-vectorizer"
        });
    }

    public async Task<bool> RunIndexer(string indexName)
    {
        var searchIndexerClient = new SearchIndexerClient(_uri, _credentials);
        _logger.LogDebug("Fetched search indexer client");
        var resp = await searchIndexerClient.RunIndexerAsync(indexName);
        _logger.LogDebug("Ran indexer {IndexName} with status: {Status} reason phrase: {ReasonPhrase}", indexName, resp?.Status, resp?.ReasonPhrase);
        return resp is { IsError: false };
    }
}