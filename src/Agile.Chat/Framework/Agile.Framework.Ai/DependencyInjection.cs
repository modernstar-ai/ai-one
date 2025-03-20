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
            
            var defaultConfig = "default";
            var configs = Configs.AzureOpenAi;
            
            var httpContext = sp.GetService<IHttpContextAccessor>();
            var httpClient = new HttpClient(); 
            if(!string.IsNullOrWhiteSpace(httpContext?.HttpContext?.Request?.Headers?["Authorization"].ToString()))
                httpClient.DefaultRequestHeaders.Add("Authorization", httpContext.HttpContext.Request.Headers["Authorization"].ToString());
            httpClient.DefaultRequestHeaders.Add("api-key", configs.ApiKey);
            
            if (!string.IsNullOrWhiteSpace(configs.Apim.Endpoint))
            {
                //Override configs with APIM settings
                var openaiClient = new OpenAIClient(new ApiKeyCredential(defaultConfig), new OpenAIClientOptions
                {
                    Endpoint = new Uri(configs.Apim.Endpoint),
                    Transport = new HttpClientPipelineTransport(httpClient)
                });
                builder = builder
                    .AddOpenAIChatCompletion(modelId: configs.DeploymentName!, openaiClient);
            }
            else
            {
                builder = builder.AddAzureOpenAIChatCompletion(configs.DeploymentName!, configs.Endpoint,
                    configs.ApiKey!,
                    apiVersion: configs.ApiVersion);
            }
            
            if (!string.IsNullOrWhiteSpace(configs.Apim.EmbeddingsEndpoint))
            {
                var embeddingsClient = new OpenAIClient(new ApiKeyCredential(defaultConfig), new OpenAIClientOptions
                {
                    Endpoint = new Uri(configs.Apim.EmbeddingsEndpoint!),
                    Transport = new HttpClientPipelineTransport(httpClient)
                });
                builder = builder
                    .AddOpenAITextEmbeddingGeneration(modelId: configs.EmbeddingsDeploymentName!, embeddingsClient);
            }
            else
            {
                builder = builder
                    .AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!, configs.Endpoint,
                        configs.ApiKey!, apiVersion: configs.ApiVersion);
            }
            
            return builder.Build();
        });
}