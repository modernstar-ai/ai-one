using Agile.Framework.Common.Attributes;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.AzureDocumentIntelligence;

public interface IDocumentIntelligence
{
    Task<List<string>> CrackDocumentAsync(Stream fileStream, bool isTextDoc = false);
}

[Export(typeof(IDocumentIntelligence), ServiceLifetime.Singleton)]
public class DocumentIntelligence(DocumentIntelligenceClient client, ILogger<DocumentIntelligence> logger) : IDocumentIntelligence
{
    private const int ChunkSize = 2300;
    private const int ChunkOverlap = (int)(ChunkSize * 0.25);
    public async Task<List<string>> CrackDocumentAsync(Stream fileStream, bool isTextDoc = false)
    {
        var doc = await LoadFileAsync(fileStream, isTextDoc);
        var chunks = ChunkDocumentWithOverlap(doc);

        return chunks;
    }

    private async Task<string> LoadFileAsync(Stream fileStream, bool isTextDoc)
    {
        if (isTextDoc)
        {
            using var sr = new StreamReader(fileStream);
            return await sr.ReadToEndAsync();
        }
        
        var data = await BinaryData.FromStreamAsync(fileStream);
        AnalyzeDocumentOptions analyzeRequest = new AnalyzeDocumentOptions("prebuilt-read", data);
        var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, analyzeRequest);
        
        return string.Join('\n', operation.Value.Paragraphs.Select(p => p.Content));
    }

    private List<string> ChunkDocumentWithOverlap(string document)
    {
        var chunks = new List<string>();

        if (document.Length <= ChunkSize)
        {
            chunks.Add(document);
            return chunks;
        }

        int startIndex = 0;

        while (startIndex < document.Length)
        {
            int endIndex = startIndex + ChunkSize;
            string chunk = document.Substring(startIndex, Math.Min(endIndex - startIndex, document.Length - startIndex));
            chunks.Add(chunk);
            startIndex = endIndex - ChunkOverlap;
        }

        return chunks;
    }
}