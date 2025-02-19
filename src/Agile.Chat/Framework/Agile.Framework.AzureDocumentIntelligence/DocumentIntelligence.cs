using Agile.Framework.Common.Attributes;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.AzureDocumentIntelligence;

public interface IDocumentIntelligence
{
    Task<string> CrackDocumentAsync(Stream fileStream, bool isTextDoc = false);
    List<string> ChunkDocumentWithOverlap(string document, int? chunkSize = null, int? chunkOverlap = null);
}

[Export(typeof(IDocumentIntelligence), ServiceLifetime.Singleton)]
public class DocumentIntelligence(DocumentIntelligenceClient client, ILogger<DocumentIntelligence> logger) : IDocumentIntelligence
{
    private const int ChunkSize = 2300;
    private const int ChunkOverlap = (int)(ChunkSize * 0.25);
    public async Task<string> CrackDocumentAsync(Stream fileStream, bool isTextDoc = false)
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

    public List<string> ChunkDocumentWithOverlap(string document, int? chunkSize = null, int? chunkOverlap = null)
    {
        var size = chunkSize != null && chunkSize != 0 ? chunkSize.Value : ChunkSize;
        var overlap = chunkOverlap != null && chunkOverlap != 0 ? (int)(size * ((double)chunkOverlap.Value / 100)) : ChunkOverlap;
        var chunks = new List<string>();

        if (document.Length <= size)
        {
            chunks.Add(document);
            return chunks;
        }

        int startIndex = 0;

        while (startIndex < document.Length)
        {
            int endIndex = startIndex + size;
            string chunk = document.Substring(startIndex, Math.Min(endIndex - startIndex, document.Length - startIndex));
            chunks.Add(chunk);
            startIndex = endIndex - overlap;
        }

        return chunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk)).ToList();
    }
}