using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.AzureAiSearch;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureAiSearch(this IServiceCollection services) =>
        services
            .AddSingleton<SearchIndexClient>(_ =>
                new SearchIndexClient(new Uri(Configs.AzureSearch.Endpoint),
                    new AzureKeyCredential(Configs.AzureSearch.ApiKey)));
}