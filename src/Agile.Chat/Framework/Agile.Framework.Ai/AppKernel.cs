using System.Diagnostics.CodeAnalysis;
using Agile.Framework.Common.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace Agile.Framework.Ai;

public interface IAppKernel
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text);
    Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts);
    IAsyncEnumerable<StreamingKernelContent> GetChatStream(ChatHistory chatHistory, AzureOpenAIPromptExecutionSettings settings);
    IAsyncEnumerable<StreamingKernelContent>? GetPromptFileChatStream(AzureOpenAIPromptExecutionSettings settings,
        string promptRelativeDirectory,
        string promptFile,
        Dictionary<string, object?>? kernelArguments = null!);
    Task<FunctionResult> GetPromptFileChat(AzureOpenAIPromptExecutionSettings settings,
        string promptRelativeDirectory,
        string promptFile,
        Dictionary<string, object?>? kernelArguments = null!);

    void AddPlugin<T>(IServiceProvider? serviceProvider = null);
}

[Experimental("SKEXP0001")]
public class AppKernel : IAppKernel
{
    private readonly Kernel _kernel;
    private readonly ITextEmbeddingGenerationService _textEmbedding;
    private readonly IChatCompletionService _chatCompletion;

    internal AppKernel(Kernel kernel)
    {
        _kernel = kernel;
        _textEmbedding = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
    }

    public void AddPlugin<T>(IServiceProvider? serviceProvider = null)
    {
        _kernel.Plugins.AddFromType<T>(nameof(T), serviceProvider);
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text) =>
        await _textEmbedding.GenerateEmbeddingAsync(text);

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts) =>
        await _textEmbedding.GenerateEmbeddingsAsync(texts);

    public IAsyncEnumerable<StreamingKernelContent> GetChatStream(ChatHistory chatHistory, AzureOpenAIPromptExecutionSettings settings)
    {
        var responses = _chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, settings, _kernel);
        return responses;
    }

    public IAsyncEnumerable<StreamingKernelContent>? GetPromptFileChatStream(
        AzureOpenAIPromptExecutionSettings settings,
        string promptRelativeDirectory,
        string promptFile,
        Dictionary<string, object?>? kernelArguments = null!)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, promptRelativeDirectory, promptFile);
        var prompt = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(prompt)) throw new Exception("Prompt file path is empty");
        var kernelFunction = _kernel.CreateFunctionFromPromptYaml(prompt, new HandlebarsPromptTemplateFactory());
        //Create arguments
        var arguments = new KernelArguments(settings);
        // Manually add dictionary items to arguments
        foreach (var kvp in kernelArguments ?? new Dictionary<string, object?>())
            arguments[kvp.Key] = kvp.Value;

        var responses = kernelFunction.InvokeStreamingAsync(_kernel, arguments);
        return responses;
    }

    public async Task<FunctionResult> GetPromptFileChat(
        AzureOpenAIPromptExecutionSettings settings,
        string promptRelativeDirectory,
        string promptFile,
        Dictionary<string, object?>? kernelArguments = null!)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, promptRelativeDirectory, promptFile);
        var prompt = await File.ReadAllTextAsync(path);

        if (string.IsNullOrWhiteSpace(prompt)) throw new Exception("Prompt file path is empty");
        var kernelFunction = _kernel.CreateFunctionFromPromptYaml(prompt, new HandlebarsPromptTemplateFactory());
        //Create arguments
        var arguments = new KernelArguments(settings);
        // Manually add dictionary items to arguments
        foreach (var kvp in kernelArguments ?? new Dictionary<string, object?>())
            arguments[kvp.Key] = kvp.Value;

        var response = await kernelFunction.InvokeAsync(_kernel, arguments);
        return response!;
    }
}