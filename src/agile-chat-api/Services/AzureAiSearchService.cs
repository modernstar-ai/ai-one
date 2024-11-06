using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Azure.Cosmos;

namespace agile_chat_api.Services;

public interface IAzureAiSearchService
{
    Task<bool> RunIndexer(string indexName);
}

public class AzureAiSearchService : IAzureAiSearchService
{
    public static readonly string FOLDERS_INDEX_NAME =
        Environment.GetEnvironmentVariable("AZURE_SEARCH_FOLDERS_INDEX_NAME")! + "-indexer";

    private readonly Uri _uri;
    private readonly AzureKeyCredential _credentials;
    private readonly ILogger<AzureAiSearchService> _logger;

    public AzureAiSearchService(ILogger<AzureAiSearchService> logger)
    {
        _logger = logger;
        _uri = new(Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")!);
        _credentials = new(Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY")!);
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