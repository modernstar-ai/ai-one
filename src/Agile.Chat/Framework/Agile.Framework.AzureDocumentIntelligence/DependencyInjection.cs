using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Framework.AzureDocumentIntelligence;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureDocumentIntelligence(this IServiceCollection services)
    {
        if (!string.IsNullOrEmpty(Configs.AzureDocumentIntelligence.ApiKey))
        {
            return services
                  .AddSingleton<DocumentIntelligenceClient>(_ => new DocumentIntelligenceClient(
               new Uri(Configs.AzureDocumentIntelligence.Endpoint),
               new AzureKeyCredential(Configs.AzureDocumentIntelligence.ApiKey)));
        }
        else
        {
            return services
                  .AddSingleton<DocumentIntelligenceClient>(_ => new DocumentIntelligenceClient(
               new Uri(Configs.AzureDocumentIntelligence.Endpoint),
               new Azure.Identity.DefaultAzureCredential()));
        }
    }
}