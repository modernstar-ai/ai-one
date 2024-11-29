using System.Diagnostics.CodeAnalysis;
using Agile.Framework.Ai.Models;
using Agile.Framework.Common.Attributes;
using Agile.Framework.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace Agile.Framework.Ai;

public interface IAppKernel
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text);
    Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts);
    IAsyncEnumerable<Dictionary<ResponseType, string>> GetChatStream(ChatHistory chatHistory, ChatSettings settings);
    IAsyncEnumerable<Dictionary<ResponseType, string>> GetPromptFileChatStream(ChatSettings settings, 
        string promptRelativePath, 
        Dictionary<string, object?>? kernelArguments = null!);
}

[Experimental("SKEXP0001")]
[Export(typeof(IAppKernel), ServiceLifetime.Transient)]
public class AppKernel(Kernel kernel) : IAppKernel
{
     private readonly ITextEmbeddingGenerationService _textEmbedding =
        kernel.GetRequiredService<ITextEmbeddingGenerationService>();
     
     private readonly IChatCompletionService _chatCompletion =
         kernel.GetRequiredService<IChatCompletionService>();
     
     public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text) =>
         await _textEmbedding.GenerateEmbeddingAsync(text);
     
     public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts) =>
         await _textEmbedding.GenerateEmbeddingsAsync(texts);

     public async IAsyncEnumerable<Dictionary<ResponseType, string>> GetChatStream(ChatHistory chatHistory, ChatSettings settings)
     {
         var responses = _chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, settings.ParseAzureOpenAiPromptExecutionSettings(), kernel);
         await foreach (var tokens in responses)
         {
             yield return new Dictionary<ResponseType, string>()
             {
                 { ResponseType.Chat, tokens?.Content ?? string.Empty}
             };
         }
     }
     
     public async IAsyncEnumerable<Dictionary<ResponseType, string>> GetPromptFileChatStream(
         ChatSettings settings,
         string promptRelativePath,
         Dictionary<string, object?>? kernelArguments = null!)
     {
         var prompt = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, promptRelativePath));
         if(string.IsNullOrWhiteSpace(prompt)) throw new Exception("Prompt file path is empty");
         var kernelFunction = kernel.CreateFunctionFromPromptYaml(prompt);
         //Create arguments
         var arguments = new KernelArguments(settings.ParseAzureOpenAiPromptExecutionSettings());
         // Manually add dictionary items to arguments
         foreach (var kvp in kernelArguments ?? new Dictionary<string, object?>())
             arguments[kvp.Key] = kvp.Value;
         
         var responses = kernelFunction.InvokeStreamingAsync(kernel, arguments);
         await foreach (var tokens in responses)
         {
             yield return new Dictionary<ResponseType, string>()
             {
                 { ResponseType.Chat, tokens.ToString()}
             };
         }
     }
}