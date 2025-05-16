using System.ClientModel;
using System.ComponentModel;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Framework.AzureAiSearch.Models;
using Microsoft.SemanticKernel;

namespace Agile.Chat.Application.ChatCompletions.Plugins;

public class AzureAiSearchRag(ChatContainer container)
{
    [KernelFunction("get_documents")]
    [Description("Searches for documents to reference with citations. Use this whenever a user asks or inquires for information")]
    public async Task<List<string>> GetDocumentsAsync([Description("The full query to search for the appropriate documents")] string query)
    {
        ReadOnlyMemory<float> embedding;
        try
        {
            embedding = await container.AppKernel.GenerateEmbeddingAsync(query);
        }
        catch (Exception ex) when (ex is ClientResultException exception && exception.Status == 429)
        {
            throw new Exception("Rate limit exceeded");
        }

        var documents = await container.AzureAiSearch.SearchAsync(container.Assistant!.FilterOptions.IndexName,
            new AiSearchOptions(query, embedding)
            {
                DocumentLimit = container.Thread.FilterOptions.DocumentLimit,
                Strictness = container.Thread.FilterOptions.Strictness
            });

        UpdateReferenceNumbers(documents);

        container.Citations.AddRange(documents);
        return documents.Select(x => x.ToString()).ToList();
    }

    private void UpdateReferenceNumbers(List<AzureSearchDocument> documents)
    {
        var count = container.Citations.Count;
        documents.ForEach(document =>
        {
            document.ReferenceNumber = count + 1;
            count++;
        });
    }
}