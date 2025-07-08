using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.EnvironmentVariables;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Agile.Framework.Ai;

public interface IAppKernelBuilder
{
    AppKernelBuilder AddAzureOpenAIChatCompletion(string deploymentName);

    AppKernelBuilder AddAzureOpenAITextEmbeddingGeneration(string deploymentName);

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AppKernel Build();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}

[Export(typeof(IAppKernelBuilder), ServiceLifetime.Transient)]
public class AppKernelBuilder : IAppKernelBuilder
{
    private readonly IKernelBuilder _kernelBuilder;
    public AppKernelBuilder()
    {
        _kernelBuilder = Kernel.CreateBuilder();
    }

    public AppKernelBuilder AddAzureOpenAIChatCompletion(string deploymentName)
    {
        var configs = Configs.AzureOpenAi;
        var chatEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.Endpoint) ? configs.Apim.Endpoint : configs.Endpoint;
        if (!string.IsNullOrEmpty(Configs.AzureOpenAi.ApiKey))
        {
            _kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, chatEndpoint,
            configs.ApiKey!);
        }
        else
        {
            var credential = new DefaultAzureCredential();
            _kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, chatEndpoint, credential);
        }

        return this;
    }

    public AppKernelBuilder AddAzureOpenAITextEmbeddingGeneration(string deploymentName)
    {
        var configs = Configs.AzureOpenAi;
        var embeddingsEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.EmbeddingsEndpoint) ? configs.Apim.EmbeddingsEndpoint : configs.Endpoint;
        if (!string.IsNullOrEmpty(Configs.AzureOpenAi.ApiKey))
        {
            _kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(deploymentName, embeddingsEndpoint, configs.ApiKey!);
        }
        else
        {
            var credential = new DefaultAzureCredential();
            _kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(deploymentName, embeddingsEndpoint, credential);
        }

        return this;
    }

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public AppKernel Build()
    {
        return new AppKernel(_kernelBuilder.Build());
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
}