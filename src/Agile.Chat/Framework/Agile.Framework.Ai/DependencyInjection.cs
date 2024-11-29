using System.Diagnostics.CodeAnalysis;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Agile.Framework.Ai;

[Experimental("SKEXP0010")]
public static class DependencyInjection
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services) =>
        services.AddAppKernel();
    
    private static IServiceCollection AddAppKernel(this IServiceCollection services) =>
        services.AddTransient<Kernel>(_ =>
        {
            var configs = Configs.AzureOpenAi;

            return Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(configs.DeploymentName, configs.Endpoint, configs.ApiKey,
                    apiVersion: configs.ApiVersion)
                .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName, configs.Endpoint,
                    configs.ApiKey, apiVersion: configs.ApiVersion)
                .Build();
        });
}