using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Framework.Common.EnvironmentVariables;

namespace Agile.Chat.Application.Files.Utils;

public static class EventGridHelpers
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Type
    {
        BlobCreated,
        BlobDeleted,
        Unknown
    }

    public class FileMetadata
    {
        public string BlobUrl { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }
    
    public static (string, string) GetIndexAndFolderName(JsonNode eventGrid)
    {
        var subject = eventGrid?["subject"]?.ToString();
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
        var subject = eventGrid?["subject"]?.ToString();
        var fileName = Path.GetFileName(subject);
        var typeStr = eventGrid?["eventType"]?.ToString();
        Type eventType = Type.Unknown;

        if (typeStr == "Microsoft.Storage.BlobDeleted")
            eventType = Type.BlobDeleted;
        else if (typeStr == "Microsoft.Storage.BlobCreated")
            eventType = Type.BlobCreated;

        return (fileName!, eventType);
    }
    
    public static FileMetadata GetFileCreatedMetaData(JsonNode eventObj)
    {
        return new FileMetadata
        {
            BlobUrl = eventObj?["data"]?["url"]?.ToString(),
            ContentType = eventObj?["data"]?["contentType"]?.ToString(),
            ContentLength = eventObj?["data"]?["contentLength"]?.ToString() != null ? long.Parse(eventObj?["data"]?["contentLength"]?.ToString()!) : 0
        };
    }

    public static string CreateEventObjectFromCosmosFile(CosmosFile file, Type eventType)
    {
        var folder = !string.IsNullOrWhiteSpace(file.FolderName) ? file.FolderName + "/" : string.Empty;
        var type = eventType == Type.BlobDeleted ? "Microsoft.Storage.BlobDeleted" : "Microsoft.Storage.BlobCreated";
        
        return JsonSerializer.Serialize(new
        {
            subject = $"/blobServices/default/containers/{Constants.BlobContainerName}/blobs/{file.IndexName}/{folder}{file.Name}",
            eventType = type,
            data = new
            {
                url = file.Url,
                contentType = file.ContentType,
                contentLength = file.Size
            }
        });
    }
}