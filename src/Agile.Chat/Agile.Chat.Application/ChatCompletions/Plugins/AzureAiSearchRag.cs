using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using Agile.Chat.Application.ChatCompletions.Models;
using Agile.Chat.Domain.Assistants.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch.Interfaces;
using Agile.Framework.AzureAiSearch.Models;
using Microsoft.SemanticKernel;

namespace Agile.Chat.Application.ChatCompletions.Plugins;

public class AzureAiSearchRag(ChatContainer container)
{
    [KernelFunction("get_documents")]
    [Description("Searches for documents to reference with citations")]
    public async Task<List<string>> GetDocumentsAsync([Description("The full user prompt")] string userPrompt)
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
        UpdateReferenceNumbers(documents);

        container.Citations.AddRange(documents);
        return documents.Select(x => x.ToString()).ToList();
    }
    
    [KernelFunction("get_previous_documents")]
    [Description("Gets the documents from a previous assistant message to reference.")]
    public async Task<List<string>> GetPreviousDocumentsAsync([Description("The full previous assistant message")] string assistantMessage)
    {
        var message = container.Messages.FirstOrDefault(x =>
            x.Content.Contains(assistantMessage, StringComparison.InvariantCultureIgnoreCase));
        if (message == null || !message.Options.Metadata.ContainsKey(MetadataType.Citations)) return [];
        
        var citations = JsonSerializer.SerializeToNode(message.Options.Metadata[MetadataType.Citations]).Deserialize<List<Citation>>(new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});
        var tasks = new List<Task<AzureSearchDocument>>();
        foreach (var citation in citations)
        {
            tasks.Add(container.AzureAiSearch.GetChunkByIdAsync(container.Assistant?.FilterOptions.IndexName, citation.Id));
        }
        await Task.WhenAll(tasks);

        var documents = tasks.Where(x => x.Result != null).Select(x => x.Result).ToList();
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