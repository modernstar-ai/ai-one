using System.Diagnostics.CodeAnalysis;
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

     private async IAsyncEnumerable<Dictionary<ResponseType, string>> GetChatStream(ChatHistory chatHistory)
     {
         var options = new AzureOpenAIPromptExecutionSettings()
         {
            
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