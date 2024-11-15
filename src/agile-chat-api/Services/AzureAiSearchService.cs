using System.Text.Json.Nodes;
using agile_chat_api.Configurations;
using agile_chat_api.Utils;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace agile_chat_api.Services;

public interface IAzureAiSearchService
{
    Task<bool> RunIndexerAsync(string indexName);
    Task DeleteIndexAsync(string indexName);
    Task<SearchIndexer> CreateDefaultIndexerAsync(string indexName);
    Task<List<string>> GetAllIndexes();
}

public class AzureAiSearchService : IAzureAiSearchService
{
    public static string GetIndexerByIndexName(string indexName) => indexName + "-indexer";
    public static string GetSkillsetNameByIndexName(string indexName) => Constants.BlobStorageContainerName + $"-{indexName}-skillset";
    public static string GetDatasourceNameByIndexName(string indexName) => Constants.BlobStorageContainerName + $"-{indexName}-datasource";

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
    
    private async Task<SearchIndex> GetOrCreateDefaultIndexAsync(string indexName)
    {
        var indexClient = new SearchIndexClient(_uri, _credentials);
        
        SearchIndex? index = null;
        try
        {
            var indexResp = await indexClient.GetIndexAsync(indexName);
            index = indexResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var newIndex = new SearchIndex(indexName);
            newIndex.Fields = new List<SearchField>()
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
                    VectorSearchProfileName = "azureOpenAi-text-profile"
                },
                new("metadata_storage_name", SearchFieldDataType.String),
                new("metadata_storage_path", SearchFieldDataType.String)
                {
                    IsSearchable = true,
                    IsFilterable = true
                },
            };
            
            newIndex.SemanticSearch = new SemanticSearch();
            newIndex.SemanticSearch.Configurations.Add(new SemanticConfiguration("semantic-configuration", new SemanticPrioritizedFields()
            {
                TitleField = new SemanticField("title"),
                ContentFields = { new SemanticField("chunk") }
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
                    ResourceUri = new Uri(AppConfigs.AzureOpenAiEndpoint),
                    DeploymentName = AppConfigs.AzureOpenAiEmbeddingsDeploymentName,
                    ApiKey = AppConfigs.AzureOpenAiApiKey,
                    ModelName = AppConfigs.AzureOpenAiEmbeddingsModelName
                }
            });
            newIndex.VectorSearch.Profiles.Add(new VectorSearchProfile("azureOpenAi-text-profile", "algorithm")
            {
                VectorizerName = "azureOpenAi-text-vectorizer"
            });
            
            var resp = await indexClient.CreateIndexAsync(newIndex);

            index = resp.HasValue ? resp.Value : throw new Exception($"Error creating new index name: {newIndex} response: {resp.GetRawResponse().ReasonPhrase}");
        }

        return index;
    }
    
    public async Task<bool> RunIndexerAsync(string indexName)
    {
        var searchIndexerClient = new SearchIndexerClient(_uri, _credentials);
        _logger.LogDebug("Fetched search indexer client");
        var indexerName = GetIndexerByIndexName(indexName);
        
        SearchIndexer? searchIndexer = null;
        try
        {
            var indexerResponse = await searchIndexerClient.GetIndexerAsync(indexerName);
            searchIndexer = indexerResponse.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            searchIndexer = await CreateDefaultIndexerAsync(indexName);
        }

        var resp = await searchIndexerClient.RunIndexerAsync(searchIndexer.Name);
        _logger.LogDebug("Ran indexer {IndexName} with status: {Status} reason phrase: {ReasonPhrase}", indexName, resp?.Status, resp?.ReasonPhrase);
        return resp is { IsError: false };
    }

    public async Task<SearchIndexer> CreateDefaultIndexerAsync(string indexName)
    {
        var indexerName = GetIndexerByIndexName(indexName);
        var searchIndexerClient = new SearchIndexerClient(_uri, _credentials);

        var dataSource = await GetOrCreateDefaultDataSourceAsync(indexName);
        var index = await GetOrCreateDefaultIndexAsync(indexName);
        var skillset = await GetOrCreateDefaultSkillsetAsync(indexName);

        var searchIndexer = new SearchIndexer(indexerName, dataSource.Name, index.Name);
        searchIndexer.SkillsetName = skillset.Name;
        
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

        var indexerResp = await searchIndexerClient.CreateIndexerAsync(searchIndexer);

        return indexerResp.HasValue
            ? indexerResp.Value
            : throw new Exception(
                $"Failed to create indexer name: {indexerName} index name: {indexName} response: {indexerResp.GetRawResponse().ReasonPhrase}");
    }

    private async Task<SearchIndexerDataSourceConnection> GetOrCreateDefaultDataSourceAsync(string indexName)
    {
        var searchIndexerClient = new SearchIndexerClient(_uri, _credentials);
        var datasourceName = GetDatasourceNameByIndexName(indexName);
        SearchIndexerDataSourceConnection dataSource = null!;
        SearchIndexer? searchIndexer = null;
        try
        {
            var datasourceResp = await searchIndexerClient.GetDataSourceConnectionAsync(datasourceName);
            dataSource = datasourceResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            var containerSettings = new SearchIndexerDataContainer(Constants.BlobStorageContainerName);
            containerSettings.Query = indexName;

            var newDataSource = new SearchIndexerDataSourceConnection(datasourceName, 
                SearchIndexerDataSourceType.AzureBlob,
                AppConfigs.BlobStorageConnectionString, 
                containerSettings)
            {
                DataDeletionDetectionPolicy = new NativeBlobSoftDeleteDeletionDetectionPolicy()
            };

            var dataSourceCreateResp = await searchIndexerClient.CreateDataSourceConnectionAsync(newDataSource);
        
            dataSource = dataSourceCreateResp.HasValue ? dataSourceCreateResp.Value : 
                throw new Exception($"Problem creating data source connection name: {datasourceName} response: {dataSourceCreateResp.GetRawResponse().ReasonPhrase}");
        }

        return dataSource;
    }

    private async Task<SearchIndexerSkillset> GetOrCreateDefaultSkillsetAsync(string indexName)
    {
        var searchIndexerClient = new SearchIndexerClient(_uri, _credentials);
        var skillsetName = GetSkillsetNameByIndexName(indexName);
        
        SearchIndexerSkillset? skillset = null;
        try
        {
            var skillsetResp = await searchIndexerClient.GetSkillsetAsync(skillsetName);
            skillset = skillsetResp.Value;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            //OCR skill for pdf images
            var ocrSkill = new OcrSkill(new[]
            {
                new InputFieldMappingEntry("image")
                {
                    Name = "image",
                    Source = "/document/normalized_images/*",
                    Inputs = { }
                }
            }, new[]
            {
                new OutputFieldMappingEntry("text")
                {
                    TargetName = "text"
                }
            });
            ocrSkill.Name = "#1";
            ocrSkill.Context = "/document/normalized_images/*";
            ocrSkill.DefaultLanguageCode = "en";
            ocrSkill.ShouldDetectOrientation = true;
            ocrSkill.LineEnding = "Space";
            
            //Merge skill for merging the image texts with content texts
            var mergeSkill = new MergeSkill(new[]
            {
                new InputFieldMappingEntry("text")
                {
                    Name = "text",
                    Source = "/document/content",
                    Inputs = { }
                },
                new InputFieldMappingEntry("itemsToInsert")
                {
                    Name = "itemsToInsert",
                    Source = "/document/normalized_images/*/text",
                    Inputs = { }
                },
                new InputFieldMappingEntry("offsets")
                {
                    Name = "offsets",
                    Source = "/document/normalized_images/*/contentOffset",
                    Inputs = { }
                }
            }, new[]
            {
                new OutputFieldMappingEntry("mergedText")
                {
                    Name = "mergedText",
                    TargetName = "mergedText"
                }
            });
            mergeSkill.Name = "#2";
            mergeSkill.Context = "/document";
            mergeSkill.InsertPreTag = " ";
            mergeSkill.InsertPostTag = " ";
            
            //Split skill for splitting up chunks of documents to embed
            var splitSkill = new SplitSkill(new[]
            {
                new InputFieldMappingEntry("text")
                {
                    Name = "text",
                    Source = "/document/mergedText",
                    Inputs = { }
                }
            }, new[]
            {
                new OutputFieldMappingEntry("textItems")
                {
                    Name = "textItems",
                    TargetName = "pages"
                }
            });
            splitSkill.Name = "#3";
            splitSkill.Description = "Split skill to chunk documents";
            splitSkill.Context = "/document";
            splitSkill.DefaultLanguageCode = "en";
            splitSkill.TextSplitMode = TextSplitMode.Pages;
            splitSkill.MaximumPageLength = 2000;
            splitSkill.PageOverlapLength = 500;
            splitSkill.Unit = "characters";
            
            //Embedding skill to embed each chunk of text
            var embeddingSkill = new AzureOpenAIEmbeddingSkill(new[]
            {
                new InputFieldMappingEntry("text")
                {
                    Name = "text",
                    Source = "/document/pages/*",
                    Inputs = { }
                }
            }, new[]
            {
                new OutputFieldMappingEntry("embedding")
                {
                    Name = "embedding",
                    TargetName = "text_vector"
                }
            });
            embeddingSkill.Name = "#4";
            embeddingSkill.Context = "/document/pages/*";
            embeddingSkill.ResourceUri = new Uri(AppConfigs.AzureOpenAiEndpoint);
            embeddingSkill.DeploymentName = AppConfigs.AzureOpenAiEmbeddingsDeploymentName;
            embeddingSkill.ModelName = AppConfigs.AzureOpenAiEmbeddingsModelName;
            embeddingSkill.ApiKey = AppConfigs.AzureOpenAiApiKey;
            
            var skillsets = new List<SearchIndexerSkill>()
            {
                ocrSkill,
                mergeSkill,
                splitSkill,
                embeddingSkill
            };

            var newSkillset = new SearchIndexerSkillset(skillsetName, skillsets);
            newSkillset.Description = "Skillset to chunk documents and generate embeddings";
            newSkillset.CognitiveServicesAccount = new CognitiveServicesAccountKey(AppConfigs.AzureAiServicesKey);
            newSkillset.IndexProjection = new SearchIndexerIndexProjection(new[]
            {
                new SearchIndexerIndexProjectionSelector(indexName, "parent_id", "/document/pages/*", new[]
                {
                    new InputFieldMappingEntry("text_vector")
                    {
                        Source = "/document/pages/*/text_vector"
                    },
                    new InputFieldMappingEntry("chunk")
                    {
                        Source = "/document/pages/*"
                    },
                    new InputFieldMappingEntry("title")
                    {
                        Source = "/document/title"
                    },
                    new InputFieldMappingEntry("metadata_storage_name")
                    {
                        Source = "/document/metadata_storage_name"
                    },
                    new InputFieldMappingEntry("metadata_storage_path")
                    {
                        Source = "/document/metadata_storage_path"
                    }
                })
            });
            newSkillset.IndexProjection.Parameters = new SearchIndexerIndexProjectionsParameters
            {
                ProjectionMode = "skipIndexingParentDocuments"
            };

            var skillsetCreateResp = await searchIndexerClient.CreateSkillsetAsync(newSkillset);
            skillset = skillsetCreateResp.HasValue ? skillsetCreateResp.Value : 
                throw new Exception($"Ran into error creating skillset name: {skillsetName} Response: {skillsetCreateResp.GetRawResponse().ReasonPhrase}");
        }

        return skillset;
    }

    public async Task DeleteIndexAsync(string indexName)
    {
        var indexerName = GetIndexerByIndexName(indexName);
        var datasourceName = GetDatasourceNameByIndexName(indexName);
        var skillsetName = GetSkillsetNameByIndexName(indexName);

        var client = new SearchIndexerClient(_uri, _credentials);
        var indexClient = new SearchIndexClient(_uri, _credentials);

        var respSkillset = await client.DeleteSkillsetAsync(skillsetName);
        var respDatasource = await client.DeleteDataSourceConnectionAsync(datasourceName);
        var respIndexer = await client.DeleteIndexerAsync(indexerName);
        var respIndex = await indexClient.DeleteIndexAsync(indexName);

        if (respSkillset.IsError || respDatasource.IsError || respIndexer.IsError || respIndex.IsError)
            throw new Exception($"Ran into error deleting. skillset resp: {respSkillset.ReasonPhrase} datasource resp: {respDatasource.ReasonPhrase} Indexer resp: {respIndexer.ReasonPhrase} index resp: {respIndex.ReasonPhrase}");
    }
}