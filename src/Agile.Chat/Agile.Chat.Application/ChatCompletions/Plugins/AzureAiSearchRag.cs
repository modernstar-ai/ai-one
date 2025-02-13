using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.Models;
using Mapster;
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
            new AiSearchOptions(query, container.Assistant.FilterOptions.IndexName, embedding)
            {
                DocumentLimit = container.Thread.FilterOptions.DocumentLimit,
                Strictness = container.Thread.FilterOptions.Strictness,
                FolderFilters = container.Assistant.FilterOptions.Folders
            });
        var refDocs = UpdateReferenceNumbers(documents);

        container.Citations.AddRange(refDocs);
        return documents.Select(x => x.ToString()).ToList();
    }

    private List<AzureSearchDocWithRefNo> UpdateReferenceNumbers(List<AzureSearchDocument> documents)
    {
        var count = container.Citations.Count;
        var refDocs = documents.Adapt<List<AzureSearchDocWithRefNo>>();
        refDocs.ForEach(document =>
        {
            document.ReferenceNumber = count + 1;
            count++;
        });

        return refDocs;
    }
}