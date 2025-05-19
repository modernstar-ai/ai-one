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

        var result = await container.AzureAiSearch.SearchAsync(container.Assistant!.FilterOptions.IndexName,
            new AiSearchOptions(query, embedding)
            {
                DocumentLimit = container.Thread.FilterOptions.DocumentLimit,
                Strictness = container.Thread.FilterOptions.Strictness
            });

        var documents = UpdateReferenceNumbers(result);

        container.Citations.AddRange(documents);
        return documents.Select(x => x.ToString()).ToList();
    }

    private List<AzureSearchDocumentExt> UpdateReferenceNumbers(List<AzureSearchDocument> documents)
    {
        var count = container.Citations.Count;
        var result = new List<AzureSearchDocumentExt>();

        foreach (var item in documents)
        {
            var doc = new AzureSearchDocumentExt(item.Id, item.FileId, item.Chunk, item.Name, item.Url, item.Tags, item.ChunkVector, item.NameVector);
            doc.ReferenceNumber = count + 1;
            count++;
        }

        return result;

    }
}