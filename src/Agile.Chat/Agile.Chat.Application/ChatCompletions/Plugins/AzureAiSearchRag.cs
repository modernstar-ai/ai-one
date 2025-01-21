using System.ClientModel;
using System.ComponentModel;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureAiSearch.Models;
using Microsoft.SemanticKernel;

namespace Agile.Chat.Application.ChatCompletions.Plugins;

public class AzureAiSearchRag(ChatContainer container)
{
    [KernelFunction("get_documents")]
    [Description("Searches for documents to reference with citations")]
    public async Task<List<AzureSearchDocument>> GetDocumentsAsync([Description("The full user prompt")] string userPrompt)
    {
        ReadOnlyMemory<float> embedding;
        try
        {
            embedding = await container.AppKernel.GenerateEmbeddingAsync(userPrompt);
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
        {
            throw new Exception("Rate limit exceeded");
        }
            
        var documents = await container.AzureAiSearch.SearchAsync(container.Assistant!.FilterOptions.IndexName, 
            new AiSearchOptions(userPrompt, embedding)
            {
                DocumentLimit = container.Thread.FilterOptions.DocumentLimit,
                Strictness = container.Thread.FilterOptions.Strictness
            });
        var count = container.Citations.Count;
        documents.ForEach(document =>
        {
            document.ReferenceNumber = count + 1;
            count++;
        });

        container.Citations.AddRange(documents);
        return documents;
    }
}