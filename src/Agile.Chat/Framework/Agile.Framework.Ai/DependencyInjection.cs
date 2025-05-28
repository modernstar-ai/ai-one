using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Agile.Framework.Common.EnvironmentVariables;
using Azure;
using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OpenAI;

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

            builder = builder.AddAzureOpenAIChatCompletion(configs.DeploymentName!, chatEndpoint,
                configs.ApiKey!,
                apiVersion: configs.ApiVersion);
            
            builder = builder
                .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!, embeddingsEndpoint,
                    configs.ApiKey!);
            
            return builder.Build();
        });
}