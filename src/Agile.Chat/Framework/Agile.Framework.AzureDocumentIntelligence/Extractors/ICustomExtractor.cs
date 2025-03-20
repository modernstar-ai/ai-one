namespace Agile.Framework.AzureDocumentIntelligence.Extractors;

public interface ICustomExtractor
{
    public Task<string> ExtractTextAsync(Stream fileStream);
}