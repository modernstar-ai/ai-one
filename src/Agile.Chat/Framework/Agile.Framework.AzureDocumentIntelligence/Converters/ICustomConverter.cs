namespace Agile.Framework.AzureDocumentIntelligence.Converters;

public interface ICustomConverter
{
    public Task<string> ExtractDocumentAsync(Stream fileStream);
}