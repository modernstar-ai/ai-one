namespace Agile.Framework.AzureDocumentIntelligence.Converters;

public interface ICustomConverter
{
    public Task<Stream> ConvertDocumentAsync(Stream fileStream);
}