using System.Diagnostics.CodeAnalysis;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Agile.Framework.Ai;

[Experimental("SKEXP0010")]
public static class DependencyInjection
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services) =>
        services.AddAppKernel();

    private static IServiceCollection AddAppKernel(this IServiceCollection services) =>
        services.AddTransient<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            var configs = Configs.AzureOpenAi;
            var chatEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.Endpoint) ? configs.Apim.Endpoint : configs.Endpoint;
            var embeddingsEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.EmbeddingsEndpoint) ? configs.Apim.EmbeddingsEndpoint : configs.Endpoint;

            if (!string.IsNullOrEmpty(Configs.AzureOpenAi.ApiKey))
            {
                builder = builder.AddAzureOpenAIChatCompletion(configs.DeploymentName!, chatEndpoint,
               configs.ApiKey!,
               apiVersion: configs.ApiVersion);

                builder = builder
                    .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!, embeddingsEndpoint,
                        configs.ApiKey!);
            }
            else
            {
                var credential = new DefaultAzureCredential();
                builder = builder.AddAzureOpenAIChatCompletion(configs.DeploymentName!, chatEndpoint, credential,
                    apiVersion: configs.ApiVersion);
                builder = builder
                    .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!, embeddingsEndpoint,
                        credential, apiVersion: configs.ApiVersion);
            }

            return builder.Build();
        });
}