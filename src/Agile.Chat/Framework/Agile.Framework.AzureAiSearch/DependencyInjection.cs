using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.AzureAiSearch;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureAiSearch(this IServiceCollection services)
    {
        return services.AddSingleton(_ =>
        {
            if (!string.IsNullOrEmpty(Configs.AzureSearch.ApiKey))
            {
                return new SearchIndexClient(
                   new Uri(Configs.AzureSearch.Endpoint),
                   new AzureKeyCredential(Configs.AzureSearch.ApiKey));
            }
            else
            {
                return new SearchIndexClient(
                    new Uri(Configs.AzureSearch.Endpoint),
                    new Azure.Identity.DefaultAzureCredential());
            }
        });
    }
}