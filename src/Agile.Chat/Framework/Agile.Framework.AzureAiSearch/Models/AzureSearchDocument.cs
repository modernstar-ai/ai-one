using System.Text.Json.Serialization;

namespace Agile.Framework.AzureAiSearch.Models;

public class AzureSearchDocument
{
    public string Chunk { get; set; }
    public string Title { get; set; }
    [JsonPropertyName("metadata_storage_path")]
    public string Url { get; set; }

    public override string ToString()
    {
        return $"""
                Title: {Title}
                Chunk: {Chunk}
                
                
                """;
    }
}