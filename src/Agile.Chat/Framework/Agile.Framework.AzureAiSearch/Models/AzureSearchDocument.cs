using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Agile.Framework.AzureAiSearch.Models;

public class AzureSearchDocument
{
    public static AzureSearchDocument Create(string fileId, string chunk, string fileName, string url, ReadOnlyMemory<float> vector)
    {
        return new AzureSearchDocument()
        {
            Id = Guid.NewGuid() + "_" + fileId,
            FileId = fileId,
            Chunk = chunk,
            Name = fileName,
            Url = url,
            Vector = vector
        };
    }
    
    public int ReferenceNumber { get; set; }
    [JsonPropertyName("chunk_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("file_id")]
    public string FileId { get; set; }
    
    [JsonPropertyName("chunk")]
    public string Chunk { get; set; }
    [JsonPropertyName("title")]
    public string Name { get; set; }
    [JsonPropertyName("metadata_storage_path")]
    public string Url { get; set; }
    
    [JsonPropertyName("text_vector")]
    public ReadOnlyMemory<float> Vector { get; set; }
    
    public new string ToString()
    {
        return $"""
                Reference Number:
                {ReferenceNumber}
                Title:
                {Name}
                Chunk:
                {RemoveExtraWhitespaces(Chunk)}
                
                """;
    }
    
    string RemoveExtraWhitespaces(string input)
    {
        // First, remove redundant spaces (multiple spaces replaced by one)
        input = Regex.Replace(input, @" +", " ");

        // Then, reduce consecutive tabs to a single tab
        input = Regex.Replace(input, @"\t+", "\t");

        // Reduce consecutive newlines to a single newline
        input = Regex.Replace(input, @"\n+", "\a");
        
        input = Regex.Replace(input, @" \a+", "\a");
        input = Regex.Replace(input, @"\a+", "\n");
        // Return the modified result
        return input;
    }
}