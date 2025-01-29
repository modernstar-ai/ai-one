using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Agile.Framework.Common.EnvironmentVariables;
using Microsoft.AspNetCore.Http;
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
            var defaultConfig = "default";
            var configs = Configs.AzureOpenAi;

            var endpoint = configs.Endpoint;
            var httpClient = new HttpClient();
            
            if (!string.IsNullOrWhiteSpace(configs.Apim.Endpoint))
            {
                //Override configs with APIM settings
                endpoint = configs.Apim.Endpoint;
                var httpContext = sp.GetService<IHttpContextAccessor>();
                var authHeader = httpContext?.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);
            }

            return Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(configs.DeploymentName ?? defaultConfig, endpoint, configs.ApiKey ?? defaultConfig,
                    apiVersion: configs.ApiVersion ?? defaultConfig, httpClient: httpClient)
                .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName ?? defaultConfig, endpoint,
                    configs.ApiKey ?? defaultConfig, apiVersion: configs.ApiVersion ?? defaultConfig, httpClient: httpClient)
                .Build();
        });
}