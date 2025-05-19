using System.Text.RegularExpressions;

namespace Agile.Framework.AzureAiSearch.Models;

public class AzureSearchDocumentExt : AzureSearchDocument
{
    public AzureSearchDocumentExt(string id, string fileId, string chunk, string fileName, string url, List<string> tags, ReadOnlyMemory<float> chunkVector, ReadOnlyMemory<float> nameVector)
    {
        Id = id;
        FileId = fileId;
        Chunk = chunk;
        Name = fileName;
        Url = url;
        ChunkVector = chunkVector;
        NameVector = nameVector;
        Tags = tags;
    }

    public int ReferenceNumber { get; set; }

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