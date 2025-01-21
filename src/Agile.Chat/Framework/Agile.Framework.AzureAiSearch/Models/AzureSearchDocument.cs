using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Agile.Framework.AzureAiSearch.Models;

public class AzureSearchDocument
{
    public int ReferenceNumber { get; set; }
    [JsonPropertyName("chunk_id")]
    public string Id { get; set; }
    [JsonPropertyName("chunk")]
    public string Chunk { get; set; }
    [JsonPropertyName("title")]
    public string Name { get; set; }
    [JsonPropertyName("metadata_storage_path")]
    public string Url { get; set; }
    
    public string ToString(int index)
    {
        return $"""
                Reference Number:
                {index}
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