// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace MinimalApi.Extensions;

internal static class ServiceCollectionExtensions
{
    //private static readonly DefaultAzureCredential s_azureCredential = new();

    private static string? GetEnvVar(string key) => Environment.GetEnvironmentVariable(key);

    public static string GetBlobStorageConnectionString(string accountName, string accountKey)
    {
        // Construct the connection string
        string connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
        return connectionString;
    }

    internal static IServiceCollection AddAzureServices(this IServiceCollection services)
    {
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageAccountEndpoint = GetEnvVar("AZURE_STORAGE_ACCOUNT_ENDPOINT") ?? throw new ArgumentNullException() ;

            //var blobServiceClient = new BlobServiceClient(new Uri(azureStorageAccountEndpoint), s_azureCredential);
            var accountName = GetEnvVar("AZURE_STORAGE_ACCOUNT_NAME") ?? throw new ArgumentNullException();
            var accountKey = GetEnvVar("AZURE_STORAGE_ACCOUNT_KEY") ?? throw new ArgumentNullException();
            var bloblStorageConnectionString = GetBlobStorageConnectionString(accountName,accountKey);
            var blobServiceClient = new BlobServiceClient(bloblStorageConnectionString);

            return blobServiceClient;
        });

        services.AddSingleton<BlobContainerClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureStorageContainer = GetEnvVar("AZURE_STORAGE_ACCOUNT_CONTAINER");
            return sp.GetRequiredService<BlobServiceClient>().GetBlobContainerClient(azureStorageContainer);
        });

        services.AddSingleton<ISearchService, AzureSearchService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var azureSearchServiceEndpoint = GetEnvVar("AZURE_SEARCH_ENDPOINT") ?? throw new ArgumentNullException() ;
            var azureSearchIndex = GetEnvVar("AZURE_SEARCH_INDEX_NAME_RAG") ?? throw new ArgumentNullException();
            

            //var searchClient = new SearchClient(
            //                   new Uri(azureSearchServiceEndpoint), azureSearchIndex, s_azureCredential);
            var azureSearchKey = GetEnvVar("AZURE_SEARCH_API_KEY") ?? throw new ArgumentNullException();
            var azureSearchKeyCredential = new Azure.AzureKeyCredential(azureSearchKey);
            var searchClient = new SearchClient(
                               new Uri(azureSearchServiceEndpoint), azureSearchIndex, azureSearchKeyCredential);

            return new AzureSearchService(searchClient);
        });

        services.AddSingleton<DocumentAnalysisClient>(sp =>
        {

            
            var azureOpenAiServiceEndpoint = GetEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new ArgumentNullException();

            //var documentAnalysisClient = new DocumentAnalysisClient(
            //    new Uri(azureOpenAiServiceEndpoint), s_azureCredential);

            var azureDocIntelligentKey = GetEnvVar("AZURE_DOCUMENT_INTELLIGENCE_KEY") ?? throw new ArgumentNullException();
            var azureDocIntelligenceCredential = new Azure.AzureKeyCredential(azureDocIntelligentKey);

            var documentAnalysisClient = new DocumentAnalysisClient(
                new Uri(azureOpenAiServiceEndpoint), azureDocIntelligenceCredential);

            return documentAnalysisClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();

            var azureOpenAiServiceEndpoint = GetEnvVar("AZURE_OPENAI_ENDPOINT") ?? throw new ArgumentNullException();

            //var openAIClient = new OpenAIClient(new Uri(azureOpenAiServiceEndpoint), s_azureCredential);
            var azureOpenAIKey = GetEnvVar("AZURE_SEARCH_API_KEY") ?? throw new ArgumentNullException();
            var azureAOAICredential = new Azure.AzureKeyCredential(azureOpenAIKey);

            var openAIClient = new OpenAIClient(new Uri(azureOpenAiServiceEndpoint), azureAOAICredential);

            return openAIClient;
        });

        services.AddSingleton<AzureBlobStorageService>();
        services.AddSingleton<ReadRetrieveReadChatService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var openAIClient = sp.GetRequiredService<OpenAIClient>();
            var searchClient = sp.GetRequiredService<ISearchService>();
            //return new ReadRetrieveReadChatService(searchClient, openAIClient, config, tokenCredential: s_azureCredential);
            var azureOpenAIKey = GetEnvVar("AZURE_SEARCH_API_KEY") ?? throw new ArgumentNullException();
            var azureAOAICredential = new Azure.AzureKeyCredential(azureOpenAIKey);
            return new ReadRetrieveReadChatService(searchClient, openAIClient, config);
        });
        services.AddSingleton<SimpleChatService>(sp =>
        {           
            var openAIClient = sp.GetRequiredService<OpenAIClient>();
            return new SimpleChatService(openAIClient);
        });

        return services;
    }

    internal static IServiceCollection AddCrossOriginResourceSharing(this IServiceCollection services)
    {
        services.AddCors(
            options =>
                options.AddDefaultPolicy(
                    policy =>
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()));

        return services;
    }
}
