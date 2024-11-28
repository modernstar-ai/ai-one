using System.Diagnostics.CodeAnalysis;
using Agile.Framework.Ai.Models;
using Agile.Framework.Common.Enums;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace Agile.Framework.Ai;

[Experimental("SKEXP0001")]
public class AppKernel(Kernel kernel)
{
     private readonly ITextEmbeddingGenerationService _textEmbedding =
        kernel.GetRequiredService<ITextEmbeddingGenerationService>();
     
     private readonly IChatCompletionService _chatCompletion =
         kernel.GetRequiredService<IChatCompletionService>();


     private async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text) =>
         await _textEmbedding.GenerateEmbeddingAsync(text);
     
     private async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> texts) =>
         await _textEmbedding.GenerateEmbeddingsAsync(texts);

     private async IAsyncEnumerable<Dictionary<ResponseType, string>> GetChatStream(ChatHistory chatHistory, ChatSettings settings)
     {
         var options = new AzureOpenAIPromptExecutionSettings()
         {
            Temperature = settings.Temperature,
            TopP = settings.TopP,
            MaxTokens = settings.MaxTokens
         };
         
         var responses = _chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, options, kernel);
         await foreach (var tokens in responses)
         {
             yield return new Dictionary<ResponseType, string>()
             {
                 { ResponseType.Chat, tokens?.Content ?? string.Empty}
             };
         }
     }
}