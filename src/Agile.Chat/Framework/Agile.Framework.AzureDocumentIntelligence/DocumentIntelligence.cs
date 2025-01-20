using Agile.Framework.Common.Attributes;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agile.Framework.AzureDocumentIntelligence;

public interface IDocumentIntelligence
{
    Task CrackDocumentAsync(Uri fileUri);
}

[Export(typeof(IDocumentIntelligence), ServiceLifetime.Singleton)]
public class DocumentIntelligence(DocumentIntelligenceClient client, ILogger<DocumentIntelligence> logger) : IDocumentIntelligence
{
    public async Task CrackDocumentAsync(Uri fileUri)
    {
        var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, new AnalyzeDocumentOptions("prebuilt-layout", fileUri));
        if (operation.HasValue)
        {
            logger.LogInformation($"Document {fileUri} cracked");
        }
    }
}