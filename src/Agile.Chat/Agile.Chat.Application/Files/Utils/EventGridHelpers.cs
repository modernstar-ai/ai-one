using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Agile.Chat.Application.Files.Utils;

public static class EventGridHelpers
{
    public enum Type
    {
        BlobCreated = 0,
        BlobDeleted = 1,
        Unknown = 2
    }

    public class FileMetaData
    {
        public string BlobUrl { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }
    
    public static (string, string) GetIndexAndFolderName(JsonNode eventGrid)
    {
        var subject = eventGrid.AsArray().FirstOrDefault()?["subject"]?.ToString();
        if (string.IsNullOrWhiteSpace(subject))
            return (string.Empty, string.Empty);
        
        // Define the regular expression to match the pattern
        string pattern = @"/blobs/([^/]+)(?:/([^/]+(?:/[^/]+)*))?/.*";

        // Create a regex object
        var regex = new Regex(pattern);

        // Match the path against the pattern
        var match = regex.Match(subject);
        if (!match.Success) return (string.Empty, string.Empty);
        
        // Extract the INDEX_NAME and FOLDERS_HERE
        string indexName = match.Groups[1].Value; // First captured group
        string folder = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;   // Second captured group

        return (indexName, folder);
    }
    
    public static (string, Type) GetFileNameAndEventType(JsonNode eventGrid)
    {
        var subject = eventGrid.AsArray().FirstOrDefault()?["subject"]?.ToString();
        var fileName = Path.GetFileName(subject);
        var typeStr = eventGrid.AsArray().FirstOrDefault()?["eventType"]?.ToString();
        Type eventType = Type.Unknown;

        if (typeStr == "Microsoft.Storage.BlobDeleted")
            eventType = Type.BlobDeleted;
        else if (typeStr == "Microsoft.Storage.BlobCreated")
            eventType = Type.BlobCreated;

        return (fileName!, eventType);
    }
    
    public static FileMetaData GetFileCreatedMetaData(JsonNode eventGrid)
    {
        var eventObj = eventGrid.AsArray().FirstOrDefault();

        return new FileMetaData
        {
            BlobUrl = eventObj?["data"]?["url"]?.ToString(),
            ContentType = eventObj?["data"]?["contentType"]?.ToString(),
            ContentLength = eventObj?["data"]?["contentLength"]?.ToString() != null ? long.Parse(eventObj?["data"]?["contentLength"]?.ToString()!) : 0
        };
    }
}